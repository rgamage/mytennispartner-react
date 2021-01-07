using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace MyTennisPartner.Core.Identity
{
    public static class IdentityExtensions
    {
        public static string FirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity)?.FindFirst(ClaimTypes.GivenName);
            // Test for null to avoid issues during local testing
            return claim != null ? claim.Value : string.Empty;
        }


        public static string LastName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity)?.FindFirst(ClaimTypes.Surname);
            // Test for null to avoid issues during local testing
            return claim != null ? claim.Value : string.Empty;
        }

        public static string GetAllErrorMessages(this IdentityResult result)
        {
            if (result?.Succeeded == true) return string.Empty;
            var messages = string.Join(Environment.NewLine, result.Errors.Select(e => e.Description).ToArray());
            return messages;
        }

        /// <summary>
        /// given a user, look up the custom claim of memberId for that user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static int GetMemberId(this ClaimsPrincipal user)
        {
            if (user == null) return 0;
            var memberIdString = user.Claims.FirstOrDefault(c => c.Type == "memberId")?.Value ?? "0";
            _ = int.TryParse(memberIdString, out int memberId);
            return memberId;
        }

    }
}
