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