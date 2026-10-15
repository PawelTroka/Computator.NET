import { test, expect, type Page } from '@playwright/test';
const key = 'computator.script-document.v1';
const original = 'write("original λ");';
type Reads = { pending: Record<string, () => void>; releaseOnUnmount?: string };

test.beforeEach(async ({ page }) => {
  await page.addInitScript(() => {
    const reads: Reads = { pending: {} };
    (window as unknown as { scriptReads: Reads }).scriptReads = reads;
    const read = File.prototype.arrayBuffer;
    File.prototype.arrayBuffer = function () {
      if (!this.name.startsWith('delayed-')) return read.call(this);
      return new Promise<ArrayBuffer>((resolve, reject) => {
        void read.call(this).then(bytes => { reads.pending[this.name] = () => resolve(bytes); }, reject);
      });
    };
    // Complete the real read as the old editor leaves the DOM, before a later
    // browser action could accidentally flush React's deferred effect cleanup.
    new MutationObserver(() => {
      if (reads.releaseOnUnmount && !document.querySelector('#scriptTextArea')) {
        const name = reads.releaseOnUnmount; reads.releaseOnUnmount = undefined;
        reads.pending[name]();
      }
    }).observe(document, { childList: true, subtree: true });
  });
  await page.route('**/api/autocomplete/**', route => route.fulfill({ json: [], headers: { 'access-control-allow-origin': '*' } }));
  await page.goto('/scripting');
  await page.locator('#scriptTextArea textarea').fill(original);
});
async function delayed(page: Page, name: string, code = 'write("late");') {
  await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({ name, mimeType: 'text/plain', buffer: Buffer.from(code) });
  await expect.poll(() => page.evaluate(name => Boolean((window as unknown as { scriptReads: Reads }).scriptReads.pending[name]), name)).toBe(true);
}
async function release(page: Page, name: string) {
  await page.evaluate(name => (window as unknown as { scriptReads: Reads }).scriptReads.pending[name](), name);
}
async function stored(page: Page) { return page.evaluate(key => JSON.parse(sessionStorage.getItem(key)!).code as string, key); }

test('pending script import cannot overwrite a later edit', async ({ page }) => {
  await delayed(page, 'delayed-edit.tsl');
  await page.locator('#scriptTextArea textarea').fill('write("newer edit");');
  await release(page, 'delayed-edit.tsl');
  expect(await stored(page)).toBe('write("newer edit");');
});

test('newest script import wins over an earlier read', async ({ page }) => {
  await delayed(page, 'delayed-first.tsl');
  await delayed(page, 'delayed-second.tsl', 'write("newest λ");');
  page.once('dialog', dialog => dialog.accept());
  await release(page, 'delayed-second.tsl');
  await expect.poll(() => stored(page)).toBe('write("newest λ");');
  await release(page, 'delayed-first.tsl');
  expect(await stored(page)).toBe('write("newest λ");');
});

test('leaving the script editor invalidates an import at the unmount commit', async ({ page }) => {
  await delayed(page, 'delayed-navigation.tsl');
  await page.evaluate(() => { (window as unknown as { scriptReads: Reads }).scriptReads.releaseOnUnmount = 'delayed-navigation.tsl'; });
  // Accepting any accidental late prompt makes the data-loss regression visible.
  page.on('dialog', dialog => dialog.accept());
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  expect(await stored(page)).toBe(original);
  await expect(page.locator('#scriptTextArea')).toContainText('original λ');
});
