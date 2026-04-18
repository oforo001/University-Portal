using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public IActionResult Details(int id)
        {
            if (id == null)
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostViewModel postViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return Json(new { error = string.Join("<br>", errors) });
            }

            postViewModel.Post.Id = 0;

            if (postViewModel.FeatureImage != null && postViewModel.FeatureImage.Length > 0)
            {
                if (postViewModel.FeatureImage.Length > MaxFileSize)
                {
                    return Json(new { error = "Rozmiar obrazu musi być mniejszy niż 4MB." });
                }

                var ext = Path.GetExtension(postViewModel.FeatureImage.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(ext))
                {
                    return Json(new { error = "Dozwolone formaty: .jpg, .jpeg, .png" });
                }

                postViewModel.Post.FeatureImagePath =
                    await UploadFileToFolder(postViewModel.FeatureImage);
            }

            await _context.Posts.AddAsync(postViewModel.Post);
            await _context.SaveChangesAsync();


            return Json(new
            {
                success = true,
                message = "Post został dodany.",
                redirectUrl = Url.Action("Index", "Post")
            });
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var postFromDB = await _context.Posts.FirstOrDefaultAsync(post => post.Id == id);
            if (postFromDB == null)
            {
                return NotFound();
            }

            EditPostViewModel editPostViewModel = new EditPostViewModel
            {
                Post = postFromDB,
                Categories = _context.Categories
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList()
            };
            return View(editPostViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditPostViewModel editPostViewModel)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return Json(new { error = string.Join("<br>", errors) });
            }

            var postFromDB = await _context.Posts.AsNoTracking()
                                                 .FirstOrDefaultAsync(p => p.Id == editPostViewModel.Post.Id);

            if (editPostViewModel.FeatureImage != null)
            {
                var ext = Path.GetExtension(editPostViewModel.FeatureImage.FileName).ToLower();
                if (!_allowedExtensions.Contains(ext))
                {
                    return Json(new { error = "Nieprawidłowy format obrazu. Dozwolone formaty: .jpg, .jpeg, .png" });
                }

                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads",
                                                    Path.GetFileName(postFromDB.FeatureImagePath));

                if (System.IO.File.Exists(existingFilePath))
                    System.IO.File.Delete(existingFilePath);

                editPostViewModel.Post.FeatureImagePath = await UploadFileToFolder(editPostViewModel.FeatureImage);
            }
            else
            {
                editPostViewModel.Post.FeatureImagePath = postFromDB.FeatureImagePath;
            }

            _context.Posts.Update(editPostViewModel.Post);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Post został zaktualizowany." });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddComment([FromBody] Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Content))
                return Json(new { error = "Komentarz nie może być pusty." });

            var postExists = await _context.Posts.AnyAsync(p => p.Id == comment.PostId);

            if (!postExists)
                return Json(new { error = "Nieprawidłowy post." });

            comment.Id = 0;

            comment.UserName = User.Identity?.Name ?? "Nieznany";

            comment.CommentDate = DateTime.UtcNow;

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            var warsawTime = TimeZoneInfo.ConvertTimeFromUtc(
                comment.CommentDate,
                TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time")
            );

            return Json(new
            {
                id = comment.Id,
                username = comment.UserName,
                commentDate = warsawTime.ToString("MMM dd, yyyy HH:mm"),
                content = comment.Content
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid("Musisz być administratorem, aby usunąć komentarz.");

            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return NotFound(new { error = "Nie znaleziono komentarza." });

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Komentarz został usunięty." });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!User.IsInRole("Admin"))
                return Forbid("Musisz być administratorem, aby usunąć post.");

            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post został usunięty." });
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