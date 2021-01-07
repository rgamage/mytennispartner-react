using System;

namespace MyTennisPartner.Utilities
{
    public static class ApplicationConstants
    {
        public const int LoginTokenTimeoutInMinutes = 720;
        public const string userTimeZone = "Pacific Standard Time";
        public static readonly TimeZoneInfo AppTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(userTimeZone);
        public static readonly TimeZoneInfo UtcTimeZoneInfo = TimeZoneInfo.Utc;
        public const string courtReservationBaseUrl = "https://goldriver.tennisbookings.com/";
        public const string courtReservationGridReferralUrl = "https://goldriver.tennisbookings.com/ViewSchedules.aspx";
        public const string League = "Group";
        public const string league = "group";
    }
}
