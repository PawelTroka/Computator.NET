import { test, expect, type Page } from '@playwright/test';
import { readFile } from 'node:fs/promises';

// Every calculation below reaches the real published portable service. No route
// mocks, response substitution, Windows backend or test-only evaluator is used.
async function replaceCode(page: Page, id: string, code: string) {
  const input = page.locator(`#${id} textarea`);
  await input.focus();
  await input.press('ControlOrMeta+A');
  await page.keyboard.insertText(code);
}

async function execute(page: Page, button: string, operation: string) {
  const pending = page.waitForResponse(response =>
    new URL(response.url()).pathname === '/api/v2/execute' &&
    response.request().method() === 'POST' && response.request().postDataJSON().operation === operation,
  { timeout: 35_000 });
  await page.getByRole('button', { name: button, exact: true }).click();
  const response = await pending;
  expect(response.ok(), await response.text()).toBeTruthy();
  return response;
}

test('the local application states its scope and calculates with changing inputs', async ({ page, request }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await expect(page.getByTestId('portable-notice')).toContainText('Portable local preview.');
  const status = await request.get('/api/portable/status');
  expect(status.ok()).toBeTruthy();
  const runtime = await status.json();
  expect(runtime).toMatchObject({ implementation: 'portable-local-poc', chart: false, nativeGsl: false });
  if (process.env.COMPUTATOR_EXPECTED_ARCHITECTURE)
    expect(runtime.architecture).toBe(process.env.COMPUTATOR_EXPECTED_ARCHITECTURE);
  await replaceCode(page, 'expressionTextArea', '2x+3');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('7');
  await execute(page, 'Calculate!', 'calculate');
  await expect(page.getByRole('status')).toHaveText('17');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('9');
  await execute(page, 'Calculate!', 'calculate');
  await expect(page.getByRole('status')).toHaveText('21');
  await page.screenshot({ path: test.info().outputPath('portable-calculator.png'), fullPage: true });
});

test('UTF-8 script document survives navigation and reload, executes and exports', async ({ page }) => {
  const source = 'write(3+4); write(" zażółć λ");';
  await page.goto('/scripting');
  await page.getByLabel('Import .tsl', { exact: true }).setInputFiles({
    name: 'portable-λ.tsl', mimeType: 'text/plain', buffer: Buffer.from(source)
  });
  await expect(page.locator('#scriptTextArea')).toContainText('zażółć');
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await page.reload();
  const response = await execute(page, 'Process script', 'script');
  expect(response.request().postDataJSON().expression).toBe(source);
  await expect(page.getByRole('status')).toHaveText('7 zażółć λ');
  await page.screenshot({ path: test.info().outputPath('portable-script.png'), fullPage: true });
  const download = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Export .tsl', exact: true }).click();
  const file = await download;
  expect(file.suggestedFilename()).toBe('portable-λ.tsl');
  expect(await readFile((await file.path())!, 'utf8')).toBe(source);
});

test('unsaved custom functions execute across tools and saved UTF-8 definitions export', async ({ page }) => {
  const definitions = '// zażółć λ\nstatic real PortableProbe(real x) { return x+7; }';
  await page.goto('/custom-functions');
  await page.getByLabel('Import .tslf', { exact: true }).setInputFiles({
    name: 'portable-functions.tslf', mimeType: 'text/plain', buffer: Buffer.from(definitions)
  });
  await expect(page.getByRole('status')).toHaveText('Unsaved changes');
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'PortableProbe(3)');
  const calculation = await execute(page, 'Calculate!', 'calculate');
  expect(calculation.request().postDataJSON().customFunctions).toBe(definitions);
  await expect(page.getByRole('status')).toHaveText('10');
  await page.getByRole('link', { name: 'Scripting', exact: true }).click();
  await replaceCode(page, 'scriptTextArea', 'write(PortableProbe(5));');
  await execute(page, 'Process script', 'script');
  await expect(page.getByRole('status')).toHaveText('12');
  await page.getByRole('link', { name: 'Custom functions', exact: true }).click();
  await page.getByRole('button', { name: 'Save in this tab', exact: true }).click();
  await page.reload();
  const pending = page.waitForEvent('download');
  await page.getByRole('button', { name: 'Export .tslf', exact: true }).click();
  expect(await readFile((await (await pending).path())!, 'utf8')).toBe(definitions);
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await replaceCode(page, 'expressionTextArea', 'PortableProbe(9)');
  await execute(page, 'Calculate!', 'calculate');
  await expect(page.getByRole('status')).toHaveText('16');
});

test('the existing numerical screen uses the actual portable methods', async ({ page }) => {
  await page.goto('/numerical-calculations');
  await replaceCode(page, 'expressionTextArea', '2');
  await expect(page.getByLabel('Method:', { exact: true })).toBeEnabled();
  await expect(page.getByLabel('Method:', { exact: true })).toHaveValue("Simpson's method");
  await page.getByRole('spinbutton', { name: 'a', exact: true }).fill('0');
  await page.getByRole('spinbutton', { name: 'b', exact: true }).fill('3');
  await page.getByRole('spinbutton', { name: 'N', exact: true }).fill('100');
  await execute(page, 'Compute!', 'integral');
  await expect.poll(async () => Number(await page.getByRole('status').textContent())).toBeCloseTo(6, 8);
  // Editing the root tolerance before switching tools must not add epsilon to
  // the portable derivative contract; its shared algorithm chooses the step.
  await page.getByLabel('Operation:', { exact: true }).selectOption('Function root');
  await page.getByRole('spinbutton', { name: 'ε', exact: true }).fill('');
  await page.getByLabel('Operation:', { exact: true }).selectOption('Derivative');
  await expect(page.getByRole('spinbutton', { name: 'ε', exact: true })).toHaveCount(0);
  await replaceCode(page, 'expressionTextArea', 'x*x');
  await page.getByRole('spinbutton', { name: 'x', exact: true }).fill('3');
  const derivative = await execute(page, 'Compute!', 'derivative');
  expect(derivative.request().postDataJSON()).not.toHaveProperty('epsilon');
  await expect.poll(async () => Number(await page.getByRole('status').textContent())).toBeCloseTo(6, 5);
  await page.screenshot({ path: test.info().outputPath('portable-numerical.png'), fullPage: true });
});

test('unsupported charts and compilation failures are visible and corrected input recovers', async ({ page }) => {
  await page.goto('/calculate');
  await page.getByRole('link', { name: 'Chart', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Chart', exact: true })).toBeVisible();
  await replaceCode(page, 'expressionTextArea', 'x*x');
  const chartResponse = page.waitForResponse(response =>
    response.url().endsWith('/api/v2/chart') && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Draw chart!', exact: true }).click();
  const chart = await chartResponse;
  expect(chart.status()).toBe(422);
  await expect(page.getByRole('alert')).toHaveText((await chart.json()).detail);
  await page.screenshot({ path: test.info().outputPath('portable-chart-unavailable.png'), fullPage: true });
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  // Both routes use the same Ace DOM ID. The URL can change before React
  // replaces the Chart editor, so wait for the destination screen before typing.
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await replaceCode(page, 'expressionTextArea', 'missing(2)');
  await expect(page.locator('#expressionTextArea .ace_content')).toContainText('missing(2)');
  await expect(page.getByRole('button', { name: 'Calculate!', exact: true })).toBeEnabled();
  const invalid = page.waitForResponse(response => response.url().endsWith('/api/v2/execute'));
  await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
  expect((await invalid).status()).toBe(400);
  await expect(page.getByRole('alert')).toContainText('missing');
  await replaceCode(page, 'expressionTextArea', '6*7');
  await execute(page, 'Calculate!', 'calculate');
  await expect(page.getByRole('status')).toHaveText('42');
});

test('cancelling actual running TSL returns to idle and permits another calculation', async ({ page, request }) => {
  await page.goto('/scripting');
  await replaceCode(page, 'scriptTextArea', 'while (true) { }');
  await page.getByRole('button', { name: 'Process script', exact: true }).click();
  await expect.poll(async () => (await (await request.get('/api/portable/status')).json()).activeWorkerCount).toBe(1);
  await page.getByRole('button', { name: 'Cancel execution', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveText('Execution cancelled.');
  await expect.poll(async () => (await (await request.get('/api/portable/status')).json()).activeWorkerCount).toBe(0);
  await replaceCode(page, 'scriptTextArea', 'write(7*6);');
  await execute(page, 'Process script', 'script');
  await expect(page.getByRole('status')).toHaveText('42');
});
