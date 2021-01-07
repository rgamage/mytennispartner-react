import * as React from "react";
import { NavLink, RouteComponentProps } from 'react-router-dom';
import { Route } from 'react-router-dom';
import { LeagueSummary } from "./LeagueSummary";
import { LeagueDetails } from "./LeagueDetails";
import { LeagueRoster } from "./LeagueRoster";
import { LeagueSchedule } from "./LeagueSchedule";
import LeagueSummaryViewModel from "../../models/viewmodels/LeagueSummaryViewModel"
import LeagueService from "../../services/LeagueService";
import { MemberNameViewModel } from "../..//models/viewmodels/MemberviewModel";
import Branding from "../../branding/Branding";
import Notification from "../../utilities/Notification";
import LeagueAvailability from "./LeagueAvailability";
import { AppStateProps } from "../../models/AppStateProps";

let leagueService = new LeagueService();

export interface LeagueTabProps extends AppStateProps, RouteComponentProps<{ leagueId: string }> {
    leagueState: LeagueStates;
}


interface LeagueStates {
    leagueId: number;
    league: LeagueSummaryViewModel;
    leagueNotFound: boolean;
    editIsEnabled: boolean;
    isLoading: boolean;
}

interface ManageLeaguesProps extends AppStateProps, RouteComponentProps<{ leagueId: string }> {
    leagueId: number;
}

export class ManageLeagues extends React.Component<ManageLeaguesProps, LeagueStates> {
    constructor(props: ManageLeaguesProps) {
        super(props);
        let leagueId = props.match.params.leagueId;
        this.state = {
            leagueId: parseInt(leagueId),
            league: new LeagueSummaryViewModel(),
            leagueNotFound: false,
            editIsEnabled: false,
            isLoading: false
        }
        console.log(`LeagueManager: constructor running, leagueId = ${this.state.leagueId}`);
    }

    componentDidMount() {
        if (this.state.leagueId > 0) {
            //console.log("Calling getleagueSummary: leagueId = " + this.state.leagueId);
            console.log(`LeagueManager: componentDidMount, getting leagueSummary`);
            this.getLeagueSummary(this.state.leagueId);
        }
        else {
            //console.log("Calling create League");
            //console.log(`In componentDidMount: props = ${JSON.stringify(this.props)}`);
            console.log(`LeagueManager: componentDidMount, creating new league because this.state.leagueId == null or 0`);
            this.createLeague();
        }
    }

    saveLeague = (league: LeagueSummaryViewModel) => {
        this.setState({ isLoading: true });
        if (this.state.league.leagueId) {
            // update existing league
            league = LeagueSummaryViewModel.mapLeagueViewModelToLeague(league) as LeagueSummaryViewModel;
            leagueService.updateLeague(league).then((response) => {
                if (!response.is_error) {
                    Notification.notifySuccess();
                }
                else {
                    console.log("ERROR updating league");
                    Notification.notifyError(response.error_content.errorMessage);
                }
                this.setState({ isLoading: false });
            });
        }
        else {
            // create new league
            leagueService.createLeague(this.state.league).then((response) => {
                if (!response.is_error) {
                    Notification.notifySuccess("New league created!");
                    let league = response.content;
                    this.getLeagueSummary(league.leagueId);
                    // we just created a new league, now redirect to roster tab to start adding members
                    this.props.history.push(`/leagues/roster/${league.leagueId}`);
                }
                else {
                    console.log("ERROR creating league");
                    Notification.notifyError(response.error_content.errorMessage);
                }
                this.setState({ isLoading: false });
            });
        }
    }

    deleteLeague = () => {
        leagueService.deleteLeague(this.state.leagueId).then((response) => {
            if (!response.is_error) {
                // navigate to Search Teams list
                this.props.history.push("/leagues/search");
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    createLeague = () => {
        //console.log(`In createLeague: this.props = ${JSON.stringify(this.props)}`);
        let member = this.props.appState.member;
        if (member) {
            let newLeague = new LeagueSummaryViewModel();
            newLeague.owner = this.props.appState.member as MemberNameViewModel;
            newLeague.owner.value = this.props.appState.member.memberId;
            newLeague.owner.label = `${this.props.appState.member.firstName} ${this.props.appState.member.lastName}`;
            newLeague.homeVenue = this.props.appState.member.homeVenue;
            newLeague.leagueId = 0;
            console.log(`set owner = ${newLeague.owner.firstName}`);
            this.setState({ league: newLeague, editIsEnabled: true });
            this.props.app.setState({ addTopOffset: true });
        }
    }

    getLeagueSummary = (id: number) => {
        if (!id) {
            //console.log("getLeagueSummary, id=0");
            this.setState({ leagueNotFound: true });
            return;
        }
        leagueService.getLeagueSummary(id).then((response) => {
            if (!response.is_error) {
                let league = response.content;
                let isOwner = league.owner && (league.owner.memberId == (this.props.appState.member && this.props.appState.member.memberId));
                let isCaptain = league.isCaptain;
                let isAdmin = this.props.appState.user && this.props.appState.user.isAdmin;
                let editIsEnabled = isOwner || isCaptain || isAdmin;
                league = LeagueSummaryViewModel.mapLeagueToViewModel(league) as LeagueSummaryViewModel;
                this.setState({ leagueId: id, league: league, editIsEnabled: editIsEnabled});
                this.props.app.setState({ addTopOffset: editIsEnabled });
            }
            else {
                console.log("Failed to get a league");
                this.setState({ leagueNotFound: true });
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
    }

    //updateMatchAvailability = (matchId, memberId, leagueId, value: Availability) => {
    //    matchService.UpdateMatchAvailability(matchId, memberId, leagueId, value, MatchDeclineAction.InviteAll, [])
    //        .then((response) => {
    //            if (response.is_not_found) {
    //                Notification.notifyError("Sorry, this match is no longer available.  For more info, look in the Matches tab for that League.");
    //            }
    //            else if (!response.is_error) {
    //                console.log(`Setting state avail = ${response.content}`);
    //            }
    //            else {
    //                console.log("Error updating match availability");
    //                Notification.notifyError(response.error_content.errorMessage);
    //            }
    //        })
    //        .catch((err) => {
    //            console.log("Error updating match availability");
    //            Notification.notifyError(err.message);
    //        });
    //}

    render() {
        if (this.state.leagueNotFound) {
            return <div>
                <h2>Manage {Branding.League}s</h2>
                <p><i className="fa fa-warning" />&ensp;No {Branding.League} found</p>
            </div>
        }
        if (this.state.league.leagueId == undefined) {
            //console.log(`LeagueManager render: returning null. league.leagueId = ${this.state.league.leagueId}`);
            return null;
        }
        //console.log(`LeagueManager render: passed null check.  league.leagueId = ${this.state.league.leagueId}`);
        return <div className="mtp-main-content">
            <h2>{this.state.league.name}</h2>
            {
                //<div className="form-check">
                //    <input className="form-check-input" onChange={this.handleInputChange} type="checkbox" name="editIsEnabled" checked={this.state.editIsEnabled} id="manageLeaguesCheckbox1" />
                //    <label className="form-check-label" htmlFor="manageLeaguesCheckbox1">
                //        {Branding.League} Edit Enabled
                //    </label>
                //</div>
            }

            <ul className="nav nav-tabs" id="myTab" role="tablist">
                <li hidden={this.state.league.leagueId == 0} className="nav-item tab-label">
                    <NavLink className="nav-link" id="summary-tab" to={'/leagues/summary/' + this.state.leagueId} role="tab" aria-controls="summary" aria-selected="true">Summary</NavLink>
                </li>
                <li hidden={this.state.league.leagueId == 0} className="nav-item tab-label">
                    <NavLink className="nav-link" id="roster-tab" to={'/leagues/roster/' + this.state.leagueId} role="tab" aria-controls="roster" aria-selected="false">Roster</NavLink>
                </li>
                <li hidden={this.state.league.leagueId == 0} className="nav-item tab-label">
                    <NavLink className="nav-link" id="schedule-tab" to={'/leagues/schedule/' + this.state.leagueId} role="tab" aria-controls="schedule" aria-selected="false">Matches</NavLink>
                </li>
                <li hidden={this.state.league.leagueId == 0} className="nav-item tab-label">
                    <NavLink className="nav-link" id="availability-tab" to={'/leagues/availability/' + this.state.leagueId} role="tab" aria-controls="availability" aria-selected="false">Availability</NavLink>
                </li>
                <li className="nav-item tab-label">
                    <NavLink className="nav-link" id="details-tab" to={'/leagues/details/' + this.state.leagueId} role="tab" aria-controls="details" aria-selected="false">Settings</NavLink>
                </li>
            </ul>
            <div className="row">
                <div id="ManageLeaguesRouteDiv" className="col-12">
                    <Route path='/leagues/summary/:id' render={(routeProps) => <LeagueSummary   {...routeProps} appState={this.props.appState} leagueState={this.state} app={this.props.app} />} />
                    <Route path='/leagues/details/:id' render={(routeProps) => <LeagueDetails   {...routeProps} appState={this.props.appState} leagueState={this.state} saveLeague={this.saveLeague} deleteLeague={this.deleteLeague} isLoading={this.state.isLoading} app={this.props.app} />} />
                    <Route path='/leagues/roster/:id'   render={(routeProps) => <LeagueRoster    {...routeProps} appState={this.props.appState} leagueState={this.state} app={this.props.app}/>} />
                    <Route path='/leagues/schedule/:id' render={(routeProps) => <LeagueSchedule  {...routeProps} appState={this.props.appState} leagueState={this.state} app={this.props.app} />} />
                    <Route path='/leagues/availability/:id' render={(routeProps) => <LeagueAvailability  {...routeProps} appState={this.props.appState} leagueState={this.state} updateAvailability={this.props.app.updateMatchAvailability} app={this.props.app} />} />
                </div>
            </div>
        </div>
    }
}