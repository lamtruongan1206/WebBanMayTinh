namespace WebBanMayTinh.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            byte[] fileBytes,
            string fileName);
    }
}
