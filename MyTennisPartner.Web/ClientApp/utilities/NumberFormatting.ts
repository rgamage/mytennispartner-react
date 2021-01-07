export default class NumberFormatting {

    // function to format positive integer to two digits, e.g. 4 => "04", 25 => "25", 0 => "00", etc.
    static TwoDigitInteger(n: number): string {
        return n > 9 ? "" + n : "0" + n;
    }

    static OneDigitAfterDecimal(n: number): string {
        return Number(n).toFixed(1);
    }

    // this is how you create a GUID in javascript, apparently.
    // stolen from this Stack Overflow question
    // https://stackoverflow.com/questions/105034/create-guid-uuid-in-javascript
    static guid(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
}
