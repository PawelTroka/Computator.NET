import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';
const key = 'computator.script-document.v1';
const source = '// zażółć gęślą — λ\nwrite(2);';
async function replace(page: Page, code: string) {
  // Ace updates its textarea aria-label to describe the cursor after edits.
  // Use the same stable editor input as the existing workflow tests.
  const input = page.locator('#scriptTextArea textarea');
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText(code);
}
async function stored(page: Page) {
  return page.evaluate(key => JSON.parse(sessionStorage.getItem(key)!).code as string, key);
}
test.beforeEach(async ({ page }) => {
  await page.route('**/api/autocomplete/**', route => route.fulfill({ json: [], headers: { 'access-control-allow-origin': '*' } }));
});
test('draft survives route changes and reload without being executed', async ({ page }) => {
  const executions: string[] = [];
  page.on('request', request => { if (request.url().endsWith('/v2/execute')) executions.push(request.url()); });
  await page.goto('/scripting'); await replace(page, source);
  await expect.poll(() => stored(page)).toBe(source);
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await expect(page.locator('#scriptTextArea')).toContainText('zażółć');
  await page.reload(); await expect(page.locator('#scriptTextArea')).toContainText('write(2)');
  expect(await stored(page)).toBe(source); expect(executions).toEqual([]);
});
test('UTF-8 import and export preserve filename and exact document bytes', async ({ page }) => {
  await page.goto('/scripting');
  await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({ name: 'Próba.TSL', mimeType: 'text/plain', buffer: Buffer.from('\uFEFF' + source) });
  await expect.poll(() => stored(page)).toBe(source);
  const pending = page.waitForEvent('download'); await page.getByRole('button', { name: 'Export .tsl', exact: true }).click();
  const file = await pending; expect(file.suggestedFilename()).toBe('Próba.TSL');
  expect(await readFile((await file.path())!, 'utf8')).toBe(source);
});
test('invalid documents leave the current draft intact', async ({ page }) => {
  await page.goto('/scripting'); await replace(page, source);
  for (const [name, buffer] of [
    ['wrong.tslf', Buffer.from('write(99);')], ['invalid.tsl', Buffer.from([0xc3, 0x28])],
    ['large.tsl', Buffer.alloc(262145, 32)], ['long.tsl', Buffer.alloc(65537, 32)]
  ] as const) {
    await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({ name, mimeType: 'text/plain', buffer });
    await expect(page.getByRole('alert')).toBeVisible(); expect(await stored(page)).toBe(source);
  }
});
test('replacement and new-document actions respect explicit cancellation', async ({ page }) => {
  await page.goto('/scripting'); await replace(page, source);
  const declined = page.waitForEvent('dialog');
  page.once('dialog', dialog => dialog.dismiss());
  await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({ name: 'other.tsl', mimeType: 'text/plain', buffer: Buffer.from('write(9);') });
  await declined;
  await expect(page.locator('#scriptTextArea')).toContainText('zażółć');
  page.once('dialog', dialog => dialog.dismiss()); await page.getByRole('button', { name: 'New script', exact: true }).click();
  expect(await stored(page)).toBe(source);
  page.once('dialog', dialog => dialog.accept()); await page.getByRole('button', { name: 'New script', exact: true }).click();
  expect(await page.evaluate(key => sessionStorage.getItem(key), key)).toBeNull();
  await expect(page.getByRole('button', { name: 'Process script' })).toBeDisabled();
});
test('an unreadable stored draft is not silently overwritten', async ({ page }) => {
  await page.goto('/scripting');
  const original = '{"schemaVersion":99,"code":"keep me","name":"old.tsl"}';
  await page.evaluate(({ key, original }) => sessionStorage.setItem(key, original), { key, original });
  await page.reload(); await expect(page.getByRole('alert')).toContainText('has not been overwritten');
  await replace(page, source);
  expect(await page.evaluate(key => sessionStorage.getItem(key), key)).toBe(original);
  page.once('dialog', dialog => dialog.accept()); await page.getByRole('button', { name: 'New script', exact: true }).click();
  await replace(page, source); await expect.poll(() => stored(page)).toBe(source);
  await expect(page.getByRole('alert')).toHaveCount(0);
});
test('denied storage still keeps drafts across client navigation and allows export', async ({ page }) => {
  await page.addInitScript(() => { Storage.prototype.setItem = () => { throw new DOMException('Denied', 'QuotaExceededError'); }; });
  await page.goto('/scripting'); await replace(page, source);
  await expect(page.getByRole('alert')).toContainText('remains in memory');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await expect(page.locator('#scriptTextArea')).toContainText('zażółć');
  const pending = page.waitForEvent('download'); await page.getByRole('button', { name: 'Export .tsl', exact: true }).click();
  expect(await readFile((await (await pending).path())!, 'utf8')).toBe(source);
});
test('independent tabs do not exchange script drafts', async ({ page, context }) => {
  await page.goto('/scripting'); await replace(page, source);
  const other = await context.newPage();
  try {
    await other.goto('/scripting'); await expect(other.getByRole('button', { name: 'Process script' })).toBeDisabled();
    await replace(other, 'write(123);'); expect(await stored(page)).toBe(source);
  } finally { await other.close(); }
});
