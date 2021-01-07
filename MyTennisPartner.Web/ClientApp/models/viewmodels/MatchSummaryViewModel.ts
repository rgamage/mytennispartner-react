import { Availability, PlayFormat } from "./Enums";

export default class MatchSummaryViewModel {
    leagueId: number;
    matchId: number;
    leagueName: string;
    venueName: string;
    startTimeLocal: string;
    warmupTimeLocal: string;
    totalAvailable: number;
    availability: Availability;
    expectedPlayersPerLine: number;
    format: PlayFormat;
}