using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Services
{
    /// <summary>
    /// class to hold e-mail recipient
    /// </summary>
    public class MailRecipient
    {
        /// <summary>
        /// email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName { get; set; }
    }
}
