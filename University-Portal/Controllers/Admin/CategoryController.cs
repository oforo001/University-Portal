using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using University_Portal.AppServices.Blog;
using University_Portal.Data;
using University_Portal.Models;

namespace University_Portal.Controllers.Admin
{
    [Authorize(Roles="Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationContext _context;

        public CategoryController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            var (success, message, category) = await BlogPostClient.CreateCategoryAsync(_context, HttpContext, name);
            return Json(new
            {
                success,
                message,
                id = category?.Id,
                name = category?.Name
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await BlogPostClient.DeleteCategoryAsync(_context, HttpContext, id);
            return Json(new { success, message });
        }
    }
}
