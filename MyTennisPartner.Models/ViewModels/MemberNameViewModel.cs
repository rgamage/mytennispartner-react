using MyTennisPartner.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MyTennisPartner.Models.ViewModels
{
    public class MemberNameViewModel: SelectOptionViewModel
    {
        [Required]
        public int MemberId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string HomeVenueName { get; set; }
        public bool IsSubstitute { get; set; }
        public Availability Availability { get; set; }
        public Gender Gender { get; set; }
        public bool IsCaptain { get; set; }
        public int LeagueMemberId { get; set; }
        
        /// <summary>
        /// used for sorting of ordered sub list, to adjust for recent play, etc.
        /// </summary>
        public int Points { get; set; }
        public DateTime ResponseDate { get; set; }
        
        /// <summary>
        /// for UI display, selection from a list, etc.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// member's full name
        /// </summary>
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }

    }
}
