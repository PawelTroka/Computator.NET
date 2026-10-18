import { test, expect, type Page } from '@playwright/test';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

const directory = resolve('..', 'Computator.NET.Core', 'Static');
const catalogue = readFileSync(resolve(directory, 'functions.xml'), 'utf8');
const descriptions = new Map(Array.from(catalogue.matchAll(/<FunctionInfo Signature="([^"]+)">[\s\S]*?<Description><!\[CDATA\[([\s\S]*?)\]\]><\/Description>/g),
  match => [match[1], match[2]]));
type Figure = { name: string; width: number; height: number; alt: string; caption: string };
const figures: Record<string, Figure> = JSON.parse(readFileSync(resolve(directory, 'reference-figures', 'manifest.json'), 'utf8')).figures;
const ackley = descriptions.get('Ackley(xi1,xi2, ...)');
if (!ackley) throw Error('Missing shipped Ackley article');

async function openArticle(page: Page, route: string, editor: string, description: string) {
  await page.route('**/api/autocomplete/**', request => request.fulfill({
    headers: { 'access-control-allow-origin': '*' },
    json: [{ text: 'Ackley(xi1,xi2, ...)', menuText: 'real Ackley(real xi1, real xi2, ...)', details: {
      title: 'Ackley optimization landscape', category: 'Optimization benchmarks', type: 'SpecialFunctions', description
    } }]
  }));
  await page.goto(route);
  await page.locator(`${editor} textarea`).fill('Ackley');
  await page.locator(`${editor} textarea`).press('Control+Space');
  const tooltip = page.getByRole('tooltip');
  await expect(tooltip).toBeVisible();
  return tooltip;
}

for (const [route, editor] of [['/calculate', '#expressionTextArea'], ['/scripting', '#scriptTextArea']]) {
  test(`shipped Ackley article and explanatory diagram load offline in ${route}`, async ({ page }, info) => {
    const remoteImages: string[] = [];
    page.on('request', request => {
      if (request.resourceType() === 'image' && new URL(request.url()).hostname !== '127.0.0.1') remoteImages.push(request.url());
    });
    const tooltip = await openArticle(page, route, editor, ackley!);
    const equation = tooltip.locator('.reference-equation img.reference-math');
    await expect(equation).toHaveCount(1);
    await expect.poll(() => equation.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
    const plot = tooltip.locator('img.reference-plot');
    await expect(plot).toHaveCount(1);
    const [id,asset] = Object.entries(figures).find(([,asset]) => asset.name === 'Ackley')!;
    await expect(plot).toHaveAttribute('src', `/reference-figures/${id}.png`);
    await expect(plot).toHaveAttribute('alt',asset.alt);
    await expect.poll(() => plot.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
    expect(await plot.evaluate((node: HTMLImageElement) => node.naturalWidth)).toBe(3*asset.width);
    await expect(tooltip.locator('.reference-figure')).toContainText(asset.caption);
    expect(await plot.evaluate(node => {
      const bounds=node.getBoundingClientRect(), parent=node.parentElement!.getBoundingClientRect();
      return bounds.width <= parent.width + 1 && Math.abs(bounds.width/bounds.height-600/374) < 0.01;
    })).toBe(true);
    expect(remoteImages).toEqual([]);
    await plot.scrollIntoViewIfNeeded();
    await page.screenshot({ path: info.outputPath('ackley-landscape-diagram.png') });
  });
}

test('every diagram is served as a PNG and loaded through the production allowlist', async ({ page, request }) => {
  const fragment = Object.entries(figures).map(([id,asset]) =>
    `<div class="reference-figure"><img class="reference-plot" src="reference-figures/${id}.png" width="${asset.width}" height="${asset.height}" alt="${asset.name} landscape" /></div>`).join('');
  const tooltip = await openArticle(page,'/calculate','#expressionTextArea',fragment);
  const plots = tooltip.locator('img.reference-plot');
  await expect(plots).toHaveCount(8);
  await expect.poll(() => plots.evaluateAll(nodes => nodes.every(node => (node as HTMLImageElement).complete && (node as HTMLImageElement).naturalWidth > 0))).toBe(true);
  for (const [id,asset] of Object.entries(figures)) {
    const response = await request.get(`/reference-figures/${id}.png`);
    expect(response.headers()['content-type']).toContain('image/png');
    const bytes = await response.body();
    expect(bytes.subarray(0,8).equals(Buffer.from([137,80,78,71,13,10,26,10]))).toBe(true);
    expect(bytes.readUInt32BE(16)).toBe(3*asset.width);
    expect(bytes.readUInt32BE(20)).toBe(3*asset.height);
  }
});

test('diagram support preserves the inert strict path and metadata boundary', async ({ page }) => {
  const [id,asset] = Object.entries(figures)[0];
  const image = (overrides: Record<string,string>) => '<img '+Object.entries({ class: 'reference-plot',
    src: `reference-figures/${id}.png`, width: String(asset.width), height: String(asset.height), alt: 'Diagram', ...overrides
  }).map(([name,value]) => `${name}="${value}"`).join(' ')+' />';
  const invalid = [{ src: 'https://reference-must-not-load.invalid/diagram.png' },
    { src: 'reference-figures/../settings.json' }, { src: `reference-equations/${id}.png` },
    { class: 'reference-math' }, { class: 'reference-plot extra' }, { width: '2401' }, { height: '801' },
    { 'data-depth': '0' }, { alt: '' }];
  const remote: string[] = [];
  page.on('request',request => { if (request.url().includes('reference-must-not-load')) remote.push(request.url()); });
  const tooltip = await openArticle(page,'/calculate','#expressionTextArea',image({ style:'position:fixed',onerror:'window.referenceExecuted=true' })+
    invalid.map(attributes => image(attributes)).join(''));
  await expect(tooltip.locator('img')).toHaveCount(1);
  await expect(tooltip.locator('img')).not.toHaveAttribute('style');
  await expect(tooltip.locator('img')).not.toHaveAttribute('onerror');
  expect(await page.evaluate(() => (window as unknown as { referenceExecuted?: boolean }).referenceExecuted)).toBeUndefined();
  expect(remote).toEqual([]);
});
