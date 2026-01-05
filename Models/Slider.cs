using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models
{
    public class Slider
    {
        public Guid Id { get; set; }
        [Display(Name = "Đường dẫn")]
        public string? ImageUrl { get; set; }
        [Display(Name = "Tiêu đề")]
        [MinLength(1, ErrorMessage = "Tiêu đề quá ngắn"), Required]
        public string Title { get; set; }
        [Display(Name = "Mô tả")]
        [MinLength(1, ErrorMessage = "Mô tả quá ngắn"), Required]
        public string Description { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }
}
