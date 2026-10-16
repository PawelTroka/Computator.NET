import { test, expect } from '@playwright/test';

test('the live API supplies the shipped constant reference to the existing editor', async ({ page }, info) => {
  // No route interception: use the published frontend and actual Windows API.
  const loaded = page.waitForResponse(response => response.url().endsWith('/autocomplete/expression') && response.request().method() === 'GET');
  await page.goto('/calculate');
  const response = await loaded;
  expect(response.ok()).toBeTruthy();
  const items = await response.json() as { text: string; details: { title: string; type: string; description: string } }[];
  const charge = items.find(item => item.text === 'ElementaryCharge');
  expect(charge).toBeDefined();
  expect(charge!.details.type).toBe('PhysicalConstants');
  expect(charge!.details.description).toContain('reference-summary');
  expect(charge!.details.description).toContain('Stored application value:');
  expect(charge!.details.description).toContain('1.602176634');
  expect(charge!.details.description).toContain('reference-math');
  await page.locator('#expressionTextArea textarea').fill('ElementaryCharge');
  await page.locator('#expressionTextArea textarea').press('Control+Space');
  await expect(page.locator('.ace_autocomplete')).toContainText('ElementaryCharge');
  const tooltip = page.getByRole('tooltip');
  await expect(tooltip).toContainText('Elementary charge');
  await expect(tooltip).toContainText('1.602176634');
  await expect(tooltip).toContainText('Stored application value:');
  await expect(tooltip).not.toContainText('<div');
  await expect(tooltip).not.toContainText('Syntax');
  await expect(tooltip.locator('img.reference-math').first()).toBeVisible();
  await expect.poll(() => tooltip.locator('img.reference-math').first().evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
  await expect(tooltip.locator('.reference-arguments strong')).toBeVisible();
  await expect(tooltip.locator('.reference-source a[href^="https://"]')).toHaveCount(1);
  await page.screenshot({ path: info.outputPath('live-constant-tooltip.png') });
});

test('the live function reference renders typed syntax and a mathematical definition as HTML', async ({ page }, info) => {
  const loaded = page.waitForResponse(response => response.url().endsWith('/autocomplete/expression') && response.request().method() === 'GET');
  await page.goto('/calculate'); await loaded;
  await page.locator('#expressionTextArea textarea').fill('Ai');
  await page.locator('#expressionTextArea textarea').press('Control+Space');
  const tooltip = page.getByRole('tooltip');
  await expect(tooltip.locator('.reference-signature code')).toContainText('Ai(real x)');
  const equation = tooltip.locator('img.reference-math').first();
  await expect(equation).toHaveAttribute('alt', /y″−xy=0/);
  await expect.poll(() => equation.evaluate((node: HTMLImageElement) => node.complete && node.naturalWidth > 0)).toBe(true);
  await expect(tooltip.locator('.reference-explanation').first()).toBeVisible();
  await expect(tooltip.locator('.reference-source a[href^="https://"]')).toHaveCount(1);
  await page.screenshot({ path: info.outputPath('live-function-tooltip.png') });
});
