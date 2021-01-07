import * as React from "react";
import TextFormatting from "../../utilities/TextFormatting";

interface CheckboxInputProps {
    fieldName: string;
    callback: (event: React.FormEvent<HTMLInputElement>) => void;
    value: string | number | boolean;
    children?: any;
    subtext?: string;
    readonly?: boolean;
}

export default function FormInputCheckbox(props: CheckboxInputProps) {

        // note - add "form-check-input" class to checkbox controls, and perhaps a fixed width?

    let serverFieldName = TextFormatting.removeAllSpaces(props.fieldName);
    let clientFieldName = TextFormatting.toCamelCase(serverFieldName);
    let label = props.children || props.fieldName;
   
    return <div className="form-group row">
            <div className="col-sm-3"><strong>{label}</strong></div>
            <div className="col-sm-9">
            <div className="form-check align-self-center">
                <input id={clientFieldName} type="checkbox" disabled={props.readonly} className="form-check-input mtp-check" name={clientFieldName} onChange={props.callback} value={props.value as string | number} checked={props.value as boolean} placeholder={props.fieldName} />
                <label className="form-check-label mtp-label" htmlFor={clientFieldName}>
                        Yes
                    </label>
                    <br /><small>{props.subtext}</small>
                </div>
            </div>
        </div>
}
