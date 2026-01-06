using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using Org.BouncyCastle.Crypto.Generators;
using WebBanMayTinh.Areas.Admin.Models.Views;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Views;

namespace WebBanMayTinh.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext context;
        private readonly ILogger<UserService> logger;
        private readonly IEmailSender emailSender;

        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;
        private RoleManager<IdentityRole> roleManager;
        private IHttpContextAccessor httpContextAccessor;


        public UserService(DataContext context, ILogger<UserService> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IHttpContextAccessor httpContextAccessor) 
        {
            this.context = context;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
            this.httpContextAccessor = httpContextAccessor;
            this.roleManager = roleManager;
        }

        public async Task<IEnumerable<AppUser>> GetUsers()
        {
            var users = await context.Users
                .Where(u => !u.IsDeleted.Value)
                .ToListAsync();
            return users;
        }

        public async Task<AppUser?> GetCurrentUser()
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal is null) return null;
            var user = await userManager.GetUserAsync(principal);

            if(user is null) return null;
            if (user.IsDeleted.Value) return null;

            return user;
        }

        async Task<SignInResult> IUserService.Login(string username, string password)
        {
            var existingUser = await userManager.FindByNameAsync(username);
            if (existingUser == null) return SignInResult.Failed;
            if (existingUser.IsDeleted.Value) return SignInResult.Failed;

            try
            {
                SignInResult result = await signInManager.PasswordSignInAsync(existingUser, password, false, false);
                return result;
            } catch (Exception ex)
            {
                return SignInResult.Failed;
            }
        }

        async Task<SignInResult> IUserService.LoginWithGoogle(string email)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser == null) return SignInResult.Failed;
            if (existingUser.IsDeleted.Value) return SignInResult.Failed;
      
            try
            {
                await signInManager.SignInAsync(existingUser, false);
                return SignInResult.Success;
            }
            catch (Exception ex)
            {
                return SignInResult.Failed;
            }
        }

        async Task<IdentityResult> IUserService.Register(AppUser user, string password)
        {
            user.IsDeleted = false;

            var existedUser = await userManager.FindByEmailAsync(user.Email);

            // Nếu tài khoản chưa comfirm thì cho xóa đi tạo lại
            if (existedUser != null && !existedUser.EmailConfirmed)
            {
                await userManager.DeleteAsync(existedUser);
            }

            var result = await userManager.CreateAsync(user, password);

            var roleMember = await roleManager.FindByNameAsync("Member");
            await userManager.AddToRoleAsync(user, roleMember.ToString());

            return result;
        }

        async Task<bool> IUserService.AddUser(AppUser user, string password)
        {
            user.IsDeleted = false;

            try
            {
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var roleMember = await roleManager.FindByNameAsync("Member");
                    await userManager.AddToRoleAsync(user, roleMember.ToString());
                    return true;
                }
                else
                {
                    logger.LogError("Tạo mới không thành công");
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Lỗi tạo mới user: {message}", ex.Message);
                return false;
            }
        }

        bool IUserService.DeleteUser(string id)
        {
            var user = context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                logger.LogWarning("Không tồn tại User id = {id}", id);
                return false;
            }

            try
            {
                user.IsDeleted = false;

                context.Update(user);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Update User Error: {m}", ex.Message);
                return false;
            }
        }

        async Task<AppUser> IUserService.GetUser(string id)
        {
            return await userManager.FindByIdAsync(id);
        }

        async Task<bool> IUserService.SendChangePasswordOtp(ChangePasswordVM vm)
        {
            var user = await userManager.FindByNameAsync(vm.Username);

            if (user == null)
            {
                return false;
            }

            if (user.IsDeleted.Value) return false;

            var otp = Random.Shared.Next(100000, 999999).ToString();

            var passwordOtp = new PasswordOtp
            {
                UserId = user.Id,
                ExpiredAt = DateTime.UtcNow.AddMinutes(5),
                OtpHash = otp,
                IsUsed = false
            };

            context.PasswordOtps.Add(passwordOtp);
            await context.SaveChangesAsync();

            await emailSender.SendEmailAsync(
                user.Email,
                "Mã xác thực đổi mật khẩu",
                "Mã otp của bạn là: " + otp + " (hết hạn sau 5p)"
                );

            return true;
        }

        async Task<bool> IUserService.ChangePassword(string newPassword, string otp)
        {
            var user = await GetCurrentUser();
            if (user == null) return false;


            var otpEntity = await context.PasswordOtps
                .Where(x => x.UserId == user.Id && !x.IsUsed)
                .OrderByDescending(x => x.ExpiredAt)
                .FirstOrDefaultAsync();

            if (otpEntity == null ||
                otpEntity.ExpiredAt < DateTime.UtcNow)
            {
                return false;
            }

            otpEntity.IsUsed = true;
            await context.SaveChangesAsync();

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            var result = await userManager.ResetPasswordAsync(
                user,
                resetToken,
                newPassword
            );

            if (!result.Succeeded) return false;

            await signInManager.SignOutAsync();
            return true;
        }


        bool IUserService.UpdateUser(AppUser user)
        {
            //context.Users.Update(user);
            //context.SaveChanges();
            //return true;
            throw new NotImplementedException();

        }

        async void IUserService.Logout()
        {
            await signInManager.SignOutAsync();
        }

        async Task<string> IUserService.GenerateEmailConfirmToken(AppUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }

        async Task<bool> IUserService.VerifyPassword(string username, string password)
        {
            var user = await userManager.FindByNameAsync(username);
            try
            {
                return await userManager.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        async Task<string> IUserService.GeneratePasswordResetTokenAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email); 

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            return token;
        }

        async Task<IdentityResult> IUserService.ResetPasswordAsync(ResetPasswordVM resetPasswordVM)
        {
            var user = await userManager.FindByEmailAsync(resetPasswordVM.Email);
            var result = await userManager.ResetPasswordAsync(user, resetPasswordVM.Token, resetPasswordVM.Password);
            return result;
        }

        async Task<IdentityResult?> IUserService.ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            try
            {
                var result = await userManager.ConfirmEmailAsync(user, token);
                return result;

            } catch (Exception ex)
            {
                return null;
            }
        }

        async Task<bool> IUserService.IsExisted(string username)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == username || u.Email == username);
            if (user is null) return false;
            else return true;
        }
    }
}
