namespace WebBanMayTinh.Models.Views
{
    public class ResetPasswordVM
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ComfirmPassword { get; set; }
    }
}
