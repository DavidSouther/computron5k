import { FC, useState } from "react";
import { Memory as MemoryChip } from "../../simulator/chips/memory";
import { asm } from "../../util/asm";
import { bin, dec, hex } from "../../util/twos";
import ButtonBar from "../widgets/button_bar";

const FORMATS = ['bin', 'dec', 'hex', 'asm'];
type Formats = (typeof FORMATS)[number];

const Memory: FC<{name?: string, highlight?: number, memory: MemoryChip}> = ({name = "Memory", highlight = -1, memory}) => {
    const [format, setFormat] = useState<Formats>('dec');

    function doFormat(v: number): string {
        switch(format) {
            case 'bin': return bin(v);
            case 'hex': return hex(v);
            case 'asm': return asm(v);
            case 'dec':
            default: return dec(v);
        }
    }

    return (
    <div className="w-80 min-h-80 border border-gray-900 rounded">
        <div className="flex flex-row space-x-1 pl-2">
            <p className="flex-initial text-xl">{name}</p>
            <div className="flex-initial">
                <ButtonBar<Formats> value={format} values={FORMATS} onClick={setFormat} />
            </div>
        </div>
        <div className="w-full max-h-72 overflow-y-scroll font-mono">
            <table className="w-full h-full table-fixed border-collapse"><tbody>{
            [...memory.map((i, v) => (
                <tr key={i} className={`${i === highlight ? 'bg-gray-300': ''} hover:bg-gray-300`}>
                    <td className="border px-4 w-1/4">{hex(i)}</td>
                    <td className="border px-4 w-3/4 text-right">{doFormat(v)}</td>
                </tr>
            ))]
            }</tbody></table>
        </div>
    </div>
    );
}

export default Memory;