import { Component } from "react";

class ButtonBar<T> extends Component<{value: T, values: T[], onClick: (current: T) => void}> {
    render() {
        return (
            <div className={`grid grid-cols-${this.props.values.length} text-white divide-x`}>
                {this.props.values.map((option, i) => (
                    <button key={`${option}`}
                        className={`
                            py-1 px-2 hover:bg-gray-800
                            ${this.props.value === option ? 'bg-gray-700' : 'bg-gray-500'}
                            ${i === 0 ? 'rounded-l' : ''}
                            ${i === this.props.values.length - 1 ? 'rounded-r' : ''}
                        `}
                        onClick={() => this.props.onClick(option)}>{option}</button>
                ))}
            </div>
        )
    }
}
export default ButtonBar;