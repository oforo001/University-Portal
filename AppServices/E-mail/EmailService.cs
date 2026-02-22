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
        public async Task<string> SendEmailVerificationAsync(string toEmail)
        {
            string token = GenerateVerificationToken();
            string subject = "Twój kod weryfikacyjny";
            string body = $@"
                            <html>
                              <body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin: 0; padding: 0;'>
                                <table width='100%' cellpadding='0' cellspacing='0'>
                                  <tr>
                                    <td align='center' style='padding: 40px 0;'>
                                      <table width='400' cellpadding='0' cellspacing='0' style='background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); padding: 30px;'>
                                        <tr>
                                          <td align='center' style='padding-bottom: 20px;'>
                                            <h2 style='color: #333333; margin: 0;'>Weryfikacja e-mail</h2>
                                          </td>
                                        </tr>
                                        <tr>
                                          <td style='color: #555555; font-size: 16px; line-height: 1.5; text-align: center;'>
                                            <p>Witaj,</p>
                                            <p>Aby zakończyć rejestrację, użyj poniższego kodu weryfikacyjnego:</p>
                                            <p style='font-size: 24px; font-weight: bold; color: #1a73e8; margin: 20px 0;'>{token}</p>
                                            <p>Kod jest ważny przez <strong>15 minut</strong>.</p>
                                          </td>
                                        </tr>
                                        <tr>
                                          <td style='padding-top: 30px; font-size: 12px; color: #999999; text-align: center;'>
                                            <p>Jeśli nie prosiłeś o ten kod, skontaktuj się z administratorem.</p>
                                          </td>
                                        </tr>
                                      </table>
                                    </td>
                                  </tr>
                                </table>
                              </body>
                            </html>";

            await SendEmailAsync(new EmailDto
            {
                To = toEmail,
                Subject = subject,
                Body = body
            });

            return token;
        }
        private static string GenerateVerificationToken()
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            int number = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
            return number.ToString("D6");
        }
    }
}