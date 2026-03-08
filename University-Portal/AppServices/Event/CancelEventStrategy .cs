using Microsoft.EntityFrameworkCore;
using University_Portal.Data;
using University_Portal.ViewModels.AdminViewModels;

namespace University_Portal.AppServices.Event
{
    public class CancelEventStrategy : IEventActionStrategy
    {
        public async Task<(bool Success, string Message)> ExecuteAsync(ApplicationContext context, int? eventId, string? userId, EventCreateViewModel? model, IWebHostEnvironment? env)
        {
            var registration = await context.EventRegistrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId && !r.IsCancelled);

            if (registration == null)
                return (false, "Nie znaleziono aktywnej rejestracji.");

            registration.IsCancelled = true;
            await context.SaveChangesAsync();

            return (true, "Anulowano rejestrację.");
        }
    }
}
