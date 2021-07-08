import './App.css';
import CPU from './renderer/views/cpu';
import {CPU as Computer} from './simulator/chips/cpu';
import { Memory } from './simulator/chips/memory';
import { HACK } from './testing/mult';

const RAM = new Memory(new Int16Array([2, 3, 0]));
const ROM = new Memory(HACK);

const COMPUTER = new Computer({ROM});

function App() {
  return (
    <div className="font-base text-black bg-gray-200 w-screen h-screen px-4 py-2">
      <CPU cpu={COMPUTER} />
    </div>
  );
}

export default App;
