import { test, expect } from '@playwright/test';

test('Cancel execution interrupts the real request and the next script succeeds', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  await page.goto('/scripting');
  const input = page.locator('#scriptTextArea textarea');
  await input.focus();
  await input.press('ControlOrMeta+A');
  await page.keyboard.insertText('while(true) {}');
  const submitted = page.waitForRequest(request => request.method() === 'POST' && request.url().endsWith('/v2/execute'));
  await page.getByRole('button', { name: 'Process script' }).click();
  await submitted;
  await page.getByRole('button', { name: 'Cancel execution', exact: true }).click();
  await expect(page.getByRole('alert')).toHaveText('Execution cancelled.');
  await expect(page.getByRole('button', { name: 'Process script' })).toBeEnabled();
  await input.focus();
  await input.press('ControlOrMeta+A');
  await page.keyboard.insertText('write("zażółć π");');
  await page.getByRole('button', { name: 'Process script' }).click();
  await expect(page.getByRole('status')).toHaveText('zażółć π');
  await expect(page.getByRole('alert')).toHaveCount(0);
  expect(errors).toEqual([]);
});
