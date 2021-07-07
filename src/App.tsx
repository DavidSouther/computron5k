import './App.css';
import CPU from './renderer/views/cpu';
import { Memory } from './simulator/chips/memory';

const RAM = new Memory(new Int16Array([1, -1, 0, 256]));
const ROM= new Memory(new Int16Array([
  0b1110_101010_000_000,
  0b1111_110000_010_000,
  0b1110_001110_010_101,
  0b1110_101010_000_111,
  0b1111_110010_011_000,
]));

function App() {
  return (
    <div className="font-base text-black bg-gray-200 w-screen h-screen px-4 py-2">
      <CPU memory={ROM} />
    </div>
  );
}

export default App;
