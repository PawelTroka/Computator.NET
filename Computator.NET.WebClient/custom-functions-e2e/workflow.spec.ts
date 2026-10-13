import { test, expect, type Page } from '@playwright/test';
const definition = (value: number) => `static real CustomProbe(real x) { return x + ${value}; }`;
async function calculate(page: Page, expected: string) {
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await page.locator('#expressionTextArea textarea').fill('CustomProbe(0)/1');
  await page.getByRole('button', { name: 'Calculate!' }).click();
  await expect(page.getByRole('status')).toHaveText(expected);
}
test('real functions flow from unsaved editor through calculation, script and reload', async ({ page }) => {
  await page.goto('/custom-functions');
  await page.locator('#customFunctionsTextArea textarea').fill(definition(42));
  await calculate(page, '42');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.locator('#customFunctionsTextArea textarea').fill(definition(73));
  await calculate(page, '73');
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await page.locator('#scriptTextArea textarea').fill('write(CustomProbe(0)/1);');
  await page.getByRole('button', { name: 'Process script' }).click();
  await expect(page.getByRole('status')).toHaveText('73');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByRole('button', { name: 'Save in this tab' }).click();
  await page.reload(); await calculate(page, '73');
});
test('UTF-8 ts lf import, export and real chart retain custom definitions', async ({ page }) => {
  await page.goto('/custom-functions');
  await page.getByLabel('Import .tslf').setInputFiles({ name: 'probe.tslf', mimeType: 'text/plain', buffer: Buffer.from('// zażółć\n' + definition(4)) });
  await expect(page.locator('#customFunctionsTextArea .ace_content')).toContainText('CustomProbe');
  const download = page.waitForEvent('download'); await page.getByRole('button', { name: 'Export .tslf' }).click();
  expect((await download).suggestedFilename()).toBe('custom-functions.tslf');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await page.locator('#expressionTextArea textarea').fill('CustomProbe(x)');
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
    await a.locator('#customFunctionsTextArea textarea').fill(definition(11));
    await b.locator('#customFunctionsTextArea textarea').fill(definition(99));
    await Promise.all([calculate(a, '11'), calculate(b, '99')]);
  } finally { await first.close(); await second.close(); }
});
