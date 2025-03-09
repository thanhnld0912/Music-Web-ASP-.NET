using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MusicWeb.Data;
using MusicWeb.Models;
using System.Linq;
namespace MusicWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role);

                if (user.Role == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
                else
                    return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        /*Wait until finish figma)*/
        //[HttpPost]
        //public IActionResult Login(string email, string password)
        //{
        //    var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

        //    if (user != null)
        //    {
        //        HttpContext.Session.SetString("UserId", user.Id.ToString());
        //        HttpContext.Session.SetString("UserRole", user.Role);
        //        HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl ?? "/images/default-avatar.png");

        //        if (user.Role == "Admin")
        //            return RedirectToAction("Dashboard", "Admin");
        //        else
        //            return RedirectToAction("Index", "Home");
        //    }

        //    ViewBag.Error = "Invalid email or password.";
        //    return View();
        //}


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return RedirectToAction("Login", new { register = "true" });
        }


        [HttpPost]
        public IActionResult Register(string name, string email, string password)
        {
            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.RegisterError = "Email already exists.";
                return View("Login");
            }

            var newUser = new User { Username = name, Email = email, Password = password, Role = "User" };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        /*Wait until finish figma*/
        //[HttpPost]
        //public IActionResult UpdateProfile(IFormFile avatarFile)
        //{
        //    var userId = HttpContext.Session.GetString("UserId");
        //    if (userId == null) return RedirectToAction("Login");

        //    var user = _context.Users.Find(int.Parse(userId));
        //    if (user == null) return NotFound();

        //    if (avatarFile != null)
        //    {
        //        var filePath = Path.Combine("wwwroot/images", avatarFile.FileName);
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            avatarFile.CopyTo(stream);
        //        }
        //        user.AvatarUrl = "/images/" + avatarFile.FileName;
        //        _context.SaveChanges();

        //        HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl);
        //    }

        //    return RedirectToAction("Profile");
        //}


    }

}
