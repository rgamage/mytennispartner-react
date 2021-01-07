import * as React from "react";
import { Async } from 'react-select';
import SelectOption, { SelectOptionList } from '../../models/SelectOption';
import TextFormatting from "../../utilities/TextFormatting";
import { IErrorResponse } from "../../utilities/ErrorHandling";
import ValidationMessage from "./ValidationMessage";

interface FormInputSelectAsyncProps {
    fieldName: string;
    callback: (propName: string) => (selectedOption: SelectOption | null) => void;
    value: SelectOption;
    errors: IErrorResponse;
    children?: any;
    loadOptions: (search: string) => Promise<SelectOption[]>;
    disabled?: boolean;
}

export default function FormInputSelectAsync(props: FormInputSelectAsyncProps) {

    let serverFieldName = TextFormatting.removeAllSpaces(props.fieldName);
    let clientFieldName = TextFormatting.toCamelCase(serverFieldName);
    let label = props.children || props.fieldName;

    return <div className="form-group row">
        <label htmlFor={clientFieldName} className="col-sm-3 col-form-label"><strong>{label}</strong></label>
        <div className="col-sm-9">
            <Async
                name={clientFieldName} 
                value={props.value}
                onChange={props.callback(clientFieldName)}
                loadOptions={props.loadOptions}
                blurInputOnSelect
                isDisabled={props.disabled}
                defaultOptions  //loads list of options before user types, if true
                cacheOptions={false}
            />
            <ValidationMessage errors={props.errors} fieldName={serverFieldName} />
        </div>
    </div >
}
