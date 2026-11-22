import { test, expect, type APIRequestContext, type Page } from '@playwright/test';

async function symbolic(request: APIRequestContext, body: Record<string, unknown>) {
  const response = await request.post('/api/v2/symbolic', { data: body });
  expect(response.status(), await response.text()).toBe(200);
  return await response.json() as { result: string; latex: string; status: string; warnings: string[] };
}
async function calculate(page: Page, operation: string) {
  const response = page.waitForResponse(response => response.url().endsWith('/api/v2/symbolic') && response.request().method() === 'POST');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  const result = await response;
  expect(result.request().postDataJSON().operation).toBe(operation);
  expect(result.status(), await result.text()).toBe(200);
  const body = await result.json();
  await expect(page.getByRole('status')).toHaveText(body.result);
  expect(body.latex).not.toBe('');
  return body;
}

// These browser tests use the published application and its real child workers.
// No route is intercepted and no test-specific mathematical evaluator is used.
const cases = [
  ['simplify', 'x'], ['expand', 'x'], ['factor', 'x'], ['differentiate', 'cos'],
  ['integrate', 'C'], ['definite-integrate', '1'], ['limit', '1'], ['solve', '2'],
  ['solve-system', '2'], ['substitute', '7'], ['evaluate', '1.414213562373095'],
  ['taylor', 'x'], ['determinant', '-2'], ['inverse', '-2'], ['transpose', '6'], ['trace', '5']
];
for (const [operation, expected] of cases) {
  test(`symbolic ${operation} example executes from the real browser`, async ({ page }) => {
    await page.goto('/symbolic-calculations');
    await page.getByLabel('Operation', { exact: true }).selectOption(operation);
    await page.getByRole('button', { name: 'Load example', exact: true }).click();
    const result = await calculate(page, operation);
    expect(result.result).toContain(expected);
    expect(result.status).toBe(operation === 'evaluate' ? 'approximate' : 'exact');
    await page.getByText('LaTeX', { exact: true }).click();
    await expect(page.locator('.symbolic-latex')).toHaveText(result.latex);
    if (operation === 'differentiate' || operation === 'inverse') {
      await page.evaluate(() => window.scrollTo(0, 0));
      await page.screenshot({ path: test.info().outputPath(`symbolic-${operation}.png`), fullPage: true });
    }
  });
}

test('exact rational, derivative and polynomial answers survive complete HTTP/worker round trips', async ({ request }) => {
  expect((await symbolic(request, { expression: '1/3 + 1/6' })).result.replace(/\s/g, '')).toBe('1/2');
  const derivative = await symbolic(request, { operation: 'differentiate', expression: 'x^4', order: 2 });
  const value = await symbolic(request, { operation: 'substitute', expression: derivative.result, substitutions: { x: '2' } });
  expect(value.result).toBe('48');
  const expanded = await symbolic(request, { operation: 'expand', expression: '(x+y)^3' });
  expect((await symbolic(request, { operation: 'substitute', expression: expanded.result, substitutions: { x: '2', y: '3' } })).result).toBe('125');
  expect((await symbolic(request, { operation: 'definite-integrate', expression: 'x^2', lowerBound: '0', upperBound: '3' })).result).toBe('9');
});

test('Unicode source, simultaneous substitutions and explicit domains reach the same engine', async ({ request }) => {
  expect((await symbolic(request, { operation: 'substitute', expression: 'α² + β', substitutions: { α: '2', β: '3' } })).result).toBe('7');
  const swapped = await symbolic(request, { operation: 'substitute', expression: 'x-y', substitutions: { x: 'y', y: 'x' } });
  expect((await symbolic(request, { operation: 'substitute', expression: swapped.result, substitutions: { x: '2', y: '3' } })).result).toBe('1');
  const complex = await symbolic(request, { operation: 'solve', expression: 'x^2+1=0', domain: 'complex' });
  const real = await symbolic(request, { operation: 'solve', expression: 'x^2+1=0', domain: 'real' });
  expect(complex.result).toContain('i');
  expect(real.result).not.toContain('i');
  expect(real.result).not.toBe(complex.result);
});

test('unresolved integrals retain their status and corrected input recovers after parser errors', async ({ page }) => {
  await page.goto('/symbolic-calculations');
  await page.getByLabel('Operation', { exact: true }).selectOption('integrate');
  await page.getByLabel('Expression', { exact: true }).fill('sin(x^x)');
  const unresolved = await calculate(page, 'integrate');
  expect(unresolved.status).toBe('unevaluated');
  expect(unresolved.warnings.length).toBeGreaterThan(0);
  await expect(page.getByText('Unresolved result', { exact: true })).toBeVisible();
  await page.getByLabel('Expression', { exact: true }).fill('x+');
  const invalid = page.waitForResponse(response => response.url().endsWith('/api/v2/symbolic'));
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  expect((await invalid).status()).toBe(400);
  await expect(page.getByRole('alert')).toBeVisible();
  await page.getByLabel('Operation', { exact: true }).selectOption('simplify');
  await page.getByLabel('Expression', { exact: true }).fill('6*7');
  expect((await calculate(page, 'simplify')).result).toBe('42');
});

test('symbolic requests retain HTTP origin, media, size and mathematical input boundaries', async ({ request }) => {
  const forbidden = await request.post('/api/v2/symbolic', { headers: { Origin: 'https://example.invalid' }, data: { expression: '1' } });
  expect(forbidden.status()).toBe(403);
  const media = await request.post('/api/v2/symbolic', { headers: { 'Content-Type': 'text/plain' }, data: '{}' });
  expect(media.status()).toBe(415);
  for (const body of [{ expression: 'x'.repeat(8193) }, { expression: 'x', operation: 'unknown' },
    { expression: 'x', order: 999, operation: 'differentiate' }, { expression: 'System.IO.File.Delete("test")' }]) {
    const invalid = await request.post('/api/v2/symbolic', { data: body });
    expect(invalid.status(), await invalid.text()).toBe(400);
  }
  const oversized = await request.post('/api/v2/symbolic', { data: { expression: 'x'.repeat(262145) } });
  expect(oversized.status()).toBe(413);
  const alternate = await request.post('/api/v2/execute', { data: { operation: 'symbolic', symbolic: { expression: '1' } } });
  expect(alternate.status()).toBe(400);
  expect((await symbolic(request, { expression: '7*6' })).result).toBe('42');
  expect((await (await request.get('/api/portable/status')).json()).symbolic).toBe(true);
});

test('matrix screen stays usable at a narrow viewport', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/symbolic-calculations');
  await page.getByLabel('Operation', { exact: true }).selectOption('inverse');
  await page.getByRole('button', { name: 'Load example', exact: true }).click();
  await calculate(page, 'inverse');
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth + 1)).toBe(true);
  await page.evaluate(() => window.scrollTo(0, 0));
  await page.screenshot({ path: test.info().outputPath('symbolic-mobile.png'), fullPage: true });
});
