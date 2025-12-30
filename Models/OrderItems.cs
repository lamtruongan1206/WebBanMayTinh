using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanMayTinh.Models
{
    public class OrderItems
    {
        public Guid Id { get; set; }

        [Required]
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        public decimal Price { get; set; } 

        [Required]
        public int Quantity { get; set; }

        [NotMapped]
        public decimal Total => Price * Quantity;
    }
}
