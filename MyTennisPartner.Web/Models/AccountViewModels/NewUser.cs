using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Web.Models.AccountViewModels
{
    public class NewUser
    {
        [Required]
        [EmailAddress]
        public string username { get; set; }

        [Required]
        [MinLength(6)]
        public string password { get; set; }

        [Required]
        public string firstName { get; set; }

        [Required]
        public string lastName { get; set; }
    }
}
