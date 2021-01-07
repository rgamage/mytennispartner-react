using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyTennisPartner.Utilities;
using MyTennisPartner.Models.Users;
using MyTennisPartner.Web.Models;

namespace MyTennisPartner.Web.Utilities
{
    public static class AuthenticationHelper
    {
        public static string GetEmailFromPrincipal(ClaimsPrincipal principal)
        {
            var email = principal.FindFirstValue(ClaimTypes.Email);
            return email;
        }

        public static string GetNameFromPrincipal(ClaimsPrincipal principal)
        {
            var name = principal.FindFirstValue(ClaimTypes.Name);
            return name;
        }

        public static string GetFirstNameFromPrincipal(ClaimsPrincipal principal)
        {
            var firstName = principal.FindFirstValue(ClaimTypes.GivenName);
            return firstName;
        }

        public static string GetLastNameFromPrincipal(ClaimsPrincipal principal)
        {
            var lastName = principal.FindFirstValue(ClaimTypes.Surname);
            return lastName;
        }

        public static string CreateToken(ApplicationUser user, JwtOptions jwtOptions, bool keepMeLoggedIn=false)
        {
            if (jwtOptions is null || user is null) return string.Empty;

            // Generate and issue a JWT token
            var claims = new Claim[] {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
              };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
              issuer: jwtOptions.issuer,
              audience: jwtOptions.issuer,
              claims: claims,
              expires: DateTime.Now.AddMinutes(keepMeLoggedIn ? 60*24*365 : ApplicationConstants.LoginTokenTimeoutInMinutes),
              signingCredentials: creds);

            var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);
            return serializedToken;
        }


    }
}
