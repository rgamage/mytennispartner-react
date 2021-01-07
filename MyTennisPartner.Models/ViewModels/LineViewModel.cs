using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyTennisPartner.Models.Utilities;

namespace MyTennisPartner.Models.ViewModels
{
    public class LineViewModel
    {
        public LineViewModel()
        {
            Players = new List<PlayerViewModel>();
            Guid = System.Guid.NewGuid().ToString();
        }

        [Required]
        public int LineId { get; set; }
        [Required]
        public int MatchId { get; set; }
        [Required]
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public string CourtNumber { get; set; }
        public List<PlayerViewModel> Players { get; }

        /// <summary>
        /// used to associate a player with a line, before the line has an id
        /// </summary>
        public string Guid { get; set; }

        public bool IsReserved { get; set; }
        /// <summary>
        /// true if some issue with line, for example auto-reserve flag set, but no players in line that can reserve a court
        /// </summary>
        public bool LineWarning { get; set; }
        /// <summary>
        /// true if user has overridden the court number (for use when auto-reserve system is managing court number)
        /// </summary>
        public bool CourtNumberOverridden { get; set; }
        /// <summary>
        /// list of courts available, according to reservation system
        /// </summary>
        public string CourtsAvailable { get; set; }

        /// <summary>
        /// for UI display of icon showing that the court will be reserved, but hasn't been yet
        /// </summary>
        public bool CourtReservationPending { get; set; }

        /// <summary>
        /// for UI display of icon showing court has been confirmed/reserved
        /// </summary>
        public bool CourtConfirmed { get; set; }

        /// <summary>
        /// for UI display of icon showing court is un-confirmed / preliminary
        /// </summary>
        public bool CourtUnConfirmed { get; set; }

        /// <summary>
        /// for UI display of some kind of warning for court number, like court will be able to be reserved
        /// </summary>
        public bool CourtNumberWarning { get; set; }

        /// <summary>
        /// for UI display of court number status
        /// </summary>
        public string CourtNumberStatus { get; set; }

        /// <summary>
        /// for UI display, whether to show button to get avail courts
        /// </summary>
        public bool ShowCourtsAvailableButton { get; set; }

        /// <summary>
        /// for UI display of too many or missing players for each line
        /// </summary>
        public int ExpectedNumberOfPlayers { get; set; }

        /// <summary>
        /// given a match, populate the UI display information fields, for the UI
        /// </summary>
        /// <param name="match"></param>
        public void SetDisplayInformation(MatchViewModel match)
        {            
            CourtNumberStatus = IsReserved ? "Court has been reserved" : "Court has not been reserved";
            CourtUnConfirmed = !IsReserved && !string.IsNullOrEmpty(CourtNumber);
            CourtConfirmed = IsReserved && !string.IsNullOrEmpty(CourtNumber);
            if (match == null) return;
            ExpectedNumberOfPlayers = string.IsNullOrEmpty(CourtNumber) ? DataConstants.MaxPlayersOnRoster : match.ExpectedPlayersPerLine;
            ShowCourtsAvailableButton = !IsReserved && CourtsAvailable == null && match.VenueHasReservationSystem && match.AutoReserveCourts;
            if (IsReserved && match.AutoReserveCourts && LineWarning && match.VenueHasReservationSystem)
            {
                CourtNumberStatus = "Court will be auto-reserved";
            }
            if (!IsReserved && match.AutoReserveCourts && LineWarning && match.VenueHasReservationSystem)
            {
                CourtNumberStatus = "Court will NOT be auto-reserved, because no player on this court can reserve courts";
            }
            CourtNumberWarning = LineWarning && match.VenueHasReservationSystem;
        }
    }
}