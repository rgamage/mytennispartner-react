import * as React from "react";
import { NavLink, Redirect } from 'react-router-dom';
import AuthService from "../services/Auth";
import AppStateManager from '../states/AppStateManager';
import AppConstants from "../models/app-constants";

let authService = new AuthService();
let appStateManager = new AppStateManager();

export interface Props {
    hidden?: boolean;
    signOut: () => void;
}

interface State {
    hasSignedOut: boolean;
}

export default class NavBarMobile extends React.Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            hasSignedOut: false
        }
    }

    componentDidMount() {
    }

    goBack = (event: React.MouseEvent<HTMLElement>) => {
        event.preventDefault();
        window.history.back();
    }

    signOut = (event: React.MouseEvent<HTMLAnchorElement>) => {
        this.props.signOut();
        this.setState({ hasSignedOut: true });
    }

    render() {
        // we've just signed out, so re-direct to appropriate landing page
        if (this.state.hasSignedOut
        ) {
            this.setState({ hasSignedOut: false });
            return <Redirect to={AppConstants.LandingPageAfterLogout} />
        }
        if (this.props.hidden) return null;
        return <nav id="navbar-mobile-menu" className="navbar navbar-light py-0 fixed-top fixed-top-2 d-block d-sm-none">
            <ul className="navbar-nav navbar-horizontal d-flex justify-content-center">
                <li className="nav-item py-0 mr-auto">
                    <NavLink className="nav-link" onClick={this.goBack} to="#"><i className="fa fa-arrow-left" /></NavLink>
                </li>
                <li className="nav-item py-0">
                <NavLink className="nav-link" to="/dashboard"><img className="navbar-image mobile" src="images/tennis-ball_32_black.png" /><span className="sr-only">(current)</span></NavLink>
                </li>
                <li className="nav-item py-0 ">
                    <NavLink className="nav-link" to="/leagues/search"><i className="fa fa-th-large" /></NavLink>
                </li>
                <li className="nav-item py-0 ">
                    <NavLink className="nav-link" to="/players"><i className="fa fa-users" /></NavLink>
                </li>
                <li className="nav-item py-0 dropdown ml-auto">
                    <a className="nav-link dropdown-toggle" href="#" id="navbarDropdown" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                        <i className="fa fa-user" /> 
                    </a>
                    <div className="dropdown-menu" aria-labelledby="navbarDropdown">
                        <NavLink className="nav-link dropdown-item" to="/account">My Account</NavLink>
                        <NavLink className="nav-link dropdown-item" to="/profile">My Profile</NavLink>
                        <NavLink onClick={this.signOut} className="nav-link dropdown-item" to="#">Sign Out</NavLink>
                    </div>
                </li>
            </ul>
        </nav>
    }
}