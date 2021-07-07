import {FC, useState} from 'react';
import { CPU as CPUChip } from '../../simulator/chips/cpu';
import MemoryGUI from '../components/memory';

const CPU: FC<{cpu: CPUChip}> = ({cpu}) => {
    const state = () => [cpu.PC, cpu.A, cpu.D];
    const [[PC, A, D], setState] = useState(state());
    const tick = () => {
        cpu.tick();
        setState(state());
    };
    const reset = () => {
        cpu.reset();
        setState(state());
    };

    return (<>
        <h1 className="text-6xl">CPU Emulator</h1>
        <div className="space-x-1">
            <button className="p-2 rounded border border-black bg-gray-50 hover:bg-gray-200 active:bg-gray-300"
                onClick={tick}
            >&gt;</button>
            <button className="p-2 rounded border border-black bg-gray-50 hover:bg-gray-200 active:bg-gray-300"
                onClick={reset}
            >&lt;&lt;</button>
            <span>PC: {PC}</span>
            <span>A: {A}</span>
            <span>D: {D}</span>
        </div>
        <div>
            <MemoryGUI name="RAM" memory={cpu.RAM} />
            <MemoryGUI name="ROM" memory={cpu.ROM} highlight={PC} />
        </div>
    </>);
};

export default CPU;