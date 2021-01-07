using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Web.Models.AccountViewModels
{
    public class UpdateAccountViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
    }
}
