using MyTennisPartner.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    public class ReservationCredentialsViewModel
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public CourtReservationProvider CourtReservationProvider { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
