using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Services
{
    public class UserService : IUserService
    {
        private readonly DataContext context;
        private readonly ILogger<UserService> logger;


        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;


        public UserService(DataContext context, ILogger<UserService> logger,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager) 
        {
            this.context = context;
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<IEnumerable<AppUser>> GetUsers()
        {
            var users = await context.Users.ToListAsync();
            return users;
        }

        async Task<bool> IUserService.Login(string username, string password)
        {
            var existingUser = await userManager.FindByNameAsync(username);
            if (existingUser == null) return false;

            try
            {
                SignInResult result = await signInManager.PasswordSignInAsync(existingUser, password, false, false);
                return result.Succeeded;
            } catch (Exception ex)
            {
                return false;
            }
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
            //} catch (Exception ex)
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
    }
}
