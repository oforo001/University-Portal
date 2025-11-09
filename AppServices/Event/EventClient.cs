using University_Portal.Data;
using University_Portal.AppServices.Event;
using University_Portal.ViewModels.AdminViewModels;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace University_Portal.AppServices.Events
{
    /// <summary>
    /// Statyczna klasa pomocnicza upraszczająca logikę w kontrolerze 'HomeController'.
    /// Wewnętrznie wykorzystuje wzorzec Strategii do obsługi różnych operacji na wydarzeniach.
    /// </summary>
    public static class EventClient
    {
        /// <summary>
        /// Rejestruje użytkownika na wydarzenie, wykorzystując strategię <see cref="RegisterEventStrategy"/>.
        /// </summary>
        public static async Task<(bool Success, string Message)> RegisterAsync(
            ApplicationContext context,
            int eventId,
            string userId)
        {
            var strategy = new RegisterEventStrategy();
            return await ExecuteStrategyAsync(strategy, context, eventId, userId);
        }

        /// <summary>
        /// Anuluje rejestrację użytkownika na wydarzenie, wykorzystując strategię <see cref="CancelEventStrategy"/>.
        /// </summary>
        public static async Task<(bool Success, string Message)> CancelAsync(
            ApplicationContext context,
            int eventId,
            string userId)
        {
            var strategy = new CancelEventStrategy();
            return await ExecuteStrategyAsync(strategy, context, eventId, userId);
        }
        public static async Task<(bool Success, string Message)> CreateEventAsync(
            ApplicationContext context,
            string userId,
            EventCreateViewModel model,
            IWebHostEnvironment env)
        {
            var strategy = new CreateEventStrategy();
            return await ExecuteStrategyAsync(strategy, context, null, userId, model, env);
        }
        public static async Task<(bool Success, string Message)> EditEventAsync(
        ApplicationContext context,
        int eventId,
        string userRole,
        EventCreateViewModel model,
        IWebHostEnvironment env)
        {
            var strategy = new EditEventStrategy();
            return await ExecuteStrategyAsync(strategy, context, eventId, userRole, model, env);
        }



        private static async Task<(bool Success, string Message)> ExecuteStrategyAsync(
            IEventActionStrategy strategy,
            ApplicationContext context,
            int? eventId,
            string userId,
            EventCreateViewModel? model = null,
            IWebHostEnvironment? env = null)
        {
            return await strategy.ExecuteAsync(context, eventId, userId, model, env);
        }
    }
}
