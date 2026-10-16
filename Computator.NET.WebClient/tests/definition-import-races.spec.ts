import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';
const original = 'static real Probe(real x) { return x+1; }';
const edited = 'static real Probe(real x) { return x+9; }';
type DeferredReads = { pending: Record<string, () => Promise<void>> };

test.beforeEach(async ({ page }) => {
  await page.addInitScript(() => {
    const reads: DeferredReads = { pending: {} };
    (window as unknown as { definitionReads: DeferredReads }).definitionReads = reads;
    const read = File.prototype.arrayBuffer;
    File.prototype.arrayBuffer = function () {
      if (!this.name.startsWith('delayed-')) return read.call(this);
      return new Promise<ArrayBuffer>((resolve, reject) => {
        reads.pending[this.name] = async () => {
          try { resolve(await read.call(this)); } catch (error) { reject(error); }
        };
      });
    };
  });
  await page.goto('/custom-functions');
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({ name: 'original.tslf', mimeType: 'text/plain', buffer: Buffer.from(original) });
  await expect(page.locator('#customFunctionsTextArea')).toContainText('x+1');
});
async function delayed(page: Page, name: string, buffer = Buffer.from(edited)) {
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({ name, mimeType: 'text/plain', buffer });
  await expect.poll(() => page.evaluate(name => Boolean((window as unknown as { definitionReads: DeferredReads }).definitionReads.pending[name]), name)).toBe(true);
}
async function release(page: Page, name: string) {
  // Wait for the controlled real File read, not an arbitrary frame/timer delay.
  await page.evaluate(name => (window as unknown as { definitionReads: DeferredReads }).definitionReads.pending[name](), name);
}
async function exported(page: Page) {
  const pending = page.waitForEvent('download'); await page.getByRole('button', { name: 'Export .tslf', exact: true }).click();
  return readFile((await (await pending).path())!, 'utf8');
}

test('late valid import cannot overwrite more recent editor changes', async ({ page }) => {
  await delayed(page, 'delayed-old.tslf');
  const input = page.locator('#customFunctionsTextArea textarea');
  await input.focus(); await input.press('ControlOrMeta+A'); await page.keyboard.insertText('// newer edit\n' + original);
  await release(page, 'delayed-old.tslf');
  expect(await exported(page)).toBe('// newer edit\n' + original);
  await expect(page.getByRole('button', { name: 'Cancel import' })).toHaveCount(0);
});

test('latest selected file wins when reads finish in reverse order', async ({ page }) => {
  await delayed(page, 'delayed-first.tslf', Buffer.from('// old file\n' + original));
  await delayed(page, 'delayed-second.tslf', Buffer.from('\uFEFF// zażółć λ\n' + edited));
  await release(page, 'delayed-second.tslf');
  await expect(page.locator('#customFunctionsTextArea')).toContainText('zażółć');
  await release(page, 'delayed-first.tslf');
  expect(await exported(page)).toBe('// zażółć λ\n' + edited);
});

test('clearing definitions invalidates an in-flight file without resurrecting them', async ({ page }) => {
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await delayed(page, 'delayed-clear.tslf');
  page.once('dialog', dialog => dialog.accept());
  await page.getByRole('button', { name: 'Clear definitions', exact: true }).click();
  await release(page, 'delayed-clear.tslf');
  expect(await exported(page)).toBe('');
  expect(await page.evaluate(() => sessionStorage.getItem('computator.custom-functions.v1'))).toBeNull();
});

test('leaving the page invalidates import without changing definitions in the provider', async ({ page }) => {
  await delayed(page, 'delayed-navigation.tslf');
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  // React Router can transition after the click resolves. Release the file only
  // once the destination committed, which is the behavior this case exercises.
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await expect(page.locator('#customFunctionsTextArea')).toHaveCount(0);
  await release(page, 'delayed-navigation.tslf');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Custom functions', exact: true })).toBeVisible();
  expect(await exported(page)).toBe(original);
});

test('explicit cancellation preserves the original document and permits the next import', async ({ page }) => {
  await delayed(page, 'delayed-cancel.tslf');
  await page.getByRole('button', { name: 'Cancel import', exact: true }).click();
  await release(page, 'delayed-cancel.tslf'); expect(await exported(page)).toBe(original);
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({ name: 'next.TSLF', mimeType: 'text/plain', buffer: Buffer.from(edited) });
  await expect(page.locator('#customFunctionsTextArea')).toContainText('x+9'); expect(await exported(page)).toBe(edited);
});

test('stale decoding failure does not replace the latest successful state with an error', async ({ page }) => {
  await delayed(page, 'delayed-invalid.tslf', Buffer.from([0xc3, 0x28]));
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({ name: 'valid.tslf', mimeType: 'text/plain', buffer: Buffer.from(edited) });
  await expect(page.locator('#customFunctionsTextArea')).toContainText('x+9');
  await release(page, 'delayed-invalid.tslf');
  expect(await exported(page)).toBe(edited); await expect(page.getByRole('alert')).toHaveCount(0);
});

test('current malformed and oversized imports still fail without replacing the document', async ({ page }) => {
  for (const [name, buffer] of [
    ['invalid.tslf', Buffer.from([0xc3, 0x28])], ['bytes.tslf', Buffer.alloc(262145)],
    ['characters.tslf', Buffer.alloc(65537, 32)], ['wrong.tsl', Buffer.from(edited)]
  ] as const) {
    await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({ name, mimeType: 'text/plain', buffer });
    await expect(page.getByRole('alert')).toBeVisible(); expect(await exported(page)).toBe(original);
    await expect(page.getByRole('button', { name: 'Cancel import' })).toHaveCount(0);
  }
});
