using Microsoft.EntityFrameworkCore;
using University_Portal.Data;

namespace University_Portal.AppServices.Event
{
    public class DeleteEventStrategy : IEventActionStrategy
    {
        public async Task<(bool Success, string Message)> ExecuteAsync(
            ApplicationContext context,
            int? eventId,
            string? userRole,
            ViewModels.AdminViewModels.EventCreateViewModel? model,
            IWebHostEnvironment? env)
        {
            if (!userRole.Contains("Admin"))
                return (false, "Błąd wewnętrzny");

            if (context == null)
                return (false, "Błąd wewnętrzny: brak kontekstu bazy danych.");

            if (eventId == null)
                return (false, "Nie podano ID wydarzenia.");

            var ev = await context.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            if (ev == null)
                return (false, "Nie znaleziono wydarzenia.");

            if (!string.IsNullOrEmpty(ev.ImagePath) && env != null)
            {
                try
                {
                    var filePath = Path.Combine(env.WebRootPath,
                        ev.ImagePath.Replace('/', Path.DirectorySeparatorChar));

                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    return (false, $"Nie udało się usunąć pliku obrazu: {ex.Message}");
                }
            }

            context.Events.Remove(ev);
            await context.SaveChangesAsync();

            return (true, $"Wydarzenie '{ev.Title}' zostało pomyślnie usunięte.");
        }
    }
}
