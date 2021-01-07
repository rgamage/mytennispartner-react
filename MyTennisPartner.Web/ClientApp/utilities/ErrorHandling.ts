import * as React from "react";
import ArrayHelper from "./ArrayHelper";

export interface IBadRequestResponse {
    field: string;
    messages: string[];
    [key: string]: string | string[];
}

export interface IErrorResponse {
    errorMessage?: string;
    validationErrors?: IBadRequestResponse[];
}

export default class ErrorHandling {

    // image link is broken, so show placeholder instead of broken link
    static handleImageError = (event: React.SyntheticEvent<HTMLImageElement>) => {
        event.currentTarget.src = "/images/profile-placeholder.png";
    }

    // get ModelState validation message by field name
    static getValidationError = (errors: IErrorResponse, field: string): string => {
        if (errors && errors.validationErrors != null) {
            let validationErrors = errors.validationErrors.filter(e => e.field == field);
            if (ArrayHelper.isNonEmptyArray(validationErrors)) {
                let message = validationErrors[0].messages[0];
                return message;
            }
        }
        return null;
    }

    // get "General" validation errors, or ModelState errors with field/property = "" (applies to all, or no property in particular)
    static getValidationErrorGeneral = (errors: IErrorResponse): string => {
        if (errors && errors.validationErrors != null) {
            let validationErrors = errors.validationErrors.filter(e => e.field == "");
            if (ArrayHelper.isNonEmptyArray(validationErrors)) {
                let message = validationErrors[0].messages[0];
                return message;
            }
        }
        return null;
    }
}
