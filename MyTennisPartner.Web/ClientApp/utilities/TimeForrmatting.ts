import * as moment from "moment";
import NumberFormatting from "./NumberFormatting";

export default class TimeFormatting {
    /**
     * method to convert time string into display-friendly time, e.g. "18:00" => "6:00 pm"
     * @param input
     */
    static toDisplayTime(input: string) {
        if (!input) {
            return "unknown time";
        }

        let strArray = input.split(":");
        if (strArray.length < 2) {
            return "unknown time";
        }

        let hrs = strArray[0];
        let minutes = strArray[1];
        let displayHours = parseInt(hrs) % 12;
        let ampm = parseInt(hrs) < 13 ? "AM" : "PM";
        let result = `${displayHours}:${minutes} ${ampm}`;
        return result;
    }

    static utcDateToLocalMoment(utcDate: Date) {
        return moment(moment.utc(utcDate).local());
    }

    // converts time string like "14:00" from UTC to local time ("07:00" if GMT-7 is local zone)
    static utcToLocalTimeString(utcTimeString: string) {
        if (!utcTimeString) return "";
        if (!utcTimeString.includes(":")) {
            // invalid input, return input string
            return utcTimeString;
        }
        let UtcOffsetMinutes = moment().utcOffset();
        let hrUtc = parseInt(utcTimeString.split(":")[0]);
        let minUtc = parseInt(utcTimeString.split(":")[1]);
        let totalMinutesUtc = 60 * hrUtc + minUtc;
        let totalMinutesLocal = totalMinutesUtc + UtcOffsetMinutes;
        if (totalMinutesLocal < 0) {
            totalMinutesLocal += 24 * 60;
        }
        let hrsLocal = Math.floor(totalMinutesLocal / 60) % 24;
        let minLocal = totalMinutesLocal % 60;
        let localTimeString = `${NumberFormatting.TwoDigitInteger(hrsLocal)}:${NumberFormatting.TwoDigitInteger(minLocal)}`;
        return localTimeString;
    }

    // convert local time string ("08:00") to UTC time string ("15:00" if GMT-7 time zone)
    static localToUtcTimeString(localTimeString: string): string {
        if (!localTimeString) return "";
        if (!localTimeString.includes(":")) {
            // invalid input, return input string
            return localTimeString;
        }
        let UtcOffsetMinutes = moment().utcOffset();
        let hrsLocal = parseInt(localTimeString.split(":")[0]);
        let minLocal = parseInt(localTimeString.split(":")[1]);
        let totalMinutesLocal = 60 * hrsLocal + minLocal;
        let totalMinutesUtc = totalMinutesLocal - UtcOffsetMinutes;
        if (totalMinutesUtc < 0) {
            totalMinutesUtc += 24 * 60;
        }
        let hrsUtc = Math.floor(totalMinutesUtc / 60) % 24;
        let minUtc = totalMinutesUtc % 60;
        let utcTimeString = `${NumberFormatting.TwoDigitInteger(hrsUtc)}:${NumberFormatting.TwoDigitInteger(minUtc)}`;
        return utcTimeString;
    }

    static localTimeStringToDisplayString(localTime: string) {
        //let displayString = moment(localTime).format("ddd, MMM Do, h:mm a");
        let displayString = moment(localTime).format("ddd M/D, h:mma");
        return displayString;
    }
}
