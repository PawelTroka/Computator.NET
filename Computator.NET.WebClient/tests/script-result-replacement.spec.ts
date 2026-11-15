import { test, expect, type Page } from '@playwright/test';

const headers = { 'access-control-allow-origin': '*',
  'access-control-allow-methods': 'GET, POST, OPTIONS', 'access-control-allow-headers': 'content-type' };
const original = 'write("original");';
const replacement = 'write("replacement");';
type Replacement = 'New script' | 'Import .tsl';
type DeferredRun = { waiting: boolean; aborts: number; release?: () => void; settled?: Promise<void> };

async function replaceDocument(page: Page, action: Replacement, accept = true) {
  page.once('dialog', dialog => accept ? dialog.accept() : dialog.dismiss());
  if (action === 'New script') await page.getByRole('button', { name: action, exact: true }).click();
  else await page.getByLabel(action, { exact: true }).setInputFiles({
    name: 'replacement.tsl', mimeType: 'text/plain', buffer: Buffer.from(replacement)
  });
}

async function deferFirstResult(page: Page) {
  await page.addInitScript(() => {
    const run: DeferredRun = { waiting: false, aborts: 0 };
    (window as unknown as { deferredScriptRun: DeferredRun }).deferredScriptRun = run;
    const fetch = window.fetch;
    let first = true;
    window.fetch = async (input, init) => {
      const response = await fetch(input, init);
      if (String(input).endsWith('/v2/execute') && first) {
        first = false;
        init?.signal?.addEventListener('abort', () => { run.aborts++; });
        const json = response.json.bind(response);
        response.json = () => {
          const decoded = (async () => {
            const body: unknown = await json();
            // Keep a real response's decoded body pending even after transport
            // cancellation. A stale continuation must not publish into a new run.
            await new Promise<void>(resolve => { run.release = resolve; run.waiting = true; });
            return body;
          })();
          run.settled = decoded.then(() => {}, () => {});
          return decoded;
        };
      }
      return response;
    };
  });
}
async function releaseResult(page: Page) {
  await page.evaluate(async () => {
    const run = (window as unknown as { deferredScriptRun: DeferredRun }).deferredScriptRun;
    if (!run.release || !run.settled) throw new Error('No script result is waiting for release.');
    run.release();
    // Observe settlement of the promise returned to the execution hook, then
    // cross a full rendering opportunity before checking unchanged output.
    // Otherwise a negative assertion could pass before stale state is committed.
    await run.settled;
    await new Promise<void>(resolve => requestAnimationFrame(() => requestAnimationFrame(() => resolve())));
  });
}
async function aborts(page: Page) {
  return page.evaluate(() => (window as unknown as { deferredScriptRun: DeferredRun }).deferredScriptRun.aborts);
}
async function start(page: Page) {
  await page.goto('/scripting');
  await page.locator('#scriptTextArea textarea').fill(original);
  await page.getByRole('button', { name: 'Process script', exact: true }).click();
}
async function waitForResult(page: Page) {
  await expect.poll(() => page.evaluate(() =>
    (window as unknown as { deferredScriptRun: DeferredRun }).deferredScriptRun.waiting)).toBe(true);
}

test.beforeEach(async ({ page }) => {
  await page.route('**/api/**', route => {
    if (route.request().method() === 'OPTIONS') return route.fulfill({ headers, status: 204 });
    if (route.request().url().includes('/autocomplete/')) return route.fulfill({ headers, json: [] });
    return route.fulfill({ headers, json: { result: 'original output' } });
  });
});

for (const action of ['New script', 'Import .tsl'] as const) {
  for (const failed of [false, true]) {
    test(`${action} clears completed ${failed ? 'errors' : 'output'} from the previous document`, async ({ page }) => {
      if (failed) await page.route('**/api/v2/execute', route => route.fulfill({
        headers, status: 400, json: { title: 'Original script failed.' }
      }));
      await start(page);
      const output = page.getByRole('region', { name: 'Output', exact: true });
      if (failed) await expect(output.getByRole('alert')).toHaveText('Original script failed.');
      else await expect(output.getByRole('status')).toHaveText('original output');
      await replaceDocument(page, action);
      await expect(output.getByRole('status')).toHaveText('');
      await expect(output.getByRole('alert')).toHaveCount(0);
    });

    test(`${action} aborts the old run and ignores its late ${failed ? 'error' : 'output'} after a replacement run`, async ({ page }) => {
      await deferFirstResult(page);
      const sources: string[] = [];
      await page.route('**/api/v2/execute', route => {
        const expression = route.request().postDataJSON().expression as string;
        sources.push(expression);
        return expression === original && failed
          ? route.fulfill({ headers, status: 400, json: { title: 'Late original error.' } })
          : route.fulfill({ headers, json: { result: expression === original ? 'late original output' : 'replacement output' } });
      });
      await start(page); await waitForResult(page);
      const output = page.getByRole('region', { name: 'Output', exact: true });
      await expect(output.getByRole('status')).toHaveText('Calculating…');
      await replaceDocument(page, action);
      await expect.poll(() => aborts(page)).toBe(1);
      await expect(output.getByRole('status')).toHaveText('');
      await expect(output.getByRole('alert')).toHaveCount(0);
      await expect(output.getByRole('button', { name: 'Cancel execution' })).toHaveCount(0);
      if (action === 'New script') await page.locator('#scriptTextArea textarea').fill(replacement);
      await page.getByRole('button', { name: 'Process script', exact: true }).click();
      await expect(output.getByRole('status')).toHaveText('replacement output');
      await releaseResult(page);
      await expect(output.getByRole('status')).toHaveText('replacement output');
      await expect(output.getByRole('alert')).toHaveCount(0);
      expect(sources).toEqual([original, replacement]);
    });
  }

  test(`declining ${action} preserves the pending execution and its output`, async ({ page }) => {
    await deferFirstResult(page);
    await start(page); await waitForResult(page);
    await replaceDocument(page, action, false);
    expect(await aborts(page)).toBe(0);
    await expect(page.locator('#scriptTextArea')).toContainText('original');
    await expect(page.getByRole('button', { name: 'Cancel execution' })).toBeVisible();
    await releaseResult(page);
    await expect(page.getByRole('region', { name: 'Output', exact: true }).getByRole('status')).toHaveText('original output');
  });
}

test('an import rejected by document-name validation preserves the pending execution', async ({ page }) => {
  await deferFirstResult(page);
  await start(page); await waitForResult(page);
  await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({
    name: 'x'.repeat(181) + '.tsl', mimeType: 'text/plain', buffer: Buffer.from(replacement)
  });
  await expect(page.getByRole('alert')).toContainText('valid file name');
  expect(await aborts(page)).toBe(0);
  await expect(page.locator('#scriptTextArea')).toContainText('original');
  await releaseResult(page);
  await expect(page.getByRole('region', { name: 'Output', exact: true }).getByRole('status')).toHaveText('original output');
});

test('a failed New script storage reset preserves the pending execution', async ({ page }) => {
  await deferFirstResult(page);
  await start(page); await waitForResult(page);
  await page.evaluate(() => {
    Storage.prototype.removeItem = () => { throw new DOMException('Denied', 'SecurityError'); };
  });
  await replaceDocument(page, 'New script');
  await expect(page.getByRole('alert')).toContainText('could not clear');
  expect(await aborts(page)).toBe(0);
  await expect(page.locator('#scriptTextArea')).toContainText('original');
  await releaseResult(page);
  await expect(page.getByRole('region', { name: 'Output', exact: true }).getByRole('status')).toHaveText('original output');
});

test('a successful import held only in memory still resets the previous execution', async ({ page }) => {
  await deferFirstResult(page);
  await page.addInitScript(() => {
    Storage.prototype.setItem = () => { throw new DOMException('Full', 'QuotaExceededError'); };
  });
  await start(page); await waitForResult(page);
  await replaceDocument(page, 'Import .tsl');
  await expect.poll(() => aborts(page)).toBe(1);
  await expect(page.locator('#scriptTextArea')).toContainText('replacement');
  await expect(page.getByRole('alert')).toContainText('remains in memory');
  await expect(page.getByRole('button', { name: 'Process script', exact: true })).toBeEnabled();
  await releaseResult(page);
  await expect(page.getByRole('region', { name: 'Output', exact: true }).getByRole('status')).toHaveText('');
});
