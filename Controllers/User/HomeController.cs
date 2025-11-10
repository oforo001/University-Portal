using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using University_Portal.Data;
using University_Portal.Models;
using University_Portal.ViewModels.Home;
using University_Portal.AppServices.Event;
using University_Portal.AppServices.Events;

namespace University_Portal.Controllers.User
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .OrderByDescending(e => e.Date)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    Date = e.Date,
                    Location = e.Location,
                    ImagePath = e.ImagePath,
                    MaxParticipants = e.MaxParticipants,
                    RegisteredCount = _context.EventRegistrations
                        .Count(r => r.EventId == e.Id && !r.IsCancelled) 
                })
                .ToListAsync();

            List<int> registeredEventIds = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                registeredEventIds = await _context.EventRegistrations
                    .Where(r => r.UserId == userId && !r.IsCancelled) 
                    .Select(r => r.EventId)
                    .ToListAsync();
            }

            ViewBag.RegisteredEventIds = registeredEventIds;
            return View(events);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegisterForEvent(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Nie można zidentyfikować użytkownika.";
                RedirectToAction(nameof(Index));
            }

            var registerUserToEvent = await EventClient.RegisterAsync(_context, id, userId);

            TempData[registerUserToEvent.Success ? "Success" : "Error"] = registerUserToEvent.Message;
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cancel(int id) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Nie można zidentyfikować użytkownika.";
                RedirectToAction(nameof(Index));
            }

            var userCancelFromEvent = await EventClient.CancelAsync(_context, id, userId);

            TempData[userCancelFromEvent.Success ? "Success" : "Error"] = userCancelFromEvent.Message;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
