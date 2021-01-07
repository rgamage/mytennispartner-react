using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Web.Models.AccountViewModels
{
    /// <summary>
    /// class to hold app Id and secret for external logins like Facebook, Google, Twitter, etc.
    /// </summary>
    public class ExternalSecretsViewModel
    {
        /// <summary>
        /// App Id or key
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// App secret
        /// </summary>
        public string Secret { get; set; }
    }
}
