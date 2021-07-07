export const assert = (condition: boolean, message = "Assert failed"): void|never => {
    if (!condition) throw new Error(message);
    return;
}