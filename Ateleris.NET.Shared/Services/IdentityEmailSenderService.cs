using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Ateleris.NET.Shared.Services;

public class IdentityEmailSenderService<TUser, TUserKey>(
    ILogger<IdentityEmailSenderService<TUser, TUserKey>> logger,
    IConfiguration configuration,
    IEmailTemplateProvider templateProvider,
    EmailTemplateRenderer templateRenderer,
    EmailOptions? emailOptions = null
) : IEmailSender<TUser> where TUser : IdentityUser<TUserKey> where TUserKey : IEquatable<TUserKey>
{
    private readonly EmailOptions _options = emailOptions ?? new EmailOptions(configuration);

    public async Task SendEmailAsync(string email, string subject, MimeEntity content, CancellationToken ct = default)
    {
        if (Env.IsProduction)
        {
            logger.LogTrace("Sending email to {email} with subject {subject}", email, subject);
            var m = new MimeMessage
            {
                Subject = subject,
                Body = content
            };
            m.From.Add(new MailboxAddress("", _options.FromAddress));
            m.To.Add(new MailboxAddress("", email));

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.Auto, ct);
                await client.AuthenticateAsync(_options.Username, _options.Password, ct);
                await client.SendAsync(m, ct);
                await client.DisconnectAsync(true, ct);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to send email");
            }
        }
        else
        {
            logger.LogInformation("In Production an email would be sent to {Email} with subject {Subject}:\n{Content}", email, subject, content);
        }
    }

    public async Task SendConfirmationLinkAsync(TUser user, string email, string confirmationLink)
    {
        logger.LogTrace("Sending account confirmation link to user {user}", user.UserName);

        var template = await templateProvider.GetConfirmAccountTemplateAsync(_options.Domain);

        var emailContent = templateRenderer.RenderStandardEmail(
            template.Title,
            template.Message,
            confirmationLink,
            template.ButtonText);

        var content = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailContent };
        await SendEmailAsync(email, template.Subject, content);
    }

    public async Task SendPasswordResetLinkAsync(TUser user, string email, string resetLink)
    {
        logger.LogTrace("Sending password reset link to user {user}", user.Email);

        var template = await templateProvider.GetResetPasswordTemplateAsync(_options.Domain);

        var emailContent = templateRenderer.RenderStandardEmail(
            template.Title,
            template.Message,
            resetLink,
            template.ButtonText);

        var content = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailContent };
        await SendEmailAsync(email, template.Subject, content);
    }

    public async Task SendPasswordResetCodeAsync(TUser user, string email, string resetCode)
    {
        logger.LogTrace("Sending password reset code to user {user}", user.UserName);

        var template = await templateProvider.GetResetPasswordCodeTemplateAsync(_options.Domain);

        var emailContent = templateRenderer.RenderCodeEmail(
            template.Title,
            template.Message,
            resetCode);

        var content = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailContent };
        await SendEmailAsync(email, template.Subject, content);
    }
}

public class EmailOptions
{
    public string Username { get; }
    public string Password { get; }
    public string FromAddress { get; }
    public string SmtpHost { get; }
    public int SmtpPort { get; }
    public string Domain { get; }

    public EmailOptions(IConfiguration configuration)
    {
        Username = configuration["MailSender:UserName"]
            ?? throw new InvalidOperationException("MailSender:UserName configuration missing");
        Password = configuration["MailSender:Password"]
            ?? throw new InvalidOperationException("MailSender:Password configuration missing");
        FromAddress = configuration["MailSender:Address"]
            ?? throw new InvalidOperationException("MailSender:Address configuration missing");
        SmtpHost = configuration["MailSender:smtpHost"]
            ?? throw new InvalidOperationException("MailSender:smtpHost configuration missing");
        SmtpPort = int.Parse(configuration["MailSender:smtpPort"]
            ?? throw new InvalidOperationException("MailSender:smtpPort configuration missing"));
        Domain = configuration["MailSender:Domain"] ?? "example.com";
    }

    public EmailOptions(string username, string password, string fromAddress, string smtpHost, int smtpPort, string domain = "example.com")
    {
        Username = username;
        Password = password;
        FromAddress = fromAddress;
        SmtpHost = smtpHost;
        SmtpPort = smtpPort;
        Domain = domain;
    }
}
