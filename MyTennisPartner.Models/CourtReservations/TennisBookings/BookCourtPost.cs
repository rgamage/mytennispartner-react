using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTennisPartner.Models.CourtReservations.TennisBookings
{
    /// <summary>
    /// class to hold post data, for booking a court on tennisbookings.com
    /// </summary>
    public class BookCourtPost : TennisBookingsPost
    {
        public BookCourtPost(ReservationDetails reservation)
        {
            if (reservation is null) throw new Exception("BookCourtPost constructor: reservation cannot be null");

            // set action
            Action = "Save";

            // set SerializedString - this is a long string containing various parameters
            // example of actual value: "txtflagstring÷3271|19271||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷564705|||N|563782|||N|÷txtplayers÷hidden¥n1÷Randy Gamage÷÷text¥n2÷Larry Miller÷÷text¥y2÷false÷÷checkbox¥n3÷÷÷text¥y3÷false÷÷checkbox¥n4÷÷÷text¥y4÷false÷÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷false÷fs3271÷radio¥rb19271÷true÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷false÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷false÷chkemailreminder÷checkbox"
            // another example:         "txtflagstring÷3271|19263||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷564705|||N||Pat||Y||Joe||Y||Dave||Y|÷txtplayers÷hidden¥n1÷Randy Gamage÷÷text¥n2÷Pat÷÷text¥y2÷true÷÷checkbox¥n3÷Joe÷÷text¥y3÷true÷÷checkbox¥n4÷Dave÷÷text¥y4÷true÷÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷true÷fs3271÷radio¥rb19271÷false÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷true÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷true÷chkemailreminder÷checkbox¥cbohours÷12÷cbohours÷select-one"
            var singlesDoublesCode = reservation.IsDoubles ? 19263 : 19271;
            var template1 = $"txtflagstring÷3271|{singlesDoublesCode}||÷txtflagstring÷hidden¥dbg÷÷dbg÷hidden¥txtplayers÷";

            var sb = new StringBuilder(template1);
            foreach (var member in reservation.Members)
            {
                sb.Append(member.IsGuest ? $"|{member.FirstName}||Y|" : $"{member.MemberNumber}|||N|");
            }
            sb.Append("÷txtplayers÷hidden¥n1÷");
            int i = 1;
            foreach (var member in reservation.Members)
            {
                var middleSpace = string.IsNullOrEmpty(member.LastName) ? "" : " ";
                if (i == 1)
                {
                    sb.Append($"{member.FirstName}{middleSpace}{member.LastName}÷÷text¥n2÷");
                }
                if (i == 3 || i == 4)
                {
                    // Pat÷÷text¥y2÷true÷÷checkbox¥n3÷Joe÷÷text¥y3÷true÷÷checkbox¥n4÷Dave÷÷text¥y4÷true÷
                    sb.Append($"÷checkbox¥n{i}÷");
                }
                if (i > 1)
                {
                    sb.Append($"{member.FirstName}{middleSpace}{member.LastName}÷÷text¥y{i}÷");
                    sb.Append(member.IsGuest ? "true÷" : "false÷");
                }
                i++;
            }
            // if there are less than four players, pad with empty names
            for(var j=4-(4-reservation.Members.Count)+1; j<5; j++)
            {
                if (j == 3 || j == 4)
                {
                    sb.Append($"÷checkbox¥n{j}÷");
                }
                sb.Append($"÷÷text¥y{j}÷false÷");
            }

            var IsDoublesString = reservation.IsDoubles ? "true" : "false";
            var IsSinglesString = reservation.IsDoubles ? "false" : "true";
            var template3 = $"÷checkbox¥txtmessage÷÷txtmessage÷textarea¥rb19263÷{IsDoublesString}÷fs3271÷radio¥rb19271÷{IsSinglesString}÷fs3271÷radio¥rb-5÷false÷fs3271÷radio¥chkpublic÷false÷chkpublic÷checkbox¥chkemailconfirmation÷false÷chkemailconfirmation÷checkbox¥chkemailconfirmationtoothers÷false÷chkemailconfirmationtoothers÷checkbox¥chkemailreminder÷false÷chkemailreminder÷checkbox¥cbohours÷1÷cbohours÷select-one";
            sb.Append(template3);
            SerializedString = sb.ToString();
        }
    }

    public class ReservationMember
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int MemberNumber { get; set; }
        public bool IsGuest { get { return MemberNumber > 0 ? false : true; } }
    }

    public class ReservationDetails
    {
        public ReservationDetails(List<ReservationMember> members, TimeSlot timeslot, bool isDoubles, int courtNumber)
        {
            Members = members;
            TimeSlot = timeslot;
            IsDoubles = isDoubles;
            CourtNumber = courtNumber;
        }

        public List<ReservationMember> Members { get; }
        public TimeSlot TimeSlot { get; set; }
        public bool IsDoubles { get; set; }
        public int CourtNumber { get; set; }
    }
}
