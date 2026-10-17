import { test, expect, type Dialog, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';

const storageKey = 'computator.custom-functions.v1';
const savedCode = 'static real Probe(real x) { return x + 1; }';
const draftCode = 'static real Probe(real x) { return x + 9; }';

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

async function exported(page: Page) {
  const download = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Export .tslf', exact: true }).click();
  return readFile((await (await download).path())!, 'utf8');
}

// Exercise a real browser reload. When there is an unload warning, choose Stay
// and require that the actual document survives. A missing guard really reloads
// the app, exposing the lost draft instead of merely inspecting event listeners.
async function reloadAndStay(page: Page) {
  let completed!: (warned: boolean) => void;
  let failed!: (error: unknown) => void;
  let warned = false;
  const outcome = new Promise<boolean>((resolve, reject) => { completed = resolve; failed = reject; });
  const loaded = () => completed(false);
  const dismiss = (dialog: Dialog) => {
    if (dialog.type() !== 'beforeunload') { failed(new Error(`Unexpected ${dialog.type()} dialog`)); return; }
    warned = true;
    void dialog.dismiss().then(() => completed(true), failed);
  };
  page.on('dialog', dismiss); page.on('load', loaded);
  try {
    // A cancelled reload deliberately never reaches Playwright's navigation
    // milestone. Observe the real load or dismissed warning instead.
    void page.reload().catch(error => { if (!warned) failed(error); });
    return await outcome;
  } finally { page.off('dialog', dismiss); page.off('load', loaded); }
}

test('unsaved definitions survive a cancelled reload after navigating to another tool', async ({ page }) => {
  await edit(page, draftCode);
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  const warned = await reloadAndStay(page);
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  expect(await exported(page), 'The unsaved definitions must still be available after choosing Stay').toBe(draftCode);
  expect(warned, 'Reload must offer a chance to keep the unsaved definitions').toBe(true);
  expect(await page.evaluate(key => sessionStorage.getItem(key), storageKey)).toBeNull();
});

test('saving and importing identical saved code permit reload without a warning', async ({ page }) => {
  await edit(page, savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Saved state');
  expect(await reloadAndStay(page)).toBe(false);
  expect(await exported(page)).toBe(savedCode);
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({
    name: 'identical.tslf', mimeType: 'text/plain', buffer: Buffer.from(savedCode)
  });
  await expect(page.getByRole('status')).toHaveText('Saved state');
  expect(await reloadAndStay(page)).toBe(false);
  expect(await exported(page)).toBe(savedCode);
});

test('imported changes warn until successfully saved', async ({ page }) => {
  await edit(page, savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  const confirmation = page.waitForEvent('dialog');
  page.once('dialog', dialog => dialog.accept());
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({
    name: 'edited.tslf', mimeType: 'text/plain', buffer: Buffer.from(draftCode)
  });
  const dialog = await confirmation;
  expect(dialog.type()).toBe('confirm');
  expect(dialog.message()).toContain('Replace the current custom functions?');
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  const warned = await reloadAndStay(page);
  expect(await exported(page)).toBe(draftCode);
  expect(warned).toBe(true);
  expect(await page.evaluate(key => JSON.parse(sessionStorage.getItem(key)!).code, storageKey)).toBe(savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Saved state');
  expect(await reloadAndStay(page)).toBe(false);
  expect(await exported(page)).toBe(draftCode);
});

test('a failed save keeps the warning active until saving succeeds', async ({ page }) => {
  await edit(page, savedCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await page.evaluate(key => {
    const setItem = Storage.prototype.setItem;
    Storage.prototype.setItem = function (name, value) {
      if (name === key) throw new DOMException('Storage is full', 'QuotaExceededError');
      return setItem.call(this, name, value);
    };
    (window as unknown as { restoreDefinitionStorage: () => void }).restoreDefinitionStorage = () => { Storage.prototype.setItem = setItem; };
  }, storageKey);
  await edit(page, draftCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('could not save');
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  const warned = await reloadAndStay(page);
  expect(await exported(page)).toBe(draftCode);
  expect(warned).toBe(true);
  expect(await page.evaluate(key => JSON.parse(sessionStorage.getItem(key)!).code, storageKey)).toBe(savedCode);
  await page.evaluate(() => (window as unknown as { restoreDefinitionStorage: () => void }).restoreDefinitionStorage());
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveCount(0);
  await expect(page.getByRole('status')).toHaveText('Saved state');
  expect(await reloadAndStay(page)).toBe(false);
  expect(await exported(page)).toBe(draftCode);
});

test('unreadable stored definitions stay preserved while the in-memory draft is protected', async ({ page }) => {
  const unreadable = '{"schemaVersion":99,"code":"original"}';
  await page.evaluate(({ key, value }) => sessionStorage.setItem(key, value), { key: storageKey, value: unreadable });
  await page.reload();
  await expect(page.getByRole('alert')).toContainText('could not be read');
  await edit(page, draftCode);
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('Clear explicitly');
  const warned = await reloadAndStay(page);
  expect(await exported(page)).toBe(draftCode);
  expect(warned).toBe(true);
  expect(await page.evaluate(key => sessionStorage.getItem(key), storageKey)).toBe(unreadable);
});

test('explicitly clearing definitions removes the unload warning', async ({ page }) => {
  await edit(page, draftCode);
  page.once('dialog', dialog => dialog.accept());
  await page.getByRole('button', { name: 'Clear definitions', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Saved state');
  expect(await reloadAndStay(page)).toBe(false);
  expect(await exported(page)).toBe('');
  expect(await page.evaluate(key => sessionStorage.getItem(key), storageKey)).toBeNull();
});
