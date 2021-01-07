using System;
using System.Collections.Generic;
using System.Text;
using MyTennisPartner.Models.ViewModels;

namespace MyTennisPartner.Models.Utilities
{
    /// <summary>
    /// helper class to support creation of mapping links for users,
    /// for example to pull up a navigation map to a club, on their mobile device
    /// </summary>
    public static class GpsMapping
    {
        /// <summary>
        /// base url for a navigation link depends upon whether the client is iOS or not, because Apple
        /// </summary>
        /// <param name="isIosPlatform"></param>
        /// <returns></returns>
        public static string GetBaseMappingUrl(bool isIosPlatform)
        {
            var baseUrl = $"{(isIosPlatform ? "maps" : "https")}://maps.google.com/maps?daddr=daddr=";
            return baseUrl;
        }

        /// <summary>
        /// get mapping url to allow user to navigate to destination.  Depends on client platform
        /// </summary>
        /// <param name="address"></param>
        /// <param name="isIosPlatform"></param>
        /// <returns></returns>
        public static string GetMapUrl(AddressViewModel address, bool isIosPlatform)
        {
            if (address == null) return string.Empty;
            var baseUrl = GpsMapping.GetBaseMappingUrl(isIosPlatform);
            var addressString = $"{address.Street1}, {address.Street2}, {address.City}, {address.State}, {address.Zip}";
            var url = $"{baseUrl}{Uri.EscapeUriString(addressString)}";
            return url;
        }

    }
}
