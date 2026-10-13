import { test, expect } from '@playwright/test';

test('a clean IDE-style launch builds assets and calculates through the real API', async ({ page }) => {
  const failures: string[] = [];
  page.on('pageerror', error => failures.push(error.message));
  const response = await page.goto('/calculate');
  expect(response?.status()).toBe(200);
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await page.locator('#expressionTextArea textarea').fill('2x');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('21');
  await page.getByRole('button', { name: 'Calculate!' }).click();
  await expect(page.getByRole('status')).toHaveText('42');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('9');
  await page.getByRole('button', { name: 'Calculate!' }).click();
  await expect(page.getByRole('status')).toHaveText('18');
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
