import { test, expect, type Page } from '@playwright/test';

async function replaceCode(page: Page, id: string, source: string) {
  const input = page.locator(`#${id} textarea`);
  await input.press('ControlOrMeta+A');
  await page.keyboard.insertText(source);
}

test('literal values and comment boundaries reach the actual mathematical API', async ({ page }) => {
  const definitions = 'public static double Code(string value) { return value[0]; }';
  await page.goto('/custom-functions');
  await replaceCode(page, 'customFunctionsTextArea', definitions);
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();

  for (const [expression, expected] of [
    ['Code("*")', '42'],
    ["'*'", '42'],
    ['Code(@"*") + 2*3', '48'],
    ['2/* * comment */*3', '6'],
    ['2 // * comment\n+3', '5'],
  ]) {
    await replaceCode(page, 'expressionTextArea', expression);
    const pending = page.waitForResponse(response => response.request().method() === 'POST' && response.url().endsWith('/v2/execute'), { timeout: 35_000 });
    await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
    const response = await pending;
    expect(response.request().postDataJSON()).toMatchObject({ expression, customFunctions: definitions });
    expect(response.ok()).toBeTruthy();
    await expect(page.getByRole('status')).toHaveText(expected);
  }
});
