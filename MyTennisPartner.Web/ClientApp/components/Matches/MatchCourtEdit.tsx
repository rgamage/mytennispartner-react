import * as React from "react";
import PlayerViewModel from "../../models/viewmodels/PlayerViewModel";
import { Availability } from "../../models/viewmodels/Enums";

export interface Props {
    editIsEnabled: boolean;
    players: PlayerViewModel[];
    deletePlayer: (memberIndex: number) => (event: any) => void;
    addPlayer: (lineIndex: number) => (event: any) => void;
    splitCourt?: boolean;
    lineIndex: number;
    expectedPlayersPerLine: number;
}

interface State {
}

export default class MatchCourtEdit extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
    }

    componentDidMount() {
    }

    render() {
        let missingPlayers = this.props.expectedPlayersPerLine - this.props.players.length;
        let showAddPlayer = missingPlayers > 0;
        return <div className="row">
            {this.props.players.map((player, memberIndex) => {
                return <div key={100 * this.props.lineIndex + 1 + memberIndex} className="col-12 col-md-6 court-edit-item">
                    <div className="row">
                        <div className={`col-12 court-edit-name${player.isSubstitute ? " sub" : ""}`}>
                            <span hidden={!this.props.editIsEnabled} onClick={this.props.deletePlayer(player.memberId)}>
                                <i className="fa fa-times" />
                            </span>
                            <span className={(player.availability == Availability.Unavailable ? "strike-through " : "") + (player.canReserveCourt ? "bold" : "")}>
                                {player.firstName} {player.lastName}
                             </span>
                             <i hidden={player.availability != Availability.Confirmed} className="fa fa-check" />
                             <span hidden={player.availability != Availability.Unknown}> ?</span>
                        </div>
                    </div>
                </div>
            }
            )}
            {(showAddPlayer && this.props.editIsEnabled) &&
                <div className="col-12 col-md-6 court-edit-item">
                    <div className="row">
                        <div className="col-12 court-edit-name add-new-player">
                            <a href="#" onClick={this.props.addPlayer(this.props.lineIndex)}>
                                <i className="fa fa-plus" />Add New Player
                            </a>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
}