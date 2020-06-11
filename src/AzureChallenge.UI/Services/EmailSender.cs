using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
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
        private readonly IConfiguration configuration;

        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor,
                            IConfiguration configuration)
        {
            Options = optionsAccessor.Value;
            this.configuration = configuration;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            await Execute(subject, message, email);
        }

        public async Task Execute(string subject, string message, string email)
        {
            var apiKey = configuration.GetSection("SendGrid_Api_Key").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("azchallenge-noreply@az-challenge.azurewebsites.net", "Az Challenge (do not reply)");
            List<EmailAddress> tos = new List<EmailAddress>
            {
                new EmailAddress(email)
            };

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", message, false);
            await client.SendEmailAsync(msg);
        }
    }
}
