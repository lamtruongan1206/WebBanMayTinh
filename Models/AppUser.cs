using Microsoft.AspNetCore.Identity;

namespace WebBanMayTinh.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
