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

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
                return (false, "Niepoprawny adres e-mail lub hasło.");

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return (false, "Użytkownik nie został znaleziony.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            return (true, role);
        }
    }
}
