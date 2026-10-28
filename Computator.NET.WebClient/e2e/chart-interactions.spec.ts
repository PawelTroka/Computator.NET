import { test, expect, type Locator, type Page, type Response } from '@playwright/test';
import { readFile, writeFile } from 'node:fs/promises';

// These tests run through the published Windows API and same-origin web host.
// Expressions and unsaved definitions are evaluated by the production worker;
// there are no intercepted requests, response fixtures or test-only evaluators.
type Bounds = { xMin: number; xMax: number; yMin: number; yMax: number };
type Curve = { kind: string; x: number[]; y: (number | null)[]; finiteSamples: number; missingSamples: number };
type ComplexGrid = { kind: string; x: number[]; y: number[];
  real: (number | null)[][]; imaginary: (number | null)[][]; finiteSamples: number; missingSamples: number };
const definition = 'static real BrowserCurveProbe(real x) { return x*x + 1; }';

async function evidence(name: string, bytes: Buffer) {
  // Keep successful-run screenshots in the uploaded test-results artifact;
  // body-only attachments are discarded by this suite's JUnit reporter.
  const path = test.info().outputPath(name + '.png');
  await writeFile(path, bytes);
  await test.info().attach(name, { path, contentType: 'image/png' });
}

async function edit(page: Page, code: string, editor = 'expressionTextArea') {
  const input = page.locator(`#${editor} textarea`);
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText(code);
}
function chartResponse(page: Page, expression: string) {
  return page.waitForResponse(response => new URL(response.url()).pathname === '/api/v2/chart' &&
    response.request().method() === 'POST' && response.request().postDataJSON().expression === expression,
  { timeout: 35_000 });
}
async function successful(response: Response) {
  expect(response.status(), await response.text()).toBe(200);
  expect(response.headers()['content-type']).toContain('application/json');
  return response.json();
}
async function ready(page: Page) {
  await expect(page.getByRole('button', { name: 'Cancel chart', exact: true })).toHaveCount(0, { timeout: 35_000 });
  await expect(page.getByTestId('chart-2d')).toBeVisible();
  await expect(page.getByRole('alert')).toHaveCount(0);
}
async function draw(page: Page, expression: string, add = false) {
  await edit(page, expression);
  const pending = chartResponse(page, expression);
  await page.getByRole('button', { name: add ? 'Add function' : 'Draw chart!', exact: true }).click();
  const response = await pending, data = await successful(response);
  await ready(page);
  return { response, data };
}
async function bounds(canvas: Locator): Promise<Bounds> {
  return JSON.parse((await canvas.getAttribute('data-bounds'))!);
}
async function pixels(canvas: Locator) {
  return canvas.evaluate((element: HTMLCanvasElement) => {
    const data = element.getContext('2d')!.getImageData(0, 0, element.width, element.height).data;
    let colored = 0, hash = 2166136261;
    for (let index = 0; index < data.length; index += 4) {
      const x = (index / 4) % element.width, y = Math.floor(index / 4 / element.width);
      if (x > element.width * 0.15 && x < element.width * 0.85 &&
          y > element.height * 0.1 && y < element.height * 0.7 &&
          Math.max(data[index], data[index + 1], data[index + 2]) -
          Math.min(data[index], data[index + 1], data[index + 2]) > 35) colored++;
      hash = Math.imul(hash ^ data[index], 16777619);
      hash = Math.imul(hash ^ data[index + 1], 16777619);
      hash = Math.imul(hash ^ data[index + 2], 16777619);
    }
    return { colored, hash };
  });
}
async function exportPng(page: Page, name: string) {
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Download PNG', exact: true }).click();
  const download = await pending;
  expect(download.suggestedFilename()).toBe('computator-chart.png');
  const bytes = await readFile((await download.path())!);
  expect(bytes.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  expect(bytes.readUInt32BE(16)).toBeGreaterThan(100);
  expect(bytes.readUInt32BE(20)).toBeGreaterThan(100);
  expect(bytes.length).toBeGreaterThan(2048);
  // Decode the actual download as well as inspecting its PNG header. A blank
  // screenshot with the right signature is not successful chart export.
  const colored = await page.evaluate(async source => {
    const image = new Image(); image.src = source; await image.decode();
    const canvas = document.createElement('canvas');
    canvas.width = image.width; canvas.height = image.height;
    const context = canvas.getContext('2d')!; context.drawImage(image, 0, 0);
    const pixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
    let count = 0;
    for (let index = 0; index < pixels.length; index += 4) {
      const x = (index / 4) % canvas.width, y = Math.floor(index / 4 / canvas.width);
      if (x > canvas.width * 0.15 && x < canvas.width * 0.85 &&
          y > canvas.height * 0.1 && y < canvas.height * 0.7 &&
          Math.max(pixels[index], pixels[index + 1], pixels[index + 2]) -
          Math.min(pixels[index], pixels[index + 1], pixels[index + 2]) > 35) count++;
    }
    return count;
  }, 'data:image/png;base64,' + bytes.toString('base64'));
  expect(colored).toBeGreaterThan(100);
  await evidence(name, bytes);
  return bytes;
}

test('real API custom-function overlays are resampled by wheel zoom and pointer pan, then exported', async ({ page }) => {
  test.setTimeout(180_000);
  await page.goto('/custom-functions'); await edit(page, definition, 'customFunctionsTextArea');
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  const first = await draw(page, 'BrowserCurveProbe(x)');
  const curve = first.data as Curve;
  expect(curve.kind).toBe('curve');
  expect(curve.finiteSamples).toBe(curve.x.length); expect(curve.missingSamples).toBe(0);
  for (const index of [0, Math.floor(curve.x.length / 2), curve.x.length - 1])
    expect(curve.y[index]).toBeCloseTo(curve.x[index] ** 2 + 1, 9);
  expect(first.response.request().postDataJSON()).toMatchObject({ interactive2D: true, customFunctions: definition });
  const second = await draw(page, 'cos(x)', true);
  expect((second.data as Curve).kind).toBe('curve');
  const canvas = page.getByTestId('chart-2d');
  await expect(canvas).toHaveAttribute('data-series-count', '2');
  expect((await pixels(canvas)).colored).toBeGreaterThan(100);
  const original = await bounds(canvas), originalImage = await pixels(canvas);
  await evidence('real-api-overlays-before', await canvas.screenshot());

  await edit(page, 'UnsubmittedInvalidExpression(');
  const zoomRequests = [chartResponse(page, 'BrowserCurveProbe(x)'), chartResponse(page, 'cos(x)')];
  const rect = (await canvas.boundingBox())!;
  await page.mouse.move(rect.x + rect.width * 0.52, rect.y + rect.height * 0.48);
  await page.mouse.wheel(0, -350);
  const zoomResponses = await Promise.all(zoomRequests);
  const zoomData = await Promise.all(zoomResponses.map(successful)) as Curve[];
  await ready(page);
  const zoomed = await bounds(canvas);
  expect(zoomed.xMax - zoomed.xMin).toBeLessThan(original.xMax - original.xMin);
  expect((await pixels(canvas)).hash).not.toBe(originalImage.hash);
  for (const response of zoomResponses)
    expect(response.request().postDataJSON()).toMatchObject({ ...zoomed, customFunctions: definition });
  expect(zoomData[0].x[0]).toBeCloseTo(zoomed.xMin, 9);
  expect(zoomData[0].x.at(-1)).toBeCloseTo(zoomed.xMax, 9);
  const center = Math.floor(zoomData[0].x.length / 2);
  expect(zoomData[0].y[center]).toBeCloseTo(zoomData[0].x[center] ** 2 + 1, 9);

  const panRequests = [chartResponse(page, 'BrowserCurveProbe(x)'), chartResponse(page, 'cos(x)')];
  await page.mouse.move(rect.x + rect.width * 0.5, rect.y + rect.height * 0.5);
  await page.mouse.down();
  await page.mouse.move(rect.x + rect.width * 0.64, rect.y + rect.height * 0.58, { steps: 8 });
  await page.mouse.up();
  const panResponses = await Promise.all(panRequests);
  await Promise.all(panResponses.map(successful)); await ready(page);
  const panned = await bounds(canvas);
  expect(panned.xMin).toBeLessThan(zoomed.xMin);
  expect(panned.yMin).toBeGreaterThan(zoomed.yMin);
  expect(panned.xMax - panned.xMin).toBeCloseTo(zoomed.xMax - zoomed.xMin, 9);
  await evidence('real-api-overlays-panned', await canvas.screenshot());
  await exportPng(page, 'real-api-overlays-export');
});

test('real complex samples support cached display controls and numerical navigation', async ({ page, request }) => {
  test.setTimeout(150_000);
  const numerical = await request.post('/api/v2/chart', {
    data: { expression: 'z²', interactive2D: true, complexGridSize: 9, xMin: -2, xMax: 2, yMin: -2, yMax: 2 }
  });
  expect(numerical.status(), await numerical.text()).toBe(200);
  const grid = await numerical.json() as ComplexGrid;
  expect(grid.kind).toBe('complex');
  expect(grid.x).toHaveLength(9); expect(grid.y).toHaveLength(9);
  expect(grid.finiteSamples).toBe(81); expect(grid.missingSamples).toBe(0);
  expect(grid.real[0][0]).toBeCloseTo(0, 9); expect(grid.imaginary[0][0]).toBeCloseTo(8, 9);
  expect(grid.real[4][4]).toBeCloseTo(0, 9); expect(grid.imaginary[4][4]).toBeCloseTo(0, 9);
  expect(grid.real[4][8]).toBeCloseTo(4, 9); expect(grid.imaginary[4][8]).toBeCloseTo(0, 9);

  let chartRequests = 0;
  page.on('request', incoming => {
    if (new URL(incoming.url()).pathname === '/api/v2/chart' && incoming.method() === 'POST') chartRequests++;
  });
  await page.goto('/'); await draw(page, 'z²');
  const canvas = page.getByTestId('chart-2d');
  await expect(canvas).toHaveAttribute('aria-label', 'Interactive complex chart');
  expect((await pixels(canvas)).colored).toBeGreaterThan(1000);
  const count = chartRequests;
  await page.getByLabel('Complex contours', { exact: true }).selectOption({ label: 'None' });
  await page.getByLabel('Color assignment', { exact: true }).selectOption({ label: 'Greater is darker' });
  const initial = await pixels(canvas), initialPng = await exportPng(page, 'real-api-complex-dark');
  await page.getByLabel('Color assignment', { exact: true }).selectOption({ label: 'Greater is lighter' });
  await expect.poll(async () => (await pixels(canvas)).hash).not.toBe(initial.hash);
  await page.getByLabel('Complex contours', { exact: true }).selectOption({ label: 'Linear' });
  await page.getByLabel('Contour spacing', { exact: true }).fill('1.5');
  const axes = await pixels(canvas);
  await page.getByRole('checkbox', { name: 'Show axes', exact: true }).uncheck();
  await expect.poll(async () => (await pixels(canvas)).hash).not.toBe(axes.hash);
  const displayedPng = await exportPng(page, 'real-api-complex-linear-export');
  expect(displayedPng.equals(initialPng)).toBe(false);
  expect(chartRequests).toBe(count);
  await evidence('real-api-complex-selected', await canvas.screenshot());

  const before = await bounds(canvas), pending = chartResponse(page, 'z²');
  await page.getByRole('button', { name: 'Zoom in', exact: true }).click();
  const response = await pending, sampled = await successful(response) as ComplexGrid;
  await ready(page);
  const after = await bounds(canvas);
  expect(after.xMax - after.xMin).toBeLessThan(before.xMax - before.xMin);
  expect(response.request().postDataJSON()).toMatchObject({ ...after, interactive2D: true });
  const x = sampled.x[0], y = sampled.y[0];
  expect(sampled.real[0][0]).toBeCloseTo(x * x - y * y, 8);
  expect(sampled.imaginary[0][0]).toBeCloseTo(2 * x * y, 8);
  await expect(page.getByLabel('Complex contours', { exact: true })).toHaveValue('linear');
  await expect(page.getByRole('checkbox', { name: 'Show axes', exact: true })).not.toBeChecked();
});

test('interactive numerical requests keep singular holes and the established PNG contract', async ({ request }) => {
  test.setTimeout(100_000);
  const singular = await request.post('/api/v2/chart', {
    data: { expression: '1/x', interactive2D: true, curveSamples: 65, xMin: -2, xMax: 2, yMin: -5, yMax: 5 }
  });
  expect(singular.status(), await singular.text()).toBe(200);
  const curve = await singular.json() as Curve;
  expect(curve.kind).toBe('curve');
  const center = curve.x.indexOf(0);
  expect(center).toBeGreaterThanOrEqual(0); expect(curve.y[center]).toBeNull();
  expect(curve.missingSamples).toBeGreaterThan(0);
  expect(curve.y[0]).toBeCloseTo(-0.5, 9); expect(curve.y.at(-1)).toBeCloseTo(0.5, 9);

  const image = await request.post('/api/v2/chart', {
    data: { expression: 'x²', width: 320, height: 240, xMin: -2, xMax: 2, yMin: -1, yMax: 5 }
  });
  expect(image.status(), await image.text()).toBe(200);
  expect(image.headers()['content-type']).toContain('image/png');
  const bytes = await image.body();
  expect(bytes.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  expect(bytes.readUInt32BE(16)).toBe(320); expect(bytes.readUInt32BE(20)).toBe(240);
  expect(bytes.length).toBeGreaterThan(1024);
});
