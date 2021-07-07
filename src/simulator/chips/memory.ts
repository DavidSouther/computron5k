import { assert } from "../../util/assert";

export class Memory {
    private memory: Int16Array;

    get size(): number {
        return this.memory.length;
    }

    constructor(memory: ArrayBuffer|number) {
        if (typeof memory === 'number') {
            this.memory = new Int16Array(memory);
        } else {
            this.memory = new Int16Array(memory);
        }
    }

    get(index: number): number {
        if (index < 0 || index >= this.size) return 0xffff;
        return this.memory[index];
    }

    set(index: number, value: number) {
        if (index < 0 || index >= this.size) return 0xffff;
        this.memory[index] = value & 0xffff;
    }

    *map<T>(fn: (index: number, value: number) => T, start = 0, end = this.size): Iterable<T> {
        assert(start < end);
        for(let i = start; i < end; i++) {
            yield fn(i, this.get(i));
        }
    }
}
