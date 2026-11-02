import { useCallback, useEffect, useRef, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { useCustomFunctions } from '../helpers/CustomFunctionsContext';
import { useChartDraft } from '../helpers/ToolDraftContext';
import { Expression } from './Expression';
import { NumericInput } from './NumericInput';
import { SurfacePlot, isSurfaceGrid, type SurfaceGrid } from './SurfacePlot';

type Plot = { expression: string; customFunctions: string; xMin: number; xMax: number; yMin: number; yMax: number; gridSize: number };
type RenderedPlot = { kind: 'image'; url: string; expression: string } | { kind: 'surface'; grid: SurfaceGrid; expression: string };

export function Chart() {
  const { expression, setExpression, xMin, setXMin, xMax, setXMax, yMin, setYMin, yMax, setYMax } = useChartDraft();
  const [rendered, setRendered] = useState<RenderedPlot>();
  const [gridSize, setGridSize] = useState(65);
  const [error, setError] = useState(''), [pending, setPending] = useState(false), [status, setStatus] = useState('');
  const container = useRef<HTMLDivElement>(null);
  const active = useRef<AbortController | null>(null), imageUrl = useRef<string | null>(null);
  const requested = useRef<Plot | null>(null);
  // A surface resizes client-side without recompiling the user's function.
  const shownSurface = useRef<Plot | null>(null);
  const functions = useCustomFunctions();

  const renderPlot = useCallback(async (plot: Plot) => {
    if (!container.current) return;
    active.current?.abort();
    const controller = new AbortController(); active.current = controller;
    setError(''); setPending(true); setStatus('Rendering chart…');
    try {
      const width = Math.min(2048, Math.max(200, Math.round(container.current.clientWidth)));
      const height = Math.min(2048, Math.max(200, Math.round(window.innerHeight - container.current.getBoundingClientRect().top)));
      const response = await fetch(`${WEB_API_BASE_URL}/v2/chart`, {
        method: 'POST', headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...plot, width, height }), signal: controller.signal
      });
      if (!response.ok) {
        let detail = `Chart generation failed (${response.status}).`;
        if (response.headers.get('content-type')?.includes('json')) {
          try {
            const problem = await response.json() as { title?: unknown; detail?: unknown };
            if (typeof problem.detail === 'string') detail = problem.detail;
            else if (typeof problem.title === 'string') detail = problem.title;
          } catch { /* Preserve the HTTP error when a proxy returns malformed JSON. */ }
        }
        throw new Error(detail);
      }
      const contentType = response.headers.get('content-type')?.toLowerCase() ?? '';
      if (contentType.includes('application/json')) {
        const json: unknown = await response.json();
        if (!isSurfaceGrid(json)) throw new Error('Invalid 3D surface response.');
        if (active.current !== controller || controller.signal.aborted) return;
        if (imageUrl.current) URL.revokeObjectURL(imageUrl.current);
        imageUrl.current = null;
        shownSurface.current = plot;
        setRendered({ kind: 'surface', grid: json, expression: plot.expression });
        setStatus('3D surface samples ready.');
        return;
      }
      if (!contentType.startsWith('image/png')) throw new Error('Invalid chart response.');
      const image = await response.blob();
      if (active.current !== controller || controller.signal.aborted) return;
      const url = URL.createObjectURL(image);
      if (imageUrl.current) URL.revokeObjectURL(imageUrl.current);
      imageUrl.current = url;
      shownSurface.current = null;
      setRendered({ kind: 'image', url, expression: plot.expression });
      setStatus('Chart ready.');
    } catch (reason) {
      if (active.current === controller && !controller.signal.aborted) {
        setError(reason instanceof Error ? reason.message : 'The chart could not be generated.');
        setStatus('Chart generation failed.');
        // Resizing must not repeatedly execute a failed definition.
        requested.current = null;
      }
    } finally {
      if (active.current === controller) { active.current = null; setPending(false); }
    }
  }, []);

  function draw() {
    if (!expression.trim()) return;
    if (![xMin, xMax, yMin, yMax].every(Number.isFinite) || xMin >= xMax || yMin >= yMax) {
      setError('Each minimum must be smaller than its maximum, and all bounds must be finite.'); return;
    }
    const plot = { expression, customFunctions: functions.code, xMin, xMax, yMin, yMax, gridSize };
    requested.current = plot;
    void renderPlot(plot);
  }
  function cancel() {
    // Invalidate before abort: a late response/finally cannot change the next run.
    const controller = active.current; active.current = null; requested.current = null;
    controller?.abort(); setPending(false); setError(''); setStatus('Chart rendering cancelled.');
  }
  useEffect(() => {
    let timeout: ReturnType<typeof setTimeout>;
    const resize = () => {
      clearTimeout(timeout);
      timeout = setTimeout(() => {
        // Resize only the explicitly requested plot, never unsubmitted editor text.
        if (requested.current && requested.current !== shownSurface.current) void renderPlot(requested.current);
      }, 200);
    };
    window.addEventListener('resize', resize);
    return () => { clearTimeout(timeout); window.removeEventListener('resize', resize); };
  }, [renderPlot]);
  useEffect(() => () => {
    requested.current = null; const controller = active.current; active.current = null; controller?.abort();
    if (imageUrl.current) URL.revokeObjectURL(imageUrl.current);
  }, []);

  return <><h1>Chart</h1><p>Write an expression and draw its chart.</p><Expression expression={expression} onExpressionChange={setExpression} />
    {error && <p role="alert" className="text-danger">{error}</p>}
    <p role="status" aria-live="polite">{status}</p>
    <div className="row g-3"><div className="col-md-9" ref={container} aria-busy={pending}>
      {rendered?.kind === 'image' ? <figure><img className="chart-image" src={rendered.url} alt={rendered.expression} />
        <figcaption>Rendered expression: <code>{rendered.expression}</code></figcaption></figure>
        : rendered?.kind === 'surface' ? <SurfacePlot grid={rendered.grid} expression={rendered.expression} />
        : <p>Enter an expression and choose Draw chart.</p>}
    </div><div className="col-md-3">
      <div className="d-flex flex-wrap gap-2 mb-3">
        <button type="button" className="btn btn-primary btn-lg" disabled={!expression.trim() || pending} onClick={draw}>Draw chart!</button>
        {pending && <button type="button" className="btn btn-outline-danger" onClick={cancel}>Cancel chart</button>}
        {rendered?.kind === 'image' && <a className="btn btn-outline-secondary" href={rendered.url} download="computator-chart.png">Download PNG</a>}
      </div>
      <label className="form-label" htmlFor="surface-grid-size">3D surface quality</label>
      <select className="form-select mb-3" id="surface-grid-size" value={gridSize}
        onChange={event => setGridSize(Number(event.target.value))}>
        <option value={33}>Fast (33 × 33)</option>
        <option value={65}>Balanced (65 × 65)</option>
        <option value={97}>Detailed (97 × 97)</option>
      </select>
      <h2 className="h6">Chart area values:</h2><ul className="list-group">
        <li className="list-group-item"><NumericInput label="X" subscriptLabel="MIN" value={xMin} onValueChanged={setXMin} /></li>
        <li className="list-group-item"><NumericInput label="X" subscriptLabel="MAX" value={xMax} onValueChanged={setXMax} /></li>
        <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MIN" value={yMin} onValueChanged={setYMin} /></li>
        <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MAX" value={yMax} onValueChanged={setYMax} /></li>
      </ul></div></div></>;
}
