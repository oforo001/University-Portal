using University_Portal.Data;
using University_Portal.ViewModels.AdminViewModels;
using University_Portal.ViewModels.BlogViewModel;

namespace University_Portal.AppServices.BlogPost
{
    public interface IBlogPostStrategy
    {
        Task<(bool Success, string Message)> ExecuteAsync(
            ApplicationContext context,
            int? postId,
            string? userIdOrRole,
            PostViewModel? model,
            IWebHostEnvironment? env);
    }
}
