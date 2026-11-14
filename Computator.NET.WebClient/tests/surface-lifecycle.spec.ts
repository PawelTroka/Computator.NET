import { test, expect, type Page } from '@playwright/test';

const axis = Array.from({ length: 9 }, (_, index) => index - 4);
const grid = { kind: 'surface', x: axis, y: axis,
  z: axis.map(y => axis.map(x => x + y)), finiteSamples: 81, missingSamples: 0 };
const headers = { 'access-control-allow-origin': '*' };

type Pending = { node: HTMLElement; resolve: (value?: unknown) => void; reject: (error: Error) => void };
type Renderer = { plots: Pending[]; cameras: Pending[]; exports: Pending[]; downloads: string[] };
declare global { interface Window { surfaceLifecycle: Renderer } }

// Hold the actual component's renderer operations at deterministic boundaries.
// No timing assumptions or GPU are needed to exercise React/Plotly ownership;
// chart-surface.spec.ts separately covers the real WebGL renderer and PNG bytes.
test.beforeEach(async ({ page }) => {
  await page.addInitScript(() => {
    const getContext = HTMLCanvasElement.prototype.getContext;
    HTMLCanvasElement.prototype.getContext = function (kind: string, ...options: unknown[]) {
      if (kind === 'webgl' || kind === 'webgl2') return {};
      return Reflect.apply(getContext, this, [kind, ...options]);
    } as typeof getContext;
    window.surfaceLifecycle = { plots: [], cameras: [], exports: [], downloads: [] };
    const click = HTMLAnchorElement.prototype.click;
    HTMLAnchorElement.prototype.click = function () {
      if (this.download === 'computator-chart.png') window.surfaceLifecycle.downloads.push(this.href);
      else click.call(this);
    };
  });
  await page.route('**/api/autocomplete/**', route => route.fulfill({ json: [], headers }));
  await page.route('**/api/v2/chart', route => route.fulfill({ json: grid, headers }));
  await page.route('**/assets/plotly-gl3d.min-*.js', route => route.fulfill({
    contentType: 'text/javascript', body: `
      const state = window.surfaceLifecycle;
      const pending = (items, node) => new Promise((resolve, reject) => items.push({ node, resolve, reject }));
      export default {
        newPlot(node) {
          node.dataset.renderedSurface = String(state.plots.length + 1);
          node.textContent = 'Surface ' + node.dataset.renderedSurface;
          node.on = () => {};
          return pending(state.plots, node);
        },
        purge(node) { delete node.dataset.renderedSurface; node.replaceChildren(); },
        relayout(node) { return pending(state.cameras, node); },
        toImage(node) { return pending(state.exports, node); },
        Plots: { resize() { return Promise.resolve(); } }
      };`
  }));
});

async function draw(page: Page, index: number) {
  const input = page.locator('#expressionTextArea textarea');
  await input.focus(); await input.press('ControlOrMeta+A');
  await page.keyboard.insertText(`x+y+${index}`);
  await page.getByRole('button', { name: 'Draw chart!', exact: true }).click();
  await expect.poll(() => page.evaluate(() => window.surfaceLifecycle.plots.length)).toBe(index);
}

async function complete(page: Page, type: 'plots' | 'cameras' | 'exports', index: number, error?: string) {
  await page.evaluate(async ({ type, index, error }) => {
    const action = window.surfaceLifecycle[type][index - 1];
    if (error) action.reject(new Error(error));
    else action.resolve(type === 'exports' ? 'data:image/png;base64,c3VyZmFjZQ==' : undefined);
    // Run the promise continuation and its React update before negative assertions.
    await new Promise<void>(resolve => requestAnimationFrame(() => resolve()));
  }, { type, index, error });
}

async function ready(page: Page, index: number) {
  await complete(page, 'plots', index);
  await expect(page.getByText('Interactive surface ready.', { exact: false })).toBeVisible();
  await expect(page.getByTestId('surface-chart')).toHaveAttribute('data-rendered-surface', String(index));
}

test('late initialization of a replaced surface cannot erase the ready replacement', async ({ page }) => {
  await page.goto('/');
  await draw(page, 1);
  await draw(page, 2);
  await ready(page, 2);
  await complete(page, 'plots', 1);
  await expect(page.getByTestId('surface-chart')).toHaveAttribute('data-rendered-surface', '2');
  await expect(page.getByTestId('surface-chart')).toHaveText('Surface 2');
  await expect(page.getByRole('button', { name: 'Rotate left' })).toBeEnabled();
  await expect(page.getByRole('alert')).toHaveCount(0);
});

test('late initialization failure belongs to the removed surface', async ({ page }) => {
  await page.goto('/');
  await draw(page, 1);
  await draw(page, 2);
  await ready(page, 2);
  await complete(page, 'plots', 1, 'Old renderer failed.');
  await expect(page.getByTestId('surface-chart')).toHaveAttribute('data-rendered-surface', '2');
  await expect(page.getByRole('alert')).toHaveCount(0);
});

test('clearing during initialization removes the old renderer and permits another draw', async ({ page }) => {
  await page.goto('/');
  await draw(page, 1);
  await page.getByRole('button', { name: 'Clear chart', exact: true }).click();
  await expect(page.getByTestId('surface-chart')).toHaveCount(0);
  await complete(page, 'plots', 1);
  await expect(page.getByTestId('surface-chart')).toHaveCount(0);
  expect(await page.evaluate(() => window.surfaceLifecycle.plots[0].node.isConnected)).toBe(false);
  await draw(page, 2);
  await ready(page, 2);
});

test('a late camera failure cannot add an error to a replacement surface', async ({ page }) => {
  await page.goto('/');
  await draw(page, 1); await ready(page, 1);
  await page.getByRole('button', { name: 'Rotate left' }).click();
  await expect.poll(() => page.evaluate(() => window.surfaceLifecycle.cameras.length)).toBe(1);
  await draw(page, 2); await ready(page, 2);
  await complete(page, 'cameras', 1, 'Old camera failed.');
  await expect(page.getByRole('alert')).toHaveCount(0);
  await expect(page.getByTestId('surface-chart')).toHaveAttribute('data-rendered-surface', '2');
});

test('a replaced surface cannot download its stale export or block the new export', async ({ page }) => {
  await page.goto('/');
  await draw(page, 1); await ready(page, 1);
  await page.getByRole('button', { name: 'Download PNG', exact: true }).click();
  await expect.poll(() => page.evaluate(() => window.surfaceLifecycle.exports.length)).toBe(1);
  await draw(page, 2); await ready(page, 2);
  await complete(page, 'exports', 1);
  expect(await page.evaluate(() => window.surfaceLifecycle.downloads)).toEqual([]);
  await page.getByRole('button', { name: 'Download PNG', exact: true }).click();
  await expect.poll(() => page.evaluate(() => window.surfaceLifecycle.exports.length)).toBe(2);
  await complete(page, 'exports', 2);
  expect(await page.evaluate(() => window.surfaceLifecycle.downloads)).toHaveLength(1);
});

test('route departure cancels publication of an in-flight surface export', async ({ page }) => {
  await page.goto('/');
  await draw(page, 1); await ready(page, 1);
  await page.getByRole('button', { name: 'Download PNG', exact: true }).click();
  await expect.poll(() => page.evaluate(() => window.surfaceLifecycle.exports.length)).toBe(1);
  await page.getByRole('link', { name: 'Calculate', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await complete(page, 'exports', 1);
  expect(await page.evaluate(() => window.surfaceLifecycle.downloads)).toEqual([]);
});
