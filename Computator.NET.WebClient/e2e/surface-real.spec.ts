import { test, expect } from '@playwright/test';
import { readFile } from 'node:fs/promises';

// Runs only against the actual Windows mathematical API and published web host.
// No route mocks or predetermined server responses are used.
test('f(x,y) real API samples and browser camera/export agree with a paraboloid', async ({ page, request }) => {
  const numerical = await request.post('/api/v2/chart', {
    data: { expression: 'x*x + y*y', gridSize: 9, xMin: -2, xMax: 2, yMin: -2, yMax: 2 }
  });
  expect(numerical.status(), await numerical.text()).toBe(200);
  const grid = await numerical.json() as {
    kind: string; x: number[]; y: number[]; z: (number | null)[][]; finiteSamples: number; missingSamples: number
  };
  expect(grid.kind).toBe('surface');
  expect(grid.x).toHaveLength(9); expect(grid.y).toHaveLength(9);
  expect(grid.finiteSamples).toBe(81); expect(grid.missingSamples).toBe(0);
  expect(grid.z[0][0]).toBeCloseTo(8, 8);
  expect(grid.z[4][4]).toBeCloseTo(0, 8);
  expect(grid.z[8][8]).toBeCloseTo(8, 8);

  await page.goto('/');
  await page.locator('#expressionTextArea textarea').fill('x*x+y*y');
  await page.getByLabel('3D surface quality').selectOption('33');
  const pending = page.waitForResponse(response =>
    new URL(response.url()).pathname === '/api/v2/chart' && response.request().method() === 'POST',
    { timeout: 35000 });
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  const response = await pending;
  expect(response.status()).toBe(200);
  expect(response.headers()['content-type']).toContain('application/json');
  const shown = await response.json() as { z: number[][]; finiteSamples: number };
  expect(shown.finiteSamples).toBe(33 * 33);
  expect(shown.z[16][16]).toBeCloseTo(0, 8);
  const surface = page.getByTestId('surface-chart');
  await expect(surface).toBeVisible();
  await expect(page.getByText('Interactive surface ready.', { exact: false })).toBeVisible({ timeout: 25000 });
  const eye = () => surface.evaluate(element => {
    const graph = element as HTMLDivElement & { layout?: { scene?: { camera?: { eye?: { x: number; y: number; z: number } } } } };
    return graph.layout?.scene?.camera?.eye ?? null;
  });
  const initial = await eye();
  expect(initial).not.toBeNull();
  await page.getByRole('button', { name: 'Rotate left' }).click();
  await expect.poll(eye).not.toEqual(initial);
  const rotated = await eye();
  const rect = await surface.boundingBox();
  expect(rect).not.toBeNull();
  const x = rect!.x + rect!.width * 0.5, y = rect!.y + rect!.height * 0.5;
  await page.mouse.move(x, y); await page.mouse.wheel(0, -500);
  await expect.poll(eye).not.toEqual(rotated);
  const downloading = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Download PNG' }).click();
  const download = await downloading;
  const png = await readFile((await download.path())!);
  expect(download.suggestedFilename()).toBe('computator-chart.png');
  expect(png.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  expect(png.length).toBeGreaterThan(2048);
});
