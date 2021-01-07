import AppConstants from '../models/app-constants';
import RestUtilities, { RestResponse } from './RestUtilities';
import AuthStore from '../stores/Auth';
import LeagueSearchViewModel from '../models/viewmodels/LeagueSearchViewModel';
import LeagueSummaryViewModel from "../models/viewmodels/LeagueSummaryViewModel";

export default class LeagueService {

    searchLeagues(search: string): Promise<RestResponse<LeagueSearchViewModel[]>> {
        let url = `${AppConstants.searchLeagues}?search=${search}`;
        return RestUtilities.get<LeagueSearchViewModel[]>(url)
            .then((response) => {
                return response;
            });
    }

    getLeaguesByMember(memberId: number, pageSize: number): Promise<RestResponse<LeagueSearchViewModel[]>> {
        let url = `${AppConstants.leaguesByMember}?memberId=${memberId}&pageSize=${pageSize}`;
        return RestUtilities.get<LeagueSearchViewModel[]>(url)
            .then((response) => {
                return response;
            });
    }

    getLeagueSummary(id: number): Promise<RestResponse<LeagueSummaryViewModel>> {
        let url = `${AppConstants.getLeagueSummary}/${id}`;
        return RestUtilities.get<LeagueSummaryViewModel>(url)
            .then((response) => {
                return response;
            });
    }

    getLeagueEditability(leagueId: number, memberId: number): Promise<RestResponse<boolean>> {
        let url = `${AppConstants.getLeagueEditAbility}/${leagueId}/${memberId}`;
        return RestUtilities.get<boolean>(url)
            .then((response) => {
                return response;
            });
    }

    createLeague(league: LeagueSummaryViewModel): Promise<RestResponse<LeagueSummaryViewModel>> {
        return RestUtilities.post<LeagueSummaryViewModel>(AppConstants.createLeague, league)
            .then((response) => {
                return response;
            });
    }

    updateLeague(league: LeagueSummaryViewModel): Promise<RestResponse<LeagueSummaryViewModel>> {
        return RestUtilities.put<LeagueSummaryViewModel>(`${AppConstants.updateLeague}/${league.leagueId}`, league)
            .then((response) => {
                return response;
            });
    }

    deleteLeague(leagueId: number) {
        let url = `${AppConstants.deleteLeague}/${leagueId}`;
        return RestUtilities.delete(url)
            .then((response) => {
                return response;
            });
    }

}

