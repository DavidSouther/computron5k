import { Component } from "react";

class ButtonBar<T> extends Component<{value: T, values: T[], onClick: (current: T) => void}> {
    render() {
        return (
            <div className="grid grid-cols-3 my-2">
                {this.props.values.map(option => (
                    <button className={`
                            rounded py-1 px-2 mx-2 hover:bg-gray-800
                            ${this.props.value === option ? 'bg-gray-700' : 'bg-gray-500'}
                        `}
                        onClick={() => this.props.onClick(option)}>{option}</button>
                ))}
            </div>
        )
    }
}
export default ButtonBar;