import * as Config from "../config";
import * as React from "react";
import "isomorphic-fetch";

import AceEditor from "react-ace";
import { CustomCompleter } from "../helpers/CustomCompleter";
import * as brace from 'brace';


import 'brace/mode/csharp';
import 'brace/snippets/csharp';
import 'brace/theme/github';
import 'brace/ext/searchbox';
import 'brace/ext/language_tools';
import 'brace/ext/keybinding_menu'
import 'brace/ext/settings_menu'

interface IExpressionProps {
    expression: string;
    onExpressionChange: (expr: string) => void;
}

interface IExpressionState {
    
}

export class Expression extends React.Component<IExpressionProps, {}>
{
    public constructor(props: IExpressionProps) {
        super(props);
        //this.handleChange = this.handleChange.bind(this);
        const langTools = brace.acequire("ace/ext/language_tools");
        const customCompleter = new CustomCompleter("expression");
        langTools.setCompleters([customCompleter]);
    }

    public componentDidMount(): void
    {
        
    }

    public render(): JSX.Element
    {
        return <div className="input-group input-group-lg">
            <span className="input-group-addon"><span className="glyphicon glyphicon-chevron-right" aria-hidden="true"></span> Expression:</span>
            <AceEditor
                className="form-control" aria-describedby="expression" 
                editorProps={{
                    $blockScrolling: Infinity
                }}
                       width="100%"
                       height="100%"
                       mode="csharp"
                       minLines={1}
                       maxLines={1}
                       theme="github"
                       name="expressionTextArea"
                       onChange={s => this.props.onExpressionChange(s.replace('\n',"").replace('\r',""))}
                       fontSize={36}
                       showPrintMargin={false}
                       showGutter={false}
                       highlightActiveLine={false}
                       value={this.props.expression}
                       enableBasicAutocompletion={true}
                       enableLiveAutocompletion={false}
                       tabSize={4}
                       setOptions={{
                    enableSnippets: false,
                    showLineNumbers: false,
                    autoScrollEditorIntoView: true
                }} />
               </div>;
    }

    private handleChange(event: React.ChangeEvent<HTMLInputElement>): void {
        const newValue = event.target.value;
        if (newValue != null) {
            console.log(`Changing expression to ${newValue}`);
            if (this.props.onExpressionChange != null)
                this.props.onExpressionChange(newValue);
        }

    }
}