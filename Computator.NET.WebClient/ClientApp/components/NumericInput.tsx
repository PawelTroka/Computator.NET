import * as Config from "../config";
import * as React from "react";
import "isomorphic-fetch";


export interface INumericInputProps
{
    label : string;
    subscriptLabel? : string;
    value : number;
    onValueChanged? : (val : number) => void;
    stepValue? : number;
}

export class NumericInput extends React.Component<INumericInputProps, {}>
{
    public static defaultProps: Partial<INumericInputProps> =
    {
        stepValue: 1,
        onValueChanged: val => {},
        subscriptLabel: ""
    };

    public constructor(props : INumericInputProps)
    {
        super(props);
        //this.onChange.bind(this);
    }

    public render() : JSX.Element
    {
        return <div className="input-group">
                   <span className="input-group-addon">{this.props.label}<sub>{this.props.subscriptLabel}</sub> = </span>
                   <input defaultValue={this.props.value.toString()} onChange={e => this.onChange(e)} className="form-control" type="number" step={this.props.stepValue} />
               </div>;
    }

    private onChange(event : React.ChangeEvent<HTMLInputElement>)
    {
        const newValue = event.target.value;
        const newNumber = Number(newValue);
        if (!isNaN(newNumber))
        {
            console.log(`Changing value on ${this.props.label}${this.props.subscriptLabel} to ${newNumber}`);

            if (this.props.onValueChanged != null)
                this.props.onValueChanged(newNumber);
        }
    }
}
