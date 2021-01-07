import * as React from "react";
import SelectOption, { SelectOptionList } from '../../models/SelectOption';
import TextFormatting from "../../utilities/TextFormatting";
import { IErrorResponse } from "../../utilities/ErrorHandling";
import ValidationMessage from "./ValidationMessage";

interface FormInputSelectProps {
    fieldName: string;
    callback: (event: React.ChangeEvent<HTMLSelectElement>) => void;
    value: number | string | string[];
    errors: IErrorResponse;
    optionList: SelectOptionList;
    multi?: boolean;
    children?: any;
}

export default function FormInputSelectHtml(props: FormInputSelectProps) {

    let serverFieldName = TextFormatting.removeAllSpaces(props.fieldName);
    let clientFieldName = TextFormatting.toCamelCase(serverFieldName);
    let label = props.children || props.fieldName;

    return <div className="form-group row">
        <label htmlFor={clientFieldName} className="col-sm-3 col-form-label"><strong>{label}</strong></label>
        <div className="col-sm-9">
            <select className="form-control" value={props.value} onChange={props.callback} name={clientFieldName}>
                {props.optionList.options.map(opt =>
                    <option key={opt.value} value={opt.value}>{opt.label}</option>
                )}
            </select>
            <ValidationMessage errors={props.errors} fieldName={serverFieldName} />
        </div>
    </div >
}
