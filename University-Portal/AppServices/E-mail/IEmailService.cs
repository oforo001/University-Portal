using University_Portal.Models;

namespace University_Portal.AppServices.E_mail
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailDto request);
        Task SendEmailVerificationAsync(string toEmail, string token);
        Task SendPasswordResetEmailAsync(string toEmail, string token);
        Task SendWelcomeEmailAsync(string toEmail, string fullName, string password, string role);
    }
}
