using University_Portal.Data;
using University_Portal.Models;
using Microsoft.AspNetCore.Http;

namespace University_Portal.AppServices.Blog
{
    public interface IBlogCategoryStrategy
    {
        Task<(bool Success, string Message, Category? Result)> ExecuteAsync(
            ApplicationContext context,
            HttpContext httpContext,
            Category? category = null,
            int? categoryId = null);
    }
}