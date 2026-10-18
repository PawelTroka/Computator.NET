import { useEffect, useMemo } from 'react';
import AceEditor from 'react-ace';
import 'ace-builds/src-noconflict/mode-csharp';
import 'ace-builds/src-noconflict/theme-github';
import 'ace-builds/src-noconflict/ext-language_tools';
import 'ace-builds/src-noconflict/ext-searchbox';
import { CustomCompleter } from '../helpers/CustomCompleter';
import { SpecialSymbolsHandler } from '../helpers/SpecialSymbolsHandler';

export function Expression({ expression, onExpressionChange }: {
  expression: string; onExpressionChange: (expression: string) => void;
}) {
  const completer = useMemo(() => new CustomCompleter('expression'), []);
  useEffect(() => () => completer.dispose(), [completer]);
  return <div className="input-group input-group-lg mb-3">
    <span className="input-group-text">Expression:</span>
    <div className="expression-editor">
      <AceEditor name="expressionTextArea" mode="csharp" theme="github"
        width="100%" minLines={1} maxLines={1} fontSize={20} showPrintMargin={false} showGutter={false}
        highlightActiveLine={false} value={expression} tabSize={1}
        onChange={text => {
          // Literals and comments can contain meaningful asterisks and newlines.
          // Preserve their source, including unfinished input, without attempting
          // to parse every C# literal form while the user is still typing.
          const source = /["']|\/[/*]/.test(text) ? text
            : SpecialSymbolsHandler.replace(text.replace(/[\r\n]+/g, ' '), false);
          onExpressionChange(source);
        }}
        onLoad={editor => {
          editor.completers = [completer];
          editor.container.querySelector('textarea')?.setAttribute('aria-label', 'Expression');
          editor.renderer.setScrollMargin(10, 10);
          editor.commands.bindKey('Enter|Shift-Enter', 'null');
        }}
        enableBasicAutocompletion enableLiveAutocompletion={false}
        setOptions={{ useWorker: false, enableSnippets: false, showLineNumbers: false }} />
    </div>
  </div>;
}
