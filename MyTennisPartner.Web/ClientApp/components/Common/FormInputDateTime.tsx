import * as React from "react";
import TextFormatting from "../../utilities/TextFormatting";
import AppConstants from "../../models/app-constants";
import DateCalculations from "../../utilities/DateCalculations";
import * as moment from "moment";

interface FormInputDateTimeProps {
    fieldName: string;
    value: moment.Moment;
    callback: (name: string, dateTime: moment.Moment) => void;
}

interface DateModel {
    month: number;
    day: number;
    year: number;
    time: string;
}

interface FormInputDateTimeState {
   //model: DateModel;
}

export default class FormInputDateTime extends React.Component<FormInputDateTimeProps, FormInputDateTimeState>  {
    constructor(props: FormInputDateTimeProps) {
        super(props);
    }

    populateModel = (valueMoment: moment.Moment): DateModel => {
        let model = {
            month: valueMoment.month(),
            day: valueMoment.date(),
            year: valueMoment.year(),
            time: valueMoment.format("HH:mm")
        }
        return model;
    }

    handleSelectChangeHtml = (event: React.ChangeEvent<HTMLSelectElement>) => {
        let model = this.populateModel(this.props.value);
        //console.log(`In handler, first fetch: model time = ${model.time}, value = ${this.props.value.toString()}`);
        let name = event.target.name;
        let value = event.target.value;
        model[name] = value;
        //console.log(`In handler, after update: model time = ${model.time}`);
        let hours = parseInt(model.time.substr(0, 2));
        let minutes = parseInt(model.time.substr(3, 2));
        //console.log(`In handler, hours = ${hours}, minutes = ${minutes}`);
        let selectedDate = new Date(model.year, model.month, model.day, hours, minutes);
        //console.log(`new selectedDate = ${selectedDate.toString()}`);
        //console.log(`model.month = ${model.month}`);
        let selectedDateMoment = moment(selectedDate);
        if (moment().isAfter(selectedDateMoment)) {
            // user selected a time before now, so assume they mean next year
            selectedDateMoment.year(moment().year() + 1);
        }
        else {
            // else just assume current year
            selectedDateMoment.year(moment().year());
        }
        this.props.callback(this.props.fieldName, selectedDateMoment);
    }

    render() {
        let serverFieldName = TextFormatting.removeAllSpaces(this.props.fieldName);
        let clientFieldName = TextFormatting.toCamelCase(serverFieldName);
        let label = this.props.children || this.props.fieldName;
        let model = this.populateModel(this.props.value);
        return <div className="mtp-datetime-control">
            <div className="form-group row">
                <label htmlFor={clientFieldName} className="col-12 col-sm-3 col-form-label"><strong>{label}</strong></label>
                <div className="col-4 col-sm-3">
                    <select className="form-control" value={model.time} onChange={this.handleSelectChangeHtml} name="time">
                        {AppConstants.timeOfDay.options.map(opt =>
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        )}
                    </select>
                </div>
                <div className="col-3 col-sm-3">
                    <select className="form-control" value={model.month} onChange={this.handleSelectChangeHtml} name="month">
                        {AppConstants.months.options.map(opt =>
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        )}
                    </select>
                </div>
                <div className="col-3 col-sm-2">
                    <select className="form-control" value={model.day} onChange={this.handleSelectChangeHtml} name="day">
                        {DateCalculations.getDaysInMonth(model.month).map(opt =>
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        )}
                    </select>
                </div>
                <div className="col-1 align-self-center">
                    {model.year}
                </div>
                {model.year > moment().year() &&
                    <div className="col-12 col-sm-9 offset-sm-3">
                        <small><i className="fa fa-warning" />&nbsp;Note: you have selected a date in the next year</small>
                    </div>
                }
            </div>
            
        </div>
    }
}
