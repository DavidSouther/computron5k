import { ASSIGN, COMMANDS, JUMP } from "../simulator/chips/alu";

type CommandOps = keyof typeof COMMANDS.op;
type JumpOps = keyof typeof JUMP.op;
type StoreOps = keyof typeof ASSIGN.op;

export function asm(op: number): string {
    if (op & 0x8000) return cInstruction(op);
    return aInstruction(op);
}

function cInstruction(op: number): string {
    op = op & 0xffff; // Clear high order bits
    const mop = (op & 0x1000) >> 12;
    let cop = ((op & 0b0000111111000000) >> 6) as CommandOps; 
    let sop = ((op & 0b0000000000111000) >> 3) as StoreOps;
    let jop = (op & 0b0000000000000111) as JumpOps;

    if (COMMANDS.op[cop] === undefined) {
        // Invalid commend
        return "#ERR";
    }

    let command = COMMANDS.op[cop];
    if (mop) command = command.replace(/A/g, 'M');

    const store = ASSIGN.op[sop];
    const jump = JUMP.op[jop];

    let instruction = command;
    if (store) instruction = `${store}=${instruction}`;
    if (jump) instruction = `${instruction};${jump}`;

    return instruction;
}

function aInstruction(op: number): string {
    return '@' + (op & 0x7fff).toString(10);
}

export function op(asm: string): number {
    if (asm[0] === '@') {
        return aop(asm);
    } else {
        return cop(asm);
    }
}

function aop(asm: string): number {
    return parseInt(asm.substring(1), 10);
}

function cop(asm: string): number {
    let parts = asm.match(/(?:([AMD]{1,3})=)?([-!01ADM&|]{1,3})(?:;(JGT|JLT|JGE|JLE|JEQ|JMP))?/)!;
    if (parts.length === 2) {
        parts = ['', parts[1], ''];
    } else if (parts.length === 3) {
        if (parts[2][0] === ';') {
            parts = ['', parts[1], parts[2]];
        } else {
            parts = [parts[1], parts[2], ''];
        }
    }
    const [_, assign, operation, jump] = parts;
    const mode = operation.indexOf('M') > 0 ? 1 : 0;
    const aop = ASSIGN.asm[assign as keyof typeof ASSIGN.asm] ?? 0;
    const jop = JUMP.asm[jump as keyof typeof JUMP.asm] ?? 0;
    const cop = COMMANDS.asm[operation as keyof typeof COMMANDS.asm] ?? 0;

    return 0xD000 | (mode << 12) | (cop << 6) | (aop << 3) | jop;
}