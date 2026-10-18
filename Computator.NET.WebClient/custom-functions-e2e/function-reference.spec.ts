import { test, expect } from '@playwright/test';

interface Completion {
  text: string;
  menuText: string;
  details: { title: string; type: string; description: string };
}

const articles = [
  { name: 'constant', input: 'ElementaryCharge', signature: 'ElementaryCharge', type: 'PhysicalConstants',
    invocations: ['ElementaryCharge'] },
  { name: 'function', input: 'Ai', signature: 'Ai(x)', type: 'SpecialFunctions',
    invocations: ['Ai(real x)'] },
  { name: 'overloads', input: 'ConicalP', signature: 'ConicalP(μ,λ,x)', type: 'SpecialFunctions',
    invocations: ['ConicalP(integer μ, real λ, real x)', 'ConicalP(real μ, real λ, real x)'] }
];

for (const [mode, route, editor] of [
  ['expression', '/calculate', '#expressionTextArea'],
  ['scripting', '/scripting', '#scriptTextArea']
]) {
  test(`the live ${mode} reference preserves teaching content and loads only packaged equations`, async ({ page, baseURL }, info) => {
    // No route interception: use the published frontend and actual Windows API.
    const frontendOrigin = new URL(baseURL!).origin;
    const remoteImages: string[] = [];
    page.on('request', request => {
      if (request.resourceType() === 'image' && new URL(request.url()).origin !== frontendOrigin)
        remoteImages.push(request.url());
    });
    const loaded = page.waitForResponse(response => response.url().endsWith(`/autocomplete/${mode}`) && response.request().method() === 'GET');
    await page.goto(route);
    const response = await loaded;
    expect(response.ok()).toBeTruthy();
    const items = await response.json() as Completion[];

    for (const article of articles) {
      await test.step(article.name, async () => {
        const matches = items.filter(item => item.text === article.signature);
        // The menus still expose typed overloads. Descriptions use the same
        // shipped article, with the redundant signature block hidden by the UI.
        const captions = article.invocations.map(invocation => mode === 'scripting' ? `real ${invocation}` : invocation);
        expect(matches.map(item => item.menuText).sort()).toEqual([...captions].sort());
        const entry = matches[0];
        expect(entry.details.type).toBe(article.type);
        expect(entry.details.description).toContain('reference-summary');
        expect(entry.details.description).toContain('reference-math');
        for (const invocation of article.invocations)
          expect(entry.details.description).toContain(`<code>real ${invocation}</code>`);

        const input = page.locator(`${editor} textarea`);
        await input.focus();
        await input.press('Escape');
        // Ace's textarea is an input buffer. Replace the document through its
        // keyboard selection command before switching to the next article.
        await input.press('ControlOrMeta+A');
        await page.keyboard.insertText(article.input);
        await input.press('Control+Space');
        for (const caption of captions)
          await expect(page.locator('.ace_autocomplete')).toContainText(caption);
        const tooltip = page.getByRole('tooltip');
        await expect(tooltip.locator('.reference-title')).toHaveText(entry.details.title);
        await expect(tooltip.locator('.reference-signature')).toHaveCount(0);
        for (const invocation of article.invocations)
          await expect(tooltip).not.toContainText(`real ${invocation}`);
        await expect(tooltip).not.toContainText('Syntax');
        await expect(tooltip).not.toContainText('<div');
        await expect(tooltip.locator('.reference-explanation').first()).toBeVisible();
        await expect(tooltip.locator('.reference-arguments strong')).toBeVisible();
        await expect(tooltip.locator('.reference-source a[href^="https://"]')).toHaveCount(1);

        if (article.name === 'constant') {
          await expect(tooltip).toContainText('1.602176634');
          await expect(tooltip.locator('.reference-arguments')).toContainText('Stored application value: 1.602176565E-19.');
        } else if (article.name === 'function') {
          await expect(tooltip.locator('img.reference-math').first()).toHaveAttribute('alt', /y″−xy=0/);
          await expect(tooltip.locator('.reference-arguments')).toContainText('Real x');
        } else {
          // Removing both typed signature lines must preserve the distinct
          // supported orders and parameter restrictions in the authored prose.
          await expect(tooltip.locator('.reference-arguments')).toContainText('x>−1');
          await expect(tooltip.locator('.reference-note')).toContainText('Integer μ accepts only 0 and 1.');
          await expect(tooltip.locator('.reference-note')).toContainText('Real μ allows ±0.5 as well');
        }

        const equations = tooltip.locator('img.reference-math, img.reference-math-inline');
        expect(await equations.count()).toBeGreaterThan(0);
        for (const equation of await equations.all()) {
          await expect(equation).toBeVisible();
          await expect(equation).toHaveAttribute('src', /^\/reference-equations\/[a-f0-9]{64}\.png$/);
          await expect.poll(() => equation.evaluate((node: HTMLImageElement) =>
            node.complete && node.naturalWidth === 3 * Number(node.getAttribute('width')) &&
            node.naturalHeight === 3 * Number(node.getAttribute('height')))).toBe(true);
          const imageResponse = await page.request.get((await equation.getAttribute('src'))!);
          expect(imageResponse.ok()).toBeTruthy();
          expect(imageResponse.headers()['content-type']).toContain('image/png');
          expect((await imageResponse.body()).subarray(0, 8).equals(Buffer.from([137, 80, 78, 71, 13, 10, 26, 10]))).toBe(true);
        }
        for (const inline of await tooltip.locator('.reference-inline').all()) {
          const image = inline.locator('img');
          await expect(image).toHaveAttribute('aria-hidden', 'true');
          await expect(image).toHaveAttribute('alt', '');
          expect((await inline.locator('.reference-math-text').textContent())!.trim()).not.toBe('');
        }
        expect(remoteImages).toEqual([]);
        await page.screenshot({ path: info.outputPath(`live-${mode}-${article.name}-reference.png`) });
      });
    }
  });
}
