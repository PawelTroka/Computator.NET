import { createContext, useContext, useState, type ReactNode } from 'react';
const key = 'computator.custom-functions.v1';
const maximum = 65536;
type State = { code: string; setCode: (code: string) => void; save: () => void; clear: () => void; dirty: boolean; error: string };
const Context = createContext<State | null>(null);
function read() {
  try {
    const raw = sessionStorage.getItem(key);
    if (!raw) return { code: '', error: '' };
    const value: unknown = JSON.parse(raw);
    if (!value || typeof value !== 'object' || !('schemaVersion' in value) || value.schemaVersion !== 1 ||
        !('code' in value) || typeof value.code !== 'string' || value.code.length > maximum) throw new Error();
    return { code: value.code, error: '' };
  } catch { return { code: '', error: 'Stored definitions could not be read. They have not been overwritten. Clear explicitly to start again.' }; }
}
export function CustomFunctionsProvider({ children }: { children: ReactNode }) {
  const [initial] = useState(read);
  const [code, update] = useState(initial.code), [saved, setSaved] = useState(initial.code);
  const [error, setError] = useState(initial.error), [blocked, setBlocked] = useState(Boolean(initial.error));
  function setCode(next: string) { if (next.length <= maximum) update(next); else setError('Definitions exceed 65,536 characters.'); }
  function save() {
    if (blocked) { setError('Stored definitions could not be read. Clear explicitly before replacing them.'); return; }
    try { sessionStorage.setItem(key, JSON.stringify({ schemaVersion: 1, code })); setSaved(code); setError(''); }
    catch { setError('The browser could not save this tab’s definitions. Export a .tslf copy instead.'); }
  }
  function clear() {
    try { sessionStorage.removeItem(key); update(''); setSaved(''); setBlocked(false); setError(''); }
    catch { setError('The browser could not clear stored definitions.'); }
  }
  return <Context.Provider value={{ code, setCode, save, clear, dirty: code !== saved, error }}>{children}</Context.Provider>;
}
export function useCustomFunctions() {
  const context = useContext(Context);
  if (!context) throw new Error('CustomFunctionsProvider is missing.');
  return context;
}
