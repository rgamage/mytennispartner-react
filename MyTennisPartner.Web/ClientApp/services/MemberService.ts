import AppConstants from '../models/app-constants';
import RestUtilities, { RestResponse } from './RestUtilities';
import AuthStore from '../stores/Auth';
import MemberViewModel, { MemberNameViewModel } from "../models/viewmodels/MemberViewModel";
import LeagueMemberViewModel from "../models/viewmodels/LeagueMemberViewModel";
import { Gender } from '../models/viewmodels/Enums';
import MatchViewModel from '../models/viewmodels/MatchViewModel';

interface IMemberResponse {
    result: MemberViewModel;
}

export default class MemberService {

    getMembers(): Promise<RestResponse<MemberViewModel[]>> {
        let url = `${AppConstants.getMembers}`;
        return RestUtilities.get<MemberViewModel[]>(url)
            .then((response) => {
                return response;
            });
    }

    getMemberInfo(userId: string): Promise<RestResponse<MemberViewModel>> {
        let url = `${AppConstants.getMemberByUser}/${userId}`;
        return RestUtilities.get<MemberViewModel>(url)
            .then((response) => {
                return response;
            });
    }

    createMember(member: MemberViewModel): Promise<RestResponse<MemberViewModel>> {
        return RestUtilities.post<MemberViewModel>(AppConstants.createMember, member)
            .then((response) => {
                    return response;
                });
    }

    updateMember(member: MemberViewModel): Promise<RestResponse<MemberViewModel>> {
        return RestUtilities.put<MemberViewModel>(`${AppConstants.updateMember}/${member.memberId}`, member)
            .then((response) => {
                return response;
            });
    }

    updateLeagueMember(leagueMember: LeagueMemberViewModel): Promise<RestResponse<LeagueMemberViewModel>> {
        return RestUtilities.put<LeagueMemberViewModel>(`${AppConstants.leagueMembersApi}`, leagueMember)
            .then((response) => {
                return response;
            });
    }

    getLeagueMembers(leagueId: number, memberId: number = 0): Promise<RestResponse<MemberViewModel[]>> {
        let url = `${AppConstants.leagueMembersApi}/${leagueId}?memberId=${memberId}`;
        return RestUtilities.get<MemberViewModel[]>(url)
            .then((response) => {
                return response;
            });
    }

    searchMembers(search: string, leagueId: number = 0, excludeLeagueId: number = 0, gender: Gender = Gender.Unknown, numResults: number = 10): Promise<RestResponse<MemberNameViewModel[]>> {
        let url = `${AppConstants.searchMembers}?search=${search}&gender=${gender}&leagueId=${leagueId}&excludeLeagueId=${excludeLeagueId}&numResults=${numResults}`;
        return RestUtilities.get<MemberNameViewModel[]>(url)
            .then((response) => {
                return response;
            });
    }

    getPlayerPickList(match: MatchViewModel): Promise<RestResponse<MemberNameViewModel[]>> {
        let url = AppConstants.getPlayerPickList;
        return RestUtilities.post<MemberNameViewModel[]>(url, match)
            .then((response) => {
                return response;
            });
    }

    getSubPickList(matchId: number): Promise<RestResponse<MemberNameViewModel[]>> {
        let url = `${AppConstants.getSubPickList}?matchId=${matchId}`;
        return RestUtilities.get<MemberNameViewModel[]>(url)
            .then((response) => {
                return response;
            });
    }

    deleteLeagueMember(leagueId: number, memberId: number) {
        let url = `${AppConstants.leagueMembersApi}/${leagueId}/${memberId}`;
        return RestUtilities.delete(url)
            .then((response) => {
                return response;
            });
    }

    addLeagueMember(leagueId: number, memberId: string, isSub: boolean=false) {
        let url = `${AppConstants.leagueMembersApi}/${leagueId}/${memberId}?isSub=${isSub.toString()}`;
        return RestUtilities.post<void>(url, null)
            .then((response) => {
                return response;
            });
    }
}

