using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Bulky.Utility
{
    public class EmailSender : IEmailSender //add in program.cs
    {
        //public Task SendEmailAsync(string email, string subject, string htmlMessage)
        //{
        //    //todo: implement logic to send email
        //    return Task.CompletedTask;
        //}

        //just to test, might not work with gmail account

        public string SendGridSecret { get; set; }

        public EmailSender(IConfiguration _config)
        {
            SendGridSecret = _config.GetValue<string>("SendGrid:SecretKey");
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //todo: implement logic to send email
            var client = new SendGridClient(SendGridSecret);

            var from = new EmailAddress("bulkyrobin@gmail.com", "Bulky");
            var to = new EmailAddress(email);
            var message = MailHelper.CreateSingleEmail(from, to, subject, "", htmlMessage);

            return client.SendEmailAsync(message);
        }
    }
}
