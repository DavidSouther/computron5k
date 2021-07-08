import { Component } from "react";

enum Mode {
    VIEW,
    EDIT,
}

class InlineEdit extends Component<{value: string, onChange: (value: string) => void}> {
    state = {
        mode: Mode.VIEW,
    };

    render() {
        switch (this.state.mode) {
            case Mode.EDIT: return this.edit();
            case Mode.VIEW: return this.view();
        }
    }

    private view() {
        return (<span onClick={() => this.setState({mode: Mode.EDIT})}>{this.props.value}</span>);
    }

    private edit() {
        return (<input type="text" defaultValue={this.props.value} onBlur={({target: {value}}) => this.update(value)} />)
    }

    private update(value: string) {
        this.props.onChange(value);
        this.setState({mode: Mode.VIEW})
    }
}

export default InlineEdit;