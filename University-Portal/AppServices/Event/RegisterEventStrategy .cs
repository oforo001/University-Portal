using Microsoft.EntityFrameworkCore;
using University_Portal.Data;
using University_Portal.Models;
using University_Portal.ViewModels.AdminViewModels;

namespace University_Portal.AppServices.Event
{
    public class RegisterEventStrategy : IEventActionStrategy
    {
        public async Task<(bool Success, string Message)> ExecuteAsync(ApplicationContext context, int? eventId, string? userId, EventCreateViewModel? model, IWebHostEnvironment? env)
        {
            int eId = eventId.Value;

            if (string.IsNullOrEmpty(userId))
                return (false, "Nie można zidentyfikować użytkownika.");

            var ev = await context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null)
                return (false, "Wydarzenie nie zostało znalezione.");

            if (ev.Date < DateTime.UtcNow)
                return (false, "Nie można zapisać się na zakończone wydarzenie.");

            var registration = await context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (registration != null && !registration.IsCancelled)
                return (false, "Jesteś już zapisany na to wydarzenie.");

            var activeCount = await context.EventRegistrations
                .CountAsync(r => r.EventId == eventId && !r.IsCancelled);

            if (activeCount >= ev.MaxParticipants)
                return (false, "Brak wolnych miejsc na to wydarzenie.");

            if (registration == null)
            {
                context.EventRegistrations.Add(new EventRegistration
                {
                    EventId = eId,
                    UserId = userId,
                    RegisteredAt = DateTime.UtcNow,
                    IsCancelled = false
                });
            }
            else
            {
                registration.IsCancelled = false;
                registration.RegisteredAt = DateTime.UtcNow;
            }

            try
            {
                await context.SaveChangesAsync();
                return (true, "Zapisano Cię na wydarzenie.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "Wystąpił błąd przy zapisie. Spróbuj ponownie.");
            }
        }
    }
}
