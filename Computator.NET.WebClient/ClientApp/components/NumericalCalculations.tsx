import * as Config from '../config';
import * as React from "react";
import { RouteComponentProps } from 'react-router';
import { Expression } from "./Expression";
import "isomorphic-fetch";
import { NumericInput } from "./NumericInput";

interface IDictionary<T> {
    [Key: string]: T;
}

interface INumericalCalculationsState {
    a: number;
    b: number;
    x: number;
    epsilon: number;
    n: number;
    order: number;

    operations: string[];
    methods: IDictionary<string[]>;

    operation: string;
    method: string;

    expression: string;
    result: string;
}

export class NumericalCalculations extends React.Component<RouteComponentProps<{}>, INumericalCalculationsState>
{
    public constructor()
    {
        super();
        const methodPlaceHolder = "loading...";
        const operations = ["Integral", "Derivative", "Function root"];
        const methods = { [operations[0]] : [methodPlaceHolder] };

        this.state = { a: -1, b: 1, x: 0, epsilon: 1e-6, n: 1e5, order: 1, operation: operations[0], operations: operations, result: "", expression: "", method: methodPlaceHolder, methods: methods };
    }

    public componentDidMount(): void
    {
        const methodPlaceHolder = "loading...";
        const operations = new Array<string>("Integral", "Derivative", "Function root");
        const operationsUrls = new Array<string>("integral", "derivative", "function-root");

        for (var i = 0; i < operationsUrls.length; i++) {
            const operation = operations[i];
            const operationUri = operationsUrls[i];
            const apiUrl = `${Config.WEB_API_BASE_URL}/numerical-calculations/${operationUri}/list-methods`;
            console.log(`Calling ${apiUrl}`);

            fetch(apiUrl)
                .then(response => response.text() as Promise<string>)
                .then(data => {
                    console.log(`Got result: ${data}`);
                    const loadedMethods = JSON.parse(data) as string[];
                    console.log(`Parsed result: ${loadedMethods}`);


                    this.setState(prevState => prevState.methods[operation] = loadedMethods);
                })
                .then(() => {
                    if (this.state.method == methodPlaceHolder && this.state.methods[operations[0]][0] != methodPlaceHolder)
                        this.setState(prevState => prevState.method = this.state.methods[operations[0]][0]);
                });
        }
    }

    public render(): JSX.Element
    {
        return <div>
            <h1>Numerical calculations</h1>
            <p>Try out many different numerical methods on your expression</p>
            <br />
            <Expression expression={this.state.expression} onExpressionChange={v => this.setState(prevState => prevState.expression = v)} />
            <br />

            <div className="col-md-9">
                <ul className="list-group">
                    <li className="list-group-item">
                        <div className="input-group">
                            <span className="input-group-addon">Operation: </span>
                            <select className="form-control" defaultValue={this.state.operation} onChange={e => this.onOperationChange(e)}>
                                {this.state.operations.map(operation =>
                                    <option key={operation} value={operation}>{operation}</option>
                                )}
                            </select>
                        </div>
                    </li>
                    <li className="list-group-item">
                        <div className="input-group">
                            <span className="input-group-addon">Method: </span>
                            <select className="form-control" defaultValue={this.state.method} onChange={e => this.onMethodChange(e)}>
                                {this.state.methods[this.state.operation].map(method =>
                                    <option key={this.state.operation+":"+method} value={method}>{method}</option>
                                )}
                            </select>
                        </div>
                    </li>
                </ul>
            </div>
            <div className="col-md-3">
                <button className="btn btn-primary btn-lg" onClick={e => this.computeClicked(e)}><br />Compute!<br /><br /></button>
            </div>
            <br />

            {this.state.operation != "Derivative" &&
            <div className="col-md-4">
                <div className="panel panel-default">
                    <div className="panel-heading">Interval:</div>
                    <ul className="list-group">

                        <li className="list-group-item">
                            <NumericInput label="a" value={this.state.a} onValueChanged={v => this.setState(prevState => prevState.a = v)} />
                        </li>

                        <li className="list-group-item">
                            <NumericInput label="b" value={this.state.b} onValueChanged={v => this.setState(prevState => prevState.b = v)} />
                        </li>
                    </ul>
                </div>
            </div>
            }

            {this.state.operation == "Derivative" &&
            <div className="col-md-4">
                <div className="panel panel-default">
                    <div className="panel-heading">Derivative of order n at point x:</div>
                    <ul className="list-group">

                        <li className="list-group-item">
                            <NumericInput label="x" value={this.state.x} onValueChanged={v => this.setState(prevState => prevState.x = v)} />
                        </li>
                        <li className="list-group-item">
                            <NumericInput label="order" value={this.state.order} onValueChanged={v => this.setState(prevState => prevState.order = v)} />
                        </li>
                    </ul>
                </div>
            </div>
            }

            {this.state.operation != "Integral" &&
            <div className="col-md-4">
                <div className="panel panel-default">
                    <div className="panel-heading">Max error:</div>
                    <div className="panel-body">
                        <NumericInput label="ε" value={this.state.epsilon} onValueChanged={v => this.setState(prevState => prevState.epsilon = v)} />
                    </div>
                </div>
            </div>
            }

            {this.state.operation != "Derivative" &&
            <div className="col-md-4">
                <div className="panel panel-default">
                    <div className="panel-heading">Number of steps:</div>
                    <div className="panel-body">
                        <NumericInput label="N" value={this.state.n} onValueChanged={v => this.setState(prevState => prevState.n = v)} />
                    </div>
                </div>
            </div>
            }

            <div className="col-md-12">

                <div className="panel panel-default">
                    <div className="panel-heading">
                        <h3 className="panel-title"><span className="glyphicon glyphicon-chevron-down" aria-hidden="true"></span> Result:</h3>
                    </div>
                    <div className="panel-body">
                        <strong>{this.state.result}</strong>
                    </div>
                </div>
            </div>
        </div>;
    }

    private computeClicked(event: React.MouseEvent<HTMLButtonElement>): void
    {
        //event.preventDefault();

        if (this.state.expression != null && this.state.expression !== "")
        {
            const op = this.state.operation.toLowerCase().replace(" ", "-");

            let apiUrl = `${Config.WEB_API_BASE_URL}/numerical-calculations/${op}/${encodeURIComponent(this.state.method)}/${encodeURIComponent(this.state.expression)}/`;

            if (op == "integral")
                apiUrl += `${this.state.a}/${this.state.b}/${this.state.n}`;
            else if (op == "function-root")
                apiUrl += `${this.state.a}/${this.state.b}/${this.state.epsilon}/${this.state.n}`;
            else if (op == "derivative")
                apiUrl += `${this.state.x}/${this.state.order}/${this.state.epsilon}`;
            else
                throw Error(`Unknown operation - '${op}'`);
            
            console.log(`Calling ${apiUrl}`);

            fetch(apiUrl)
                .then(response => response.text() as Promise<string>)
                .then(data => {
                    console.log(`Got result: ${data}`);
                    this.setState(prevState => prevState.result = data);
                });
        }
    }

    private onOperationChange(event: React.ChangeEvent<HTMLSelectElement>)
    {
        const newValue = event.target.value;
        if (newValue != null) {
            console.log(`Changed operation to ${newValue}`);
            this.setState(prevState => prevState.operation = newValue);

            var newMethod = this.state.methods[newValue][0];
            console.log(`Changed method to ${newMethod}`);
            this.setState(prevState => prevState.method = newMethod);
        }
    }

    private onMethodChange(event: React.ChangeEvent<HTMLSelectElement>)
    {
        const newValue = event.target.value;
        if (newValue != null) {
            console.log(`Changed method to ${newValue}`);
            this.setState(prevState => prevState.method = newValue);
        }
    }
}
