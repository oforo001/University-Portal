using University_Portal.Models;
using University_Portal.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using University_Portal.Helpers;
using University_Portal.AppServices.Account;

namespace University_Portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;
        public AccountController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
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
                return View(viewModel);

            var (success, message) = await AccountClient.LoginAsync(signInManager, userManager, viewModel);

            if (!success)
            {
                TempData["Error"] = message;
                return View(viewModel);
            }

            return message switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // this IActionResult assums there is only one Admin created by first Data seeding -> all new registration Role=User
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser()
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
                    // if registration was successfull assign the registered entity 'user' role
                    await userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in createdUser.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }
            return View(model);
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
