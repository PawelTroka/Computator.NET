import { useCallback, useEffect, useRef, useState } from 'react';
import { WEB_API_BASE_URL } from '../config';
import { useCustomFunctions } from '../helpers/CustomFunctionsContext';
import { useChartDraft } from '../helpers/ToolDraftContext';
import { Expression } from './Expression';
import { NumericInput } from './NumericInput';
import { Plot2D } from './Plot2D';
import { isChart2DData, validBounds, type Bounds, type PlotLayer } from './Chart2DData';

type Definition = { id: string; expression: string; customFunctions: string; color: string; visible: boolean };
type SampledLayer = PlotLayer & { customFunctions: string };
type Plot = Definition & Bounds;
type RenderedPlot = { kind: 'image'; url: string; expression: string }
  | { kind: 'data'; layers: SampledLayer[]; bounds: Bounds; home: Bounds };
type RenderRequest = { definitions: Definition[]; bounds: Bounds; home: Bounds; operation: 'draw' | 'add' | 'navigate' | 'resize' };
const colors = ['#1769c2', '#c0392b', '#16814a', '#8e44ad', '#b36b00', '#087f8c', '#c23679', '#555e68'];
const maximumLayers = 8;

export function Chart() {
  const { expression, setExpression, xMin, setXMin, xMax, setXMax, yMin, setYMin, yMax, setYMax } = useChartDraft();
  const [rendered, setRendered] = useState<RenderedPlot>();
  const [viewport, setViewport] = useState<Bounds>({ xMin, xMax, yMin, yMax });
  const [error, setError] = useState(''), [pending, setPending] = useState(false), [status, setStatus] = useState('');
  const container = useRef<HTMLDivElement>(null);
  const active = useRef<AbortController | null>(null), imageUrl = useRef<string | null>(null);
  const shown = useRef<RenderedPlot | undefined>(undefined);
  const requestedImage = useRef<Plot | null>(null);
  const navigationTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  const nextId = useRef(0);
  const functions = useCustomFunctions();

  const publish = useCallback((plot: RenderedPlot | undefined) => {
    shown.current = plot; setRendered(plot);
  }, []);
  const invalidate = useCallback(() => {
    if (navigationTimer.current) clearTimeout(navigationTimer.current);
    navigationTimer.current = null;
    // Invalidate before abort: a late response/finally cannot publish into a new run.
    const controller = active.current; active.current = null; controller?.abort();
  }, []);
  const releaseImage = useCallback(() => {
    if (imageUrl.current) URL.revokeObjectURL(imageUrl.current);
    imageUrl.current = null;
  }, []);
  const updateDraftBounds = useCallback((bounds: Bounds) => {
    setXMin(bounds.xMin); setXMax(bounds.xMax); setYMin(bounds.yMin); setYMax(bounds.yMax);
  }, [setXMin, setXMax, setYMin, setYMax]);

  const renderPlot = useCallback(async (request: RenderRequest) => {
    if (!container.current) return;
    invalidate();
    const controller = new AbortController(); active.current = controller;
    requestedImage.current = null;
    setError(''); setPending(true);
    setStatus(request.operation === 'navigate' ? 'Sampling the new chart area…' : 'Rendering chart…');
    try {
      const width = Math.min(2048, Math.max(200, Math.round(container.current.clientWidth)));
      const height = Math.min(2048, Math.max(200, Math.round(window.innerHeight - container.current.getBoundingClientRect().top)));
      const layers: SampledLayer[] = [];
      // A bounded, sequential batch preserves each submitted definition snapshot,
      // does not exhaust the shared worker queue, and publishes all layers atomically.
      for (const definition of request.definitions) {
        const plot = { ...definition, ...request.bounds };
        const response = await fetch(`${WEB_API_BASE_URL}/v2/chart`, {
          method: 'POST', headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ expression: plot.expression, customFunctions: plot.customFunctions,
            ...request.bounds, width, height, interactive2D: true, curveSamples: 1025, complexGridSize: 257 }),
          signal: controller.signal
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
          const data: unknown = await response.json();
          if (active.current !== controller || controller.signal.aborted) return;
          if (!isChart2DData(data) || data.x[0] !== request.bounds.xMin || data.x.at(-1) !== request.bounds.xMax ||
              (data.kind === 'complex' && (data.y[0] !== request.bounds.yMin || data.y.at(-1) !== request.bounds.yMax)))
            throw new Error('Invalid interactive chart response.');
          if (request.definitions.length > 1 && data.kind !== 'curve')
            throw new Error('Overlays support explicit real functions. Use Draw chart! to replace the chart with a complex function.');
          layers.push({ ...definition, data, bounds: request.bounds });
        } else {
          if (!contentType.startsWith('image/png')) throw new Error('Invalid chart response.');
          if (request.definitions.length !== 1 || request.operation === 'add')
            throw new Error('Overlays support explicit real functions. Use Draw chart! for this expression.');
          const image = await response.blob();
          if (active.current !== controller || controller.signal.aborted) return;
          const url = URL.createObjectURL(image);
          releaseImage(); imageUrl.current = url;
          requestedImage.current = plot;
          publish({ kind: 'image', url, expression: definition.expression });
          setStatus('Chart ready.');
          return;
        }
      }
      if (active.current !== controller || controller.signal.aborted) return;
      releaseImage();
      publish({ kind: 'data', layers, bounds: request.bounds, home: request.home });
      setViewport(request.bounds);
      if (request.operation === 'navigate') updateDraftBounds(request.bounds);
      setStatus('Chart ready.');
    } catch (reason) {
      if (active.current === controller && !controller.signal.aborted) {
        setError(reason instanceof Error ? reason.message : 'The chart could not be generated.');
        setStatus('Chart generation failed.');
        requestedImage.current = null;
        // Failed or cancelled refinement must not leave old samples labelled as a new viewport.
        if (shown.current?.kind === 'data') setViewport(shown.current.bounds);
      }
    } finally {
      if (active.current === controller) { active.current = null; setPending(false); }
    }
  }, [invalidate, publish, releaseImage, updateDraftBounds]);

  function draw(add = false) {
    if (!expression.trim()) return;
    const current = shown.current;
    const bounds = add && current?.kind === 'data' ? current.bounds : { xMin, xMax, yMin, yMax };
    if (!validBounds(bounds)) {
      setError('Each minimum must be smaller than its maximum, and bounds and their spans must be finite.'); return;
    }
    const oldLayers = add && current?.kind === 'data' ? current.layers : [];
    if (add && (oldLayers.length === 0 || oldLayers.some(layer => layer.data.kind !== 'curve') || oldLayers.length >= maximumLayers)) return;
    const definition: Definition = { id: String(++nextId.current), expression, customFunctions: functions.code,
      color: colors[(nextId.current - 1) % colors.length], visible: true };
    // Adding a curve uses the current chart area, not unsubmitted range edits.
    const area = add && current?.kind === 'data' ? current.bounds : bounds;
    const home = add && current?.kind === 'data' ? current.home : area;
    void renderPlot({ definitions: [...oldLayers, definition], bounds: area, home, operation: add ? 'add' : 'draw' });
  }
  function cancel() {
    invalidate(); requestedImage.current = null;
    if (shown.current?.kind === 'data') setViewport(shown.current.bounds);
    setPending(false); setError(''); setStatus('Chart rendering cancelled.');
  }
  function clear() {
    invalidate(); requestedImage.current = null; releaseImage(); publish(undefined);
    setPending(false); setError(''); setStatus('Chart cleared.');
  }
  function navigate(bounds: Bounds, immediately = false) {
    const current = shown.current;
    if (current?.kind !== 'data' || !validBounds(bounds)) return;
    invalidate(); requestedImage.current = null;
    setViewport(bounds); setPending(true); setError(''); setStatus('Sampling the new chart area…');
    const request: RenderRequest = { definitions: current.layers, bounds, home: current.home, operation: 'navigate' };
    if (immediately) void renderPlot(request);
    else navigationTimer.current = setTimeout(() => { navigationTimer.current = null; void renderPlot(request); }, 180);
  }
  function changeLayer(id: string, visible?: boolean) {
    const current = shown.current;
    if (current?.kind !== 'data' || pending) return;
    const layers = visible === undefined ? current.layers.filter(layer => layer.id !== id)
      : current.layers.map(layer => layer.id === id ? { ...layer, visible } : layer);
    if (layers.length === 0) clear();
    else publish({ ...current, layers });
  }

  useEffect(() => {
    let timeout: ReturnType<typeof setTimeout>;
    const resize = () => {
      clearTimeout(timeout);
      timeout = setTimeout(() => {
        // Numerical plots resize from cached samples. Preserve legacy PNG fallback
        // and never execute unsubmitted editor text or cancelled/failed requests.
        const plot = requestedImage.current;
        if (plot) void renderPlot({ definitions: [plot], bounds: plot, home: plot, operation: 'resize' });
      }, 200);
    };
    window.addEventListener('resize', resize);
    return () => { clearTimeout(timeout); window.removeEventListener('resize', resize); };
  }, [renderPlot]);
  useEffect(() => () => {
    requestedImage.current = null; invalidate(); releaseImage();
  }, [invalidate, releaseImage]);

  const canOverlay = rendered?.kind === 'data' && rendered.layers.every(layer => layer.data.kind === 'curve');
  return <><h1>Chart</h1><p>Draw a real or complex function. Add real functions to compare them on the same axes.</p>
    <Expression expression={expression} onExpressionChange={setExpression} />
    {error && <p role="alert" className="text-danger">{error}</p>}
    <p role="status" aria-live="polite">{status}</p>
    <div className="row g-3"><div className="col-md-9" ref={container} aria-busy={pending}>
      {rendered?.kind === 'image' ? <figure><img className="chart-image" src={rendered.url} alt={rendered.expression} />
        <figcaption>Rendered expression: <code>{rendered.expression}</code></figcaption>
        <p className="text-muted small">This expression uses the server image renderer. Adjust the chart area and draw again to change its view.</p></figure>
        : rendered?.kind === 'data' ? <Plot2D layers={rendered.layers} bounds={viewport} pending={pending}
          onBoundsChange={navigate} onReset={() => navigate(rendered.home, true)} />
        : <p>Enter an expression and choose Draw chart.</p>}
    </div><div className="col-md-3">
      <div className="d-flex flex-wrap gap-2 mb-3">
        <button type="button" className="btn btn-primary btn-lg" disabled={!expression.trim() || pending} onClick={() => draw()}>Draw chart!</button>
        {canOverlay && rendered?.kind === 'data' && <button type="button" className="btn btn-outline-primary" disabled={!expression.trim() || pending || rendered.layers.length >= maximumLayers}
          onClick={() => draw(true)}>Add function</button>}
        {pending && <button type="button" className="btn btn-outline-danger" onClick={cancel}>Cancel chart</button>}
        {rendered && <button type="button" className="btn btn-outline-secondary" onClick={clear}>Clear chart</button>}
        {rendered?.kind === 'image' && <a className="btn btn-outline-secondary" href={rendered.url} download="computator-chart.png">Download PNG</a>}
      </div>
      {rendered?.kind === 'data' && <section className="mb-3" aria-label="Chart functions">
        <h2 className="h6">Functions ({rendered.layers.length}/{rendered.layers[0].data.kind === 'curve' ? maximumLayers : 1})</h2>
        <ul className="list-group">{rendered.layers.map(layer => <li className="list-group-item" key={layer.id}>
          <label className="d-flex align-items-center gap-2"><input type="checkbox" className="form-check-input" checked={layer.visible}
            disabled={pending} aria-label={`Show ${layer.expression}`} onChange={event => changeLayer(layer.id, event.target.checked)} />
            <span aria-hidden="true" style={{ background: layer.color, width: 16, height: 4, flexShrink: 0 }} />
            <code style={{ overflowWrap: 'anywhere' }}>{layer.expression}</code></label>
          <button type="button" className="btn btn-sm btn-outline-secondary mt-2" aria-label={`Remove ${layer.expression}`}
            disabled={pending} onClick={() => changeLayer(layer.id)}>Remove</button>
        </li>)}</ul>
        {canOverlay && <p className="small text-muted mt-2">Up to eight real functions. Each keeps the custom definitions submitted with it.</p>}
      </section>}
      <h2 className="h6">Chart area values:</h2><ul className="list-group">
        <li className="list-group-item"><NumericInput label="X" subscriptLabel="MIN" value={xMin} onValueChanged={setXMin} /></li>
        <li className="list-group-item"><NumericInput label="X" subscriptLabel="MAX" value={xMax} onValueChanged={setXMax} /></li>
        <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MIN" value={yMin} onValueChanged={setYMin} /></li>
        <li className="list-group-item"><NumericInput label="Y" subscriptLabel="MAX" value={yMax} onValueChanged={setYMax} /></li>
      </ul>
      {rendered?.kind === 'data' && <button type="button" className="btn btn-outline-secondary mt-2" disabled={pending || !validBounds({ xMin, xMax, yMin, yMax })}
        onClick={() => navigate({ xMin, xMax, yMin, yMax }, true)}>Apply bounds</button>}
    </div></div></>;
}
