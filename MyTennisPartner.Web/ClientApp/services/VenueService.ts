import AppConstants from '../models/app-constants';
import RestUtilities, { RestResponse } from './RestUtilities';
import VenueViewModel from "../models/viewmodels/VenueViewModel";

interface IVenueResponse {
    result: VenueViewModel;
}

export default class VenueService {

    searchVenues(search: string): Promise<RestResponse<VenueViewModel[]>> {
        let url = `${AppConstants.searchVenues}?name=${search}`;
        return RestUtilities.get<VenueViewModel[]>(url)
            .then((response) => {
                return response;
            });
    }
}