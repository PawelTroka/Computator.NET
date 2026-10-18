import { test, expect } from '@playwright/test';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

// Use an actual committed equation, not a one-pixel stand-in. These tests run
// against both the Vite preview and the published ASP.NET host via existing Cake.
const manifest = JSON.parse(readFileSync(resolve('..', 'Computator.NET.Core', 'Static', 'reference-equations', 'manifest.json'), 'utf8'));
const equations = manifest.equations as Record<string, { tex: string; width: number; height: number }>;
const gamma = Object.entries(equations).find(([, equation]) => equation.tex.includes('Gamma(z)'));
if (!gamma) throw new Error('The shipped gamma integral is missing');
const [id, asset] = gamma;

for (const [route, editor] of [['/calculate', '#expressionTextArea'], ['/scripting', '#scriptTextArea']]) {
  for (const caption of ['real ReferenceEquationProbe(real x)', 'ReferenceEquationProbe(x)']) {
  test(`offline equation without repeated signatures in ${route} with ${caption}`, async ({ page }, info) => {
    const remoteImages: string[] = [];
    page.on('request', request => {
      if (request.resourceType() === 'image' && !request.url().startsWith('http://127.0.0.1:4173/')) remoteImages.push(request.url());
    });
    await page.route('**/api/autocomplete/**', route => route.fulfill({
      headers: { 'access-control-allow-origin': '*' },
      json: [{ text: 'ReferenceEquationProbe(x)', menuText: caption, details: {
        title: 'Offline typeset reference', category: 'Functions', type: 'ElementaryFunctions',
        description: '<div class="function-reference"><p class="reference-signature"><code>real ReferenceEquationProbe(real x)</code><br/><code>complex ReferenceEquationProbe(complex x)</code></p>' +
          '<p class="reference-summary">A definition for both real and complex inputs.</p>' +
          `<p class="reference-equation"><img class="reference-math" src="reference-equations/${id}.png" width="${asset.width}" height="${asset.height}" alt="A genuine typeset equation" onerror="window.referenceExecuted=true" style="opacity:0" /></p>` +
          '<p class="reference-explanation">Interpret the result before using it in another computation.</p>' +
          '<div class="reference-worked-example"><p><strong>Worked example</strong></p><pre><code>Probe(1)</code></pre><p>Result: one</p></div></div>'
      } }]
    }));
    await page.goto(route);
    await page.locator(`${editor} textarea`).fill('ReferenceEquationProbe');
    await page.locator(`${editor} textarea`).press('Control+Space');
    const tooltip = page.getByRole('tooltip');
    await expect(tooltip.locator('.reference-signature')).toHaveCount(0);
    await expect(tooltip).not.toContainText('complex ReferenceEquationProbe(complex x)');
    await expect(page.locator('.ace_autocomplete')).toContainText(caption);
    await expect(tooltip.locator('.reference-summary')).toHaveText('A definition for both real and complex inputs.');
    await expect(tooltip).not.toContainText('real ReferenceEquationProbe(real x)');
    const equation = tooltip.locator('img.reference-math');
    await expect(equation).toHaveAttribute('src', `/reference-equations/${id}.png`);
    await expect.poll(() => equation.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
    expect(await equation.evaluate((node: HTMLImageElement) => node.naturalWidth)).toBe(3 * asset.width);
    await expect(equation).not.toHaveAttribute('style');
    await expect(equation).not.toHaveAttribute('onerror');
    await expect(tooltip.locator('.reference-worked-example pre code')).toHaveText('Probe(1)');
    expect(remoteImages).toEqual([]);
    await page.screenshot({ path: info.outputPath('offline-equation-without-signatures.png') });
  });
  }
}

test('packaged equation endpoint returns PNG bytes, not the SPA fallback', async ({ request }) => {
  const response = await request.get(`/reference-equations/${id}.png`);
  expect(response.ok()).toBeTruthy();
  expect(response.headers()['content-type']).toContain('image/png');
  expect((await response.body()).subarray(0, 8).equals(Buffer.from([137,80,78,71,13,10,26,10]))).toBe(true);
});

for (const [route, editor] of [['/calculate', '#expressionTextArea'], ['/scripting', '#scriptTextArea']]) {
  test(`unopened reference articles do not fetch their equations in ${route}`, async ({ page }) => {
    const other = Object.entries(equations).find(([otherId]) => otherId !== id)!;
    const images: string[] = [];
    page.on('request', request => {
      const path = new URL(request.url()).pathname;
      if (path.startsWith('/reference-equations/')) images.push(path);
    });
    await page.route('**/api/autocomplete/**', request => request.fulfill({
      headers: { 'access-control-allow-origin': '*' },
      json: [
        ['SelectedEquationProbe', id, asset],
        ['UnopenedEquationProbe', other[0], other[1]]
      ].map(([name, equationId, dimensions]) => {
        const size = dimensions as typeof asset;
        return { text: `${name}(x)`, menuText: `real ${name}(real x)`, details: {
          title: name, type: 'SpecialFunctions',
          description: `<p class="reference-equation"><img class="reference-math" src="reference-equations/${equationId}.png" width="${size.width}" height="${size.height}" alt="${name} equation" /></p>`
        } };
      })
    }));
    await page.goto(route);
    const input = page.locator(`${editor} textarea`);
    await input.fill('SelectedEquationProbe');
    await input.press('Control+Space');
    const tooltip = page.getByRole('tooltip');
    await expect(tooltip.locator('.reference-title')).toHaveText('SelectedEquationProbe');
    const equation = tooltip.locator('img.reference-math');
    await expect.poll(() => equation.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
    // Completing and displaying this article proves the entire autocomplete
    // response was processed. Its unopened sibling must not issue a request.
    expect([...new Set(images)]).toEqual([`/reference-equations/${id}.png`]);
  });
}
