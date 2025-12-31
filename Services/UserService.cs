using System.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext context;
        private readonly ILogger<UserService> logger;
        private readonly IEmailSender emailSender;

        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;


        public UserService(DataContext context, ILogger<UserService> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IEmailSender emailSender) 
        {
            this.context = context;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.emailSender = emailSender;
        }

        public async Task<IEnumerable<AppUser>> GetUsers()
        {
            var users = await context.Users.ToListAsync();
            return users;
        }

        async Task<SignInResult> IUserService.Login(string username, string password)
        {
            var existingUser = await userManager.FindByNameAsync(username);
            if (existingUser == null) return SignInResult.Failed;

            try
            {
                SignInResult result = await signInManager.PasswordSignInAsync(existingUser, password, false, false);
                
                //if (result.Succeeded)
                //{
                //    var receciver = existingUser.Email;
                //    var subject = "Đăng nhập thiết bị thành công";
                //    var message = "Xin chào bạn, chúng tôi là Shop bán máy tinh";

                //    await emailSender.SendEmailAsync(receciver, subject, message);

                //    return true;
                //}
                
                return result;
            } catch (Exception ex)
            {
                return SignInResult.Failed;
            }
        }

        async Task<IdentityResult> IUserService.Register(AppUser user, string password)
        {
            var existedUser = await userManager.FindByEmailAsync(user.Email);

            // Nếu tài khoản chưa comfirm thì cho xóa đi tạo lại
            if (existedUser != null && !existedUser.EmailConfirmed)
            {
                await userManager.DeleteAsync(existedUser);
            }

            var result = await userManager.CreateAsync(user, password);

            return result;
        }

        async Task<bool> IUserService.AddUser(AppUser user, string password)
        {
            try
            {
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                    return true;
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
            //var user = context.Users.FirstOrDefault(x => x.Id == id);
            //if (user == null)
            //{
            //    logger.LogWarning("Không tồn tại User id = {id}", id);
            //    return false;
            //}

            //try
            //{
            //    context.Users.Remove(user);
            //    context.SaveChanges();
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError("Update User Error: {m}", ex.Message);
            //    return false;
            //}
            throw new NotImplementedException();
        }

        AppUser IUserService.GetUser(string id)
        {
            //var user = context.Users.FirstOrDefault(x => x.Id==id);
            //return user;
            throw new NotImplementedException();
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
    }
}
