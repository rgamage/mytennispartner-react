//import * as React from "react";
//import Select, { Option, OptionValues } from 'react-select';
//import SelectOption from '../../models/SelectOption';
//import TextFormatting from "../../utilities/TextFormatting";
//import { AmPm } from "../../models/ViewModels/Enums";
//import AppConstants from "../../models/app-constants";
//import { TimeOfDay } from "../../models/viewmodels/TimeOfDay";

//interface FormInputTimeProps {
//    fieldName: string;
//    callback: (selectedTimeOfDay: TimeOfDay | null) => void;
//    value: TimeOfDay;
//    children?: any;
//}

//interface FormInputTimeState {
//    selectedTimeOfDay: TimeOfDay;
//    serverFieldName: string;
//    clientFieldName: string;
//    label: string;
//}

///**
// * Component to provide a user input control for time of day
// * @param props
// */
//export default class FormInputTime extends React.Component<FormInputTimeProps, FormInputTimeState> {
//    constructor(props: FormInputTimeProps) {
//        super();
//        let serverFieldName = TextFormatting.removeAllSpaces(props.fieldName);
//        this.state = {
//            selectedTimeOfDay: props.value,
//            serverFieldName: TextFormatting.removeAllSpaces(props.fieldName),
//            clientFieldName: TextFormatting.toCamelCase(serverFieldName),
//            label: props.children || props.fieldName
//        }
//    }

//    handleSelectChange = (propName: string) => (selectedOption: Option<OptionValues> | null) => {
//        let time = this.state.selectedTimeOfDay;
//        time[propName] = ((selectedOption && selectedOption.value) as number) || 0;
//        this.setState({ selectedTimeOfDay: time });
//        console.log(`FormInputTime: Selected: ${selectedOption && selectedOption.label}`);
//    }

//    render() {
//        return <div className="form-group">
//          <div className="form-inline row">
//            <label htmlFor={this.state.clientFieldName} className="col-sm-3 col-form-label"><strong>{this.state.label}</strong></label>
//            <div className="col-sm-9">
//                <Select
//                    className="form-control"
//                    name={this.state.clientFieldName + "hours"}
//                    value={this.state.selectedTimeOfDay.hours}
//                    onChange={this.handleSelectChange("hours")}
//                    options={AppConstants.hours}
//                />
//                <Select
//                    className="form-control"
//                    name={this.state.clientFieldName + "minutes"}
//                    value={this.state.selectedTimeOfDay.minutes}
//                    onChange={this.handleSelectChange("minutes")}
//                    options={AppConstants.minutes}
//                />
//                <Select
//                    className="form-control"
//                    name={this.state.clientFieldName + "ampm"}
//                    value={this.state.selectedTimeOfDay.ampm}
//                    onChange={this.handleSelectChange("ampm")}
//                    options={AppConstants.ampm}
//                />
//            </div>
//          </div>
//        </div >
//    }
//}
