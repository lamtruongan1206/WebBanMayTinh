using System.ComponentModel.DataAnnotations;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace WebBanMayTinh.Models
{
    public class ProductReview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public Guid OrderId { get; set; } 

        [Required]
        public string UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } 

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        public Product? Product { get; set; }
        public Order? Order { get; set; }
        public AppUser? User { get; set; }
    }
}
