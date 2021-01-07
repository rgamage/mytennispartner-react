import * as React from "react";
import { Link } from 'react-router-dom';
import AppConstants from "../../models/app-constants";
import { LeagueTabProps } from "./LeagueManager";
import MatchViewModel from "../../models/viewmodels/MatchViewModel";
import MatchService from "../../services/MatchService";
import Loader from "../Common/Loader";
import LeagueMatchItem, { MatchItemViews } from "./LeagueMatchItem";
import DatePicker from 'react-datepicker';
import * as moment from "moment";
import Notification from "../../utilities/Notification";
import ArrayHelper from "../../utilities/ArrayHelper";
import Branding from "../../branding/Branding";

let matchService = new MatchService();

interface LeagueScheduleStates {
    matches: MatchViewModel[];
    showFuture: boolean;
    page: number;
    doneLoading: boolean;
    showDetails: boolean;
    startTime: moment.Moment;
}

export class LeagueSchedule extends React.Component<LeagueTabProps, LeagueScheduleStates> {
    constructor(props: LeagueTabProps) {
        super(props);
        this.state = {
            matches: [] as MatchViewModel[],
            showFuture: true,
            page: 1,
            doneLoading: false,
            showDetails: false
        } as LeagueScheduleStates
    }

    componentDidMount() {
        this.getMatches();
    }

    getMatches = () => {
        let leagueId = this.props.leagueState.leagueId;
        //console.log("called getMatches with showFuture = " + this.state.showFuture.toString());
        matchService.getMatchesByLeague(leagueId, !this.state.showFuture, this.state.showFuture, this.state.page, AppConstants.pageSize)
            .then((response) => {
                if (!response.is_error) {
                    this.setState({ matches: response.content, doneLoading: true });
                }
                else {
                    if (response.is_not_found) {
                        this.setState({ matches: [], doneLoading: true });
                        return;
                    }
                    this.setState({ doneLoading: true });
                    Notification.notifyError(response.error_content.errorMessage);
                }
            })
            .catch((err) => {
                console.log("Error getting matches");
                this.setState({ doneLoading: true });
                Notification.notifyError(err.message);
            });
    }

    showPast = (event) => {
        event.preventDefault();
        this.setState({ showFuture: false },
            () => this.getMatches()
        );
    }

    showFuture = (event) => {
        event.preventDefault();
        this.setState({ showFuture: true },
            () => this.getMatches()
        );
    }

    showDetails = (event) => {
        this.setState({ showDetails: true });
    }

    handleCalendarChange = (date: moment.Moment) => {
        this.setState({ startTime: date });
    }

    render() {
        if (!this.state.doneLoading) {
            return <Loader />
        }

        return <div className="league-schedule">
            <div>
                <div className="row justify-content-between">
                    <div className="col-8">
                        <ul className="nav nav-pills" role="tablist">
                            <li className="nav-item">
                                <a href="/" className={"nav-link mtp-pill-link-first " + (this.state.showFuture ? "active" : "")} onClick={this.showFuture} id="regular-tab" role="tab" aria-controls="regular" aria-selected="true">Upcoming</a>
                            </li>
                            <li className="nav-item">
                                <a href="/" className={"nav-link mtp-pill-link-last " + (this.state.showFuture ? "" : "active")} onClick={this.showPast} id="substitute-tab" role="tab" aria-controls="substitute" aria-selected="false">History</a>
                            </li>
                        </ul>
                    </div>
                    {this.props.leagueState.editIsEnabled && <Link to={`/matches/details/0/${this.props.leagueState.leagueId}`} className="col-3 add-match-link">
                        <span>+</span>
                        <span className="d-none d-sm-inline-block">Add Match</span>
                    </Link>
                    }
                </div>
                <br />
                <div className="match-list">
                    <div hidden={ArrayHelper.isNonEmptyArray(this.state.matches)}>
                        No matches available for this {Branding.League}.
                    </div>
                    {
                        this.state.matches.map(match =>
                            <div key={match.matchId} onClick={this.showDetails}>
                                <LeagueMatchItem match={match} editIsEnabled={this.props.leagueState.editIsEnabled} appState={this.props.appState} app={this.props.app} view={MatchItemViews.AllMembers} getMatches={this.getMatches} updateMatchAvailability={null} />
                            </div>
                        )
                    }
                </div>
            </div>
        </div>
    }
}