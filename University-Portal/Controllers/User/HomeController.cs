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
    [Authorize]
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

        [HttpPost]
        public async Task<IActionResult> RegisterForEvent(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    message = "Aby zarejestrować się na wydarzenie, musisz być zalogowany."
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var registerUserToEvent = await EventClient.RegisterAsync(_context, id, userId);

            return Json(new { success = registerUserToEvent.Success, message = registerUserToEvent.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    message = "Aby anulować rejestrację, musisz być zalogowany."
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userCancelFromEvent = await EventClient.CancelAsync(_context, id, userId);

            return Json(new { success = userCancelFromEvent.Success, message = userCancelFromEvent.Message });
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
