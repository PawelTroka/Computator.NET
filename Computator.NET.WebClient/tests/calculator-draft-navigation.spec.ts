import { test, expect, type Page } from '@playwright/test';

const headers = { 'access-control-allow-origin': '*' };
const formula = 'Probe(7) + x² - y';
const definition = 'static real Probe(real x) { return x + 11; }';
type Execution = { operation: string; expression: string; x: number; y: number; customFunctions: string };

test.beforeEach(async ({ context }) => {
  await context.route('**/api/autocomplete/**', route => route.fulfill({ json: [], headers }));
});

async function edit(page: Page, id: string, code: string) {
  const input = page.locator(`#${id} textarea`);
  await input.focus(); await input.press('ControlOrMeta+A');
  if (code) await page.keyboard.insertText(code); else await input.press('Backspace');
}

async function inputs(page: Page, code: string, x: number, y: number) {
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  if (code) await expect(page.locator('#expressionTextArea')).toContainText(code);
  else await expect(page.getByRole('button', { name: 'Calculate!', exact: true })).toBeDisabled();
  await expect(page.getByRole('spinbutton', { name: 'x', exact: true })).toHaveValue(String(x));
  await expect(page.getByRole('spinbutton', { name: 'y', exact: true })).toHaveValue(String(y));
}

test('defining a custom function retains the exact calculator draft and uses current definitions on explicit execution', async ({ page }) => {
  const executions: Execution[] = [];
  await page.route('**/api/v2/execute', route => {
    executions.push(route.request().postDataJSON());
    return route.fulfill({ json: { result: executions.length === 1 ? '27.25' : '28.25' }, headers });
  });
  await page.goto('/calculate');
  await edit(page, 'expressionTextArea', formula);
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('2.5');
  await page.getByRole('spinbutton', { name: 'y', exact: true }).fill('-3');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await edit(page, 'customFunctionsTextArea', definition);
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();

  await inputs(page, formula, 2.5, -3);
  await expect(page.getByRole('status')).toHaveText('');
  expect(executions).toEqual([]);
  await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('27.25');
  expect(executions).toEqual([{ operation: 'calculate', expression: formula, x: 2.5, y: -3, customFunctions: definition }]);

  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  const updatedDefinition = definition.replace('+ 11', '+ 12');
  await edit(page, 'customFunctionsTextArea', updatedDefinition);
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await inputs(page, formula, 2.5, -3);
  await expect(page.getByRole('status')).toHaveText('');
  expect(executions).toHaveLength(1);
  await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('28.25');
  expect(executions[1]).toEqual({ operation: 'calculate', expression: formula, x: 2.5, y: -3, customFunctions: updatedDefinition });
});

test('back and forward navigation retain the latest calculator inputs independently of another expression editor', async ({ page }) => {
  await page.goto('/calculate');
  await edit(page, 'expressionTextArea', 'sin(x) + 2·x');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('-12.5');
  await page.getByRole('spinbutton', { name: 'y', exact: true }).fill('4');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await edit(page, 'expressionTextArea', 'x + 99');
  await page.goBack();
  await inputs(page, 'sin(x) + 2·x', -12.5, 4);

  await edit(page, 'expressionTextArea', 'x - y');
  await page.getByRole('spinbutton', { name: 'y', exact: true }).fill('-0.125');
  await page.goForward();
  await expect(page.getByRole('heading', { name: 'Chart', exact: true })).toBeVisible();
  await page.goBack();
  await inputs(page, 'x - y', -12.5, -0.125);
});

test('leaving aborts pending execution while preserving later edits without replaying the request or result', async ({ page }) => {
  await page.addInitScript(() => {
    const original = window.fetch;
    (window as unknown as { calculatorAborts: number }).calculatorAborts = 0;
    window.fetch = (input, init) => {
      if (String(input).endsWith('/v2/execute')) init?.signal?.addEventListener('abort', () => {
        (window as unknown as { calculatorAborts: number }).calculatorAborts++;
      });
      return original(input, init);
    };
  });
  let release!: () => void;
  const gate = new Promise<void>(resolve => { release = resolve; });
  const executions: Execution[] = [];
  await page.route('**/api/v2/execute', async route => {
    executions.push(route.request().postDataJSON());
    const first = executions.length === 1;
    if (first) await gate;
    await route.fulfill({ json: { result: first ? '3' : '13' }, headers }).catch(() => { /* The first native fetch is aborted on route departure. */ });
  });
  try {
    await page.goto('/calculate');
    await edit(page, 'expressionTextArea', 'x + 1');
    await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('2');
    const sent = page.waitForRequest('**/api/v2/execute');
    await page.getByRole('button', { name: 'Calculate!', exact: true }).click(); await sent;
    await expect(page.getByRole('status')).toHaveText('Calculating…');
    await edit(page, 'expressionTextArea', 'x + 7');
    await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('6');
    await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
    await expect.poll(() => page.evaluate(() => (window as unknown as { calculatorAborts: number }).calculatorAborts)).toBe(1);
    await page.getByRole('link', { name: 'Calculate', exact: true }).click();
    release();
    await inputs(page, 'x + 7', 6, 0);
    await expect(page.getByRole('status')).toHaveText('');
    await expect(page.getByRole('alert')).toHaveCount(0);
    await expect(page.getByRole('button', { name: 'Cancel execution', exact: true })).toHaveCount(0);
    expect(executions).toHaveLength(1);

    await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
    await expect(page.getByRole('status')).toHaveText('13');
    expect(executions).toEqual([
      { operation: 'calculate', expression: 'x + 1', x: 2, y: 0, customFunctions: '' },
      { operation: 'calculate', expression: 'x + 7', x: 6, y: 0, customFunctions: '' }
    ]);
  } finally { release(); }
});

test('explicitly clearing the expression and resetting numbers keeps them clear across navigation', async ({ page }) => {
  await page.goto('/calculate');
  await edit(page, 'expressionTextArea', 'x + 1');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('12');
  await page.getByRole('spinbutton', { name: 'y', exact: true }).fill('-4');
  await edit(page, 'expressionTextArea', '');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('0');
  await page.getByRole('spinbutton', { name: 'y', exact: true }).fill('0');
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await inputs(page, '', 0, 0);
});

test('fresh tabs and page reloads start with defaults and do not share calculator drafts', async ({ page, context }) => {
  await page.goto('/calculate');
  await edit(page, 'expressionTextArea', 'x + 101');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('101');
  const second = await context.newPage();
  try {
    await second.goto('/calculate');
    await inputs(second, '', 0, 0);
    await edit(second, 'expressionTextArea', 'y - 42');
    await second.getByRole('spinbutton', { name: 'y', exact: true }).fill('42');
    await inputs(page, 'x + 101', 101, 0);
    await page.reload();
    await inputs(page, '', 0, 0);
    await inputs(second, 'y - 42', 0, 42);
  } finally { await second.close(); }
});
