using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Portal.Models;
using University_Portal.ViewModels.AdminViewModels;

namespace University_Portal.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserManagementController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var userListViewModel = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userListViewModel.Add(new UserListViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles,
                    IsActive = user.IsActive,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt,
                    LastUpdatedAt = user.LastUpdatedAt
                });
            }

            return View(userListViewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest(new { message = string.Join(", ", errors) });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Użytkownik z tym adresem e-mail już istnieje." });

            if (!await _roleManager.RoleExistsAsync(model.Role))
                return BadRequest(new { message = "Wybrana rola nie istnieje." });

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(500, new { message = errors });
            }

            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new { message = "Użytkownik został utworzony." });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Nie znaleziono użytkownika." });

            if (user.IsActive)
                return BadRequest(new { success = false, message = "Użytkownik jest już aktywny." });

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded
                ? Ok(new { success = true, message = "Użytkownik został aktywowany." })
                : StatusCode(500, new { success = false, message = "Błąd podczas aktywowania użytkownika." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { success = false, message = "Nie znaleziono użytkownika." });

            var currentUserId =  _userManager.GetUserId(User);
            if (currentUserId == user.Id)
                return BadRequest(new { success = false, message = "Nie możesz deaktywowac własnego konta." });

            if (!user.IsActive)
                return BadRequest(new { success = false, message = "Użytkownik jest już deaktywowany." });

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded
                ? Ok(new { success = true, message = "Użytkownik został deaktywowany." })
                : StatusCode(500, new { success = false, message = "Błąd podczas deaktywowania użytkownika." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { message = "Nieprawidłny identyfikator użytkownika." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Nie znaleziono użytkownika." });

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == user.Id)
                return BadRequest(new { message = "Nie możesz usunąć własnego konta." });

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count == 1 && admins.First().Id == user.Id)
                return BadRequest(new { message = "Nie możesz usunąć ostatniego administratora." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(500, new { message = $"Błąd usuwania użytkownika: {errors}" });
            }

            return Ok(new { message = "Użytkownik został usunięty." });
        }

    }
}
