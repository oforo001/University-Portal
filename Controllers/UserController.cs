using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace University_Portal.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
