using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using University_Portal.Data;
using University_Portal.Models;
using University_Portal.ViewModels.AdminViewModels;

namespace University_Portal.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(ApplicationContext context, IWebHostEnvironment env, UserManager<AppUser> userManager)
        {
               _context = context;
               _env = env;
               _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var showEvents = await _context.Events.OrderByDescending(e => e.Date).ToListAsync();
            return View(showEvents);
        }
        [HttpGet]
        public IActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string imagePath = null;
            if (model.Image != null && model.Image.Length > 0)
            {

                if (model.Image.Length > 4 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", "File size must be less than 4MB.");
                    return View(model);
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(model.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("Image", "Only .jpg, .jpeg, .png, .gif files are allowed.");
                    return View(model);
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var uploadPath = Path.Combine(_env.WebRootPath, "uploads", "events");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }
                // Save relative path for DB
                imagePath = Path.Combine("uploads", "events", fileName).Replace("\\", "/");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var newEvent = new Event
            {
                Title = model.Title,
                Description = model.Description,
                ImagePath = imagePath,
                Date = model.Date,
                Location = model.Location,
                MaxParticipants = model.MaxParticipants,
                OrganizerId = user.Id
            };

            _context.Events.Add(newEvent); // Save to DB
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }

    }
}
