import { asm } from "./asm";

describe('asm', () => {
    it('converts int16 to asm', () => {
        expect(asm(0x0000)).toBe('@0');
        expect(asm(12)).toBe('@12');

        expect(asm(0b1110_101010_000_000)).toBe('0');
        expect(asm(0b1111_110000_010_000)).toBe('D=M');
        expect(asm(0b1110_001110_010_101)).toBe('D=D-1;JNE');
        expect(asm(0b1110_101010_000_111)).toBe('0;JMP');
        expect(asm(0b1111_110010_011_000)).toBe('MD=M-1');
    });
});