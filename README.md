# CompuTS

Typescript web implementation of Elements of Computing System's nand2tetris project.

## Architecture

```
src/
    renderer/
        views/
            hdl.ts - View and run single chips
            cpu.ts - View and run a Hack CPU with memory, screen, & keyboard
            vm.ts - View and run a Jack VM program with mem, scrn, & kbd
        components/
            editor.ts - Text editor for .hdl, .hack, .jackvm, and .jack files
            screen.ts - 512x256 B&W screen
            keyboard.ts - Show current keyboard value
            memory.ts - View & edit a block of byte-array memory
            alu.ts - Render the internal state of the ALU
            cpu.ts - Render the internal state of the CPU (A/D registers, etc)
            chip.ts - Render the input/output pins & internal pieces of an hdl
        lib/
            panel.ts - layout helpers
    simulator/ - Internal state of various components
        chips/ - Typescript implementations of built-in chips
            add.ts
            or.ts
            xor.ts
            ...
            cpu.ts
            memory.ts
            screen.ts
        vm/ - Typescript implementations of Jack methods
            array.ts
            keyboard.ts
            memory.ts
            output.ts
            screen.ts
            string.ts
            sys.ts
        
            

