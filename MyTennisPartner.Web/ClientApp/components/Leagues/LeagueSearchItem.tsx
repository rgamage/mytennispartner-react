import * as React from 'react';
import LeagueSearchViewModel from "../../models/viewmodels/LeagueSearchViewModel";
import AppConstants from "../../models/app-constants";
import TimeFormatting from "../../utilities/TimeForrmatting";
import LeagueSummaryViewModel from '../../models/viewmodels/LeagueSummaryViewModel';

interface LeagueSearchItemProps {
    league: LeagueSearchViewModel;
}

export default function LeagueSearchItem(props: LeagueSearchItemProps) {

    let leagueLetter = props.league && props.league.name.substr(0, 1) || "?";
    let league = LeagueSummaryViewModel.mapLeagueToViewModel(props.league);
    let matchStartTimeLocal = league.matchStartTimeLocal;

    return <div className="league-list-item-wrapper">
    <div className="league-list-item">
        <div className="row">
            <div className="col-2">
                    <div className="round-icon square" style={{ backgroundColor: props.league.venueIconColor }}>{leagueLetter}</div>
            </div>
            <div className="col-10 align-self-center">
                <div className="row">
                    <div className="col-sm align-self-center">
                        <big>
                        {props.league.name}
                        {props.league.isCaptain &&
                            <span className="member-icon">
                                <i className="fa fa-star" title="{Branding.League} Captain" />
                            </span>
                            }
                        </big>
                    </div>
                    <div className="col-sm align-self-center">
                        {props.league.venueName || "Unknown Venue"}
                    </div>
                    <div className="col-sm align-self-center">
                            {AppConstants.Frequency.options[props.league.meetingFrequency].label}, {AppConstants.dayOfWeek.options[props.league.meetingDay].label}s at {TimeFormatting.toDisplayTime(matchStartTimeLocal)}
                    </div>
                </div>
            </div>
        </div>
        </div>
    </div>;
}