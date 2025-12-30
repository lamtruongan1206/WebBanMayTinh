using System.ComponentModel.DataAnnotations;
using WebBanMayTinh.Models;

namespace WebBanMayTinh.Areas.Admin.Models.Views
{
    public class OrderEditVM
    {
        public Guid OrderId { get; set; }
        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; }
        
    }
}
