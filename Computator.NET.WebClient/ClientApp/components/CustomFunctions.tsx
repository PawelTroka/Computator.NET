import { useLayoutEffect, useRef, useState } from 'react';
import AceEditor from 'react-ace';
import 'ace-builds/src-noconflict/mode-csharp';
import 'ace-builds/src-noconflict/theme-ambiance';
import { useCustomFunctions } from '../helpers/CustomFunctionsContext';

export function CustomFunctions() {
  const functions = useCustomFunctions();
  const [fileError, setFileError] = useState(''), [importing, setImporting] = useState(false);
  const revision = useRef(0);
  // Invalidate at the unmount commit, not in a deferred passive effect after the
  // replacement screen is visible. File reads can finish in that interval.
  useLayoutEffect(() => () => { revision.current++; }, []);

  function discardPendingImport() {
    // File.arrayBuffer has no abort API. Invalidate its result immediately instead
    // of letting a late read replace edits, cleared definitions or another file.
    revision.current++; setImporting(false); setFileError('');
  }
  function edit(code: string) { discardPendingImport(); functions.setCode(code); }
  async function importFile(file: File | undefined) {
    if (!file) return;
    const importingRevision = ++revision.current;
    setImporting(true); setFileError('');
    try {
      if (!file.name.toLowerCase().endsWith('.tslf') || file.size > 262144) throw new Error('Choose a UTF-8 .tslf file of at most 256 KiB.');
      const bytes = await file.arrayBuffer();
      if (revision.current !== importingRevision) return;
      const text = new TextDecoder('utf-8', { fatal: true }).decode(bytes).replace(/^\uFEFF/, '');
      if (text.length > 65536) throw new Error('Definitions exceed 65,536 characters.');
      functions.setCode(text); setFileError('');
    } catch (reason) {
      if (revision.current === importingRevision) setFileError(reason instanceof Error ? reason.message : 'The file could not be read.');
    } finally {
      if (revision.current === importingRevision) setImporting(false);
    }
  }
  function exportFile() {
    const url = URL.createObjectURL(new Blob([functions.code], { type: 'text/plain;charset=utf-8' }));
    const link = document.createElement('a'); link.href = url; link.download = 'custom-functions.tslf'; link.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
  return <><h1>Custom functions</h1>
    <p>These definitions are used by calculations, scripts, numerical methods and charts in this tab. Unsaved edits take effect on the next calculation.</p>
    <p>Run only code you trust. This is not a sandbox.</p>
    <AceEditor name="customFunctionsTextArea" mode="csharp" theme="ambiance" width="100%" minLines={12} maxLines={30}
      value={functions.code} onChange={edit} setOptions={{ useWorker: false }}
      onLoad={editor => { editor.container.querySelector('textarea')?.setAttribute('aria-label', 'Custom function definitions'); }} />
    <div className="d-flex flex-wrap gap-2 my-3">
      <button className="btn btn-primary" type="button" onClick={functions.save}>Save in this tab</button>
      <button className="btn btn-outline-secondary" type="button" onClick={exportFile}>Export .tslf</button>
      <label className="btn btn-outline-secondary">Import .tslf<input aria-label="Import .tslf" type="file" accept=".tslf"
        className="visually-hidden" onChange={event => { void importFile(event.target.files?.[0]); event.target.value = ''; }} /></label>
      {importing && <button className="btn btn-outline-secondary" type="button" onClick={discardPendingImport}>Cancel import</button>}
      <button className="btn btn-outline-danger" type="button" onClick={() => {
        if (confirm('Clear custom functions and the saved copy in this tab?')) { discardPendingImport(); functions.clear(); }
      }}>Clear definitions</button>
    </div>
    <p data-testid="definitions-import-state" aria-live="polite">{importing ? 'Reading definitions… Editing, clearing or leaving this page cancels this import.' : ''}</p>
    <p role="status">{functions.dirty ? 'Unsaved changes' : 'Saved state'}</p>
    {(fileError || functions.error) && <p role="alert">{fileError || functions.error}</p>}
  </>;
}
