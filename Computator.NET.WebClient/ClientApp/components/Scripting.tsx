import { useEffect, useMemo, useState } from 'react';
import AceEditor from 'react-ace';
import 'ace-builds/src-noconflict/mode-csharp';
import 'ace-builds/src-noconflict/snippets/csharp';
import 'ace-builds/src-noconflict/theme-ambiance';
import 'ace-builds/src-noconflict/ext-language_tools';
import 'ace-builds/src-noconflict/ext-searchbox';
import 'ace-builds/src-noconflict/ext-keybinding_menu';
import 'ace-builds/src-noconflict/ext-settings_menu';
import { CustomCompleter } from '../helpers/CustomCompleter';
import { SpecialSymbolsHandler } from '../helpers/SpecialSymbolsHandler';
import { useTextResult } from '../helpers/useTextResult';
import { ResultPanel } from './ResultPanel';

export function Scripting() {
  const [expression, setExpression] = useState('');
  const request = useTextResult();
  const completer = useMemo(() => new CustomCompleter('scripting'), []);
  useEffect(() => () => completer.dispose(), [completer]);
  return <><h1>Scripting</h1><p>Write a script in TSL. Run only scripts you trust.</p>
    <div className="row g-3"><div className="col-md-9"><section className="card">
      <div className="card-header">Script:</div><div className="card-body script-editor-panel">
        <AceEditor name="scriptTextArea" mode="csharp" theme="ambiance" width="100%" minLines={15} maxLines={25}
          fontSize={16} showPrintMargin={false} showGutter highlightActiveLine value={expression} tabSize={4}
          onChange={text => setExpression(SpecialSymbolsHandler.replace(text, false))}
          onLoad={editor => { editor.completers = [completer]; editor.container.querySelector('textarea')?.setAttribute('aria-label', 'TSL script'); }}
          enableBasicAutocompletion enableLiveAutocompletion setOptions={{ useWorker: false, enableSnippets: true, showLineNumbers: true }} />
      </div></section></div>
      <div className="col-md-3"><button type="button" className="btn btn-primary btn-lg" disabled={!expression.trim() || request.pending}
        onClick={() => void request.execute('script', { expression })}>Process script</button></div>
    </div><ResultPanel {...request} heading="Output" />
  </>;
}
