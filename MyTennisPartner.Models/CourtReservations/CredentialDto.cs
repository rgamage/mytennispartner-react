using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyTennisPartner.Models.CourtReservations
{
    /// <summary>
    /// data transfer object to hold court reservation credentials
    /// </summary>
    public class CredentialDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int MemberId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? MemberNumber { get; set; }
    }
}
