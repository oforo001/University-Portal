using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using University_Portal.Data;
using University_Portal.Models;
using University_Portal.ViewModels.Home;
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
        public async Task<IActionResult> Register(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // getting user id


            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId); // displays all events 

            if (registration == null) // if user was not registered
            {
                _context.EventRegistrations.Add(new EventRegistration
                {
                    EventId = id,
                    UserId = userId,
                    RegisteredAt = DateTime.UtcNow,
                    IsCancelled = false
                });

                TempData["Success"] = "You have registered for the event!";
            }
            else if (registration.IsCancelled) // in case user canceled the registration and wants to register one more time
            {
                registration.IsCancelled = false;
                registration.RegisteredAt = DateTime.UtcNow;

                TempData["Success"] = "You have re-registered for the event!";
            }
            else
            {
                TempData["Error"] = "You are already registered for this event.";
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Cancel(int id) 
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var registration = await _context.EventRegistrations.FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId && r.IsCancelled == false);
            if (registration != null) 
            {
                registration.IsCancelled = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "You have cancelled your registration.";
            }
            else
            {
                TempData["Error"] = "Registration not found or already cancelled.";
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
