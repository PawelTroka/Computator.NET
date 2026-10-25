import { useEffect, useRef, useState } from 'react';

// The sample grid is produced by the same server-side expression evaluator used
// for numerical calculations. Plotly never parses or evaluates user definitions.
export type SurfaceGrid = {
  kind: 'surface';
  x: number[];
  y: number[];
  z: (number | null)[][];
  finiteSamples: number;
  missingSamples: number;
};

export function isSurfaceGrid(value: unknown): value is SurfaceGrid {
  if (!value || typeof value !== 'object') return false;
  const grid = value as Record<string, unknown>;
  if (grid.kind !== 'surface' || !Array.isArray(grid.x) || !Array.isArray(grid.y) ||
      !Array.isArray(grid.z) || grid.x.length < 9 || grid.x.length > 129 ||
      grid.y.length !== grid.x.length || grid.z.length !== grid.y.length ||
      typeof grid.finiteSamples !== 'number' || typeof grid.missingSamples !== 'number') return false;
  if (!grid.x.every((x: unknown) => typeof x === 'number' && Number.isFinite(x)) ||
      !grid.y.every((y: unknown) => typeof y === 'number' && Number.isFinite(y))) return false;
  let finite = 0;
  for (const row of grid.z) {
    if (!Array.isArray(row) || row.length !== grid.x.length) return false;
    for (const height of row) {
      if (height === null) continue;
      if (typeof height !== 'number' || !Number.isFinite(height)) return false;
      finite++;
    }
  }
  return finite >= 3 && finite === grid.finiteSamples &&
    grid.missingSamples === grid.x.length * grid.y.length - finite;
}

export function SurfacePlot({ grid, expression }: { grid: SurfaceGrid; expression: string }) {
  const host = useRef<HTMLDivElement>(null);
  const plotly = useRef<(typeof import('plotly.js-gl3d-dist-min'))['default'] | null>(null);
  const [ready, setReady] = useState(false), [exporting, setExporting] = useState(false);
  const [error, setError] = useState('');
  useEffect(() => {
    const div = host.current;
    if (!div) return;
    let disposed = false;
    setReady(false); setError('');
    let resize: ResizeObserver | null = null;
    const start = async () => {
      try {
        // Load the offline, pinned WebGL surface bundle only for f(x,y).
        const { default: Plotly } = await import('plotly.js-gl3d-dist-min');
        if (disposed) return;
        plotly.current = Plotly;
        const axis = (name: string) => ({
          title: { text: name, font: { size: 15, color: '#eff0f3' } },
          tickfont: { color: '#eff0f3', size: 12 }, gridcolor: '#5b6570',
          zerolinecolor: '#c2cad5', backgroundcolor: '#252930', showbackground: true
        });
        await Plotly.newPlot(div, [{
          type: 'surface', x: grid.x, y: grid.y, z: grid.z,
          connectgaps: false, showscale: true,
          colorscale: 'Viridis',
          hovertemplate: 'x: %{x:.4g}<br>y: %{y:.4g}<br>f(x,y): %{z:.4g}<extra></extra>'
        }], {
          autosize: true, margin: { l: 0, r: 0, b: 0, t: 0 },
          paper_bgcolor: '#23262c', font: { color: '#eff0f3' },
          scene: {
            xaxis: axis('x'), yaxis: axis('y'), zaxis: axis('f(x,y)'),
            aspectmode: 'auto', dragmode: 'turntable',
            camera: { eye: { x: 1.55, y: 1.55, z: 1.2 } }
          }
        }, { responsive: true, scrollZoom: true, displaylogo: false,
          modeBarButtonsToRemove: ['sendDataToCloud'] });
        if (disposed) { Plotly.purge(div); return; }
        // A real Plotly camera update is observable for browser interaction tests.
        const graph = div as HTMLDivElement & { on?: (name: string, listener: (value: Record<string, unknown>) => void) => void };
        graph.on?.('plotly_relayout', event => {
          if (event['scene.camera']) div.dataset.camera = JSON.stringify(event['scene.camera']);
        });
        resize = new ResizeObserver(() => { void Plotly.Plots.resize(div); });
        resize.observe(div);
        setReady(true);
      } catch (reason) {
        if (!disposed) setError(reason instanceof Error ? reason.message : '3D rendering is unavailable.');
      }
    };
    void start();
    return () => {
      disposed = true;
      resize?.disconnect();
      if (plotly.current) plotly.current.purge(div);
      plotly.current = null;
    };
  }, [grid, expression]);

  async function download() {
    const div = host.current;
    if (!div || !plotly.current || !ready) return;
    setExporting(true); setError('');
    try {
      // Plotly composites WebGL + axis annotations into a real image.
      const png = await plotly.current.toImage(div, { format: 'png', width: 1200, height: 800, scale: 1 });
      const link = document.createElement('a');
      link.href = png; link.download = 'computator-chart.png';
      document.body.appendChild(link); link.click(); link.remove();
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : 'PNG export failed.');
    } finally { setExporting(false); }
  }

  return <figure>
    <div className="chart-surface" ref={host} role="img"
      aria-label={'Interactive 3D surface: ' + expression} data-testid="surface-chart" />
    <figcaption>Rendered expression: <code>{expression}</code></figcaption>
    <p className="text-muted small" role="status" aria-live="polite">
      {ready ? 'Interactive surface ready. Drag to rotate; use the mouse wheel or touch to zoom.'
        : error ? '3D rendering failed.' : 'Preparing interactive 3D view…'}
    </p>
    {error && <p className="text-danger" role="alert">{error}</p>}
    <button type="button" className="btn btn-outline-secondary" onClick={() => { void download(); }}
      disabled={!ready || exporting}>{exporting ? 'Exporting…' : 'Download PNG'}</button>
    <p className="text-muted small">Non-finite samples are omitted: {grid.missingSamples} of {grid.x.length * grid.y.length}.</p>
  </figure>;
}
