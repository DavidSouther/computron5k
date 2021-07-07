import { FC, useState } from "react";
import { Memory as MemoryChip } from "../../simulator/chips/memory";
import { bin, dec, hex } from "../../util/twos";
import ButtonBar from "../widgets/button_bar";

const Memory: FC<{memory: MemoryChip}> = ({memory}) => {
    const [format, setFormat] = useState<'bin'|'dec'|'hex'>('dec');

    function doFormat(v: number): string {
        switch(format) {
            case 'bin': return bin(v);
            case 'hex': return hex(v);
            case 'dec':
            default: return dec(v);
        }
    }

    return (
    <div>
        <ButtonBar<'bin'|'dec'|'hex'> value={format} values={['bin', 'dec', 'hex']} onClick={setFormat} />
        <div className="overflow-y-scroll">
            <table className="border-collapse"><tbody>{
            [...memory.map((i, v) => (
                <tr key={i}>
                    <td className="border px-4">{i}</td>
                    <td className="border px-4">{doFormat(v)}</td>
                </tr>
            ))]
            }</tbody></table>
        </div>
    </div>
    );
}

export default Memory;