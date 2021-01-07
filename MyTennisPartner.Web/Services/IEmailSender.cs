using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string textMessage = null);
        Task SendEmailAsync(IEnumerable<MailRecipient> toList, string subject, string htmlMessage, string textMessage = null);
    }
}
