using System;
using System.Collections.Generic;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// container to hold list of lines for a given match, along with the list of LMMs which holds player availability
    /// </summary>
    public class LinesWithAvailabilityViewModel
    {
        public LinesWithAvailabilityViewModel(List<LineViewModel> lines, List<PlayerViewModel> players, List<MemberNameViewModel> subs)
        {
            Lines = lines;
            Players = players;
            Subs = subs;
        }

        public List<LineViewModel> Lines { get; }
        public List<PlayerViewModel> Players { get; }
        public List<MemberNameViewModel> Subs { get; }
    }
}
