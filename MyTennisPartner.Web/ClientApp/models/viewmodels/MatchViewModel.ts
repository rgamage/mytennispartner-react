import VenueViewModel from "./VenueViewModel";
import { PlayFormat, Availability } from "./Enums";
import LineViewModel from "./LineViewModel";
import * as moment from "moment";
import PlayerViewModel from "./PlayerViewModel";

export default class MatchViewModel {
    constructor() {
        //this.homeMatch = true;
        //let startTime = new Date();
        //startTime.setHours((startTime.getHours() + 1) % 24, 0, 0);
        //this.startTime = startTime;
        //this.endTime = startTime;
        //this.endTime.setHours((startTime.getHours() + 2) % 24, 30, 0);
        //this.warmupTime = startTime;
        //this.warmupDuration = 0;
        //this.lines = [];
        //this.format = PlayFormat.MensDoubles;
    }

    matchId: number;
    leagueId: number;
    sessionId?: number;
    startTime: Date;
    endTime: Date;
    warmupTime: Date;
    matchVenue: VenueViewModel;
    homeMatch: boolean;
    format: PlayFormat;
    autoReserveCourts: boolean;
    venueHasReservationSystem: boolean;
    lines: LineViewModel[];
    players: PlayerViewModel[];
    markNewCourtsReserved: boolean;
    markNewPlayersConfirmed: boolean;

    // client-only fields
    startTimeMoment: moment.Moment;
    endTimeMoment: moment.Moment;
    warmupTimeMoment: moment.Moment;
    showWarmupTime: boolean;
    venueIconColor: string;
    playerAvailability: Availability;
    leagueName: string;
    warmupDuration: number;
    matchDuration: number;
    expectedPlayersPerLine: number;
    startTimeLocal?: Date;

    static mapMatchToViewModel(match: MatchViewModel): MatchViewModel {
        match.startTimeMoment = moment(moment.utc(match.startTime).local());
        match.endTimeMoment = moment(moment.utc(match.endTime).local());
        match.warmupTimeMoment = moment(moment.utc(match.warmupTime).local());
        match.warmupDuration = match.startTimeMoment.diff(match.warmupTimeMoment, "minutes");
        match.matchDuration = match.endTimeMoment.diff(match.startTimeMoment, "minutes");
        return match;
    }

    static mapMatchViewModelToMatch(match: MatchViewModel): MatchViewModel {
        match.startTime = match.startTimeMoment.toDate();
        match.startTimeLocal = match.startTime;
        match.endTime =    match.startTimeMoment.clone().add(match.matchDuration, "minutes").toDate();
        match.warmupTime = match.startTimeMoment.clone().subtract(match.warmupDuration, "minutes").toDate();
        return match;
    }
}