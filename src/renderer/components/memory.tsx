import { FC, useState } from "react";
import { Memory as MemoryChip } from "../../simulator/chips/memory";
import { asm, op } from "../../util/asm";
import { bin, dec, hex, int10, int16, int2 } from "../../util/twos";
import ButtonBar from "../widgets/button_bar";
import InlineEdit from "../widgets/inline_edit";
import VirtualScroll from "../widgets/virtual_scroll";

const FORMATS = ["bin", "dec", "hex", "asm"];
type Formats = typeof FORMATS[number];

const MemoryBlock: FC<{
  memory: MemoryChip;
  highlight?: number;
  editable?: boolean;
  format: (v: number) => string;
  update: (i: number, value: string, previous: number) => void;
}> = ({ memory, highlight = -1, editable = false, format, update }) => (
  <VirtualScroll<[number, number]>
    settings={{ count: 20, maxIndex: memory.size }}
    get={(o, l) => memory.map((i, v) => [i, v], o, o + l)}
    row={([i, v]) => (
      <MemoryCell
        index={i}
        value={format(v)}
        editable={editable}
        highlight={i === highlight}
        onChange={(value) => update(i, value, v)}
      />
    )}
  />
);

const MemoryCell: FC<{
  index: number;
  value: string;
  highlight: boolean;
  editable?: boolean;
  onChange?: (v: string) => void;
}> = ({ index, value, highlight, editable = false, onChange = () => {} }) => (
  <div
    key={index}
    className={`${highlight ? "bg-gray-300" : ""} hover:bg-gray-300`}
    style={{ height: 20 }}
  >
    <div className="inline-block px-4 w-1/4">{hex(index)}</div>
    <div className="inline-block px-4 w-3/4 text-right">
      {editable ? (
        <InlineEdit value={value} onChange={(v: string) => onChange(v)} />
      ) : (
        <span>{value}</span>
      )}
    </div>
  </div>
);

const Memory: FC<{
  name?: string;
  highlight?: number;
  editable?: boolean;
  memory: MemoryChip;
}> = ({ name = "Memory", highlight = -1, editable = true, memory }) => {
  const [format, setFormat] = useState<Formats>("dec");

  function doFormat(v: number): string {
    switch (format) {
      case "bin":
        return bin(v);
      case "hex":
        return hex(v);
      case "asm":
        return asm(v);
      case "dec":
      default:
        return dec(v);
    }
  }

  function update(cell: number, value: string, previous: number) {
    let current: number;
    switch (format) {
      case "asm":
        current = op(value);
        break;
      case "bin":
        current = int2(value);
        break;
      case "hex":
        current = int16(value);
        break;
      case "dec":
      default:
        current = int10(value);
        break;
    }
    if (isFinite(current) && current <= 0xffff) {
      memory.set(cell, current);
    }
  }

  return (
    <div className="w-80 min-h-80 border border-gray-900 rounded">
      <div className="flex flex-row space-x-1 pl-2">
        <p className="flex-initial text-xl">{name}</p>
        <div className="flex-initial">
          <ButtonBar<Formats>
            value={format}
            values={FORMATS}
            onClick={setFormat}
          />
        </div>
      </div>
      <div className="w-full h-auto font-mono">
        <MemoryBlock
          memory={memory}
          highlight={highlight}
          editable={editable}
          format={doFormat}
          update={update}
        />
      </div>
    </div>
  );
};

export default Memory;
