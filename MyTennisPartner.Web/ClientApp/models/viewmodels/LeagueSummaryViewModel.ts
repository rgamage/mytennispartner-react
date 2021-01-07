import VenueViewModel from "./VenueViewModel";
import { MemberNameViewModel } from "./MemberviewModel"
import AppConstants from "../app-constants";
import { PlayFormat, DayOfWeek, AmPm, Frequency } from "./Enums";
import Branding from "../../branding/Branding";
import * as moment from "moment";
import NumberFormatting from "../../utilities/NumberFormatting";
import LeagueSearchViewModel from "./LeagueSearchViewModel";
import TimeFormatting from "../../utilities/TimeForrmatting";

export default class LeagueSummaryViewModel {
    constructor() {

        // set defaults for new league
        this.name = `My New ${Branding.League}`;
        this.description = "My Description";
        this.details = "Detailed description / notes";
        this.minimumAge = 18;
        this.minimumRanking = "3.0";
        this.maximumRanking = "4.0";
        this.defaultNumberOfLines = 4;
        this.rotatePartners = false;
        this.defaultFormat = PlayFormat.MensDoubles;
        this.scoreTracking = false;
        this.isTemplate = false;
        this.meetingFrequency = Frequency.Weekly;
        this.meetingDay = DayOfWeek.Sunday;
        this.warmupTimeMinutes = 0;
        this.maxNumberRegularMembers = 16;
        this.matchStartTime = "17:00";
        this.isCaptain = true;
        this.autoAddToLineup = false;
        this.autoReserveCourts = false;
    }

    static mapLeagueToViewModel(model: LeagueSummaryViewModel | LeagueSearchViewModel) {
        // map UTC match start time to local time for client
        model.matchStartTimeLocal = TimeFormatting.utcToLocalTimeString(model.matchStartTime);
        return model;
    }

    static mapLeagueViewModelToLeague(model: LeagueSummaryViewModel | LeagueSearchViewModel) {
        // map local time to UTC match start time for server
        model.matchStartTime = TimeFormatting.localToUtcTimeString(model.matchStartTimeLocal);
        model.matchStartTimeLocal = model.matchStartTime;
        return model;
    }

    leagueId: number;
    name: string;
    description: string;
    details: string;
    owner: MemberNameViewModel;
    minimumAge: number;
    minimumRanking: string;
    maximumRanking: string;
    defaultNumberOfLines: number;
    rotatePartners: boolean;
    defaultFormat: PlayFormat;
    scoreTracking: boolean;
    isTemplate: boolean;
    meetingFrequency: Frequency; 
    meetingDay: DayOfWeek; 
    warmupTimeMinutes: number;
    homeVenue: VenueViewModel; 
    maxNumberRegularMembers: number;
    matchStartTime: string;
    isCaptain: boolean;
    numberMatchesPerSession: number;
    autoAddToLineup: boolean;
    autoReserveCourts: boolean;
    markNewCourtsReserved: boolean;
    markNewPlayersConfirmed: boolean;

    // server-calculated fields 
    regularMemberCount: number;
    subMemberCount: number;
    upcomingMatchCount: number;
    totalMatchCount: number;

    // client-calculated fields
    matchStartTimeLocal: string;
}
