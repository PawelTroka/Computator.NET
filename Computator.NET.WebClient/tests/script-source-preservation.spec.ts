import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';

const key = 'computator.script-document.v1';
const source = [
  'write("* and \\\"quoted*\\\"");',
  "write('*');",
  'write(@"C:\\*\\""quoted*""");',
  'write($"* {2*3}");',
  'write($@"C:\\*\\{2*3}");',
  'write("""raw * and "quoted*" text""");',
  'write("""',
  'raw * across lines',
  '""");',
  'write($$"""* {literal} {{2*3}}""");',
  '/* preserve * and both comment delimiters */',
  '// keep this * too',
  'write(2*3); write(2·3);'
].join('\n');

async function stored(page: Page) {
  return page.evaluate(key => JSON.parse(sessionStorage.getItem(key)!).code as string, key);
}

test.beforeEach(async ({ page }) => {
  await page.route('**/api/**', route => {
    const headers = { 'access-control-allow-origin': '*', 'access-control-allow-methods': 'GET, POST, OPTIONS', 'access-control-allow-headers': 'content-type' };
    if (route.request().method() === 'OPTIONS') return route.fulfill({ status: 204, headers });
    if (route.request().url().includes('/autocomplete/')) return route.fulfill({ headers, json: [] });
    return route.fulfill({ headers, json: { result: 'script accepted' } });
  });
});

test('pasted script literals, comments and operators reach the API unchanged', async ({ page }) => {
  await page.goto('/scripting');
  const editor = page.locator('#scriptTextArea textarea');
  await editor.focus(); await page.keyboard.insertText(source);
  await expect.poll(() => stored(page)).toBe(source);
  const pending = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute'));
  await page.getByRole('button', { name: 'Process script', exact: true }).click();
  expect((await pending).postDataJSON()).toMatchObject({ operation: 'script', expression: source });
  await expect(page.getByRole('status')).toHaveText('script accepted');
});

test('typing preserves literal asterisks and block comment delimiters', async ({ page }) => {
  await page.goto('/scripting');
  const editor = page.locator('#scriptTextArea textarea');
  await editor.pressSequentially('write("*"); /* note */ write(2*3);');
  await expect.poll(() => stored(page)).toBe('write("*"); /* note */ write(2*3);');
});

test('unrelated edits preserve imported source through drafts and export', async ({ page }) => {
  await page.goto('/scripting');
  await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({ name: 'literal-probe.tsl', mimeType: 'text/plain', buffer: Buffer.from(source) });
  await expect.poll(() => stored(page)).toBe(source);
  const editor = page.locator('#scriptTextArea textarea');
  await editor.press('ControlOrMeta+End');
  await editor.pressSequentially(' // edited');
  const edited = source + ' // edited';
  await expect.poll(() => stored(page)).toBe(edited);
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await page.reload();
  await expect.poll(() => stored(page)).toBe(edited);
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Export .tsl', exact: true }).click();
  const download = await pending;
  expect(download.suggestedFilename()).toBe('literal-probe.tsl');
  expect(await readFile((await download.path())!, 'utf8')).toBe(edited);
});
