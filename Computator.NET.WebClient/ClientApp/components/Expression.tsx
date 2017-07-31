import * as Config from "../config";
import * as React from "react";
import "isomorphic-fetch";

interface IExpressionProps
{
    expression : string;
    onExpressionChange: (expr: string) => void;
}

export class Expression extends React.Component<IExpressionProps, {}>
{
    public constructor(props: IExpressionProps)
    {
        super(props);
        //this.handleChange = this.handleChange.bind(this);
    }

    public render(): JSX.Element
    {
        return <div className="input-group input-group-lg">
                    <span className="input-group-addon">Expression:</span>
                    <input type="text" defaultValue={this.props.expression} onChange={e => this.handleChange(e)} className="form-control" placeholder="here write expression, example: 2x-cos(x)" aria-describedby="expression" />
                </div>;
        
    }

    private handleChange(event : React.ChangeEvent<HTMLInputElement>) : void {
        const newValue = event.target.value;
        if (newValue!=null)
        {
            console.log(`Changing expression to ${newValue}`);
            if (this.props.onExpressionChange != null)
                this.props.onExpressionChange(newValue);
        }

    }
}