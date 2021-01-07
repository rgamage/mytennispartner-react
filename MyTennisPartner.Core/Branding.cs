using System;
using System.Collections.Generic;
using System.Text;

namespace MyTennisPartner.Core
{
    /// <summary>
    /// class that holds info about the application that may change, depending on terminology, etc.
    /// </summary>
    public static class Branding
    {
#pragma warning disable IDE1006 // Naming Styles
        public static string League => "Group";
        public static string Leagues => "Groups";
        public static string league => League.ToLower();
        public static string Venue => "Club";
        public static string Venues => "Clubs";
        public static string venue => Venue.ToLower();
        public static string Match => "Match";
        public static string match => Match.ToLower();
        public static string Matches => "Matches";
        public static string matches => Matches.ToLower();
        public static string AppName => "My Tennis Partner";
        public static string AppShortName => "MTP";
        public static string AppTagLine => "Organizing tennis made easy";
        public static string SiteLogoImage => "/images/site-logo-small.png";
        public static string DashboardName => "Home";
#pragma warning restore IDE1006 // Naming Styles
    }
}
