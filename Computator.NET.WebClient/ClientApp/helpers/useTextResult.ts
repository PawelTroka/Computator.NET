import { useEffect, useRef, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';

export function useTextResult() {
  const [result, setResult] = useState('');
  const [error, setError] = useState('');
  const [pending, setPending] = useState(false);
  const active = useRef<AbortController | null>(null);
  useEffect(() => () => active.current?.abort(), []);
  async function run(path: string) {
    active.current?.abort();
    const controller = new AbortController();
    active.current = controller;
    setPending(true); setError(''); setResult('');
    try {
      const response = await fetch(`${WEB_API_BASE_URL}/${path}`, { signal: controller.signal });
      const body = await response.text();
      if (!response.ok) throw new Error(body || `Request failed (${response.status}).`);
      if (!controller.signal.aborted) setResult(body);
    } catch (reason) {
      if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : 'The request failed.');
    } finally {
      if (!controller.signal.aborted) setPending(false);
    }
  }
  return { result, error, pending, run };
}
