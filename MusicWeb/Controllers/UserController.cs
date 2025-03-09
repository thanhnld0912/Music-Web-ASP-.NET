using Microsoft.AspNetCore.Mvc;

namespace MusicWeb.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
