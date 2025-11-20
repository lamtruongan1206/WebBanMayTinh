using Microsoft.AspNetCore.Mvc;
using WebBanMayTinh.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WebBanMayTinh.Controllers
{
    public class RegisterController : Controller
    {
        ShopBanMayTinhContext conn = new ShopBanMayTinhContext();
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterAction(User user)
        {
            var existingUser = conn.Users.FirstOrDefault(u => (u.Email == user.Email || u.Username == user.Username));
            if (existingUser != null)
            {
                ViewBag.RegisterError = "Tài khoản này đã được sử dụng.";
                return View("Index");
            }
            var roleUser = conn.Roles.FirstOrDefault(r => r.Name == Common.RoleEnums.User.ToString());

            user.UpdateAt = DateOnly.FromDateTime(DateTime.Now);
            user.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
            user.Role = roleUser;

            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

            if (string.IsNullOrEmpty(user.Password))
            {
                ViewBag.RegisterError = "Mật khẩu không thể bỏ trống";
                return View("Index");
            }

            user.Password = passwordHasher.HashPassword(user, user.Password);

            conn.Users.Add(user);
            conn.SaveChanges();
            return RedirectToAction("Index", "Login");
        }
    }
}
