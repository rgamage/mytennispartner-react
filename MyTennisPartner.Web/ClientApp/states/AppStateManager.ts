import { AppState, AppProps } from '../components/App';
import { UserInfo } from '../models/viewmodels/UserViewModels';
import MemberViewModel from '../models/viewmodels/MemberViewModel';

export default class AppStateManager {

    // this function is outside of the class, and can be kept in an external, easily-testable library
    //setLoginState = (isLoggedIn: boolean, displayName: string) => {
    //    return (state: AppState, props: AppProps) => {
    //        return { isLoggedIn: isLoggedIn, userDisplayName: displayName };
    //    }
    //}

    // this function is outside of the class, and can be kept in an external, easily-testable library
    setUserInfo = (user: UserInfo | null) => {
        return function setAppUser(state: AppState, props: AppProps) {
            return {
                user: user
            };
        }
    }
    // this function is outside of the class, and can be kept in an external, easily-testable library
    setMemberInfo = (member: MemberViewModel | null) => {
        return function setAppUser(state: AppState, props: AppProps) {
            return {
                member: member
            };
        }
    }
}