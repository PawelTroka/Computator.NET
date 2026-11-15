import { test, expect, type Page } from '@playwright/test';

// Exercise the actual published catalogue, Ace replacement and execution worker.
// Qualified names must replace the type prefix rather than duplicating it.
async function complete(page: Page, editor: string, source: string, completed: string, caption: RegExp) {
  const input = page.locator(`#${editor} textarea`);
  await input.focus();
  await input.press('ControlOrMeta+A');
  await page.keyboard.insertText(source);
  await input.press('Control+Space');
  const popup = page.locator('.ace_autocomplete');
  const content = page.locator(`#${editor} .ace_text-layer`);
  // Ace renders a thin-space marker; the exact source is independently checked
  // in the real execution request below, where U+2009 must remain unchanged.
  const rendered = completed.replace(/\u2009/g, '·');
  // Ace may insert a unique match immediately; overloaded methods show a menu.
  await expect.poll(async () => await popup.isVisible() || (await content.textContent()) === rendered).toBe(true);
  if (await popup.isVisible()) {
    const selected = popup.locator('.ace_line.ace_selected');
    // WebKit's innerText inserts line breaks around Ace's highlighted spans;
    // the menu item's accessible name preserves the actual method signature.
    for (let index = 0; index < 50 && !caption.test(await selected.getAttribute('aria-label') ?? ''); index++) {
      const position = await selected.getAttribute('aria-posinset');
      await input.press('ArrowDown');
      await expect(selected).not.toHaveAttribute('aria-posinset', position!);
    }
    await expect(selected).toContainText(caption);
    await input.press('Enter');
  }
  await expect(content).toHaveText(rendered);
  await input.focus();
}

test('both portable catalogues expose the existing scientific families with typed overloads', async ({ request }) => {
  for (const mode of ['expression', 'scripting']) {
    const response = await request.get(`/api/autocomplete/${mode}`);
    expect(response.ok()).toBeTruthy();
    const items: { text: string; menuText: string }[] = await response.json();
    const names = items.map(item => item.text);
    for (const name of ['Euclid.GreatestCommonDivisor', 'Polynomial.Evaluate', 'Polynomial.Zero',
      'DifferentialEquation.rungeKutta4', 'Integral.simpsonMethod', 'Derivative.derivative', 'FunctionRoot.BrentMethod'])
      expect(names).toContain(name);
    expect(items.filter(item => item.text === 'Euclid.PowerOfTwo').map(item => item.menuText)).toEqual([
      'int Euclid.PowerOfTwo(int exponent)', 'integer Euclid.PowerOfTwo(integer exponent)'
    ]);
    expect(names.includes('write')).toBe(mode === 'scripting');
  }
});

test('qualified calculator completions produce executable expressions without duplicate prefixes', async ({ page }) => {
  await page.goto('/calculate');
  const examples = [
    { prefix: 'Euclid.Greatest', name: 'Euclid.GreatestCommonDivisor', arguments: '(12,18)', result: 6 },
    { prefix: 'Polynomial.Eval', name: 'Polynomial.Evaluate', arguments: '(2,1,-3,2)', result: 3 },
    { prefix: 'DifferentialEquation.run', name: 'DifferentialEquation.rungeKutta4', arguments: '((t,u) => 2t,3.0,0.0,2.0,21)[20]', result: 7 },
    { prefix: 'Integral.simp', name: 'Integral.simpsonMethod', arguments: '(t => t²,0,1,100)', result: 1 / 3 },
    { prefix: 'Derivative.deriv', name: 'Derivative.derivative', arguments: '(t => t²,3)', result: 6 },
    { prefix: 'FunctionRoot.Brent', name: 'FunctionRoot.BrentMethod', arguments: '(t => t²-2,0,2)', result: Math.SQRT2 },
    { prefix: 'new Poly', name: 'new Polynomial', arguments: '(1,-3,2).Evaluate(2)', result: 3 },
    { prefix: 'Polynomial.Ze', name: 'Polynomial.Zero', arguments: '.Evaluate(2)', result: 0 },
    { prefix: '1-Integral.simp', name: '1-Integral.simpsonMethod', caption: 'Integral.simpsonMethod', arguments: '(t => t²,0,1,100)', result: 2 / 3 },
    { prefix: '2*Euclid.Greatest', name: '2·Euclid.GreatestCommonDivisor', caption: 'Euclid.GreatestCommonDivisor', arguments: '(12,18)', result: 12 },
    { prefix: '1-\u2009Integral.simp', name: '1-\u2009Integral.simpsonMethod', caption: 'Integral.simpsonMethod', arguments: '(t => t²,0,1,100)', result: 2 / 3 }
  ];
  for (const example of examples) {
    const escaped = (example.caption ?? example.name).replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    await complete(page, 'expressionTextArea', example.prefix, example.name,
      new RegExp(escaped + (example.name.endsWith('.Zero') ? '' : '\\(')));
    await page.keyboard.insertText(example.arguments);
    const pending = page.waitForResponse(response => response.url().endsWith('/api/v2/execute'));
    await page.getByRole('button', { name: 'Calculate!', exact: true }).click();
    const response = await pending;
    expect(response.request().postDataJSON().expression).toBe(example.name + example.arguments);
    expect(response.ok(), await response.text()).toBeTruthy();
    await expect.poll(async () => Number(await page.getByRole('status').textContent())).toBeCloseTo(example.result, 7);
  }
  await page.screenshot({ path: test.info().outputPath('portable-scientific-completion.png'), fullPage: true });
});

test('scientific help preserves generic argument types in the visible tooltip', async ({ page }) => {
  await page.goto('/calculate');
  const input = page.locator('#expressionTextArea textarea');
  await input.focus();
  await page.keyboard.insertText('Integral.');
  await input.press('Control+Space');
  await expect(page.locator('.ace_autocomplete')).toBeVisible();
  await expect(page.getByRole('tooltip')).toContainText('Func<real, real>');
  await page.screenshot({ path: test.info().outputPath('portable-scientific-help.png'), fullPage: true });
});

test('qualified completion inside a script preserves the surrounding call', async ({ page }) => {
  await page.goto('/scripting');
  await complete(page, 'scriptTextArea', 'write(Integral.simp', 'write(Integral.simpsonMethod', /Integral\.simpsonMethod\(/);
  await page.keyboard.insertText('(t => t²,0,1,100));');
  const pending = page.waitForResponse(response => response.url().endsWith('/api/v2/execute'));
  await page.getByRole('button', { name: 'Process script', exact: true }).click();
  const response = await pending;
  expect(response.request().postDataJSON().expression).toBe('write(Integral.simpsonMethod(t => t²,0,1,100));');
  expect(response.ok(), await response.text()).toBeTruthy();
  await expect.poll(async () => Number(await page.getByRole('status').textContent())).toBeCloseTo(1 / 3, 10);
});
