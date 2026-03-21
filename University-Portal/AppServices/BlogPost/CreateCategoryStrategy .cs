using University_Portal.Data;
using University_Portal.Models;
using Microsoft.EntityFrameworkCore;

namespace University_Portal.AppServices.Blog
{
    public class CreateCategoryStrategy : IBlogCategoryStrategy
    {
        private const int MaxAllowedCategories = 10;
        private const int MaxNameAllowedLength = 100;
        public async Task<(bool Success, string Message, Category? Result)> ExecuteAsync(
            ApplicationContext context,
            HttpContext httpContext,
            Category? category = null,
            int? categoryId = null)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.Name))
                return (false, "Nazwa kategorii jest wymagana", null);

            category.Name = category.Name.Trim();

            if (category.Name.Length > MaxNameAllowedLength)
                return (false, $"Nazwa kategorii nie może być dłuższa niż {MaxNameAllowedLength} znaków", null);

            var totalCategories = await context.Categories.CountAsync();

            if (totalCategories >= MaxAllowedCategories)
                return (false, $"Nie można utworzyć więcej niż {MaxAllowedCategories} kategorii", null);

            var exists = await context.Categories
                .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());

            if (exists)
                return (false, "Kategoria już istnieje", null);

            context.Categories.Add(category);
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return (false, "Nie udało się utworzyć kategorii", null);
            }

            return (true, "Kategoria została utworzona", category);
        }
    }
}