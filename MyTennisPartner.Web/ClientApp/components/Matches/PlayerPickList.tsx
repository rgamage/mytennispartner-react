import * as React from "react";
import { MemberNameViewModel } from "../../models/viewmodels/MemberViewModel";
import Branding from "../../branding/Branding";
import { Link } from "react-router-dom";
import { Availability } from "../../models/viewmodels/Enums";
import Loader from "../Common/Loader";
import OkCancelBar from "../Common/OkCancelBar";
import { App } from "../App";
import ArrayHelper from "../../utilities/ArrayHelper";
import { AppStateProps } from "ClientApp/models/AppStateProps";


export interface Props extends AppStateProps {
    playerList: MemberNameViewModel[];
    onClose: (playerList: MemberNameViewModel[]) => void;    
    leagueId: number;
    loadingPickList: boolean;
    isMatchDeclineList: boolean;  // true if this list is being used when a player is declining a match (picking a sub)
}

class pageModel {
    constructor() {
        this.selectAllRegular = false;
        this.selectAllSubs = false;
        this.selectAllAvail = false;
    }
    selectAllRegular: boolean;
    selectAllSubs: boolean;
    selectAllAvail: boolean;
}

interface State {
    playerList: MemberNameViewModel[];
    model: pageModel;
}

export default class PlayerPickList extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            playerList: props.playerList,
            model: new pageModel()
        }
    }

    componentDidUpdate() {
        if (!ArrayHelper.isNonEmptyArray(this.state.playerList)) {
            // we have not yet received a valid player list, so let's try to update it
            if (ArrayHelper.isNonEmptyArray(this.props.playerList)) {
                this.setState({ playerList: this.props.playerList });
            }
        }
    }

    // function to handle input field changes, set corresponding state variables
    handlePlayerCheckChange = (index: number) => (event: React.FormEvent<HTMLInputElement>) => {
        const target = event.currentTarget;
        let value = target.checked;
        let playerList = this.state.playerList;
        playerList[index].selected = value as boolean;
        this.setState({ playerList: playerList });
    }

    handleCheckChange = (event: React.FormEvent<HTMLInputElement>) => {
        console.log(`handleCheckChange before changes: selectReg = ${this.state.model.selectAllRegular}, selectSubs = ${this.state.model.selectAllSubs}, selectAllAvail = ${this.state.model.selectAllAvail}`);
        const target = event.currentTarget;
        let value = target.checked;
        let name = target.name;
        let model = this.state.model;
        model[name] = value;
        console.log(`handleCheckChange about to set: selectReg = ${model.selectAllRegular}, selectSubs = ${model.selectAllSubs}, selectAvail = ${model.selectAllAvail}`);
        let players = this.state.playerList;
        for (let i = 0; i < players.length; i++) {
            let p = players[i];
            if (name == "selectAllRegular" && !p.isSubstitute && p.availability != Availability.Unavailable) {
                p.selected = value;
            }
            if (name == "selectAllSubs" && p.isSubstitute && p.availability != Availability.Unavailable) {
                p.selected = value;
            }
            if (name == "selectAllAvail" && p.availability == Availability.Confirmed) {
                p.selected = value;
            }
        }
        this.setState({ model: model, playerList: players });
    }

    onOk = (event: any) => {
        console.log("Ok clicked");
        this.props.onClose(this.state.playerList && this.state.playerList.filter(p => p.selected))
    }

    onCancel = (event: any) => {
        console.log("Cancel clicked");
        this.props.onClose([])
    }

    render() {
        if (this.props.loadingPickList) {
            return <Loader />
        }
        let playerList = this.state.playerList || [];
        return <div className="player-picklist">
            <OkCancelBar show onOk={this.onOk} onCancel={this.onCancel} addOffset={this.props.appState.isMobileWindowSize} />
            <div className="row">
                <div className="col-12 col-sm-4">
                    <input className="custom-control-input" name="selectAllRegular" id="selectAllRegular" onChange={this.handleCheckChange} type="checkbox" checked={this.state.model.selectAllRegular} />
                    <label className="custom-control-label" htmlFor="selectAllRegular">
                        All Regulars
                    </label>
                </div>
                <div className="col-12 col-sm-4">
                    <input className="custom-control-input" name="selectAllSubs" id="selectAllSubs" onChange={this.handleCheckChange} type="checkbox" checked={this.state.model.selectAllSubs} />
                    <label className="custom-control-label sub" htmlFor="selectAllSubs">
                        All Subs
                    </label>
                </div>
                <div className="col-12 col-sm-4">
                    <input className="custom-control-input" name="selectAllAvail" id="selectAllAvail" onChange={this.handleCheckChange} type="checkbox" checked={this.state.model.selectAllAvail} />
                    <label className="custom-control-label" htmlFor="selectAllAvail">
                        All Available
                    </label>
                    <i className="fa fa-check" />
                </div>
            </div>
            <div className="row">
                 { 
                playerList.map((player, index) =>
                    <div key={index} className="col-12 col-sm-6 col-md-4">
                            <input className="custom-control-input pointer" id={`player${index}`} onChange={this.handlePlayerCheckChange(index)} type="checkbox" checked={this.state.playerList[index].selected || false} />
                        <label className={`custom-control-label pointer${player.isSubstitute ? " sub " : " "}${player.availability == Availability.Unavailable ? "strike-through " : ""}${index == 0 && this.props.isMatchDeclineList && player.availability == Availability.Confirmed ? "bold" : ""}`} htmlFor={`player${index}`}>
                                <span>{player.firstName} {player.lastName}</span>
                                <span>
                                    <i hidden={player.availability != Availability.Confirmed} className="margin-left fa fa-check" />
                                    <i hidden={player.availability != Availability.Unknown} className="margin-left fa fa-clock-o" />
                                </span>
                                {(index == 0 && this.props.isMatchDeclineList && player.availability == Availability.Confirmed) &&
                                    <span>(Recommended)</span>
                                }
                            </label>
                    </div>
                    )}
            </div>
            <h4 hidden={playerList.length > 0} style={{marginTop: "30px"}}>Don't see the players you're looking for?  They may already be in the line-up, or you may need to add them to your {Branding.league}'s <Link to={`/leagues/roster/${this.props.leagueId}`}>roster.</Link></h4>
        </div>
    }
}