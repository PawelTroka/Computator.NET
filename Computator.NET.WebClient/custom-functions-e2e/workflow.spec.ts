import { test, expect, type Page } from '@playwright/test';
const definition = (value: number) => `static real CustomProbe(real x) { return x + ${value}; }`;
async function replaceCode(page: Page, id: string, value: string) {
  const input = page.locator(`#${id} textarea`);
  await input.focus();
  // Ace's hidden textarea is an input buffer, not the whole document. Select the
  // editor document through its real keyboard command before inserting new text.
  await input.press('ControlOrMeta+A');
  await page.keyboard.insertText(value);
}
async function calculate(page: Page, expected: string) {
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'CustomProbe(0)/1');
  const pending = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute'));
  await page.getByRole('button', { name: 'Calculate!' }).click();
  expect((await pending).postDataJSON().customFunctions).toBe(definition(Number(expected)));
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
  await page.getByRole('button', { name: 'Process script' }).click();
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
  expect((await download).suggestedFilename()).toBe('custom-functions.tslf');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'CustomProbe(x)');
  const response = page.waitForResponse(response => response.url().endsWith('/v2/chart') && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  expect((await response).ok()).toBeTruthy();
  await expect.poll(() => page.getByRole('img', { name: 'CustomProbe(x)', exact: true }).evaluate((image: HTMLImageElement) => image.naturalWidth)).toBeGreaterThan(100);
});
test('independent browser sessions never borrow another user’s definitions', async ({ browser }) => {
  const first = await browser.newContext(), second = await browser.newContext();
  try {
    const a = await first.newPage(), b = await second.newPage();
    await a.goto('http://127.0.0.1:5173/custom-functions'); await b.goto('http://127.0.0.1:5173/custom-functions');
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
  await page.getByRole('button', { name: 'Calculate!' }).click();
  await expect(page.getByRole('alert')).toContainText('missing');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await replaceCode(page, 'customFunctionsTextArea', definition(7));
  await calculate(page, '7');
});
