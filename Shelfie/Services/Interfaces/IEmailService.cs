using Shelfie.Models.Dto;

namespace Shelfie.Services;

public interface IEmailService
{
    Task<EmailResultDto> SendEmailConfirmationAsync(string toEmail, string username, string confirmationToken);
    Task<EmailResultDto> SendWelcomeEmailAsync(string toEmail, string username);
}