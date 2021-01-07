import SelectOption from "../models/SelectOption";

export default class DateCalculations {
    // return list of numbers representing days in month, for drop-down to select day of month
    public static getDaysInMonth(month: number): SelectOption[] {
        let numDays = 0;
        switch (month) {
            case 0:
            case 2:
            case 4:
            case 6:
            case 7:
            case 9:
            case 11:
                numDays = 31;
                break;
            case 1: numDays = 29;
                break;
            default: numDays = 30;
        }
        let days = [];
        for (var i = 1; i <= numDays; i++) {
            let x = new SelectOption();
            x.value = i;
            x.label = i.toString();
            days.push(x);
        }
        return days;
    }

}