using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Controllers
{
    public class LoginController : Controller
    {
    ShopBanMayTinhContext _context = new ShopBanMayTinhContext();


        // GET: /Login
        public IActionResult Login()
        {
            // Nếu đã login rồi, chuyển thẳng sang Admin hoặc User
            var role = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(role))
            {
                if (role == "Admin")
                    return RedirectToAction("Index", "Admin"); // Admin
                else
                    return RedirectToAction("Index", "User"); // User
            }

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
                return View("Login");
            }

            // Kiểm tra email + password
            var user = _context.Users
                               .Include(u => u.Role)
                               .FirstOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Lưu thông tin đăng nhập vào session
                HttpContext.Session.SetString("UserEmail", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role?.Name ?? "User");

                // Chuyển sang Admin hoặc User dựa vào Role
                if (user.Role != null && user.Role.Name == "Admin")
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "User");
            }

            // Nếu đăng nhập sai
            ViewBag.LoginError = "Email hoặc mật khẩu không đúng.";
            return View("Login");
        }

        // POST: /Login/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Xóa session khi đăng xuất
            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }
    }
}
