import { test, expect } from '@playwright/test';
import { readFile } from 'node:fs/promises';

test('cancel an actual chart computation and render/download the next plot', async ({ page }) => {
  await page.goto('/custom-functions');
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({ name: 'slow.tslf', mimeType: 'text/plain',
    buffer: Buffer.from('static real Slow(real x) { while(true) {} }') });
  await expect(page.locator('#customFunctionsTextArea')).toContainText('while(true)');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  const editor = page.locator('#expressionTextArea textarea');
  await editor.fill('Slow(x)');
  const sent = page.waitForRequest(request => request.url().endsWith('/v2/chart') && request.method() === 'POST');
  await page.getByRole('button', { name: 'Draw chart!' }).click(); await sent;
  await page.getByRole('button', { name: 'Cancel chart' }).click();
  await expect(page.getByRole('status')).toHaveText('Chart rendering cancelled.');
  // Ace's textarea is an input buffer: fill() again appends to its document.
  await editor.focus(); await editor.press('ControlOrMeta+A'); await page.keyboard.insertText('x');
  const response = page.waitForResponse(response => response.url().endsWith('/v2/chart') && response.request().method() === 'POST', { timeout: 35_000 });
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  const actual = await response;
  expect(actual.request().postDataJSON().expression).toBe('x');
  expect(actual.ok()).toBeTruthy();
  expect(actual.headers()['content-type']).toContain('application/json');
  const samples = await actual.json();
  expect(samples.kind).toBe('curve');
  expect(samples.y).toEqual(samples.x);
  const chart = page.getByTestId('chart-2d');
  await expect(chart).toBeVisible();
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  const bytes = Buffer.from(await chart.evaluate(async (canvas: HTMLCanvasElement) => {
    if (canvas.width <= 100) throw new Error('The chart did not render.');
    return Array.from(new Uint8Array(await (await fetch(canvas.toDataURL('image/png'))).arrayBuffer()));
  }));
  expect(bytes.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  const pending = page.waitForEvent('download'); await page.getByRole('button', { name: 'Download PNG' }).click();
  expect(await readFile((await (await pending).path())!)).toEqual(bytes);
});
