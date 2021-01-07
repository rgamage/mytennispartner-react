import AppConstants from '../models/app-constants';
import RestUtilities, { RestResponse } from './RestUtilities';
import AuthStore from '../stores/Auth';
import LinesWithAvailability from "../models/viewmodels/LinesWithAvailability";

export default class LineService {

    getLinesByMatch(matchId: number, memberId: number): Promise<RestResponse<LinesWithAvailability>> {
        const url = `${AppConstants.getLinesByMatchApi}?matchId=${matchId}&memberId=${memberId}`;
        return RestUtilities.get<LinesWithAvailability>(url)
            .then((response) => {
                return response;
            });
    }
}