using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Services;
using WebBanMayTinh.Utils;

namespace WebBanMayTinh.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShopBanMayTinhContext context;
        private ILogger<AccountController> logger;
        private IUserService userService;
        public AccountController(
            ShopBanMayTinhContext context,
            ILogger<AccountController> logger,
            IUserService userService)
        {
            this.context = context;
            this.logger = logger;
            this.userService = userService;
        }
        // GET: AccountController
        [HttpGet]
        public ActionResult Profile(string? name = "")
        {
            var user = context.Users.FirstOrDefault(x => x.Username == name || x.Email == name);

            if (!User.Identity.IsAuthenticated || user == null)
            {
                return Redirect("/login");
            }


            return View(user);
        }


      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile (User user, IFormFile? avatar)
        {

            if (!ModelState.IsValid)
                return View(nameof(Profile), user);

            // Lấy user hiện tại trong DB
            var currentUser = context.Users.FirstOrDefault(u => u.Id == user.Id);

            if (currentUser == null)
            {
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
