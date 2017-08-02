import * as Config from '../config';
import * as React from "react";
import "isomorphic-fetch";

interface IScriptState {
    expression: string;
    result: string;
}

export class Scripting extends React.Component<{}, IScriptState>
{
    public constructor()
    {
        super();
        this.state = { result: "", expression: "" };
    }

    public render(): JSX.Element
    {
        const outputStyle = {
            whiteSpace: "pre-wrap"
        };
        return <div>
            <h1>Scripting</h1>
            <p>Write down script in TSL</p>
            <br />

            <div className="row">
                <div className="col-md-9">
                    <div className="form-group">
                        <label htmlFor="scriptTextArea">Script:</label>
                        <textarea className="form-control" id="scriptTextArea" defaultValue={this.state.expression} onChange={e => this.handleChange(e)} rows={14} cols={45} />
                    </div>
                </div>
                <div className="col-md-3">
                    <button className="btn btn-primary btn-lg pull-right" onClick={e => this.handleSubmit(e)}>Process<br />script</button>
                </div>
            </div>
            <br />
               <div className="row">
            <div className="col-md-12">
                    <div className="panel panel-default">
                        <div className="panel-heading">
                            <h3 className="panel-title">Output: </h3>
                        </div>
                        <div className="panel-body">
                            <strong style={outputStyle}>
                                {this.state.result}
                            </strong>
                        </div>
                    </div>
            </div></div>
        </div>;
    }

    private handleChange(event: React.ChangeEvent<HTMLTextAreaElement>): void
    {
        const newValue = event.target.value;
        if (newValue != null)
        {
            console.log(`Changing script to ${newValue}`);
            this.setState(prevState => prevState.expression = newValue);
        }

    }

    private handleSubmit(event : React.MouseEvent<HTMLButtonElement>) : void
    {
        event.preventDefault();
        if (this.state.expression != null && this.state.expression !== "")
            this.process(this.state.expression);
    }

    private process(expression : string): void
    {
        const apiUrl = `${Config.WEB_API_BASE_URL}/script/${encodeURIComponent(expression)}`;
        console.log(`Calling ${apiUrl}`);

        fetch(apiUrl)
            .then(response => response.text() as Promise<string>)
            .then(data => {
                console.log(`Got result: ${data}`);
                this.setState(prevState => prevState.result = data);
            });
    }
}
