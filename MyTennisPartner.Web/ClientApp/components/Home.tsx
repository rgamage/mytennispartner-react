import * as React from 'react';
import { AppState, AppProps, LoginState } from './App';
import { AppStatePropsRoute } from '../models/AppStateProps';
import { Link } from 'react-router-dom';
import AppConstants from '../models/app-constants';
import { Redirect } from 'react-router-dom';
import Branding from "../branding/Branding";

interface HomeState {
    redirectOnLogin: boolean;
    redirectToProfile: boolean;
}

export class Home extends React.Component<AppStatePropsRoute, HomeState> {
    constructor(props: AppStatePropsRoute) {
        super(props);
        this.state = {
            redirectOnLogin: false,
            redirectToProfile: false
        }
    }

    componentDidUpdate(prevProps: AppStatePropsRoute) {
        // check if we just logged in.  If so, then redirect to proper page
        console.log(`checking login state... `);
        if (prevProps.appState.loginState != LoginState.LoggedIn &&
            this.props.appState.loginState == LoginState.LoggedIn) {
            console.log('setting state to re-direct to login page');
            this.setState({ redirectOnLogin: true });

            //if (this.props.appState.member == null || this.props.appState.member == undefined) {
            //    // user has logged in, but has no profile, so direct them to profile page
            //    console.log('setting state to re-direct to profile page');
            //    this.setState({ redirectToProfile: true });
            //}
            //else {
            //    console.log('setting state to re-direct to login page');
            //    this.setState({ redirectOnLogin: true });
            //}

        }
        if (this.props.appState.loginState == LoginState.MemberNotFound) {
            this.setState({ redirectToProfile: true });
        }
    }

    public render() {
        if (this.state.redirectOnLogin) {
            console.log('re-directing to login page');
            return <Redirect to={AppConstants.LandingPageAfterLogin} />
        }
        if (this.state.redirectToProfile) {
            console.log('re-directing to profile page');
            return <Redirect to="/profile" />
        }
        return <div>
            <div className="row splash hero">
                <div className="center col-12">
                    <h1>{Branding.AppName}</h1>
                    <p>{Branding.AppTagLine}</p>
                    {!(this.props.appState.user && this.props.appState.user.isLoggedIn) && <div className="row">
                        <div className="d-block d-sm-none col-2 offset-2">
                            <Link to="/signIn"><button className="btn btn-lg btn-primary">Sign In</button></Link>
                        </div>
                        <div className="d-block d-sm-none col-2 offset-2">
                            <Link to="/register"><button className="btn btn-lg btn-warning">Sign Up</button></Link>
                        </div>
                    </div>}
                </div>
            </div>
            <div className="row home-slides">
                <div className="col-xs-12 col-sm-12 col-md-4">
                    <img className="rounded" src="./images/slide1.png" />
                    <p>Create your own {Branding.league} with a few clicks and easily add players to your roster.  You can have 'regular' members and 'sub' members.  Your {Branding.league} plays by your rules.  Select the frequency of play, scoring rules, format, number of courts/lines, etc.  Players have access to your rules and guidelines, schedules, court assignments, and contact info for other members.</p>
                </div>
                <div className="col-xs-12 col-sm-12 col-md-4">
                    <img className="rounded" src="./images/slide2.png" />
                    <p>Matches can be scheduled in several ways, depending on your preference.  When a new match is scheduled, players can set their availability.  The system can auto-add players to the line-up as they respond, or the captain can manage the line-up manually.  Court assignments can be manual, in order of response, or randomized.  When a committed player needs to cancel, the system can help find a sub by inviting all other available and/or sub players in the roster.  First responder will be added to the match in their place.</p>
                </div>
                <div className="col-xs-12 col-sm-12 col-md-4">
                    <img className="rounded" src="./images/slide3.png" />
                    <p>Stay in touch with your {Branding.league} and match schedules wherever you go.  Quickly check upcoming matches, player availability, and {Branding.venue} location/directions.  Managing subs and cancellations is made easy with automated alerts and match invites when someone cannot play.</p>
                </div>
            </div>
        </div>;
    }
}
