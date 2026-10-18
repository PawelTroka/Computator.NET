import { useEffect, useRef, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { useCustomFunctions } from './CustomFunctionsContext';

function problemMessage(body: unknown, fallback: string) {
  if (!body || typeof body !== 'object' || Array.isArray(body)) return fallback;
  const problem = body as { detail?: unknown; title?: unknown; errors?: unknown };
  const messages: string[] = [];
  if (typeof problem.detail === 'string' && problem.detail.trim()) messages.push(problem.detail.trim());
  if (problem.errors && typeof problem.errors === 'object' && !Array.isArray(problem.errors)) {
    for (const values of Object.values(problem.errors)) {
      if (Array.isArray(values)) {
        for (const value of values) if (typeof value === 'string' && value.trim()) messages.push(value.trim());
      }
    }
  }
  if (messages.length) return [...new Set(messages)].join(' ');
  return typeof problem.title === 'string' && problem.title.trim() ? problem.title.trim() : fallback;
}

export function useTextResult() {
  const [result, setResult] = useState(''), [error, setError] = useState(''), [pending, setPending] = useState(false);
  const active = useRef<AbortController | null>(null);
  const functions = useCustomFunctions();
  useEffect(() => () => active.current?.abort(), []);
  function cancel() {
    active.current?.abort(); active.current = null;
    setPending(false); setError('Execution cancelled.');
  }
  async function execute(operation: string, parameters: Record<string, unknown>) {
    active.current?.abort();
    const controller = new AbortController(); active.current = controller;
    setPending(true); setError(''); setResult('');
    try {
      const response = await fetch(`${WEB_API_BASE_URL}/v2/execute`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...parameters, operation, customFunctions: functions.code }), signal: controller.signal
      });
      const fallback = `Request failed (${response.status}).`;
      let body: unknown;
      try { body = await response.json(); }
      catch { throw new Error(response.ok ? 'Invalid execution response.' : fallback); }
      if (!response.ok) throw new Error(problemMessage(body, fallback));
      if (!body || typeof body !== 'object' || !('result' in body) || typeof body.result !== 'string') throw new Error('Invalid execution response.');
      if (!controller.signal.aborted) setResult(body.result);
    } catch (reason) {
      if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : 'The request failed.');
    } finally { if (!controller.signal.aborted) setPending(false); }
  }
  return { result, error, pending, execute, cancel };
}
