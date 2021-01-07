import ContactViewModel from "./ContactViewModel";
import AddressViewModel from "./AddressViewModel";
import SelectOption from "../SelectOption";

export default class VenueViewModel extends SelectOption {
    venueId: number;
    name: string;
    venueAddress: AddressViewModel;
    venueContact: ContactViewModel;
}