using Microsoft.AspNetCore.Identity.UI.Services;

namespace Bulky.Utility
{
    public class EmailSender : IEmailSender //add in program.cs
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //todo: implement logic to send email
            return Task.CompletedTask;
        }
    }
}
