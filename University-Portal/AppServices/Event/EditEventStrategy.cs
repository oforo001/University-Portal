using Microsoft.EntityFrameworkCore;
using University_Portal.Data;
using University_Portal.ViewModels.AdminViewModels;

namespace University_Portal.AppServices.Event
{
    public class EditEventStrategy : IEventActionStrategy
    {
        public async Task<(bool Success, string Message)> ExecuteAsync(
            ApplicationContext context,
            int? eventId,
            string? userRole,
            EventCreateViewModel? model,
            IWebHostEnvironment? env)
        {
            if (!userRole.Contains("Admin"))
                return (false, "Błąd wewnętrzny");

            if (context == null)
                return (false, "Błąd wewnętrzny: brak kontekstu bazy danych.");
            if (eventId == null)
                return (false, "Nie podano ID wydarzenia.");
            if (model == null)
                return (false, "Brak danych do edycji wydarzenia.");

            var eventToUpdate = await context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventToUpdate == null)
                return (false, "Nie znaleziono wydarzenia.");
            var currentParticipantsCount = await context.EventRegistrations
                .CountAsync(r => r.EventId == eventId && !r.IsCancelled);

            if (model.MaxParticipants < currentParticipantsCount)
            {
                return (false,
                    $"Nie można ustawić maksymalnej liczby uczestników na '{model.MaxParticipants}', " +
                    $"ponieważ '{currentParticipantsCount}' uczestników jest już zapisanych.");
            }

            eventToUpdate.Title = model.Title?.Trim();
            eventToUpdate.Description = model.Description?.Trim();
            eventToUpdate.Date = model.Date;
            eventToUpdate.Location = model.Location?.Trim();
            eventToUpdate.MaxParticipants = model.MaxParticipants;

            if (model.Image != null && model.Image.Length > 0)
            {
                if (model.Image.Length > 4 * 1024 * 1024)
                    return (false, "Rozmiar pliku nie może przekraczać 4MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(model.Image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext))
                    return (false, "Dozwolone są tylko pliki .jpg, .jpeg, .png, .gif.");

                var fileName = $"{Guid.NewGuid()}{ext}";
                var uploadDir = Path.Combine(env.WebRootPath, "uploads", "events");

                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await model.Image.CopyToAsync(stream);

                eventToUpdate.ImagePath = Path.Combine("uploads", "events", fileName)
                    .Replace("\\", "/");
            }

            await context.SaveChangesAsync();

            return (true, $"Wydarzenie: {eventToUpdate.Title} zostało pomyślnie zaktualizowane.");
        }
    }
}
