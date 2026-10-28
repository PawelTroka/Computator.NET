import { test, expect, type Locator, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';

type Bounds = { xMin: number; xMax: number; yMin: number; yMax: number };
type ChartRequest = Bounds & {
  expression: string; customFunctions: string; interactive2D: boolean;
  curveSamples?: number; complexGridSize?: number;
};
const headers = { 'access-control-allow-origin': '*' };
const definition = 'static real Probe(real x) { return x*x + 1; }';
const originalBounds: Bounds = { xMin: -5, xMax: 5, yMin: -5, yMax: 5 };

// Fixture values depend on the requested mathematical domain. A gesture that
// merely stretches the old pixels cannot satisfy the request/bounds assertions.
function curve(request: ChartRequest) {
  const count = request.curveSamples ?? 257;
  const x = Array.from({ length: count }, (_, index) =>
    request.xMin * (1 - index / (count - 1)) + request.xMax * index / (count - 1));
  const source = request.expression.replaceAll('·', '*');
  const y = x.map(value => source.includes('Probe') ? value * value + 1
    : source === 'cos(x)' ? Math.cos(value) : source === '-x' ? -value
      : source === 'x+1' ? value + 1 : source === 'x²' ? value * value : value);
  return { kind: 'curve', x, y, finiteSamples: count, missingSamples: 0 };
}
function complex(request: ChartRequest) {
  const count = request.complexGridSize ?? 65;
  const axis = (min: number, max: number) => Array.from({ length: count }, (_, index) =>
    min * (1 - index / (count - 1)) + max * index / (count - 1));
  const x = axis(request.xMin, request.xMax), y = axis(request.yMin, request.yMax);
  return { kind: 'complex', x, y, real: y.map(imaginary => x.map(real => real * real - imaginary * imaginary)),
    imaginary: y.map(imaginary => x.map(real => 2 * real * imaginary)),
    finiteSamples: count * count, missingSamples: 0 };
}
async function edit(page: Page, code: string, editor = 'expressionTextArea') {
  const input = page.locator(`#${editor} textarea`);
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText(code);
}
async function ready(page: Page) {
  await expect(page.getByRole('button', { name: 'Cancel chart', exact: true })).toHaveCount(0);
  await expect(page.getByTestId('chart-2d')).toBeVisible();
  await expect(page.getByRole('alert')).toHaveCount(0);
}
async function draw(page: Page, code: string, add = false) {
  await edit(page, code);
  await page.getByRole('button', { name: add ? 'Add function' : 'Draw chart!', exact: true }).click();
  await ready(page);
}
async function bounds(canvas: Locator): Promise<Bounds> {
  return JSON.parse((await canvas.getAttribute('data-bounds'))!);
}
async function pixels(canvas: Locator) {
  return canvas.evaluate((element: HTMLCanvasElement) => {
    const data = element.getContext('2d')!.getImageData(0, 0, element.width, element.height).data;
    let colored = 0, hash = 2166136261;
    for (let index = 0; index < data.length; index += 4) {
      if (Math.max(data[index], data[index + 1], data[index + 2]) -
          Math.min(data[index], data[index + 1], data[index + 2]) > 35) colored++;
      hash = Math.imul(hash ^ data[index], 16777619);
      hash = Math.imul(hash ^ data[index + 1], 16777619);
      hash = Math.imul(hash ^ data[index + 2], 16777619);
    }
    return { colored, hash, width: element.width, height: element.height };
  });
}
async function png(page: Page) {
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Download PNG', exact: true }).click();
  const download = await pending;
  expect(download.suggestedFilename()).toBe('computator-chart.png');
  const bytes = await readFile((await download.path())!);
  expect(bytes.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  expect(bytes.readUInt32BE(16)).toBeGreaterThan(100);
  expect(bytes.readUInt32BE(20)).toBeGreaterThan(100);
  expect(bytes.length).toBeGreaterThan(2048);
  return bytes;
}
async function mocked(page: Page, requests: ChartRequest[], kind: 'curve' | 'complex' = 'curve') {
  await page.route('**/api/v2/chart', route => {
    const request = route.request().postDataJSON() as ChartRequest;
    requests.push(request);
    return route.fulfill({ headers, json: kind === 'curve' ? curve(request) : complex(request) });
  });
}

test.beforeEach(async ({ page }) => {
  await page.route('**/api/autocomplete/**', route => route.fulfill({ headers, json: [] }));
});

test('wheel and pointer pan resample every overlay with submitted definitions and coordinate bounds', async ({ page }) => {
  const requests: ChartRequest[] = [];
  await mocked(page, requests);
  await page.goto('/custom-functions'); await edit(page, definition, 'customFunctionsTextArea');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await draw(page, 'Probe(x)'); await draw(page, 'cos(x)', true);
  const canvas = page.getByTestId('chart-2d');
  await expect(canvas).toHaveAttribute('aria-label', 'Interactive real chart');
  await expect(canvas).toHaveAttribute('data-series-count', '2');
  expect(await bounds(canvas)).toEqual(originalBounds);
  expect((await pixels(canvas)).colored).toBeGreaterThan(100);
  await test.info().attach('real-overlays-before-zoom', { body: await canvas.screenshot(), contentType: 'image/png' });

  // Unsubmitted editor/bounds changes must not leak into gesture requests.
  await edit(page, 'ThisIsNotSubmitted(');
  await page.getByRole('spinbutton', { name: 'XMIN', exact: true }).fill('-88');
  await page.getByRole('spinbutton', { name: 'YMIN', exact: true }).fill('-99');
  const beforeWheel = requests.length, beforePixels = await pixels(canvas);
  const rect = (await canvas.boundingBox())!;
  await page.mouse.move(rect.x + rect.width * 0.55, rect.y + rect.height * 0.45);
  await page.mouse.wheel(0, -400);
  await expect.poll(() => requests.length).toBeGreaterThanOrEqual(beforeWheel + 2);
  await ready(page);
  const zoomed = await bounds(canvas);
  expect(zoomed.xMax - zoomed.xMin).toBeLessThan(10);
  expect(zoomed.yMax - zoomed.yMin).toBeLessThan(10);
  expect(zoomed.xMin).toBeGreaterThan(-5); expect(zoomed.xMax).toBeLessThan(5);
  expect((await pixels(canvas)).hash).not.toBe(beforePixels.hash);
  const wheelRequests = requests.slice(beforeWheel);
  expect(wheelRequests.map(request => request.expression).sort()).toEqual(['Probe(x)', 'cos(x)'].sort());
  for (const request of wheelRequests) {
    expect(request).toMatchObject({ ...zoomed, customFunctions: definition, interactive2D: true });
  }

  const beforePan = requests.length;
  await page.mouse.move(rect.x + rect.width * 0.5, rect.y + rect.height * 0.5);
  await page.mouse.down();
  await page.mouse.move(rect.x + rect.width * 0.64, rect.y + rect.height * 0.59, { steps: 8 });
  await page.mouse.up();
  await expect.poll(() => requests.length).toBeGreaterThanOrEqual(beforePan + 2);
  await ready(page);
  const panned = await bounds(canvas);
  expect(panned.xMin).toBeLessThan(zoomed.xMin);
  expect(panned.yMin).toBeGreaterThan(zoomed.yMin);
  expect(panned.xMax - panned.xMin).toBeCloseTo(zoomed.xMax - zoomed.xMin, 9);
  expect(panned.yMax - panned.yMin).toBeCloseTo(zoomed.yMax - zoomed.yMin, 9);
  for (const request of requests.slice(beforePan)) expect(request.customFunctions).toBe(definition);
  await test.info().attach('real-overlays-after-pan', { body: await canvas.screenshot(), contentType: 'image/png' });
  const exporting = await png(page);
  await test.info().attach('real-overlays-export', { body: exporting, contentType: 'image/png' });

  await page.getByRole('button', { name: 'Reset view', exact: true }).click();
  await expect.poll(() => bounds(canvas)).toEqual(originalBounds);
  await ready(page);
  const count = requests.length;
  await page.setViewportSize({ width: 1100, height: 800 });
  await page.waitForTimeout(350);
  expect(requests).toHaveLength(count);
  await expect(canvas).toHaveAttribute('data-series-count', '2');
});

test('visibility, removal and clear operate on distinct real function layers', async ({ page }) => {
  const requests: ChartRequest[] = [];
  await mocked(page, requests);
  await page.goto('/'); await draw(page, 'x'); await draw(page, '-x', true);
  const canvas = page.getByTestId('chart-2d');
  await expect(page.getByRole('checkbox', { name: 'Show x', exact: true })).toBeChecked();
  await expect(page.getByRole('checkbox', { name: 'Show -x', exact: true })).toBeChecked();
  await expect(canvas).toHaveAttribute('data-series-count', '2');
  const count = requests.length, combined = await pixels(canvas);
  await page.getByRole('checkbox', { name: 'Show -x', exact: true }).uncheck();
  await expect(canvas).toHaveAttribute('data-series-count', '1');
  await expect.poll(async () => (await pixels(canvas)).hash).not.toBe(combined.hash);
  expect(requests).toHaveLength(count);
  await page.getByRole('checkbox', { name: 'Show -x', exact: true }).check();
  await expect(canvas).toHaveAttribute('data-series-count', '2');
  await page.getByRole('button', { name: 'Remove x', exact: true }).click();
  await expect(page.getByRole('checkbox', { name: 'Show x', exact: true })).toHaveCount(0);
  await expect(canvas).toHaveAttribute('data-series-count', '1');
  await page.getByRole('button', { name: 'Pan right', exact: true }).click();
  await expect.poll(() => requests.length).toBeGreaterThan(count);
  await ready(page);
  expect(requests.slice(count).map(request => request.expression)).toEqual(['-x']);
  await page.getByRole('button', { name: 'Clear chart', exact: true }).click();
  await expect(canvas).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Download PNG', exact: true })).toHaveCount(0);
  const clearedCount = requests.length;
  await page.setViewportSize({ width: 1080, height: 760 }); await page.waitForTimeout(350);
  expect(requests).toHaveLength(clearedCount);
  await draw(page, 'x+1');
  await expect(canvas).toHaveAttribute('data-series-count', '1');
});

test('keyboard navigation updates coordinates and Chromium touch pinch resamples its domain', async ({ page, context, browserName }) => {
  const requests: ChartRequest[] = [];
  await mocked(page, requests);
  await page.goto('/'); await draw(page, 'x²');
  const canvas = page.getByTestId('chart-2d');
  await canvas.focus(); await canvas.press('+');
  await expect.poll(() => requests.length).toBe(2); await ready(page);
  const zoomed = await bounds(canvas);
  expect(zoomed.xMax - zoomed.xMin).toBeLessThan(10);
  await canvas.press('ArrowRight');
  await expect.poll(() => requests.length).toBe(3); await ready(page);
  const panned = await bounds(canvas);
  expect(panned.xMin).toBeGreaterThan(zoomed.xMin);
  expect(panned.xMax - panned.xMin).toBeCloseTo(zoomed.xMax - zoomed.xMin, 9);
  await canvas.press('Home');
  await expect.poll(() => requests.length).toBe(4); await ready(page);
  expect(await bounds(canvas)).toEqual(originalBounds);

  if (browserName === 'chromium') {
    // The CDP input domain delivers a native two-finger gesture. This is not
    // dispatchEvent or an invocation of the component's callback from a test.
    const session = await context.newCDPSession(page);
    try {
      await session.send('Emulation.setTouchEmulationEnabled', { enabled: true });
      await canvas.scrollIntoViewIfNeeded();
      const rect = (await canvas.boundingBox())!, x = rect.x + rect.width * 0.55, y = rect.y + rect.height * 0.45;
      const points = (distance: number) => [{ x: x - distance, y, id: 1 }, { x: x + distance, y, id: 2 }];
      await session.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: points(45) });
      await session.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: points(90) });
      await session.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
      await expect.poll(() => requests.length).toBeGreaterThan(4); await ready(page);
      const pinched = await bounds(canvas);
      expect(pinched.xMax - pinched.xMin).toBeLessThan(7);
      expect(pinched.yMax - pinched.yMin).toBeLessThan(7);
      expect(requests.at(-1)).toMatchObject(pinched);
    } finally { await session.detach(); }
  }
});

test('complex color, contour and axes controls redraw cached values and export the selected display', async ({ page }) => {
  const requests: ChartRequest[] = [];
  await mocked(page, requests, 'complex');
  await page.goto('/'); await draw(page, 'z²');
  const canvas = page.getByTestId('chart-2d');
  await expect(canvas).toHaveAttribute('aria-label', 'Interactive complex chart');
  expect((await pixels(canvas)).colored).toBeGreaterThan(1000);
  const count = requests.length;
  await page.getByLabel('Complex contours', { exact: true }).selectOption({ label: 'None' });
  await page.getByLabel('Color assignment', { exact: true }).selectOption({ label: 'Greater is darker' });
  const darker = await pixels(canvas), darkerPng = await png(page);
  await page.getByLabel('Color assignment', { exact: true }).selectOption({ label: 'Greater is lighter' });
  await expect.poll(async () => (await pixels(canvas)).hash).not.toBe(darker.hash);
  const lighter = await pixels(canvas);
  await page.getByLabel('Complex contours', { exact: true }).selectOption({ label: 'Logarithmic' });
  await expect.poll(async () => (await pixels(canvas)).hash).not.toBe(lighter.hash);
  const logarithmic = await pixels(canvas);
  await page.getByLabel('Contour spacing', { exact: true }).fill('2');
  await expect.poll(async () => (await pixels(canvas)).hash).not.toBe(logarithmic.hash);
  await page.getByLabel('Complex contours', { exact: true }).selectOption({ label: 'Linear' });
  const axes = await pixels(canvas);
  await page.getByRole('checkbox', { name: 'Show axes', exact: true }).uncheck();
  await expect.poll(async () => (await pixels(canvas)).hash).not.toBe(axes.hash);
  const selectedPng = await png(page);
  expect(selectedPng.equals(darkerPng)).toBe(false);
  expect(requests).toHaveLength(count);
  await test.info().attach('complex-selected-display', { body: await canvas.screenshot(), contentType: 'image/png' });
  await test.info().attach('complex-selected-export', { body: selectedPng, contentType: 'image/png' });
  await page.getByRole('button', { name: 'Zoom in', exact: true }).click();
  await expect.poll(() => requests.length).toBe(count + 1);
  await ready(page);
  const zoomed = await bounds(canvas);
  expect(zoomed.xMax - zoomed.xMin).toBeLessThan(10);
  expect(requests.at(-1)).toMatchObject({ expression: 'z²', ...zoomed, interactive2D: true });
  await expect(page.getByLabel('Complex contours', { exact: true })).toHaveValue('linear');
  await expect(page.getByRole('checkbox', { name: 'Show axes', exact: true })).not.toBeChecked();
});

test('cancelling a resample preserves the last viewport and ignores its late response after recovery', async ({ page }) => {
  let release!: () => void;
  const gate = new Promise<void>(resolve => { release = resolve; });
  const requests: ChartRequest[] = [];
  await page.route('**/api/v2/chart', async route => {
    const request = route.request().postDataJSON() as ChartRequest;
    requests.push(request);
    if (requests.length === 2) await gate;
    await route.fulfill({ headers, json: curve(request) }).catch(() => { /* Cancelled transport. */ });
  });
  try {
    await page.goto('/'); await draw(page, 'x');
    const canvas = page.getByTestId('chart-2d');
    const original = await bounds(canvas), originalImage = await pixels(canvas);
    await page.getByRole('button', { name: 'Zoom in', exact: true }).click();
    await expect.poll(() => requests.length).toBe(2);
    await expect(page.getByRole('button', { name: 'Download PNG', exact: true })).toBeDisabled();
    await page.getByRole('button', { name: 'Cancel chart', exact: true }).click();
    await expect(page.getByRole('status').first()).toContainText('cancelled');
    expect(await bounds(canvas)).toEqual(original);
    expect((await pixels(canvas)).hash).toBe(originalImage.hash);
    await draw(page, 'x+1');
    const recovered = await pixels(canvas);
    release(); await page.waitForTimeout(250);
    expect((await pixels(canvas)).hash).toBe(recovered.hash);
    await expect(page.getByRole('checkbox', { name: 'Show x+1', exact: true })).toBeVisible();
    await expect(page.getByRole('checkbox', { name: 'Show x', exact: true })).toHaveCount(0);
    expect(requests).toHaveLength(3);
  } finally { release(); }
});

for (const malformed of ['shape', 'empty-domain', 'wrong-domain'] as const) {
test(`${malformed} numerical response preserves the last correctly labelled and exportable chart`, async ({ page }) => {
  let count = 0;
  await page.route('**/api/v2/chart', route => {
    const request = route.request().postDataJSON() as ChartRequest;
    const valid = curve(request);
    const invalid = malformed === 'shape'
      ? { kind: 'curve', x: [0, 1], y: [7], finiteSamples: 2, missingSamples: 0 }
      : malformed === 'empty-domain' ? { ...valid, y: valid.y.map(() => null), finiteSamples: 0, missingSamples: valid.y.length }
        : curve({ ...request, xMin: 100, xMax: 200 });
    return route.fulfill({ headers, json: ++count === 1 ? valid : invalid });
  });
  await page.goto('/'); await draw(page, 'x');
  const canvas = page.getByTestId('chart-2d'), before = await pixels(canvas);
  await edit(page, 'x+1'); await page.getByRole('button', { name: 'Draw chart!', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText(/invalid/i);
  expect((await pixels(canvas)).hash).toBe(before.hash);
  await expect(page.getByRole('checkbox', { name: 'Show x', exact: true })).toBeVisible();
  await expect(page.getByRole('checkbox', { name: 'Show x+1', exact: true })).toHaveCount(0);
  await png(page);
  await page.setViewportSize({ width: 1080, height: 760 }); await page.waitForTimeout(350);
  expect(count).toBe(2);
});
}

test('adding a complex plot does not silently replace existing real overlays', async ({ page }) => {
  const requests: ChartRequest[] = [];
  await page.route('**/api/v2/chart', route => {
    const request = route.request().postDataJSON() as ChartRequest;
    requests.push(request);
    return route.fulfill({ headers, json: request.expression.includes('z') ? complex(request) : curve(request) });
  });
  await page.goto('/'); await draw(page, 'x');
  const before = await pixels(page.getByTestId('chart-2d'));
  await edit(page, 'z²'); await page.getByRole('button', { name: 'Add function', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText(/real|complex|overlay/i);
  await expect(page.getByTestId('chart-2d')).toHaveAttribute('aria-label', 'Interactive real chart');
  expect((await pixels(page.getByTestId('chart-2d'))).hash).toBe(before.hash);
  await expect(page.getByRole('checkbox', { name: 'Show x', exact: true })).toBeChecked();
  await page.getByRole('button', { name: 'Draw chart!', exact: true }).click();
  await ready(page);
  await expect(page.getByTestId('chart-2d')).toHaveAttribute('aria-label', 'Interactive complex chart');
});
