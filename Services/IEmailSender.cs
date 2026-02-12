using System.Threading.Tasks;

namespace CRM.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string html);
    }
}
