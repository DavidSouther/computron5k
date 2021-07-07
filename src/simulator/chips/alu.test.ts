import { alu, COMMANDS, Flags } from "./alu";

describe('alu', () => {
    it('calculates', () => {
        expect(alu(COMMANDS.asm['0'], 123, 456)).toEqual([0, Flags.Zero]);
        expect(alu(COMMANDS.asm['D+A'], 123, 456)).toEqual([579, Flags.Positive]);
        expect(alu(COMMANDS.asm['D-A'], 123, 456)).toEqual([-333 & 0xffff, Flags.Negative]);
        expect(alu(COMMANDS.asm['A-D'], 123, 456)).toEqual([333, Flags.Positive]);
        expect(alu(COMMANDS.asm['D&A'], 0b1010, 0b1101)).toEqual([0b1000, Flags.Positive]);
        expect(alu(COMMANDS.asm['D|A'], 0b1010, 0b1101)).toEqual([0b1111, Flags.Positive]);
    });
});