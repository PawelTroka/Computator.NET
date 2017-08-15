import * as Config from '../config';
import * as React from "react";
import { RouteComponentProps } from 'react-router';
import "isomorphic-fetch";
import AceEditor from "react-ace";
import { CustomCompleter } from "../helpers/CustomCompleter";
import * as brace from 'brace';


import 'brace/mode/csharp';
import 'brace/snippets/csharp';
import 'brace/theme/ambiance';
import 'brace/ext/searchbox';
import 'brace/ext/language_tools';
import 'brace/ext/keybinding_menu'
import 'brace/ext/settings_menu'

interface IScriptState {
    expression: string;
    result: string;
}

export class Scripting extends React.Component<RouteComponentProps<{}>, IScriptState>
{
    public constructor() {
        super();
        this.state = { result: "", expression: "" };

        const langTools = brace.acequire("ace/ext/language_tools");
        const customCompleter = new CustomCompleter("scripting");
        langTools.setCompleters([customCompleter]);
    }

    public render(): JSX.Element {
        const outputStyle = {
            whiteSpace: "pre-wrap"
        };
        const editorStyle = {
            border: "1px solid lightgray"
        }
        const editorPanelStyle = {
            backgroundColor: "#3d3d3d"
        }

        return <div>
            <h1>Scripting</h1>
            <p>Write down script in TSL</p>
            <br />

            <div className="row">
                <div className="col-md-9">
                    <div className="panel panel-default">
                        <div className="panel-heading">
                            <h3 className="panel-title">Script: </h3>
                        </div>
                        <div className="panel-body" style={editorPanelStyle}>
                            <AceEditor
                                editorProps={{
                                    $blockScrolling: Infinity
                                }}
                                width="100%"
                                height="100%"
                                mode="csharp"
                                minLines={15}
                                maxLines={25}
                                theme="ambiance"
                                name="scriptTextArea"
                                onChange={s => this.onChange(s)}
                                fontSize={16}
                                showPrintMargin={false}
                                showGutter={true}
                                highlightActiveLine={true}
                                value={this.state.expression}
                                enableBasicAutocompletion={true}
                                enableLiveAutocompletion={true}
                                tabSize={4}
                                setOptions={{
                                    enableSnippets: true,
                                    showLineNumbers: true,
                                    autoScrollEditorIntoView: true
                                }} />
                        </div></div></div>
                <div className="col-md-3">
                    <button className="btn btn-primary btn-lg" onClick={e => this.handleSubmit(e)}>Process<br />script</button>
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

    private onChange(newValue: string): void {
        if (newValue != null) {
            console.log(`Changing script to ${newValue}`);
            this.setState(prevState => prevState.expression = newValue);
        }

    }

    private handleChange(event: React.ChangeEvent<HTMLTextAreaElement>): void {
        const newValue = event.target.value;
        if (newValue != null) {
            console.log(`Changing script to ${newValue}`);
            this.setState(prevState => prevState.expression = newValue);
        }

    }

    private handleSubmit(event: React.MouseEvent<HTMLButtonElement>): void {
        event.preventDefault();
        if (this.state.expression != null && this.state.expression !== "")
            this.process(this.state.expression);
    }

    private process(expression: string): void {
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
