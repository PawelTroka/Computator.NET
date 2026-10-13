import { test, expect } from '@playwright/test';

test('a clean IDE-style launch builds assets and calculates through the real API', async ({ page }) => {
  test.setTimeout(90_000);
  const failures: string[] = [];
  page.on('pageerror', error => failures.push(error.message));
  const response = await page.goto('/calculate');
  expect(response?.status()).toBe(200);
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await page.locator('#expressionTextArea textarea').fill('2x');
  for (const [input, result] of [['21', '42'], ['9', '18']]) {
    await page.getByRole('spinbutton', { name: 'x', exact: true }).fill(input);
    // The first actual Roslyn invocation is deliberately cold in this suite.
    // Await and inspect the real HTTP response before checking the rendered value;
    // a pending request must not be confused with a successful calculation.
    const calculation = page.waitForResponse(response => response.url().endsWith('/api/v2/execute') &&
      response.request().method() === 'POST', { timeout: 30_000 });
    await page.getByRole('button', { name: 'Calculate!' }).click();
    const reply = await calculation;
    expect(reply.ok(), await reply.text()).toBe(true);
    expect(await reply.json()).toMatchObject({ result });
    await expect(page.getByRole('status')).toHaveText(result);
  }
  await page.reload();
  await expect(page.locator('#expressionTextArea')).toBeVisible();
  expect(failures).toEqual([]);
});

test('fresh startup serves actual JS/CSS while unknown APIs remain 404', async ({ request, page }) => {
  const requests: string[] = [];
  page.on('response', response => { if (response.status() === 404) requests.push(response.url()); });
  await page.goto('/scripting');
  await expect(page.locator('#scriptTextArea')).toBeVisible();
  expect(requests).toEqual([]);
  expect((await request.get('/api/does-not-exist')).status()).toBe(404);
});
