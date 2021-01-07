import { MemberNameViewModel } from "./MemberViewModel";
import PlayerViewModel from "./PlayerViewModel";

// class to hold league availability grid data, to show player avail for upcoming matches
export default class LeagueAvailabilityViewModel {
    memberName: MemberNameViewModel;
    leaguePlayers: PlayerViewModel[];
}
