import { test, expect } from '@playwright/test';

for (const [route, editor] of [['/calculate', '#expressionTextArea'], ['/scripting', '#scriptTextArea']]) {
test(`HTML reference retains safe formatting in the ${route} Ace tooltip`, async ({ page }, info) => {
  const unexpected: string[] = [];
  page.on('request', request => { if (request.url().includes('reference-must-not-load')) unexpected.push(request.url()); });
  await page.route('**/api/autocomplete/**', route => route.fulfill({
    headers: { 'access-control-allow-origin': '*' },
    json: [{ text: 'ReferenceProbe(x)', menuText: 'real ReferenceProbe(real x)', details: {
      title: 'Reference probe <em>literal title</em>', category: 'Functions', type: 'ElementaryFunctions',
      description: '<div class="function-reference"><p class="reference-summary" style="background:url(https://reference-must-not-load.invalid/style)">Meaning: <b>π &amp; γ</b>.</p><p>Arguments: x &gt; 0.</p>' +
        '<p class="reference-formula"><code>x<sup>2</sup> + a<sub>n</sub> ≤ ∞</code></p>' +
        '<a href="https://dlmf.nist.gov/5" onclick="window.referenceExecuted=true">Primary source</a>' +
        '<a href="javascript:window.referenceExecuted=true">Unsafe link</a>' +
        '<a href="data:text/html,unsafe">Data link</a>' +
        '<svg onload="window.referenceExecuted=true"><a href="javascript:alert(1)">SVG payload</a></svg>' +
        '<iframe src="https://reference-must-not-load.invalid/frame"></iframe>' +
        '<style>body{background:url(https://reference-must-not-load.invalid/css)}</style>' +
        '<script>window.referenceExecuted=true</script><img src="https://reference-must-not-load.invalid/a.png" onerror="window.referenceExecuted=true">' +
        '<p>Example: <code>ReferenceProbe(1)</code></p></div>'
    } }]
  }));
  await page.goto(route);
  await page.locator(`${editor} textarea`).fill('ReferenceProbe');
  await page.locator(`${editor} textarea`).press('Control+Space');
  await expect(page.locator('.ace_autocomplete .ace_line').first()).toContainText('ReferenceProbe');
  // Keyboard selection already opens Ace's documentation. Do not introduce a
  // redundant mouse-hover transition into this keyboard-accessibility contract.
  const tooltip = page.getByRole('tooltip');
  await expect(tooltip).toContainText('Meaning: π & γ');
  await expect(tooltip).toContainText('Arguments: x > 0');
  await expect(tooltip).toContainText('Example: ReferenceProbe(1)');
  await expect(tooltip.locator('.reference-title')).toHaveText('Reference probe <em>literal title</em>');
  await expect(tooltip.locator('.reference-title em')).toHaveCount(0);
  await expect(tooltip.locator('.reference-summary b')).toHaveText('π & γ');
  await expect(tooltip.locator('.reference-formula code sup')).toHaveText('2');
  await expect(tooltip.locator('.reference-formula code sub')).toHaveText('n');
  await expect(tooltip.locator('.reference-formula code')).toContainText('≤ ∞');
  await expect(tooltip.getByRole('link', { name: 'Primary source' })).toHaveAttribute('href', 'https://dlmf.nist.gov/5');
  await expect(tooltip.getByRole('link', { name: 'Primary source' })).toHaveAttribute('rel', 'noopener noreferrer');
  await expect(tooltip.locator('a[href]')).toHaveCount(1);
  await expect(tooltip.locator('script,img,iframe,svg,style,[style],[onclick],[onerror]')).toHaveCount(0);
  await expect(tooltip).not.toContainText('<p>');
  await expect(tooltip).not.toContainText('referenceExecuted');
  await expect(tooltip).not.toContainText('SVG payload');
  expect(await page.evaluate(() => (window as unknown as { referenceExecuted?: boolean }).referenceExecuted)).toBeUndefined();
  expect(unexpected).toEqual([]);
  await page.screenshot({ path: info.outputPath('function-reference-tooltip.png') });
});
}
