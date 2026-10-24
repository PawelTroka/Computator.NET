import { test, expect, type Locator, type Page } from '@playwright/test';

const headers = { 'access-control-allow-origin': '*',
  'access-control-allow-methods': 'GET, POST, OPTIONS', 'access-control-allow-headers': 'content-type' };
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFklEQVR4nGP8//8/AwMDEwMDAwMDAwAkBgMB/DXemwAAAABJRU5ErkJggg==', 'base64');

async function typeNumber(input: Locator, text: string) {
  await input.fill('');
  // Real keystrokes matter: fill() supplies the final value atomically and misses
  // the valid intermediate -0 and 2e1 values that were being reformatted.
  await input.pressSequentially(text, { delay: 25 });
  await expect(input).toHaveValue(text);
}
async function expression(page: Page) {
  await page.locator('#expressionTextArea textarea').fill('x');
}

test.beforeEach(async ({ page }) => {
  await page.route('**/api/**', route => {
    if (route.request().method() === 'OPTIONS') return route.fulfill({ headers, status: 204 });
    const path = new URL(route.request().url()).pathname;
    if (path.includes('/autocomplete/')) return route.fulfill({ headers, json: [] });
    if (path.endsWith('/list-methods')) return route.fulfill({ headers, json: ['Method A'] });
    if (path.endsWith('/v2/chart')) return route.fulfill({ headers, contentType: 'image/png', body: png });
    return route.fulfill({ headers, json: { result: '42' } });
  });
});

for (const value of ['-0.25', '-0.005', '2e10', '2e-12', '1.20e-10', '-3.75e12']) {
  test(`calculator keyboard entry preserves ${value}`, async ({ page }) => {
    await page.goto('/calculate'); await expression(page);
    const input = page.getByRole('spinbutton', { name: 'x', exact: true });
    await typeNumber(input, value);
    const sent = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute'));
    await page.getByRole('button', { name: 'Calculate!' }).click();
    expect((await sent).postDataJSON()).toMatchObject({ operation: 'calculate', expression: 'x', x: Number(value) });
    // The existing blur normalization may change notation, but not its value.
    expect(Number(await input.inputValue())).toBe(Number(value));
    await expect(page.getByRole('status')).toHaveText('42');
  });
}

test('chart uses the negative fractional minimum and scientific maximum entered on the keyboard', async ({ page }) => {
  await page.goto('/'); await expression(page);
  await typeNumber(page.getByRole('spinbutton', { name: 'XMIN', exact: true }), '-0.25');
  await typeNumber(page.getByRole('spinbutton', { name: 'XMAX', exact: true }), '2e10');
  const sent = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/chart'));
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  expect((await sent).postDataJSON()).toMatchObject({ expression: 'x', xMin: -0.25, xMax: 2e10, yMin: -5, yMax: 5 });
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
});

test('derivative uses the entered negative point and small scientific tolerance', async ({ page }) => {
  await page.goto('/numerical-calculations'); await expression(page);
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative');
  await typeNumber(page.getByRole('spinbutton', { name: 'x', exact: true }), '-0.25');
  await typeNumber(page.getByRole('spinbutton', { name: 'ε', exact: true }), '2e-12');
  const sent = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute'));
  await page.getByRole('button', { name: 'Compute!' }).click();
  expect((await sent).postDataJSON()).toMatchObject({ operation: 'derivative', x: -0.25, epsilon: 2e-12, order: 1 });
  await expect(page.getByRole('status')).toHaveText('42');
});

test('number arrows, clearing, blur and navigation keep their existing value behavior', async ({ page }) => {
  await page.goto('/calculate');
  const input = page.getByRole('spinbutton', { name: 'x', exact: true });
  await typeNumber(input, '-0.25');
  await input.press('ArrowUp');
  await expect(input).toHaveValue('0.75');
  await input.fill('');
  await input.blur();
  await expect(input).toHaveValue('0.75');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('spinbutton', { name: 'x', exact: true })).toHaveValue('0.75');
});


test('changing numerical operations synchronizes reused inputs with their new values', async ({ page }) => {
  await page.goto('/numerical-calculations');
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative');
  const point = page.getByRole('spinbutton', { name: 'x', exact: true });
  await typeNumber(point, '10');
  // selectOption dispatches the change without first blurring the focused
  // numeric input. The same component must still accept its new external value.
  await page.getByLabel('Operation:', { exact: true }).selectOption('Integral');
  await expect(page.getByRole('spinbutton', { name: 'a', exact: true })).toHaveValue('-1');
  await expect(page.getByRole('spinbutton', { name: 'b', exact: true })).toHaveValue('1');
  await typeNumber(page.getByRole('spinbutton', { name: 'a', exact: true }), '-0.25');
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative');
  await expect(page.getByRole('spinbutton', { name: 'x', exact: true })).toHaveValue('10');
  await expect(page.getByRole('spinbutton', { name: 'order', exact: true })).toHaveValue('1');
});

test('unfinished signs and exponents recover the last valid value on blur', async ({ page }) => {
  await page.goto('/calculate');
  const input = page.getByRole('spinbutton', { name: 'x', exact: true });
  await input.fill('7.5');
  await input.fill('');
  await input.pressSequentially('-');
  await input.blur();
  await expect(input).toHaveValue('7.5');
  await input.fill('');
  await input.pressSequentially('7e-', { delay: 25 });
  await input.blur();
  await expect(input).toHaveValue('7');
  await input.fill('0');
  await input.blur();
  await expect(input).toHaveValue('0');
});
