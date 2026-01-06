using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebBanMayTinh.Models
{
    public class Brand
    {
        [Key]
        public int Id { get; set; }
        [MinLength(1), Display(Name = "Tên")]
        public string? Name { get; set; }
        [MinLength(4), Display(Name = "Mô tả")]
        public string? Description { get; set; }
        [MinLength(4)]
        public string? Slug { get; set; }
        
    }
}
