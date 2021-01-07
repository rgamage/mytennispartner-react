using MyTennisPartner.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// generic address class - look into copying standard practices for address field behaviors, data types, validation, etc.
    /// </summary>
    public class AddressViewModel
    {
        [Required]
        public int AddressId { get; set; }

        [StringLength(50)]
        public string Street1 { get; set; }

        [StringLength(50)]
        public string Street2 { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(2)]
        public string State { get; set; }

        [StringLength(10)]
        public string Zip { get; set; }

        // foreign key
        public int VenueId { get; set; }
    }
}
