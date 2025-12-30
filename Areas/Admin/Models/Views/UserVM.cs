namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class UserVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }    
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } 
        public List<string> Roles { get; set; }
    }
}
