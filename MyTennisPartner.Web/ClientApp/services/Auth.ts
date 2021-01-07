import AppConstants from '../models/app-constants';
import RestUtilities from './RestUtilities';
import AuthStore from '../stores/Auth';
import { NewUserViewModel, LoginViewModel, ResetPasswordViewModel, ForgotPasswordViewModel, CheckPasswordViewModel, EmailConfirmationViewModel } from "../models/viewmodels/UserViewModels";

interface IAuthResponse {
    token: string;
}

export default class Auth {

    signIn(loginInfo: LoginViewModel) {
        return RestUtilities.post<IAuthResponse>(`${AppConstants.loginUrl}?email=${loginInfo.email}`, loginInfo)
            .then((response) => {
                if (!response.is_error) {
                    this.setToken(response.content && response.content.token);
                }
                return response;
            });
    }

    register(newUser: NewUserViewModel) {
        // add ?name parameter solely to assist with App Insights and other logging, as it will make it easy to see who is having trouble
        // in the event of registration errors
        return RestUtilities.post<IAuthResponse>(`${AppConstants.registerUrl}?name=${newUser.firstName}_${newUser.lastName}`, newUser)
            .then((response) => {
                if (!response.is_error) {
                    this.setToken(response.content && response.content.token);
                }
                return response;
            });
    }

    forgot(forgotPasswordViewModel: ForgotPasswordViewModel) {
        return RestUtilities.post<IAuthResponse>(AppConstants.forgotPasswordUrl, forgotPasswordViewModel)
            .then((response) => {
                return response;
            });
    }

    reset(resetPasswordViewModel: ResetPasswordViewModel) {
        return RestUtilities.post<IAuthResponse>(AppConstants.resetPasswordUrl, resetPasswordViewModel)
            .then((response) => {
                return response;
            });
    }

     signOut(): void {
        AuthStore.removeToken();
    }

    getUserInfo() {
        return RestUtilities.get(AppConstants.getUserInfoUrl)
            .then((response) => {
                return response.content;
            }).catch((err) => {
                return null;
            })
    }

    setToken(token?: string) {
        AuthStore.setToken(token || 'empty token');
    }

    checkPassword(checkPasswordViewModel: CheckPasswordViewModel) {
        return RestUtilities.post<IAuthResponse>(AppConstants.checkPasswordUrl, checkPasswordViewModel)
            .then((response) => {
                return response;
            })
    }

    requestEmailConfirmation(emailConfirmationViewModel: EmailConfirmationViewModel) {
        return RestUtilities.post<IAuthResponse>(AppConstants.sendConfirmEmailUrl, emailConfirmationViewModel)
            .then((response) => {
                return response;
            })
    }
}