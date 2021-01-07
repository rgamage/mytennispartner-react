using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    public class MatchAvailabilityViewModel
    {
        public MatchAvailabilityViewModel()
        {
            Matches = new List<MatchSummaryViewModel>();
        }
        public LeagueSummaryViewModel League { get; set; }
        public List<MatchSummaryViewModel> Matches { get; }
    }
}
