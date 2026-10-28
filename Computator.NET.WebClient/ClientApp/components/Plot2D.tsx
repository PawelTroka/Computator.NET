import { useEffect, useId, useMemo, useRef, useState, type KeyboardEvent, type PointerEvent } from 'react';
import { panBounds, zoomBounds, type Bounds, type ComplexData, type PlotLayer } from './Chart2DData';
import './Plot2D.css';

type Contours = 'none' | 'logarithmic' | 'linear';
type Colors = 'darker' | 'lighter';
type Display = { contours: Contours; colors: Colors; step: number; axes: boolean };
type Size = { width: number; height: number };
type Position = { x: number; y: number };
type PlotArea = { left: number; top: number; width: number; height: number };
type Props = {
  layers: PlotLayer[]; bounds: Bounds; pending: boolean;
  onBoundsChange: (bounds: Bounds) => void; onReset: () => void;
};
type Gesture = { bounds: Bounds; points: Map<number, Position> };

function plotArea(size: Size, layers: PlotLayer[]): PlotArea {
  return { left: 70, top: 22, width: Math.max(80, size.width - 92),
    height: Math.max(100, size.height - 80 - Math.max(1, layers.filter(layer => layer.visible).length) * 20) };
}

function format(value: number): string {
  if (value === 0) return '0';
  const absolute = Math.abs(value);
  return absolute >= 1e5 || absolute < 1e-3 ? value.toExponential(3) : Number(value.toPrecision(5)).toString();
}

function ticks(minimum: number, maximum: number): number[] {
  const raw = (maximum - minimum) / 5;
  if (!Number.isFinite(raw) || raw <= 0) return [minimum, maximum];
  const magnitude = 10 ** Math.floor(Math.log10(raw));
  const normal = raw / magnitude;
  const step = (normal <= 1 ? 1 : normal <= 2 ? 2 : normal <= 5 ? 5 : 10) * magnitude;
  if (!Number.isFinite(step) || step <= 0) return [minimum, maximum];
  const first = Math.ceil(minimum / step) * step;
  const output: number[] = [];
  for (let index = 0; index < 12; index++) {
    const value = first + index * step;
    if (!Number.isFinite(value) || value > maximum) break;
    if (value >= minimum && (index === 0 || value > output[output.length - 1])) output.push(value === 0 ? 0 : value);
  }
  return output.length ? output : [minimum, maximum];
}

// HSV phase coloring follows the desktop's hue convention: positive real is red,
// negative real cyan. Linear/logarithmic bands use direct arithmetic, never the
// desktop's potentially unbounded magnitude loops. The lighter direction uses
// the documented black-zero/white-infinity meaning of GreaterIsLighter.
function complexColor(real: number, imaginary: number, display: Display): [number, number, number] {
  const hue = ((Math.atan2(imaginary, real) / (2 * Math.PI)) + 1) % 1;
  const magnitude = Math.hypot(real, imaginary);
  let saturation: number, value: number;
  if (display.contours === 'none' || !Number.isFinite(magnitude)) {
    const lightness = 2 * Math.atan(magnitude) / Math.PI;
    value = display.colors === 'darker' ? 1 - lightness : lightness;
    saturation = Math.min(1, 2 * Math.min(value, 1 - value));
  } else {
    let fraction = 0;
    if (display.contours === 'linear') {
      const band = magnitude / display.step;
      fraction = Number.isFinite(band) ? band - Math.floor(band) : 0;
    } else if (magnitude > 1) {
      const logarithm = Math.log(magnitude) / Math.log(display.step);
      const band = Math.floor(logarithm);
      // Division before subtraction avoids powers that overflow near Double.MaxValue.
      const base = Math.exp(band * Math.log(display.step));
      fraction = Number.isFinite(base) && base > 0 ? (magnitude / base - 1) / (display.step - 1) : 0;
    } else fraction = magnitude;
    fraction = Math.max(0, Math.min(1, fraction));
    const triangular = fraction < 0.5 ? 2 * fraction : 2 * (1 - fraction);
    saturation = 0.4 + 0.6 * (1 - (1 - triangular) ** 3);
    value = 0.6 + 0.4 * (1 - triangular ** 3);
    // Desktop contour modes encode magnitude bands independently of assignment.
    // Here the selected direction remains useful with contours as well.
    if (display.colors === 'lighter') value = 0.6 + 0.4 * triangular ** 3;
  }
  const sector = Math.floor(hue * 6), f = hue * 6 - sector;
  const p = value * (1 - saturation), q = value * (1 - saturation * f), t = value * (1 - saturation * (1 - f));
  const rgb = [[value, t, p], [q, value, p], [p, value, t], [p, q, value], [t, p, value], [value, p, q]][sector % 6];
  return rgb.map(channel => Math.round(255 * channel)) as [number, number, number];
}

function colorField(data: ComplexData, display: Display): HTMLCanvasElement {
  const canvas = document.createElement('canvas');
  canvas.width = data.x.length; canvas.height = data.y.length;
  const context = canvas.getContext('2d');
  if (!context) return canvas;
  const image = context.createImageData(canvas.width, canvas.height);
  for (let row = 0; row < canvas.height; row++) {
    for (let column = 0; column < canvas.width; column++) {
      const real = data.real[row][column], imaginary = data.imaginary[row][column];
      const offset = ((canvas.height - row - 1) * canvas.width + column) * 4;
      const color = real === null || imaginary === null ? [255, 255, 255] : complexColor(real, imaginary, display);
      image.data[offset] = color[0]; image.data[offset + 1] = color[1]; image.data[offset + 2] = color[2]; image.data[offset + 3] = 255;
    }
  }
  context.putImageData(image, 0, 0);
  return canvas;
}

function fraction(value: number, minimum: number, maximum: number): number {
  // Limit only off-screen canvas coordinates; neither source samples nor visible
  // values are clamped. This avoids feeding Infinity to canvas for extreme curves.
  return Math.max(-1e6, Math.min(1e6, (value - minimum) / (maximum - minimum)));
}

function fitText(context: CanvasRenderingContext2D, text: string, width: number): string {
  if (context.measureText(text).width <= width) return text;
  // Definitions can be long. Find the visible prefix in logarithmic work rather
  // than repeatedly measuring every progressively shorter full expression.
  let minimum = 0, maximum = text.length;
  while (minimum < maximum) {
    const middle = Math.ceil((minimum + maximum) / 2);
    if (context.measureText(text.slice(0, middle) + '…').width <= width) minimum = middle;
    else maximum = middle - 1;
  }
  return text.slice(0, minimum) + '…';
}

function paint(canvas: HTMLCanvasElement, size: Size, bounds: Bounds, layers: PlotLayer[],
  fields: Map<string, HTMLCanvasElement>, display: Display) {
  const ratio = Math.min(3, window.devicePixelRatio || 1);
  canvas.width = Math.round(size.width * ratio); canvas.height = Math.round(size.height * ratio);
  const context = canvas.getContext('2d');
  if (!context) return;
  context.scale(ratio, ratio);
  const area = plotArea(size, layers);
  const px = (x: number) => area.left + fraction(x, bounds.xMin, bounds.xMax) * area.width;
  const py = (y: number) => area.top + (1 - fraction(y, bounds.yMin, bounds.yMax)) * area.height;
  context.fillStyle = '#ffffff'; context.fillRect(0, 0, size.width, size.height);
  context.save(); context.beginPath(); context.rect(area.left, area.top, area.width, area.height); context.clip();
  for (const layer of layers.filter(item => item.visible && item.data.kind === 'complex')) {
    const field = fields.get(layer.id);
    if (!field) continue;
    // Samples are anchored to coordinates, including during preview while a new
    // viewport is being sampled. Navigation never changes the meaning of pixels.
    const left = px(layer.bounds.xMin), top = py(layer.bounds.yMax);
    const right = px(layer.bounds.xMax), bottom = py(layer.bounds.yMin);
    const cellWidth = (right - left) / (field.width - 1), cellHeight = (bottom - top) / (field.height - 1);
    context.imageSmoothingEnabled = false;
    // Grid values live at nodes, including both endpoints. Center each raster
    // pixel on its sampled coordinate; clipping leaves half-cells at the edges.
    // This also keeps the nearest-sample readout aligned with the visible field.
    context.save(); context.beginPath(); context.rect(left, top, right - left, bottom - top); context.clip();
    context.drawImage(field, left - cellWidth / 2, top - cellHeight / 2,
      right - left + cellWidth, bottom - top + cellHeight);
    context.restore();
  }
  if (display.axes) {
    context.strokeStyle = '#b0b6bf70'; context.lineWidth = 1;
    for (const x of ticks(bounds.xMin, bounds.xMax)) { context.beginPath(); context.moveTo(px(x), area.top); context.lineTo(px(x), area.top + area.height); context.stroke(); }
    for (const y of ticks(bounds.yMin, bounds.yMax)) { context.beginPath(); context.moveTo(area.left, py(y)); context.lineTo(area.left + area.width, py(y)); context.stroke(); }
    context.strokeStyle = '#364356'; context.lineWidth = 1.2;
    if (bounds.xMin <= 0 && bounds.xMax >= 0) { context.beginPath(); context.moveTo(px(0), area.top); context.lineTo(px(0), area.top + area.height); context.stroke(); }
    if (bounds.yMin <= 0 && bounds.yMax >= 0) { context.beginPath(); context.moveTo(area.left, py(0)); context.lineTo(area.left + area.width, py(0)); context.stroke(); }
  }
  for (const layer of layers.filter(item => item.visible)) {
    if (layer.data.kind !== 'curve') continue;
    context.strokeStyle = layer.color; context.lineWidth = 2; context.beginPath();
    let connected = false;
    for (let index = 0; index < layer.data.x.length; index++) {
      const y = layer.data.y[index];
      if (y === null) { connected = false; continue; }
      const xPixel = px(layer.data.x[index]), yPixel = py(y);
      if (!Number.isFinite(xPixel) || !Number.isFinite(yPixel)) { connected = false; continue; }
      if (connected) context.lineTo(xPixel, yPixel); else context.moveTo(xPixel, yPixel);
      connected = true;
    }
    context.stroke();
  }
  context.restore();
  context.strokeStyle = '#6a7584'; context.lineWidth = 1; context.strokeRect(area.left, area.top, area.width, area.height);
  context.fillStyle = '#273446'; context.font = '12px system-ui, sans-serif';
  const complex = layers.some(layer => layer.data.kind === 'complex');
  if (display.axes) {
    context.textAlign = 'center'; context.textBaseline = 'top';
    for (const x of ticks(bounds.xMin, bounds.xMax)) context.fillText(format(x), px(x), area.top + area.height + 8, Math.max(45, area.width / 5));
    context.textAlign = 'right'; context.textBaseline = 'middle';
    for (const y of ticks(bounds.yMin, bounds.yMax)) context.fillText(format(y), area.left - 8, py(y), 58);
    context.textAlign = 'center'; context.textBaseline = 'top';
    context.fillText(complex ? 'Re(z)' : 'x', area.left + area.width / 2, area.top + area.height + 29);
    context.save(); context.translate(12, area.top + area.height / 2); context.rotate(-Math.PI / 2);
    context.textBaseline = 'top'; context.fillText(complex ? 'Im(z)' : 'y', 0, 0); context.restore();
  }
  context.textAlign = 'left'; context.textBaseline = 'middle'; context.font = '12px system-ui, sans-serif';
  let legendY = area.top + area.height + 61;
  for (const layer of layers.filter(item => item.visible)) {
    context.fillStyle = layer.data.kind === 'complex' ? '#7242a8' : layer.color;
    context.fillRect(area.left, legendY - 3, 20, 3);
    context.fillStyle = '#273446';
    context.fillText(fitText(context, layer.expression, area.width - 30), area.left + 28, legendY - 1);
    legendY += 20;
  }
}

export function Plot2D({ layers, bounds, pending, onBoundsChange, onReset }: Props) {
  const canvas = useRef<HTMLCanvasElement>(null), host = useRef<HTMLDivElement>(null);
  const id = useId();
  const [size, setSize] = useState<Size>({ width: 640, height: 460 });
  const [display, setDisplay] = useState<Display>({ contours: 'logarithmic', colors: 'darker', step: Math.E, axes: true });
  const [stepText, setStepText] = useState(String(Math.E));
  const [readout, setReadout] = useState(''), [exportError, setExportError] = useState('');
  const [exporting, setExporting] = useState(false), [dragging, setDragging] = useState(false);
  const mounted = useRef(true), exportingNow = useRef(false);
  const points = useRef(new Map<number, Position>()), gesture = useRef<Gesture | null>(null);
  const latest = useRef({ bounds, onBoundsChange, onReset, size, layers });
  latest.current = { bounds, onBoundsChange, onReset, size, layers };
  const complex = layers.some(layer => layer.data.kind === 'complex');
  const step = Number(stepText);
  const validStep = stepText.trim() !== '' && Number.isFinite(step) && step > (display.contours === 'logarithmic' ? 1 : 0);
  const fields = useMemo(() => new Map(layers.filter(layer => layer.data.kind === 'complex')
    .map(layer => [layer.id, colorField(layer.data as ComplexData, display)] as const)), [layers, display.contours, display.colors, display.step]);

  useEffect(() => {
    mounted.current = true;
    return () => { mounted.current = false; };
  }, []);

  useEffect(() => { setReadout(''); }, [bounds.xMin, bounds.xMax, bounds.yMin, bounds.yMax, layers]);

  useEffect(() => {
    const element = host.current;
    if (!element) return;
    const resize = () => {
      const width = Math.max(220, Math.floor(element.clientWidth));
      const height = Math.max(360, Math.min(580, Math.round(width * 0.66))) + Math.max(0, layers.length - 3) * 20;
      setSize(current => current.width === width && current.height === height ? current : { width, height });
    };
    resize(); const observer = new ResizeObserver(resize); observer.observe(element);
    return () => observer.disconnect();
  }, [layers.length]);

  useEffect(() => {
    if (canvas.current) paint(canvas.current, size, bounds, layers, fields, display);
  }, [size, bounds, layers, fields, display]);

  function localPosition(clientX: number, clientY: number): Position {
    const rectangle = canvas.current!.getBoundingClientRect();
    const current = latest.current.size;
    return { x: (clientX - rectangle.left) * current.width / rectangle.width,
      y: (clientY - rectangle.top) * current.height / rectangle.height };
  }
  function inside(point: Position): boolean {
    const area = plotArea(latest.current.size, latest.current.layers);
    return point.x >= area.left && point.x <= area.left + area.width && point.y >= area.top && point.y <= area.top + area.height;
  }
  useEffect(() => {
    const element = canvas.current;
    if (!element) return;
    const wheel = (event: WheelEvent) => {
      const point = localPosition(event.clientX, event.clientY);
      if (!inside(point) || !event.deltaY) return;
      event.preventDefault();
      const current = latest.current, area = plotArea(current.size, current.layers);
      const delta = event.deltaY * (event.deltaMode === 1 ? 16 : event.deltaMode === 2 ? 120 : 1);
      const factor = Math.exp(Math.max(-1, Math.min(1, delta * 0.002)));
      current.onBoundsChange(zoomBounds(current.bounds, factor,
        (point.x - area.left) / area.width, 1 - (point.y - area.top) / area.height));
    };
    element.addEventListener('wheel', wheel, { passive: false });
    return () => element.removeEventListener('wheel', wheel);
  }, []);

  function beginGesture() {
    gesture.current = { bounds: { ...latest.current.bounds }, points: new Map(points.current) };
  }
  function pointerDown(event: PointerEvent<HTMLCanvasElement>) {
    if (event.button !== 0 && event.button !== 1) return;
    const point = localPosition(event.clientX, event.clientY);
    if (!inside(point)) return;
    event.preventDefault(); event.currentTarget.focus(); event.currentTarget.setPointerCapture(event.pointerId);
    points.current.set(event.pointerId, point); beginGesture(); setDragging(true); setReadout('');
  }
  function pointerMove(event: PointerEvent<HTMLCanvasElement>) {
    const point = localPosition(event.clientX, event.clientY);
    if (points.current.has(event.pointerId)) {
      points.current.set(event.pointerId, point);
      const initial = gesture.current;
      if (!initial) return;
      const entries = [...initial.points.keys()].filter(key => points.current.has(key));
      const area = plotArea(latest.current.size, latest.current.layers);
      if (entries.length >= 2) {
        const a = initial.points.get(entries[0])!, b = initial.points.get(entries[1])!;
        const c = points.current.get(entries[0])!, d = points.current.get(entries[1])!;
        const distance = Math.hypot(a.x - b.x, a.y - b.y), currentDistance = Math.hypot(c.x - d.x, c.y - d.y);
        if (distance < 2 || currentDistance < 2) return;
        const before = { x: (a.x + b.x) / 2, y: (a.y + b.y) / 2 }, after = { x: (c.x + d.x) / 2, y: (c.y + d.y) / 2 };
        const next = zoomBounds(initial.bounds, distance / currentDistance,
          (before.x - area.left) / area.width, 1 - (before.y - area.top) / area.height);
        latest.current.onBoundsChange(panBounds(next, (before.x - after.x) / area.width, (after.y - before.y) / area.height));
      } else {
        const start = initial.points.get(event.pointerId);
        if (start) latest.current.onBoundsChange(panBounds(initial.bounds, (start.x - point.x) / area.width, (point.y - start.y) / area.height));
      }
      return;
    }
    if (!inside(point)) { setReadout(''); return; }
    const current = latest.current, area = plotArea(current.size, current.layers);
    const x = current.bounds.xMin + (point.x - area.left) / area.width * (current.bounds.xMax - current.bounds.xMin);
    const y = current.bounds.yMax - (point.y - area.top) / area.height * (current.bounds.yMax - current.bounds.yMin);
    let text = complex ? `Re(z): ${format(x)}   Im(z): ${format(y)}` : `x: ${format(x)}   y: ${format(y)}`;
    const layer = current.layers.find(item => item.visible && item.data.kind === 'complex');
    if (layer?.data.kind === 'complex' && x >= layer.bounds.xMin && x <= layer.bounds.xMax && y >= layer.bounds.yMin && y <= layer.bounds.yMax) {
      const column = Math.round((x - layer.bounds.xMin) / (layer.bounds.xMax - layer.bounds.xMin) * (layer.data.x.length - 1));
      const row = Math.round((y - layer.bounds.yMin) / (layer.bounds.yMax - layer.bounds.yMin) * (layer.data.y.length - 1));
      const real = layer.data.real[row][column], imaginary = layer.data.imaginary[row][column];
      text += real === null || imaginary === null ? '   nearest sample: undefined' : `   nearest f(z): ${format(real)} ${imaginary < 0 ? '−' : '+'} ${format(Math.abs(imaginary))}i`;
    }
    setReadout(text);
  }
  function pointerEnd(event: PointerEvent<HTMLCanvasElement>) {
    if (!points.current.has(event.pointerId)) return;
    points.current.delete(event.pointerId);
    if (event.currentTarget.hasPointerCapture(event.pointerId)) event.currentTarget.releasePointerCapture(event.pointerId);
    if (points.current.size) beginGesture(); else { gesture.current = null; setDragging(false); }
  }
  function keyDown(event: KeyboardEvent<HTMLCanvasElement>) {
    const current = latest.current;
    const movement: Record<string, [number, number]> = { ArrowLeft: [-0.15, 0], ArrowRight: [0.15, 0], ArrowUp: [0, 0.15], ArrowDown: [0, -0.15] };
    if (movement[event.key]) { event.preventDefault(); current.onBoundsChange(panBounds(current.bounds, ...movement[event.key])); }
    else if (event.key === '+' || event.key === '=') { event.preventDefault(); current.onBoundsChange(zoomBounds(current.bounds, 0.8)); }
    else if (event.key === '-' || event.key === '_') { event.preventDefault(); current.onBoundsChange(zoomBounds(current.bounds, 1.25)); }
    else if (event.key === 'Home') { event.preventDefault(); current.onReset(); }
  }
  function changeContour(contours: Contours) {
    const nextStep = Number(stepText);
    if (!Number.isFinite(nextStep) || nextStep <= (contours === 'logarithmic' ? 1 : 0)) {
      setStepText(String(Math.E)); setDisplay(current => ({ ...current, contours, step: Math.E }));
    } else setDisplay(current => ({ ...current, contours, step: nextStep }));
  }
  function changeStep(text: string) {
    setStepText(text);
    const number = Number(text);
    if (text.trim() && Number.isFinite(number) && number > (display.contours === 'logarithmic' ? 1 : 0))
      setDisplay(current => ({ ...current, step: number }));
  }
  async function download() {
    const source = canvas.current;
    if (!source || pending || exportingNow.current) return;
    exportingNow.current = true; setExporting(true); setExportError('');
    try {
      // toBlob snapshots this canvas, so a later viewport update cannot silently
      // change the image the user requested. Route departure cancels publication.
      const blob = await new Promise<Blob>((resolve, reject) => source.toBlob(value => value ? resolve(value) : reject(new Error('PNG export failed.')), 'image/png'));
      if (!mounted.current) return;
      const url = URL.createObjectURL(blob), link = document.createElement('a');
      link.href = url; link.download = 'computator-chart.png'; document.body.appendChild(link); link.click(); link.remove();
      // A later task allows Firefox to begin its download before the URL retires.
      setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch (error) { if (mounted.current) setExportError(error instanceof Error ? error.message : 'PNG export failed.'); }
    finally { exportingNow.current = false; if (mounted.current) setExporting(false); }
  }

  return <figure className="plot2d">
    <div ref={host}>
      <canvas ref={canvas} className="plot2d-canvas" style={{ height: size.height }} tabIndex={0}
        role="img" aria-label={complex ? 'Interactive complex chart' : 'Interactive real chart'}
        aria-describedby={`${id}-instructions`} data-testid="chart-2d" data-bounds={JSON.stringify(bounds)}
        data-series-count={layers.filter(layer => layer.visible).length} data-dragging={dragging}
        onPointerDown={pointerDown} onPointerMove={pointerMove} onPointerUp={pointerEnd} onPointerCancel={pointerEnd}
        onLostPointerCapture={pointerEnd} onPointerLeave={() => { if (!points.current.size) setReadout(''); }} onKeyDown={keyDown}>
        {complex ? 'Complex domain coloring' : 'Real function curves'}. Use the chart controls to change the visible coordinate range.
      </canvas>
    </div>
    <figcaption className="plot2d-hint" id={`${id}-instructions`}>Drag to pan; use the mouse wheel or pinch to zoom. Keyboard: arrows to pan, +/− to zoom, Home to reset.</figcaption>
    <div className="plot2d-readout">{readout || (pending ? 'Updating samples for this view…' : 'Move over the chart to inspect coordinates.')}</div>
    <div className="plot2d-toolbar" role="group" aria-label="2D chart controls">
      <button type="button" className="btn btn-outline-secondary" onClick={() => onBoundsChange(zoomBounds(bounds, 0.8))}>Zoom in</button>
      <button type="button" className="btn btn-outline-secondary" onClick={() => onBoundsChange(zoomBounds(bounds, 1.25))}>Zoom out</button>
      <button type="button" className="btn btn-outline-secondary" onClick={() => onBoundsChange(panBounds(bounds, -0.2, 0))}>Pan left</button>
      <button type="button" className="btn btn-outline-secondary" onClick={() => onBoundsChange(panBounds(bounds, 0.2, 0))}>Pan right</button>
      <button type="button" className="btn btn-outline-secondary" onClick={() => onBoundsChange(panBounds(bounds, 0, 0.2))}>Pan up</button>
      <button type="button" className="btn btn-outline-secondary" onClick={() => onBoundsChange(panBounds(bounds, 0, -0.2))}>Pan down</button>
      <button type="button" className="btn btn-outline-secondary" onClick={onReset}>Reset view</button>
      <button type="button" className="btn btn-outline-secondary" disabled={pending || exporting} onClick={() => { void download(); }}>{exporting ? 'Exporting…' : 'Download PNG'}</button>
    </div>
    <div className="plot2d-options">
      {complex && <>
        <div><label htmlFor={`${id}-contours`}>Complex contours</label><select id={`${id}-contours`} className="form-select" value={display.contours} onChange={event => changeContour(event.target.value as Contours)}>
          <option value="none">None</option><option value="logarithmic">Logarithmic</option><option value="linear">Linear</option>
        </select></div>
        <div><label htmlFor={`${id}-colors`}>Color assignment</label><select id={`${id}-colors`} className="form-select" value={display.colors} onChange={event => setDisplay(current => ({ ...current, colors: event.target.value as Colors }))}>
          <option value="darker">Greater is darker</option><option value="lighter">Greater is lighter</option>
        </select></div>
        <div><label htmlFor={`${id}-spacing`}>Contour spacing</label><input id={`${id}-spacing`} className="form-control" type="number" step="any" value={stepText}
          disabled={display.contours === 'none'} aria-invalid={!validStep} aria-describedby={!validStep ? `${id}-spacing-error` : undefined} onChange={event => changeStep(event.target.value)} /></div>
      </>}
      <div className="form-check"><input id={`${id}-axes`} className="form-check-input" type="checkbox" checked={display.axes} onChange={event => setDisplay(current => ({ ...current, axes: event.target.checked }))} />
        <label className="form-check-label" htmlFor={`${id}-axes`}>Show axes</label></div>
    </div>
    {complex && !validStep && <p className="text-danger small" id={`${id}-spacing-error`}>Contour spacing must be finite and greater than {display.contours === 'logarithmic' ? '1' : '0'}. The last valid spacing is still displayed.</p>}
    {exportError && <p className="text-danger" role="alert">{exportError}</p>}
  </figure>;
}
