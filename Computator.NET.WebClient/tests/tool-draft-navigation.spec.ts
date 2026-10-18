import { test, expect, type Page } from '@playwright/test';

const headers = { 'access-control-allow-origin': '*' };
const png = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAFklEQVR4nGP8//8/AwMDEwMDAwMDAwAkBgMB/DXemwAAAABJRU5ErkJggg==', 'base64');
const definition = 'static real Probe(real x) { return x + 11; }';
type Request = Record<string, unknown>;

test.beforeEach(async ({ context }) => {
  await context.route('**/api/autocomplete/**', route => route.fulfill({ json: [], headers }));
  await context.route('**/api/numerical-calculations/**/list-methods', route => route.fulfill({ json: ['Method A', 'Method B'], headers }));
});

async function edit(page: Page, id: string, code: string) {
  const input = page.locator(`#${id} textarea`);
  await input.focus(); await input.press('ControlOrMeta+A');
  if (code) await page.keyboard.insertText(code); else await input.press('Backspace');
}

async function number(page: Page, name: string, value: number) {
  await page.getByRole('spinbutton', { name, exact: true }).fill(String(value));
}

async function values(page: Page, expected: Record<string, number>) {
  for (const [name, value] of Object.entries(expected)) {
    await expect(page.getByRole('spinbutton', { name, exact: true })).toHaveValue(String(value));
  }
}

async function formula(page: Page, code: string) {
  await expect(page.locator('#expressionTextArea .ace_content')).toHaveText(code);
}

async function visit(page: Page, name: string) {
  await page.getByRole('link', { name, exact: true }).click();
  await expect(page.getByRole('heading', { name, exact: true })).toBeVisible();
}

test('chart retains the formula and every bound through Custom functions and redraws only when asked', async ({ page }) => {
  const requests: Request[] = [];
  await page.route('**/api/v2/chart', route => {
    requests.push(route.request().postDataJSON());
    return route.fulfill({ body: png, contentType: 'image/png', headers });
  });
  await page.goto('/');
  await edit(page, 'expressionTextArea', 'Probe(x) + x²');
  const bounds = { XMIN: -12.5, XMAX: 6.5, YMIN: -8, YMAX: 17 };
  for (const [name, value] of Object.entries(bounds)) await number(page, name, value);
  await visit(page, 'Custom functions');
  await edit(page, 'customFunctionsTextArea', definition);
  await visit(page, 'Chart');
  await formula(page, 'Probe(x) + x²'); await values(page, bounds);
  await expect(page.getByRole('status')).toBeEmpty();
  expect(requests).toEqual([]);
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  expect(requests).toHaveLength(1);
  expect(requests[0]).toMatchObject({ expression: 'Probe(x) + x²', customFunctions: definition,
    xMin: -12.5, xMax: 6.5, yMin: -8, yMax: 17 });

  await visit(page, 'Custom functions');
  const updated = definition.replace('+ 11', '+ 12');
  await edit(page, 'customFunctionsTextArea', updated);
  await visit(page, 'Chart');
  await formula(page, 'Probe(x) + x²'); await values(page, bounds);
  await expect(page.getByRole('status')).toBeEmpty();
  await expect(page.locator('.chart-image')).toHaveCount(0);
  await expect(page.getByRole('link', { name: 'Download PNG' })).toHaveCount(0);
  expect(requests).toHaveLength(1);
  await page.getByRole('button', { name: 'Draw chart!' }).click();
  await expect(page.getByRole('status')).toHaveText('Chart ready.');
  expect(requests[1]).toMatchObject({ expression: 'Probe(x) + x²', customFunctions: updated });
});

test('numerical navigation retains the operation, selected method and inputs including fields hidden by the operation', async ({ page }) => {
  const requests: Request[] = [];
  await page.route('**/api/v2/execute', route => {
    requests.push(route.request().postDataJSON());
    return route.fulfill({ json: { result: '42' }, headers });
  });
  await page.goto('/numerical-calculations');
  await edit(page, 'expressionTextArea', 'Probe(x) - 4');
  await number(page, 'a', -2.5); await number(page, 'b', 3.75); await number(page, 'N', 1200);
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative');
  await number(page, 'x', -0.625); await number(page, 'order', 3); await number(page, 'ε', 0.0002);
  await page.getByLabel('Operation:', { exact: true }).selectOption('Function root');
  await page.getByLabel('Method:', { exact: true }).selectOption('Method B');
  await visit(page, 'Custom functions');
  await edit(page, 'customFunctionsTextArea', definition);
  await visit(page, 'Numerical calculations');
  await formula(page, 'Probe(x) - 4');
  await expect(page.getByLabel('Operation:', { exact: true })).toHaveValue('Function root');
  await expect(page.getByLabel('Method:', { exact: true })).toHaveValue('Method B');
  await values(page, { a: -2.5, b: 3.75, N: 1200, 'ε': 0.0002 });
  await expect(page.getByRole('status')).toBeEmpty();
  expect(requests).toEqual([]);
  await page.getByRole('button', { name: 'Compute!' }).click();
  await expect(page.getByRole('status')).toHaveText('42');
  expect(requests).toEqual([{ expression: 'Probe(x) - 4', operation: 'function-root', method: 'Method B',
    a: -2.5, b: 3.75, n: 1200, epsilon: 0.0002, customFunctions: definition }]);

  await visit(page, 'Custom functions');
  const updated = definition.replace('+ 11', '+ 12');
  await edit(page, 'customFunctionsTextArea', updated);
  await visit(page, 'Numerical calculations');
  await expect(page.getByRole('status')).toBeEmpty();
  await expect(page.getByLabel('Method:', { exact: true })).toHaveValue('Method B');
  expect(requests).toHaveLength(1);
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative');
  await values(page, { x: -0.625, order: 3, 'ε': 0.0002 });
  await page.getByRole('button', { name: 'Compute!' }).click();
  await expect(page.getByRole('status')).toHaveText('42');
  expect(requests[1]).toEqual({ expression: 'Probe(x) - 4', operation: 'derivative', method: 'Method A',
    x: -0.625, order: 3, epsilon: 0.0002, customFunctions: updated });
  await page.getByLabel('Operation:', { exact: true }).selectOption('Integral');
  await values(page, { a: -2.5, b: 3.75, N: 1200 });
});

test('Back and Forward retain independent Chart, Numerical calculations and Calculate drafts', async ({ page }) => {
  await page.goto('/'); await edit(page, 'expressionTextArea', 'x + 91'); await number(page, 'XMIN', -9);
  await visit(page, 'Numerical calculations');
  await edit(page, 'expressionTextArea', 'x - 23'); await number(page, 'a', -2.3);
  await page.getByLabel('Method:', { exact: true }).selectOption('Method B');
  await page.goBack(); await formula(page, 'x + 91'); await values(page, { XMIN: -9 });
  await edit(page, 'expressionTextArea', 'x + 92');
  await page.goForward(); await formula(page, 'x - 23'); await values(page, { a: -2.3 });
  await expect(page.getByLabel('Method:', { exact: true })).toHaveValue('Method B');
  await visit(page, 'Calculate'); await edit(page, 'expressionTextArea', 'x + 45'); await number(page, 'x', 4.5);
  await visit(page, 'Chart'); await formula(page, 'x + 92'); await values(page, { XMIN: -9 });
  await visit(page, 'Numerical calculations'); await formula(page, 'x - 23'); await values(page, { a: -2.3 });
  await visit(page, 'Calculate'); await formula(page, 'x + 45'); await values(page, { x: 4.5 });
});

for (const tool of [
  { name: 'Chart', path: '/', endpoint: 'chart', button: 'Draw chart!', field: 'XMIN', initial: -2, next: -7 },
  { name: 'Numerical calculations', path: '/numerical-calculations', endpoint: 'execute', button: 'Compute!', field: 'a', initial: -2, next: -7 }
]) {
  test(`${tool.name} cancels on departure and retains later edits without replaying the request or its late result`, async ({ page }) => {
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
    const requests: Request[] = [];
    await page.route(`**/api/v2/${tool.endpoint}`, async route => {
      requests.push(route.request().postDataJSON());
      if (requests.length === 1) await gate;
      await route.fulfill(tool.endpoint === 'chart' ? { body: png, contentType: 'image/png', headers }
        : { json: { result: '17' }, headers }).catch(() => { /* Fetch was cancelled on departure. */ });
    });
    try {
      await page.goto(tool.path); await edit(page, 'expressionTextArea', 'x + 1'); await number(page, tool.field, tool.initial);
      const sent = page.waitForRequest(`**/api/v2/${tool.endpoint}`);
      await page.getByRole('button', { name: tool.button, exact: true }).click(); await sent;
      await edit(page, 'expressionTextArea', 'x + 7'); await number(page, tool.field, tool.next);
      await visit(page, 'Custom functions');
      await expect.poll(() => page.evaluate(() => (window as unknown as { toolAborts: number }).toolAborts)).toBe(1);
      await visit(page, tool.name); release();
      await formula(page, 'x + 7'); await values(page, { [tool.field]: tool.next });
      await expect(page.getByRole('status')).toBeEmpty();
      await expect(page.getByRole('alert')).toHaveCount(0);
      await expect(page.locator('.chart-image')).toHaveCount(0);
      await page.clock.install(); await page.setViewportSize({ width: 1100, height: 850 }); await page.clock.runFor(500);
      expect(requests).toHaveLength(1);
      await page.getByRole('button', { name: tool.button, exact: true }).click();
      await expect(page.getByRole('status')).toHaveText(tool.endpoint === 'chart' ? 'Chart ready.' : '17');
      expect(requests).toHaveLength(2);
      expect(requests[1]).toMatchObject({ expression: 'x + 7', [tool.endpoint === 'chart' ? 'xMin' : 'a']: tool.next });
    } finally { release(); }
  });
}

test('cleared expressions and reset values stay cleared across navigation', async ({ page }) => {
  await page.goto('/'); await edit(page, 'expressionTextArea', 'x + 1'); await number(page, 'XMIN', -12);
  await edit(page, 'expressionTextArea', ''); await number(page, 'XMIN', -4);
  await visit(page, 'Numerical calculations'); await edit(page, 'expressionTextArea', 'x + 2'); await number(page, 'a', -23);
  await edit(page, 'expressionTextArea', ''); await number(page, 'a', 0);
  await visit(page, 'Chart');
  await expect(page.getByRole('button', { name: 'Draw chart!' })).toBeDisabled(); await values(page, { XMIN: -4 });
  await visit(page, 'Numerical calculations');
  await expect(page.getByRole('button', { name: 'Compute!' })).toBeDisabled(); await values(page, { a: 0 });
});

test('new tabs and page reloads use the existing defaults for both tools', async ({ page, context }) => {
  await page.goto('/'); await edit(page, 'expressionTextArea', 'x + 111'); await number(page, 'XMIN', -11);
  await visit(page, 'Numerical calculations'); await edit(page, 'expressionTextArea', 'x + 222'); await number(page, 'a', -22);
  const second = await context.newPage();
  try {
    await second.goto('/');
    await expect(second.getByRole('button', { name: 'Draw chart!' })).toBeDisabled();
    await values(second, { XMIN: -5, XMAX: 5, YMIN: -5, YMAX: 5 });
    await edit(second, 'expressionTextArea', 'x + 333');
    await visit(second, 'Numerical calculations');
    await expect(second.getByRole('button', { name: 'Compute!' })).toBeDisabled();
    await expect(second.getByLabel('Operation:', { exact: true })).toHaveValue('Integral');
    await values(second, { a: -1, b: 1, N: 100000 });
    await edit(second, 'expressionTextArea', 'x + 444');
    await formula(page, 'x + 222'); await values(page, { a: -22 });
    await page.reload();
    await expect(page.getByRole('button', { name: 'Compute!' })).toBeDisabled(); await values(page, { a: -1, b: 1, N: 100000 });
    await visit(page, 'Chart');
    await expect(page.getByRole('button', { name: 'Draw chart!' })).toBeDisabled(); await values(page, { XMIN: -5 });
    await formula(second, 'x + 444');
    await visit(second, 'Chart'); await formula(second, 'x + 333');
  } finally { await second.close(); }
});

test('returning reloads numerical methods and uses an available method if the saved choice was removed', async ({ page }) => {
  let reload = false;
  await page.route('**/api/numerical-calculations/**/list-methods', route => route.fulfill({ json: reload ? ['Method C'] : ['Method A', 'Method B'], headers }));
  const requests: Request[] = [];
  await page.route('**/api/v2/execute', route => {
    requests.push(route.request().postDataJSON());
    return route.fulfill({ json: { result: '42' }, headers });
  });
  await page.goto('/numerical-calculations'); await edit(page, 'expressionTextArea', 'x');
  await page.getByLabel('Method:', { exact: true }).selectOption('Method B');
  await visit(page, 'Custom functions'); reload = true;
  await visit(page, 'Numerical calculations');
  await formula(page, 'x');
  await expect(page.getByLabel('Method:', { exact: true })).toHaveValue('Method C');
  expect(requests).toEqual([]);
  await page.getByRole('button', { name: 'Compute!' }).click();
  await expect(page.getByRole('status')).toHaveText('42');
  expect(requests[0]).toMatchObject({ expression: 'x', method: 'Method C' });
});
