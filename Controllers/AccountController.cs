using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Utils;

namespace WebBanMayTinh.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopBanMayTinhContext context;
        public AccountController(ShopBanMayTinhContext context)
        {
            this.context = context;
        }
        // GET: AccountController
        [HttpGet]
        public ActionResult Profile()
        {
            var username = HttpContext.Session.GetString("Username");

            var user = context.Users.FirstOrDefault(x => x.Username == username);

            if (username == null || user == null)
            {
                return Redirect("/login");
            }

            return View(user);
        }


      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile (User user, IFormFile? avatar)
        {

            Console.WriteLine("Cập nhật thông tin cá nhân");
            if (!ModelState.IsValid)
                return View(nameof(Profile), user);

            Console.WriteLine("Id: " + user.Id);
            Console.WriteLine("Username: " + user.Username);

            // Lấy user hiện tại trong DB
            var currentUser = context.Users.FirstOrDefault(u => u.Id == user.Id);

            if (currentUser == null)
            {
                Console.WriteLine("Không thấy user");
                return NotFound();
            }

            // Cập nhật thông tin cơ bản
            currentUser.FirstName = user.FirstName;
            currentUser.LastName = user.LastName;
            currentUser.Phone = user.Phone;
            currentUser.Address = user.Address;
            currentUser.UpdateAt = DateOnly.FromDateTime(DateTime.Now);
            if (avatar != null)
            {
                currentUser.Avatar = await FileUtils.Upload(avatar);
            }

            context.Update(currentUser);
            await context.SaveChangesAsync();

            TempData["success"] = "Cập nhật thông tin thành công!";

            Console.WriteLine("Trở về trang home");
            return Redirect("/");
        }

        // GET: AccountController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AccountController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AccountController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AccountController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AccountController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AccountController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
