import { test, expect } from '@playwright/test';

test('the exact unpacked application computes without exposing a backend token', async ({ page }) => {
  const outgoing: string[] = [];
  page.on('request', request => outgoing.push(request.url()));
  await page.goto('/calculate');
  await page.locator('#expressionTextArea textarea').fill('2x');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('21');
  const request = page.waitForRequest(request => request.url().endsWith('/api/v2/execute'));
  await page.getByRole('button', { name: 'Calculate!' }).click();
  const submitted = await request;
  expect(submitted.headers()['x-computator-session']).toBeUndefined();
  expect(submitted.url()).toBe('http://127.0.0.1:4190/api/v2/execute');
  await expect(page.getByRole('status')).toHaveText('42');
  expect(outgoing.some(url => url.includes(':60999'))).toBe(false);
});

test('packaged proxy accepts JSON custom definitions and rejects unauthorized callers', async ({ request }) => {
  const response = await request.post('/api/v2/execute', { data: {
    operation: 'calculate', expression: 'Probe(x)', x: 6,
    customFunctions: 'static real Probe(real x) { return x/2; }'
  } });
  expect(response.ok(), await response.text()).toBe(true);
  expect(await response.json()).toMatchObject({ result: '3' });
  expect((await request.get('http://127.0.0.1:60999/api/calculate/real/2x/21')).status()).toBe(403);
  expect((await request.post('/api/v2/execute', { headers: { Origin: 'https://untrusted.example' }, data: { expression: '1' } })).status()).toBe(403);
});

test('packaged charts are real PNGs and deep links survive a reload', async ({ page, request }) => {
  const response = await request.post('/api/v2/chart', { data: { expression: 'x', width: 300, height: 300 } });
  expect(response.ok(), await response.text()).toBe(true);
  expect(response.headers()['content-type']).toContain('image/png');
  expect((await response.body()).subarray(0, 8).toString('hex')).toBe('89504e470d0a1a0a');
  await page.goto('/custom-functions');
  await expect(page.getByRole('heading', { name: 'Custom functions', exact: true })).toBeVisible();
  await page.reload();
  await expect(page.getByRole('heading', { name: 'Custom functions', exact: true })).toBeVisible();
});
