import AddressViewModel from "../models/viewmodels/AddressViewModel";

export default class GpsMapping {
    static getBaseMappingUrl = (): string => {
        let baseUrl = ""
        // If it's an iPhone..
        if ((navigator.platform.indexOf("iPhone") != -1)
            || (navigator.platform.indexOf("iPod") != -1)
            || (navigator.platform.indexOf("iPad") != -1))
            baseUrl = "maps://maps.google.com/maps?daddr=daddr=";
        else
            baseUrl = "http://maps.google.com/maps?daddr=daddr=";
        return baseUrl;
    }

    static getMapUrl(address: AddressViewModel): string {
        var base = GpsMapping.getBaseMappingUrl();
        var addressString = `${address.street1}, ${address.street2}, ${address.city}, ${address.state}, ${address.zip}`;
        var url = `${base}${encodeURIComponent(addressString)}`;
        return url;
    }
}