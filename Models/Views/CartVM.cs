namespace WebBanMayTinh.Models.DTO;

    public class CartVM
    {
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }

        public string Name { get; set; }
        public decimal Price { get; set; }

        public int Quantity { get; set; }
        public string Image { get; set; }
        public bool Checked { get; set; }
    }


