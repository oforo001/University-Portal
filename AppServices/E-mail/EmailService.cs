using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using University_Portal.Models;

namespace University_Portal.AppServices.E_mail
{
    public class EmailService : IEmailService
    {
        public IConfiguration _config { get; }
        public EmailService(IConfiguration config)
        {
            _config=config;
        }

        public async Task SendEmailAsync(EmailDto request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUsername").Value));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_config.GetSection("EmailHost").Value, int.Parse(_config.GetSection("Port").Value), SecureSocketOptions.SslOnConnect);

            await smtp.AuthenticateAsync(_config.GetSection("EmailUsername").Value, _config.GetSection("EmailPassword").Value);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}