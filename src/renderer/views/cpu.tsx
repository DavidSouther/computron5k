import {FC} from 'react';
import { Memory as MemoryChip } from '../../simulator/chips/memory';
import MemoryGUI from '../components/memory';

const CPU: FC<{memory: MemoryChip}> = ({memory}) => (<>
    <h1 className="text-6xl">CPU Emulator</h1>
    <MemoryGUI memory={memory} />
</>);

export default CPU;