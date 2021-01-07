import * as React from "react";
import { Link, NavLink, Redirect } from 'react-router-dom';
import AppConstants from "../models/app-constants";
import { AppStatePropsRoute } from "../models/AppStateProps";
import MemberViewModel from '../models/viewmodels/MemberViewModel';
import MemberService from '../services/MemberService';
import MemberImageService from '../services/MemberImageService';
//import 'react-select/dist/react-select.css';
import SelectOption, { SelectOptionList } from '../models/SelectOption';
import 'react-datepicker/dist/react-datepicker.css';
import FormInputText from "./Common/FormInputText";
import FormInputSelect from "./Common/FormInputSelect";
import VenueService from "../services/VenueService";
import FormInputSelectAsync from "./Common/FormInputSelectAsync";
import Notification from "../utilities/Notification";
import FormInputSelectHtml from "./Common/FormInputSelectHtml";
import ErrorHandling, { IErrorResponse } from "../utilities/ErrorHandling";
import ConfirmationModal from "./Common/ConfirmationModal";
import Branding from "../branding/Branding";
import FormInputCheckbox from "./Common/FormInputCheckbox";
import ReservationServiceLoginResult from "../models/viewmodels/ReservationServiceLoginResult";
import ArrayHelper from "../utilities/ArrayHelper";
import ReservationService from "../services/ReservationService";
import ReservationCredentials from "../models/viewmodels/ReservationCredentials";
import Loader from "./Common/Loader";

let memberService = new MemberService();
let memberImageService = new MemberImageService();
let venueService = new VenueService();
let reservationService = new ReservationService();

interface MyProfileStates {
    model: MemberViewModel;
    errors?: IErrorResponse;
    imageKey: string;
    newProfileCompleted: boolean;
    clientImage: any;
    awaitingModelUpdate: boolean;
    showModal: boolean;
    showNotifications: boolean;
    reservationLoginResults: ReservationServiceLoginResult[];
    isLoadingReservationResults: boolean;
    hasEditedSparetimeInfo: boolean;
    saveOnModalClose: boolean;
    showReservationModal: boolean;
    reservationLoginFailed: boolean;
    showReservationFields: boolean;
}

export class MyProfile extends React.Component<AppStatePropsRoute, MyProfileStates> {
    constructor(props: AppStatePropsRoute) {
        super(props);

        console.log("MyProfile - constructor running...");
        let hasMemberProfile: boolean = props.appState.member != null;
        console.log(`MyProfile - hasMemberProfile = ${hasMemberProfile}`);
        let member = hasMemberProfile ? props.appState.member : new MemberViewModel();
        if (!hasMemberProfile && props.appState.user != null && props.appState.member == null) {
            console.log(`setting firstName = ${props.appState.user.firstName}`);
            member.firstName = props.appState.user.firstName;
            member.lastName = props.appState.user.lastName;
        }
        this.memberId = member && member.memberId || 0;
        member.userId = props.appState.user && props.appState.user.userId || "";

        this.state = {
            model: member,
            errors: null,
            imageKey: `${(new Date()).getSeconds()}.${(new Date()).getMilliseconds()}`,
            newProfileCompleted: false,
            clientImage: null,
            awaitingModelUpdate: false,
            showModal: false,
            showNotifications: false,
            reservationLoginResults: [],
            isLoadingReservationResults: false,
            hasEditedSparetimeInfo: false,
            saveOnModalClose: false,
            showReservationModal: false,
            reservationLoginFailed: false,
            showReservationFields: false
        }
    }

    uploadFile: File;
    memberId: number;
    fileInputElement: HTMLInputElement;
    openModalButtonElement: HTMLButtonElement;
    openReservationModalButtonElement: HTMLButtonElement;

    componentWillMount() {
    }

    componentDidMount() {
        // get any updates to the member profile
        if (this.state.awaitingModelUpdate) {
            if (this.props.appState.member) {
                this.setState({ model: this.props.appState.member, awaitingModelUpdate: false });
            }
        }
    }

    saveProfile = () => {
        //console.log("saveProfile running");
        let member = this.state.model;

        // clear errors for page, and update member with uploaded file (if any)
        this.setState({ errors: null, model: member, hasEditedSparetimeInfo: false });

        // create or update member, depending on if exists or not
        if (this.props.appState.member) {
            this.updateMember();
        }
        else {
            this.createMember();
        }
    }

    saveChanges = (event: any) => {
        //console.log("saveChanges running");
        event.preventDefault();
        if (this.state.hasEditedSparetimeInfo && this.state.model.spareTimeUsername && this.state.model.spareTimePassword) {
            this.setState({ saveOnModalClose: true });
            this.showReservationModal();
            this.onTestCourtLogin(event);
        }
        else {
            this.saveProfile();
        }

        // clear errors for page, and update member with uploaded file (if any)
        this.setState({ errors: null });
    }

    updateMemberImage = (memberId: number) => {
        memberImageService.createMemberImage(memberId, this.uploadFile).then((response) => {
            if (response) {
                if (!response.is_error) {
                    if (response.content) {
                        let imageKey = `${(new Date()).getSeconds()}.${(new Date()).getMilliseconds()}`;
                        //this.setState({ imageKey: imageKey, awaitingModelUpdate: true, clientImage: null })
                        this.setState({ imageKey: imageKey })
                        //this.props.app.setState({ hasUpdatedUserInfo: true });
                    }
                }
                else {
                    console.log("ERROR creating member image");
                    this.setState({ errors: response.error_content });
                    Notification.notifyError(response.error_content.errorMessage);
                }
            }
        }).catch((err) => {
            Notification.notifyError(`Error on image create: ${err.message}`);
            //alert("Caught an error on image create");
        });
    }

    createMember = () => {
        //let userId = this.props.appState.user && this.props.appState.user.userId || "";

        memberService.createMember(this.state.model).then((response) => {
            if (response) {
                if (!response.is_error) {
                    if (response.content) {
                        let member = response.content;
                        this.setState({ newProfileCompleted: true, awaitingModelUpdate: true });
                        this.props.app.setState({ hasUpdatedUserInfo: true });
                        if (this.uploadFile) {
                            this.updateMemberImage(member.memberId);
                        }
                        this.showModal();
                    }
                }
                else {
                    this.setState({ errors: response.error_content });
                    if (!response.is_bad_request) {
                        console.log("ERROR creating member");
                        Notification.notifyError(response.error_content.errorMessage);
                    }
                }
            }
        });
    }

    updateMember = () => {
        memberService.updateMember(this.state.model).then(response => {
            if (!response.is_error) {
                let member = response.content;
                this.setState({ awaitingModelUpdate: true });
                this.props.app.setState({ hasUpdatedUserInfo: true });
                Notification.notifySuccess("Your changes have been saved!");
                if (this.uploadFile) {
                    this.updateMemberImage(member.memberId);
                }
            }
            else {
                this.setState({ errors: response.error_content });
                if (!response.is_bad_request) {
                    console.log("ERROR updating member");
                    Notification.notifyError(response.error_content.errorMessage);
                }
            }
        });
    }

    handleFileChange = (event: React.FormEvent<HTMLInputElement>) => {
        this.uploadFile = event.currentTarget.files[0];
        let clientImage = null;
        var reader = new FileReader();
        reader.onloadend = () => {
            clientImage = reader.result;
            this.setState({ clientImage: clientImage });
        }
        reader.readAsDataURL(this.uploadFile);
    }

    //handleDateChange = (selectedMoment: any) => {
    //    if (selectedMoment) {
    //        let date = selectedMoment.toDate();
    //        let model = this.state.model;
    //        model.birthdate = date;
    //        this.setState({ model: model });
    //    }
    //}

    // function to handle input field changes, set corresponding state variables
    handleInputChange = (event: React.FormEvent<HTMLInputElement>) => {
        this.setState({ errors: null });
        const target = event.currentTarget;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.name;
        let model = this.state.model;
        let hasEditedSparetimeInfo = this.state.hasEditedSparetimeInfo;
        if (name == "spareTimeUsername" || name == "spareTimePassword") {
            // remember if we have edited court login creds, so we can test them on save
            hasEditedSparetimeInfo = true;
        }
        model[name] = value;
        //console.log(`Setting field '${name}' to '${value}'`);
        this.setState({
            model: model, hasEditedSparetimeInfo: hasEditedSparetimeInfo
        });
    }

    handleReservationCheckboxChange = (event: React.FormEvent<HTMLInputElement>) => {
        this.setState({ errors: null });
        const target = event.currentTarget;
        const value = target.checked;
        this.setState({ showReservationFields: value });
    }

    handleSelectChangeMulti = (propName: string) => (selectedOption: SelectOption[] | null) => {
        this.setState({ errors: null });
        let member = this.state.model;
        member[propName] = selectedOption || [];
        this.setState({ model: member });
    }

    handleSelectChangeHtml = (event: React.ChangeEvent<HTMLSelectElement>) => {
        this.setState({ errors: null });
        let model = this.state.model;
        let name = event.target.name;
        let value = event.target.value;
        model[name] = value;
        this.setState({ model: model });
        //console.log(`Selected: ${value} as value`);
    }

    handleSelectChange = (propName: string) => (selectedOption: SelectOption | null) => {
        this.setState({ errors: null });
        let model = this.state.model;
        model[propName] = ((selectedOption && selectedOption.value) as number) || 0;
        this.setState({ model: model });
        //console.log(`Selected: ${selectedOption && selectedOption.label}`);
    }

    handleImageError = (event: React.SyntheticEvent<HTMLImageElement>) => {
        //console.log("executing handleImageError")
        // image link is broken, so show placeholder instead of broken link
        event.currentTarget.src = "/images/profile-placeholder.png";
    }

    searchVenues = (search: string): Promise<SelectOption[]> => {
        return venueService.searchVenues(search).then(response => {
            return response.content;
        });
    }

    handleSelectChangeAsync = (propName: string) => (selectedOption: SelectOption | null) => {
        this.setState({ errors: null });
        let model = this.state.model;
        model[propName] = selectedOption;
        this.setState({ model: model });
        //console.log(`Selected: ${selectedOption && selectedOption.label}`);
    }

    onModalClosed = () => {
        this.props.history.push("/dashboard");
    }

    showModal = () => {
        this.openModalButtonElement.click();
    }

    toggleNotifications = (event: any) => {
        let showNotifications = this.state.showNotifications;
        this.setState({ showNotifications: !showNotifications });
    }

    onTestCourtLogin = (event: any) => {
        event.preventDefault();
        this.setState({ reservationLoginResults: [], isLoadingReservationResults: true });
        console.log("onTestCourtLogin fired");
        let creds = new ReservationCredentials();
        creds.username = this.state.model.spareTimeUsername;
        creds.password = this.state.model.spareTimePassword;
        creds.host = "host"; // dummy value for this method, since we are scanning several hosts
        reservationService.testSparetimeLogins(creds).then(response => {
            if (!response.is_error) {
                let results = response.content;
                this.setState({ reservationLoginResults: results, isLoadingReservationResults: false });
                this.setMemberNumber(results);
            }
            else {
                this.setState({ errors: response.error_content, isLoadingReservationResults: false });
                if (!response.is_bad_request) {
                    console.log("ERROR testing court logins");
                    Notification.notifyError(response.error_content.errorMessage);
                }
            }
        });
    }

    setMemberNumber = (results: ReservationServiceLoginResult[]) => {
        if (ArrayHelper.isNonEmptyArray(results)) {
            let member = this.state.model;
            let saveMember = false;
            let autoReserveVenuesArray: string[] = [];
            results.forEach(r => {
                if (r.loginSuccess) {
                    // we found the member's number, so set it in their profile
                    member.spareTimeMemberNumber = r.memberNumber;
                    autoReserveVenuesArray.push(`[${r.venueId}]`);
                    saveMember = true;
                }
            })
            if (saveMember) {
                // we found a number, so save it in their profile
                member.autoReserveVenues = autoReserveVenuesArray.join(",");
                this.setState({ model: member, reservationLoginFailed: false, saveOnModalClose: true });
            }
            else {
                // we failed our login test
                this.setState({ reservationLoginFailed: true });
            }
        }
    }

    onReservationModalConfirm = () => {
        if (this.state.saveOnModalClose) {
            this.setState({ saveOnModalClose: false });
            this.saveProfile();
        }
    }

    showReservationModal = () => {
        this.openReservationModalButtonElement.click();
    }

    render() {
        // if not logged in, redirect to login page
        if (!this.props.appState.user || !this.props.appState.user.isLoggedIn) {
            //return <Redirect to={`${AppConstants.loginUrl}?returnUrl=/profile`} />
        }

        //console.log(`Role list values = ${this.state.model && this.state.model.rolesList || "unknown"}`);
        let x = this.state.model && this.state.model.rolesList || "unknown";
        //console.log(`x is null: ${x === null}, x is undefined: ${x === undefined}, x is array: ${x instanceof Array}`);

        const genderValue = this.state.model && this.state.model.gender || 0;
        const birthYearValue = this.state.model && this.state.model.birthYear && this.state.model.birthYear.toString() || "";
        //const selectedDate = moment(this.state.model && this.state.model.birthdate || new Date());

        if (!this.props.appState.user) {
            return <div>
                <p>You must be logged in to view your profile</p>
                </div>
        }

        let imageUrl = `${AppConstants.memberImageFile}/${this.memberId}?timeStamp=${this.state.imageKey}`;

        return <div className="container my-profile">
            <button hidden ref={button => this.openModalButtonElement = button} data-toggle="modal" data-target="#confirmationModal"></button>
            <ConfirmationModal
                title="Profile Completed"
                onConfirm={this.onModalClosed}
                noCancel >
                <div className="alert alert-success">
                    <p>
                        <i className="fa fa-check-circle" />  Thank you for completing your profile!  You can now join existing {Branding.league}s, creating your own {Branding.league}s, and spend more time playing tennis and less time managing it!
                    </p>
                </div>
                <p>
                    Click OK and you will be directed to your "My Tennnis" page, your main point to see at a glance what matches you have coming up, reminders, alerts, etc.
                </p>
            </ConfirmationModal>
            <ConfirmationModal
                id="reservationModal"
                title="Court Reservation Test"
                onConfirm={this.state.isLoadingReservationResults ? null : this.onReservationModalConfirm}
                noCancel >
                <div>
                    <p>
                        Here are the results from your court reservation login test:
                    </p>
                    {!ArrayHelper.isNonEmptyArray(this.state.reservationLoginResults) && <div>
                        <p>Testing your court login credentials...</p>
                        <Loader />
                    </div>}
                    {this.state.reservationLoginResults.map((result, i) =>
                        <big key={i}>
                            <div className="row">
                                <div className="offset-1 col-10 offset-sm-2 col-sm-8">
                                    <i hidden={result.loginSuccess} className="fa fa-times" />
                                    <i hidden={!result.loginSuccess} className="fa fa-check" />
                                    <span className="margin-left">{result.venueName}</span>
                                </div>
                            </div>
                        </big>
                    )}
                    {this.state.reservationLoginFailed && <div>
                        <p className="highlight-error">Your login credentials are not valid at any of the clubs.  Re-enter your court reservation username and password, or <Link to="/reservations">follow this guide</Link> for resolving reservation login issues.  You can ignore this message if you will not be using the auto-reservation feature.</p>
                    </div>}
                </div>
            </ConfirmationModal>
            <h2>My Profile</h2>
            <div className="row margin-bottom">
                <div className="col-sm-8">
                    <h4>{this.state.model && this.state.model.firstName} {this.state.model && this.state.model.lastName}</h4>
                    {!this.props.appState.member && 
                        <div className="alert alert-info" role="alert">
                            <p><i className="fa fa-info-circle" /> Let's get your profile created, so we can start organizing your tennis!  Fill out the info below and you'll be on your way.</p>
                        </div>
                    }
                </div>
                <div className="col-sm-4">
                    <img className="profile-image pointer" key={this.state.imageKey} src={this.state.clientImage || imageUrl} onError={this.handleImageError} onClick={() => this.fileInputElement.click()} />
                    <div>Click image to update</div>
                </div>
            </div>
            <form>
                {this.state.newProfileCompleted &&
                    <div className="alert alert-success" role="alert">
                        <i className="fa fa-info-circle" /> Success!  Your new profile has been created.  To get started, go to <Link to="/dashboard">My Tennis</Link>.
                    </div>
                }
                <FormInputText fieldName="First Name" callback={this.handleInputChange} value={this.state.model.firstName} errors={this.state.errors}/>
                <FormInputText fieldName="Last Name" callback={this.handleInputChange} value={this.state.model.lastName} errors={this.state.errors} />
                <FormInputSelectAsync fieldName="Home Venue" loadOptions={this.searchVenues} callback={this.handleSelectChangeAsync} value={this.state.model.homeVenue} errors={this.state.errors}>{"Home " + Branding.Venue}</FormInputSelectAsync>
                <FormInputSelectHtml fieldName="Skill Ranking" callback={this.handleSelectChangeHtml} value={this.state.model && this.state.model.skillRanking || 0} optionList={AppConstants.rankings} errors={this.state.errors} />
                <FormInputSelectHtml fieldName="Gender" callback={this.handleSelectChangeHtml} value={genderValue} optionList={AppConstants.genders} errors={this.state.errors} />
                <FormInputText fieldName="Zip Code" callback={this.handleInputChange} value={this.state.model.zipCode || ""} errors={this.state.errors} />
                <FormInputText fieldName="Phone Number" callback={this.handleInputChange} value={this.state.model.phoneNumber || ""} errors={this.state.errors} />
                {
                    //<FormInputSelect fieldName="Member Roles" callback={this.handleSelectChangeMulti} value={this.state.model.memberRoles} options={AppConstants.roles.options} errors={this.state.errors} multi />
                }
                {
                    // this is not working.  for ref, working sandbox example of multi-select:
                    // https://codesandbox.io/s/pkwwq0wlq0
                    //<FormInputSelect fieldName="Member Roles" callback={this.handleSelectChangeMulti} value={this.state.model.memberRoles} options={AppConstants.roles} errors={this.state.errors} multi />
                }
                <FormInputText fieldName="Birth Year" callback={this.handleInputChange} value={birthYearValue} errors={this.state.errors} />
                {
                 //   <FormInputSelect fieldName="Member Roles" callback={this.handleSelectChangeMulti} value={this.state.model.memberRoles} options={AppConstants.roles} errors={this.state.errors} multi />
                 //   <FormInputSelect fieldName="Player Preferences" callback={this.handleSelectChangeMulti} value={this.state.model.playerPreferences} options={AppConstants.playformats} errors={this.state.errors} multi />
                }
                <div className="form-group row">
                    <label  className="col-sm-3 col-form-label"><strong>Notifications</strong></label>
                    <div className="col-sm-9">
                        <NavLink className="form-control inherit-height" to="#profileNotifications" onClick={this.toggleNotifications} data-toggle="collapse" role="button" aria-expanded="false">
                            Click here to edit your notification preferences
                        </NavLink>
                    </div>
                </div>

                <div className={`collapse ${this.state.showNotifications ? 'show' : ''}`} id="profileNotifications">
                    <div className="offset-sm-3 col-sm-9">
                        <p>Notify me when the following types of events occur:</p>
                    </div>
                    <FormInputCheckbox fieldName="NotifyMatchAdded" callback={this.handleInputChange} value={this.state.model.notifyMatchAdded}>
                        New Match Scheduled
                    </FormInputCheckbox>
                    <FormInputCheckbox fieldName="NotifyAddOrRemoveMeFromMatch" callback={this.handleInputChange} value={this.state.model.notifyAddOrRemoveMeFromMatch}>
                        Add/Remove From Match
                    </FormInputCheckbox>
                    <FormInputCheckbox fieldName="NotifyMatchDetailsChangeOrCancelled" callback={this.handleInputChange} value={this.state.model.notifyMatchDetailsChangeOrCancelled}>
                        Match Time/Place Changed
                    </FormInputCheckbox>
                    <FormInputCheckbox fieldName="NotifySubForMatchOpening" callback={this.handleInputChange} value={this.state.model.notifySubForMatchOpening}>
                        Sub Opportunity
                    </FormInputCheckbox>
                    <FormInputCheckbox fieldName="NotifyCourtChange" callback={this.handleInputChange} value={this.state.model.notifyCourtChange}>
                        Court Change
                    </FormInputCheckbox>
                </div>

                <div className="form-group row">
                    <div className="col-sm-3"><strong>Use Court Reservation System</strong></div>
                    <div className="col-sm-9">
                        <div className="form-check align-self-center">
                            <input id="showCourtReservationFields" type="checkbox" className="form-check-input mtp-check" name="showCourtReservationFields" onChange={this.handleReservationCheckboxChange} checked={this.state.showReservationFields} />
                            <label className="form-check-label mtp-label" htmlFor="showCourtReservationFields">
                                Yes
                    </label>
                            <br /><small>Only check this box if you want to auto-reserve outdoor courts.  Requires TennisBookings account.</small>
                        </div>
                    </div>
                </div>

                {this.state.showReservationFields && <div>
                    <div className="margin-bottom">
                        <strong>Optional:</strong> Input your court reservation system credentials. This will allow the MTP site to reserve courts on your behalf, for your matches.  
                        Currently only available for TennisBookings.com system, which is used by Spare Time clubs.  If you know your tennis bookings username and password, enter it below.
                        If you're not sure, <Link to="/reservations">refer to this guide</Link> on getting access set up for online court reservations.
                    </div>
                    <FormInputText fieldName="SpareTime Username" callback={this.handleInputChange} value={this.state.model.spareTimeUsername || ""} errors={this.state.errors} subtext="username used to reserve courts">Tennis Bookings Username</FormInputText>
                    <FormInputText fieldName="SpareTime Password" callback={this.handleInputChange} value={this.state.model.spareTimePassword || ""} errors={this.state.errors} subtext="password used to reserve courts">Tennis Bookings Password</FormInputText>
                  {this.props.appState.member && <div>
                    <div className="form-group row">
                        <label className="col-sm-3 col-form-label"><strong>Test Login:</strong></label>
                        <div className="col-sm-9">
                            <button ref={button => this.openReservationModalButtonElement = button} className="btn btn-secondary" onClick={this.onTestCourtLogin} data-toggle="modal" data-target="#reservationModal">
                                Test Court Reservation Login
                            </button>
                        </div>
                    </div>
                    </div>
                    }
                </div>
                }

                <div hidden className="form-group row authEtc">
                    <label htmlFor="memberImage" className="col-sm-3 col-form-label"><strong>Profile photo</strong></label>
                    <div className="col-sm-9">
                        <input hidden ref={input => this.fileInputElement = input} type="file" name="memberImage" onChange={this.handleFileChange} className="form-control" placeholder="Image path" />
                        {ErrorHandling.getValidationError(this.state.errors, "MemberImage") &&
                            <p className="input-valid-error">{ErrorHandling.getValidationError(this.state.errors, "MemberImage")}</p>
                        }
                    </div>
                </div>
                <button onClick={this.saveChanges} className="btn btn-lg btn-primary btn-block authEtc" type="submit">
                    Save Changes
                    </button>
            </form>
        </div>
    }
}