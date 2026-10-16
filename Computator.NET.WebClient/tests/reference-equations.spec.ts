import { test, expect } from '@playwright/test';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

// Use an actual committed equation, not a one-pixel stand-in. These tests run
// against both the Vite preview and the published ASP.NET host via existing Cake.
const manifest = JSON.parse(readFileSync(resolve('..', 'Computator.NET.Core', 'Static', 'reference-equations', 'manifest.json'), 'utf8'));
const [id, asset] = Object.entries(manifest.equations)[0] as [string, { width: number; height: number }];

for (const [route, editor] of [['/calculate', '#expressionTextArea'], ['/scripting', '#scriptTextArea']]) {
  test(`offline equation and context-aware overload help in ${route}`, async ({ page }, info) => {
    const remoteImages: string[] = [];
    page.on('request', request => {
      if (request.resourceType() === 'image' && !request.url().startsWith('http://127.0.0.1:4173/')) remoteImages.push(request.url());
    });
    await page.route('**/api/autocomplete/**', route => route.fulfill({
      headers: { 'access-control-allow-origin': '*' },
      json: [{ text: 'ReferenceEquationProbe(x)', menuText: 'real ReferenceEquationProbe(real x)', details: {
        title: 'Offline typeset reference', category: 'Functions', type: 'ElementaryFunctions',
        description: '<div class="function-reference"><p class="reference-signature"><code>real ReferenceEquationProbe(real x)</code><br/><code>complex ReferenceEquationProbe(complex x)</code></p>' +
          '<p class="reference-summary">A definition with retained alternate overload information.</p>' +
          `<p class="reference-equation"><img class="reference-math" src="reference-equations/${id}.png" width="${asset.width}" height="${asset.height}" alt="A genuine typeset equation" onerror="window.referenceExecuted=true" style="opacity:0" /></p>` +
          '<p class="reference-explanation">Interpret the result before using it in another computation.</p>' +
          '<div class="reference-worked-example"><p><strong>Worked example</strong></p><pre><code>Probe(1)</code></pre><p>Result: one</p></div></div>'
      } }]
    }));
    await page.goto(route);
    await page.locator(`${editor} textarea`).fill('ReferenceEquationProbe');
    await page.locator(`${editor} textarea`).press('Control+Space');
    const tooltip = page.getByRole('tooltip');
    await expect(tooltip.locator('.reference-signature code')).toHaveCount(1);
    await expect(tooltip.locator('.reference-signature code')).toHaveText('complex ReferenceEquationProbe(complex x)');
    await expect(tooltip).not.toContainText('real ReferenceEquationProbe(real x)');
    const equation = tooltip.locator('img.reference-math');
    await expect(equation).toHaveAttribute('src', `/reference-equations/${id}.png`);
    await expect.poll(() => equation.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
    expect(await equation.evaluate((node: HTMLImageElement) => node.naturalWidth)).toBe(3 * asset.width);
    await expect(equation).not.toHaveAttribute('style');
    await expect(equation).not.toHaveAttribute('onerror');
    await expect(tooltip.locator('.reference-worked-example pre code')).toHaveText('Probe(1)');
    expect(remoteImages).toEqual([]);
    await page.screenshot({ path: info.outputPath('offline-equation-and-overloads.png') });
  });
}

test('packaged equation endpoint returns PNG bytes, not the SPA fallback', async ({ request }) => {
  const response = await request.get(`/reference-equations/${id}.png`);
  expect(response.ok()).toBeTruthy();
  expect(response.headers()['content-type']).toContain('image/png');
  expect((await response.body()).subarray(0, 8).equals(Buffer.from([137,80,78,71,13,10,26,10]))).toBe(true);
});
