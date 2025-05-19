using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace University_Portal.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
