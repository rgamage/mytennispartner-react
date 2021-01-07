using MyTennisPartner.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Models.ViewModels
{
    public class PlayerPreferenceViewModel: SelectOptionViewModel
    {
        [Required]
        public int PlayerPreferenceId { get; set; }
        public PlayFormat Format { get; set; }
        public int MemberId { get; set; }
    }
}