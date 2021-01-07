import PlayerViewModel from "./PlayerViewModel";
import NumberFormatting from "../../utilities/NumberFormatting";

export default class LineViewModel {
    constructor() {
        this.lineId = 0;
        this.players = [];
        this.guid = NumberFormatting.guid();
        this.courtNumber = "";
        this.isReserved = false;
        this.courtNumberOverridden = false;
        this.courtsAvailable = "";
    }

    lineId: number;
    matchId: number;
    leagueId: number;
    leagueName: string;
    courtNumber: string;
    players: PlayerViewModel[];
    guid: string;
    isReserved: boolean;
    lineWarning: boolean;
    courtNumberOverridden: boolean;
    courtsAvailable: string;

    // client-only properties
    showAddPlayer: boolean;
}
