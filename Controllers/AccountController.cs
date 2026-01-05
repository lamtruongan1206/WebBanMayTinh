using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Views;
using WebBanMayTinh.Services;
using WebBanMayTinh.Utils;

namespace WebBanMayTinh.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext context;
        private ILogger<AccountController> logger;
        private IUserService userService;
        private IEmailSender emailSender;
        public AccountController(
            DataContext context,
            ILogger<AccountController> logger,
            IUserService userService,
            IEmailSender emailSender)
        {
            this.context = context;
            this.logger = logger;
            this.userService = userService;
            this.emailSender = emailSender;
        }
        
        [HttpGet, Authorize]
        public ActionResult Profile(string? name = "")
        {
            var user = context.Users.FirstOrDefault(x => x.UserName == name || x.Email == name);
            if (!User.Identity.IsAuthenticated || user == null)
            {
                return Redirect("/login");
            }
            return View(user);
        }
      
        [HttpPost, Authorize]
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

        public ActionResult Create()
        {
            return View();
        }

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
            if (currentUser != null && currentUser.EmailConfirmed == true)
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

                var result = await userService.Register(user, userVM.Password);
                
                if (result.Succeeded)
                {
                    var token = await userService.GenerateEmailConfirmToken(user);

                    var url = Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new  {userId = user.Id, token},
                        Request.Scheme
                        );

                    await emailSender.SendEmailAsync(user.Email, "Xác thực tài khoản", $"Click vào link để xác nhận: {url}");
                    TempData["success"] = "Tạo tài khoản thành công, hãy vào gmail để xác thực tài khoản trước nhé";
                    return Redirect("/account/login");
                }
                else
                {
                    if (result.Errors.Count() >= 1)
                    {
                        TempData["error"] = result.Errors.First().Code.Contains("PasswordRequiresDigit") ? "Mật khẩu yêu cầu phải có cả số và chữ" : "Tạo tài khoản thất bại";
                    }
                    else
                    {
                        TempData["error"] = "Tạo tài khoản thất bại";
                    }

                    return View();
                }
            }
            catch
            {
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest();

            var result = await userService.ConfirmEmail(userId, token);

            if (result is null) return BadRequest();

            if (result.Succeeded)
            {
                return Redirect("/");
            }
            else
            {
                return Redirect("/account/login");
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

            var signInResult = await userService.Login(
                vm.Username, vm.Password);

            if (signInResult.Succeeded)
            {
                return Redirect("/");
            }
            else if (signInResult.IsNotAllowed)
            {
                TempData["error"] = "Tài khoản của bạn chưa được xác thực, hãy vào gmail để xác thực tài khoản";
                return View();
            } 
            else
            {
                TempData["error"] = "Tài khoản mật khẩu không chính xác";
                return View(vm);
            }
        }


        [HttpPost, Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            userService.Logout();
            return Redirect("/");   
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost, Authorize]
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

        public async Task<IActionResult> ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!await userService.IsExisted(email))
            {
                TempData["error"] = "Tài khoản này không tồn tại";
                return View("ForgotPassword");
            }

            var token = await userService.GeneratePasswordResetTokenAsync(email);

            var callbackUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { token, email },
                Request.Scheme
            );

            await emailSender.SendEmailAsync(
                email,
                "Reset mật khẩu",
                $"Click vào link: {callbackUrl}"
            );

            TempData["success"] = "Email xác thực đã được gửi đến Email của bạn, hãy kiếm tra và đổi lại mật khẩu";
            return View("ForgotPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View(new ResetPasswordVM
            {
                Token = token,
                Email = email
            });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (!await userService.IsExisted(model.Email))
                return RedirectToAction("ResetPasswordConfirmation");

            var result = await userService.ResetPasswordAsync(model);

            if (result.Succeeded)
            {
                TempData["success"] = "Đổi mật khẩu mới thành công";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> ChangePassword(string name)
        {
            return View(new ChangePasswordVM
            {
                Username = name ?? User.Identity!.Name!
            });
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
        {
            if (vm.NewPassword != vm.ConfirmPassword)
            {
                TempData["error"] = "Mật khẩu không trùng khớp";
                return View(vm);
            }

            if (!await userService.VerifyPassword(vm.Username, vm.OldPassword))
            {
                TempData["error"] = "Mật khẩu cũ không đúng";
                return View(vm);
            }

            var isSent = await userService.SendChangePasswordOtp(vm);
            if (isSent)
            {
                TempData["success"] = "Vui lòng kiểm tra email của bạn để lấy mã otp";
                HttpContext.Session.SetString("NewPassword", vm.NewPassword);
                
                ViewBag.NewPassword = vm.NewPassword;

                return RedirectToAction("VerifyOtp");
            }
            else
            {
                TempData["error"] = "Không thể gửi mã otp";
                return View();
            }
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> VerifyOtp()
        {
            return View();
        }

        [Authorize, HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var newPassword = HttpContext.Session.GetString("NewPassword");
            if (string.IsNullOrEmpty(otp))
            {
                TempData["error"] = "Đã có lỗi xảy ra";
                return RedirectToAction("ChangePassword");
            }
            var result = await userService.ChangePassword(newPassword, otp.Trim());
            if (result)
            {
                TempData["success"] = "Đổi mật khẩu thành công";
                return Redirect("/account/login");
            }
            else
            {
                TempData["error"] = "Không thể đổi mật khẩu";
                return RedirectToAction("ChangePassword");
            }

        }


        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }

        public async Task<IActionResult> GoogleResponse ()
        {
            var result = await HttpContext
                .AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (result == null || !result.Succeeded || result.Principal == null)
            {
                return Redirect("/account/login");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var name = result.Principal.FindFirstValue(ClaimTypes.Name);
            var picture = result.Principal.FindFirstValue("picture");

            var isExisted = await userService.IsExisted(email);

            if (!isExisted)
            {
                var user = new AppUser
                {
                    Email = email,
                    UserName = email,
                    FirstName = name,
                    EmailConfirmed = true,
                    Avatar = picture,
                };

                await userService.AddUser(user, "ComputerShop123");
            }

            await userService.LoginWithGoogle(email);

            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Value,
                claim.Type,
                claim.Issuer,
                claim.OriginalIssuer
            });

            //TempData["success"] = "Đăng nhập thành công";
            return Redirect("/");
        }
    }
}
