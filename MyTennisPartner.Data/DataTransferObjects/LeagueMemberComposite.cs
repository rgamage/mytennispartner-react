using MyTennisPartner.Data.Models;
using MyTennisPartner.Models.ViewModels;

namespace MyTennisPartner.Data.DataTransferObjects
{
    /// <summary>
    /// class to hold composite info from member, leaguem venu
    /// </summary>
    public class LeagueMemberComposite: MemberViewModel
    {
        public Member Member { get; set; }
        public Venue Venue { get; set; }
        public LeagueMember LeagueMember { get; set; }
        public string VenueName { get; set; }
    }
}
