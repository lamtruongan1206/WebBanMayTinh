using Microsoft.AspNetCore.Identity;
using WebBanMayTinh.Models;
using WebBanMayTinh.Models.Views;

namespace WebBanMayTinh.Services
{
    public interface IUserService
    {
        Task<bool> AddUser(AppUser user, string password);
        bool UpdateUser(AppUser user);
        bool DeleteUser(string id);
        Task<AppUser?> GetUser(string id);
        Task<IEnumerable<AppUser>> GetUsers();
        Task<SignInResult> Login(string user, string password);
        Task<IdentityResult> Register(AppUser user, string password);
        Task<bool> IsExisted(string username);
        Task<bool> VerifyPassword(string username, string password);
        Task<string> GenerateEmailConfirmToken(AppUser user);
        Task<IdentityResult?> ConfirmEmail(string userId, string token);
        Task<bool> SendChangePasswordOtp(ChangePasswordVM vm);
        Task<bool> ChangePassword(string newPassword, string otp);
        Task<string> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordVM resetPasswordVM);
        void Logout();
    }
}
