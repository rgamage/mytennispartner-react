import * as React from 'react';
import MatchViewModel from "../../models/viewmodels/MatchViewModel";
import AppConstants from "../../models/app-constants";
import { NavLink, Link, Redirect } from 'react-router-dom';
import TimeFormatting from "../../utilities/TimeForrmatting";
import GpsMapping from "../../utilities/GpsMapping";
import LineService from "../../services/LineService";
import LineViewModel from "../../models/viewmodels/LineViewModel";
import MatchService from "../../services/MatchService";
import { Availability, MatchDeclineAction } from "../../models/viewmodels/Enums";
import Notification from "../../utilities/Notification";
import Branding from '../../branding/Branding';
import AvailabilityModal from '../Common/AvailabilityModal';
import PlayerViewModel from '../../models/viewmodels/PlayerViewModel';
import Loader from '../Common/Loader';
import ArrayHelper from "../../utilities/ArrayHelper";
import AvailabilityRequest from '../../models/viewmodels/AvailabilityRequest';
import { AppStateProps } from 'ClientApp/models/AppStateProps';

var lineService = new LineService();

interface LeagueMatchItemProps extends AppStateProps {
    match: MatchViewModel;
    editIsEnabled: boolean;
    view: MatchItemViews;
    getMatches: Function;
    updateMatchAvailability: (availabilityRequest: AvailabilityRequest, callback?: () => void) => void;
}

interface LeagueMatchItemStates {
    lines: LineViewModel[];
    showLines: boolean;
    players: PlayerViewModel[];
    doneLoading: boolean;
    availabilityRequest: AvailabilityRequest;
}

// enum used to determine how/where this view is used - for a specific member, or for all members
export enum MatchItemViews {
    MemberSpecific,
    AllMembers
}

export default class LeagueMatchItem extends React.Component<LeagueMatchItemProps, LeagueMatchItemStates> {
    constructor(props: LeagueMatchItemProps) {
        super(props);
        this.state = {
            lines: [] as LineViewModel[],
            showLines: false,
            players: [] as PlayerViewModel[],
            doneLoading: true,
            availabilityRequest: new AvailabilityRequest(
                props.match.matchId,
                props.match.leagueId,
                props.appState.member.memberId,
                Availability.Unknown,
                true
            )
        }
    }

    componentDidMount() {
        if (this.props.appState.member) {
            //this.fetchLines(this.props.match.matchId);
        }
    }

    componentDidUpdate() {
    }

    fetchLines = (matchId: number) => {
        // if we are showing all lines, then memberId param = 0, else = actual memberId to show only the member's line
        let memberId = 0;  // set to 0 to show all lines, or actual memberId to show only member's line
        if (!this.state.lines.length) {
            // only fetch lines if we have not yet done it
            this.getLines(matchId, memberId);
        }
    }

    getLines = (matchId: number, memberId: number) => {
        this.setState({ doneLoading: false });
        lineService.getLinesByMatch(matchId, memberId)
            .then((response) => {
                if (!response.is_error) {
                    this.setState({ lines: response.content.lines, players: response.content.players, doneLoading: true });
                }
                else {
                    this.setState({ doneLoading: true });
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
            .catch((err) => {
                this.setState({ doneLoading: true });
                console.log("Error getting lines");
                Notification.notifyError(err.message);
            });
    }

    toggleLineView = (event: any) => {
        let showLines = this.state.showLines;
        this.setState({ showLines: !showLines }, () => {
            this.fetchLines(this.props.match.matchId);
        });
    }

    onConfirmMatch = (event: any) => {
        event.preventDefault();
        let request = this.state.availabilityRequest;
        request.availability = Availability.Confirmed;
        this.props.updateMatchAvailability(request, () => this.getLines(request.matchId, request.memberId));
    }

    onCancelMatch = (event: any) => {
        event.preventDefault();
        let request = this.state.availabilityRequest;
        request.availability = Availability.Unavailable;
        console.log(`leagueMatchItem: request.matchId = ${request.matchId}`);
        this.props.updateMatchAvailability(request, () => this.getLines(request.matchId, request.memberId));
    }

    onUndecidedMatch = (event: any) => {
        // todo: we should remove ability for user to select 'undecided' from 'confirmed'
        // because then it will be unclear what action to take, etc.  If they are confirmed, 
        // then they can cancel but not change to 'undecided'
        event.preventDefault();
        let request = this.state.availabilityRequest;
        request.availability = Availability.Unknown;
        this.props.updateMatchAvailability(request, () => this.getLines(request.matchId, request.memberId));
    }

    render() {
        let startTime = TimeFormatting.utcDateToLocalMoment(this.props.match.startTime);
        let venueLetter = this.props.match.matchVenue && this.props.match.matchVenue.name.substr(0, 1) || "?";
        let address = this.props.match.matchVenue && this.props.match.matchVenue.venueAddress;
        let addressUrl = GpsMapping.getMapUrl(address);
        let modalId = `confirm-modal-${this.props.match.matchId}`;
        let modalIdTarget = `#${modalId}`;
        return <div>
            <AvailabilityModal
                title="Respond to Match"
                id={modalId}
                onConfirmMatch={this.onConfirmMatch}
                onCancelMatch={this.onCancelMatch}
                onUndecidedMatch={this.onUndecidedMatch}
            />
            <div className="match-list-item">
                <div className="league-match-item">
                    <div className="row">
                        <NavLink onClick={this.toggleLineView} className="col-10 align-self-center" to={"#leagueMatchDetails" + this.props.match.matchId} data-toggle="collapse" role="button" aria-expanded="false">
                            <div className="row">
                                <div hidden={true} className="col-2 col-sm-1 align-self-center">
                                    <div className="round-icon" style={{ backgroundColor: this.props.match.venueIconColor }}>{venueLetter}</div>
                                </div>
                                <div className={`match-date-block col-3 col-sm-2 align-self-center day-${startTime.weekday()}`} style={{ backgroundColor: this.props.match.venueIconColor }}>
                                    <div>{startTime.format("M/D")}</div>
                                    <div><big><strong>{startTime.format("ddd").toUpperCase()}</strong></big></div>
                                    <div>{startTime.format("h:mm a")}</div>
                                </div>
                                <div className={"col align-self-center" + (this.props.match.playerAvailability == Availability.Unavailable ? " strike-through" : "")} title={AppConstants.playformats.options[this.props.match.format].label}>
                                    <div className="row">
                                        <div className="col-sm">
                                            {this.props.match.matchVenue && this.props.match.matchVenue.name || "Unknown Venue"}
                                        </div>
                                        <div className="col-sm">
                                            {this.props.match.leagueName}
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </NavLink>
                        {this.props.view == MatchItemViews.MemberSpecific &&
                            <div className="col-2 align-self-center match-confirm">
                                <a href="#" data-toggle="modal" data-target={modalIdTarget}>
                                    <i className={"fa " + (this.props.match.playerAvailability == Availability.Confirmed ? "fa-check-circle thumbs-up" : this.props.match.playerAvailability == Availability.Unavailable ? "fa-times-circle thumbs-down" : "fa-check-circle-o thumbs-inactive")} title="Accept/Decline Match" />
                                </a>
                                <a href="#" data-toggle="modal" data-target={modalIdTarget} hidden={this.props.match.playerAvailability != Availability.Unknown}>
                                    <i className="fa fa-times-circle-o thumbs-inactive" title="Decline Match" />
                                </a>
                            </div>
                        }
                    </div>
                    <div className={`row collapse margin-top-10 ${this.state.showLines ? 'show' : ''}`} id={"leagueMatchDetails" + this.props.match.matchId}>
                        <div className="league-match-details col-12">
                            <div className="row lineup-heading">
                                <div className="col-12 league-match-link">
                                    <span><strong>Line-up</strong></span>
                                    <Link to={`/matches/details/${this.props.match.matchId}/${this.props.match.leagueId}`}>Edit</Link>
                                    <Link to={`/leagues/summary/${this.props.match.leagueId}`}>{Branding.League}</Link>
                                    <a href={addressUrl}>Map</a>
                                </div>
                            </div>
                            <div className="row">
                                <div className="col line-heading-container">
                                    {!this.state.doneLoading && <Loader />}
                                    {(this.state.lines.length == 0 ||
                                        (this.state.lines.length == 1 && this.state.players.length == 0)) &&
                                        <div className="center-col mt-3">
                                        <p><i className="fa fa-info-circle"></i> The line-up for this match has not been set.</p>
                                        </div>}
                                    {this.state.lines.map(line => {
                                        let players = this.state.players && this.state.players.filter(p => p.lineId == line.lineId);
                                        if (!ArrayHelper.isNonEmptyArray(players)) {
                                            players = [];
                                        }
                                        // show court number, or "Court Unassigned" if no number
                                        let courtNumberText = line.courtNumber ? line.courtNumber : "Unassigned";
                                        // if not reserved, show a ? after number
                                        courtNumberText = courtNumberText + ((!line.isReserved && (line.courtNumber != "") && (line.courtNumber != null)) ? "?" : "");
                                        let title = "";
                                        if (this.props.match.autoReserveCourts && line.courtNumber && !line.isReserved && !line.lineWarning) {
                                            title = "Court will be auto-reserved";
                                        }
                                        if (this.props.match.autoReserveCourts && line.courtNumber && !line.isReserved && line.lineWarning) {
                                            title = "Court will NOT be auto-reserved, no players in line-up can reserve court";
                                        }
                                         return <div key={line.lineId}>
                                             <div className="line-heading" title={title}>
                                                <span className="court-label">Court {courtNumberText}
                                                    {line.isReserved && <i className="fa fa-check" />}
                                                    {this.props.match.autoReserveCourts && !line.isReserved && this.props.match.venueHasReservationSystem && <i className="fa fa-clock-o" />}
                                                    {line.lineWarning && this.props.match.venueHasReservationSystem && <i className="fa fa-exclamation-triangle" />}
                                                 </span> 
                                            </div>
                                            <div className="row no-gutters">
                                                {(players.length > this.props.match.expectedPlayersPerLine && line.courtNumber != "") &&
                                                    <div className="col-12 highlight-error">
                                                        <small><i className="fa fa-warning" />&nbsp;Too many players on this court</small>
                                                    </div>
                                                }
                                                {players.map((player, index) => {
                                                    //let availViewModel = this.state.availabilities && this.state.availabilities.find(a => a.leagueMemberId == player.leagueMemberId);
                                                    let avail = (player.availability == null ? Availability.Unknown : player.availability);
                                                    return <div key={player.id} className="line-item col-6 col-sm-3 align-self-center">
                                                        <div className={"truncate " + (avail == Availability.Unavailable ? " strike-through" : "")}>
                                                            <i hidden={avail != Availability.Confirmed} className="fa fa-check" />
                                                            <span hidden={avail != Availability.Unknown}>? </span>
                                                            <span className={
                                                                //(player.memberId == this.props.appState.member.memberId ? "bold " : "")
                                                                (player.canReserveCourt ? "bold " : "")
                                                                + ((index > this.props.match.expectedPlayersPerLine - 1) && line.courtNumber != "" ? "highlight-error " : "")
                                                                + (player.isSubstitute ? "sub" : "")
                                                            }>
                                                                {player.firstName} {player.lastName}
                                                            </span>
                                                        </div>
                                                    </div>
                                                }
                                                )}
                                            </div>
                                        </div>
                                        }
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    }
}

