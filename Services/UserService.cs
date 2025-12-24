using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Services
{
    public class UserService : IUserService
    {
        private readonly ShopBanMayTinhContext context;
        private readonly ILogger<UserService> logger;

        public UserService(ShopBanMayTinhContext context, ILogger<UserService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public IEnumerable<User> GetUsers()
        {
            var users = context.Users
                .Include(r => r.Role)
                .ToList();
            return users;
        }

        bool IUserService.AddUser(User user)
        {
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

            var existingUser = context.Users.FirstOrDefault(u => u.Username == user.Username
            || u.Email == user.Email);

            if (existingUser != null)
            {
                logger.LogWarning("User is existing");
                return false;
            }

            try
            {
                user.Id = Guid.NewGuid();
                var hashedPassword = passwordHasher.HashPassword(user, "");
                user.Password = hashedPassword;
                
                user.CreatedAt = user.UpdateAt = DateOnly.FromDateTime(DateTime.Now);

                context.Users.Add(user);
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Lỗi tạo mới user: {message}", ex.Message);
                return false;
            }
        }

        bool IUserService.DeleteUser(Guid id)
        {
            var user = context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                logger.LogWarning("Không tồn tại User id = {id}", id);
                return false;
            }

            try
            {
                context.Users.Remove(user);
                context.SaveChanges();
                return true;
            } catch (Exception ex)
            {
                logger.LogError("Update User Error: {m}", ex.Message);
                return false;
            }
        }

        User IUserService.GetUser(Guid id)
        {
            var user = context.Users.FirstOrDefault(x => x.Id==id);
            return user;
        }

        bool IUserService.UpdateUser(User user)
        {
            context.Users.Update(user);
            context.SaveChanges();
            return true;
        }
    }
}
