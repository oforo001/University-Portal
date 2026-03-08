using University_Portal.Models;

namespace University_Portal.AppServices.E_mail
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailDto request);
    }
}
