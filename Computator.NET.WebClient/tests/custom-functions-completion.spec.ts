import { test, expect, type Page, type Route } from '@playwright/test';
import { readFile } from 'node:fs/promises';

const headers = { 'access-control-allow-origin': '*' };
const catalogue = [{ text: 'sin(x)', menuText: 'real sin(real x)', details: {
  title: 'Sine', category: 'Trigonometry',
  description: '<p class="reference-summary">Returns the sine of an angle in radians.</p>'
} }];
const input = (page: Page) => page.locator('#customFunctionsTextArea textarea');
async function edit(page: Page, text: string) {
  await input(page).focus(); await input(page).press('ControlOrMeta+A');
  await page.keyboard.insertText(text);
}
async function exported(page: Page) {
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Export .tslf', exact: true }).click();
  return readFile((await (await pending).path())!, 'utf8');
}

test('cold completion inserts a function into exact source and the next calculation uses it', async ({ page }, info) => {
  let respond: (() => Promise<void>) | undefined;
  await page.route('**/api/autocomplete/scripting', route => {
    respond = () => route.fulfill({ headers, json: catalogue });
  });
  await page.route('**/api/autocomplete/expression', route => route.fulfill({ headers, json: [] }));
  await page.route('**/api/v2/execute', route => route.fulfill({ headers, json: { result: '0' } }));
  await page.goto('/custom-functions');
  const prefix = '// zażółć λ — keep * and "sin"\nstatic real Probe(real x) { return ';
  await edit(page, prefix + 'si');
  await input(page).press('Control+Space');
  await expect.poll(() => Boolean(respond)).toBe(true);
  await expect(page.locator('.ace_autocomplete')).toBeHidden();
  await respond!();
  await expect(page.locator('.ace_autocomplete')).toContainText('real sin(real x)');
  await expect(page.getByRole('tooltip')).toContainText('Returns the sine of an angle in radians.');
  await page.screenshot({ path: info.outputPath('custom-functions-completion.png') });
  await input(page).press('Tab');
  await page.keyboard.insertText('; }');
  const expected = prefix + 'sin(x); }';
  expect(await exported(page)).toBe(expected);
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Saved state');
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await page.locator('#expressionTextArea textarea').fill('Probe(0)');
  const request = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute'));
  await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
  expect((await request).postDataJSON()).toMatchObject({ operation: 'calculate', expression: 'Probe(0)', customFunctions: expected });
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  expect(await exported(page)).toBe(expected);
});

test('leaving aborts a pending catalogue and returning attaches a fresh completer', async ({ page }) => {
  let first: Route | undefined;
  let requests = 0;
  await page.route('**/api/autocomplete/scripting', route => {
    requests++;
    if (requests === 1) { first = route; return; }
    return route.fulfill({ headers, json: catalogue });
  });
  await page.route('**/api/autocomplete/expression', route => route.fulfill({ headers, json: [] }));
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/custom-functions');
  await edit(page, 'si'); await input(page).press('Control+Space');
  await expect.poll(() => Boolean(first)).toBe(true);
  const failed = page.waitForEvent('requestfailed', request => request.url().endsWith('/autocomplete/scripting'));
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await failed;
  await expect(page.locator('#customFunctionsTextArea')).toHaveCount(0);
  await first!.fulfill({ headers, json: [{ ...catalogue[0], text: 'stale(x)', menuText: 'stale(x)' }] });
  await expect(page.locator('.ace_autocomplete')).toBeHidden();
  await expect(page.getByRole('tooltip')).toHaveCount(0);
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await input(page).focus(); await input(page).press('Control+End'); await input(page).press('Control+Space');
  await expect(page.locator('.ace_autocomplete')).toContainText('real sin(real x)');
  await expect(page.locator('.ace_autocomplete')).not.toContainText('stale');
  expect(requests).toBe(2);
  expect(await exported(page)).toBe('si');
  expect(errors).toEqual([]);
});

test('unavailable completion keeps definitions editable, saveable and exportable', async ({ page }) => {
  let requests = 0;
  await page.route('**/api/autocomplete/scripting', route => {
    requests++; return route.fulfill({ headers, status: 503, json: { title: 'Unavailable' } });
  });
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/custom-functions');
  await edit(page, 'static real Probe(real x) { return x*x; }');
  await input(page).press('Control+Space');
  await expect.poll(() => requests).toBe(1);
  await expect(page.locator('.ace_autocomplete')).toBeHidden();
  await page.keyboard.insertText('\n// still editable');
  const expected = 'static real Probe(real x) { return x*x; }\n// still editable';
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  expect(await exported(page)).toBe(expected);
  expect(await page.evaluate(() => JSON.parse(sessionStorage.getItem('computator.custom-functions.v1')!).code)).toBe(expected);
  expect(errors).toEqual([]);
});
