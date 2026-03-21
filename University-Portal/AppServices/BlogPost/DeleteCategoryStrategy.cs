using University_Portal.Data;
using Microsoft.EntityFrameworkCore;
using University_Portal.Models;

namespace University_Portal.AppServices.Blog
{
    public class DeleteCategoryStrategy : IBlogCategoryStrategy
    {
        public async Task<(bool Success, string Message, Category? Result)> ExecuteAsync(
            ApplicationContext context,
            HttpContext httpContext,
            Category? category = null,
            int? categoryId = null)
        {
            if (!categoryId.HasValue)
                return (false, "Nieprawidłowy identyfikator kategorii", null);

            var cat = await context.Categories.FindAsync(categoryId.Value);
            if (cat == null)
                return (false, "Kategoria nie istnieje", null);

            var hasPosts = await context.Posts.AnyAsync(p => p.CategoryId == categoryId.Value);
            if (hasPosts)
                return (false, "Kategoria nie może być usunięta, jest przypisana do postów", null);

            context.Categories.Remove(cat);
            await context.SaveChangesAsync();

            return (true, "Kategoria została usunięta", null);
        }
    }
}