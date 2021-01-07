import PlayerViewModel from "../models/viewmodels/PlayerViewModel";
import MemberViewModel, { MemberNameViewModel } from "../models/viewmodels/MemberViewModel";
import { Availability } from "../models/viewmodels/Enums";

export default class ModelMapping {
    static mapMemberToPlayer(m: MemberNameViewModel, leagueId: number, matchId: number, lineId?: number): PlayerViewModel {
        let p = new PlayerViewModel();
        p.firstName = m.firstName;
        p.lastName = m.lastName;
        p.gender = m.gender;
        p.memberId = m.memberId;
        p.isSubstitute = m.isSubstitute;
        p.isCaptain = m.isCaptain;
        p.leagueMemberId = m.leagueMemberId;
        p.availability = Availability.Unknown;
        p.id = m.playerId;
        p.matchId = matchId;
        p.leagueId = leagueId;
        p.lineId = lineId;
        return p;
    }

    static mapLineMemberToMember(p: PlayerViewModel): MemberViewModel {
        let m = new MemberViewModel();
        m.firstName = p.firstName;
        m.lastName = p.lastName;
        m.gender = p.gender;
        m.memberId = p.memberId;
        m.isSubstitute = p.isSubstitute;
        m.isCaptain = p.isCaptain;        
        return m;
    }

    /**
     * Given an availability enum, return an appropriate english word description
     * @param a
     */
    static yesNoMaybeAvail(a: Availability) {
        switch (a) {
            case Availability.Confirmed: return "yes";
            case Availability.Unavailable: return "no";
            case Availability.Unknown: return "maybe";
            default: return "maybe";
        }
    }
}