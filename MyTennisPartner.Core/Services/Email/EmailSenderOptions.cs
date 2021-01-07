using System;
using System.Text.RegularExpressions;

namespace MyTennisPartner.Core.Services.Email
{
    public class EmailSenderOptions
    {
        private string smtpConfig;

        public string EmailFromName { get; set; }
        public string EmailFromAddress { get; set; }
        public string SiteAdminEmail { get; set; }

        public string Username { get; protected set; }
        public string Password { get; protected set; }
        public string Host { get; protected set; }
        public int Port { get; protected set; }
        public bool Ssl { get; protected set; }

        public string SmtpConfig
        {
            get { return smtpConfig; }
            set
            {
                smtpConfig = value;

                // smtpConfig is in username:password@localhost:1025:ssl format; extract the part
                var smtpConfigPartsRegEx = new Regex(@"(.*)\:(.*)@(.+)\:([0-9]+)\:?(ssl)?");
                var smtpConfigPartsMatch = smtpConfigPartsRegEx.Match(value);

                Username = smtpConfigPartsMatch.Groups[1].Value;
                Password = smtpConfigPartsMatch.Groups[2].Value;
                Host = smtpConfigPartsMatch.Groups[3].Value;
                Port = Convert.ToInt32(smtpConfigPartsMatch.Groups[4].Value);
                Ssl = smtpConfigPartsMatch.Groups[5].Value == "ssl" ? true : false;
            }
        }
    }
}
