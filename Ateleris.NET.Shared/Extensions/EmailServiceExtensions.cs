using Ateleris.NET.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ateleris.NET.Shared.Extensions;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailServices<TUser, TUserKey>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<EmailTemplateOptions>? configureTemplates = null)
        where TUser : IdentityUser<TUserKey> where TUserKey : IEquatable<TUserKey>
    {
        if (!services.Any(s => s.ServiceType == typeof(IEmailTemplateProvider)))
        {
            throw new InvalidOperationException($"Service of type {typeof(IEmailTemplateProvider)} must be registered before adding email services");
        }

        var templateOptions = new EmailTemplateOptions
        {
            Domain = configuration["MailSender:Domain"] ?? "example.com"
        };
        configureTemplates?.Invoke(templateOptions);

        services.AddSingleton(templateOptions);
        services.AddSingleton<EmailTemplateRenderer>();

        services.AddTransient<IEmailSender<TUser>, IdentityEmailSenderService<TUser, TUserKey>>();

        return services;
    }

    public static IServiceCollection AddDefaultEmailTemplateProvider(this IServiceCollection services)
    {
        services.AddSingleton<IEmailTemplateProvider>(new StaticEmailTemplateProvider(new StaticEmailTemplateProviderOptions()));
        return services;
    }
}

public class StaticEmailTemplateProviderOptions
{
    public EmailTemplate ConfirmAccount { get; set; } = new(
        "Confirm your account on {0}",
        "Confirm Your Account",
        "Thank you for registering. Please click the button below to confirm your account.",
        "Confirm Account");

    public EmailTemplate ResetPassword { get; set; } = new(
        "Reset your password on {0}",
        "Reset Your Password",
        "You requested to reset your password. Please click the button below to proceed.",
        "Reset Password");

    public EmailTemplate ResetPasswordCode { get; set; } = new(
        "Your password reset code for {0}",
        "Your Reset Code",
        "Please use the following code to reset your password:",
        "");

    public EmailTemplate AccountApproved { get; set; } = new(
        "Account Approved - {0}",
        "Account Approved",
        "Great news! Your account has been approved. You can now access the platform.",
        "Access Platform");

    public EmailTemplate AccountApprovedWithConfirmation { get; set; } = new(
        "Account Approved - {0}",
        "Account Approved - Please Confirm Your Email",
        "Great news! Your account has been approved. However, before you can access the platform, you need to confirm your email address. Please click the button below to confirm your email and complete your account setup.",
        "Confirm Email & Access Platform");
}

public class StaticEmailTemplateProvider(StaticEmailTemplateProviderOptions options) : IEmailTemplateProvider
{
    private readonly StaticEmailTemplateProviderOptions _options = options;

    public Task<EmailTemplate> GetConfirmAccountTemplateAsync(string domain)
    {
        var template = _options.ConfirmAccount with { Subject = string.Format(_options.ConfirmAccount.Subject, domain) };
        return Task.FromResult(template);
    }

    public Task<EmailTemplate> GetResetPasswordTemplateAsync(string domain)
    {
        var template = _options.ResetPassword with { Subject = string.Format(_options.ResetPassword.Subject, domain) };
        return Task.FromResult(template);
    }

    public Task<EmailTemplate> GetResetPasswordCodeTemplateAsync(string domain)
    {
        var template = _options.ResetPasswordCode with { Subject = string.Format(_options.ResetPasswordCode.Subject, domain) };
        return Task.FromResult(template);
    }

    public Task<EmailTemplate> GetAccountApprovedTemplateAsync(string domain)
    {
        var template = _options.AccountApproved with { Subject = string.Format(_options.AccountApproved.Subject, domain) };
        return Task.FromResult(template);
    }

    public Task<EmailTemplate> GetAccountApprovedWithConfirmationTemplateAsync(string domain)
    {
        var template = _options.AccountApprovedWithConfirmation with { Subject = string.Format(_options.AccountApprovedWithConfirmation.Subject, domain) };
        return Task.FromResult(template);
    }
}
