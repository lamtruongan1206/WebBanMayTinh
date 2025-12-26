using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public class Address
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string ReceiverName { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string AddressLine { get; set; }

        public string Ward { get; set; }
        public string District { get; set; }
        public string Province { get; set; }
        public bool IsDefault { get; set; }

        [Required]
        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
