using Microsoft.AspNetCore.Mvc;

namespace MusicWeb.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult NewFeedAdmin()
        {
            return View();
        }
        public IActionResult Manage()
        {
            return View();
        }
    }
}
