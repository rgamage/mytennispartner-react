import * as React from "react";
import { NavLink, RouteComponentProps } from 'react-router-dom';
import { LoginState, AppState } from '.././App';
import Notification from "../../utilities/Notification";
import MatchService from "../../services/MatchService";
import { Availability } from "../../models/viewmodels/Enums";
import Loader from "../Common/Loader";
import MatchViewModel from "../../models/viewmodels/MatchViewModel";
import { AppStatePropsRoute } from "../../models/AppStateProps";

let matchService = new MatchService();

interface State {
    matchId: number,
    leagueId: number,
    referringMemberId: number,
    availability: number,
    success: boolean,
    fail: boolean,
    match: MatchViewModel,
    isSubOpportunity: boolean
}

export default class RespondToMatch extends React.Component<AppStatePropsRoute, State> {
    constructor(props: AppStatePropsRoute) {
        super(props);

        const search = props.location.search;
        const params = new URLSearchParams(search);
        let matchId = params.get('matchId');
        let leagueId = params.get('leagueId');
        let referringMemberId = params.get('referringMemberId');
        let availability = params.get('availability');

        this.state = {
            matchId: parseInt(matchId),
            leagueId: parseInt(leagueId),
            referringMemberId: parseInt(referringMemberId),
            availability: parseInt(availability),
            success: false,
            fail: false,
            match: null,
            isSubOpportunity: parseInt(referringMemberId) > 0  // if we have a referring member, then it is a sub opportunity
        }
        console.log(`RespondToMatch: state on load = ${JSON.stringify(this.state)}`);
    }

    componentDidMount() {
         matchService.RespondToMatch(this.state.matchId, this.state.leagueId, this.state.referringMemberId, this.state.availability as Availability).then((response) => {
             if (!response.is_error) {
                 this.setState({ success: true, fail: false });
                 matchService.getMatch(this.state.matchId).then((response2) => {
                     if (!response2.is_error) {
                         let match = response2.content;
                         MatchViewModel.mapMatchToViewModel(match);
                         this.setState({ match: match });
                     }
                 })
             }
             else {
                 this.setState({ success: false, fail: true });
                 Notification.notifyError(response.error_content.errorMessage);
             }
         })
             .catch((err) => {
                 if (this.props.appState.loginState == LoginState.LoggedIn) {
                     console.log("Error responding to match");
                     Notification.notifyError(err.message);
                 }
             });
    }

    render() {
        let message = null;

        if (!this.state.success && !this.state.fail) {
            message = <div>
                <Loader />
            </div>
        }

        if (this.state.success && this.state.availability == Availability.Confirmed) {
            if (this.state.isSubOpportunity) {
                message = <div className="alert alert-success" role="alert">
                    <strong>Match Confirmed!</strong>  You have been added to this match.
            </div>
            }
            else {
                message = <div className="alert alert-success" role="alert">
                    <strong>Availability Confirmed!</strong>  Thank you for letting us know you are available.
            </div>
            }
        }

        if (this.state.success && this.state.availability == Availability.Unavailable) {
            if (this.state.isSubOpportunity) {
                message = <div className="alert alert-success" role="alert">
                    <strong>Match Declined.</strong>  Thank you for letting us know you are unavailable.
            </div>
            }
            else {
                message = <div className="alert alert-success" role="alert">
                    <strong>Sorry you can't make it.</strong>  Thank you for letting us know you are unavailable.
            </div>
            }
        }

        if (this.state.fail) {
            message = <div className="alert alert-error" role="alert">
                <strong>Failed</strong>  Something went wrong.
            </div>
        }

        return <div className="mtp-main-content respond-to-match">
            {message}
            {this.state.match && <div>
                <div>{this.state.match.leagueName}</div>
                <div>{this.state.match.startTimeMoment.format("dddd MMMM Do, h:mm a")}</div>
                <div>{this.state.match.matchVenue.name}</div>
              </div>
            }
            <br />
            <div>
                To review your current matches, return to your <NavLink to="/dashboard">My Tennis</NavLink> page.
            </div>
        </div>
    }
}