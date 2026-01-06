using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class UserVM
    {
        public AppUser User {  get; set; }
        public List<string> Roles { get; set; }
    }

    public class UserDetailVM : AppUser
    {
        public IList<string> Roles { get; set; }
    }
}
