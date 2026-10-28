import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';
const definition = (value: number) => `static real CustomProbe(real x) { return x + ${value}; }`;
async function replaceCode(page: Page, id: string, value: string) {
  const input = page.locator(`#${id} textarea`);
  await input.focus();
  // Ace's hidden textarea is an input buffer, not the whole document. Select the
  // editor document through its real keyboard command before inserting new text.
  await input.press('ControlOrMeta+A');
  await page.keyboard.insertText(value);
}
function executionResponse(page: Page) {
  // Real worker startup/compilation has its own 30-second deadline. Wait for
  // that response before applying Playwright's short DOM-rendering deadline.
  return page.waitForResponse(response => response.request().method() === 'POST' && response.url().endsWith('/v2/execute'), { timeout: 35_000 });
}
async function calculate(page: Page, expected: string) {
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'CustomProbe(0)/1');
  const pending = executionResponse(page);
  await page.getByRole('button', { name: 'Calculate!' }).click();
  const response = await pending;
  expect(response.request().postDataJSON().customFunctions).toBe(definition(Number(expected)));
  expect(response.ok()).toBeTruthy();
  await expect(page.getByRole('status')).toHaveText(expected);
}
test('real functions flow from unsaved editor through calculation, script and reload', async ({ page }) => {
  await page.goto('/custom-functions');
  await replaceCode(page, 'customFunctionsTextArea', definition(42));
  await calculate(page, '42');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await replaceCode(page, 'customFunctionsTextArea', definition(73));
  await calculate(page, '73');
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await replaceCode(page, 'scriptTextArea', 'write(CustomProbe(0)/1);');
  const script = executionResponse(page);
  await page.getByRole('button', { name: 'Process script' }).click();
  expect((await script).ok()).toBeTruthy();
  await expect(page.getByRole('status')).toHaveText('73');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByRole('button', { name: 'Save in this tab' }).click();
  await page.reload(); await calculate(page, '73');
});
test('UTF-8 import, export and real chart retain custom definitions', async ({ page }) => {
  await page.goto('/custom-functions');
  await page.getByLabel('Import .tslf').setInputFiles({ name: 'probe.tslf', mimeType: 'text/plain', buffer: Buffer.from('// zażółć\n' + definition(4)) });
  await expect(page.locator('#customFunctionsTextArea .ace_content')).toContainText('CustomProbe');
  const download = page.waitForEvent('download'); await page.getByRole('button', { name: 'Export .tslf' }).click();
  const exported = await download;
  expect(exported.suggestedFilename()).toBe('custom-functions.tslf');
  expect(await readFile((await exported.path())!, 'utf8')).toBe('// zażółć\n' + definition(4));
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'CustomProbe(x)');
  const response = page.waitForResponse(response => response.url().endsWith('/v2/chart') && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  const actual = await response;
  expect(actual.ok()).toBeTruthy();
  expect(actual.request().postDataJSON().customFunctions).toBe('// zażółć\n' + definition(4));
  const samples = await actual.json();
  expect(samples.kind).toBe('curve');
  samples.x.forEach((x: number, index: number) => expect(samples.y[index]).toBeCloseTo(x + 4, 10));
  await expect(page.getByTestId('chart-2d')).toBeVisible();
});

test('custom functions reach real numerical integration', async ({ page }) => {
  await page.goto('/custom-functions');
  await replaceCode(page, 'customFunctionsTextArea', definition(4));
  await page.getByRole('link', { name: 'Numerical calculations', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'CustomProbe(x)');
  await expect(page.getByLabel('Method:', { exact: true })).toBeEnabled();
  await page.getByRole('spinbutton', { name: 'a', exact: true }).fill('0');
  await page.getByRole('spinbutton', { name: 'b', exact: true }).fill('2');
  await page.getByRole('spinbutton', { name: 'N', exact: true }).fill('100');
  const integral = executionResponse(page);
  await page.getByRole('button', { name: 'Compute!' }).click();
  expect((await integral).ok()).toBeTruthy();
  await expect.poll(async () => Number(await page.getByRole('status').textContent())).toBeCloseTo(10, 6);
});
test('independent browser sessions never borrow another user’s definitions', async ({ browser, baseURL }) => {
  const first = await browser.newContext({ baseURL }), second = await browser.newContext({ baseURL });
  try {
    const a = await first.newPage(), b = await second.newPage();
    await a.goto('/custom-functions'); await b.goto('/custom-functions');
    await replaceCode(a, 'customFunctionsTextArea', definition(11));
    await replaceCode(b, 'customFunctionsTextArea', definition(99));
    await Promise.all([calculate(a, '11'), calculate(b, '99')]);
  } finally { await first.close(); await second.close(); }
});
test('rejected stored schema is preserved until clear, then repeated saves work', async ({ page }) => {
  await page.goto('/custom-functions');
  await page.evaluate(() => sessionStorage.setItem('computator.custom-functions.v1', '{"schemaVersion":99,"code":"original"}'));
  await page.reload();
  await expect(page.getByRole('alert')).toContainText('could not be read');
  await replaceCode(page, 'customFunctionsTextArea', definition(5));
  await page.getByRole('button', { name: 'Save in this tab' }).click();
  expect(await page.evaluate(() => sessionStorage.getItem('computator.custom-functions.v1'))).toContain('original');
  page.once('dialog', dialog => dialog.accept());
  await page.getByRole('button', { name: 'Clear definitions' }).click();
  for (const value of [8, 9]) {
    await replaceCode(page, 'customFunctionsTextArea', definition(value));
    await page.getByRole('button', { name: 'Save in this tab' }).click();
    await expect(page.getByRole('alert')).toHaveCount(0);
  }
  await page.reload(); await calculate(page, '9');
});
test('invalid custom code yields a real error and a corrected definition recovers', async ({ page }) => {
  await page.goto('/custom-functions');
  await replaceCode(page, 'customFunctionsTextArea', 'static real CustomProbe(real x) { return missing; }');
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'CustomProbe(0)');
  const invalid = executionResponse(page);
  await page.getByRole('button', { name: 'Calculate!' }).click();
  expect((await invalid).ok()).toBeFalsy();
  await expect(page.getByRole('alert')).toContainText('missing');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await replaceCode(page, 'customFunctionsTextArea', definition(7));
  await calculate(page, '7');
});
