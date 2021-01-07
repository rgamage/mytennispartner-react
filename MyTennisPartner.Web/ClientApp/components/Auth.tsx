import * as React from "react";
import { Link, Redirect, RouteComponentProps } from 'react-router-dom';
import AuthService from '../services/Auth';
import { AppState, AppProps, LoginState } from './App';
import AppConstants from "../models/app-constants";
import AppStateManager from '../states/AppStateManager';
import { NewUserViewModel, LoginViewModel, ResetPasswordViewModel, ForgotPasswordViewModel, CheckPasswordViewModel, EmailConfirmationViewModel } from '../models/viewmodels/UserViewModels';
import { AppStatePropsRoute } from "../models/AppStateProps";
import Branding from "../branding/Branding";
import ErrorHandling, { IErrorResponse } from "../utilities/ErrorHandling";
import ValidationMessage from "./Common/ValidationMessage";
import Notification from "../utilities/Notification";

let authService = new AuthService();
let appStateManager = new AppStateManager();

interface FormFields {
    username?: string;
    email?: string;
    password?: string;
    confirmPassword?: string;
}

interface SignInState {
    initialLoad: boolean,
    error: IErrorResponse,
    emailConfirmSent: boolean,
    errorCode?: string,
    returnUrl: string | null,
    rememberMe: boolean,
    autologin: boolean,
    enableAutoLogin: boolean,
    keepMeLoggedIn: boolean,
    formFields: FormFields
}

function formGroupClass(field: string) {
    var className = "form-control";
    if (field) {
        className += " is-invalid"
    }
    return className;
}

export class SignIn extends React.Component<AppStatePropsRoute, SignInState> {
    constructor(props: AppStatePropsRoute) {
        super(props);
        // get uid and token from url query parameters
        const search = props.location.search;
        const params = new URLSearchParams(search);
        let usernameParameter = params.get('username');
        let passwordParameter = params.get('password');
        let usernameLocal = localStorage.getItem("username");
        let passwordLocal = localStorage.getItem("password");
        let username = usernameLocal || usernameParameter || "";
        let password = passwordLocal || passwordParameter || "";
        let rememberMe = localStorage.getItem("username") != null && localStorage.getItem("password") != null;
        let autologin: boolean = username != null && password != null;
        let keepMeLoggedIn: boolean = localStorage.getItem("keepMeLoggedIn") === 'true';
        let enableAutoLogin: boolean = localStorage.getItem("enableAutoLogin") === 'true' && keepMeLoggedIn;
        this.state = {
            initialLoad: true,
            error: null,
            emailConfirmSent: false,
            rememberMe: rememberMe || (autologin && enableAutoLogin),
            returnUrl: params.get('returnUrl'),
            autologin: autologin,
            enableAutoLogin: enableAutoLogin,
            keepMeLoggedIn: keepMeLoggedIn,
            formFields: {
                username: username,
                password: password
            }
        } as SignInState;
    }

    componentDidMount() {
        if (this.state.autologin && this.state.enableAutoLogin) {
            this.signIn();
        }
    }

    componentDidUpdate() {
    }

    signIn = () => {
        this.setState({ error: null, initialLoad: false });
        let loginInfo = new LoginViewModel();
        loginInfo.email = this.state.formFields.username;
        loginInfo.password = this.state.formFields.password;
        loginInfo.rememberMe = this.state.rememberMe;
        loginInfo.keepMeLoggedIn = this.state.keepMeLoggedIn;
        if (this.state.rememberMe) {
            this.storeCredentials();
        }

        authService.signIn(loginInfo).then(response => {
            console.log('got response from authService.signIn');
            if (!response.is_error) {
                console.log('got Non-Error response from authService.signIn');
                this.props.app.setState({ hasUpdatedUserInfo: true, loginState: LoginState.LoggedIn }, () => {
                    console.log('Inside setAppState...');
                    if (this.state.keepMeLoggedIn) {
                        console.log('keep me logged in = true');
                        localStorage.setItem("enableAutoLogin", "true");
                    }
                    //console.log('navigating to landing page');
                    //this.props.history.push(this.state.returnUrl || AppConstants.LandingPageAfterLogin);
                });
            } else {
                console.log(`got Error response from authService.signIn: ${response.error_content}`);
                this.setState({ error: response.error_content });
            }
        });
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();
        this.signIn();
    }

    requestEmailConfirmation(event: React.MouseEvent<HTMLAnchorElement>) {
        event.preventDefault();
        let model = new EmailConfirmationViewModel();
        model.email = this.state.formFields.username;
        authService.requestEmailConfirmation(model).then((response) => {
            if (!response.is_error) {
                //console.log("successfully requested email confirmation");
                this.setState({ emailConfirmSent: true });
            }
            else {
                this.setState({ error: response.error_content });
            }
        });
    }

    // function to handle input field changes, set corresponding state variables
    handleCheckboxChange = (event: React.FormEvent<HTMLInputElement>) => {
        console.log('SignIn: starting handleCheckboxChange');
        const target = event.currentTarget;
        const value = target.checked;
        const name = target.name;
        if (name == 'rememberMe') {
            console.log(`SignIn: rememberMe check changed, value = ${value}`);
            this.setState({ rememberMe: value });
            if (value) {
                this.storeCredentials();
            }
            else {
                this.clearCredentials();
            }
        }
        if (name == 'keepMeLoggedIn') {
            this.setState({ keepMeLoggedIn: value });
            localStorage.setItem("keepMeLoggedIn", value.toString());
        }
    }

    handleInputChange = (event: any) => {
        const target = event.target;
        const value = target.value;
        const name = target.name;
        let formFields = this.state.formFields;
        formFields[name] = value;
        this.setState({
            formFields: formFields
        });
    }

    storeCredentials = () => {
        console.log(`setting localStorage user=${this.state.formFields.username}`);
        localStorage.setItem("username", this.state.formFields.username);
        localStorage.setItem("password", this.state.formFields.password);
    }

    clearCredentials = () => {
        localStorage.removeItem("username");
        localStorage.removeItem("password");
    }

    render() {
        const search = this.props.location.search;
        const params = new URLSearchParams(search);
        console.log('SignIn render running');
        if (this.props.appState.loginState == LoginState.LoggedIn) {
            console.log('SignIn render - loggedIn');
            // we've logged in, so now go to landing page after login, or return url in the case of user targeting a specific url
            let nextUrl = this.state.returnUrl || AppConstants.LandingPageAfterLogin;
            console.log(`redirecting to ${nextUrl}`);
            return <Redirect to={this.state.returnUrl || AppConstants.LandingPageAfterLogin} />
        }

        let initialLoadContent = null;
        if (this.state.initialLoad) {
            if (params.get('confirmed')) {
                initialLoadContent = <div className="alert alert-success" role="alert">
                    Your email address has been successfully confirmed.
                    </div>
            }

            if (params.get('expired')) {
                initialLoadContent = <div className="alert alert-info" role="alert">
                    <strong>Not logged in or session expired</strong> You need to sign in.
                    </div>
            }

            if (this.props.history.location.state && this.props.history.location.state.signedOut) {
                initialLoadContent = <div className="alert alert-info" role="alert">
                    <strong>Signed Out</strong>
                </div>
            }

            if (this.state.autologin && this.state.enableAutoLogin) {
                initialLoadContent = <div className="alert alert-info">
                    Logging in, please stand by...
                    </div>
            }
        }
        return <div className="auth">
            <form className="formAuth" onSubmit={(e) => this.handleSubmit(e)}>
                <h2 className="formAuthHeading">Please sign in</h2>
                {initialLoadContent}
                {this.state.error &&
                    <div className="alert alert-danger" role="alert">
                    {this.state.error.errorMessage}
                        {ErrorHandling.getValidationError(this.state.error, "email_not_confirmed") && 
                        <span>&nbsp;&nbsp;To confirm this e-mail address click&nbsp;<a href="/" onClick={(e) => this.requestEmailConfirmation(e)}>here, then check your email for a confirmation link.  Click it and return to log in.</a></span>
                        }
                    </div>
                }
                {this.state.emailConfirmSent &&
                    <div className="alert alert-success" role="alert">
                        <span><i className="fa fa-check"></i> Confirmation e-mail has been sent</span>
                    </div>
                }
                <label htmlFor="inputEmail" className="form-control-label sr-only">Email address</label>
                <input type="email" name="username" id="inputEmail" onChange={this.handleInputChange} value={this.state.formFields.username} className="form-control form-control-danger" placeholder="Email address"/>
                <label htmlFor="inputPassword" className="form-control-label sr-only">Password</label>
                <input type="password" name="password" id="inputPassword" onChange={this.handleInputChange} value={this.state.formFields.password} className="form-control" placeholder="Password" />

                <div className="form-check">
                    <input type="checkbox" className="form-check-input mtp-check" id="rememberMe" name="rememberMe" onChange={this.handleCheckboxChange} checked={this.state.rememberMe} />
                    <label className="form-check-label mtp-label" htmlFor="rememberMe">
                        Remember Me
                    </label>
                </div>

                <div className="form-check margin-bottom">
                    <input type="checkbox" className="form-check-input mtp-check" id="keepMeLoggedIn" name="keepMeLoggedIn" onChange={this.handleCheckboxChange} checked={this.state.keepMeLoggedIn} />
                    <label className="form-check-label mtp-label" htmlFor="keepMeLoggedIn">
                        Keep Me Logged In
                    </label>
                </div>

                <button className="btn btn-lg btn-primary btn-block" type="submit">Sign in</button>
            </form>
            <div className="authEtc">
                Not a member? <Link to="/register">Register Here</Link>
                <br />
                Forgot your password? <Link to={`/forgot?email=${this.state.formFields.username}`}>Reset Password Here</Link>
                <br />
                <br />
                Don't want to remember a new password?  Sign in with any of these other providers:
                <br />
                <br />
                <div className="row">
                    <div className="col-2 offset-2">
                        <form id="form1" action="/api/auth/externallogin" method="post">
                            <input type="hidden" value="Facebook" name="provider" />
                            <input type="hidden" value={this.state.keepMeLoggedIn ? "true" : "false"} name="keepMeLoggedIn" />
                            <input role="link" className="pointer" type="image" src="/images/fb-icon.png" width="45" alt="sign in with Facebook" title="Sign in with Facebook" />
                        </form>
                    </div>
                    <div className="col-2 offset-1">
                        <form id="form2" action="/api/auth/externallogin" method="post">
                            <input type="hidden" value="Google" name="provider" />
                            <input type="hidden" value={this.state.keepMeLoggedIn ? "true" : "false"} name="keepMeLoggedIn" />
                            <input role="link" className="pointer" type="image" src="/images/google-icon.png" width="45" alt="sign in with Google" title="Sign in with Google"/>
                        </form>
                    </div>
                    <div className="col-2 offset-1">
                        <form id="form3" action="/api/auth/externallogin" method="post">
                            <input type="hidden" value="Twitter" name="provider" />
                            <input type="hidden" value={this.state.keepMeLoggedIn ? "true" : "false"} name="keepMeLoggedIn" />
                            <input role="link" className="pointer" type="image" src="/images/twitter-icon.png" width="45" alt="sign in with Twitter" title="Sign in with Twitter" />
                        </form>
                    </div>
                </div>
            </div>
        </div>;
    }
}

export class Register extends React.Component<any, any> {
    refs: {
        email: HTMLInputElement;
        password: HTMLInputElement;
        firstName: HTMLInputElement;
        lastName: HTMLInputElement;
    }

    state = {
        registerComplete: false,
        errors: null as IErrorResponse,
        inputModel: NewUserViewModel
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        let newUser = new NewUserViewModel();
        newUser.firstName = this.refs.firstName.value;
        newUser.lastName = this.refs.lastName.value;
        newUser.username = this.refs.email.value;
        newUser.password = this.refs.password.value;

        this.setState({ errors: null });
        authService.register(newUser).then(response => {
            if (!response.is_error) {
                this.setState({ registerComplete: true })
            } else {
                this.setState({ errors: response.error_content });
                if (!response.is_bad_request) {
                    Notification.notifyError(response.error_content.errorMessage);
                }
            }
        });
    }

    render() {
        if (this.state.registerComplete) {
            //return <RegisterComplete email={this.refs.email.value} />
            localStorage.removeItem("username");
            localStorage.removeItem("password");

            return <Redirect to={`/signin?username=${this.refs.email.value}&password=${this.refs.password.value}`} />
        } else {
            return <div className="auth">
                <form className="formAuth" onSubmit={(e) => this.handleSubmit(e)}>
                    <h2 className="formAuthHeading">Please register to join</h2>
                    {ErrorHandling.getValidationErrorGeneral(this.state.errors) &&
                        <div className="alert alert-danger" role="alert">
                            {ErrorHandling.getValidationErrorGeneral(this.state.errors)}
                        </div>
                    }
                    <div>
                        <label htmlFor="inputFirstName">First Name</label>
                        <input required type="text" id="inputFirstName" ref="firstName" className={formGroupClass(ErrorHandling.getValidationError(this.state.errors, "firstName"))} placeholder="First Name" />
                        <div className="form-control-feedback">{ErrorHandling.getValidationError(this.state.errors, "firstName")}</div>
                    </div>
                    <div>
                        <label htmlFor="inputLastName">Last Name</label>
                        <input required type="text" id="inputLastName" ref="lastName" className={formGroupClass(ErrorHandling.getValidationError(this.state.errors, "lastName"))} placeholder="Last Name" />
                        <div className="form-control-feedback">{ErrorHandling.getValidationError(this.state.errors, "lastName")}</div>
                    </div>
                    <div>
                        <label htmlFor="inputEmail">Email address</label>
                        <input required type="email" id="inputEmail" ref="email" className={formGroupClass(ErrorHandling.getValidationError(this.state.errors, "username"))} placeholder="Email address" />
                        <small id="emailHelp" className="form-text text-muted">We'll never share your email with anyone else.</small>
                        <div className="form-control-feedback">{ErrorHandling.getValidationError(this.state.errors, "username")}</div>
                    </div>
                    <div>
                        <label htmlFor="inputPassword">Password</label>
                        <input required type="password" id="inputPassword" ref="password" className={formGroupClass(ErrorHandling.getValidationError(this.state.errors, "password"))} placeholder="Password" />
                        <small id="pwdHelp" className="form-text text-muted">password must be 6 chars or more</small>
                        <div className="form-control-feedback">{ErrorHandling.getValidationError(this.state.errors, "password")}</div>
                    </div>
                    <button className="btn btn-lg btn-primary btn-block" type="submit">Sign up</button>
                </form>
                <br />
                Don't want to remember a new password?  Sign in with any of these other providers:
                <br />
                <br />
                <div className="row">
                    <div className="col-2 offset-2">
                        <form id="form1" action="/api/auth/externallogin" method="post">
                            <input type="hidden" value="Facebook" name="provider" />
                            <input role="link" className="pointer" type="image" src="/images/fb-icon.png" width="45" alt="sign in with Facebook" title="Sign in with Facebook" />
                        </form>
                    </div>
                    <div className="col-2 offset-1">
                        <form id="form2" action="/api/auth/externallogin" method="post">
                            <input type="hidden" value="Google" name="provider" />
                            <input role="link" className="pointer" type="image" src="/images/google-icon.png" width="45" alt="sign in with Google" title="Sign in with Google" />
                        </form>
                    </div>
                    <div className="col-2 offset-1">
                        <form id="form3" action="/api/auth/externallogin" method="post">
                            <input type="hidden" value="Twitter" name="provider" />
                            <input role="link" className="pointer" type="image" src="/images/twitter-icon.png" width="45" alt="sign in with Twitter" title="Sign in with Twitter" />
                        </form>
                    </div>
                </div>

            </div>;
        };
    }
}

interface RegisterCompleteProps {
    email: string;
}

export class RegisterComplete extends React.Component<RegisterCompleteProps, any> {
    render() {
        return <div className="auth">
            <div className="alert alert-success" role="alert">
                <strong>Success!</strong>  Your account has been created.
            </div>
            <p>
                A confirmation email has been sent to {this.props.email}. You will need to follow the provided link to confirm your email address before signing in.
            </p>
            <Link className="btn btn-lg btn-primary btn-block" role="button" to="/">Sign in</Link>
        </div>;
    }
}



export class ForgotPassword extends React.Component<AppStatePropsRoute, any> {

    refs: {
        email: HTMLInputElement;
        password: HTMLInputElement;
    }

    state = {
        errors: null,
        forgotComplete: false
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        let forgotInfo = new ForgotPasswordViewModel();
        forgotInfo.email = this.refs.email.value;

        // since we're resetting our password, clear the one that is stored
        localStorage.removeItem("password");

        this.setState({ errors: null });
        authService.forgot(forgotInfo).then(response => {
            if (!response.is_error) {
                this.setState({ forgotComplete: true })
            } else {
                this.setState({ errors: response.error_content });
                Notification.notifyError(response.error_content.errorMessage);
            }
        });
    }

    render() {
        const search = this.props.location.search;
        const params = new URLSearchParams(search);
        const email = params.get('email');
        const defaultEmail = email;

        if (this.state.forgotComplete) {
            return <div className="auth">
                <div className="alert alert-success" role="alert">
                    <strong>Success!</strong>  Your password reset request has been processed.
            </div>
                <p>An e-mail has been sent to your email with a confirmation link.  Click the link in the e-mail to complete the password reset process.</p>
                <Link className="btn btn-lg btn-primary btn-block" role="button" to="/">Sign in</Link>
            </div>;
        }
        
        return <div className="auth">
            <form className="formAuth" onSubmit={(e) => this.handleSubmit(e)}>
                <h2>Password Reset</h2>
                <h4 className="formAuthHeading">Please Enter E-mail</h4>
                <label htmlFor="inputEmail" className="form-control-label sr-only">Email address</label>
                <input type="email" id="inputEmail" defaultValue={defaultEmail} ref="email" className="form-control form-control-danger" placeholder="Email address" />
                <ValidationMessage errors={this.state.errors} fieldName="email" />
                {ErrorHandling.getValidationErrorGeneral(this.state.errors) &&
                    <div className="alert alert-danger" role="alert">
                    {ErrorHandling.getValidationErrorGeneral(this.state.errors)}
                    </div>
                }
                <br />
                <button className="btn btn-lg btn-primary btn-block" type="submit">Send Reset Link</button>
            </form>
        </div>;
    }
}

interface ResetState {
    uid: string;
    token: string;
    errors: IErrorResponse;
    resetComplete: boolean;
    formFields: FormFields;
}

export class ResetPassword extends React.Component<RouteComponentProps<{}>, ResetState> {
    constructor(props: RouteComponentProps<{}>) {
        super(props);

        // get uid and token from url query parameters
        const search = props.location.search;
        const params = new URLSearchParams(search);
        this.state = {
            uid: params.get('uid'),
            token: params.get('token'),
            errors: null,
            resetComplete: false,
            formFields: {
                email: params.get('email'),
                password: "",
                confirmPassword: ""
            }
        } as ResetState;
    }

    handleSubmit = (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        this.setState({ errors: null });
        let resetInfo = new ResetPasswordViewModel();
        resetInfo.email = this.state.formFields.email;
        resetInfo.password = this.state.formFields.password;
        resetInfo.confirmPassword = this.state.formFields.confirmPassword;
        resetInfo.code = this.state.token;

        authService.reset(resetInfo).then(response => {
            if (!response.is_error) {
                this.setState({ resetComplete: true });
            } else {
                this.setState({ errors: response.error_content });
            }
        });
    }

    handleInputChange = (event: any) => {
        const target = event.target;
        const value = target.value;
        const name = target.name;
        let formFields = this.state.formFields;
        formFields[name] = value;
        this.setState({
            formFields: formFields
        });
    }

    public render() {
        if (this.state.resetComplete) {
            // auto-login after resetting password
            return <Redirect to={`/signin?username=${this.state.formFields.email}&password=${this.state.formFields.password}`} />
        }
        return <div className="auth">
            <form className="formAuth" onSubmit={(e) => this.handleSubmit(e)}>
                <h2 className="formAuthHeading">Enter New Password</h2>
                {ErrorHandling.getValidationErrorGeneral(this.state.errors) &&
                    <div className="alert alert-danger" role="alert">
                        {ErrorHandling.getValidationErrorGeneral(this.state.errors)}
                    </div>
                }
                <div className="form-group">
                    <div>
                        <label htmlFor="inputEmail" className="form-control-label sr-only">Email address</label>
                        <input onChange={this.handleInputChange} name="email" type="email" id="inputEmail" defaultValue={this.state.formFields.email} className={formGroupClass(ErrorHandling.getValidationError(this.state.errors, "email"))} placeholder="Email address" />
                        <div className="invalid-feedback">{ErrorHandling.getValidationError(this.state.errors, "email")}</div>
                    </div>
                    <div>
                        <label htmlFor="inputPassword" className="form-control-label sr-only">New Password</label>
                        <input onChange={this.handleInputChange} name="password" type="password" id="inputPassword" className={formGroupClass(ErrorHandling.getValidationError(this.state.errors, "password"))} placeholder="Password" />
                        <div className="invalid-feedback">{ErrorHandling.getValidationError(this.state.errors, "password")}</div>
                    </div>
                    <div>
                        <label htmlFor="inputconfirmPassword" className="form-control-label sr-only">Confirm Password</label>
                        <input onChange={this.handleInputChange} name="confirmPassword" type="password" id="inputconfirmPassword" className={formGroupClass(ErrorHandling.getValidationError(this.state.errors, "confirmPassword"))} placeholder="Confirm Password" />
                        <div className="invalid-feedback">{ErrorHandling.getValidationError(this.state.errors, "confirmPassword")}</div>
                    </div>
                </div>
                <button className="btn btn-lg btn-primary btn-block" type="submit">Reset Password</button>
            </form>
        </div>;
    }
}

enum ExternalSignInStates {
    initialLoad,
    needPassword,
    successsPassword,
    failedPassword
}

interface ExternalSignInState {
    errors: IErrorResponse;
    email: string;
    provider: string;
    needPasswordCheck: boolean;
    stateMachine: ExternalSignInStates;
    emailConfirmSent: boolean;
}

export class ExternalSignIn extends React.Component<RouteComponentProps<{}>, ExternalSignInState> {

    constructor(props: RouteComponentProps<{}>) {
        super(props);
        //console.log("running ExternalSignIn constructor");
        const search = props.location.search;
        const params = new URLSearchParams(search);
        this.username = params.get("email") || "";
        this.state.email = this.username.includes("@") ? this.username : "";
        this.state.provider = params.get("provider") || "";
    }

    username: string;

    refs: {
        email: HTMLInputElement;
        password: HTMLInputElement;
    }

    state = {
        errors: null,
        externalComplete: false,
        email: "",
        provider: "",
        needPasswordCheck: false,
        stateMachine: ExternalSignInStates.initialLoad,
        emailConfirmSent: false
    }

    handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        switch (this.state.stateMachine) {
            case ExternalSignInStates.initialLoad:
                if (this.state.email != this.refs.email.value) {
                    //console.log(`1 state.email = ${this.state.email}, refs.email=${this.refs.email.value}`);
                    //console.log("handling case where e-mails are different");
                    this.setState({ stateMachine: ExternalSignInStates.needPassword, email: this.refs.email.value });
                    event.preventDefault();
                }
                break;
            case ExternalSignInStates.needPassword:
                this.setState({ errors: null });
                let checkPasswordInfo = new CheckPasswordViewModel();
                checkPasswordInfo.email = this.refs.email.value;
                checkPasswordInfo.password = this.refs.password.value;
                authService.checkPassword(checkPasswordInfo).then((response) => {
                    if (!response.is_error) {
                        //console.log(`2 state.email = ${this.state.email}, refs.email=${this.refs.email.value}`);
                        this.setState({ stateMachine: ExternalSignInStates.successsPassword, email: this.refs.email.value });
                    } else {
                        this.setState({ errors: response.error_content });
                    }
                });
                event.preventDefault();
                break;
            case ExternalSignInStates.successsPassword:
                // for this case we don't want to do anything - just step aside and let the normal form submit happen
                break;
        }
    }

    requestEmailConfirmation(event: React.MouseEvent<HTMLAnchorElement>) {
        event.preventDefault();
        let model = new EmailConfirmationViewModel();
        model.email = this.refs.email.value;
        authService.requestEmailConfirmation(model).then((response) => {
            if (!response.is_error) {
                //console.log("successfully requested email confirmation");
                this.setState({ emailConfirmSent: true });
            }
            else {
                this.setState({ errors: response.error_content });
            }
        });
    }

    render() {
        let needPasswordMessage = null;
        if (this.state.needPasswordCheck) {
            needPasswordMessage = <div className="alert alert-info" role="alert">
                You need to provide your password to link to a different email address.
                    </div>
        }

        if (this.state.stateMachine == ExternalSignInStates.initialLoad || this.state.stateMachine == ExternalSignInStates.successsPassword) {
            return <div className="auth">
                <div className="alert alert-success" role="alert">
                    {this.state.stateMachine == ExternalSignInStates.initialLoad &&
                        <p><strong>Success!</strong>  Your identity has been confirmed with {this.state.provider}, as {this.username}.</p>
                    }
                    {this.state.stateMachine == ExternalSignInStates.successsPassword && 
                        <p><i className="fa fa-check" /> Password Confirmed</p>
                    }
                    <p>Enter the e-mail below that you want to link to for your new user account. </p>
                </div>
                <div className="alert alert-info" role="alert">
                    <p>If you already have an account with this website that is different than your {this.state.provider} email, make sure to enter it here, so you don't create two different accounts.</p>
                </div>
                <form className="formAuth" action="/api/auth/externalloginconfirmation" method="post" onSubmit={(e) => this.handleSubmit(e)}>
                    <h2 className="formAuthHeading">Enter E-mail to link to {this.state.provider}</h2>
                    {ErrorHandling.getValidationErrorGeneral(this.state.errors) &&
                        <div className="alert alert-danger" role="alert">
                            {ErrorHandling.getValidationErrorGeneral(this.state.errors)}
                        </div>
                    }
                    <label htmlFor="inputEmail" className="form-control-label sr-only">Email address</label>
                    <input type="hidden" name="returnUrl" value="/home" />
                    <input type="email" name="email" id="inputEmail" ref="email" defaultValue={this.state.email} className="form-control form-control-danger" placeholder="Email address" />
                    <button className="btn btn-lg btn-primary btn-block" type="submit">Sign In with {this.state.provider}</button>
                </form>
            </div>;
        }

        if (this.state.stateMachine == ExternalSignInStates.needPassword) {
            return <div className="auth">
                <div className="alert alert-info" role="alert">
                    <p><strong>NOTE:</strong> To link with this e-mail address, you need to provide your {Branding.AppName} password for that e-mail address.</p>
                    <p>Enter the password below to verify your ownership of this account. </p>
                    {this.state.emailConfirmSent &&
                        <p><i className="fa fa-check" /> Confirmation E-mail Sent</p>
                    }
                </div>
                {this.state.emailConfirmSent &&
                    <div className="alert alert-success" role="alert">
                            <p><i className="fa fa-check" /> Confirmation E-mail Sent</p>
                    </div>}
                <form className="formAuth" action="/api/auth/externalloginconfirmation" method="post" onSubmit={(e) => this.handleSubmit(e)}>
                    <h2 className="formAuthHeading">Enter E-mail to link to {this.state.provider}</h2>
                    {ErrorHandling.getValidationErrorGeneral(this.state.errors) &&
                        <div className="alert alert-danger" role="alert">
                            {ErrorHandling.getValidationErrorGeneral(this.state.errors)}
                        </div>
                    }
                    {ErrorHandling.getValidationError(this.state.errors, "email_not_confirmed") &&
                        <div className="alert alert-danger" role="alert">
                            {ErrorHandling.getValidationError(this.state.errors, "email_not_confirmed")}
                            &nbsp;&nbsp;To confirm this e-mail address click&nbsp;
                            <a href="/" onClick={(e) => this.requestEmailConfirmation(e)}>here.</a>
                        </div>
                    }
                    <label htmlFor="inputEmail" className="form-control-label sr-only">Email address</label>
                    <input type="hidden" name="returnUrl" value="/home" />
                    <input type="email" name="email" id="inputEmail" ref="email" defaultValue={this.refs.email.value} className="form-control form-control-danger" placeholder="Email address" />
                    <label htmlFor="inputPassword" className="form-control-label sr-only">Password</label>
                    <input type="password" id="inputPassword" ref="password" className="form-control" placeholder="Password" />
                    <button className="btn btn-lg btn-primary btn-block" type="submit">Verify Account Password</button>
                </form>
            </div>;
        }

        if (this.state.stateMachine == ExternalSignInStates.failedPassword) {
            return <div className="auth">
                <div className="alert alert-danger" role="alert">
                    <p>Failure! Password NOT verified</p>
                </div>
            </div>
        }
        return null;
    }
}

interface SetTokenState {
    token: string;
    returnUrl: string;
}

export class SetToken extends React.Component<AppStatePropsRoute, SetTokenState> {
    constructor(props: AppStatePropsRoute) {
        super(props);

        // get token from url query parameters
        const search = props.location.search;
        const params = new URLSearchParams(search);
        this.state = {
            token: params.get('token'),
            returnUrl: params.get('returnUrl')
        } as SetTokenState;

        // set this token that was passed to us, to persist our login state on the client side
        //console.log("setting token to " + this.state.token);
        authService.setToken(this.state.token);
        this.props.app.setState({ hasUpdatedUserInfo: true });
    }

    public render() {
        let nextUrl = this.state.returnUrl || AppConstants.LandingPageAfterLogin;
        //console.log("Redirecting to " + nextUrl);
        return <Redirect to={nextUrl} />
    }
}




