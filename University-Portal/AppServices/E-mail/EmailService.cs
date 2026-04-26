using University_Portal.Models;
using Azure.Communication.Email;
using Azure;

namespace University_Portal.AppServices.E_mail
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly EmailClient _emailClient;
        private readonly string _sender;

        public EmailService(IConfiguration config)
        {
            _config = config;

            var connectionString = _config["AzureEmail:ConnectionString"];

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("Azure Email connection string missing");

            _emailClient = new EmailClient(connectionString);

            _sender = _config["AzureEmail:SenderAddress"];

            if (string.IsNullOrWhiteSpace(_sender))
                throw new Exception("Azure Email sender address missing");
        }

        // =========================
        // CORE SEND METHOD (AZURE)
        // =========================
        public async Task SendEmailAsync(EmailDto request)
        {
            var message = new EmailMessage(
                senderAddress: _sender,
                content: new EmailContent(request.Subject)
                {
                    Html = request.Body,
                    PlainText = "Please view this email in HTML format."
                },
                recipients: new EmailRecipients(new List<EmailAddress>
                {
                    new EmailAddress(request.To)
                })
            );

            try
            {
                var operation = await _emailClient.SendAsync(
                    WaitUntil.Completed,
                    message
                );

                var result = operation.Value;

                Console.WriteLine($"Status: {result.Status}");

                if (result.Status != EmailSendStatus.Succeeded)
                {
                    throw new Exception($"Email sending failed: {result.Status}");
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine("=== AZURE EMAIL ERROR ===");
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // =========================
        // VERIFICATION EMAIL
        // =========================
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

        // =========================
        // WELCOME EMAIL
        // =========================
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

        // =========================
        // PASSWORD RESET EMAIL
        // =========================
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