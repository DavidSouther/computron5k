import {FC} from 'react';
import { Memory as MemoryChip } from '../../simulator/chips/memory';
import MemoryGUI from '../components/memory';

const CPU: FC<{memory: MemoryChip}> = ({memory}) => (<>
    CPU
    <MemoryGUI memory={memory} />
</>);

export default CPU;