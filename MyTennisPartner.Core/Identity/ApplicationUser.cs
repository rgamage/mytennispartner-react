using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MyTennisPartner.Core.Identity
{

    /// <summary>
    /// these properties extend the default MS Identity user properties
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            CreateDate = DateTime.UtcNow;
            FirstName = "";
            LastName = "";
        }

        /// <summary>
        /// User's first (given) name
        /// </summary>
        [Required]
        [PersonalData]
        public string FirstName { get; set; }

        /// <summary>
        /// user's last (family) name
        /// </summary>
        [Required]
        [PersonalData]
        public string LastName { get; set; }

        /// <summary>
        /// create date of user
        /// </summary>
        public DateTime? CreateDate { get; set; }
    }
}
