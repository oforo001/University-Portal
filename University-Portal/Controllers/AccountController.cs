using University_Portal.Models;
using University_Portal.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using University_Portal.Helpers;
using University_Portal.AppServices.Account;
using University_Portal.AppServices.E_mail;

namespace University_Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        private readonly EmailService emailService;
        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, EmailService emailService)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailService = emailService;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> AccountSetup()
        {
            var isAdminFound = await userManager.GetUsersInRoleAsync("Admin");

            if (isAdminFound.Any())
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountSetup(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            var (success, message) = await AccountClient.AccountSetupAsync(userManager, viewModel);

            TempData[success ? "Success" : "Error"] = message;

            if (success)
                return RedirectToAction("Login", "Account");

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                return Json(new { success = false, errors });
            }

            var (success, message) = await AccountClient.LoginAsync(signInManager, userManager, viewModel);

            if (!success)
            {
                return Json(new { success = false, errors = new[] { message } });
            }
            string redirectUrl = message switch
            {
                "Admin" => Url.Action("Index", "Admin"),
                _ => Url.Action("Index", "Home")
            };

            return Json(new { success = true, redirectUrl });
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                return Json(new { success = false, errors });
            }

            AppUser user = new AppUser
            {
                FullName = model.Name,
                Email = model.Email,
                UserName = model.Email,
                IsActive = false,
                CreatedAt = DateTime.Now
            };

            var createdUser = await userManager.CreateAsync(user, model.Password);

            if (createdUser.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");

                try
                {
                    string token = await emailService.SendEmailVerificationAsync(user.Email);

                    user.EmailVerificationToken = token;
                    user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddMinutes(15);
                    await userManager.UpdateAsync(user);
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        success = false,
                        errors = new List<string> { "Wysyłka e-maila nie powiodła się: " + ex.Message }
                    });
                }

                return Json(new
                {
                    success = true,
                    message = "Rejestracja zakończona sukcesem! Sprawdź swoją pocztę i wprowadź kod weryfikacyjny.",
                    redirectUrl = Url.Action("VerifyEmail", "Account", new { email = user.Email })
                });
            }
            else
            {
                var errors = createdUser.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, errors });
            }
        }
        public IActionResult VerifyEmail(string email)
        {
            var model = new VerifyEmailViewModel
            {
                Email = email
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();
                return Json(new { success = false, errors });
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Json(new { success = false, errors = new List<string> { "Nie znaleziono użytkownika." } });
            }

            if (user.EmailVerificationToken != model.Token ||
                user.EmailVerificationTokenExpiry == null ||
                user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            {
                return Json(new { success = false, errors = new List<string> { "Nieprawidłowy lub wygasły kod weryfikacyjny." } });
            }

            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.IsActive = true;
            await userManager.UpdateAsync(user);

            return Json(new
            {
                success = true,
                message = "Email zweryfikowany! Możesz teraz zalogować się do systemu.",
                redirectUrl = Url.Action("Login", "Account")
            });
        }

        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                                       .SelectMany(v => v.Errors)
                                       .Select(e => e.ErrorMessage)
                                       .ToList();

                return Json(new { success = false, errors });
            }

            var (success, message) = await AccountClient.ChangePasswordAsync(userManager, model);

            if (!success)
                return Json(new { success = false, errors = new[] { message } });

            return Json(new
            {
                success = true,
                message,
                redirectUrl = Url.Action("Login", "Account")
            });
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }
    }
}
