using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GoFood.Utilities
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var host = emailSettings["Host"];
            var port = int.Parse(emailSettings["Port"]);
            var userName = emailSettings["UserName"];
            var password = emailSettings["Password"];
            var displayName = emailSettings["DisplayName"];
            var from = emailSettings["From"];

            using (var client = new SmtpClient(host, port))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(userName, password);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(from, displayName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                try 
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi gửi email: {ex.Message}");
                }
            }
        }
    }
}

