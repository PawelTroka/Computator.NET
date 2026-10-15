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
  await editor.fill('x');
  const response = page.waitForResponse(response => response.url().endsWith('/v2/chart') && response.request().postDataJSON().expression === 'x');
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  const actual = await response; expect(actual.ok()).toBeTruthy();
  const bytes = await actual.body();
  expect(bytes.subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  await expect(page.getByRole('img', { name: 'x', exact: true })).toBeVisible();
  const pending = page.waitForEvent('download'); await page.getByRole('link', { name: 'Download PNG' }).click();
  expect(await readFile((await (await pending).path())!)).toEqual(bytes);
});
