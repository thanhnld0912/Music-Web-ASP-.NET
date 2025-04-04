using Microsoft.AspNetCore.Mvc;
using MusicWeb.Models;

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

        // Create admin if it does not exist
        private void CreateAdminIfNotExist()
        {
            if (!_context.Users.Any(u => u.Email == "admin@musicweb.com"))
            {
                var admin = new User
                {
                    Username = "Admin",
                    Email = "admin@musicweb.com",
                    Password = "admin123", // Lưu ý: Cần mã hóa mật khẩu trong thực tế
                    Role = "Admin",
                    Bio = "No content"  // Cung cấp giá trị mặc định cho trường Bio
                };
                _context.Users.Add(admin);
                _context.SaveChanges();

                // Automatically log in the admin after creation
                var user = _context.Users.FirstOrDefault(u => u.Email == "admin@musicweb.com");
                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("Username", user.Username);
                }
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

                // Kiểm tra nếu người dùng là admin và chuyển hướng đến newfeedadmin
                if (user.Email == "admin@musicweb.com")
                {
                    // Điều hướng đến trang newfeedadmin
                    return RedirectToAction("NewFeedAdmin", "Home");
                }

                // Nếu không phải admin, điều hướng đến trang newfeed
                return RedirectToAction("NewFeed", "Home");
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

            var newUser = new User { Username = name, Email = email, Password = password, Role = "User" , Bio ="No Bio" };
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }
    }

}

