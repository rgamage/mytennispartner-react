import * as React from "react";
import Select from 'react-select';
import SelectOption, { SelectOptionList } from '../../models/SelectOption';
import TextFormatting from "../../utilities/TextFormatting";
import { IErrorResponse } from "../../utilities/ErrorHandling";
import ValidationMessage from "./ValidationMessage";

interface FormInputSelectProps {
    fieldName: string;
    callback: (propName: string) => (selectedOption: SelectOption[] | null) => void;
    value: SelectOption[];
    errors: IErrorResponse;
    options: any;
    multi?: boolean;
    children?: any;
}

export default function FormInputSelect(props: FormInputSelectProps) {

    let serverFieldName = TextFormatting.removeAllSpaces(props.fieldName);
    let clientFieldName = TextFormatting.toCamelCase(serverFieldName);
    let label = props.children || props.fieldName;

    return <div className="form-group row">
        <label htmlFor={clientFieldName} className="col-sm-3 col-form-label"><strong>{label}</strong></label>
        <div className="col-sm-9">
            <Select
                name={clientFieldName}
                value={props.value}
                onChange={props.callback(clientFieldName)}
                options={props.options}
                isMulti={props.multi}
                blurInputOnSelect
            />
            <ValidationMessage errors={props.errors} fieldName={serverFieldName} />
        </div>
    </div >
}
