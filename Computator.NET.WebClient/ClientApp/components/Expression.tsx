import * as Config from "../config";
import * as React from "react";
import "isomorphic-fetch";

interface IExpressionState
{
    expression: string;
}

export class Expression extends React.Component<{}, IExpressionState>
{
    public constructor()
    {
        super();
        this.setState({ expression: "" });
        this.handleChange = this.handleChange.bind(this);
    }

    public render(): JSX.Element {
        return <div className="input-group input-group-lg">
                    <span className="input-group-addon">Expression:</span>
                    <input type="text" onChange={this.handleChange} className="form-control" placeholder="here write expression, example: 2x-cos(x)" aria-describedby="expression" />
                </div>;
        
    }

    private handleChange(event : React.ChangeEvent<HTMLInputElement>) : void
    {
        this.setState({ expression: event.target.value });
    }
}