using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;

namespace MyTennisPartner.Web.Services.Reservations
{
    public class CourtReservationClient : RestClient
    {
        public CourtReservationClient()
        {
            CookieContainer = new CookieContainer();
            UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";
            this.AddDefaultHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            this.AddDefaultHeader("Accept-Encoding", "gzip, deflate, sdch, br");
            this.AddDefaultHeader("Accept-Language", "en-US,en;q=0.8");
            this.AddDefaultHeader("Cache-Control", "no-cache");

            // to do by consumer of this client: Set url-specific parameters, like these examples
            //this.AddDefaultHeader("Host", "goldriver.tennisbookings.com");
            //BaseUrl = new Uri("https://goldriver.tennisbookings.com/")l;
        }
    }
}
