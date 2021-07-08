export const COMMANDS = {
    asm: {
          '0': 0b101010, // 42 0x2A
          '1': 0b111111, // 63 0x3F
         '-1': 0b111010, // 58 0x3A
          'D': 0b001100, // 12 0x0C
          'A': 0b110000, // 48 0x30
         '!D': 0b001101, // 13 0x0D
         '!A': 0b110001, // 49 0x31
         '-D': 0b001111, // 15 0x0F
         '-A': 0b110011, // 51 0x33
        'D+1': 0b011111, // 31 0x1F
        'A+1': 0b110111, // 55 0x37
        'D-1': 0b001110, // 14 0x0E
        'A-1': 0b110010, // 50 0x32
        'D+A': 0b000010, //  2 0x02
        'D-A': 0b010011, // 19 0x13
        'A-D': 0b000111, //  7 0x07
        'D&A': 0b000000, //  0 0x00
        'D|A': 0b010101, // 21 0x15
    },
    op: {
        0x2A: '0',
        0x3F: '1',
        0x3A: '-1',
        0x0C: 'D',
        0x30: 'A',
        0x0D: '!D',
        0x31: '!A',
        0x0F: '-D',
        0x33: '-A',
        0x1F: 'D+1',
        0x37: 'A+1',
        0x0E: 'D-1',
        0x32: 'A-1',
        0x02: 'D+A',
        0x13: 'D-A',
        0x07: 'A-D',
        0x00: 'D&A',
        0x15: 'D|A',
    },
}

export const ASSIGN = {
    asm: {
        '': 0x0,
        'M': 0b001,
        'D': 0b010,
        'MD': 0b011,
        'A': 0b100,
        'AM': 0b101,
        'AD': 0b110,
        'AMD': 0b111,
    },
    op: {
        0x0: '',
        0x1: 'M',
        0x2: 'D',
        0x3: 'MD',
        0x4: 'A',
        0x5: 'AM',
        0x6: 'AD',
        0x7: 'AMD', 
    }
}

export const JUMP = {
    asm: {
        '': 0b0,
        'JGT': 0b001,
        'JEQ': 0b010,
        'JGE': 0b011,
        'JLT': 0b100,
        'JNE': 0b101,
        'JLE': 0b110,
        'JMP': 0b111,
    },
    op: {
        0x0: '',
        0x1: 'JGT',
        0x2: 'JEQ',
        0x3: 'JGE',
        0x4: 'JLT',
        0x5: 'JNE',
        0x6: 'JLE',
        0x7: 'JMP',
    }
}

export enum Flags {
    Positive = 0x00,
    Zero = 0x01,
    Negative = 0x02,
} 

export function alu(op: number, d: number, a: number): [number, Flags] {
    let o = 0;
    switch(op) {
        case 0x2A: o = 0; break;
        case 0x3F: o = 1; break;
        case 0x3A: o = -1; break;
        case 0x0C: o = d; break;
        case 0x30: o = a; break;
        case 0x0D: o = ~d; break;
        case 0x31: o = ~a; break;
        case 0x0F: o = -d; break;
        case 0x33: o = -a; break;
        case 0x1F: o = d + 1; break;
        case 0x37: o = a + 1; break;
        case 0x0E: o = d - 1; break;
        case 0x32: o = a - 1; break;
        case 0x02: o = d + a; break;
        case 0x13: o = d - a; break;
        case 0x07: o = a - d; break;
        case 0x00: o = d & a; break;
        case 0x15: o = d | a; break;
    }
    
    o = o & 0xffff;
    const flags = o === 0 ? Flags.Zero : (o & 0x8000) ? Flags.Negative : Flags.Positive;
    return [o, flags];
}