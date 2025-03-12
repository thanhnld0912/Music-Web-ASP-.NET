using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MusicWebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult NewFeed()
        {
            return View();
        }

        public IActionResult ProfileUser()
        {
            return View();
        }


        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Loader()
        {
            return View();
        }
    }
}