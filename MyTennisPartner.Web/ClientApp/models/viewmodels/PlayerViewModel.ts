import { Availability, Gender } from "./Enums";

export default class PlayerViewModel {
    id: number;
    leagueId: number;
    matchId: number;
    memberId: number;
    leagueMemberId: number;
    lineId: number;
    availability: Availability;
    modifiedDate: Date;
    isInLineup: boolean;
    isSubstitute: boolean;
    gender: Gender;
    firstName: string;
    lastName: string;
    isCaptain: boolean;
    guid: string;
    canReserveCourt: boolean;
    playerInitials: string;
    fullName: string;
}
