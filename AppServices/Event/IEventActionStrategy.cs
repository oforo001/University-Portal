using University_Portal.Data;
using University_Portal.ViewModels.AdminViewModels;
using Microsoft.AspNetCore.Hosting;

namespace University_Portal.AppServices.Event
{
    public interface IEventActionStrategy
    {
        Task<(bool Success, string Message)> ExecuteAsync(
            ApplicationContext context,
            int? eventId,
            string? userIdOrRole,
            EventCreateViewModel? model,
            IWebHostEnvironment? env);
    }
}