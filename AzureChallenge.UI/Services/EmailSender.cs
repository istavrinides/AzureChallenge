using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AzureChallenge.UI.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            await Execute(subject, message, email);
        }

        public async Task Execute(string subject, string message, string email)
        {
            using(SmtpClient client = new SmtpClient("smtp.office365.com", 587))
            {
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(Options.EmailUserName, Options.EmailPassword);

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress("az-challenge@outlook.com");
                mailMessage.To.Add(email);
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = message;
                mailMessage.Subject = subject;
                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
