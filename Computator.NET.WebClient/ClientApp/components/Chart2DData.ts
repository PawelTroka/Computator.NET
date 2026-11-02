/** Numeric samples come from the existing server evaluator, never a JavaScript expression parser. */
export type Bounds = { xMin: number; xMax: number; yMin: number; yMax: number };
export type CurveData = {
  kind: 'curve'; x: number[]; y: (number | null)[]; finiteSamples: number; missingSamples: number;
};
export type ComplexData = {
  kind: 'complex'; x: number[]; y: number[];
  real: (number | null)[][]; imaginary: (number | null)[][];
  finiteSamples: number; missingSamples: number;
};
export type Chart2DData = CurveData | ComplexData;
export type PlotLayer = {
  id: string; expression: string; data: Chart2DData; color: string; visible: boolean; bounds: Bounds;
};

export function validBounds(bounds: Bounds): boolean {
  return [bounds.xMin, bounds.xMax, bounds.yMin, bounds.yMax,
    bounds.xMax - bounds.xMin, bounds.yMax - bounds.yMin].every(Number.isFinite) &&
    bounds.xMin < bounds.xMax && bounds.yMin < bounds.yMax;
}

/** Anchors are fractions measured from the mathematical minimum, including y. */
export function zoomBounds(bounds: Bounds, factor: number, anchorX = 0.5, anchorY = 0.5): Bounds {
  const sx = bounds.xMax - bounds.xMin, sy = bounds.yMax - bounds.yMin;
  const x = bounds.xMin + sx * anchorX, y = bounds.yMin + sy * anchorY;
  const next = { xMin: x - sx * factor * anchorX, xMax: x + sx * factor * (1 - anchorX),
    yMin: y - sy * factor * anchorY, yMax: y + sy * factor * (1 - anchorY) };
  // Collapsed intervals and overflow must not turn an ordinary gesture into an invalid request.
  return factor > 0 && Number.isFinite(factor) && validBounds(next) ? next : bounds;
}

/** Pan offsets are fractions of the current mathematical spans. */
export function panBounds(bounds: Bounds, dx: number, dy: number): Bounds {
  const x = (bounds.xMax - bounds.xMin) * dx, y = (bounds.yMax - bounds.yMin) * dy;
  const next = { xMin: bounds.xMin + x, xMax: bounds.xMax + x,
    yMin: bounds.yMin + y, yMax: bounds.yMax + y };
  return validBounds(next) ? next : bounds;
}

/** Retain enough digits to distinguish positions within a zoomed coordinate span. */
export function formatChartNumber(value: number, span?: number): string {
  if (value === 0) return '0';
  if (!Number.isFinite(value)) return String(value);
  const absolute = Math.abs(value);
  // Subtract logarithms instead of dividing value/span: valid finite intervals
  // can be extremely small beside a large offset. Seventeen digits round-trip a double.
  const precision = span !== undefined && Number.isFinite(span) && span > 0
    ? Math.max(5, Math.min(17, Math.floor(Math.log10(absolute)) - Math.floor(Math.log10(span)) + 3)) : 5;
  const rounded = Number(value.toPrecision(precision));
  if (!Number.isFinite(rounded)) return value.toString();
  return absolute < 1e-3 || (absolute >= 1e5 && precision <= 6)
    ? rounded.toExponential() : rounded.toString();
}

function isAxis(value: unknown, minimum: number, maximum: number): value is number[] {
  return Array.isArray(value) && value.length >= minimum && value.length <= maximum &&
    value.every((item: unknown, index: number) => typeof item === 'number' && Number.isFinite(item) &&
      (index === 0 || item > value[index - 1]));
}

export function isChart2DData(value: unknown, allowEmpty = false): value is Chart2DData {
  if (!value || typeof value !== 'object') return false;
  const data = value as Record<string, unknown>;
  if (!Number.isInteger(data.finiteSamples) || !Number.isInteger(data.missingSamples) ||
      (data.finiteSamples as number) < (allowEmpty ? 0 : 1) || (data.missingSamples as number) < 0) return false;
  let finite = 0, total = 0;
  if (data.kind === 'curve') {
    if (!isAxis(data.x, 33, 8193) || !Array.isArray(data.y) || data.y.length !== data.x.length) return false;
    for (const item of data.y) {
      if (item === null) continue;
      if (typeof item !== 'number' || !Number.isFinite(item)) return false;
      finite++;
    }
    total = data.x.length;
  } else if (data.kind === 'complex') {
    if (!isAxis(data.x, 9, 257) || !isAxis(data.y, 9, 257) ||
        !Array.isArray(data.real) || !Array.isArray(data.imaginary) ||
        data.real.length !== data.y.length || data.imaginary.length !== data.y.length) return false;
    for (let row = 0; row < data.y.length; row++) {
      const real = data.real[row], imaginary = data.imaginary[row];
      if (!Array.isArray(real) || !Array.isArray(imaginary) ||
          real.length !== data.x.length || imaginary.length !== data.x.length) return false;
      for (let column = 0; column < real.length; column++) {
        const re: unknown = real[column], im: unknown = imaginary[column];
        if (re === null && im === null) continue;
        if (typeof re !== 'number' || typeof im !== 'number' ||
            !Number.isFinite(re) || !Number.isFinite(im)) return false;
        finite++;
      }
    }
    total = data.x.length * data.y.length;
  } else return false;
  return finite === data.finiteSamples && total - finite === data.missingSamples;
}
