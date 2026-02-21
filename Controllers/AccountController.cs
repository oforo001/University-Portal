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
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var createdUser = await userManager.CreateAsync(user, model.Password);

            if (createdUser.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "User");

                try
                {
                    await emailService.SendEmailAsync(new EmailDto
                    {
                        To = user.Email,
                        Subject = "Witamy w University Portal!",
                        Body = @"
                            <html>
                            <body style='font-family: Arial, sans-serif; color: #333;'>
                              <div style='max-width: 600px; margin: auto; border: 1px solid #ccc; padding: 20px; border-radius: 10px;'>
                                  <h2 style='color: #2c3e50;'>Witamy w University Portal!</h2>
                                  <p>Dziękujemy za rejestrację. Twój dostęp do portalu został utworzony.</p>
                                  <p>
                                      <a href='https://your-portal.com/login'
                                          style='display: inline-block; padding: 10px 20px; background-color: #28a745; color: white; text-decoration: none; border-radius: 5px;'>
                                             Zaloguj się teraz
                                     </a>
                                </p>
                                 <p style='font-size: 0.85em; color: #777;'>
                                    Jeżeli nie rejestrowałeś się na portalu, skontaktuj się z administratorem.
                                 </p>
                            </div>
                          </body>
                          </html>"
                    });
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
                    message = "Rejestracja zakończona sukcesem! Możesz się teraz zalogować."
                });
            }
            else
            {
                var errors = createdUser.Errors.Select(e => e.Description).ToList();
                return Json(new { success = false, errors });
            }
        }
        public IActionResult VerifyEmail()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Something went wrong");
                    return View(model);
                }
                else
                {
                    return RedirectToAction("ChangePassword", "Account",
                        new { username = user.UserName });
                }
            }
            return View(model);
        }

        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddPasswordAsync(user, model.NewPassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email not found");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Something went wrong");
                return View(model);
            }
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");

        }
    }
}
