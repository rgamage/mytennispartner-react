import AppConstants from '../models/app-constants';
import RestUtilities, { RestResponse } from './RestUtilities';
import ReservationServiceLoginResult from '../models/viewmodels/ReservationServiceLoginResult';
import ReservationCredentials from '../models/viewmodels/ReservationCredentials';

export default class ReservationService {

    testSparetimeLogins(creds: ReservationCredentials): Promise<RestResponse<ReservationServiceLoginResult[]>> {
        let url = AppConstants.testSparetimeLogins;
        return RestUtilities.post<ReservationServiceLoginResult[]>(url, creds)
            .then((response) => {
                return response;
            });
    }

    updateTargetCourts(matchId: number): Promise<RestResponse<string>> {
        let url = `${AppConstants.updateTargetCourts}?matchId=${matchId}`;
        return RestUtilities.post<string>(url, null)
            .then((response) => {
                return response;
            });
    }

}