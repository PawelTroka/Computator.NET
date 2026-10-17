import { test, expect, type Page } from '@playwright/test';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const directory = resolve('..', 'Computator.NET.Core', 'Static');
const catalogue = readFileSync(resolve(directory, 'functions.xml'), 'utf8');
const manifest = JSON.parse(readFileSync(resolve(directory, 'reference-equations', 'manifest.json'), 'utf8'));
type InlineAsset = { mode?: string; tex: string; width: number; height: number; depth: number };
const inlineAssets = Object.entries(manifest.equations as Record<string, InlineAsset>).filter(([, asset]) => asset.mode === 'inline');
const laguerre = catalogue.match(/<FunctionInfo Signature="Laguerre\(n,α,x\)">([\s\S]*?)<\/FunctionInfo>/)?.[1];
const description = laguerre?.match(/<Description><!\[CDATA\[([\s\S]*?)\]\]><\/Description>/)?.[1];
if (!description || inlineAssets.length < 100) throw new Error('The shipped inline reference content is missing.');

async function openReference(page: Page, route: string, editor: string, fragment: string) {
  await page.route('**/api/autocomplete/**', request => request.fulfill({
    headers: { 'access-control-allow-origin': '*' },
    json: [{ text: 'InlineReferenceProbe(x)', menuText: 'real InlineReferenceProbe(real x)', details: {
      title: 'Generalized Laguerre polynomial', category: 'Orthogonal polynomials', type: 'SpecialFunctions', description: fragment
    } }]
  }));
  await page.goto(route);
  await page.locator(`${editor} textarea`).fill('InlineReferenceProbe');
  await page.locator(`${editor} textarea`).press('Control+Space');
  const tooltip = page.getByRole('tooltip');
  await expect(tooltip).toBeVisible();
  return tooltip;
}

for (const [route, editor] of [['/calculate', '#expressionTextArea'], ['/scripting', '#scriptTextArea']]) {
  for (const width of [360, 1280]) {
    test(`shipped Laguerre prose scales and aligns inline in ${route} at ${width}px`, async ({ page }, info) => {
      await page.setViewportSize({ width, height: 900 });
      const remote: string[] = [];
      page.on('request', request => {
        if (request.resourceType() === 'image' && !request.url().startsWith('http://127.0.0.1:4173/')) remote.push(request.url());
      });
      const tooltip = await openReference(page, route, editor, description!);
      const summary = tooltip.locator('.reference-summary');
      const images = summary.locator('img.reference-math-inline');
      await expect(images).toHaveCount(3);
      await expect(summary.locator('img[alt="x^α exp(−x)"]')).toHaveCount(1);
      await expect(summary.locator('br')).toHaveCount(0);
      await expect(tooltip.locator('.reference-signature')).toHaveCount(0);
      await expect(page.locator('.ace_autocomplete')).toContainText('real InlineReferenceProbe(real x)');
      await expect.poll(() => images.evaluateAll(nodes => nodes.every(node => (node as HTMLImageElement).complete && (node as HTMLImageElement).naturalWidth > 0))).toBe(true);
      for (const fontSize of [17, 25.5]) {
        await tooltip.locator('.computator-reference').evaluate((node: HTMLElement, size) => { node.style.fontSize = `${size}px`; }, fontSize);
        const metrics = await images.evaluateAll(nodes => nodes.map(node => {
          const image = node as HTMLImageElement;
          const marker = document.createElement('span');
          marker.style.cssText = 'display:inline-block;width:0;height:0;vertical-align:baseline';
          image.after(marker);
          const bounds = image.getBoundingClientRect();
          const style = getComputedStyle(image);
          const measured = {
            width: bounds.width, height: bounds.height,
            authoredWidth: Number(image.getAttribute('width')), authoredHeight: Number(image.getAttribute('height')),
            font: parseFloat(style.fontSize), display: style.display,
            baselineDifference: bounds.bottom - Number(image.dataset.depth) / 1000 * parseFloat(style.fontSize) - marker.getBoundingClientRect().top
          };
          marker.remove();
          return measured;
        }));
        for (const metric of metrics) {
          expect(metric.display).toBe('inline-block');
          expect(metric.font).toBe(fontSize);
          expect(Math.abs(metric.width - metric.authoredWidth / 20 * fontSize)).toBeLessThan(0.15);
          expect(Math.abs(metric.height - metric.authoredHeight / 20 * fontSize)).toBeLessThan(0.15);
          expect(Math.abs(metric.baselineDifference)).toBeLessThan(0.15);
        }
        expect(await summary.evaluate(node => node.scrollWidth <= node.clientWidth + 1)).toBe(true);
      }
      // Keep the normal text-size view as review evidence, after checking 150%.
      await tooltip.locator('.computator-reference').evaluate((node: HTMLElement) => { node.style.fontSize = ''; });
      expect(remote).toEqual([]);
      await page.screenshot({ path: info.outputPath('inline-laguerre.png') });
    });
  }
}

test('inline images retain only reconstructed bounded styles', async ({ page }) => {
  const [id, asset] = inlineAssets.find(([, equation]) => equation.tex === 'x^\\alpha\\exp(-x)')!;
  const img = (attributes: Record<string, string>) => {
    const values = { class: 'reference-math-inline', src: `reference-equations/${id}.png`, alt: 'weight',
      width: String(asset.width), height: String(asset.height), 'data-depth': String(asset.depth), ...attributes };
    return '<img ' + Object.entries(values).map(([name, value]) => `${name}="${value}"`).join(' ') + ' />';
  };
  const invalid = [
    { 'data-depth': '' }, { 'data-depth': 'NaN' }, { 'data-depth': '-1001' }, { 'data-depth': '5000' },
    { 'data-depth': '+1' }, { 'data-depth': '01' }, { 'data-depth': '1;background:red' },
    { width: '321' }, { height: '101' }, { alt: '' }, { class: 'reference-math-inline extra' },
    { src: 'data:image/svg+xml,unsafe' }, { src: 'https://reference-must-not-load.invalid/a.png' }
  ];
  const tooltip = await openReference(page, '/calculate', '#expressionTextArea', '<p>' +
    img({ style: 'position:fixed;width:999px', onerror: 'window.referenceExecuted=true' }) +
    invalid.map(attributes => img(attributes)).join('') + '</p><pre><code>Print("$x^2$ and \\(x\\)");</code></pre>');
  const images = tooltip.locator('img');
  await expect(images).toHaveCount(1);
  await expect(images).not.toHaveAttribute('onerror');
  expect(await images.evaluate(node => (node as HTMLElement).style.position)).toBe('');
  expect(await images.evaluate(node => (node as HTMLElement).style.width)).toBe(`${asset.width / 20}em`);
  await expect(tooltip.locator('pre code')).toHaveText('Print("$x^2$ and \\(x\\)");');
  expect(await page.evaluate(() => (window as unknown as { referenceExecuted?: boolean }).referenceExecuted)).toBeUndefined();
});

test('all shipped inline assets load through the actual reference sanitizer', async ({ page }) => {
  const fragment = inlineAssets.map(([id, asset], index) => `<p>Expression ${index}: <img class="reference-math-inline" src="reference-equations/${id}.png" width="${asset.width}" height="${asset.height}" data-depth="${asset.depth}" alt="Inline expression ${index}" /> in prose.</p>`).join('');
  const tooltip = await openReference(page, '/calculate', '#expressionTextArea', fragment);
  const images = tooltip.locator('img.reference-math-inline');
  await expect(images).toHaveCount(inlineAssets.length);
  await expect.poll(() => images.evaluateAll(nodes => nodes.every(node => (node as HTMLImageElement).complete && (node as HTMLImageElement).naturalWidth > 0))).toBe(true);
  const sizes = await images.evaluateAll(nodes => nodes.map(node => ({ width: (node as HTMLImageElement).naturalWidth, height: (node as HTMLImageElement).naturalHeight })));
  sizes.forEach((size, index) => {
    expect(size.width).toBe(3 * inlineAssets[index][1].width);
    expect(size.height).toBe(3 * inlineAssets[index][1].height);
  });
  // Check visible glyph ink independently of the image-box/CSS relationship.
  // These descender-free glyphs reach MathJax's baseline; allow one raster pixel
  // for their tiny outline overshoot and antialiasing at the exported 3x size.
  for (const tex of ['n', 'x']) {
    const [id, asset] = inlineAssets.find(([, equation]) => equation.tex === tex)!;
    const inkBottom = await tooltip.locator(`img.reference-math-inline[src$="${id}.png"]`).evaluate(node => {
      const image = node as HTMLImageElement;
      const canvas = document.createElement('canvas');
      canvas.width = image.naturalWidth; canvas.height = image.naturalHeight;
      const context = canvas.getContext('2d')!;
      context.drawImage(image, 0, 0);
      const pixels = context.getImageData(0, 0, canvas.width, canvas.height).data;
      let bottom = 0;
      for (let y = 0; y < canvas.height; y++) for (let x = 0; x < canvas.width; x++) {
        if (pixels[(y * canvas.width + x) * 4 + 3] > 0) bottom = y + 1;
      }
      return bottom;
    });
    expect(Math.abs(inkBottom - 3 * (asset.height - asset.depth / 50))).toBeLessThan(1);
  }
});
