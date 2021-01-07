using MyTennisPartner.Models.Enums;
using System;
using System.Collections.Generic;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// class to hold basic sumamry info about matches, lines, and players
    /// </summary>
    public class MatchDetailsViewModel
    {
        public MatchDetailsViewModel(List<MatchSummaryViewModel> matches, List<LineViewModel> lines, List<PlayerViewModel> players)
        {
            Matches = matches;
            Lines = lines;
            Players = players;
        }

        public List<MatchSummaryViewModel> Matches { get; }
        public List<LineViewModel> Lines { get; }
        public List<PlayerViewModel> Players { get; }
    }
}
