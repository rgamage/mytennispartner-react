import * as React from "react";
import { MatchDeclineAction } from "../../models/viewmodels/Enums";
import { MemberNameViewModel } from "../../models/viewmodels/MemberViewModel";
import ArrayHelper from "../../utilities/ArrayHelper";
import PlayerPickList from "./PlayerPickList";
import { App } from "../App";
import MemberService from "../../services/MemberService";
import Notification from "../../utilities/Notification";
import { AppStateProps } from "ClientApp/models/AppStateProps";

let memberService = new MemberService();

interface MatchDeclineProps extends AppStateProps {
    leagueId: number;
    matchId: number;
    onClose: (action: MatchDeclineAction, members: MemberNameViewModel[]) => void;
}

interface MatchDeclineState {
    showPlayerList: boolean;
    players: MemberNameViewModel[];
    loadingPickList: boolean;
}

export class MatchDecline extends React.Component<MatchDeclineProps, MatchDeclineState> {
    constructor(props: MatchDeclineProps) {
        super(props);
        this.state = {
            showPlayerList: false,
            players: [],
            loadingPickList: false
        };
    }

    onDoNothing = (event: any) => {
        this.props.onClose(MatchDeclineAction.DoNothing, null);
    }

    onInviteAll = (event: any) => {
        this.props.onClose(MatchDeclineAction.InviteAll, null);
    }

    onSelectPlayers = (event: any) => {
        // show player list so user can select which players to invite
        this.setState({ showPlayerList: true });
        this.getSubPickList();
    }

    onClosePickList = (playerList: MemberNameViewModel[]) => {
        // hide player pick list
        this.setState({ showPlayerList: false });
        // call close method, if we got a list of players
        if (ArrayHelper.isNonEmptyArray(playerList)) {
            this.props.onClose(MatchDeclineAction.InviteSome, playerList);
        }
        // else we keep ourselves open for user to choose another action
    }

    getSubPickList = () => {
        memberService.getSubPickList(this.props.matchId).then(response => {
            if (!response.is_error) {
                let playerList = response.content;
                if (!ArrayHelper.isNonEmptyArray(playerList)) {
                    playerList = [];
                }
                this.setState({ players: playerList, loadingPickList: false });
            }
            else {
                this.setState({ loadingPickList: false });
                Notification.notifyError(response.error_content.errorMessage);
            }
        });

    }

    render() {
        if (this.state.showPlayerList) {
            return <div className="mtp-main-content">
                <div className="row">
                    <div className="col-12">
                        <PlayerPickList playerList={this.state.players} onClose={this.onClosePickList} leagueId={this.props.leagueId} app={this.props.app} appState={this.props.appState} loadingPickList={this.state.loadingPickList} isMatchDeclineList={true} />
                    </div>
                </div>
            </div>
        }
        return <div className="mtp-main-content">
            <div className="row margin-top">
                <div className="col-12">
                    <h4>
                        Please select which actions to take, in order to fill your spot in the line-up.
                    </h4> 
                </div>
                <div className="col-12 center-col">
                    <button className="btn btn-primary" onClick={this.onInviteAll}>Invite All</button>
                </div>
                <div className="col-12 center-col">
                    <p>
                        Sends match invitation to all available sub and regular players who are not already in the line-up.  First respondent will be added to the line-up in your place.  You will remain in the line-up until your spot is taken.
                    </p>
                </div>
                <div className="col-12 center-col">
                    <button className="btn btn-primary" onClick={this.onSelectPlayers}>Invite Some</button>
                </div>
                <div className="col-12 center-col">
                    <p>
                        Select which player(s) to invite to the match.  First respondent will be added to the match in your place.
                    </p>
                </div>
                <div className="col-12 center-col">
                    <button className="btn btn-primary" onClick={this.onDoNothing}>Invite None</button>
                </div>
                <div className="col-12 center-col">
                    <p>
                        No action will be taken.  You will remain in the line-up, marked as "unavailable".  You will remain responsible for filling your spot in the line-up.
                    </p>
                </div>
            </div>
        </div>
    }
}