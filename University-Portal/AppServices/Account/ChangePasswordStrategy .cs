using Microsoft.AspNetCore.Identity;
using University_Portal.Models;
using University_Portal.ViewModels;

namespace University_Portal.AppServices.Account
{
    public class ChangePasswordStrategy : IAccountActionStrategy<ChangePasswordViewModel>
    {
        private readonly UserManager<AppUser> userManager;

        public ChangePasswordStrategy(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(ChangePasswordViewModel model)
        {
            if (model.NewPassword != model.ConfirmNewPassword)
                return (false, "Hasła nie są takie same.");

            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return (false, "Nie znaleziono użytkownika.");

            if (!user.IsActive)
                return (false, "Konto użytkownika nie jest aktywne.");

            var removeResult = await userManager.RemovePasswordAsync(user);

            if (!removeResult.Succeeded)
            {
                var error = removeResult.Errors.FirstOrDefault()?.Description
                            ?? "Nie udało się usunąć starego hasła.";
                return (false, error);
            }

            var addResult = await userManager.AddPasswordAsync(user, model.NewPassword);

            if (!addResult.Succeeded)
            {
                var error = addResult.Errors.FirstOrDefault()?.Description
                            ?? "Nie udało się ustawić nowego hasła.";
                return (false, error);
            }

            return (true, "Hasło zostało zmienione pomyślnie.");
        }
    }
}