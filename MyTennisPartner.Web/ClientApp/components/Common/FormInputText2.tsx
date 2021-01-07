import * as React from "react";
import TextFormatting from "../../utilities/TextFormatting";
import ErrorHandling, { IErrorResponse } from "../../utilities/ErrorHandling";
import ValidationMessage from "./ValidationMessage";

interface TextInputProps {
    fieldName: string;
    callback: (index: number) => (event: React.FormEvent<HTMLInputElement>) => void;
    value: string | number | boolean;
    errors: IErrorResponse;
    children?: any;
    subtext?: string;
    number?: boolean;
    index?: number;
}

export default function FormInputText2(props: TextInputProps) {

    let serverFieldName = TextFormatting.removeAllSpaces(props.fieldName);
    let clientFieldName = TextFormatting.toCamelCase(serverFieldName);
    let type = props.number ? "number" : clientFieldName.startsWith("email") ? "email" : clientFieldName.startsWith("password") ? "password" : "text";
    let label = props.children || props.fieldName;

        return <div>
                <input type={type} name={clientFieldName} onChange={props.callback(props.index)} value={props.value as string | number} className="form-control" placeholder={label} />
                <small>{props.subtext}</small>
                <ValidationMessage errors={props.errors} fieldName={serverFieldName} />
            </div>
}
