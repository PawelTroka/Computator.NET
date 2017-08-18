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

export class Expression extends React.Component<IExpressionProps, {}>
{
    public constructor(props: IExpressionProps) {
        super(props);
        const langTools = brace.acequire("ace/ext/language_tools");
        const customCompleter = new CustomCompleter("expression");
        langTools.setCompleters([customCompleter]);
    }

    public componentDidMount(): void
    {
        if (this.aceEditor.editor != null) {
            //set padding
            this.aceEditor.editor.renderer.setScrollMargin(10, 10);

            // make mouse position clipping nicer
            this.aceEditor.editor.renderer.screenToTextCoordinates = function(x: any, y: any) {
                var pos = this.pixelToScreenCoordinates(x, y);
                return this.session.screenToDocumentPosition(
                    Math.min(this.session.getScreenLength() - 1, Math.max(pos.row, 0)),
                    Math.max(pos.column, 0)
                );
            }

            // disable Enter Shift-Enter keys
            this.aceEditor.editor.commands.bindKey("Enter|Shift-Enter", "null");
        }
    }
    private aceEditor : any;

    public render(): JSX.Element {
        const editorStyle = { padding: "5px" };
        return <div className="input-group input-group-lg">
            <span className="input-group-addon"><span className="glyphicon glyphicon-chevron-right" aria-hidden="true"></span> Expression:</span>
            <AceEditor
                ref={ae => this.aceEditor = ae}
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
                onChange={text => this.props.onExpressionChange(text.replace(/[\r\n]+/g, " "))}
                onPaste={text => this.props.onExpressionChange(text.replace(/[\r\n]+/g, " "))}
                fontSize={20}
                showPrintMargin={false}
                showGutter={false}
                highlightActiveLine={false}
                value={this.props.expression}
                enableBasicAutocompletion={true}
                enableLiveAutocompletion={false}
                tabSize={1}
                setOptions={{
                    enableSnippets: false,
                    showLineNumbers: false,
                    autoScrollEditorIntoView: true
                }} />
        </div>;
    }
}