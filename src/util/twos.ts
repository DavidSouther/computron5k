enum Hex {
    _0 = 0,
    _1 = 1,
    _2 = 2,
    _3 = 3,
    _4 = 4,
    _5 = 5,
    _6 = 6,
    _7 = 7,
    _8 = 8,
    _9 = 9,
    _A = 10,
    _B = 11,
    _C = 12,
    _D = 13,
    _E = 14,
    _F = 15,
}

export function chars(i: number): string {
    return Hex[i].charAt(1);
}

export function bits(i: number): string {
    switch (i) {
        case 0x0: return '0000';
        case 0x1: return '0001';
        case 0x2: return '0010';
        case 0x3: return '0011';
        case 0x4: return '0100';
        case 0x5: return '0101';
        case 0x6: return '0110';
        case 0x7: return '0111';
        case 0x8: return '1000';
        case 0x9: return '1001';
        case 0xa: return '1010';
        case 0xb: return '1011';
        case 0xc: return '1100';
        case 0xd: return '1101';
        case 0xe: return '1110';
        case 0xf: return '1111';
        default: return 'erro';
    }
}

function int(n: string, radix: number): number {
    let i = parseInt(n.replace(/[^\d+-\.xa-fA-F]/g, ''), radix);
    i = i & 0xffff;
    if (i & 0x8000) {
        return -((~i+1) & 0xffff);
    }
    return i;
}

export function int16(i: string): number {
    return int(i, 16);
} 

export function int10(i: string): number {
    return int(i, 10);
}

export function int2(i: string): number {
    return int(i, 2);
}

export function hex(i: number): string {
    const hu = chars((i & 0xf000) >> 12);
    const hl = chars((i & 0x0f00) >> 8)
    const lu = chars((i & 0x00f0) >> 4);
    const ll = chars((i & 0x000f));

    return `0x${hu}${hl}${lu}${ll}`;
}

export function bin(i: number): string {
    const hu = bits((i & 0xf000) >> 12);
    const hl = bits((i & 0x0f00) >> 8)
    const lu = bits((i & 0x00f0) >> 4);
    const ll = bits((i & 0x000f));

    return `${hu} ${hl} ${lu} ${ll}`;
}

export function dec(i: number): string {
    const s = Math.sign(i) === -1 ? '-' : '';
    i = Math.abs(i);
    return `${s}${i}`;
}