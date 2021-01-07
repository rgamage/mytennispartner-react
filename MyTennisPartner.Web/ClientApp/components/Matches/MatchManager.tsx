import * as React from "react";
import { RouteComponentProps } from 'react-router-dom';
import { App } from '.././App';
import { Route } from 'react-router-dom';
import Notification from "../../utilities/Notification";
import MatchService from "../../services/MatchService";
import MatchViewModel from "../../models/viewmodels/MatchViewModel";
import MatchDetails from "./MatchDetails";
import Branding from "../../branding/Branding";
import LeagueService from "../../services/LeagueService";
import LineViewModel from "../../models/viewmodels/LineViewModel";
import { AppStateProps } from "ClientApp/models/AppStateProps";

let matchService = new MatchService();
let leagueService = new LeagueService();

export interface Props extends AppStateProps, RouteComponentProps<{ matchId: string, leagueId: string }> {
    matchId: number;
    leagueId: number;
}

interface MatchStates {
    matchId: number;
    match: MatchViewModel;
    matchNotFound: boolean;
    leagueId: number;
    editIsEnabled: boolean;
}

export default class MatchManager extends React.Component<Props, MatchStates> {
    constructor(props: Props) {
        super(props);
        this.state = {
            matchId: props.matchId,
            match: new MatchViewModel(),
            matchNotFound: false,
            leagueId: props.leagueId,
            editIsEnabled: false
        }
    }

    componentDidMount() {
        if (this.state.matchId > 0) {
            this.getMatch(this.state.matchId);
        }
        else {
            this.createMatch();
        }
    }

    getMatchEditibility = () => {
        console.log("getMatchEditibility running");
        let isAdmin = this.props.appState.user && this.props.appState.user.isAdmin;
        if (isAdmin) {
            // we're an admin, so we can edit
            //console.log(`MatchManager- setting editIsEnabled = true, we are admin.`);
            this.setState({ editIsEnabled: true });
            this.props.app.setState({ addTopOffset: true });
            return;
        }

        let memberId = this.props.appState.member && this.props.appState.member.memberId;
        if (!memberId) {
            // we don't have a member profile, so we can't edit
            //console.log(`MatchManager- setting editIsEnabled = false, we don't have a member profile`);
            this.setState({ editIsEnabled: false });
            this.props.app.setState({ addTopOffset: false });
            return;
        }
        leagueService.getLeagueEditability(this.props.leagueId, memberId).then((response) => {
            if (!response.is_error) {
                //console.log(`MatchManager- setting editIsEnabled = ${response.content}, from api`);
                this.setState({ editIsEnabled: response.content });
                this.props.app.setState({ addTopOffset: response.content });
            }
            else {
                console.log(`failed to get editability for league ${this.props.leagueId}, member ${memberId}`);
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    createMatch = () => {
        matchService.getNewMatch(this.state.leagueId).then((response) => {
            if (!response.is_error) {
                let match = MatchViewModel.mapMatchToViewModel(response.content);
                let line = new LineViewModel();
                line.courtNumber = "1";
                line.isReserved = match.markNewCourtsReserved;
                match.lines = [line] as LineViewModel[];
                this.setState({ match: match, editIsEnabled: true });
            }
            else {
                console.log('failed to get a new match');
                this.setState({ matchNotFound: true });
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    getMatch = (id: number) => {
        if (!id) {
            console.log(`getMatch failed, id=${id}`);
        }
        matchService.getMatch(id).then((response) => {
            if (!response.is_error) {
                let match = MatchViewModel.mapMatchToViewModel(response.content);
                this.setState({ match: match });
                this.getMatchEditibility();
            }
            else {
                console.log(`Failed to get a match, id=${id}`);
                this.setState({ matchNotFound: true });
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    render() {
        if (this.state.matchNotFound) {
            return <div className="mtp-main-content">
                <h2>Edit {Branding.Match}</h2>
                <p><i className="fa fa-warning" />&ensp;No {Branding.Match} found.  This match may have been cancelled or deleted.</p>
            </div>
        }
        if (!this.state.match.leagueId) {
            //console.log(`MatchManager render: no match.leagueId, returning null`);
            return null;
        }
        return this.state.match && <div className="mtp-main-content">
                <div className="row">
                    <div id="ManageMatchesRouteDiv" className="col-12">
                        <Route path='/matches/details/:id' render={(routeProps) => <MatchDetails  {...routeProps} selectedMatch={this.state.match} editIsEnabled={this.state.editIsEnabled} app={this.props.app} appState={this.props.appState}/>} />
                    </div>
                </div>
            </div>
        
    }
}