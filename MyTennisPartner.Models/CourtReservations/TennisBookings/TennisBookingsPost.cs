namespace MyTennisPartner.Models.CourtReservations.TennisBookings
{
    /// <summary>
    /// base class for post data, for TennisBookings provider
    /// </summary>
    public class TennisBookingsPost
    {
        public TennisBookingsPost()
        {
            SerializedString = "";
            p1 = "";
            p2 = "";
            p3 = "";
            p4 = "";
            p5 = "";
        }

        public string Action { get; set; }
        public string SerializedString { get; set; }
#pragma warning disable IDE1006 // Naming Styles
        public string p1 { get; set; }
        public string p2 { get; set; }
        public string p3 { get; set; }
        public string p4 { get; set; }
        public string p5 { get; set; }
#pragma warning restore IDE1006 // Naming Styles
    }
}
