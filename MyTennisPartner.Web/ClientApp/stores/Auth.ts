export default class Auth {
    static STORAGE_KEY: string = "token";

    static getToken() {
        return window.localStorage.getItem(Auth.STORAGE_KEY);
    }

    static setToken(token: string) {
        window.localStorage.setItem(Auth.STORAGE_KEY, token);
    }

    static removeToken(): void {
        console.log('removing login token');
        window.localStorage.removeItem(Auth.STORAGE_KEY);
    }
}