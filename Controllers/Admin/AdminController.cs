using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace University_Portal.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }
        
        public async Task<IActionResult> CreateEvent()
        {
            return View();
        }
    }
}
