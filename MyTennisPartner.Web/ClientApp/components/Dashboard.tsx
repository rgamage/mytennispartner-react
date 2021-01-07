import * as React from "react";
import { Link, Redirect, RouteComponentProps } from 'react-router-dom';
import { LoginState } from './App';
import AppConstants from "../models/app-constants";
import { AppStatePropsRoute } from "../models/AppStateProps";
import MatchService from "../services/MatchService";
import MatchViewModel from "../models/viewmodels/MatchViewModel";
import LeagueMatchItem, { MatchItemViews } from "./Leagues/LeagueMatchItem";
import LeagueService from "../services/LeagueService";
import LeagueSearchItem from "./Leagues/LeagueSearchItem";
import LeagueSearchViewModel from "../models/viewmodels/LeagueSearchViewModel";
import Loader from "./Common/Loader";
import Branding from "../branding/Branding";
import Notification from "../utilities/Notification";
import { Roles, Availability, MatchDeclineAction, UserPreferenceFlags } from "../models/viewmodels/Enums";
import TimeFormatting from "../utilities/TimeForrmatting";
import ArrayHelper from "../utilities/ArrayHelper";
import { MatchDecline } from "./Matches/MatchDecline";
import AvailabilityRequest from "../models/viewmodels/AvailabilityRequest";
import ConfirmationModal from "./Common/ConfirmationModal";
import AvailabilityModal from "./Common/AvailabilityModal";
import { RestResponse } from "../services/RestUtilities";
import PlayerViewModel from "../models/viewmodels/PlayerViewModel";
import MatchDetailsViewModel from "../models/viewmodels/MatchDetailsViewModel";
import MatchInfoGrid from "./Matches/MatchInfoGrid";
import MemberService from '../services/MemberService';

let matchService = new MatchService();
let leagueService = new LeagueService();
let memberService = new MemberService();
const numItemsToLoad = 3;
const showMatchInfoGrid = false;

interface DashboardStates {
    matches: MatchViewModel[],
    leagues: LeagueSearchViewModel[],
    itemsToLoad: number,
    hasMemberProfile: boolean,
    reloadData: boolean,
    matchDeclineId: number,
    prospectiveMatches: MatchDetailsViewModel,
}

export class Dashboard extends React.Component<AppStatePropsRoute, DashboardStates> {
    constructor(props: AppStatePropsRoute) {
        super(props);
        let hasMemberProfile = props.appState.member != null;
        this.state = {
            matches: [] as MatchViewModel[],
            leagues: [] as LeagueSearchViewModel[],
            itemsToLoad: hasMemberProfile ? numItemsToLoad : 0,
            hasMemberProfile: hasMemberProfile,
            reloadData: false,
            matchDeclineId: 0,
            prospectiveMatches: new MatchDetailsViewModel()
        }
    }

    componentDidMount() {
        if (this.state.itemsToLoad > 0) {
            this.fetchDashboardData();
        }
    }

    componentDidUpdate() {
        if (!this.state.hasMemberProfile) {
            let hasMemberProfile = this.props.appState.member != null;
            if (hasMemberProfile) {
                // we've recently found the member's profile, so update state and refresh our data
                this.setState({ hasMemberProfile: hasMemberProfile, reloadData: true });
            }
        }
        if (this.state.reloadData) {
            this.setState({ itemsToLoad: numItemsToLoad, reloadData: false });
            this.fetchDashboardData();
        }
    }

    fetchDashboardData = () => {
        let memberId = this.props.appState.member && this.props.appState.member.memberId || 0;
        if (memberId) {
            this.getMatches(this.props.appState.member.memberId);
            this.getLeagues(this.props.appState.member.memberId);
            this.getProspectiveMatches(this.props.appState.member.memberId);
        }
    }

    getMatches = (memberId: number) => {
        matchService.getMatchesByMember(memberId, false, true, 1, AppConstants.dashboardPageSize)
            .then((response) => {
                if (!response.is_error) {
                    this.setState({ matches: response.content, itemsToLoad: this.state.itemsToLoad - 1 });
                }
                else {
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
            .catch((err) => {
                console.log("Error getting matches");
                this.setState({ itemsToLoad: this.state.itemsToLoad - 1 });
                Notification.notifyError(err.message);
            });
    }

    getLeagues = (memberId: number) => {
        leagueService.getLeaguesByMember(memberId, AppConstants.dashboardPageSize).then((response) => {
            if (!response.is_error) {
                this.setState({ leagues: response.content, itemsToLoad: this.state.itemsToLoad - 1 });
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
            }
        })
            .catch((err) => {
                console.log("Error getting leagues");
                this.setState({ itemsToLoad: this.state.itemsToLoad - 1 });
            });
    }

    getProspectiveMatches = (memberId: number) => {
        matchService.getProspectiveMatches(memberId)
            .then((response) => {
                if (!response.is_error) {
                    this.setState({ prospectiveMatches: response.content, itemsToLoad: this.state.itemsToLoad - 1 });
                }
                else {
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
            .catch((err) => {
                console.log("Error getting prospective matches");
                this.setState({ itemsToLoad: this.state.itemsToLoad - 1 });
                Notification.notifyError(err.message);
            });
    }

    // this method is used to set possible future match availability, when user is not in the line-up
    updateAvailability = (leagueId: number, matchId: number, availability: Availability) => (event: any) => {
        let memberId = this.props.appState.member.memberId;
        //this.setState({ itemsToLoad: numItemsToLoad });
        matchService.UpdateMatchAvailability(matchId, memberId, leagueId, availability, MatchDeclineAction.DoNothing, [])
            .then((response) => {
                if (response.is_not_found) {
                    Notification.notifyError("Sorry, this match is no longer available.  For more info, look in the Matches tab for that League.");
                }
                else if (!response.is_error) {
                    this.fetchDashboardData();
                }
                else {
                    this.setState({ itemsToLoad: 0 });
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
            .catch((err) => {
                this.setState({ itemsToLoad: 0 });
                console.log("Error updating match availability");
                Notification.notifyError(err.message);
            });
    }

    onCloseDeclineModal = () => {
        this.props.app.onCloseDeclineModal(response => this.updateMatchList(response));
    }

    updateMatchAvailability = (request: AvailabilityRequest) => {
        console.log(`starting updateMatchAvailability...`);
        console.log(`Dashboard: request.matchId = ${request.matchId}`);
        this.props.app.updateMatchAvailability(request, (response) => this.updateMatchList(response));
    }

    updateMatchList(response: RestResponse<PlayerViewModel>) {
        console.log(`got response from updateMatchAvailability...`);
        if (!response.is_error) {
            // update dashboard ui to show latest availability choice from user
            let player = response.content;
            let matches = this.state.matches;
            let i = matches.findIndex(m => m.matchId == player.matchId);
            console.log(`updating match list... matchId = ${player.matchId} matches array len = ${matches.length} player = ${JSON.stringify(player)}`);
            matches[i].playerAvailability = player.availability;
            this.setState({ matches });
        }
    }

    // set parameters for avail request when user clicks the free/busy check/x
    onClickFreeBusy = (matchId: number, leagueId: number) => (event: any) => {
        event.preventDefault();
        let request = this.props.appState.availabilityRequest;
        request.matchId = matchId;
        request.leagueId = leagueId;
        request.action = MatchDeclineAction.DoNothing;
        request.memberId = this.props.appState.member.memberId;
        this.props.app.setState({ availabilityRequest: request });
    }

    onClickFree = (event: any) => {
        event.preventDefault();
        let request = this.props.appState.availabilityRequest;
        request.availability = Availability.Confirmed;
        this.updateAvailability(request.leagueId, request.matchId, Availability.Confirmed)(new Event("dummy"));
    }
    
    onClickBusy = (event: any) => {
        event.preventDefault();
        let request = this.props.appState.availabilityRequest;
        request.availability = Availability.Unavailable;
        this.updateAvailability(request.leagueId, request.matchId, Availability.Unavailable)(new Event("dummy"));
    }

    onClickUndecided = (event: any) => {
        event.preventDefault();
        let request = this.props.appState.availabilityRequest;
        request.availability = Availability.Unknown;
        this.updateAvailability(request.leagueId, request.matchId, Availability.Unknown)(new Event("dummy"));
    }

    onClickMatchInfoGrid = (leagueId: number) => (event: any) => {
        //console.log('clicked MatchInfoGrid');
        event.preventDefault();
        // when user clicks on the match info grid, we want to navigate to the avail grid for that league
        this.props.history.push(`leagues/availability/${leagueId}`)
    }

    onClickHideDeclinedMatches = (event: any) => {
        event.preventDefault();
        let member = this.props.appState.member;
        member.userPreferenceFlags &= (~UserPreferenceFlags.ShowDeclinedMatchesOnDashboard);
        this.props.app.setState({ member });
        memberService.updateMember(member);
    }

    onClickShowDeclinedMatches = (event: any) => {
        event.preventDefault();
        let member = this.props.appState.member;
        member.userPreferenceFlags |= UserPreferenceFlags.ShowDeclinedMatchesOnDashboard;
        this.props.app.setState({ member });
        memberService.updateMember(member);
    }

    render() {
        if (this.props.appState.loginState == LoginState.LoggedOut) { 
            return <Redirect to={AppConstants.LandingPageAfterLogout} />
        }
        if (this.state.itemsToLoad > 0 || this.props.appState.loginState == LoginState.FetchingData) {
            console.log(`Dashboard render- loading: loginState = ${this.props.appState.loginState}, items to load = ${this.state.itemsToLoad}`);
            return <Loader />
        }
        console.log(`Dashboard render - all items loaded`);
        let matchListTitle = "Committed Matches";
        let matchListSubtitle = this.state.matches.length == 0 ? "" : "You are in the line-up for these matches. Confirm that you will be there, or Decline to find a sub, by clicking on the Check or the X on the right side of the match.";
        let roles = this.props.appState.member && this.props.appState.member.memberRoles;
        if (roles && ArrayHelper.isNonEmptyArray(roles)) {
            if (roles.some(r => r.value == Roles.VenueAdmin)
                && !roles.some(r => r.value == Roles.Player)) {
                // user is a club manager, not a player, so customize the match list title for them
                matchListTitle = `Upcoming Matches at My ${Branding.Venue}`;
                matchListSubtitle = `These matches are coming up at your ${Branding.Venue}`;
            }
        }
        if (this.props.appState.showMatchDeclineModal) {
            return <div>
                <ConfirmationModal noCancel title={`Decline ${Branding.Match}`} onConfirm={this.onCloseDeclineModal} id="declineMatchModal">
                    {this.props.appState.availabilityRequest.postDeclineMessage}
                </ConfirmationModal>

                <MatchDecline app={this.props.app} appState={this.props.appState} leagueId={this.props.appState.availabilityRequest.leagueId} matchId={this.props.appState.availabilityRequest.matchId} onClose={this.props.app.onCloseMatchDecline} />
                </div>
        }
        return <div className="row mtp-main-content">
            <AvailabilityModal
                title="Respond to Match"
                id="freeBusyModal"
                onConfirmMatch={this.onClickFree}
                onCancelMatch={this.onClickBusy}
                onUndecidedMatch={this.onClickUndecided}
            />
            <div className="col dashboard">
                {
                    (this.props.appState.loginState == LoginState.MemberNotFound) &&
                    <Redirect to="/profile" />
                }
                {ArrayHelper.isNonEmptyArray(this.state.prospectiveMatches.matches) && <div>
                    <div className="dashboard-h2">Possible Match Dates</div>
                    <p>
                        <span className="mr-2">Let others know if you are available on these dates.</span>
                        <span className="mr-2">NOTE: If you have answered YES to any matches on this list, you may be called at any time to play!</span>
                        <span className="mr-2">Make sure to decline if your situation changes.</span>
                        {((this.props.appState.member.userPreferenceFlags & UserPreferenceFlags.ShowDeclinedMatchesOnDashboard) == UserPreferenceFlags.ShowDeclinedMatchesOnDashboard) && <a href="#" onClick={this.onClickHideDeclinedMatches}>
                            Hide my declined matches
                        </a>}
                        {(!(this.props.appState.member.userPreferenceFlags & UserPreferenceFlags.ShowDeclinedMatchesOnDashboard)) && <a href="#" onClick={this.onClickShowDeclinedMatches}>
                            Show my declined matches
                        </a>}
                    </p>
                    <div className="match-avail-list">
                        {
                            this.state.prospectiveMatches.matches.map(match => {
                                let lines = this.state.prospectiveMatches.lines.filter(l => l.matchId == match.matchId);
                                let players = this.state.prospectiveMatches.players.filter(p => p.matchId == match.matchId);
                                if (match.availability == Availability.Unknown) {
                                    return <div key={match.matchId} className="match-avail-item pointer">
                                        <div className="row">
                                            <div className="col-12 col-sm-5 align-items-center">
                                                <span className="mr-1">{TimeFormatting.localTimeStringToDisplayString(match.startTimeLocal)}, </span>
                                                <span className="truncate2 px-0">{match.leagueName}</span>
                                            </div>
                                            <div className="col-7 col-sm-3">
                                                Are you available?
                                            </div>
                                            <div className="col-2 col-sm-2 align-self-center center-col px-0">
                                                <button className="btn btn-success btn-slim" onClick={this.updateAvailability(match.leagueId, match.matchId, Availability.Confirmed)}>
                                                    Yes
                                                </button>
                                            </div>
                                            <div className="col-2 col-sm-2 align-self-center center-col" onClick={this.updateAvailability(match.leagueId, match.matchId, Availability.Unavailable)}>
                                                <button className="btn btn-danger btn-slim">
                                                    No
                                                </button> 
                                            </div>
                                            <div className="col-12">
                                                {match.venueName}
                                            </div>
                                        </div>
                                        {showMatchInfoGrid && <div onClick={this.onClickMatchInfoGrid(match.leagueId)}>
                                            <MatchInfoGrid match={match} lines={lines} players={players} />
                                        </div>}
                                    </div>
                                }
                                else {
                                    if (!(this.props.appState.member.userPreferenceFlags & UserPreferenceFlags.ShowDeclinedMatchesOnDashboard) && match.availability == Availability.Unavailable) return null;
                                    return <div key={match.matchId} className="match-avail-item pointer">
                                        <div className={`row ${match.availability == Availability.Unavailable ? "strike-through-grey" : ""}`}>
                                            <div className="col-7 col-sm-8 align-items-center text-decoration-inherit">
                                                <span className="mr-1">{TimeFormatting.localTimeStringToDisplayString(match.startTimeLocal)}, </span>
                                                <span className="truncate2 px-0">{match.leagueName}</span>
                                            </div>
                                            {match.availability == Availability.Confirmed && <React.Fragment>
                                                <div className="col-4 col-sm-4 align-self-center center-col">
                                                    <a href="#" data-toggle="modal" data-target="#freeBusyModal" onClick={this.onClickFreeBusy(match.matchId, match.leagueId)} title="click to edit your availability">
                                                        <big><i className="fa fa-check" /></big>
                                                    </a>
                                                </div>
                                            </React.Fragment>}
                                            {match.availability == Availability.Unavailable && <React.Fragment>
                                                <div className="col-4 col-sm-4 align-self-center center-col">
                                                    <a href="#" data-toggle="modal" data-target="#freeBusyModal" onClick={this.onClickFreeBusy(match.matchId, match.leagueId)} title="click to edit your availability">
                                                        <big><i className="fa fa-times" /></big>
                                                    </a>
                                                </div>
                                            </React.Fragment>}
                                            <div className="col-12">
                                                {match.venueName}
                                            </div>
                                        </div>
                                        {showMatchInfoGrid && <div onClick={this.onClickMatchInfoGrid(match.leagueId)}>
                                            <MatchInfoGrid match={match} lines={lines} players={players} />
                                        </div>}
                                    </div>
                                }
                            })
                        }
                        {this.state.prospectiveMatches.matches.length == 0 &&
                            <div>
                                No upcoming match opportunities
                    </div>
                        }
                    </div>
                </div>}
                <div className="dashboard-h2">{matchListTitle}</div>
                <p>{matchListSubtitle}</p>
            <div className="match-list">
                {
                    this.state.matches.map(match =>
                        <div key={match.matchId}>
                                <LeagueMatchItem match={match} editIsEnabled={false} app={this.props.app} appState={this.props.appState} view={MatchItemViews.MemberSpecific} getMatches={this.getMatches} updateMatchAvailability={this.updateMatchAvailability} />
                        </div>
                    )
                }
                {this.state.matches.length == 0 &&
                    <div>
                        No upcoming {matchListTitle}
                    </div>
                }
            </div>
            <div className="dashboard-h2">My {Branding.League}s</div>
            <div className="league-list">
                <div className="row justify-content-end">
                    <div hidden={this.props.appState.member == null} className="col-6 add-league-link">
                        <Link to={"/leagues/details/0"}><i className="fa fa-plus" />&nbsp;Create New {Branding.League}</Link>
                    </div>
                </div>
                {this.state.leagues.map(league =>
                    <Link key={league.leagueId} to={"/leagues/summary/" + league.leagueId}>
                        <LeagueSearchItem key={league.leagueId} league={league} />
                    </Link>
                )}
                {this.state.leagues.length == 0 &&
                        <div>
                        <span>
                            You are not currently a member of any {Branding.League}s. &nbsp;
                        </span>
                        {this.props.appState.member && <span>
                            Now that you're registered, you can ask a {Branding.League} Captain to add you to their {Branding.League}.  Once you are on the roster, your {Branding.League} will show up here.  Public {Branding.League}s can be viewed on the <Link to="/leagues/search">{Branding.League}s</Link> page.
                        </span>
                        }
                    </div>
                }
            </div>
            </div>
        </div>
    }
}