using System.ComponentModel.DataAnnotations;

namespace WebBanMayTinh.Models.Views
{
    public class CancelRequestVM
    {
        [Required]
        public Guid OrderId {  get; set; }  

        [Required(ErrorMessage = "Không được để trống")]  
        public string? CancelReason { get; set; }
    }
}
