import LeagueSummaryViewModel from "./LeagueSummaryViewModel";
import MatchSummaryViewModel from "./MatchSummaryViewModel";
import LineViewModel from "./LineViewModel";
import PlayerViewModel from "./PlayerViewModel";

export default class MatchDetailsViewModel {
    constructor() {
        this.matches = [] as MatchSummaryViewModel[];
        this.lines = [] as LineViewModel[];
        this.players = [] as PlayerViewModel[];
    }
    matches: MatchSummaryViewModel[];
    lines: LineViewModel[];
    players: PlayerViewModel[];
}