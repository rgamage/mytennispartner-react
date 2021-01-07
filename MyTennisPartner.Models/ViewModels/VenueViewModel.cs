using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    public class VenueViewModel: SelectOptionViewModel
    {
        [Required]
        public int VenueId { get; set; }

        // name of venue
        public string Name { get; set; }

        public AddressViewModel VenueAddress { get; set; }

        // contact person that actively manages the tennis (courts, etc.) for venue
        public ContactViewModel VenueContact { get; set; }

        public ReservationSystemViewModel ReservationSystem { get; set; }
    }
}
