import { useCallback, useEffect, useRef, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { useCustomFunctions } from '../helpers/CustomFunctionsContext';
import { Expression } from './Expression';
import { NumericInput } from './NumericInput';
export function Chart() {
  const [expression, setExpression] = useState('');
  const [xMin, setXMin] = useState(-5), [xMax, setXMax] = useState(5), [yMin, setYMin] = useState(-5), [yMax, setYMax] = useState(5);
  const [source, setSource] = useState<string>(), [error, setError] = useState(''), [pending, setPending] = useState(false);
  const container = useRef<HTMLDivElement>(null), active = useRef<AbortController | null>(null), imageUrl = useRef<string | null>(null);
  const functions = useCustomFunctions();
  const draw = useCallback(async () => {
    if (!expression.trim() || !container.current) return;
    if (xMin >= xMax || yMin >= yMax) { setError('Each minimum must be smaller than its maximum.'); return; }
    active.current?.abort(); const controller = new AbortController(); active.current = controller;
    setError(''); setPending(true);
    try {
      const width = Math.min(2048, Math.max(200, Math.round(container.current.clientWidth)));
      const height = Math.min(2048, Math.max(200, Math.round(window.innerHeight - container.current.getBoundingClientRect().top)));
      const response = await fetch(`${WEB_API_BASE_URL}/v2/chart`, { method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ expression, customFunctions: functions.code, width, height, xMin, xMax, yMin, yMax }), signal: controller.signal });
      if (!response.ok) { const problem = await response.json() as { title?: string; detail?: string }; throw new Error(problem.detail || problem.title || 'Chart generation failed.'); }
      if (!response.headers.get('content-type')?.startsWith('image/png')) throw new Error('Invalid chart response.');
      const image = await response.blob();
      if (!controller.signal.aborted) {
        if (imageUrl.current) URL.revokeObjectURL(imageUrl.current);
        imageUrl.current = URL.createObjectURL(image); setSource(imageUrl.current);
      }
    } catch (reason) { if (!controller.signal.aborted) setError(reason instanceof Error ? reason.message : 'The chart could not be generated.'); }
    finally { if (!controller.signal.aborted) setPending(false); }
  }, [expression, xMin, xMax, yMin, yMax, functions.code]);
  useEffect(() => {
    let timeout: ReturnType<typeof setTimeout>;
    const resize = () => { clearTimeout(timeout); timeout = setTimeout(() => { void draw(); }, 200); };
    window.addEventListener('resize', resize);
    return () => { clearTimeout(timeout); window.removeEventListener('resize', resize); };
  }, [draw]);
  useEffect(() => () => { active.current?.abort(); if (imageUrl.current) URL.revokeObjectURL(imageUrl.current); }, []);
  return <><h1>Chart</h1><p>Write an expression and draw its chart.</p><Expression expression={expression} onExpressionChange={setExpression} />
    {error && <p role="alert" className="text-danger">{error}</p>}
    <div className="row g-3"><div className="col-md-9" ref={container}>
      {source ? <img className="chart-image" src={source} alt={expression} /> : <p>Enter an expression and choose Draw chart.</p>}
    </div><div className="col-md-3">
      <button type="button" className="btn btn-primary btn-lg mb-3" disabled={!expression.trim() || pending} onClick={() => void draw()}>Draw chart!</button>
      <h2 className="h6">Chart area values:</h2><ul className="list-group">
        <li className="list-group-item"><NumericInput label="X" subscriptLabel="MIN" value={xMin} onValueChanged={setXMin} /></li>
        <li className="list-group-item"><NumericInput label="X" subscriptLabel="MAX" value={xMax} onValueChanged={setXMax} /></li>
        <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MIN" value={yMin} onValueChanged={setYMin} /></li>
        <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MAX" value={yMax} onValueChanged={setYMax} /></li>
      </ul></div></div></>;
}
