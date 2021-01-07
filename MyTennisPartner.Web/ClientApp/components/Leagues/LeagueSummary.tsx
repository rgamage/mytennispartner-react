import * as React from "react";
import AppConstants from "../../models/app-constants";
import { LeagueTabProps } from "./LeagueManager";
import TimeFormatting from "../../utilities/TimeForrmatting";

interface LeagueSummaryStates {
}

export class LeagueSummary extends React.Component<LeagueTabProps, LeagueSummaryStates> {
    constructor(props: LeagueTabProps) {
        super(props);
        //console.log(`LeagueSummary constructor`);
    }

    render() {
        if (!this.props.leagueState.leagueId) {
            return null;
        }
        console.log(`LeagueSummary render: league is null? ${this.props.leagueState.league == null}, leagueId=${this.props.leagueState.leagueId}, league.leagueId = ${this.props.leagueState.league.leagueId}, league owner is null? ${this.props.leagueState.league.owner == null}`);
        return <div>
            <p>&ensp;</p>
            <p><strong>Owner:</strong>&ensp;{this.props.leagueState.league.owner.firstName} {this.props.leagueState.league.owner.lastName}</p>
            <ul>
                <li>Has {this.props.leagueState.league.regularMemberCount} regular members, {this.props.leagueState.league.subMemberCount} subs</li>
                <li>{AppConstants.playformats.options[this.props.leagueState.league.defaultFormat].label}, {this.props.leagueState.league.minimumRanking}-{this.props.leagueState.league.maximumRanking}, ages {this.props.leagueState.league.minimumAge}+</li>
                <li>Plays {AppConstants.frequency.options[this.props.leagueState.league.meetingFrequency].label}, {AppConstants.dayOfWeek.options[this.props.leagueState.league.meetingDay].label}s at {TimeFormatting.toDisplayTime(this.props.leagueState.league.matchStartTimeLocal)} on {this.props.leagueState.league.defaultNumberOfLines} courts.</li>
                <li>Plays at {this.props.leagueState.league.homeVenue && this.props.leagueState.league.homeVenue.name} in {this.props.leagueState.league.homeVenue && this.props.leagueState.league.homeVenue.venueAddress.city}</li>
            </ul>
            <p><strong>Notes: </strong>{this.props.leagueState.league.details}</p>
            <form></form>
        </div>
    }
}