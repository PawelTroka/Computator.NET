import { useEffect, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { Expression } from './Expression';
import { NumericInput } from './NumericInput';
import { ResultPanel } from './ResultPanel';
import { useTextResult } from '../helpers/useTextResult';
import { numericalOperations as operations, type NumericalOperation as Operation, useNumericalDraft } from '../helpers/NumericalDraftContext';
export function NumericalCalculations() {
  const { a, setA, b, setB, x, setX, epsilon, setEpsilon, n, setN, order, setOrder,
    expression, setExpression, operation, setOperation, selectedMethod, setSelectedMethod } = useNumericalDraft();
  const [methods, setMethods] = useState<Record<Operation, string[]>>({ Integral: [], Derivative: [], 'Function root': [] });
  const [loadError, setLoadError] = useState('');
  const request = useTextResult();
  const method = methods[operation].includes(selectedMethod) ? selectedMethod : methods[operation][0] ?? '';
  function compute() {
    // Hidden fields may retain invalid edits from another operation. Let the
    // server apply its defaults to parameters that this operation does not use.
    const parameters = operation === 'Integral' ? { a, b, n }
      : operation === 'Derivative' ? { x, order, epsilon }
      : { a, b, n, epsilon };
    void request.execute(operations[operation], { expression, method, ...parameters });
  }
  useEffect(() => {
    const controller = new AbortController();
    for (const name of Object.keys(operations) as Operation[]) {
      void fetch(`${WEB_API_BASE_URL}/numerical-calculations/${operations[name]}/list-methods`, { signal: controller.signal })
        .then(response => { if (!response.ok) throw new Error('Could not load numerical methods.'); return response.json(); })
        .then((values: unknown) => {
          if (!Array.isArray(values) || !values.every(value => typeof value === 'string')) throw new Error('Invalid numerical-method response.');
          if (!controller.signal.aborted) setMethods(previous => ({ ...previous, [name]: values }));
        }).catch(reason => { if (!controller.signal.aborted) setLoadError(reason instanceof Error ? reason.message : 'Could not load numerical methods.'); });
    }
    return () => controller.abort();
  }, []);
  return <><h1>Numerical calculations</h1><p>Try different numerical methods on your expression.</p>
    <Expression expression={expression} onExpressionChange={setExpression} />
    {loadError && <p role="alert" className="text-danger">{loadError}</p>}
    <div className="row g-3 mb-3"><div className="col-md-9"><div className="input-group mb-2">
      <label htmlFor="operation" className="input-group-text">Operation:</label>
      <select id="operation" className="form-select" value={operation} onChange={event => { setOperation(event.target.value as Operation); setSelectedMethod(''); }}>
        {(Object.keys(operations) as Operation[]).map(name => <option key={name}>{name}</option>)}
      </select></div><div className="input-group"><label htmlFor="method" className="input-group-text">Method:</label>
      <select id="method" className="form-select" value={method} disabled={!methods[operation].length} onChange={event => setSelectedMethod(event.target.value)}>
        {!method && <option value="">Loading…</option>}{methods[operation].map(name => <option key={name}>{name}</option>)}
      </select></div></div>
      <div className="col-md-3"><button type="button" className="btn btn-primary btn-lg" disabled={!expression.trim() || !method || request.pending}
        onClick={compute}>Compute!</button></div>
    </div><div className="row g-3"><div className="col-md-4"><section className="card">
      <div className="card-header">{operation === 'Derivative' ? 'Derivative at point x:' : 'Interval:'}</div><div className="card-body">
        {operation === 'Derivative' ? <><NumericInput label="x" value={x} onValueChanged={setX} /><NumericInput label="order" value={order} onValueChanged={setOrder} /></>
          : <><NumericInput label="a" value={a} onValueChanged={setA} /><NumericInput label="b" value={b} onValueChanged={setB} /></>}
      </div></section></div>
      {operation !== 'Integral' && <div className="col-md-4"><section className="card"><div className="card-header">Max error:</div><div className="card-body">
        <NumericInput label="ε" value={epsilon} onValueChanged={setEpsilon} stepValue={1e-6} /></div></section></div>}
      {operation !== 'Derivative' && <div className="col-md-4"><section className="card"><div className="card-header">Number of steps:</div><div className="card-body">
        <NumericInput label="N" value={n} onValueChanged={setN} /></div></section></div>}
    </div><ResultPanel {...request} />
  </>;
}
