import * as React from "react";
import { Link } from 'react-router-dom';
import { AppStatePropsRoute } from "../../models/AppStateProps";
import LeagueSearchViewModel from "../../models/viewmodels/LeagueSearchViewModel";
import LeagueService from "../../services/LeagueService";
import LeagueSearchItem from "./LeagueSearchItem";
import Loader from "../Common/Loader";
import Branding from "../../branding/Branding";
import Notification from "../../utilities/Notification";

interface SearchLeaguesStates {
    leagues: LeagueSearchViewModel[];
    doneLoading: boolean;
    search: string;
}

let leagueService = new LeagueService();

export class SearchLeagues extends React.Component<AppStatePropsRoute, SearchLeaguesStates> {
    constructor(props) {
        super(props);
        this.state = {
            leagues: [],
            doneLoading: false,
            search: ""
        }
        this.getLeagues();
    }

    getLeagues = () => {
        leagueService.searchLeagues(this.state.search).then((response) => {
            if (!response.is_error) {
                this.setState({ leagues: response.content, doneLoading: true });
            }
            else {
                Notification.notifyError(response.error_content.errorMessage);
                this.setState({ doneLoading: true });
            }
        })
        .catch((err) => {
            console.log("Error searching leagues");
            this.setState({ doneLoading: true });
            Notification.notifyError(err.message);
        });
    }

    render() {
        let league = new LeagueSearchViewModel();
        league.name = "My League Name";
        return <div className="row mtp-main-content">
        <div className="col">
            <h2>Search {Branding.League}s</h2>
            <p>Find {Branding.League}s near you, at your age and skill level that you would like to join.  Some {Branding.League}s are open to anyone, some are invitation-only, and others are completely private (hidden from this list).  Click on the {Branding.League} for more information.</p>
            <div className="row justify-content-end">
                    <div hidden={this.props.appState.member == null} className="col-6 add-league-link">
                        <Link to={"/leagues/details/0"}><i className="fa fa-plus" />&nbsp;Create New {Branding.League}</Link>
                </div>
            </div>
            {!this.state.doneLoading &&
                <Loader />
            }

            {this.state.leagues.map(league =>
                <Link className="league-list" key={league.leagueId} to={"/leagues/summary/" + league.leagueId }>
                    <LeagueSearchItem key={league.leagueId} league={league} />
                </Link>
            )}

            </div>
        </div>
    }
}