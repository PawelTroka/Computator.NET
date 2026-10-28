import { test, expect, type Locator, type Page, type Response } from '@playwright/test';
import { readFile, writeFile } from 'node:fs/promises';

// Copied into e2e/ only by the disposable PR #294 + 2D compatibility workflow.
// This is a same-page lifecycle acceptance test against the published Windows
// API and real WebGL renderer. It has no intercepted requests or response fixtures.
type Bounds = { xMin: number; xMax: number; yMin: number; yMax: number };
type Curve = { kind: string; x: number[]; y: (number | null)[]; finiteSamples: number; missingSamples: number };
type Surface = { kind: string; x: number[]; y: number[]; z: (number | null)[][]; finiteSamples: number; missingSamples: number };
type ComplexGrid = { kind: string; x: number[]; y: number[]; real: (number | null)[][]; imaginary: (number | null)[][] };
type Eye = { x: number; y: number; z: number };

async function edit(page: Page, expression: string) {
  const input = page.locator('#expressionTextArea textarea');
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText(expression);
}
function responseFor(page: Page, expression: string) {
  return page.waitForResponse(response => new URL(response.url()).pathname === '/api/v2/chart' &&
    response.request().method() === 'POST' && response.request().postDataJSON().expression === expression,
  { timeout: 35_000 });
}
async function successful(response: Response) {
  expect(response.status(), await response.text()).toBe(200);
  expect(response.headers()['content-type']).toContain('application/json');
  return response.json();
}
async function draw(page: Page, expression: string) {
  await edit(page, expression);
  const pending = responseFor(page, expression);
  await page.getByRole('button', { name: 'Draw chart!', exact: true }).click();
  const response = await pending, data = await successful(response);
  await expect(page.getByRole('button', { name: 'Cancel chart', exact: true })).toHaveCount(0);
  await expect(page.getByRole('alert')).toHaveCount(0);
  return data;
}
async function ready2D(page: Page, kind: 'real' | 'complex') {
  await expect(page.getByTestId('chart-2d')).toHaveAttribute('aria-label', `Interactive ${kind} chart`);
  await expect(page.getByTestId('chart-2d')).toBeVisible();
  await expect(page.getByTestId('surface-chart')).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Cancel chart', exact: true })).toHaveCount(0);
  await expect(page.getByRole('alert')).toHaveCount(0);
}
async function bounds(canvas: Locator): Promise<Bounds> {
  return JSON.parse((await canvas.getAttribute('data-bounds'))!);
}
async function capture(target: Locator, name: string) {
  const path = test.info().outputPath(name + '.png');
  const image = await target.screenshot({ path });
  await test.info().attach(name, { path, contentType: 'image/png' });
  return image;
}
async function canvasPixels(canvas: Locator) {
  return canvas.evaluate((element: HTMLCanvasElement) => {
    const bytes = element.getContext('2d')!.getImageData(0, 0, element.width, element.height).data;
    let colored = 0, hash = 2166136261;
    for (let index = 0; index < bytes.length; index += 4) {
      if (Math.max(bytes[index], bytes[index + 1], bytes[index + 2]) -
          Math.min(bytes[index], bytes[index + 1], bytes[index + 2]) > 35) colored++;
      hash = Math.imul(hash ^ bytes[index], 16777619);
      hash = Math.imul(hash ^ bytes[index + 1], 16777619);
      hash = Math.imul(hash ^ bytes[index + 2], 16777619);
    }
    return { colored, hash };
  });
}
async function exportPng(page: Page, name: string, dimensions?: [number, number]) {
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Download PNG', exact: true }).click();
  const download = await pending;
  expect(download.suggestedFilename()).toBe('computator-chart.png');
  const image = await readFile((await download.path())!);
  expect(image.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  expect(image.length).toBeGreaterThan(2048);
  if (dimensions) expect([image.readUInt32BE(16), image.readUInt32BE(20)]).toEqual(dimensions);
  // Check the composed download, including WebGL, rather than accepting a PNG
  // signature from an empty image or merely inspecting the camera metadata.
  const colored = await page.evaluate(async source => {
    const image = new Image(); image.src = source; await image.decode();
    const canvas = document.createElement('canvas'); canvas.width = image.width; canvas.height = image.height;
    const context = canvas.getContext('2d')!; context.drawImage(image, 0, 0);
    const bytes = context.getImageData(0, 0, canvas.width, canvas.height).data;
    let count = 0;
    for (let index = 0; index < bytes.length; index += 4)
      if (Math.max(bytes[index], bytes[index + 1], bytes[index + 2]) -
          Math.min(bytes[index], bytes[index + 1], bytes[index + 2]) > 35) count++;
    return count;
  }, 'data:image/png;base64,' + image.toString('base64'));
  expect(colored).toBeGreaterThan(100);
  const path = test.info().outputPath(name + '.png');
  await writeFile(path, image); await test.info().attach(name, { path, contentType: 'image/png' });
}

test('real API switches real → WebGL → real → complex on one page with working interactions and export', async ({ page }) => {
  test.setTimeout(90_000);
  const scriptErrors: string[] = [], expressions: string[] = [];
  page.on('pageerror', error => scriptErrors.push(error.message));
  page.on('request', request => {
    if (new URL(request.url()).pathname === '/api/v2/chart' && request.method() === 'POST')
      expressions.push(request.postDataJSON().expression);
  });
  await page.goto('/');
  const canvas = page.getByTestId('chart-2d'), surface = page.getByTestId('surface-chart');

  const first = await draw(page, 'x*x') as Curve;
  expect(first.kind).toBe('curve'); expect(first.missingSamples).toBe(0);
  for (const index of [0, Math.floor(first.x.length / 2), first.x.length - 1])
    expect(first.y[index]).toBeCloseTo(first.x[index] ** 2, 9);
  await ready2D(page, 'real'); expect((await canvasPixels(canvas)).colored).toBeGreaterThan(100);
  const initialBounds = await bounds(canvas);
  await capture(canvas, 'switch-01-real-before');
  const zoomRequest = responseFor(page, 'x*x');
  await page.getByRole('button', { name: 'Zoom in', exact: true }).click();
  const zoomResponse = await zoomRequest, zoomedCurve = await successful(zoomResponse) as Curve;
  await ready2D(page, 'real');
  const zoomedBounds = await bounds(canvas);
  expect(zoomedBounds.xMax - zoomedBounds.xMin).toBeLessThan(initialBounds.xMax - initialBounds.xMin);
  expect(zoomResponse.request().postDataJSON()).toMatchObject({ ...zoomedBounds, interactive2D: true });
  expect(zoomedCurve.x[0]).toBe(zoomedBounds.xMin); expect(zoomedCurve.x.at(-1)).toBe(zoomedBounds.xMax);
  await capture(canvas, 'switch-02-real-zoomed');

  await page.getByLabel('3D surface quality', { exact: true }).selectOption('33');
  const grid = await draw(page, 'x*x+y*y') as Surface;
  expect(grid.kind).toBe('surface'); expect(grid.finiteSamples).toBe(33 * 33); expect(grid.missingSamples).toBe(0);
  for (const [row, column] of [[0, 0], [16, 16], [32, 32]])
    expect(grid.z[row][column]).toBeCloseTo(grid.x[column] ** 2 + grid.y[row] ** 2, 9);
  await expect(canvas).toHaveCount(0); await expect(surface).toBeVisible();
  // Unavailable WebGL fails this Windows acceptance gate; it is not a passed
  // fallback or an expected skip. The Linux focused suite tests fallback separately.
  await expect(page.getByText('Interactive surface ready.', { exact: false })).toBeVisible({ timeout: 25_000 });
  await expect(page.getByRole('alert')).toHaveCount(0);
  await expect.poll(() => surface.locator('canvas').count()).toBeGreaterThan(0);
  const eye = () => surface.evaluate(element => (element as HTMLDivElement & {
    layout?: { scene?: { camera?: { eye?: Eye } } }
  }).layout?.scene?.camera?.eye ?? null);
  const originalEye = await eye(); expect(originalEye).not.toBeNull();
  const surfaceBefore = await capture(surface, 'switch-03-surface-before');
  const cameraRequestCount = expressions.length;
  await page.getByRole('button', { name: 'Rotate left', exact: true }).click();
  await expect.poll(eye).not.toEqual(originalEye);
  const rotated = await eye(); expect(rotated).not.toBeNull();
  const rectangle = (await surface.boundingBox())!;
  await page.mouse.move(rectangle.x + rectangle.width / 2, rectangle.y + rectangle.height / 2);
  await page.mouse.wheel(0, -400);
  await expect.poll(async () => {
    const current = await eye(); return current ? Math.hypot(current.x, current.y, current.z) : Infinity;
  }).toBeLessThan(Math.hypot(rotated!.x, rotated!.y, rotated!.z));
  const surfaceAfter = await capture(surface, 'switch-04-surface-rotated-zoomed');
  expect(surfaceAfter.equals(surfaceBefore)).toBe(false);
  await exportPng(page, 'switch-05-surface-export', [1200, 800]);
  expect(expressions).toHaveLength(cameraRequestCount);

  const returned = await draw(page, 'sin(x)') as Curve;
  expect(returned.kind).toBe('curve');
  for (const index of [0, Math.floor(returned.x.length / 3), returned.x.length - 1])
    expect(returned.y[index]).toBeCloseTo(Math.sin(returned.x[index]), 9);
  await ready2D(page, 'real'); await expect(page.getByRole('button', { name: 'Rotate left', exact: true })).toHaveCount(0);
  const returnedBounds = await bounds(canvas), returnedZoom = responseFor(page, 'sin(x)');
  await page.getByRole('button', { name: 'Zoom in', exact: true }).click();
  const returnedResponse = await returnedZoom, returnedSamples = await successful(returnedResponse) as Curve;
  await ready2D(page, 'real');
  const afterReturn = await bounds(canvas);
  expect(afterReturn.xMax - afterReturn.xMin).toBeLessThan(returnedBounds.xMax - returnedBounds.xMin);
  expect(returnedResponse.request().postDataJSON()).toMatchObject({ ...afterReturn, interactive2D: true });
  expect(returnedSamples.y[0]).toBeCloseTo(Math.sin(returnedSamples.x[0]), 9);
  expect((await canvasPixels(canvas)).colored).toBeGreaterThan(100);
  await capture(canvas, 'switch-06-real-returned-zoomed'); await exportPng(page, 'switch-07-real-export');

  const complex = await draw(page, 'z*z') as ComplexGrid;
  expect(complex.kind).toBe('complex');
  expect(complex.real[0][0]).toBeCloseTo(complex.x[0] ** 2 - complex.y[0] ** 2, 9);
  expect(complex.imaginary[0][0]).toBeCloseTo(2 * complex.x[0] * complex.y[0], 9);
  await ready2D(page, 'complex'); expect((await canvasPixels(canvas)).colored).toBeGreaterThan(1000);
  await capture(canvas, 'switch-08-complex-before');
  const displayRequestCount = expressions.length;
  await page.getByLabel('Complex contours', { exact: true }).selectOption('none');
  await page.getByLabel('Color assignment', { exact: true }).selectOption('darker');
  const darker = await canvasPixels(canvas);
  await page.getByLabel('Color assignment', { exact: true }).selectOption('lighter');
  await expect.poll(async () => (await canvasPixels(canvas)).hash).not.toBe(darker.hash);
  expect(expressions).toHaveLength(displayRequestCount);
  const complexBounds = await bounds(canvas), panRequest = responseFor(page, 'z*z');
  await page.getByRole('button', { name: 'Pan right', exact: true }).click();
  const panResponse = await panRequest, panned = await successful(panResponse) as ComplexGrid;
  await ready2D(page, 'complex');
  const pannedBounds = await bounds(canvas);
  expect(pannedBounds.xMin).toBeGreaterThan(complexBounds.xMin);
  expect(pannedBounds.xMax - pannedBounds.xMin).toBeCloseTo(complexBounds.xMax - complexBounds.xMin, 9);
  expect(panResponse.request().postDataJSON()).toMatchObject({ ...pannedBounds, interactive2D: true });
  expect(panned.real[0][0]).toBeCloseTo(panned.x[0] ** 2 - panned.y[0] ** 2, 9);
  await expect(page.getByLabel('Color assignment', { exact: true })).toHaveValue('lighter');
  await capture(canvas, 'switch-09-complex-panned'); await exportPng(page, 'switch-10-complex-export');
  expect(expressions).toEqual(['x*x', 'x*x', 'x*x+y*y', 'sin(x)', 'sin(x)', 'z*z', 'z*z']);
  expect(scriptErrors).toEqual([]);
});
