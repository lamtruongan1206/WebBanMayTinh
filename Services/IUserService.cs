using WebBanMayTinh.Models;

namespace WebBanMayTinh.Services
{
    public interface IUserService
    {
        Task<bool> AddUser(AppUser user, string password);
        bool UpdateUser(AppUser user);
        bool DeleteUser(string id);
        User? GetUser(string id);
        Task<IEnumerable<AppUser>> GetUsers();
        Task<bool> Login(string user, string password);
        void Logout();
    }
}
