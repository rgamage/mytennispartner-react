import * as React from "react";
import TextFormatting from "../../utilities/TextFormatting";
import ErrorHandling, { IErrorResponse } from "../../utilities/ErrorHandling";
import ValidationMessage from "./ValidationMessage";

interface TextInputProps {
    fieldName: string;
    callback: (event: React.FormEvent<HTMLInputElement>) => void;
    value: string | number | boolean;
    errors: IErrorResponse;
    children?: any;
    subtext?: string;
    number?: boolean;
}

export default function FormInputText(props: TextInputProps) {

    let serverFieldName = TextFormatting.removeAllSpaces(props.fieldName);
    let clientFieldName = TextFormatting.toCamelCase(serverFieldName);
    let type = props.number ? "number" : clientFieldName.startsWith("email") ? "email" : clientFieldName.startsWith("password") || clientFieldName.endsWith("Password") ? "password" : "text";
    let label = props.children || props.fieldName;

        return <div className="form-group row">
            <label htmlFor={clientFieldName} className="col-sm-3 col-form-label"><strong>{label}</strong></label>
            <div className="col-sm-9">
                <input type={type} name={clientFieldName} onChange={props.callback} value={props.value as string | number} className="form-control" placeholder={label} />
                <small>{props.subtext}</small>
                <ValidationMessage errors={props.errors} fieldName={serverFieldName} />
            </div>
        </div>
}
