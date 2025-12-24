using WebBanMayTinh.Models;

namespace WebBanMayTinh.Services
{
    public interface IUserService
    {
        bool AddUser(User user);
        bool UpdateUser(User user);
        bool DeleteUser(Guid id);
        User? GetUser(Guid id);
        IEnumerable<User> GetUsers();

    }
}
