using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// viewmodel to hold league availabilities for upcoming matches
    /// Each instance holds the member, and a collection of match availabilities for that member
    /// </summary>
    public class LeagueAvailabilityViewModel
    {
        public LeagueAvailabilityViewModel(MemberNameViewModel memberName, List<PlayerViewModel> leaguePlayers)
        {
            MemberName = memberName;
            LeaguePlayers = leaguePlayers;
        }

        public MemberNameViewModel MemberName { get; }
        public List<PlayerViewModel> LeaguePlayers { get; }
    }
}
