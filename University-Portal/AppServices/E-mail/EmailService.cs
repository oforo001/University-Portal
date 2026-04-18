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
        public async Task SendEmailVerificationAsync(string toEmail, string token)
        {
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
                                            <p>Aby zakończyć, użyj poniższego kodu weryfikacyjnego:</p>
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
        }
        public async Task SendWelcomeEmailAsync(string toEmail, string fullName, string password, string role)
        {
            string subject = "Witamy w University Portal!";
            string body = $@"
                            <html>
                              <body style='font-family: Arial, sans-serif; background-color: #f4f6f8; margin:0;padding:0;'>
                                <table width='100%' cellpadding='0' cellspacing='0'>
                                  <tr>
                                    <td align='center' style='padding:40px 0;'>
                                      <table width='400' cellpadding='0' cellspacing='0' style='background-color:#fff;border-radius:8px;padding:30px;'>
                                        <tr>
                                          <td align='center' style='padding-bottom:20px;'>
                                            <h2 style='color:#333;'>Witamy, {fullName}!</h2>
                                          </td>
                                        </tr>
                                        <tr>
                                          <td style='color:#555;font-size:16px;line-height:1.5;text-align:center;'>
                                            <p>Twoje konto zostało utworzone w University Portal.</p>
                                            <p><strong>E-mail:</strong> {toEmail}</p>
                                            <p><strong>Hasło:</strong> {password}</p>
                                            <p><strong>Rola:</strong> {role}</p>
                                            <p>Prosimy zalogować się i zmienić hasło po pierwszym logowaniu.</p>
                                          </td>
                                        </tr>
                                        <tr>
                                          <td style='padding-top:20px;font-size:12px;color:#999;text-align:center;'>
                                            <p>Jeśli nie spodziewałeś się tego maila, skontaktuj się z administratorem.</p>
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
        }
        public async Task SendEmailAsync(EmailDto request)
        {
            var host = _config["EmailHost"];
            var port = int.Parse(_config["Port"]);
            var username = _config["EmailUsername"];
            var password = _config["EmailPassword"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("SMTP config missing.");
            }

            try
            {
                var email = new MimeMessage();

                email.From.Add(MailboxAddress.Parse(username));
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = request.Body
                };

                using var smtp = new SmtpClient
                {
                    Timeout = 30000
                };

                await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(username, password);

                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EMAIL ERROR FULL: {ex}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string token)
        {
            string subject = "Reset hasła";

            string body = $@"
                            <html>
                            <body>
                                <h2>Reset hasła</h2>
                                <p>Użyj poniższego kodu aby zresetować hasło:</p>
                                <h1>{token}</h1>
                                <p>Kod jest ważny przez 15 minut.</p>
                            </body>
                            </html>";

            await SendEmailAsync(new EmailDto
            {
                To = toEmail,
                Subject = subject,
                Body = body
            });
        }

    }
}