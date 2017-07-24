import * as React from 'react';

interface CounterState {
    currentCount: number;
}

export class Calculate extends React.Component<{}, CounterState> {
    constructor() {
        super();
        this.state = { currentCount: 0 };
    }

    public render() {
        return <div>
            <h1>Calculate</h1>

            <p>This is a simple example of a React component.</p>

            <p>Current count: <strong>{ this.state.currentCount }</strong></p>

            <button onClick={ () => { this.incrementCounter() } }>Increment</button>
        </div>;
    }

    incrementCounter() {
        this.setState({
            currentCount: this.state.currentCount + 3
        });
    }
}
