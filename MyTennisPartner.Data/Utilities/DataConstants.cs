using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Data.Utilities
{
    /// <summary>
    /// global constants for date/models
    /// </summary>
    public static class DataConstants
    {
        // todo: remove hard-coding of time zone here, move to more global place, or add ability for users to set individually, or base it
        // on their zip code when they register/update their profile
        public static readonly TimeZoneInfo AppTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
    }
}
