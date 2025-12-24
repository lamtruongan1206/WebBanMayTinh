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
        ShopBanMayTinhContext _context;
        ILogger<LoginController> logger;

        public LoginController(
            ShopBanMayTinhContext context,
            ILogger<LoginController> logger)
        {
            this.logger = logger;
            this._context = context;
        }

        // GET: /Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login/LoginAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAction(string email, string password)
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
                ViewBag.LoginError = "User = null.";
                return View("Index");
            }

            PasswordHasher<User> hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password ?? "", password);
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.LoginError = "Email hoặc mật khẩu không đúng.";
                return View("Index");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
                new Claim("Avatar", user.Avatar ?? "assets/img/avatars/user_default.png")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

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
