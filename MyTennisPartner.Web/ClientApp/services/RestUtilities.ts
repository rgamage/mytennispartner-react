import AuthStore from '../stores/Auth';
import { IBadRequestResponse, IErrorResponse } from "../utilities/ErrorHandling";
import Notification from "../utilities/Notification";

const StatusCodes = {
    Success: 200,
    BadRequest: 400,
    NotFound: 404,
    InternalServerError: 500
}

export interface IApiResponse<T> {
    statusCode: number;  // e.g. 200, 404, 500, etc
    data?: T;          // in case of successful response, holds model/object data
    message?: string;    // holds error message, in case of some errors
    errors?: IErrorResponse;  // holds list of modelstate validation errors
}

export class RestResponse<T> {
    constructor(response: IApiResponse<T>, statusCode: number) {
        this.statusCode = response.statusCode || statusCode;
        this.is_error = this.statusCode >= 400;
        this.is_not_found = this.statusCode == 404;
        this.is_bad_request = this.statusCode == 400;
        if (!this.is_error) {
            if ((response as Object).hasOwnProperty("data")) {
                this.content = response.data;
            }
            else {
                this.content = response as any;
            }
        }
        else {
            this.error_content = response.errors;
        }
    }

    statusCode: number;
    is_error?: boolean;
    error_content?: IErrorResponse;
    is_not_found?: boolean;
    is_bad_request?: boolean;
    content?: T;
};

export default class RestUtilities {

    static get<T>(url: string): Promise<RestResponse<T>> {
        return RestUtilities.request<T>('GET', url);
    }

    static delete(url: string): Promise<RestResponse<void>> {
        return RestUtilities.request<void>('DELETE', url);
    }

    static put<T>(url: string, data: Object | string): Promise<RestResponse<T>> {
        return RestUtilities.request<T>('PUT', url, data);
    }

    static post<T>(url: string, data: Object | string): Promise<RestResponse<T>> {
        return RestUtilities.request<T>('POST', url, data);
    }

    static postFile<T>(url: string, data: Object | string): Promise<RestResponse<T>> {
        return RestUtilities.request<T>('POST', url, data, true);
    }

    private static request<T>(method: string, url: string, data?: any, fileData: boolean = false): Promise<RestResponse<T>> {
        let responseStatus: number = 200;
        //let isBadRequest = false;
        //let isNotFound = false;
        //let isError = false;
        let body = data;
        let headers: any = {
            'Authorization': `Bearer ${AuthStore.getToken()}`,
            'Accept': 'application/json'
        };

        if (data) {
            if ((typeof data === 'object')) {
                if (!fileData) {
                    headers['Content-Type'] = 'application/json';
                }
                body = fileData ? data : JSON.stringify(data);
            } else {
                headers['Content-Type'] = 'application/x-www-form-urlencoded';
            }
        }

        return fetch(url, {
            method: method,
            headers: headers,
            body: body
            //credentials: "include"
        }).then((response: any) => {
            if (response.status == 401) {
                // Unauthorized; redirect to sign-in
                AuthStore.removeToken();
                window.location.replace(`/signin?expired=1&returnUrl=${localStorage.getItem("lastPath")}`);
            }
            responseStatus = response.status;;
            //isError = response.status >= 400;
            //isBadRequest = response.status == StatusCodes.BadRequest;
            //isNotFound = response.status == StatusCodes.NotFound;

            let responseContentType = response.headers.get("content-type");
            if (responseContentType && responseContentType.indexOf("application/json") !== -1) {
                return response.json();
            } else {
                return response.text();
            }
        }).then((responseContent: any) => {
            /* handle various possibilities of responses
                200 - Return response.data if it exists (our ApiResult std), or just response if not
                400 - Return response.errors, which contains our ApiResult for modelstate validation errors (array of BadRequestResponses)
                404 - Return reponse.message, containing Not Found message
                500 - Return reponse.message, containing internal server error message
            */
            let response = new RestResponse<T>(responseContent, responseStatus);
            if (response.statusCode >= 500) {
                //ToDo: add global error message mechanism
                //Notification.notifyError(response.error_content.errorMessage);
            }
            return response;
        });
    }
}