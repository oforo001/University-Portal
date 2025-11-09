using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using University_Portal.Data;
using University_Portal.Models;
using University_Portal.ViewModels.AdminViewModels;

namespace University_Portal.AppServices.Event
{
    public class CreateEventStrategy : IEventActionStrategy
    {
        public async Task<(bool Success, string Message)> ExecuteAsync(
            ApplicationContext context,
            int? eventId,
            string userId,
            EventCreateViewModel? model = null,
            IWebHostEnvironment? env = null)

        {
            if (model == null)
                return (false, "Brak danych wydarzenia.");

            if (env == null)
                return (false, "Środowisko aplikacji nie jest dostępne.");

            // Validate image
            string imagePath = null;
            if (model.Image != null && model.Image.Length > 0)
            {
                if (model.Image.Length > 4 * 1024 * 1024)
                    return (false, "Rozmiar pliku musi być mniejszy niż 4MB.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var ext = Path.GetExtension(model.Image.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                    return (false, "Dozwolone formaty: .jpg, .jpeg, .png, .gif.");

                var fileName = $"{Guid.NewGuid()}{ext}";
                var uploadPath = Path.Combine(env.WebRootPath, "uploads", "events");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imagePath = Path.Combine("uploads", "events", fileName).Replace("\\", "/");
            }

            var newEvent = new Models.Event
            {
                Title = model.Title,
                Description = model.Description,
                ImagePath = imagePath,
                Date = model.Date,
                Location = model.Location,
                MaxParticipants = model.MaxParticipants,
                OrganizerId = userId
            };

            context.Events.Add(newEvent);
            await context.SaveChangesAsync();

            return (true, "Wydarzenie zostało utworzone pomyślnie!");
        }
    }
}
