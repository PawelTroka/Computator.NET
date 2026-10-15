import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';

export const scriptStorageKey = 'computator.script-document.v1';
export const maximumScriptCharacters = 65536;
export const maximumScriptBytes = 262144;
type Document = { code: string; name: string };
type State = Document & { error: string; unstored: boolean; replace: (code: string, name?: string) => void; clear: () => void };
const Context = createContext<State | null>(null);

function validName(name: string) {
  return name.length <= 180 && /\.tsl$/i.test(name) && !/[\\/\x00-\x1f\x7f]/.test(name);
}
function read(): Document & { error: string; blocked: boolean } {
  try {
    const raw = sessionStorage.getItem(scriptStorageKey);
    if (raw === null) return { code: '', name: 'script.tsl', error: '', blocked: false };
    const value: unknown = JSON.parse(raw);
    if (!value || typeof value !== 'object' || !('schemaVersion' in value) || value.schemaVersion !== 1 ||
        !('code' in value) || typeof value.code !== 'string' || value.code.length > maximumScriptCharacters ||
        !('name' in value) || typeof value.name !== 'string' || !validName(value.name)) throw new Error();
    return { code: value.code, name: value.name, error: '', blocked: false };
  } catch {
    return { code: '', name: 'script.tsl', blocked: true,
      error: 'The stored script could not be read and has not been overwritten. Export your current work, then choose New script to reset it.' };
  }
}

// Lives above Routes: a denied/full browser store must not also lose the in-memory
// draft when the user visits Custom functions or another calculation screen.
export function ScriptDocumentProvider({ children }: { children: ReactNode }) {
  const [initial] = useState(read);
  const [document, setDocument] = useState<Document>({ code: initial.code, name: initial.name });
  const [blocked, setBlocked] = useState(initial.blocked);
  const [error, setError] = useState(initial.error);
  const [unstored, setUnstored] = useState(false);
  function replace(code: string, name = document.name) {
    if (code.length > maximumScriptCharacters || !validName(name)) {
      setError('Choose a .tsl document of at most 65,536 characters with a valid file name. The draft has not been replaced.');
      return;
    }
    setDocument({ code, name });
    setUnstored(true);
    if (blocked) {
      setError('The unreadable stored script is preserved. Export your current work before reloading, or choose New script to reset storage.');
      return;
    }
    try {
      sessionStorage.setItem(scriptStorageKey, JSON.stringify({ schemaVersion: 1, code, name }));
      setUnstored(false); setError('');
    } catch {
      setError('The browser could not save this draft. It remains in memory during navigation; export a .tsl copy before reloading or closing this tab.');
    }
  }
  function clear() {
    try {
      sessionStorage.removeItem(scriptStorageKey);
      setDocument({ code: '', name: 'script.tsl' }); setBlocked(false); setUnstored(false); setError('');
    } catch { setError('The browser could not clear the stored script. Your current draft has been kept.'); }
  }
  useEffect(() => {
    if (!unstored) return;
    const warn = (event: BeforeUnloadEvent) => { event.preventDefault(); event.returnValue = ''; };
    window.addEventListener('beforeunload', warn);
    return () => window.removeEventListener('beforeunload', warn);
  }, [unstored]);
  return <Context.Provider value={{ ...document, error, unstored, replace, clear }}>{children}</Context.Provider>;
}
export function useScriptDocument() {
  const state = useContext(Context);
  if (!state) throw new Error('ScriptDocumentProvider is missing.');
  return state;
}
