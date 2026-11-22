import { test, expect } from '@playwright/test';

const answer = { result: '2*x + 1', latex: '2x + 1', status: 'exact', warnings: [] };

test('Ctrl+Enter calculates equation systems through the shared form shortcut', async ({ page }) => {
  const bodies: Record<string, unknown>[] = [];
  await page.route('**/api/v2/symbolic', async route => {
    bodies.push(route.request().postDataJSON());
    await route.fulfill({ json: { ...answer, result: '[[2, 1]]' } });
  });
  await page.goto('/symbolic-calculations');
  await page.getByLabel('Operation', { exact: true }).selectOption('solve-system');
  await page.getByLabel('Equations — one per line').fill('x+y=3\nx-y=1');
  await page.getByLabel('Variables — comma separated').fill('x,y');
  await page.getByLabel('Equations — one per line').press('ControlOrMeta+Enter');
  await expect(page.getByRole('status')).toHaveText('[[2, 1]]');
  expect(bodies).toEqual([{ operation: 'solve-system', domain: 'complex', equations: ['x+y=3', 'x-y=1'], variables: ['x', 'y'] }]);
});

test('symbolic options send only relevant parameters and retain the draft across routes', async ({ page }) => {
  const bodies: Record<string, unknown>[] = [];
  await page.route('**/api/v2/symbolic', async route => {
    bodies.push(route.request().postDataJSON());
    await route.fulfill({ json: answer });
  });
  await page.goto('/symbolic-calculations');
  await page.getByLabel('Operation', { exact: true }).selectOption('differentiate');
  await page.getByLabel('Expression', { exact: true }).fill('sin(x^2)');
  await page.getByLabel('Order', { exact: true }).fill('2');
  await page.getByLabel('Variable', { exact: true }).fill('x');
  await page.getByLabel('Domain', { exact: true }).selectOption('real');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText(answer.result);
  expect(bodies[0]).toEqual({ operation: 'differentiate', expression: 'sin(x^2)', variable: 'x', order: 2, domain: 'real' });
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await page.getByRole('link', { name: 'Symbolic calculations', exact: true }).click();
  await expect(page.getByLabel('Operation', { exact: true })).toHaveValue('differentiate');
  await expect(page.getByLabel('Expression', { exact: true })).toHaveValue('sin(x^2)');
  await expect(page.getByLabel('Order', { exact: true })).toHaveValue('2');
  await page.getByLabel('Operation', { exact: true }).selectOption('simplify');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText(answer.result);
  expect(bodies[1]).toEqual({ operation: 'simplify', expression: 'sin(x^2)', domain: 'real' });
});

test('optional evaluation substitutions are simultaneous request data and duplicates are rejected', async ({ page }) => {
  const bodies: Record<string, unknown>[] = [];
  await page.route('**/api/v2/symbolic', async route => {
    bodies.push(route.request().postDataJSON());
    await route.fulfill({ json: { ...answer, result: '7', status: 'approximate' } });
  });
  await page.goto('/symbolic-calculations');
  await page.getByLabel('Operation', { exact: true }).selectOption('evaluate');
  await page.getByLabel('Expression', { exact: true }).fill('x^2+y');
  await page.getByLabel('Optional substitutions — one per line').fill('x = 2\ny = 3');
  await page.getByLabel('Decimal precision').fill('40');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('7');
  expect(bodies[0]).toEqual({ operation: 'evaluate', expression: 'x^2+y', domain: 'complex', precision: 40, substitutions: { x: '2', y: '3' } });
  await expect(page.getByText('Numerical approximation', { exact: true })).toBeVisible();
  await page.getByLabel('Optional substitutions — one per line').fill('x = 2\nx = 3');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('repeated');
  expect(bodies).toHaveLength(1);
  await page.getByRole('button', { name: 'Load example', exact: true }).click();
  await expect(page.getByLabel('Optional substitutions — one per line')).toHaveValue('');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('7');
  expect(bodies[1]).toMatchObject({ expression: 'sqrt(2)', precision: 30, substitutions: {} });
});

test('cancellation and edited input prevent stale results from replacing a later answer', async ({ page }) => {
  let release: (() => void) | undefined;
  await page.route('**/api/v2/symbolic', async route => {
    if (route.request().postDataJSON().expression === 'slow') {
      await new Promise<void>(resolve => { release = resolve; });
      await route.fulfill({ json: { ...answer, result: 'obsolete' } }).catch(() => {});
    } else await route.fulfill({ json: { ...answer, result: '42' } });
  });
  await page.goto('/symbolic-calculations');
  await page.getByLabel('Expression', { exact: true }).fill('slow');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('Calculating…');
  await expect.poll(() => Boolean(release)).toBe(true);
  await page.getByRole('button', { name: 'Cancel calculation' }).click();
  await expect(page.getByRole('alert')).toHaveText('Calculation cancelled.');
  await page.getByLabel('Expression', { exact: true }).fill('6*7');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText('42');
  release?.();
  await expect(page.getByRole('status')).toHaveText('42');
  await expect(page.getByText('obsolete', { exact: true })).toHaveCount(0);
});

test('result, LaTeX and warnings remain inert text and unresolved answers are labelled', async ({ page }) => {
  const text = '<img src=x onerror="window.symbolicInjected=true">';
  await page.route('**/api/v2/symbolic', route => route.fulfill({ json: { result: text, latex: text, warnings: [text], status: 'unevaluated' } }));
  await page.goto('/symbolic-calculations');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText(text);
  await expect(page.getByText('Unresolved result', { exact: true })).toBeVisible();
  await page.getByText('LaTeX', { exact: true }).click();
  await expect(page.locator('.symbolic-latex')).toHaveText(text);
  await expect(page.locator('.symbolic-workspace img')).toHaveCount(0);
  expect(await page.evaluate(() => 'symbolicInjected' in window)).toBe(false);
});

test('timeout and invalid responses are visible and a valid request recovers', async ({ page }) => {
  let call = 0;
  await page.route('**/api/v2/symbolic', route => {
    call++;
    return call === 1 ? route.fulfill({ status: 504, json: { detail: 'The symbolic time budget was exceeded.' } })
      : call === 2 ? route.fulfill({ json: { result: 'missing status' } }) : route.fulfill({ json: answer });
  });
  await page.goto('/symbolic-calculations');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveText('The symbolic time budget was exceeded.');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveText('Invalid symbolic response.');
  await page.getByRole('button', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('status')).toHaveText(answer.result);
  await expect(page.getByRole('alert')).toHaveCount(0);
});
