import * as React from 'react';
import Layout from './Layout';
import AppStateManager from '../states/AppStateManager';
import AuthService from "../services/Auth";
import { UserInfo } from '../models//viewmodels/UserViewModels';
import MemberViewModel, { MemberNameViewModel } from '../models/viewmodels/MemberViewModel';
import MemberService from '../services/MemberService';
import Notification from "../utilities/Notification";
import { RouteComponentProps } from 'react-router';
import AppConstants from '../models/app-constants';
import AvailabilityRequest from '../models/viewmodels/AvailabilityRequest';
import { Availability, MatchDeclineAction } from '../models/viewmodels/Enums';
import ArrayHelper from '../utilities/ArrayHelper';
import MatchService from "../services/MatchService";
import * as $ from 'jquery';
import { RestResponse } from '../services/RestUtilities';
import PlayerViewModel from '../models/viewmodels/PlayerViewModel';
import Branding from '../branding/Branding';

let authService = new AuthService();
let appStateManager = new AppStateManager();
let memberService = new MemberService();
let matchService = new MatchService();

export enum LoginState {
    LoggedOut,
    FetchingData,
    MemberNotFound,
    LoggedIn
}

    export interface AppState {
        countValue: number;
        hasUpdatedUserInfo: boolean;
        member?: MemberViewModel;
        user?: UserInfo;
        showMobileMenu: boolean;
        loginState: LoginState
        isMobileWindowSize: boolean;
        addTopOffset: boolean;  // used to offset top of content, to allow for fixed-top items like SaveDeleteBar
        availabilityRequest: AvailabilityRequest;
        showMatchDeclineModal: boolean;
    }

export interface AppProps extends RouteComponentProps<{}> {
        debug?: boolean;
    }
    
    export class App extends React.Component<AppProps, AppState> {
        constructor(props: AppProps) {
            super(props);
            this.state = {
                countValue: 1,
                hasUpdatedUserInfo: true,
                showMobileMenu: false,
                loginState: LoginState.LoggedOut,
                isMobileWindowSize: false,
                addTopOffset: false,
                availabilityRequest: new AvailabilityRequest(0, 0, 0, Availability.Unknown, false),
                showMatchDeclineModal: false
            } as AppState;
            let encodedPath = encodeURIComponent(props.location.pathname + props.location.search);
            localStorage.setItem("lastPath", encodedPath);
        }

        signOut = () => {
            authService.signOut();
            this.setState({
                user: null,
                member: null,
                loginState: LoginState.LoggedOut
            },
                () => {
                    console.log('setting enableAutoLogin to false');
                    localStorage.setItem("enableAutoLogin", "false");
                }
            )
        }

        checkForUpdatedUser() {
            if (this.state.hasUpdatedUserInfo) {
                this.setState({ hasUpdatedUserInfo: false });
                this.getUserInfo();
            }
        }

        componentDidMount() {
            this.checkForUpdatedUser();
            window.addEventListener("resize", this.resize.bind(this));
            this.resize();
        }

        resize() {
            let currentisMobileWindowSize = (window.innerWidth <= AppConstants.MobileWindowSize);
            if (currentisMobileWindowSize !== this.state.isMobileWindowSize) {
                this.setState({ isMobileWindowSize: currentisMobileWindowSize });
            }
        }

        componentDidUpdate() {
            this.checkForUpdatedUser();
        }

        setAppState = (updater: any, callback?: Function) => {
            this.setState(updater, () => {
                if (this.props.debug) {
                    //console.log('setAppState', JSON.stringify(this.state));
                }
                //this.checkForUpdatedUser();
                if (callback) {
                    callback();
                }
            });
        }

        getUserInfo() {
            this.setState({ loginState: LoginState.FetchingData });
            console.log(`login: fetching data`);
            authService.getUserInfo().then((user) => {
                console.log(`login: received data`);
                let u = user as UserInfo;
                if (u) {
                    console.log(`login: got user info`);
                    this.setState({ loginState: LoginState.FetchingData });
                    this.setState(appStateManager.setUserInfo(u));
                    memberService.getMemberInfo(u.userId).then((response) => {
                        console.log(`login: got member info`);
                        if (!response.is_error) {
                            if (response.content) {
                                console.log(`login: member is logged in - setting state to logged in`);
                                this.setState({ loginState: LoginState.LoggedIn });
                            }
                            else {
                                this.setState({ loginState: LoginState.MemberNotFound });
                            }
                            this.setState(appStateManager.setMemberInfo(response.content));
                        }
                        else if (response.is_not_found) {
                            console.log(`app component: setting member to null...`);
                            this.setState({ member: null, loginState: LoginState.MemberNotFound });
                        }
                        else {
                            this.setState({ member: null, loginState: LoginState.MemberNotFound });
                            Notification.notifyError(response.error_content.errorMessage);
                        }
                    });
                }
                else {
                    this.setState({ loginState: LoginState.LoggedOut });
                    console.log(`app component - no user found, setting user = null, member = null`);
                    this.setState({ user: null, member: null });
                }
            });
        }

        onCloseDeclineModal = (callback?: (response: RestResponse<PlayerViewModel>) => void) => {
            this.updateMatchAvailability(this.state.availabilityRequest, callback);
        }

        onCloseMatchDecline = (action: MatchDeclineAction, members: MemberNameViewModel[]) => {
            console.log(`onCloseMatchDecline`);
            let request = this.state.availabilityRequest;
            request.action = action;
            request.members = members;
            this.setState({ availabilityRequest: request });
            // show modal to explain what happens next
            $("#declineMatchModal").modal();
        }

        updateMatchAvailability = (request: AvailabilityRequest, callback?: (response: RestResponse<PlayerViewModel>) => void) => {
            if (request.availability == Availability.Unavailable && !this.state.showMatchDeclineModal && request.isInLineup) {
                // user is declining match in which they are in the line-up, so need to handle
                // different choices as to filling their spot, before updating avail on the server
                // save request for later, and show match decline component
                console.log(`App: request.matchId = ${request.matchId}`);
                this.setState({ availabilityRequest: request, showMatchDeclineModal: true });
                return;
            }
            // hide match decline component, we are done with it
            this.setState({ showMatchDeclineModal: false });
            let inviteMemberIds = [];
            if (ArrayHelper.isNonEmptyArray(request.members)) {
                inviteMemberIds = request.members.map(m => m.memberId);
            }
            console.log(`App: request.matchId = ${request.matchId}`);
            matchService.UpdateMatchAvailability(request.matchId, request.memberId, request.leagueId, request.availability, request.action, inviteMemberIds)
                .then((response) => {
                    if (response.is_not_found) {
                        let message = `Sorry, this match is no longer available.  For more info, look in the Matches tab for that ${Branding.League}.`;
                        Notification.notifyError(message);
                    }
                    else if (!response.is_error) {
                        if (callback) callback(response);
                    }
                    else {
                        console.log("Error updating match availability");
                        Notification.notifyError(response.error_content.errorMessage);
                    }
                })
                .catch((err) => {
                    console.log("Error updating match availability");
                    Notification.notifyError(err.message);
                });
        }

        public render() {
            return (
                <Layout signOut={this.signOut} app={this} appState={this.state}/>
            );
        }
    }
