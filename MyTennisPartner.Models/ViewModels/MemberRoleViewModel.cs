using MyTennisPartner.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MyTennisPartner.Models.ViewModels
{
    /// <summary>
    /// view model for sending to client
    /// </summary>
    public class MemberRoleViewModel: SelectOptionViewModel
    {
        /// <summary>
        /// Id
        /// </summary>
        [Required]
        public int MemberRoleId { get; set; }

        /// <summary>
        /// member id associated with this role
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// role enum
        /// </summary>
        public Role Role { get; set; }
    }
}
