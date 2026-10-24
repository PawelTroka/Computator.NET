import { test, expect, type Page } from '@playwright/test';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { createHash } from 'node:crypto';

const directory = resolve('..', 'Computator.NET.Core', 'Static');
const catalogue = readFileSync(resolve(directory, 'functions.xml'), 'utf8');
type Figure = { name: string; sha256: string; width: number; height: number; alt: string; caption: string };
const figures = JSON.parse(readFileSync(resolve(directory, 'reference-figures', 'manifest.json'), 'utf8')).figures as Record<string, Figure>;
const examples = [['sin(x)', 'UnitCircle'], ['Normal.CDF(mean,stddev,x)', 'StandardNormal']];

function shippedArticle(signature: string) {
  const escaped = signature.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const entry = catalogue.match(new RegExp(`<FunctionInfo Signature="${escaped}">([\\s\\S]*?)<\\/FunctionInfo>`))?.[1];
  const title = entry?.match(/<Title>([\s\S]*?)<\/Title>/)?.[1];
  const description = entry?.match(/<Description><!\[CDATA\[([\s\S]*?)\]\]><\/Description>/)?.[1];
  if (!title || !description) throw Error('Missing shipped reference: ' + signature);
  return { title, description };
}

async function openArticle(page: Page, route: string, editor: string, signature: string) {
  await page.route('**/api/autocomplete/**', request => request.fulfill({
    headers: { 'access-control-allow-origin': '*' },
    json: [{ text: 'ConceptReferenceProbe(x)', menuText: 'real ConceptReferenceProbe(real x)', details: {
      ...shippedArticle(signature), type: 'ElementaryFunctions'
    } }]
  }));
  await page.goto(route);
  await page.locator(`${editor} textarea`).fill('ConceptReferenceProbe');
  await page.locator(`${editor} textarea`).press('Control+Space');
  const tooltip = page.getByRole('tooltip');
  await expect(tooltip).toBeVisible();
  return tooltip;
}

for (const [route, editor] of [['/calculate', '#expressionTextArea'], ['/scripting', '#scriptTextArea']]) {
  for (const width of [360, 1280]) {
    for (const [signature, name] of examples) {
      test(`${name} remains readable in ${route} at ${width}px`, async ({ page }, info) => {
        await page.setViewportSize({ width, height: 900 });
        const remote: string[] = [];
        page.on('request', request => {
          if (request.resourceType() === 'image' && new URL(request.url()).hostname !== '127.0.0.1') remote.push(request.url());
        });
        const tooltip = await openArticle(page, route, editor, signature);
        const [id, figure] = Object.entries(figures).find(([, value]) => value.name === name)!;
        const image = tooltip.locator('img.reference-plot');
        await expect(image).toHaveCount(1);
        await expect(image).toHaveAttribute('src', `/reference-figures/${id}.png`);
        await expect(image).toHaveAttribute('alt', figure.alt);
        await expect(tooltip.locator('.reference-figure')).toContainText(figure.caption);
        await expect.poll(() => image.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth)).toBe(3 * figure.width);
        await expect(tooltip.locator('.reference-signature')).toHaveCount(0);
        await image.scrollIntoViewIfNeeded();
        const size = await image.evaluate((node: HTMLImageElement) => {
          const bounds = node.getBoundingClientRect();
          return { left: bounds.left, right: bounds.right, width: bounds.width, height: bounds.height,
            available: node.parentElement!.clientWidth, naturalHeight: node.naturalHeight };
        });
        expect(size.left).toBeGreaterThanOrEqual(0);
        expect(size.right).toBeLessThanOrEqual(width);
        expect(size.width).toBeLessThanOrEqual(size.available + 1);
        expect(Math.abs(size.width / size.height - figure.width / figure.height)).toBeLessThan(0.01);
        expect(size.naturalHeight).toBe(3 * figure.height);
        expect(await tooltip.locator('.reference-figure').evaluate(node => node.scrollWidth <= node.clientWidth + 1)).toBe(true);
        await page.screenshot({ path: info.outputPath(`${name.toLowerCase()}-${width}.png`) });
        const source = tooltip.locator('.reference-source a').last();
        await source.scrollIntoViewIfNeeded();
        await expect(source).toBeInViewport();
        await expect(source).toHaveAttribute('href', /^https:\/\//);
        expect(remote).toEqual([]);
      });
    }
  }
}

test('concept diagrams are served with their exact reviewed PNG hashes', async ({ request }) => {
  for (const [, name] of examples) {
    const [id, figure] = Object.entries(figures).find(([, value]) => value.name === name)!;
    const response = await request.get(`/reference-figures/${id}.png`);
    expect(response.ok()).toBeTruthy();
    expect(response.headers()['content-type']).toContain('image/png');
    const bytes = await response.body();
    expect(createHash('sha256').update(bytes).digest('hex')).toBe(figure.sha256);
    expect(bytes.readUInt32BE(16)).toBe(3 * figure.width);
    expect(bytes.readUInt32BE(20)).toBe(3 * figure.height);
  }
});

test('an unopened normal article does not fetch its diagram while sine is selected', async ({ page }) => {
  const requested: string[] = [];
  page.on('request', request => {
    const path = new URL(request.url()).pathname;
    if (path.startsWith('/reference-figures/')) requested.push(path);
  });
  await page.route('**/api/autocomplete/**', request => request.fulfill({
    headers: { 'access-control-allow-origin': '*' },
    json: [['SelectedFigureProbe', 'sin(x)'], ['UnopenedFigureProbe', 'Normal.CDF(mean,stddev,x)']]
      .map(([name, signature]) => ({ text: `${name}(x)`, menuText: `real ${name}(real x)`, details: {
        ...shippedArticle(signature), type: 'ElementaryFunctions'
      } }))
  }));
  await page.goto('/calculate');
  const input = page.locator('#expressionTextArea textarea');
  await input.fill('SelectedFigureProbe');
  await input.press('Control+Space');
  const image = page.getByRole('tooltip').locator('img.reference-plot');
  await expect.poll(() => image.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
  const [id] = Object.entries(figures).find(([, value]) => value.name === 'UnitCircle')!;
  expect([...new Set(requested)]).toEqual([`/reference-figures/${id}.png`]);
});
