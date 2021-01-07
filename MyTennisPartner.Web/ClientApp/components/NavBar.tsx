import * as React from 'react';
import { NavLink } from 'react-router-dom';
import { App, LoginState, AppState } from './App';
import { NavLogin } from './NavLogin';
import Branding from "../branding/Branding";
import { AppStateProps } from '../models/AppStateProps';


interface NavProps extends AppStateProps {
    signOut: () => void;
}

export class NavBar extends React.Component<NavProps, {}> {
    constructor(props) {
        super(props);
    }

    toggleMobileMenu = () => {
        this.props.app.setState({ showMobileMenu: !this.props.appState.showMobileMenu });
    }

    public render() {
        return <nav className='navbar navbar-expand-md fixed-top navbar-dark justify-content-start'>
            <button className="navbar-toggler navbar-toggler-right" onClick={this.toggleMobileMenu} type="button" aria-controls="navbarSupportedContent" aria-expanded={this.props.appState.showMobileMenu} aria-label="Toggle navigation">
                <span className="navbar-toggler-icon"></span>
            </button>
            <a className="navbar-brand" href="/"><img className="site-logo ml-4-sm ml-2" src={Branding.siteLogoImage} width="40" />{Branding.AppName}</a>
            <div className={"collapse navbar-collapse " + (this.props.appState.showMobileMenu ? "show" : "")} id="navbarSupportedContent">
                <ul className='navbar-nav mr-auto'>
                    <li className='nav-item'>
                        <NavLink className='nav-link' onClick={this.toggleMobileMenu} to={'/home'} exact activeClassName='active'>
                            <i className='fa fa-home'></i> Home
                        </NavLink>
                    </li>
                    {this.props.appState.loginState == LoginState.LoggedIn &&
                        <li className='nav-item'>
                            <NavLink className='nav-link' to={'/dashboard'} onClick={this.toggleMobileMenu} activeClassName='active'>
                                <img className="navbar-image" src="images/tennis-ball_32_white.png" /> {Branding.DashboardName}
                            </NavLink>
                        </li>
                    }
                    {this.props.appState.loginState == LoginState.LoggedIn &&
                        <li className='nav-item'>
                            <NavLink className='nav-link' to={'/leagues/search'} onClick={this.toggleMobileMenu} activeClassName='active'>
                                <i className='fa fa-th-list'></i> {Branding.League}s
                            </NavLink>
                        </li>
                    }
                    {this.props.appState.loginState == LoginState.LoggedIn &&
                        <li className='nav-item'>
                            <NavLink className='nav-link' to={'/players'} onClick={this.toggleMobileMenu} activeClassName='active'>
                                <i className='fa fa-users'></i> Players
                            </NavLink>
                        </li>
                    }
                </ul>
                <NavLogin app={this.props.app} signOut={this.props.signOut} appState={this.props.appState} />
            </div>
        </nav>;
    }
}
