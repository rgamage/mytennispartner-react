import * as React from "react";
import { Link, Redirect, RouteComponentProps } from 'react-router-dom';
import { AppStatePropsRoute } from "../models/AppStateProps";
import { MyAccountViewModel } from "../models/viewmodels/UserViewModels";
import AccountService from "../services/AccountService";
import FormInputText from "./Common/FormInputText";
import Notification from "../utilities/Notification";
import ErrorHandling, { IErrorResponse } from "../utilities/ErrorHandling";

let accountService = new AccountService();

interface MyAccountStates {
    model: MyAccountViewModel;
    success: boolean;
    errors?: IErrorResponse;
}

export class MyAccount extends React.Component<AppStatePropsRoute, MyAccountStates> {
    constructor(props: AppStatePropsRoute) {
        super(props);
        //console.log("My Account constructor started");
        let a = new MyAccountViewModel();
        if (props.appState.user) {
            a.email = props.appState.user.userName;
            a.firstName = props.appState.user.firstName;
            a.lastName = props.appState.user.lastName;
        }
        this.state = {
            model: a,
            success: false,
            errors: null
        };
    }

    // function to handle input field changes, set corresponding state variables
    handleInputChange = (event: any) => {
        const target = event.target;
        const value = target.type === 'checkbox' ? target.checked : target.value;
        const name = target.name;
        let model = this.state.model;
        model[name] = value;

        this.setState({
            model: model
        });
    }

    saveChanges = (event: any) => {
        event.preventDefault();
        this.setState({ errors: null, success: false });
        accountService.updateAccount(this.state.model).then((response) => {
            if (!response.is_error) {
                //console.log("account updated");
                this.props.app.setState({ hasUpdatedUserInfo: true });
                this.setState({ success: true });
                Notification.notifySuccess();
            }
            else {
                console.log("ERROR updating account");
                this.setState({ errors: response.error_content });
                Notification.notifyError(response.error_content.errorMessage);
           }
        });
    }

    render() {
        let generalError = ErrorHandling.getValidationErrorGeneral(this.state.errors);
        return <div className="container my-account">
            <h2>My Account Info</h2>
            <p>Edit your account details here</p>
            {generalError &&
                <div className="alert alert-danger" role="alert">
                    <p><i className="fa fa-cross" />{generalError}</p>
                </div>
            }
            <form>
                <FormInputText fieldName="Email" value={this.state.model.email} callback={this.handleInputChange} errors={this.state.errors} />
                <p>If you'd like to change your password, <Link to='/forgot'>Click Here</Link></p>
                <FormInputText fieldName="First Name" value={this.state.model.firstName} callback={this.handleInputChange} errors={this.state.errors} />
                <FormInputText fieldName="Last Name" value={this.state.model.lastName} callback={this.handleInputChange} errors={this.state.errors} />
                <button onClick={this.saveChanges} className="btn btn-lg btn-primary btn-block authEtc" type="submit">Save Changes</button>
            </form>
            </div>
    }
}