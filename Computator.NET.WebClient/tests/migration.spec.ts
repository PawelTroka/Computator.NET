import { test as base, expect, type Page } from '@playwright/test';
const test = base.extend<{ errors: string[] }>({
  errors: [async ({ page }, use) => {
    const errors: string[] = []; page.on('pageerror', error => errors.push(error.message));
    await use(errors); expect(errors, 'No uncaught browser exceptions').toEqual([]);
  }, { auto: true }]
});
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jL1sAAAAASUVORK5CYII=', 'base64');
test.beforeEach(async ({ page }) => {
  await page.route('http://localhost:60996/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    const headers = { 'access-control-allow-origin': '*', 'access-control-allow-methods': 'GET, POST, OPTIONS', 'access-control-allow-headers': 'content-type' };
    if (route.request().method() === 'OPTIONS') return route.fulfill({ headers, status: 204 });
    if (path.includes('/autocomplete/')) return route.fulfill({ headers, json: [{ text: 'sin(x)', menuText: 'sin(x)', details: { title: 'Sine', description: 'Sine function', category: 'Trigonometry' } }] });
    if (path.endsWith('/list-methods')) return route.fulfill({ headers, json: ['Method A', 'Method B'] });
    if (path.endsWith('/v2/chart')) return route.fulfill({ headers, contentType: 'image/png', body: png });
    return route.fulfill({ headers, json: { result: route.request().postDataJSON().operation === 'script' ? 'first line\nsecond line' : '42' } });
  });
});
async function expression(page: Page, value: string) { await page.locator('#expressionTextArea textarea').fill(value); }
function execution(page: Page) { return page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute')); }

test('calculation keeps typed numbers, JSON expressions and direct routes', async ({ page }) => {
  await page.goto('/calculate');
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await expression(page, 'x/y');
  const x = page.getByRole('spinbutton', { name: 'x', exact: true });
  await x.fill(''); await x.pressSequentially('-2.5'); await x.press('Tab'); await expect(x).toHaveValue('-2.5');
  await page.getByRole('spinbutton', { name: 'y', exact: true }).fill('4');
  const request = execution(page); await page.getByRole('button', { name: 'Calculate!' }).click();
  expect((await request).postDataJSON()).toMatchObject({ expression: 'x/y', x: -2.5, y: 4, customFunctions: '' });
  await expect(page.getByRole('status')).toHaveText('42'); await page.reload();
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
});
test('chart resizing preserves a distinct Y minimum', async ({ page }) => {
  await page.goto('/'); await expression(page, 'x');
  await page.getByRole('spinbutton', { name: 'XMIN' }).fill('-2'); await page.getByRole('spinbutton', { name: 'YMIN' }).fill('-7');
  const first = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/chart'));
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  expect((await first).postDataJSON()).toMatchObject({ xMin: -2, yMin: -7, xMax: 5, yMax: 5, expression: 'x' });
  const resized = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/chart'));
  await page.setViewportSize({ width: 1100, height: 850 });
  expect((await resized).postDataJSON()).toMatchObject({ xMin: -2, yMin: -7 });
  await expect(page.getByRole('img', { name: 'x', exact: true })).toBeVisible();
});
test('script submission and multiline output survive the editor migration', async ({ page }) => {
  await page.goto('/scripting'); await page.locator('#scriptTextArea textarea').fill('write(1)\nwrite(2)');
  const request = execution(page); await page.getByRole('button', { name: 'Process script' }).click();
  expect((await request).postDataJSON()).toMatchObject({ operation: 'script', expression: 'write(1)\nwrite(2)' });
  await expect(page.getByRole('status')).toHaveText('first line\nsecond line');
});
test('numerical operations retain notation, method and arguments', async ({ page }) => {
  await page.goto('/numerical-calculations'); await expression(page, 'x*x');
  await expect(page.locator('#expressionTextArea .ace_content')).toContainText('x·x');
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative');
  await expect(page.getByLabel('Method:', { exact: true })).toHaveValue('Method A');
  await page.getByLabel('Method:', { exact: true }).selectOption('Method B');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('3'); await page.getByRole('spinbutton', { name: 'order', exact: true }).fill('2');
  const request = execution(page); await page.getByRole('button', { name: 'Compute!' }).click();
  expect((await request).postDataJSON()).toMatchObject({ operation: 'derivative', method: 'Method B', expression: 'x·x', x: 3, order: 2, epsilon: 0.000001 });
  await expect(page.getByRole('status')).toHaveText('42');
});
test('HTTP failures are presented rather than shown as successful results', async ({ page }) => {
  await page.route('**/v2/execute', route => route.fulfill({ status: 500,
    headers: { 'access-control-allow-origin': '*' }, json: { title: 'Invalid expression' } }));
  await page.goto('/calculate'); await expression(page, 'x'); await page.getByRole('button', { name: 'Calculate!' }).click();
  await expect(page.getByRole('alert')).toHaveText('Invalid expression'); await expect(page.getByRole('button', { name: 'Calculate!' })).toBeEnabled();
});
test('mobile navigation expands, routes and closes without Bootstrap JavaScript', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 }); await page.goto('/');
  const toggle = page.getByRole('button', { name: 'Toggle navigation' }); await expect(toggle).toHaveAttribute('aria-expanded', 'false');
  await toggle.click(); await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Scripting', exact: true })).toBeVisible(); await expect(toggle).toHaveAttribute('aria-expanded', 'false');
});
test('autocomplete is attached to the current editor', async ({ page }) => {
  await page.goto('/calculate'); await expression(page, 'si'); await page.locator('#expressionTextArea textarea').press('Control+Space');
  await expect(page.locator('.ace_autocomplete')).toBeVisible(); await expect(page.locator('.ace_autocomplete')).toContainText('sin(x)');
});
test('host keeps missing API endpoints separate from SPA fallback', async ({ request }) => {
  const page = await request.get('/calculate'); expect(page.status()).toBe(200); expect(await page.text()).toContain('react-app');
  test.skip(!process.env.PLAYWRIGHT_SERVER_COMMAND, 'Requires the published ASP.NET host');
  expect((await request.get('/api/does-not-exist')).status()).toBe(404);
  const weather = await request.get('/api/SampleData/WeatherForecasts'); expect(weather.ok()).toBeTruthy(); expect(Array.isArray(await weather.json())).toBeTruthy();
});
