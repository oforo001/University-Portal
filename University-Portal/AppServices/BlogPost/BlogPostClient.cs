using University_Portal.Data;
using University_Portal.Models;
using Microsoft.AspNetCore.Http;

namespace University_Portal.AppServices.Blog
{
    public static class BlogPostClient
    {
        public static async Task<(bool Success, string Message, Category? Result)> CreateCategoryAsync(
            ApplicationContext context,
            HttpContext httpContext,
            string name)
        {
            var strategy = new CreateCategoryStrategy();
            var category = new Category { Name = name };
            return await strategy.ExecuteAsync(context, httpContext, category);
        }

        public static async Task<(bool Success, string Message)> DeleteCategoryAsync(
            ApplicationContext context,
            HttpContext httpContext,
            int categoryId)
        {
            var strategy = new DeleteCategoryStrategy();
            var (success, message, _) = await strategy.ExecuteAsync(context, httpContext, null, categoryId);
            return (success, message);
        }
    }
}