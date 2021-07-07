import { FC, useState } from "react";
import { Memory as MemoryChip } from "../../simulator/chips/memory";
import { bin, dec, hex } from "../../util/twos";

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

    return (<>
        <button onClick={() => setFormat('bin')}>bin</button>
        <button onClick={() => setFormat('dec')}>dec</button>
        <button onClick={() => setFormat('hex')}>hex</button>
        <table><tbody>{
        [...memory.map((i, v) => (
            <tr key={i}><td>{i}</td><td>{doFormat(v)}</td></tr>
        ))]
        }</tbody></table>
    </>);
}

export default Memory;