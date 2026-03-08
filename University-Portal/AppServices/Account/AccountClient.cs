using Microsoft.AspNetCore.Identity;
using University_Portal.Models;
using University_Portal.ViewModels;

namespace University_Portal.AppServices.Account
{
    /// <summary>
    /// Centralny punkt dostępu do operacji kont użytkowników.
    /// Uproszcza użycie wzorca Strategii w kontrolerach.
    /// </summary>
    public static class AccountClient
    {
        /// <summary>
        /// Tworzy konto administratora podczas pierwszej konfiguracji systemu.
        /// </summary>
        public static async Task<(bool Success, string Message)> AccountSetupAsync(
            UserManager<AppUser> userManager,
            RegisterViewModel model)
        {
            var strategy = new AccountSetupStrategy(userManager);
            return await strategy.ExecuteAsync(model);
        }

        /// <summary>
        /// Loguje użytkownika i zwraca jego rolę.
        /// </summary>
        public static async Task<(bool Success, string Message)> LoginAsync(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            LoginViewModel model)
        {
            var strategy = new LoginStrategy(signInManager, userManager);
            return await strategy.ExecuteAsync(model);
        }
    }
}
