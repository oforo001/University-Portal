using Microsoft.AspNetCore.Identity;
using University_Portal.AppServices.E_mail;
using University_Portal.Models;

namespace University_Portal.AppServices.Account
{
    public class SendPasswordResetTokenStrategy : IAccountActionStrategy<string>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _tokenService;

        public SendPasswordResetTokenStrategy(UserManager<AppUser> userManager, IEmailService emailService, IVerificationTokenService tokenService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _tokenService = tokenService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return (false, "Nie znaleziono użytkownika.");

            if (!user.IsActive)
                return (false, "Konto nieaktywne.");

            var token = _tokenService.GenerateToken();

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);

            await _emailService.SendPasswordResetEmailAsync(email, token);

            return (true, "Kod resetu został wysłany.");
        }
    }
}