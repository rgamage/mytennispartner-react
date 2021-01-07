using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MyTennisPartner.Core.Services.Email
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            if (emailSender is null)
            {
                throw new ArgumentNullException(nameof(emailSender));
            }
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }
    }
}
