using Ateleris.NET.Shared.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ateleris.NET.Shared.Extensions;

public static class EmailServiceExtensions
{
    public static IServiceCollection AddEmailServices<TUser>(
        this IServiceCollection services,
        IConfiguration configuration,
        IEmailTemplateProvider? templateProvider = null,
        Action<EmailTemplateOptions>? configureTemplates = null)
        where TUser : IdentityUser
    {
        var templateOptions = new EmailTemplateOptions
        {
            Domain = configuration["MailSender:Domain"] ?? "example.com"
        };
        configureTemplates?.Invoke(templateOptions);

        services.AddSingleton(templateOptions);
        services.AddSingleton<EmailTemplateRenderer>();
        services.AddSingleton<IEmailTemplateProvider>(templateProvider ?? new StaticEmailTemplateProvider(new StaticEmailTemplateProviderOptions()));

        services.AddScoped<IEmailSender<TUser>, IdentityEmailSenderService<TUser>>();

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
}
