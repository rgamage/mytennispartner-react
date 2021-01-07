export default class ArrayHelper {

    // check if object is an array with one or more elements
    static isNonEmptyArray(array: object) {
        if (Array.isArray(array) && array.length) {
            return true;
        }
        return false;
    }

    // get first element of an array, or return null if doesn't exist
    static firstOrNull<T>(array: T[]): T {
        if (this.isNonEmptyArray(array as Object)) {
            return array[0] as T;
        }
        return null as T;
    }

}