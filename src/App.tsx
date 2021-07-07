import './App.css';
import CPU from './renderer/views/cpu';
import { Memory } from './simulator/chips/memory';

const memory = new Memory(new Int16Array([1, -1, 0, 256]));

function App() {
  return (
    <div className="App">
      <header className="App-header">
        <CPU memory={memory} />
      </header>
    </div>
  );
}

export default App;
