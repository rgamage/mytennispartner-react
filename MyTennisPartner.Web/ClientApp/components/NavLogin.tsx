import * as React from 'react';
import { NavLink, Redirect } from 'react-router-dom';
import AppConstants from '../models/app-constants';
import { AppStateProps } from '../models/AppStateProps';

interface NavLoginProps extends AppStateProps {
    signOut: () => void;
}

interface NavLoginState {
    hasSignedOut: boolean;
}

export class NavLogin extends React.Component<NavLoginProps, NavLoginState> {
    constructor(props) {
        super(props);
        this.state = {
            hasSignedOut: false
        }
    }

    signOut = (event: React.MouseEvent<HTMLDivElement>) => {
        this.props.signOut();
        this.setState({ hasSignedOut: true });
    }

    toggleMobileMenu = () => {
        this.props.app.setState({ showMobileMenu: !this.props.appState.showMobileMenu });
    }

    render() { 
        // we've just signed out, so re-direct to appropriate landing page
        if (this.state.hasSignedOut
        ) {
            this.setState({ hasSignedOut: false });
            return <Redirect to={AppConstants.LandingPageAfterLogout} />
        }
        if (this.props.appState.user && this.props.appState.user.isLoggedIn) {
            return <ul className="navbar-nav">
                <li className="nav-item dropdown">
                    <div className="nav-link dropdown-toggle pointer" id="navbarDropdownMenuLink" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                        Hello, {this.props.appState.user.firstName}
                    </div>
                    <div className="dropdown-menu" aria-labelledby="navbarDropdownMenuLink">
                        <NavLink className="dropdown-item nav-link" onClick={this.toggleMobileMenu} to="/profile">My Profile</NavLink>
                        <NavLink className="dropdown-item nav-link" onClick={this.toggleMobileMenu} to="/account">My Account</NavLink>
                        <div onClick={this.signOut} className="dropdown-item pointer nav-link" role="link">Sign Out</div>
                    </div>
                </li>
            </ul>
        }

        return <ul className='navbar-nav'>
            <li className="nav-item">
                <NavLink className="nav-link" to={'/signin'} activeClassName='active'>
                    <button className="btn btn-outline-light"><i className="fa fa-sign-in"></i> Sign In </button>
                </NavLink>
            </li>
            <li>
                <NavLink className="nav-link" to={'/register'} activeClassName='active'>
                    <button className="btn btn-warning"> Sign Up </button>
                </NavLink>
            </li>
        </ul>
    }
}