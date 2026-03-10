using Microsoft.AspNetCore.Identity;
using University_Portal.AppServices.E_mail;
using University_Portal.Models;
using University_Portal.ViewModels;

namespace University_Portal.AppServices.Account
{
    public class ChangePasswordStrategy : IAccountActionStrategy<ChangePasswordViewModel>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public ChangePasswordStrategy(UserManager<AppUser> userManager, IEmailService emailService)
        {
            this._userManager = userManager;
            this._emailService = emailService;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(ChangePasswordViewModel model)
        {
            if (model.NewPassword != model.ConfirmNewPassword)
                return (false, "Hasła nie są takie same.");

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return (false, "Nie znaleziono użytkownika.");

            if (!user.IsActive)
                return (false, "Konto użytkownika nie jest aktywne.");

            if (string.IsNullOrEmpty(user.EmailVerificationToken) ||
                user.EmailVerificationTokenExpiry == null ||
                user.EmailVerificationTokenExpiry < DateTime.UtcNow ||
                user.EmailVerificationToken != model.VerificationCode)
            {
                return (false, "Nieprawidłowy lub wygasły kod weryfikacyjny.");
            }


            var removeResult = await _userManager.RemovePasswordAsync(user);

            if (!removeResult.Succeeded)
            {
                var error = removeResult.Errors.FirstOrDefault()?.Description
                            ?? "Nie udało się usunąć starego hasła.";
                return (false, error);
            }

            var addResult = await _userManager.AddPasswordAsync(user, model.NewPassword);

            if (!addResult.Succeeded)
            {
                var error = addResult.Errors.FirstOrDefault()?.Description
                            ?? "Nie udało się ustawić nowego hasła.";
                return (false, error);
            }

            return (true, "Hasło zostało zmienione pomyślnie.");
        }
        public async Task SendAndStoreVerificationTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) throw new Exception("Nie znaleziono użytkownika");
            if (!user.IsActive) throw new Exception("Konto nieaktywne");

            string token = await _emailService.SendEmailVerificationAsync(email);

            user.EmailVerificationToken = token;
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15);

            await _userManager.UpdateAsync(user);
        }
    }
}