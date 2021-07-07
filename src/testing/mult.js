export const JACK = `
while (R0 > 0) {
    R2 = R2 + R1
    R0 = R0 - 1
}`;

export const ASM = `
@R2
M=0
(LOOP)
  @R0
  D=M
  @END
  D;JEQ

  @R1
  D=M
  @R2
  D=D+M
  M=D

  @R0
  M=M-1
  @LOOP
  0;JMP
(END)
  @END
  0;JMP
`;

export const HACK = new Int16Array([
    0x0002, // @R2
    0xDA88, // M=0
    0x0000, // (LOOP) @R0
    0xFC10, // D=M
    0x000F, // @END
    0xD302, // D;JEQ
    0x0001, // @R1
    0xFC10, // D=M
    0x0002, // @R2
    0xF090, // D=D+M
    0xD308, // M=D
    0x0000, // @R0
    0xFC88, // M=M-1
    0x0002, // @LOOP
    0xDA87, // 0;JMP
    0x000F, // (END) @END
    0xDA87, // 0;JMP
]);