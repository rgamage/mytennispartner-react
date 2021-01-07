using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Web.Models.AccountViewModels
{
    public class CheckPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
