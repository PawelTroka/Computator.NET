import { test, expect, type Page } from '@playwright/test';

const headers = { 'access-control-allow-origin': '*' };
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFklEQVR4nGP8//8/AwMDEwMDAwMDAwAkBgMB/DXemwAAAABJRU5ErkJggg==', 'base64');
const definition = 'static real Probe(real x) { return x + 11; }';

async function edit(page: Page, code: string, id = 'expressionTextArea') {
  const input = page.locator(`#${id} textarea`);
  await input.focus(); await input.press('ControlOrMeta+A');
  if (code) await page.keyboard.insertText(code); else await input.press('Backspace');
}
async function number(page: Page, name: string, value: number) {
  await page.getByRole('spinbutton', { name, exact: true }).fill(String(value));
}
async function hasNumber(page: Page, name: string, value: number) {
  await expect(page.getByRole('spinbutton', { name, exact: true })).toHaveValue(String(value));
}
async function visit(page: Page, name: string) {
  await page.getByRole('link', { name, exact: true }).click();
}

test.beforeEach(async ({ context }) => {
  await context.route('**/api/autocomplete/**', route => route.fulfill({ headers, json: [] }));
  await context.route('**/api/numerical-calculations/**/list-methods', route => route.fulfill({ headers, json: ['Method A', 'Method B'] }));
});

test('chart retains its draft through custom-function editing and explicitly draws with current definitions', async ({ page }) => {
  const requests: Record<string, unknown>[] = [];
  await page.route('**/api/v2/chart', route => {
    requests.push(route.request().postDataJSON());
    return route.fulfill({ headers, contentType: 'image/png', body: png });
  });
  await page.goto('/'); await edit(page, 'Probe(x) + x²');
  await number(page, 'XMIN', -12.5); await number(page, 'XMAX', 3.25);
  await number(page, 'YMIN', -7); await number(page, 'YMAX', 21);
  await visit(page, 'Custom functions'); await edit(page, definition, 'customFunctionsTextArea');
  await visit(page, 'Chart');
  await expect(page.locator('#expressionTextArea')).toContainText('Probe(x) + x²');
  await hasNumber(page, 'XMIN', -12.5); await hasNumber(page, 'XMAX', 3.25);
  await hasNumber(page, 'YMIN', -7); await hasNumber(page, 'YMAX', 21);
  await expect(page.getByRole('status')).toBeEmpty();
  expect(requests).toEqual([]);
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  expect(requests).toHaveLength(1);
  expect(requests[0]).toMatchObject({ expression: 'Probe(x) + x²', customFunctions: definition,
    xMin: -12.5, xMax: 3.25, yMin: -7, yMax: 21 });
});

for (const scenario of [
  { name: 'Integral', operation: 'integral', fields: { a: -3.25, b: 8.5, N: 64 }, parameters: { a: -3.25, b: 8.5, n: 64 } },
  { name: 'Derivative', operation: 'derivative', fields: { x: 2.5, order: 3, 'ε': 0.00001 }, parameters: { x: 2.5, order: 3, epsilon: 0.00001 } },
  { name: 'Function root', operation: 'function-root', fields: { a: -4, b: 9, N: 128, 'ε': 0.00002 }, parameters: { a: -4, b: 9, n: 128, epsilon: 0.00002 } }
]) {
  test(`${scenario.name} retains its draft, method and parameters when defining a function`, async ({ page }) => {
    const requests: Record<string, unknown>[] = [];
    await page.route('**/api/v2/execute', route => {
      requests.push(route.request().postDataJSON()); return route.fulfill({ headers, json: { result: '42' } });
    });
    await page.goto('/numerical-calculations'); await edit(page, 'Probe(x) - 2');
    await page.getByLabel('Operation:', { exact: true }).selectOption(scenario.name);
    await page.getByLabel('Method:', { exact: true }).selectOption('Method B');
    for (const [name, value] of Object.entries(scenario.fields)) await number(page, name, value!);
    await visit(page, 'Custom functions'); await edit(page, definition, 'customFunctionsTextArea');
    await visit(page, 'Numerical calculations');
    await expect(page.locator('#expressionTextArea')).toContainText('Probe(x) - 2');
    await expect(page.getByLabel('Operation:', { exact: true })).toHaveValue(scenario.name);
    await expect(page.getByLabel('Method:', { exact: true })).toHaveValue('Method B');
    for (const [name, value] of Object.entries(scenario.fields)) await hasNumber(page, name, value!);
    await expect(page.getByRole('status')).toBeEmpty();
    expect(requests).toEqual([]);
    await page.getByRole('button', { name: 'Compute!' }).click();
    await expect(page.getByRole('status')).toHaveText('42');
    expect(requests).toEqual([{ expression: 'Probe(x) - 2', method: 'Method B', operation: scenario.operation,
      customFunctions: definition, ...scenario.parameters }]);
  });
}

test('chart, numerical and calculator drafts stay independent through Back/Forward and explicit clearing', async ({ page }) => {
  await page.goto('/'); await edit(page, 'x + 101'); await number(page, 'XMIN', -17);
  await visit(page, 'Numerical calculations'); await edit(page, 'x - 202');
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative'); await number(page, 'x', 3);
  await visit(page, 'Calculate'); await edit(page, 'x · 303'); await number(page, 'x', 4);
  await page.goBack();
  await expect(page.locator('#expressionTextArea')).toContainText('x - 202'); await hasNumber(page, 'x', 3);
  await page.goBack();
  await expect(page.locator('#expressionTextArea')).toContainText('x + 101'); await hasNumber(page, 'XMIN', -17);
  await edit(page, ''); await number(page, 'XMIN', -5);
  await page.goForward();
  await expect(page.locator('#expressionTextArea')).toContainText('x - 202');
  await edit(page, ''); await number(page, 'x', 0);
  await page.goForward();
  await expect(page.locator('#expressionTextArea')).toContainText('x · 303'); await hasNumber(page, 'x', 4);
  await visit(page, 'Chart');
  await expect(page.getByRole('button', { name: 'Draw chart!' })).toBeDisabled(); await hasNumber(page, 'XMIN', -5);
  await visit(page, 'Numerical calculations');
  await expect(page.getByRole('button', { name: 'Compute!' })).toBeDisabled(); await hasNumber(page, 'x', 0);
});

for (const tool of [
  { name: 'Chart', path: '/', endpoint: 'chart', button: 'Draw chart!', field: 'XMIN', initial: -5, edited: -12 },
  { name: 'Numerical calculations', path: '/numerical-calculations', endpoint: 'execute', button: 'Compute!', field: 'a', initial: -1, edited: -12 }
]) {
  test(`${tool.name} cancels on departure and keeps later edits without restoring or replaying execution`, async ({ page }) => {
    await page.addInitScript(endpoint => {
      const original = window.fetch;
      (window as unknown as { toolAborts: number }).toolAborts = 0;
      window.fetch = (input, init) => {
        if (String(input).endsWith(`/v2/${endpoint}`)) init?.signal?.addEventListener('abort', () => {
          (window as unknown as { toolAborts: number }).toolAborts++;
        });
        return original(input, init);
      };
    }, tool.endpoint);
    let release!: () => void;
    const gate = new Promise<void>(resolve => { release = resolve; });
    const requests: Record<string, unknown>[] = [];
    await page.route(`**/api/v2/${tool.endpoint}`, async route => {
      requests.push(route.request().postDataJSON());
      if (requests.length === 1) await gate;
      await route.fulfill(tool.endpoint === 'chart' ? { headers, contentType: 'image/png', body: png }
        : { headers, json: { result: '42' } }).catch(() => { /* The first request was aborted on departure. */ });
    });
    try {
      await page.goto(tool.path); await edit(page, 'x + 1');
      const sent = page.waitForRequest(`**/api/v2/${tool.endpoint}`);
      await page.getByRole('button', { name: tool.button, exact: true }).click(); await sent;
      await edit(page, 'x + 7'); await number(page, tool.field, tool.edited);
      await visit(page, 'Custom functions');
      await expect.poll(() => page.evaluate(() => (window as unknown as { toolAborts: number }).toolAborts)).toBe(1);
      await visit(page, tool.name); release();
      await expect(page.locator('#expressionTextArea')).toContainText('x + 7'); await hasNumber(page, tool.field, tool.edited);
      await expect(page.getByRole('status')).toBeEmpty(); await expect(page.getByRole('alert')).toHaveCount(0);
      await expect(page.getByRole('button', { name: /^Cancel/ })).toHaveCount(0);
      await expect(page.locator('.chart-image')).toHaveCount(0);
      // Exercise the delayed resize handler while no plot is submitted.
      // A viewport change can deliver another native resize after the next Draw.
      await page.clock.install(); await page.evaluate(() => window.dispatchEvent(new Event('resize'))); await page.clock.runFor(500);
      expect(requests).toHaveLength(1);
      await page.getByRole('button', { name: tool.button, exact: true }).click();
      await expect(page.getByRole('status')).toHaveText(tool.endpoint === 'chart' ? 'Chart ready.' : '42');
      expect(requests).toHaveLength(2);
      expect(requests[1]).toMatchObject({ expression: 'x + 7', [tool.endpoint === 'chart' ? 'xMin' : 'a']: tool.edited });
    } finally { release(); }
  });

  test(`${tool.name} drafts are independent between tabs and reset on page reload`, async ({ page, context }) => {
    await page.goto(tool.path); await edit(page, 'x + 101'); await number(page, tool.field, tool.edited);
    const second = await context.newPage();
    try {
      await second.goto(tool.path);
      await expect(second.getByRole('button', { name: tool.button, exact: true })).toBeDisabled();
      await hasNumber(second, tool.field, tool.initial);
      await edit(second, 'x - 42'); await number(second, tool.field, -42);
      await expect(page.locator('#expressionTextArea')).toContainText('x + 101'); await hasNumber(page, tool.field, tool.edited);
      await page.reload();
      await expect(page.getByRole('button', { name: tool.button, exact: true })).toBeDisabled();
      await hasNumber(page, tool.field, tool.initial);
      await expect(second.locator('#expressionTextArea')).toContainText('x - 42'); await hasNumber(second, tool.field, -42);
    } finally { await second.close(); }
  });
}

test('numerical methods are refreshed and a removed choice falls back without discarding the other inputs', async ({ page }) => {
  let methods = ['Method A', 'Method B'];
  await page.route('**/api/numerical-calculations/**/list-methods', route => route.fulfill({ headers, json: methods }));
  const requests: Record<string, unknown>[] = [];
  await page.route('**/api/v2/execute', route => {
    requests.push(route.request().postDataJSON()); return route.fulfill({ headers, json: { result: '42' } });
  });
  await page.goto('/numerical-calculations'); await edit(page, 'x - 3'); await number(page, 'a', 2);
  await number(page, 'b', 7);
  await page.getByLabel('Method:', { exact: true }).selectOption('Method B');
  await visit(page, 'Calculate'); methods = ['Replacement method']; await visit(page, 'Numerical calculations');
  await expect(page.getByLabel('Method:', { exact: true })).toHaveValue('Replacement method');
  await expect(page.locator('#expressionTextArea')).toContainText('x - 3'); await hasNumber(page, 'a', 2); await hasNumber(page, 'b', 7);
  expect(requests).toEqual([]);
  await page.getByRole('button', { name: 'Compute!' }).click(); await expect(page.getByRole('status')).toHaveText('42');
  expect(requests).toEqual([{ operation: 'integral', expression: 'x - 3', a: 2, b: 7, n: 100000,
    method: 'Replacement method', customFunctions: '' }]);
});
