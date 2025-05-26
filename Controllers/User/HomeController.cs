using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using University_Portal.Data;
using University_Portal.Models;
using System.Security.Claims;

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
        // This returns all Events after user launch the Index Page
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            // If there are any registered Events for auth. User -> it will be shown in View/Home.Index.cshtml -> if not - skip
            List<int> registeredEventIds = new List<int>();
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                registeredEventIds = await _context.EventRegistrations
                    .Where(r => r.UserId == userId)
                    .Select(r => r.EventId)
                    .ToListAsync();
            }
            // passing list with registered Events to the Index.cshtml
            ViewBag.RegisteredEventIds = registeredEventIds;
            return View(events);
        }
        [Authorize]
        public async Task<IActionResult> Register(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // This is the Identity UserId

            // Prevent duplicate registrations
            bool alreadyRegistered = await _context.EventRegistrations
                .AnyAsync(r => r.EventId == id && r.UserId == userId);

            if (!alreadyRegistered)
            {
                _context.EventRegistrations.Add(new EventRegistration
                {
                    EventId = id,
                    UserId = userId,
                    RegisteredAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["Success"] = "You have registered for the event!";
            }
            else
            {
                TempData["Error"] = "You are already registered for this event.";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
