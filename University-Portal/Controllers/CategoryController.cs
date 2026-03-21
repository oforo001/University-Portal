using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Portal.Data;
using University_Portal.Models;

namespace University_Portal.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationContext _context;

        public CategoryController(ApplicationContext context)
        {
            _context=context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new JsonResult(new { success = false, error = "Category name is required" });

            name = name.Trim();

            var exists = await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());

            if (exists)
                return new JsonResult(new { success = false, error = "Category already exists" });

            var category = new Category
            {
                Name = name
            };

            _context.Categories.Add(category);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new JsonResult(new { success = false, error = "Category already exists" });
            }

            return new JsonResult(new
            {
                success = true,
                id = category.Id,
                name = category.Name,
                message = "Category created successfully"
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new JsonResult(new { success = false, error = "Kategoria nie istnieje" });

            // Optional: prevent deleting categories in use
            var hasPosts = await _context.Posts.AnyAsync(p => p.CategoryId == id);
            if (hasPosts)
                return new JsonResult(new { success = false, error = "Kategoria nie może być usunięta, jest przypisana do postów" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }
    }
}
