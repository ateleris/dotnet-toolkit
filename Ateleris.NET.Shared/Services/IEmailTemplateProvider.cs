using System.Threading.Tasks;

namespace Ateleris.NET.Shared.Services;

public interface IEmailTemplateProvider
{
    Task<EmailTemplate> GetConfirmAccountTemplateAsync(string domain);
    Task<EmailTemplate> GetResetPasswordTemplateAsync(string domain);
    Task<EmailTemplate> GetResetPasswordCodeTemplateAsync(string domain);
    Task<EmailTemplate> GetAccountApprovedTemplateAsync(string domain);
    Task<EmailTemplate> GetAccountApprovedWithConfirmationTemplateAsync(string domain);
    Task<EmailTemplate> GetAccountCreatedByAdminTemplateAsync(string domain);
}

public record EmailTemplate(string Subject, string Title, string Message, string ButtonText);
