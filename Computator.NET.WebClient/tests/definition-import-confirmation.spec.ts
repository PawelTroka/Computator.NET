import { test, expect, type Dialog, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';

const storageKey = 'computator.custom-functions.v1';
const savedCode = 'static real Probe(real x) { return x + 1; }';
const draftCode = 'static real Probe(real x) { return x + 9; }';
const importedCode = '// zażółć λ\nstatic real Probe(real x) { return x + 42; }';

test.beforeEach(async ({ page }) => {
  await page.route('**/api/autocomplete/**', route => route.fulfill({
    headers: { 'access-control-allow-origin': '*' }, json: []
  }));
  await page.goto('/custom-functions');
});

async function edit(page: Page, code: string) {
  const input = page.locator('#customFunctionsTextArea textarea');
  await input.focus(); await input.press('ControlOrMeta+A');
  await page.keyboard.insertText(code);
  await expect(page.locator('#customFunctionsTextArea')).toContainText(code);
}

async function importCode(page: Page, code: string) {
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({
    name: 'replacement.TSLF', mimeType: 'text/plain', buffer: Buffer.from(code)
  });
  await expect(page.getByRole('button', { name: 'Cancel import', exact: true })).toHaveCount(0);
}

async function exported(page: Page) {
  const download = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Export .tslf', exact: true }).click();
  return readFile((await (await download).path())!, 'utf8');
}

function handleConfirmations(page: Page, accept: boolean) {
  const messages: string[] = [];
  page.on('dialog', async (dialog: Dialog) => {
    expect(dialog.type()).toBe('confirm');
    messages.push(dialog.message());
    if (accept) await dialog.accept(); else await dialog.dismiss();
  });
  return messages;
}

test('declining replacement preserves unsaved edits and their previous saved copy', async ({ page }) => {
  await edit(page, savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await edit(page, draftCode);
  const stored = await page.evaluate(key => sessionStorage.getItem(key), storageKey);
  const messages = handleConfirmations(page, false);

  await importCode(page, importedCode);

  expect(await exported(page), 'Declining import must keep the actual editor document').toBe(draftCode);
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  expect(await page.evaluate(key => sessionStorage.getItem(key), storageKey)).toBe(stored);
  expect(messages).toHaveLength(1);
  expect(messages[0]).toContain('Replace the current custom functions?');
  await expect(page.getByRole('alert')).toHaveCount(0);

  // The input is reset after each selection, so the same file can be retried.
  await importCode(page, importedCode);
  expect(await exported(page)).toBe(draftCode);
  expect(messages).toHaveLength(2);
});

test('accepting replacement imports exact UTF-8 source without overwriting the saved copy', async ({ page }) => {
  await edit(page, savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await edit(page, draftCode);
  const stored = await page.evaluate(key => sessionStorage.getItem(key), storageKey);
  const messages = handleConfirmations(page, true);

  await importCode(page, '\uFEFF' + importedCode);

  expect(await exported(page)).toBe(importedCode);
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  expect(await page.evaluate(key => sessionStorage.getItem(key), storageKey)).toBe(stored);
  expect(messages).toHaveLength(1);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Saved state');
  await page.reload();
  expect(await exported(page)).toBe(importedCode);
});

test('an empty file also requires confirmation before replacing saved definitions', async ({ page }) => {
  await edit(page, savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  const stored = await page.evaluate(key => sessionStorage.getItem(key), storageKey);
  const messages = handleConfirmations(page, false);

  await importCode(page, '');

  expect(await exported(page)).toBe(savedCode);
  await expect(page.getByRole('status')).toHaveText('Saved state');
  expect(await page.evaluate(key => sessionStorage.getItem(key), storageKey)).toBe(stored);
  expect(messages).toHaveLength(1);
});

test('empty editors and identical saved or unsaved definitions import without confirmation', async ({ page }) => {
  const messages = handleConfirmations(page, false);
  await importCode(page, savedCode);
  expect(await exported(page)).toBe(savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  const stored = await page.evaluate(key => sessionStorage.getItem(key), storageKey);

  await importCode(page, '\uFEFF' + savedCode);
  expect(await exported(page)).toBe(savedCode);
  await expect(page.getByRole('status')).toHaveText('Saved state');

  await edit(page, draftCode);
  await importCode(page, draftCode);
  expect(await exported(page)).toBe(draftCode);
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  expect(await page.evaluate(key => sessionStorage.getItem(key), storageKey)).toBe(stored);
  expect(messages).toEqual([]);
});
