import * as React from "react";
import LineViewModel from "../../models/viewmodels/LineViewModel";
import MatchSummaryViewModel from "../../models/viewmodels/MatchSummaryViewModel";
import PlayerViewModel from "../../models/viewmodels/PlayerViewModel";
import ModelMapping from "../../utilities/ModelMapping";
import { Availability } from "../../models/viewmodels/Enums";

interface MatchInfoGridProps {
    match: MatchSummaryViewModel;
    lines: LineViewModel[];
    players: PlayerViewModel[];
}

export default function MatchInfoGrid(props: MatchInfoGridProps) {
    if (!props.match) {
        return null;
    }

    let linePlayers: PlayerViewModel[][] = [];
    props.lines.forEach(l => {
        let players = props.players.filter(p => p.lineId == l.lineId);
        linePlayers.push(players);
    });

    let availableSubs = props.players.filter(p => p.lineId == null && p.availability == Availability.Confirmed);
    let unAvailableSubs = props.players.filter(p => p.lineId == null && p.availability == Availability.Unavailable);
    let questionableSubs = props.players.filter(p => p.lineId == null && p.availability == Availability.Unknown);

    return <div className="d-flex flex-wrap">
        {props.lines.map((line, index) =>
            <table key={line.lineId} className="match-info-grid">
                <tbody>
                    <tr>
                        <th colSpan={linePlayers[index].length}>
                            {linePlayers[index].length != props.match.expectedPlayersPerLine && <i className="fa fa-exclamation-triangle mr-1" title={`Expected players: ${props.match.expectedPlayersPerLine}, actual count: ${linePlayers[index].length}`}></i>}
                                Court {line.courtNumber}
                            {line.isReserved && <i className="fa fa-check"></i>}
                            {!line.isReserved && <span>?</span>}
                        </th>
                        <th className="space"></th>
                    </tr>
                    <tr>
                        {linePlayers[index].map(player =>
                            <td className={ModelMapping.yesNoMaybeAvail(player.availability)} title={player.fullName} key={player.memberId}>{player.playerInitials}</td>
                        )}
                        <td className="space"></td>
                    </tr>
                </tbody>
            </table>
        )}
        <table className="match-info-grid">
            <tbody>
                <tr>
                    <th>Available</th>
                </tr>
                <tr>
                    <td className="truncate2 left">
                        {availableSubs.map(p =>
                            <i className="fa fa-check" key={p.memberId} title={p.fullName} />
                        )}
                        {unAvailableSubs.map(p =>
                            <i className="fa fa-times" key={p.memberId} title={p.fullName} />
                        )}
                        {questionableSubs.map(p =>
                            <span className="maybe" key={p.memberId} title={p.fullName}>?</span>
                        )}
                    </td>
                </tr>
            </tbody>
        </table>
        </div>
}