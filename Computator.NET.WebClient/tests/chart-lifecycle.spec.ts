import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFklEQVR4nGP8//8/AwMDEwMDAwMDAwAkBgMB/DXemwAAAABJRU5ErkJggg==', 'base64');
const headers = { 'access-control-allow-origin': '*' };
async function expression(page: Page, code: string) {
  const input = page.locator('#expressionTextArea textarea');
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText(code);
}
test.beforeEach(async ({ page }) => {
  await page.route('**/api/autocomplete/**', route => route.fulfill({ json: [], headers }));
});

test('typing and resizing do not execute a chart before Draw is chosen', async ({ page }) => {
  const requests: string[] = [];
  await page.route('**/api/v2/chart', route => { requests.push(route.request().postData()!); return route.fulfill({ body: png, contentType: 'image/png', headers }); });
  await page.goto('/'); await expression(page, 'x');
  await page.clock.install(); await page.setViewportSize({ width: 1100, height: 800 }); await page.clock.runFor(500);
  expect(requests).toEqual([]);
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('img', { name: 'x', exact: true })).toBeVisible();
  expect(requests).toHaveLength(1);
});

test('resize retains submitted expression, definitions and independent bounds', async ({ page }) => {
  const requests: { expression: string; customFunctions: string; xMin: number; yMin: number }[] = [];
  await page.route('**/api/v2/chart', route => { requests.push(route.request().postDataJSON()); return route.fulfill({ body: png, contentType: 'image/png', headers }); });
  await page.goto('/'); await expression(page, 'x');
  await page.getByRole('spinbutton', { name: 'XMIN', exact: true }).fill('-2');
  await page.getByRole('spinbutton', { name: 'YMIN', exact: true }).fill('-7');
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  await expression(page, 'x+9'); await page.getByRole('spinbutton', { name: 'YMIN', exact: true }).fill('-8');
  const pending = page.waitForRequest('**/api/v2/chart');
  await page.setViewportSize({ width: 1100, height: 850 }); await pending;
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  expect(requests[1]).toMatchObject({ expression: 'x', customFunctions: '', xMin: -2, yMin: -7 });
  await expect(page.getByRole('img', { name: 'x', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('img', { name: 'x+9', exact: true })).toBeVisible();
  expect(requests.at(-1)).toMatchObject({ expression: 'x+9', yMin: -8 });
});

test('cancel aborts the request, ignores its late response and does not restart on resize', async ({ page }) => {
  await page.addInitScript(() => {
    const original = window.fetch;
    (window as unknown as { chartAborts: number }).chartAborts = 0;
    window.fetch = (input, init) => {
      if (String(input).endsWith('/v2/chart')) init?.signal?.addEventListener('abort', () => {
        (window as unknown as { chartAborts: number }).chartAborts++;
      });
      return original(input, init);
    };
  });
  let release!: () => void;
  const gate = new Promise<void>(resolve => { release = resolve; });
  let count = 0;
  await page.route('**/api/v2/chart', async route => {
    count++;
    if (count === 1) await gate;
    await route.fulfill({ body: png, contentType: 'image/png', headers }).catch(() => { /* Aborted request. */ });
  });
  try {
    await page.goto('/'); await expression(page, 'x');
    const sent = page.waitForRequest('**/api/v2/chart');
    await page.getByRole('button', { name: 'Draw chart!' }).click(); await sent;
    await page.getByRole('button', { name: 'Cancel chart' }).click();
    await expect.poll(() => page.evaluate(() => (window as unknown as { chartAborts: number }).chartAborts)).toBe(1);
    release();
    await expect(page.getByRole('status')).toHaveText('Chart rendering cancelled.');
    await expect(page.locator('.chart-image')).toHaveCount(0);
    await page.clock.install(); await page.setViewportSize({ width: 1100, height: 850 }); await page.clock.runFor(500);
    expect(count).toBe(1);
    await expression(page, 'x+1'); await page.getByRole('button', { name: 'Draw chart!' }).click();
    await expect(page.getByRole('img', { name: 'x+1', exact: true })).toBeVisible();
  } finally { release(); }
});

test('failed redraw keeps the previous correctly-labelled image and exports its actual PNG', async ({ page }) => {
  let count = 0;
  await page.route('**/api/v2/chart', route => ++count === 1
    ? route.fulfill({ body: png, contentType: 'image/png', headers })
    : route.fulfill({ status: 502, body: 'upstream unavailable', contentType: 'text/plain', headers }));
  await page.goto('/'); await expression(page, 'x'); await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  await expression(page, 'x+99'); await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('alert')).toHaveText('Chart generation failed (502).');
  await expect(page.getByRole('img', { name: 'x', exact: true })).toBeVisible();
  const pending = page.waitForEvent('download'); await page.getByRole('link', { name: 'Download PNG' }).click();
  const file = await pending;
  expect(file.suggestedFilename()).toBe('computator-chart.png');
  expect(await readFile((await file.path())!)).toEqual(png);
});

test('replacement and route departure revoke owned image URLs', async ({ page }) => {
  await page.addInitScript(() => {
    const urls = { created: [] as string[], revoked: [] as string[] };
    (window as unknown as { chartUrls: typeof urls }).chartUrls = urls;
    const create = URL.createObjectURL.bind(URL), revoke = URL.revokeObjectURL.bind(URL);
    URL.createObjectURL = blob => { const url = create(blob); urls.created.push(url); return url; };
    URL.revokeObjectURL = url => { urls.revoked.push(url); revoke(url); };
  });
  await page.route('**/api/v2/chart', route => route.fulfill({ body: png, contentType: 'image/png', headers }));
  await page.goto('/'); await expression(page, 'x'); await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  await expression(page, 'x+1'); await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('img', { name: 'x+1', exact: true })).toBeVisible();
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await expect.poll(() => page.evaluate(() => {
    const urls = (window as unknown as { chartUrls: { created: string[]; revoked: string[] } }).chartUrls;
    return { created: urls.created.length, remaining: urls.created.filter(url => !urls.revoked.includes(url)) };
  })).toEqual({ created: 2, remaining: [] });
});
