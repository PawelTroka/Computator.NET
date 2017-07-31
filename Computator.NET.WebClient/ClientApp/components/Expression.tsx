import * as Config from "../config";
import * as React from "react";
import "isomorphic-fetch";

interface IExpressionState
{
    expression: string;
}

interface IExpressionProps
{
    expression : string;
    onExpressionChange: (expr: string) => void;
}

export class Expression extends React.Component<IExpressionProps, IExpressionState>
{
    public constructor(props: IExpressionProps)
    {
        super(props);
        this.setState({ expression: props.expression });
        //this.handleChange = this.handleChange.bind(this);
    }

    public render(): JSX.Element
    {
        return <div className="input-group input-group-lg">
                    <span className="input-group-addon">Expression:</span>
                    <input type="text" defaultValue={this.props.expression} onChange={e => this.handleChange(e)} className="form-control" placeholder="here write expression, example: 2x-cos(x)" aria-describedby="expression" />
                </div>;
        
    }

    private handleChange(event : React.ChangeEvent<HTMLInputElement>) : void
    {
        if (event.target.value!=null && this.state.expression !== event.target.value)
        {
            //console.log(`Changing expression from ${}`);
            this.setState({ expression: event.target.value });
            if (this.props.onExpressionChange != null)
                this.props.onExpressionChange(event.target.value);
        }

    }
}