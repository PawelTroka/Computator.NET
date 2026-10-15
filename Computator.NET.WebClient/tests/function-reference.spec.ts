import { test, expect } from '@playwright/test';

test('HTML reference becomes readable inert text in the existing Ace tooltip', async ({ page }) => {
  const unexpected: string[] = [];
  page.on('request', request => { if (request.url().includes('reference-must-not-load')) unexpected.push(request.url()); });
  await page.route('**/api/autocomplete/**', route => route.fulfill({
    headers: { 'access-control-allow-origin': '*' },
    json: [{ text: 'ReferenceProbe(x)', menuText: 'real ReferenceProbe(real x)', details: {
      title: 'Reference probe', category: 'Functions', type: 'ElementaryFunctions',
      description: '<div><p>Meaning: <b>π &amp; γ</b>.</p><p>Arguments: x &gt; 0.</p>' +
        '<script>window.referenceExecuted=true</script><img src="https://reference-must-not-load.invalid/a.png" onerror="window.referenceExecuted=true">' +
        '<p>Example: <code>ReferenceProbe(1)</code></p></div>'
    } }]
  }));
  await page.goto('/calculate');
  await page.locator('#expressionTextArea textarea').fill('ReferenceProbe');
  await page.locator('#expressionTextArea textarea').press('Control+Space');
  const row = page.locator('.ace_autocomplete .ace_line').first();
  await expect(row).toContainText('ReferenceProbe');
  await row.hover();
  const tooltip = page.locator('.ace_tooltip');
  await expect(tooltip).toContainText('Meaning: π & γ');
  await expect(tooltip).toContainText('Arguments: x > 0');
  await expect(tooltip).not.toContainText('<p>');
  await expect(tooltip).not.toContainText('referenceExecuted');
  expect(await page.evaluate(() => (window as unknown as { referenceExecuted?: boolean }).referenceExecuted)).toBeUndefined();
  expect(unexpected).toEqual([]);
});
