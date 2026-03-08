using Microsoft.AspNetCore.Identity;
using University_Portal.Helpers;
using University_Portal.Models;
using University_Portal.ViewModels;

namespace University_Portal.AppServices.Account
{
    public class AccountSetupStrategy : IAccountActionStrategy<RegisterViewModel>
    {
        private readonly UserManager<AppUser> _userManager;

        public AccountSetupStrategy(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Tworzy konto administratora podczas pierwszej konfiguracji systemu.
        /// Sprawdza, czy konto administratora już istnieje — jeśli nie, inicjuje proces jego utworzenia.
        /// </summary>
        public async Task<(bool Success, string Message)> ExecuteAsync(RegisterViewModel model)
        {
            if (model == null)
                return (false, "Nieprawidłowe dane rejestracji.");

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Any())
                return (false, "Konto administratora już istnieje.");

            var adminUser = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.Name
            };

            var result = await _userManager.CreateAsync(adminUser, model.Password);
            if (!result.Succeeded)
            {
                var errorMsg = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, $"Nie udało się utworzyć konta administratora: {errorMsg}");
            }

            await _userManager.AddToRoleAsync(adminUser, "Admin");
            AdminSetupState.IsInitialSetUpRequered = false;

            return (true, "Konto administratora zostało pomyślnie utworzone.");
        }
    }
}
