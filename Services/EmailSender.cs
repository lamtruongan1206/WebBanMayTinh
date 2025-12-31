
using System.Net;
using System.Net.Mail;

namespace WebBanMayTinh.Services
{
    public class EmailSender : IEmailSender
    {
        Task IEmailSender.SendEmailAsync(string email, string subject, string message)
        {
            // zufj yjss veig wjmn
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("linhtk2511044@gmail.com", "zufjyjssveigwjmn"),
            };

            return client.SendMailAsync(
                new MailMessage(
                    from: "linhtk2511044@gmail.com",
                    to: email,
                    subject,
                    message));
        }
    }
}   
