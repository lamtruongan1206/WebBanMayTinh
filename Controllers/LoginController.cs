using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{
    public class LoginController : Controller
    {
    ShopBanMayTinhContext _context = new ShopBanMayTinhContext();


        // GET: /Login
        public IActionResult Index()
        {
            //// Nếu đã login rồi, chuyển thẳng sang Admin hoặc User
            //var role = HttpContext.Session.GetString("UserRole");

            //if (!string.IsNullOrEmpty(role))
            //{
            //    if (role == "Admin")
            //        return RedirectToAction("Index", "Admin"); // Admin
            //    else
            //        return RedirectToAction("Index", "Home"); // User
            //}

            return View();
        }

        // POST: /Login/LoginAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LoginAction(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.LoginError = "Vui lòng nhập email và mật khẩu.";
                return View("Index");
            }

            // Kiểm tra email + password
            var user = _context.Users
                               .Include(u => u.Role)
                               .FirstOrDefault(u => u.Email == email || u.Username == email);
            if (user == null)
            {
                ViewBag.LoginError = "Email hoặc mật khẩu không đúng.";
                return View("Index");
            }

            PasswordHasher<User> hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password ?? "", password);
            if (result == PasswordVerificationResult.Failed)
            {
                Console.WriteLine("Sai mật khẩu");
                ViewBag.LoginError = "Email hoặc mật khẩu không đúng.";
                return View("Index");
            }

            // Lưu thông tin đăng nhập vào session
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");
            HttpContext.Session.SetString("Username", user.Username ?? "");
            HttpContext.Session.SetString("UserRole", user.Role?.Name ?? "User");
            HttpContext.Session.SetString("Avatar", user.Avatar ?? "");
            //HttpContext.Session.SetString("User", JsonSerializer);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            Console.WriteLine("user role: ", user.Role.Name);

            // Chuyển sang Admin hoặc User dựa vào Role
            if (user.Role != null && user.Role.Name == "Admin")
                return RedirectToAction("Index", "Admin");
            //return RedirectToAction("Index", "User");
            return RedirectToAction("Index", "Home");


        }

        // POST: /Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
