using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using University_Portal.Data;
using University_Portal.Models;
using University_Portal.ViewModels.BlogViewModel;

namespace University_Portal.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private const long MaxFileSize = 4 * 1024 * 1024;

        public PostController(ApplicationContext context, IWebHostEnvironment webHE)
        {
            _context = context;
            _webHostEnvironment = webHE;
        }

        [HttpGet]
        public IActionResult Index(int? categoryId)
        {
            IQueryable<Post> postQuery = _context.Posts
                .Include(p => p.Category);

            if (categoryId.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            var posts = postQuery.ToList();

            ViewBag.Categories = _context.Categories.ToList();

            return View(posts);
        }
        [HttpGet]
        public IActionResult Details(int id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var post = _context.Posts.Include(p => p.Category).Include(p => p.Comments)
                .FirstOrDefault(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PostViewModel
            {
                Categories = _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (!ModelState.IsValid)
            {
                ReloadCategories(postViewModel);
                return View(postViewModel);
            }
            if (postViewModel.FeatureImage != null && postViewModel.FeatureImage.Length > 0)
            {
                if (postViewModel.FeatureImage.Length > MaxFileSize)
                {
                    ModelState.AddModelError("", "Image size must be less than 4MB.");
                    ReloadCategories(postViewModel);
                    return View(postViewModel);
                }

                var ext = Path.GetExtension(postViewModel.FeatureImage.FileName).ToLowerInvariant();

                if (!_allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", "Allowed formats: .jpg, .jpeg, .png");
                    ReloadCategories(postViewModel);
                    return View(postViewModel);
                }

                postViewModel.Post.FeatureImagePath =
                    await UploadFileToFolder(postViewModel.FeatureImage);
            }

            await _context.Posts.AddAsync(postViewModel.Post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void ReloadCategories(PostViewModel model)
        {
            model.Categories = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
        }

        private async Task<string> UploadFileToFolder(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid()}{ext}";

            var uploadsFolderPath = Path.Combine(
                _webHostEnvironment.WebRootPath,
                "uploads",
                "posts"
            );

            if (!Directory.Exists(uploadsFolderPath))
                Directory.CreateDirectory(uploadsFolderPath);

            var filePath = Path.Combine(uploadsFolderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/posts/{fileName}";
        }

    }
}
