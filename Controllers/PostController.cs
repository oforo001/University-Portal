using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SQLitePCL;
using University_Portal.Data;
using University_Portal.ViewModels.BlogViewModel;

namespace University_Portal.Controllers
{
    public class PostController : Controller 
    {
        private readonly ApplicationContext _context;
        public PostController(ApplicationContext context)
        {
            _context = context;
        }
        public IActionResult Create()
        {
            var postViewModel = new PostViewModel();
            postViewModel.Categories = _context.Categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name.ToString() }).ToList();
            return View(postViewModel); 
        }
    }
}
