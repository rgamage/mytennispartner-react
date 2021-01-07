namespace MyTennisPartner.Models.CourtReservations.TennisBookings
{
    /// <summary>
    /// post data class for logging in to tennisbookings.com
    /// </summary>
    public class LoginPost : TennisBookingsPost
    {
        public LoginPost(string username, string password, string template)
        {
            Action = "LoginUser";
            if (template != null)
            {
                SerializedString = template
                     .Replace("{username}", username)
                     .Replace("{password}", password);
            }
        }
    }
}
