using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// view model to hold availability grid for league
    /// </summary>
    public class LeagueAvailabilityGridViewModel
    {
        public LeagueAvailabilityGridViewModel(List<LeagueAvailabilityViewModel> leagueAvailabilities, List<MatchSummaryViewModel> matches)
        {
            LeagueAvailabilities = leagueAvailabilities;
            Matches = matches;
        }

        public List<LeagueAvailabilityViewModel> LeagueAvailabilities { get; }
        public List<MatchSummaryViewModel> Matches { get; }
    }
}
