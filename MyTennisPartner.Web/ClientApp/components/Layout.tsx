import * as React from 'react';
import { NavBar } from './NavBar';
import { Home } from './Home';
import { Route, Switch } from 'react-router-dom';
import { LoginState } from './App';
import { SignIn, Register, ForgotPassword, ResetPassword, ExternalSignIn, SetToken } from './Auth';
import { ErrorPage } from './Error';
import { MyProfile } from './MyProfile';
import { MyAccount } from './MyAccount';
import { Dashboard } from './Dashboard';
import { Players } from './Players/Players';
import { SearchLeagues } from './Leagues/SearchLeagues';
import { ManageLeagues } from './Leagues/LeagueManager';
import { ToastContainer, ToastStore } from 'react-toasts';
import ManageMatches from "./Matches/MatchManager";
import RespondToMatch from './Matches/RespondToMatch';
import NavBarMobile from './NavBarMobile';
import { CourtReservationsGuide } from './Guides/CourtReservationsGuide';
import { AppStateProps } from '../models/AppStateProps';

interface AppProps extends AppStateProps {
    signOut: () => void;
}

// example of a simpler function syntax (instead a class) for components that don't need their own state
export default function Layout(props: AppProps) {
    let showMobileNav = props.appState.loginState == LoginState.LoggedIn;
    return <div className={`container-fluid ${props.appState.loginState == LoginState.LoggedIn ? "logged-in" : ""} ${props.appState.addTopOffset ? "top-offset" : ""}`}>
        <ToastContainer store={ToastStore} position={ToastContainer.POSITION.TOP_CENTER} />
        <NavBar app={props.app} appState={props.appState} signOut={props.signOut} />
            {showMobileNav && <NavBarMobile signOut={props.signOut} />}
            <div className='row'>
                <div id='LayoutRouteDiv' className='col-12'>
                    <Switch>
                        <Route path='/signin'                             render={(routeProps) => <SignIn          {...routeProps} app={props.app} appState={props.appState}/>} />
                        <Route path='/profile'                            render={(routeProps) => <MyProfile       {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/account'                            render={(routeProps) => <MyAccount       {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/dashboard'                          render={(routeProps) => <Dashboard       {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/forgot'                             render={(routeProps) => <ForgotPassword  {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/leagues/search'                     render={(routeProps) => <SearchLeagues   {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/leagues/summary/:leagueId'          render={(routeProps) => <ManageLeagues   {...routeProps} app={props.app} appState={props.appState} leagueId={routeProps.match.params.leagueId} />} />
                        <Route path='/leagues/details/:leagueId'          render={(routeProps) => <ManageLeagues   {...routeProps} app={props.app} appState={props.appState} leagueId={routeProps.match.params.leagueId} />} />
                        <Route path='/leagues/roster/:leagueId'           render={(routeProps) => <ManageLeagues   {...routeProps} app={props.app} appState={props.appState} leagueId={routeProps.match.params.leagueId} />} />
                        <Route path='/leagues/schedule/:leagueId'         render={(routeProps) => <ManageLeagues   {...routeProps} app={props.app} appState={props.appState} leagueId={routeProps.match.params.leagueId} />} />
                        <Route path='/leagues/availability/:leagueId'     render={(routeProps) => <ManageLeagues   {...routeProps} app={props.app} appState={props.appState} leagueId={routeProps.match.params.leagueId} />} />
                        <Route path='/matches/details/:matchId/:leagueId' render={(routeProps) => <ManageMatches   {...routeProps} app={props.app} appState={props.appState} matchId={routeProps.match.params.matchId} leagueId={routeProps.match.params.leagueId} />} />
                        <Route path='/matches/respond'                    render={(routeProps) => <RespondToMatch  {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/players'                            render={(routeProps) => <Players         {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/setToken'                           render={(routeProps) => <SetToken        {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/register'                           render={(routeProps) => <Register        {...routeProps} />} />
                        <Route path='/reset'                              render={(routeProps) => <ResetPassword   {...routeProps} />} />
                        <Route path='/externalSignIn'                     render={(routeProps) => <ExternalSignIn  {...routeProps} />} />
                        <Route path='/reservations'                       render={(routeProps) => <CourtReservationsGuide {...routeProps} app={props.app} appState={props.appState} />} /> 
                        <Route path='/'                                   render={(routeProps) => <Home            {...routeProps} app={props.app} appState={props.appState} />} />
                        <Route path='/error/:code?'                       component={ErrorPage} />
                    </Switch>
                </div>
            </div>
        </div>;
}


