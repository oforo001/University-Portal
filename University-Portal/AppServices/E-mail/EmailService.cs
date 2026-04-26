using University_Portal.Models;
using PostmarkDotNet;

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
        //public async Task SendEmailAsync(EmailDto request)
        //{
        //    var apiKey = _config["Postmark:ApiKey"];
        //    var fromEmail = _config["Postmark:FromEmail"];
        //    var fromName = _config["Postmark:FromName"];

        //    if (string.IsNullOrEmpty(apiKey))
        //        throw new Exception("Postmark API key missing");

        //    var client = new PostmarkClient(apiKey);

        //    try
        //    {
        //        var message = new PostmarkMessage
        //        {
        //            From = fromEmail,
        //            To = request.To,
        //            Subject = request.Subject,
        //            HtmlBody = request.Body,
        //            TrackOpens = true
        //        };

        //        var response = await client.SendMessageAsync(message);

        //        if (response.Status != PostmarkStatus.Success)
        //        {
        //            throw new Exception($"Postmark error: {response.Message}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("PostmarkResponseException:");
        //        Console.WriteLine(ex.Message);
        //        Console.WriteLine(ex.InnerException?.Message);
        //        throw;
        //    }
        //}
        public async Task SendEmailAsync(EmailDto request)
        {
            var apiKey = _config["Postmark:ApiKey"]?.Trim();
            var fromEmail = _config["Postmark:FromEmail"]?.Trim();
            var fromName = _config["Postmark:FromName"]?.Trim();

            Console.WriteLine("=== EMAIL DEBUG START ===");

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Postmark API key missing");

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new Exception("Postmark FromEmail missing");

            Console.WriteLine($"ApiKey (first 6): {apiKey.Substring(0, Math.Min(6, apiKey.Length))}...");
            Console.WriteLine($"FromEmail: '{fromEmail}'");
            Console.WriteLine($"FromName: '{fromName}'");
            Console.WriteLine($"To: '{request.To}'");
            Console.WriteLine($"Subject: '{request.Subject}'");

            var client = new PostmarkClient(apiKey);

            try
            {
                var message = new PostmarkMessage
                {
                    // ✅ ALWAYS use email only for debugging
                    From = fromEmail,

                    To = request.To,
                    Subject = request.Subject,
                    HtmlBody = request.Body,
                    TrackOpens = true
                };

                Console.WriteLine("Sending email...");

                var response = await client.SendMessageAsync(message);

                Console.WriteLine("=== POSTMARK RESPONSE ===");
                Console.WriteLine($"Status: {response.Status}");
                Console.WriteLine($"Message: {response.Message}");
                Console.WriteLine($"ErrorCode: {response.ErrorCode}");

                if (response.Status != PostmarkStatus.Success)
                {
                    throw new Exception(
                        $"Postmark error: {response.Message}, Code: {response.ErrorCode}");
                }

                Console.WriteLine("=== EMAIL SENT SUCCESS ===");
            }
            catch (Postmark.Exceptions.PostmarkResponseException ex)
            {
                Console.WriteLine("=== POSTMARK RESPONSE EXCEPTION ===");
                Console.WriteLine(ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== GENERAL EMAIL ERROR ===");
                Console.WriteLine(ex.ToString());
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