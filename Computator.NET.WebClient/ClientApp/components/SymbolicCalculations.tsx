import { useEffect, useRef, useState, type FormEvent } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { initialSymbolicDraft, symbolicOperations, useSymbolicDraft, type SymbolicDraft, type SymbolicOperation } from '../helpers/SymbolicDraftContext';
import { SpecialSymbolsHandler } from '../helpers/SpecialSymbolsHandler';

type SymbolicResult = { result: string; latex: string; status: string; warnings: string[] };
const statuses: Record<string, string> = {
  exact: 'Exact result', approximate: 'Numerical approximation', unevaluated: 'Unresolved result', undefined: 'Undefined result'
};
const examples: Record<SymbolicOperation, Partial<SymbolicDraft>> = {
  simplify: { expression: '(x + 1)² - x²' }, expand: { expression: '(x + y)³' }, factor: { expression: 'x² - 1' },
  differentiate: { expression: 'sin(x²)', variable: 'x', order: '1' },
  integrate: { expression: 'x²', variable: 'x' },
  'definite-integrate': { expression: 'x²', variable: 'x', lowerBound: '0', upperBound: '1' },
  limit: { expression: 'sin(x)/x', variable: 'x', point: '0', direction: 'both' },
  solve: { expression: 'x² = 2', variable: 'x', domain: 'real' },
  'solve-system': { equations: 'x + y = 3\nx - y = 1', variables: 'x, y' },
  substitute: { expression: 'x² + y', substitutions: 'x = 2\ny = 3' },
  evaluate: { expression: 'sqrt(2)', precision: '30', substitutions: '' },
  taylor: { expression: 'sin(x)', variable: 'x', point: '0', order: '5' },
  determinant: { expression: '[[1, 2], [3, 4]]' }, inverse: { expression: '[[1, 2], [3, 4]]' },
  transpose: { expression: '[[1, 2, 3], [4, 5, 6]]' }, trace: { expression: '[[1, 2], [3, 4]]' }
};

function integer(text: string, label: string, minimum: number, maximum: number) {
  if (!/^\d+$/.test(text) || Number(text) < minimum || Number(text) > maximum)
    throw new Error(`${label} must be an integer from ${minimum} to ${maximum}.`);
  return Number(text);
}

// Each replacement is an expression; all substitutions are simultaneous. A
// null-prototype map also preserves mathematical names such as "constructor".
function substitutions(text: string, required = true) {
  const values: Record<string, string> = Object.create(null);
  for (const line of text.split(/\r?\n/).filter(line => line.trim())) {
    const match = /^\s*([\p{L}_][\p{L}\p{N}_]*)\s*=\s*(.+)$/u.exec(line);
    if (!match) throw new Error('Enter one substitution per line, for example x = y + 1.');
    if (Object.hasOwn(values, match[1])) throw new Error(`The substitution for ${match[1]} is repeated.`);
    values[match[1]] = match[2].trim();
  }
  if (required && !Object.keys(values).length) throw new Error('Enter at least one substitution.');
  return values;
}

function parameters(draft: SymbolicDraft) {
  const { operation, expression, variable, point } = draft;
  const request: Record<string, unknown> = { operation, expression, domain: draft.domain };
  if (['differentiate', 'integrate', 'definite-integrate', 'limit', 'solve', 'taylor'].includes(operation)) request.variable = variable;
  if (operation === 'differentiate' || operation === 'taylor')
    request.order = integer(draft.order, operation === 'taylor' ? 'Degree' : 'Order', operation === 'taylor' ? 0 : 1, 20);
  if (operation === 'limit' || operation === 'taylor') request.point = point;
  if (operation === 'limit') request.direction = draft.direction;
  if (operation === 'definite-integrate') { request.lowerBound = draft.lowerBound; request.upperBound = draft.upperBound; }
  if (operation === 'substitute') request.substitutions = substitutions(draft.substitutions);
  if (operation === 'evaluate') {
    request.precision = integer(draft.precision, 'Precision', 1, 100);
    request.substitutions = substitutions(draft.substitutions, false);
  }
  if (operation === 'solve-system') {
    delete request.expression;
    request.equations = draft.equations.split(/\r?\n/).map(line => line.trim()).filter(Boolean);
    request.variables = draft.variables.split(',').map(value => value.trim()).filter(Boolean);
  }
  return request;
}

function problem(body: unknown, fallback: string) {
  if (body && typeof body === 'object') {
    if ('detail' in body && typeof body.detail === 'string' && body.detail) return body.detail;
    if ('errors' in body && body.errors && typeof body.errors === 'object') {
      const values = Object.values(body.errors).flat().filter(value => typeof value === 'string');
      if (values.length) return values.join(' ');
    }
    if ('title' in body && typeof body.title === 'string') return body.title;
  }
  return fallback;
}
function isResult(value: unknown): value is SymbolicResult {
  if (!value || typeof value !== 'object') return false;
  const body = value as Partial<SymbolicResult>;
  return typeof body.result === 'string' && typeof body.latex === 'string' && typeof body.status === 'string' &&
    Object.hasOwn(statuses, body.status) && Array.isArray(body.warnings) && body.warnings.every(value => typeof value === 'string');
}

export function SymbolicCalculations() {
  const { draft, setDraft } = useSymbolicDraft();
  const [result, setResult] = useState<SymbolicResult | null>(null), [error, setError] = useState('');
  const [pending, setPending] = useState(false), [copied, setCopied] = useState('');
  const active = useRef<AbortController | null>(null);
  const expressionInput = useRef<HTMLTextAreaElement | null>(null);
  useEffect(() => () => active.current?.abort(), []);
  function cancel(message = 'Calculation cancelled.') {
    active.current?.abort(); active.current = null; setPending(false); setError(message);
  }
  function edit(patch: Partial<SymbolicDraft>) {
    cancel(''); setResult(null); setCopied(''); setDraft({ ...draft, ...patch });
  }
  function insertSymbol(symbol: string) {
    const input = expressionInput.current;
    if (!input) return;
    const start = input.selectionStart, end = input.selectionEnd;
    const value = input.value.slice(0, start) + symbol + input.value.slice(end);
    if (value.length > 8192) { setError('Expressions are limited to 8,192 characters.'); return; }
    edit(draft.operation === 'solve-system' ? { equations: value } : { expression: value });
    requestAnimationFrame(() => {
      input.focus(); input.setSelectionRange(start + symbol.length, start + symbol.length);
    });
  }
  async function compute(event: FormEvent) {
    event.preventDefault(); cancel(''); setResult(null); setCopied('');
    const controller = new AbortController(); active.current = controller;
    try {
      const body = parameters(draft); setPending(true);
      const response = await fetch(`${WEB_API_BASE_URL}/v2/symbolic`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body), signal: controller.signal
      });
      let answer: unknown;
      try { answer = await response.json(); } catch { throw new Error(`Invalid symbolic response (${response.status}).`); }
      if (!response.ok) throw new Error(problem(answer, `Calculation failed (${response.status}).`));
      if (!isResult(answer)) throw new Error('Invalid symbolic response.');
      if (active.current === controller && !controller.signal.aborted) setResult(answer);
    } catch (reason) {
      if (active.current === controller && !controller.signal.aborted)
        setError(reason instanceof Error ? reason.message : 'The calculation failed.');
    } finally {
      if (active.current === controller) { active.current = null; setPending(false); }
    }
  }
  async function copy(value: string, label: string) {
    try { await navigator.clipboard.writeText(value); setCopied(`${label} copied.`); }
    catch { setCopied('Clipboard unavailable. Select the result text to copy it.'); }
  }
  const variable = ['differentiate', 'integrate', 'definite-integrate', 'limit', 'solve', 'taylor'].includes(draft.operation);
  return <section className="symbolic-workspace"><h1>Symbolic calculations</h1>
    <p>Work with exact expressions, equations, calculus and matrices.</p>
    <form onSubmit={event => void compute(event)} onKeyDown={event => {
      if ((event.ctrlKey || event.metaKey) && event.key === 'Enter') {
        event.preventDefault(); if (!pending) event.currentTarget.requestSubmit();
      }
    }}>
      <div className="row g-3"><div className="col-lg-8"><section className="card">
        <div className="card-header">Expression and operation</div><div className="card-body">
          <label className="form-label" htmlFor="symbolic-operation">Operation</label>
          <select id="symbolic-operation" className="form-select mb-3" value={draft.operation}
            onChange={event => edit({ operation: event.target.value as SymbolicOperation })}>
            {Object.entries(symbolicOperations).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
          </select>
          {draft.operation === 'solve-system' ? <>
            <label className="form-label" htmlFor="symbolic-equations">Equations — one per line</label>
            <textarea ref={expressionInput} id="symbolic-equations" className="form-control symbolic-input" value={draft.equations} rows={4}
              maxLength={8192} onChange={event => edit({ equations: event.target.value })} required />
            <label className="form-label mt-3" htmlFor="symbolic-variables">Variables — comma separated</label>
            <input id="symbolic-variables" className="form-control" value={draft.variables} maxLength={1024}
              onChange={event => edit({ variables: event.target.value })} required />
          </> : <>
            <label className="form-label" htmlFor="symbolic-expression">{draft.operation === 'solve' ? 'Equation or expression equal to zero' : 'Expression'}</label>
            <textarea ref={expressionInput} id="symbolic-expression" className="form-control symbolic-input" value={draft.expression} rows={4}
              maxLength={8192} onChange={event => edit({ expression: event.target.value })} required />
          </>}
          <div className="d-flex align-items-center flex-wrap gap-2 mt-2" role="group" aria-label="Insert TSL notation">
            <span className="small text-muted">Insert:</span>
            <button type="button" className="btn btn-sm btn-outline-secondary" aria-label="Insert multiplication dot"
              onClick={() => insertSymbol(SpecialSymbolsHandler.DotSymbol)}>·</button>
            <button type="button" className="btn btn-sm btn-outline-secondary" aria-label="Insert squared exponent"
              onClick={() => insertSymbol('²')}>²</button>
            <button type="button" className="btn btn-sm btn-outline-secondary" aria-label="Insert cubed exponent"
              onClick={() => insertSymbol('³')}>³</button>
          </div>
          <p className="small text-muted mt-2 mb-0">Use TSL notation: <code>2·x²</code>, <code>x⁻²</code>, <code>x⁽ⁿ⁺¹⁾</code>, <code>sin²(x)</code>, <code>PI</code> and exact fractions.
            Matrices use <code>[[1, 2], [3, 4]]</code>. Ctrl+Enter calculates.</p>
          <p className="small text-muted mt-2 mb-0">Logarithms use <code>log(argument, base)</code>: <code>log(8, 2) = 3</code>.
            Use <code>log(x)</code> for base 10 and <code>ln(x)</code> for the natural logarithm.</p>
        </div></section></div>
        <div className="col-lg-4"><section className="card"><div className="card-header">Options</div><div className="card-body">
          {variable && <><label className="form-label" htmlFor="symbolic-variable">Variable</label>
            <input id="symbolic-variable" className="form-control mb-3" value={draft.variable} maxLength={64}
              onChange={event => edit({ variable: event.target.value })} required /></>}
          {(draft.operation === 'differentiate' || draft.operation === 'taylor') && <>
            <label className="form-label" htmlFor="symbolic-order">{draft.operation === 'taylor' ? 'Degree' : 'Order'}</label>
            <input id="symbolic-order" type="number" className="form-control mb-3" min={draft.operation === 'taylor' ? 0 : 1} max={20} step={1}
              value={draft.order} onChange={event => edit({ order: event.target.value })} required /></>}
          {(draft.operation === 'limit' || draft.operation === 'taylor') && <>
            <label className="form-label" htmlFor="symbolic-point">{draft.operation === 'taylor' ? 'Expansion point' : 'Approach point'}</label>
            <input id="symbolic-point" className="form-control mb-3" value={draft.point} maxLength={8192}
              onChange={event => edit({ point: event.target.value })} required /></>}
          {draft.operation === 'limit' && <><label className="form-label" htmlFor="symbolic-direction">Direction</label>
            <select id="symbolic-direction" className="form-select mb-3" value={draft.direction} onChange={event => edit({ direction: event.target.value })}>
              <option value="both">Both sides</option><option value="left">From the left</option><option value="right">From the right</option>
            </select><p className="small text-muted">Use <code>∞</code> or <code>-∞</code> for infinite limits.</p></>}
          <label className="form-label" htmlFor="symbolic-domain">Domain</label>
            <select id="symbolic-domain" className="form-select mb-3" value={draft.domain} onChange={event => edit({ domain: event.target.value })}>
              <option value="complex">Complex numbers</option><option value="real">Real numbers</option>
            </select>
          {draft.operation === 'definite-integrate' && <>
            <label className="form-label" htmlFor="symbolic-lower">Lower bound</label>
            <input id="symbolic-lower" className="form-control mb-3" value={draft.lowerBound} maxLength={8192}
              onChange={event => edit({ lowerBound: event.target.value })} required />
            <label className="form-label" htmlFor="symbolic-upper">Upper bound</label>
            <input id="symbolic-upper" className="form-control mb-3" value={draft.upperBound} maxLength={8192}
              onChange={event => edit({ upperBound: event.target.value })} required /></>}
          {['substitute', 'evaluate'].includes(draft.operation) && <><label className="form-label" htmlFor="symbolic-substitutions">
            {draft.operation === 'evaluate' ? 'Optional substitutions — one per line' : 'Substitutions — one per line'}</label>
            <textarea id="symbolic-substitutions" rows={5} className="form-control symbolic-input mb-3" value={draft.substitutions} maxLength={8192}
              onChange={event => edit({ substitutions: event.target.value })} required={draft.operation === 'substitute'} />
            <p className="small text-muted">Use <code>x = 2</code> or <code>x = y + 1</code>. Replacements are simultaneous.</p></>}
          {draft.operation === 'evaluate' && <><label className="form-label" htmlFor="symbolic-precision">Decimal precision</label>
            <input id="symbolic-precision" type="number" className="form-control mb-3" min={1} max={100} step={1} value={draft.precision}
              onChange={event => edit({ precision: event.target.value })} required /></>}
          {!variable && !['evaluate', 'substitute'].includes(draft.operation) && <p className="text-muted">This operation uses the expression as entered.</p>}
          <div className="d-flex flex-wrap gap-2"><button className="btn btn-primary" type="submit" disabled={pending}>Calculate</button>
            <button className="btn btn-outline-primary" type="button" onClick={() => edit(examples[draft.operation])}>Load example</button>
          </div>
        </div></section></div>
      </div>
    </form>
    <section className="card" aria-label="Symbolic result"><div className="card-header d-flex justify-content-between align-items-center">
      <span>{result ? statuses[result.status] : 'Result'}</span>{result && <button type="button" className="btn btn-sm btn-outline-primary"
        onClick={() => void copy(result.result, 'Result')}>Copy result</button>}</div><div className="card-body">
      {error && <p className="text-danger" role="alert">{error}</p>}
      <div className="result-text" role="status" aria-live="polite">{pending ? 'Calculating…' : result?.result ?? 'Choose an operation and calculate an expression.'}</div>
      {pending && <button className="btn btn-outline-secondary mt-3" type="button" onClick={() => cancel()}>Cancel calculation</button>}
      {result && result.warnings.length > 0 && <ul className="mt-3 mb-0 symbolic-warnings">{result.warnings.map((warning, index) => <li key={index}>{warning}</li>)}</ul>}
      {result?.latex && <details className="mt-3"><summary>LaTeX</summary><pre className="symbolic-latex mt-2">{result.latex}</pre>
        <button className="btn btn-sm btn-outline-primary" type="button" onClick={() => void copy(result.latex, 'LaTeX')}>Copy LaTeX</button></details>}
      {copied && <p className="small mt-2 mb-0" aria-live="polite">{copied}</p>}
    </div></section>
    <details className="small text-muted"><summary>Syntax and result conventions</summary>
      <p className="mt-2">Symbolic calculations use TSL mathematical notation. Paste superscripts and middle-dot multiplication directly,
        or use the insertion buttons. The symbolic input also accepts ASCII <code>x^2</code> and <code>2*x</code>.
        Custom C# / TSL function definitions and scripts are not applied here.
        Exact fractions stay exact; numerical evaluation uses the requested precision. Unresolved expressions remain visible with an explicit status.
        Indefinite integrals include an arbitrary additive constant <code>C</code>.</p>
      <p>Two-argument logarithms follow TSL argument order: <code>log(8, 2)</code> is 3.
        Result expressions use the engine's TSL notation and can be copied back into this tool.
        Exponents without a superscript spelling use <code>pow(base, exponent)</code>.</p>
      <p>Calculations have an expression limit and a time budget. If a request cannot finish, reduce its size or order and try again.
        Inputs stay in this tab when you navigate to another tool.</p>
      <button className="btn btn-sm btn-outline-secondary" type="button" onClick={() => edit(initialSymbolicDraft)}>Reset symbolic inputs</button>
    </details>
  </section>;
}
