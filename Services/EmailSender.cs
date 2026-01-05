
using System.Net;
using System.Net.Mail;

namespace WebBanMayTinh.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = CreateClient();

            var mail = new MailMessage(
                from: "linhtk2511044@gmail.com",
                to: email,
                subject,
                message
            )
            {
                IsBodyHtml = true
            };

            return client.SendMailAsync(mail);
        }

        public Task SendEmailWithAttachmentAsync(
            string toEmail,
            string subject,
            string body,
            byte[] fileBytes,
            string fileName)
        {
            var client = CreateClient();

            var mail = new MailMessage
            {
                From = new MailAddress("linhtk2511044@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            var attachment = new Attachment(
                new MemoryStream(fileBytes),
                fileName,
                "application/pdf"
            );

            mail.Attachments.Add(attachment);

            return client.SendMailAsync(mail);
        }

        private SmtpClient CreateClient()
        {
            return new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    "linhtk2511044@gmail.com",
                    "zufj yjss veig wjmn".Replace(" ", "")
                )
            };
        }
    }
}   
