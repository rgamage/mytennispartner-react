export class UserInfo {
    userName: string;
    isLoggedIn: boolean;
    firstName: string;
    lastName: string;
    userId: string;
    isAdmin: boolean;
}

export class NewUserViewModel {
    firstName: string;
    lastName: string;
    username: string;
    password: string;
}

export class LoginViewModel {
    email: string;
    password: string;
    rememberMe: boolean;
    keepMeLoggedIn: boolean;
}

export class ForgotPasswordViewModel {
    email: string;
}

export class ResetPasswordViewModel {
    email: string;
    password: string;
    confirmPassword: string;
    code: string;
}

export class CheckPasswordViewModel {
    email: string;
    password: string;
}

export class EmailConfirmationViewModel {
    email: string;
}

export class MyAccountViewModel {
    email: string;
    firstName: string;
    lastName: string;
    [key: string]: string;  // add "| boolean" for example if model has other types, or "any"
}
