using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using University_Portal.Data;
using University_Portal.Models;
using University_Portal.ViewModels.AdminViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                return NotFound();

            var model = new EventCreateViewModel
            {
                Title = ev.Title,
                Description = ev.Description,
                Date = ev.Date,
                Location = ev.Location,
                MaxParticipants = ev.MaxParticipants,
                ImagePath = ev.ImagePath 
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEvent(int id, EventCreateViewModel model)
        {
            ModelState.Remove("ImagePath");
            ModelState.Remove("Image");

            if (!ModelState.IsValid)
            {
                var ev = await _context.Events.FindAsync(id);
                if (ev != null)
                    model.ImagePath = ev.ImagePath;
                return View(model);
            }

            var eventToUpdate = await _context.Events.FindAsync(id);
            if (eventToUpdate == null)
                return NotFound();

            eventToUpdate.Title = model.Title;
            eventToUpdate.Description = model.Description;
            eventToUpdate.Date = model.Date;
            eventToUpdate.Location = model.Location;
            eventToUpdate.MaxParticipants = model.MaxParticipants;

            // Image update logic (only if new image provided)
            if (model.Image != null && model.Image.Length > 0)
            {
                if (model.Image.Length > 4 * 1024 * 1024)
                {
                    ModelState.AddModelError("Image", "File size must be less than 4MB.");
                    model.ImagePath = eventToUpdate.ImagePath;
                    return View(model);
                }
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(model.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("Image", "Only .jpg, .jpeg, .png, .gif files are allowed.");
                    model.ImagePath = eventToUpdate.ImagePath;
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
                eventToUpdate.ImagePath = Path.Combine("uploads", "events", fileName).Replace("\\", "/");
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Event updated successfully!";
            return RedirectToAction(nameof(Index));
        }




    }
}
