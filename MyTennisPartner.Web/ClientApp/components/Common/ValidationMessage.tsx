import * as React from "react";
import ErrorHandling, { IErrorResponse } from "../../utilities/ErrorHandling";

interface ValidationMessageProps {
    errors: IErrorResponse;
    fieldName: string;
}

export default function ValidationMessage(props: ValidationMessageProps) {
    if (props.errors) {
        let errorMessage = ErrorHandling.getValidationError(props.errors, props.fieldName);
        if (errorMessage) {
            return <div>
                {errorMessage &&
                <p className="input-valid-error">{errorMessage}</p>
                }
            </div>
        }
    }
    return null;
}
