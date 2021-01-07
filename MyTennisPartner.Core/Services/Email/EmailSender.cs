using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace MyTennisPartner.Core.Services.Email
{

    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        private readonly IHostingEnvironment _env;

        public EmailSender(IOptions<EmailSenderOptions> optionsAccessor, IHostingEnvironment env)
        {
            Options = optionsAccessor?.Value ?? new EmailSenderOptions();
            _env = env;
        }

        public EmailSenderOptions Options { get; }

        /// <summary>
        /// send method with alternative parameters - simple string for e-mail address
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <param name="textMessage"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage, string textMessage = null)
        {
            var toList = new List<MailRecipient> { new MailRecipient { Email = toEmail } };
            await SendEmailAsync(toList, subject, htmlMessage, textMessage);
        }

        /// <summary>
        /// send method with ability to pass multiple recipients, with display names
        /// </summary>
        /// <param name="toList"></param>
        /// <param name="subject"></param>
        /// <param name="htmlMessage"></param>
        /// <param name="textMessage"></param>
        /// <returns></returns>
        public async Task SendEmailAsync(IEnumerable<MailRecipient> toList, string subject, string htmlMessage, string textMessage = null)
        {
            if (toList is null) return;
            MailMessage mailMessage = new MailMessage();
            foreach (var addr in toList)
            {
                mailMessage.To.Add(new MailAddress(addr.Email, addr.DisplayName));
            }
            mailMessage.From = new MailAddress(Options.EmailFromAddress, Options.EmailFromName);
            mailMessage.Body = textMessage;
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.Subject = subject;
            mailMessage.SubjectEncoding = Encoding.UTF8;
             
            if (!string.IsNullOrEmpty(htmlMessage))
            {
                // apply some global styles to the html email
                // add a unique number at the bottom, to keep gmail from hiding it (gmail often hides email signatures or footers, if they are the same as prev messages)
                var refnumber = DateTime.Now.Second * DateTime.Now.Millisecond;
                var styledHtmlMessage = $"<div style=\"font-family: sans-serif; \">{htmlMessage}<div style=\"font-size: 6px; color: whitesmoke; margin-top: 10px;\" >Reference# {refnumber}</div></div>";
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(styledHtmlMessage);
                htmlView.ContentType = new System.Net.Mime.ContentType("text/html");
                mailMessage.AlternateViews.Add(htmlView);
            }

            if (_env.EnvironmentName == "LocalDevelopment")
            {
                // don't bother sending e-mails during localDev sessions
                //return;
                //mailMessage.To.Clear();
                //mailMessage.To.Add(Options.siteAdminEmail);
            }
             
            // in staging and production, bcc all e-mails to site admin
            if (_env.IsProduction() || _env.IsStaging())
            {
                mailMessage.Bcc.Add(Options.SiteAdminEmail);
            }

            using SmtpClient client = new SmtpClient(Options.Host, Options.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Options.Username, Options.Password),
                EnableSsl = Options.Ssl
            };
            await client.SendMailAsync(mailMessage);
        }
    }
}
