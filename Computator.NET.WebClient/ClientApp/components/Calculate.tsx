import * as Config from '../config';
import * as React from "react";
import { Expression } from "./Expression";
import "isomorphic-fetch";
import { NumericInput } from "./NumericInput";

interface ICalculateState {
    x: number;
    y: number;
    expression: string;
    result: string;
}

export class Calculate extends React.Component<{}, ICalculateState>
{
    public constructor() {
        super();
        this.state = { x: 0, y: 0, result: "", expression: "" };
    }

    public render(): JSX.Element {
        return <div>
            <h1>Calculate</h1>
            <p>Write down formulas you want to calculate (eg: 2x+5-PI)</p>
            <br />
            <Expression expression={this.state.expression} onExpressionChange={v => this.setState(prevState => prevState.expression = v)} />
            <br />
            <div className="row">
                <div className="col-xs-1">
                    <p>for values:</p>
                </div>

                <div className="col-xs-6">
                    <ul className="list-group">
                        <li className="list-group-item">
                            <NumericInput label="x" subscriptLabel="" value={0} stepValue={1} onValueChanged={v => this.setState(prevState => prevState.x = v)} />
                        </li>
                        <li className="list-group-item">
                            <NumericInput label="y" subscriptLabel="" value={0} stepValue={1} onValueChanged={v => this.setState(prevState => prevState.y = v)} />
                        </li>
                    </ul>

                </div>
                <div className="col-xs-2">
                    <button className="btn btn-primary btn-lg" onClick={e => this.handleSubmit(e)}>Calculate!</button>
                </div>
            </div>
            <br />
            <div className="row">
                <div className="col-xs-12">

                    <div className="panel panel-default">
                        <div className="panel-heading">
                            <h3 className="panel-title">Result: </h3>
                        </div>
                        <div className="panel-body">
                            <strong>{this.state.result}</strong>
                        </div>
                    </div>
                </div>
            </div>
        </div>;
    }

    private handleSubmit(event: React.MouseEvent<HTMLButtonElement>): void {
        event.preventDefault();
        if (this.state.expression != null && this.state.expression !== "")
            this.calculate(this.state.expression, this.state.x, this.state.y);
    }

    private calculate(expression: string, x: number, y: number): void {
        const apiUrl = `${Config.WEB_API_BASE_URL}/calculate/${encodeURIComponent(expression)}/${x}/${y}`;
        console.log(`Calling ${apiUrl}`);

        fetch(apiUrl)
            .then(response => response.text() as Promise<string>)
            .then(data => {
                console.log(`Got result: ${data}`);
                this.setState(prevState => prevState.result = data);
            });
    }
}
