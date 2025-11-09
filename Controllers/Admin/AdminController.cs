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
            ModelState.Remove("ImagePath"); 
            ModelState.Remove("Image"); 

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
                model.ImagePath = (await _context.Events.FindAsync(id))?.ImagePath;
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException();
            var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;

            var editEventResult = await EventClient.EditEventAsync(_context, id, userRole, model, _env);

            if (!editEventResult.Success)
            {
                ModelState.AddModelError(string.Empty, editEventResult.Message);

                var ev = await _context.Events.FindAsync(id);
                if (ev != null)
                    model.ImagePath = ev.ImagePath;

                return View(model);
            }

            TempData["Success"] = editEventResult.Message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("DeleteEvent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>DeleteEvent(int id)
         {
            var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException();
            var userRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;

            var deleteEventResult = await EventClient.DeleteEventAsync(_context, id, userRole, _env);

            TempData[deleteEventResult.Success ? "Success" : "Error"] = deleteEventResult.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
