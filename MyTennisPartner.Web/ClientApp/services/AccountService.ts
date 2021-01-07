import AppConstants from '../models/app-constants';
import RestUtilities from './RestUtilities';
import AuthStore from '../stores/Auth';
import { UserInfo, MyAccountViewModel } from '../models/viewmodels/UserViewModels'

//import 'es6-promise'
//import 'isomorphic-fetch';

interface IAccountResponse {
    result: MyAccountViewModel;
}

export default class AccountService {

    updateAccount(model: MyAccountViewModel) {
        return RestUtilities.put<IAccountResponse>(AppConstants.updateAccountInfo, model)
            .then((response) => {
                return response;
            });
    }
}