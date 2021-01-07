import { Frequency, DayOfWeek } from "./Enums";

export default class LeagueSearchViewModel {
    leagueId: number;
    name: string;
    description: string;
    venueName: string;
    venueIconColor: string;
    meetingFrequency: Frequency;
    meetingDay: DayOfWeek;
    matchStartTime: string;
    isCaptain: boolean;

    // client-side properties
    matchStartTimeLocal: string;
}