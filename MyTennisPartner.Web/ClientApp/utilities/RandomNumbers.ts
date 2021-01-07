export default class RandomNumbers {
    static SeededRandom = (seed: number) => {
        return parseFloat(('0.' + Math.sin(seed).toString().substr(6)));
    }
}