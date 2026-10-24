import { test, expect } from '@playwright/test';

test.beforeEach(async ({ page }) => {
  await page.route('http://localhost:5080/api/**', route => route.fulfill({ json: [] }));
});

test('dark workspace retains the existing logo and expression editor', async ({ page }, info) => {
  await page.setViewportSize({ width: 1366, height: 900 });
  await page.goto('/calculate');
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  const logo = page.locator('.brand img');
  await expect(logo).toBeVisible();
  expect(await logo.evaluate((image: HTMLImageElement) => image.naturalWidth)).toBeGreaterThan(0);
  await expect(page.locator('#expressionTextArea.ace-github')).toBeVisible();
  await page.screenshot({ path: info.outputPath('dark-workspace-desktop.png'), fullPage: true });
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
});

test('mobile workspace keeps navigation keyboard-accessible and preserves TSL', async ({ page }, info) => {
  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto('/scripting');
  await expect(page.locator('#scriptTextArea.ace-ambiance')).toBeVisible();
  const toggle = page.getByRole('button', { name: 'Toggle navigation' });
  await toggle.click();
  await expect(page.getByRole('link', { name: 'Custom functions', exact: true })).toBeVisible();
  await toggle.press('Escape');
  await expect(toggle).toHaveAttribute('aria-expanded', 'false');
  await expect(toggle).toBeFocused();
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
  await page.screenshot({ path: info.outputPath('dark-workspace-mobile.png'), fullPage: true });
});

test('keyboard users can skip navigation', async ({ page }) => {
  await page.goto('/calculate');
  await page.keyboard.press('Tab');
  await expect(page.getByRole('link', { name: 'Skip to workspace' })).toBeFocused();
  await page.keyboard.press('Enter');
  await expect(page.locator('#main-content')).toBeFocused();
});
