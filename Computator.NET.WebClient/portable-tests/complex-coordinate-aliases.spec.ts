import { test, expect } from '@playwright/test';

test('complex calculations preserve the desktop coordinate aliases through the portable worker', async ({ page }) => {
  await page.goto('/calculate');

  async function calculate(expression: string, x: string, y: string, expected: number[]) {
    const input = page.locator('#expressionTextArea textarea');
    await input.focus();
    await input.press('ControlOrMeta+A');
    await page.keyboard.insertText(expression);
    await page.getByRole('spinbutton', { name: 'x', exact: true }).fill(x);
    await page.getByRole('spinbutton', { name: 'y', exact: true }).fill(y);
    const pending = page.waitForResponse(response => new URL(response.url()).pathname === '/api/v2/execute' &&
      response.request().method() === 'POST', { timeout: 35_000 });
    await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
    const response = await pending;
    expect(response.ok(), await response.text()).toBeTruthy();
    expect(response.request().postDataJSON()).toMatchObject({ operation: 'calculate', x: Number(x), y: Number(y) });
    await expect.poll(async () => (await page.getByRole('status').textContent())?.trim()
      .replace(/[<>()]/g, '').split(/[;,]/).map(Number)).toEqual(expected);
  }

  // No route mocks: the real compiler falls back from a real to a complex result.
  await calculate('x+y*i', '3', '4', [3, 4]);
  await calculate('z+x-y', '-2', '5', [-9, 5]);

  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({
    name: 'coordinates.tslf', mimeType: 'text/plain',
    buffer: Buffer.from('static real Probe(real x) { return x/2; }')
  });
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await calculate('Probe(x)+Probe(y)*i', '-2', '5', [-1, 2.5]);
});
