import { Availability, MatchDeclineAction } from "./Enums";
import { MemberNameViewModel } from "./MemberViewModel";

export default class AvailabilityRequest {
    constructor(matchId: number, leagueId: number, memberId: number, availability: Availability, isInLineup: boolean) {
        this.matchId = matchId;
        this.leagueId = leagueId;
        this.memberId = memberId;
        this.availability = availability;
        this.members = [];
        this.isInLineup = isInLineup;
    }

    matchId: number;
    leagueId: number;
    memberId: number;
    availability: Availability;
    action: MatchDeclineAction;
    members: MemberNameViewModel[];
    isInLineup: boolean;

    get postDeclineMessage(): string {
        let message = "";
        switch (this.action) {
            case MatchDeclineAction.DoNothing:
                message = "No invitations will be sent.  You remain responsible for finding another player to fill your spot.";
                break;
            case MatchDeclineAction.InviteAll:
                message = "Invitations will be sent to all players that are not in the line-up, and have open or unknown availability.";
                break;
            case MatchDeclineAction.InviteSome:
                message = "Invitations will be sent to those players you have selected";
                break;
            default:
            // do nothing
        }
        return message;
    }
}