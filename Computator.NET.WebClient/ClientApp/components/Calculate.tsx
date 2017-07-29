import * as Config from '../config';
import * as React from "react";
import "isomorphic-fetch";

interface ICalculateState
{
    x: number;
    y: number;
    expression: string;
    result: string;
}

export class Calculate extends React.Component<{}, ICalculateState>
{
    public constructor()
    {
        super();
        this.state = { x: 0, y: 0, result: "", expression: "" };
    }

    public render() : JSX.Element
    {
        return <div>
                   <h1>Calculate</h1>

                   <p>Write down formulas you want to calculate (eg: 2x+5-PI)</p>
                   <form onSubmit={(e) => this.handleSubmit(e)}>
                       expression: <input type="text" defaultValue={this.state.expression} ref={expression => this.expressionInput = expression} required/><br/>
                       x = <input type="number" defaultValue={this.state.x.toString()} ref={x => this.xInput = x}/><br/>
                       y = <input type="number" defaultValue={this.state.y.toString()} ref={y => this.yInput = y}/><br/>
                       <br/>
                       <input type="submit" value="Calculate!"/>
                   </form>

                   <p>Result: <strong>{ this.state.result }</strong>
                   </p>
               </div>;
    }

    private handleSubmit(event: React.FormEvent<HTMLFormElement>): void
    {
        event.preventDefault();
        this.calculate(this.expressionInput.value, Number(this.xInput.value), Number(this.yInput.value));
    }

    private calculate(expression: string, x: number, y: number): void
    {
        const apiUrl = `${Config.WEB_API_BASE_URL}/calculate/${encodeURIComponent(expression)}/${x}/${y}`;
        console.log(`Calling ${apiUrl}`);

        fetch(apiUrl)
            .then(response => response.text() as Promise<string>)
            .then(data =>
            {
                console.log(`Got result: ${data}`);
                this.setState({ x: x, y: y, expression: expression, result: data });
            });
    }

    private expressionInput: HTMLDataElement;
    private xInput: HTMLDataElement;
    private yInput: HTMLDataElement;
}
