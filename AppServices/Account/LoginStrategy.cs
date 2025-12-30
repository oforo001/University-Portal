using Microsoft.AspNetCore.Identity;
using University_Portal.Models;
using University_Portal.ViewModels;

namespace University_Portal.AppServices.Account
{
    public class LoginStrategy : IAccountActionStrategy<LoginViewModel>
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public LoginStrategy(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Realizuje proces logowania użytkownika.
        /// Weryfikuje dane uwierzytelniające i zwraca wynik z informacją o roli użytkownika.
        /// </summary>
        public async Task<(bool Success, string Message)> ExecuteAsync(LoginViewModel model)
        {
            if (model == null)
                return (false, "Nieprawidłowe dane logowania.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return (false, "Niepoprawny adres e-mail lub hasło.");

            if (!user.IsActive)
                return (false, "Konto zostało dezaktywowane.");

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
                return (false, "Niepoprawny adres e-mail lub hasło.");

            //updates the AppUser.LastLoginAt for Usermanagement View
            user.LastLoginAt = DateTime.Now;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return (true, role);
        }

    }
}
