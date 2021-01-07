using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace MyTennisPartner.Web.Models.ManageViewModels
{
    public class ExternalLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; }

        public IList<AuthenticationScheme> OtherLogins { get; }

        public bool ShowRemoveButton { get; set; }

        public string StatusMessage { get; set; }
    }
}
