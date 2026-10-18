import { test, expect } from '@playwright/test';
import { readFile } from 'node:fs/promises';

test('imported and restored script executes against the actual API with live definitions', async ({ page }) => {
  const code = 'write(Probe(3)); write("zażółć λ");';
  await page.goto('/scripting');
  await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({ name: 'workflow.tsl', mimeType: 'text/plain', buffer: Buffer.from(code) });
  await expect(page.locator('#scriptTextArea')).toContainText('Probe(3)');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({ name: 'probe.tslf', mimeType: 'text/plain', buffer: Buffer.from('static real Probe(real x) { return x+7; }') });
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await page.getByRole('link', { name: 'Scripting', exact: true }).click(); await page.reload();
  const response = page.waitForResponse(response => response.url().endsWith('/v2/execute') && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Process script' }).click();
  const actual = await response; expect(actual.ok()).toBeTruthy(); expect(actual.request().postDataJSON().expression).toBe(code);
  await expect(page.getByRole('status')).toContainText('10'); await expect(page.getByRole('status')).toContainText('zażółć λ');
  const pending = page.waitForEvent('download'); await page.getByRole('button', { name: 'Export .tsl', exact: true }).click();
  expect(await readFile((await (await pending).path())!, 'utf8')).toBe(code);
});

test('edited source preserves literal output and both multiplication notations against the actual API', async ({ page }) => {
  const code = 'write("*|"); write(\'*\'); write("|"); /* * comment */ write(@"*.csv|"); write($"*{2*3}|"); write(3·4);';
  await page.goto('/scripting');
  const input = page.locator('#scriptTextArea textarea');
  await input.focus(); await page.keyboard.insertText(code);
  const response = page.waitForResponse(response => response.url().endsWith('/v2/execute') && response.request().method() === 'POST', { timeout: 35_000 });
  await page.getByRole('button', { name: 'Process script', exact: true }).click();
  const actual = await response;
  expect(actual.request().postDataJSON().expression).toBe(code);
  expect(actual.ok()).toBeTruthy();
  await expect(page.getByRole('status')).toHaveText('*|*|*.csv|*6|12');
});
