import { test, expect, type Page } from '@playwright/test';

const headers = {
  'access-control-allow-origin': '*',
  'access-control-allow-methods': 'GET, POST, OPTIONS',
  'access-control-allow-headers': 'content-type'
};
const parameterError = 'Numeric parameters must be finite and within the documented iteration/order/error limits.';
const rombergError = 'Romberg integration accepts at most 30 steps.';

async function expression(page: Page) {
  await page.locator('#expressionTextArea textarea').fill('x');
}

test.beforeEach(async ({ page }) => {
  await page.route('**/api/**', route => {
    const request = route.request();
    const path = new URL(request.url()).pathname;
    if (request.method() === 'OPTIONS') return route.fulfill({ headers, status: 204 });
    if (path.includes('/autocomplete/')) return route.fulfill({ headers, json: [] });
    if (path.endsWith('/list-methods')) return route.fulfill({ headers, json: ['Method A', "Romberg's method"] });

    // Match the API's defaults and validation of all supplied numeric fields.
    // An irrelevant invalid field would make the real API reject this request.
    const parameters = { n: 10000, order: 1, epsilon: 1e-6, ...request.postDataJSON() };
    const valid = Number.isInteger(parameters.n) && parameters.n >= 1 && parameters.n <= 1000000
      && Number.isInteger(parameters.order) && parameters.order >= 1 && parameters.order <= 10
      && Number.isFinite(parameters.epsilon) && parameters.epsilon > 0 && parameters.epsilon <= 1;
    const errors = !valid ? { parameters: [parameterError] }
      : parameters.method === "Romberg's method" && parameters.n > 30 ? { n: [rombergError] } : null;
    return errors ? route.fulfill({ headers, status: 400, contentType: 'application/problem+json', body: JSON.stringify({
      title: 'One or more validation errors occurred.', status: 400, errors
    }) }) : route.fulfill({ headers, json: { result: '42' } });
  });
});

for (const scenario of [
  { initial: 'Derivative', field: 'order', next: 'Function root', operation: 'function-root', parameters: { a: -1, b: 1, n: 100000, epsilon: 1e-6 } },
  { initial: 'Function root', field: 'ε', next: 'Integral', operation: 'integral', parameters: { a: -1, b: 1, n: 100000 } },
  { initial: 'Integral', field: 'N', next: 'Derivative', operation: 'derivative', parameters: { x: 0, order: 1, epsilon: 1e-6 } }
]) {
  test(`invalid ${scenario.field} does not block ${scenario.next} while its control is hidden`, async ({ page }) => {
    await page.goto('/numerical-calculations'); await expression(page);
    await page.getByLabel('Operation:', { exact: true }).selectOption(scenario.initial);
    await page.getByRole('spinbutton', { name: scenario.field, exact: true }).fill('0');
    await page.getByLabel('Operation:', { exact: true }).selectOption(scenario.next);
    const sent = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute'));
    await page.getByRole('button', { name: 'Compute!' }).click();
    expect((await sent).postDataJSON()).toEqual({
      expression: 'x', method: 'Method A', operation: scenario.operation, customFunctions: '', ...scenario.parameters
    });
    await expect(page.getByRole('status')).toHaveText('42');
    await expect(page.getByRole('alert')).toHaveCount(0);

    // Switching operations should not discard the user's other input values.
    await page.getByLabel('Operation:', { exact: true }).selectOption(scenario.initial);
    await expect(page.getByRole('spinbutton', { name: scenario.field, exact: true })).toHaveValue('0');
  });
}

test('numerical validation explains the rejected field and permits a corrected retry', async ({ page }) => {
  await page.goto('/numerical-calculations'); await expression(page);
  await page.getByLabel('Method:', { exact: true }).selectOption("Romberg's method");
  await page.getByRole('button', { name: 'Compute!' }).click();
  await expect(page.getByRole('alert')).toHaveText(rombergError);
  await expect(page.getByRole('button', { name: 'Compute!' })).toBeEnabled();
  await expect(page.getByRole('status')).toBeEmpty();
  await page.getByRole('spinbutton', { name: 'N', exact: true }).fill('6');
  await page.getByRole('button', { name: 'Compute!' }).click();
  await expect(page.getByRole('status')).toHaveText('42');
  await expect(page.getByRole('alert')).toHaveCount(0);
});

test('execution displays all server field messages and ignores malformed message values', async ({ page }) => {
  await page.route('**/api/v2/execute', route => route.fulfill({ headers, status: 400, json: {
    title: 'One or more validation errors occurred.',
    errors: {
      expression: ['Expression is too long.', '', null, 12],
      customFunctions: ['Definitions are too long.'],
      invalid: { message: 'This is not a field-message array.' }
    }
  } }));
  await page.goto('/scripting'); await page.locator('#scriptTextArea textarea').fill('write(1)');
  await page.getByRole('button', { name: 'Process script' }).click();
  await expect(page.getByRole('alert')).toHaveText('Expression is too long. Definitions are too long.');
  await expect(page.getByRole('button', { name: 'Process script' })).toBeEnabled();
});

for (const response of [
  { name: 'HTML proxy error', status: 502, contentType: 'text/html', body: '<html><body>Bad gateway</body></html>' },
  { name: 'empty error', status: 503, contentType: 'application/json', body: '' },
  { name: 'malformed JSON error', status: 500, contentType: 'application/problem+json', body: '{"title":' },
  { name: 'null error body', status: 502, contentType: 'application/json', body: 'null' }
]) {
  test(`${response.name} keeps its HTTP status instead of a JSON parser exception`, async ({ page }) => {
    await page.route('**/api/v2/execute', route => route.fulfill({ headers,
      status: response.status, contentType: response.contentType, body: response.body }));
    await page.goto('/calculate'); await expression(page);
    await page.getByRole('button', { name: 'Calculate!' }).click();
    await expect(page.getByRole('alert')).toHaveText(`Request failed (${response.status}).`);
    await expect(page.getByRole('status')).toBeEmpty();
    await expect(page.getByRole('button', { name: 'Calculate!' })).toBeEnabled();
  });
}

test('malformed successful JSON is an invalid response rather than a parser exception', async ({ page }) => {
  await page.route('**/api/v2/execute', route => route.fulfill({ headers, contentType: 'application/json', body: '{"result":' }));
  await page.goto('/calculate'); await expression(page);
  await page.getByRole('button', { name: 'Calculate!' }).click();
  await expect(page.getByRole('alert')).toHaveText('Invalid execution response.');
  await expect(page.getByRole('status')).toBeEmpty();
});
