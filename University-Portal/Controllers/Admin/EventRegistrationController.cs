using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Portal.Data;
using University_Portal.AppServices.E_mail;

namespace University_Portal.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/EventRegistration")]
    public class EventRegistrationController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly EmailService _emailService;

        public EventRegistrationController(
            ApplicationContext context,
            EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel([FromBody] CancelRegistrationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserId))
                return BadRequest(new { message = "Nieprawidłowe dane." });

            var registration = await _context.EventRegistrations
                .FirstOrDefaultAsync(x =>
                    x.UserId == request.UserId &&
                    x.EventId == request.EventId &&
                    !x.IsCancelled);

            if (registration == null)
                return NotFound(new { message = "Rejestracja nie istnieje lub została anulowana." });

            registration.IsCancelled = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Zapis został anulowany." });
        }
    }

    public class CancelRegistrationRequest
    {
        public string UserId { get; set; }
        public int EventId { get; set; }
    }
}