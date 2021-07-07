import { FC, useState } from "react";
import { Memory as MemoryChip } from "../../simulator/chips/memory";
import { asm } from "../../util/asm";
import { bin, dec, hex } from "../../util/twos";
import ButtonBar from "../widgets/button_bar";

const FORMATS = ['bin', 'dec', 'hex', 'asm'];
type Formats = (typeof FORMATS)[number];

const Memory: FC<{memory: MemoryChip}> = ({memory}) => {
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
    <div className="w-72 border border-gray-900 rounded">
        <ButtonBar<Formats> value={format} values={FORMATS} onClick={setFormat} />
        <div className="w-full overflow-y-scroll font-mono">
            <table className="w-full table-fixed border-collapse"><tbody>{
            [...memory.map((i, v) => (
                <tr key={i}>
                    <td className="border px-4 w-1/6">{i}</td>
                    <td className="border px-4 w-5/6 text-right">{doFormat(v)}</td>
                </tr>
            ))]
            }</tbody></table>
        </div>
    </div>
    );
}

export default Memory;