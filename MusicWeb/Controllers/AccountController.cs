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
        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Login()
        {
            CreateAdminIfNotExist();
            return View();
        }

        //Create admin because admin not exist
        private void CreateAdminIfNotExist()
        {
            if (!_context.Users.Any(u => u.Email == "admin@musicweb.com"))
            {
                var admin = new User
                {
                    Username = "Admin",
                    Email = "admin@musicweb.com",
                    Password = "admin123", // Lưu ý: Cần mã hóa mật khẩu trong thực tế
                    Role = "Admin"
                };
                _context.Users.Add(admin);
                _context.SaveChanges();
            }
        }


        public IActionResult Loader()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Lưu thông tin người dùng vào session
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("Username", user.Username);

                // Chuyển hướng đến trang Loader sau khi đăng nhập thành công
                return RedirectToAction("Loader", "Account");
            }

            // Đăng nhập thất bại
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

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
    }

}
