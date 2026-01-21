using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ateleris.NET.Shared.Localization;

public static class LocalizationValidator
{
    public static void ValidateLocalizationKeys<TKeysClass, TResourceClass>(
        IServiceProvider services,
        ILogger logger)
    {
        try
        {
            logger.LogInformation("Validating localization keys for {KeysClassName}...", typeof(TKeysClass).Name);
            using var scope = services.CreateScope();
            var locOptions = scope.ServiceProvider.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            var cultures = locOptions.Value.SupportedCultures?.Select(c => c.Name).ToList() ?? [];
            if (cultures.Count == 0)
            {
                logger.LogWarning("No supported cultures found for localization validation");
                return;
            }
            logger.LogInformation("Found {CulturesCount} cultures for validation: {CulturesList}", cultures.Count, string.Join(", ", cultures));

            var keysList = new List<string>();
            var mainKeys = typeof(TKeysClass)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                .Select(f => (string)f.GetValue(null)!)
                .ToList();
            keysList.AddRange(mainKeys);

            var nestedTypes = typeof(TKeysClass).GetNestedTypes(BindingFlags.Public);
            foreach (var nestedType in nestedTypes)
            {
                var nestedKeys = nestedType
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
                    .Select(f => (string)f.GetValue(null)!)
                    .ToList();
                keysList.AddRange(nestedKeys);
            }

            logger.LogInformation("Found {KeysListCount} localization keys to validate", keysList.Count);
            var factory = scope.ServiceProvider.GetRequiredService<IStringLocalizerFactory>();
            var localizer = factory.Create(typeof(TResourceClass));

            var missingKeysCount = 0;
            var currentCulture = CultureInfo.CurrentUICulture;

            foreach (var culture in cultures)
            {
                var missingKeys = new List<string>();
                CultureInfo.CurrentUICulture = new CultureInfo(culture);

                foreach (var key in keysList)
                {
                    var localizedString = localizer[key];
                    if (localizedString.ResourceNotFound || string.IsNullOrEmpty(localizedString.Value))
                    {
                        missingKeys.Add(key);
                    }
                }

                if (missingKeys.Count > 0)
                {
                    missingKeysCount += missingKeys.Count;
                    logger.LogWarning("Culture '{Culture}' is missing {MissingKeysCount} localization keys for {KeysClassName}: {MissingKeys}", culture, missingKeys.Count, typeof(TKeysClass).Name, string.Join(", ", missingKeys));
                }
                else
                {
                    logger.LogInformation("Culture '{Culture}' has all required localization keys for {KeysClassName}", culture, typeof(TKeysClass).Name);
                }
            }

            CultureInfo.CurrentUICulture = currentCulture;

            if (missingKeysCount > 0)
            {
                logger.LogWarning("Total of {MissingKeysCount} missing localization keys for {KeysClassName} across all cultures", missingKeysCount, typeof(TKeysClass).Name);
            }
            else
            {
                logger.LogInformation("All localization keys for {KeysClassName} are present in all cultures", typeof(TKeysClass).Name);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating localization keys for {KeysClassName}", typeof(TKeysClass).Name);
        }
    }
}
