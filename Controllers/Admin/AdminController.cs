using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using University_Portal.AppServices.Events;
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
        public IActionResult CreateEvent() // Views/Admin/CreateEvent.cshtml
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEvent(EventCreateViewModel model)
        {
            ModelState.Remove("ImagePath"); // this is the 'workaround' step to prevent 'Image is requered' errormessage
            ModelState.Remove("Image"); // same here

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var createEventResult = await EventClient.CreateEventAsync(_context, user.Id, model, _env);
            TempData[createEventResult.Success ? "Success" : "Error"] = createEventResult.Message;
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound();

            var registeredUsers = await _context.EventRegistrations
                .Where(r => r.EventId == id && !r.IsCancelled)
                .Include(r => r.User)
                .Select(r => new RegisteredUser
                {
                    UserId = r.User.Id,
                    UserName = r.User.UserName,
                    Email = r.User.Email,
                    RegisteredAt = r.RegisteredAt
                })
                .ToListAsync();

            var model = new EventCreateViewModel
            {
                Title = ev.Title,
                Description = ev.Description,
                Date = ev.Date,
                Location = ev.Location,
                MaxParticipants = ev.MaxParticipants,
                ImagePath = ev.ImagePath,
                RegisteredUsers = registeredUsers
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

            var eventToUpdate = await _context.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == id);

            if (eventToUpdate == null)
                return NotFound();

            var currentParticipiantsCount = await _context.EventRegistrations
                .CountAsync(r => r.EventId == id && !r.IsCancelled);

            if (model.MaxParticipants < currentParticipiantsCount)
            {
                ModelState.AddModelError("MaxParticipants",
                    $"Nie można ustawić maksymalnej liczby uczestników na '{model.MaxParticipants}', ponieważ " +
                    $"'{currentParticipiantsCount}' uczestników jest już zapisanych na to wydarzenie.");

                model.ImagePath = eventToUpdate.ImagePath; 
                return View(model);
            }

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
        [HttpPost, ActionName("DeleteEvent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>DeleteEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound();
            // Delete the image file if it exists
            if (!string.IsNullOrEmpty(ev.ImagePath))
            {
                var filePath = Path.Combine(_env.WebRootPath, ev.ImagePath.Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
