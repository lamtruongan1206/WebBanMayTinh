using Microsoft.AspNetCore.Identity;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Services
{
    public interface IUserService
    {
        Task<bool> AddUser(AppUser user, string password);
        bool UpdateUser(AppUser user);
        bool DeleteUser(string id);
        AppUser? GetUser(string id);
        Task<IEnumerable<AppUser>> GetUsers();
        Task<SignInResult> Login(string user, string password);
        Task<IdentityResult> Register(AppUser user, string password);
        Task<string> GenerateEmailConfirmToken(AppUser user);
        Task<IdentityResult?> ConfirmEmail(string userId, string token);
        void Logout();
    }
}
