using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace AdManagementSystem.Services
{
    public class FakeEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Log to console or ignore
            Console.WriteLine($"Email to {email} | Subject: {subject}");
            return Task.CompletedTask;
        }
    }
}
