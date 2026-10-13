import { useEffect, useRef, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { useCustomFunctions } from './CustomFunctionsContext';

export function useTextResult() {
  const [result, setResult] = useState(''), [error, setError] = useState(''), [pending, setPending] = useState(false);
  const active = useRef<AbortController | null>(null);
  const functions = useCustomFunctions();
  useEffect(() => () => active.current?.abort(), []);
  async function execute(operation: string, parameters: Record<string, unknown>) {
    active.current?.abort();
    const controller = new AbortController(); active.current = controller;
    setPending(true); setError(''); setResult('');
    try {
      const response = await fetch(`${WEB_API_BASE_URL}/v2/execute`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...parameters, operation, customFunctions: functions.code }), signal: controller.signal
      });
      const body: unknown = await response.json();
      if (!response.ok) {
        const problem = body as { detail?: string; title?: string };
        throw new Error(problem.detail || problem.title || `Request failed (${response.status}).`);
      }
      if (!body || typeof body !== 'object' || !('result' in body) || typeof body.result !== 'string') throw new Error('Invalid execution response.');
      if (!controller.signal.aborted) setResult(body.result);
    } catch (reason) {
      if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : 'The request failed.');
    } finally { if (!controller.signal.aborted) setPending(false); }
  }
  return { result, error, pending, execute };
}
