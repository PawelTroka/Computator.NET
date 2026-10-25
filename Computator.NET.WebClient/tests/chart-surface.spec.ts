import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';

const coordinates = Array.from({ length: 9 }, (_, i) => i - 4);
const grid = {
  kind: 'surface' as const, x: coordinates, y: coordinates,
  z: coordinates.map(y => coordinates.map(x => x * x + y * y)),
  finiteSamples: 81, missingSamples: 0
};
const headers = { 'access-control-allow-origin': '*' };
async function edit(page: Page, code: string) {
  const input = page.locator('#expressionTextArea textarea');
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText(code);
}
test.beforeEach(async ({ page }) => {
  await page.route('**/api/autocomplete/**', route => route.fulfill({ json: [], headers }));
});

test('surface uses real WebGL rotation, zoom, labels and exported PNG without a second API call on resize', async ({ page }) => {
  const requests: Record<string, unknown>[] = [];
  await page.route('**/api/v2/chart', route => {
    requests.push(route.request().postDataJSON());
    return route.fulfill({ json: grid, headers });
  });
  await page.goto('/'); await edit(page, 'x*x+y*y');
  const before = page.waitForRequest('**/api/v2/chart');
  await page.getByRole('button', { name: 'Draw chart!' }).click(); await before;
  const surface = page.getByTestId('surface-chart');
  await expect(surface).toBeVisible();
  await expect(page.getByText('Interactive surface ready.', { exact: false })).toBeVisible({ timeout: 20000 });
  await expect.poll(() => surface.locator('canvas').count()).toBeGreaterThan(0);
  expect(requests).toHaveLength(1);
  expect(requests[0]).toMatchObject({ gridSize: 65, xMin: -5, xMax: 5, customFunctions: '' });
  const titles = await surface.evaluate(element => {
    const layout = (element as HTMLDivElement & { layout?: {
      scene?: { xaxis?: { title?: { text?: string } }; yaxis?: { title?: { text?: string } }; zaxis?: { title?: { text?: string } } }
    } }).layout;
    return [layout?.scene?.xaxis?.title?.text, layout?.scene?.yaxis?.title?.text, layout?.scene?.zaxis?.title?.text];
  });
  expect(titles).toEqual(['x', 'y', 'f(x,y)']);
  const position = await surface.boundingBox();
  expect(position).not.toBeNull();
  const x = position!.x + position!.width * 0.48, y = position!.y + position!.height * 0.46;
  await page.mouse.move(x, y); await page.mouse.down();
  await page.mouse.move(x + 100, y + 55, { steps: 12 }); await page.mouse.up();
  await expect.poll(() => surface.getAttribute('data-camera'), { timeout: 10000 }).not.toBeNull();
  const rotated = await surface.getAttribute('data-camera');
  await page.mouse.move(x, y);
  await page.mouse.wheel(0, -400);
  await expect.poll(() => surface.getAttribute('data-camera'), { timeout: 10000 }).not.toBe(rotated);
  const download = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Download PNG' }).click();
  const file = await download;
  expect(file.suggestedFilename()).toBe('computator-chart.png');
  const bytes = await readFile((await file.path())!);
  expect(bytes.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  expect(bytes.length).toBeGreaterThan(2048);

  await page.setViewportSize({ width: 1080, height: 760 });
  await expect(surface).toBeVisible();
  await page.waitForTimeout(350);
  expect(requests).toHaveLength(1);
  await page.getByLabel('3D surface quality').selectOption('97');
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect.poll(() => requests.length).toBe(2);
  expect(requests[1]).toMatchObject({ gridSize: 97 });
});

test('a malformed surface is rejected instead of rendering unbounded or non-finite data', async ({ page }) => {
  await page.route('**/api/v2/chart', route => route.fulfill({ json: {
    ...grid, z: grid.z.map((row, index) => index === 0 ? [Number.MAX_VALUE, ...row] : row)
  }, headers }));
  await page.goto('/'); await edit(page, 'x+y');
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('alert')).toHaveText('Invalid 3D surface response.');
  await expect(page.getByTestId('surface-chart')).toHaveCount(0);
});

test('the submitted surface includes unsaved two-argument definitions from the current tab', async ({ page }) => {
  const requests: Record<string, unknown>[] = [];
  await page.route('**/api/v2/chart', route => {
    requests.push(route.request().postDataJSON());
    return route.fulfill({ json: grid, headers });
  });
  await page.goto('/custom-functions');
  await page.locator('#customFunctionsTextArea textarea').fill(
    'static real SurfaceProbe(real x, real y) { return x*x+y*y+3; }');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await edit(page, 'SurfaceProbe(x,y)');
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect.poll(() => requests.length).toBe(1);
  expect(requests[0].customFunctions).toContain('SurfaceProbe');
  await expect(page.getByTestId('surface-chart')).toBeVisible();
});

test('cancelled 3D requests do not display a stale surface', async ({ page }) => {
  let release!: () => void;
  const gate = new Promise<void>(resolve => { release = resolve; });
  let count = 0;
  await page.route('**/api/v2/chart', async route => {
    count++;
    if (count === 1) await gate;
    await route.fulfill({ json: grid, headers }).catch(() => {});
  });
  try {
    await page.goto('/'); await edit(page, 'x+y');
    const request = page.waitForRequest('**/api/v2/chart');
    await page.getByRole('button', { name: 'Draw chart!' }).click(); await request;
    await page.getByRole('button', { name: 'Cancel chart' }).click();
    release();
    await expect(page.getByRole('status').first()).toHaveText('Chart rendering cancelled.');
    await expect(page.getByTestId('surface-chart')).toHaveCount(0);
    await edit(page, 'x*x+y*y');
    await page.getByRole('button', { name: 'Draw chart!' }).click();
    await expect(page.getByTestId('surface-chart')).toBeVisible();
    expect(count).toBe(2);
  } finally { release(); }
});
