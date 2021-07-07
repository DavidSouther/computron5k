import { bin, dec, hex, int10, int16, int2 } from "./twos";

describe('twos', () => {
    it('formats as base 16', () => {
        expect(bin(0)).toBe('0000 0000 0000 0000');
        expect(bin(1)).toBe('0000 0000 0000 0001');
        expect(bin(-1)).toBe('1111 1111 1111 1111');
        expect(bin(256)).toBe('0000 0001 0000 0000');

        expect(dec(0)).toBe('0');
        expect(dec(1)).toBe('1');
        expect(dec(-1)).toBe('-1');
        expect(dec(256)).toBe('256');

        expect(hex(0)).toBe('0x0000');
        expect(hex(1)).toBe('0x0001');
        expect(hex(-1)).toBe('0xFFFF');
        expect(hex(256)).toBe('0x0100');
    });

    it('parses to integer', () => {
        expect(int2('0000000000000000')).toBe(0);
        expect(int2('0000000000000001')).toBe(1);
        expect(int2('1111111111111111')).toBe(-1);
        expect(int2('0000000100000000')).toBe(256);
        expect(int2('0000 0000 0000 0000')).toBe(0);
        expect(int2('0000 0000 0000 0001')).toBe(1);
        expect(int2('1111 1111 1111 1111')).toBe(-1);
        expect(int2('0000 0001 0000 0000')).toBe(256);

        expect(int10('0')).toBe(0);
        expect(int10('1')).toBe(1);
        expect(int10('-1')).toBe(-1);
        expect(int10('256')).toBe(256);

        expect(int16('0x0000')).toBe(0);
        expect(int16('0x0001')).toBe(1);
        expect(int16('0xffff')).toBe(-1);
        expect(int16('0xFFFF')).toBe(-1);
        expect(int16('0x0100')).toBe(256);
    });
});