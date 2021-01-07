using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// personal contact info
    /// </summary>
    public class ContactViewModel
    {
        [Required]
        public int ContactId { get; set; }

        [StringLength(30)]
        public string FirstName { get; set; }

        [StringLength(30)]
        public string LastName { get; set; }

        [StringLength(30)]
//        [Phone]
        public string Phone1 { get; set; }

        [StringLength(30)]
//        [Phone]
        public string Phone2 { get; set; }

        [StringLength(50)]
//        [EmailAddress]
        public string Email { get; set; }

        // foreign key
        public int VenueId { get; set; }
    }
}
