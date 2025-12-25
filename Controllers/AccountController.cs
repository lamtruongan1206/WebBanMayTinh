using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Views;
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
            //var user = context.Users.FirstOrDefault(x => x.Username == name || x.Email == name);
            var user = context.Users.FirstOrDefault(x => x.UserName == name || x.Email == name);
            if (!User.Identity.IsAuthenticated || user == null)
            {
                return Redirect("/login");
            }

            return View(user);
        }


      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProfile (AppUser user, IFormFile? avatar)
        {

            if (!ModelState.IsValid)
                return View(nameof(Profile), user);

            // Lấy user hiện tại trong DB
            var currentUser = context.Users.FirstOrDefault(u => u.Id == user.Id);

            if (currentUser == null)
            {
                TempData["error"] = "Cập nhật thông tin thất bại!";
                return Redirect("/account/profile?name=" + currentUser.UserName);
            }

            // Cập nhật thông tin cơ bản
            currentUser.FirstName = user.FirstName;
            currentUser.LastName = user.LastName;
            currentUser.PhoneNumber = user.PhoneNumber;
            currentUser.Address = user.Address;

            if (avatar != null)
            {
                currentUser.Avatar = await FileUtils.Upload(avatar);
            }

            context.Update(currentUser);
            await context.SaveChangesAsync();

            TempData["success"] = "Cập nhật thông tin thành công!";
            return Redirect("/account/profile?name=" + currentUser.UserName);
        }

        // GET: AccountController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AccountController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserRegisterVM userVM)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Thông tin không hợp lệ";
                return View();
            }

            var currentUser = context.Users.FirstOrDefault(u => u.UserName == userVM.Username || u.Email == userVM.Email);
            if (currentUser != null)
            {
                TempData["error"] = "Tài khoản này đã tồn tại";
                return View();
            }

            try
            {
                var user = new AppUser
                {
                    UserName = userVM.Username,
                    FirstName = userVM.FirstName ?? "",
                    LastName = userVM.LastName ?? "",
                    PhoneNumber = userVM.Phone,
                    Email = userVM.Email,
                };

                var succeeded = await userService.AddUser(user, userVM.Password);
                if (succeeded)
                {
                    TempData["success"] = "Tạo tài khoản thành công";
                    return Redirect("/account/login");
                }
                else
                {
                    TempData["error"] = "Tạo tài khoản thất bại";
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(UserLoginVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Vui lòng kiểm tra lại tài khoản và mật khẩu";
                return View();
            }

            var succeeded = await userService.Login(
                vm.Username, vm.Password);

            if (succeeded)
            {
                return Redirect("/");
            }
            else
            {
                TempData["error"] = "Tài khoản mật khẩu không chính xác";
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            userService.Logout();
            return Redirect("/");   
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
