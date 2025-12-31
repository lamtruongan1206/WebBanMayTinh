namespace WebBanMayTinh.Models
{
    public class PasswordOtp
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string OtpHash { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
