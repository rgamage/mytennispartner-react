import AppConstants from '../models/app-constants';
import RestUtilities, { RestResponse } from './RestUtilities';
import AuthStore from '../stores/Auth';
import MatchViewModel from "../models/viewmodels/MatchViewModel";
import { Availability, MatchDeclineAction } from "../models/viewmodels/Enums";
import PlayerViewModel from '../models/viewmodels/PlayerViewModel';
import LeagueAvailabilityGridViewModel from '../models/viewmodels/LeagueAvailabilityGridViewModel';
import MatchAvailabilityViewModel from '../models/viewmodels/MatchAvailabilityViewModel';
import { MemberNameViewModel } from '../models/viewmodels/MemberViewModel';
import MatchDetailsViewModel from '../models/viewmodels/MatchDetailsViewModel';

export default class MatchService {

    getMatchesByLeague(leagueId: number, showPast: boolean = false, showFuture: boolean = true, page: number = 1, pageSize: number): Promise<RestResponse<MatchViewModel[]>> {
        let url = `${AppConstants.getMatchesByLeagueApi}?leagueId=${leagueId}&showPast=${showPast.toString()}&showFuture=${showFuture.toString()}&pageNumber=${page}&pageSize=${pageSize}`;
        return RestUtilities.get<MatchViewModel[]>(url)
            .then((response) => {
                return response;
            })
    }

    getMatchesByMember(memberId: number, showPast: boolean = false, showFuture: boolean = true, page: number = 1, pageSize: number): Promise<RestResponse<MatchViewModel[]>> {
        let url = `${AppConstants.getMatchesByMemberApi}?memberId=${memberId}&showPast=${showPast.toString()}&showFuture=${showFuture.toString()}&pageNumber=${page}&pageSize=${pageSize}`;
        return RestUtilities.get<MatchViewModel[]>(url)
            .then((response) => {
                return response;
            })
    }

    getMatch(matchId: number): Promise<RestResponse<MatchViewModel>> {
        let url = `${AppConstants.matchApi}/${matchId}`;
        return RestUtilities.get<MatchViewModel>(url)
            .then((response) => {
                return response;
            })
    }

    getNewMatch(leagueId: number): Promise<RestResponse<MatchViewModel>> {
        let url = `${AppConstants.getNewMatchApi}?leagueId=${leagueId}`;
        return RestUtilities.get<MatchViewModel>(url)
            .then((response) => {
                return response;
            })
    }

    getLeagueAvailabilityGrid(leagueId: number): Promise<RestResponse<LeagueAvailabilityGridViewModel>> {
        let url = `${AppConstants.getLeagueAvailability}?leagueId=${leagueId}`;
        return RestUtilities.get<LeagueAvailabilityGridViewModel>(url)
            .then((response) => {
                return response;
            })
    }

    getUnansweredAvailability(memberId: number): Promise<RestResponse<MatchAvailabilityViewModel[]>> {
        let url = `${AppConstants.getUnansweredAvailability}?memberId=${memberId}`;
        return RestUtilities.get<MatchAvailabilityViewModel[]>(url)
            .then((response) => {
                return response;
            })
    }

    getProspectiveMatches(memberId: number): Promise<RestResponse<MatchDetailsViewModel>> {
        let url = `${AppConstants.getProspectiveMatches}?memberId=${memberId}`;
        return RestUtilities.get<MatchDetailsViewModel>(url)
            .then((response) => {
                return response;
            })
    }

    updateMatch(match: MatchViewModel): Promise<RestResponse<MatchViewModel>> {
        let url = `${AppConstants.matchApi}/${match.matchId}`;
        return RestUtilities.put<MatchViewModel>(url,match)
            .then((response) => {
                return response;
            })
    }

    UpdateMatchAvailability(matchId: number, memberId: number, leagueId: number, value: Availability, action: MatchDeclineAction, inviteMemberIds: number[]): Promise<RestResponse<PlayerViewModel>> {
        let url = `${AppConstants.updateMatchAvailability}`;
        let data = { matchId: matchId, memberId: memberId, leagueId: leagueId, value: value, action: action, inviteMemberIds: inviteMemberIds };
        return RestUtilities.put<PlayerViewModel>(url, data)
            .then((response) => {
                return response;
            })
    }

    deleteMatch(matchId: number) {
        let url = `${AppConstants.matchApi}/${matchId}`;
        return RestUtilities.delete(url)
            .then((response) => {
                return response;
            });
    }

    RespondToMatch(matchId: number, leagueId: number, referringMemberId: number, availability: Availability): Promise<RestResponse<PlayerViewModel>> {
        let url = `${AppConstants.respondToMatchApi}`;
        let data = { matchId: matchId, memberId: referringMemberId, leagueId: leagueId, value: availability };
        return RestUtilities.put<PlayerViewModel>(url, data)
            .then((response) => {
                return response;
            })
    }

    reserveCourtsForMatch(matchId: number): Promise<RestResponse<boolean>> {
        let url = `${AppConstants.reserveCourtsForMatch}?matchId=${matchId}`;
        return RestUtilities.post<boolean>(url, null)
            .then((response) => {
                return response;
            })
    }

}
