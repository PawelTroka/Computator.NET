import { useEffect, useMemo, useRef, useState } from 'react';
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
import { maximumScriptBytes, maximumScriptCharacters, useScriptDocument } from '../helpers/ScriptDocumentContext';
import { ResultPanel } from './ResultPanel';

export function Scripting() {
  const script = useScriptDocument();
  const [fileError, setFileError] = useState('');
  const revision = useRef(0);
  const request = useTextResult();
  const completer = useMemo(() => new CustomCompleter('scripting'), []);
  useEffect(() => () => { completer.dispose(); revision.current++; }, [completer]);
  async function importFile(file: File | undefined) {
    if (!file) return;
    const importing = ++revision.current;
    try {
      if (!/\.tsl$/i.test(file.name) || file.size > maximumScriptBytes) throw new Error('Choose a UTF-8 .tsl file of at most 256 KiB.');
      const code = new TextDecoder('utf-8', { fatal: true }).decode(await file.arrayBuffer()).replace(/^\uFEFF/, '');
      if (code.length > maximumScriptCharacters) throw new Error('Scripts may contain at most 65,536 characters.');
      if (revision.current !== importing) return;
      if (script.code && script.code !== code && !confirm('Replace the current script? Export it first to keep a separate copy.')) return;
      script.replace(code, file.name); setFileError('');
    } catch (reason) {
      if (revision.current === importing) setFileError(reason instanceof Error ? reason.message : 'The file could not be read.');
    }
  }
  function exportFile() {
    const url = URL.createObjectURL(new Blob([script.code], { type: 'text/plain;charset=utf-8' }));
    const link = document.createElement('a'); link.href = url; link.download = script.name;
    link.click(); setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
  return <><h1>Scripting</h1><p>Write a script in TSL. Run only scripts you trust.</p>
    <div className="d-flex flex-wrap gap-2 my-3">
      <label className="btn btn-outline-secondary">Import .tsl<input aria-label="Import .tsl" type="file" accept=".tsl"
        className="visually-hidden" onChange={event => { void importFile(event.target.files?.[0]); event.target.value = ''; }} /></label>
      <button type="button" className="btn btn-outline-secondary" onClick={exportFile}>Export .tsl</button>
      <button type="button" className="btn btn-outline-danger" onClick={() => {
        if ((script.code || script.error) && !confirm('Discard the current script and its saved draft in this tab?')) return;
        revision.current++; script.clear(); setFileError('');
      }}>New script</button>
    </div>
    <p data-testid="script-draft-state" aria-live="polite">{script.name} — {script.unstored ? 'Draft kept in memory only' : 'Draft saved in this tab'}. Export a .tsl file to keep a permanent copy.</p>
    {(fileError || script.error) && <p role="alert">{fileError || script.error}</p>}
    <div className="row g-3"><div className="col-md-9"><section className="card">
      <div className="card-header">Script:</div><div className="card-body script-editor-panel">
        <AceEditor name="scriptTextArea" mode="csharp" theme="ambiance" width="100%" minLines={15} maxLines={25}
          fontSize={16} showPrintMargin={false} showGutter highlightActiveLine value={script.code} tabSize={4}
          onChange={text => { revision.current++; script.replace(SpecialSymbolsHandler.replace(text, false)); setFileError(''); }}
          onLoad={editor => { editor.completers = [completer]; editor.container.querySelector('textarea')?.setAttribute('aria-label', 'TSL script'); }}
          enableBasicAutocompletion enableLiveAutocompletion setOptions={{ useWorker: false, enableSnippets: true, showLineNumbers: true }} />
      </div></section></div>
      <div className="col-md-3"><button type="button" className="btn btn-primary btn-lg" disabled={!script.code.trim() || request.pending}
        onClick={() => void request.execute('script', { expression: script.code })}>Process script</button></div>
    </div><ResultPanel {...request} heading="Output" />
  </>;
}
