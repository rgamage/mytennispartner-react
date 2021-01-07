using System;

namespace MyTennisPartner.Models.CourtReservations.TennisBookings
{

    /// <summary>
    /// this is the data payload sent when selecting a court and time for a reservation
    /// it does not book the time, it just sets the parameters, to prepare for the dialog for booking
    /// it is needed, for our purposes, to retrieve the member number, which comes in the reply to this post 
    /// example value from network trace: { "Action":"CheckRules","SerializedString":"","p1":"d=03172019&ts=5072|1900|1930[","p2":"","p3":"","p4":"","p5":""}
    /// {"Action":"CheckRules","SerializedString":null,"p1":"03172019&ts=5071|1900|1930[","p2":null,"p3":null,"p4":null,"p5":null}
    /// { "Action":"CheckRules","SerializedString":"","p1":"d=03172019&ts=5072|1900|1930[","p2":"","p3":"","p4":"","p5":""}
    /// </summary>
    public class CheckRulesPost : TennisBookingsPost
    {
        /// <summary>
        /// set properties in constructor
        /// </summary>
        public CheckRulesPost(int courtBaseId, int courtNumber, DateTime startTime, DateTime endTime)
        {
            // set action
            Action = "CheckRules";

            // set p1
            // example value of p1 = "d=03172019&ts=5071|1900|1930[";
            // this seems to be a magic constant to determine the court id at gold river
            // todo: check if this is the same at other clubs?
            //var courtId = 5052 + courtNumber;
            //var courtId = 6814 + courtNumber;
            var courtId = courtBaseId + courtNumber;
            p1 = $"d={startTime.ToString("MMddyyyy")}&ts={courtId}|{startTime.ToString("HHmm")}|{endTime.ToString("HHmm")}[";
        }
    }
}
