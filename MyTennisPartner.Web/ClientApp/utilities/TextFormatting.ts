export default class TextFormatting {

    // function to remove spaces
    static removeAllSpaces(input: string) {
        let result = input.replace(new RegExp(" ", 'g'), "");
        return result;
    }

    static toCamelCase(input: string) {
        let result = input.charAt(0).toLowerCase() + input.slice(1);
        return result;
    }
}
