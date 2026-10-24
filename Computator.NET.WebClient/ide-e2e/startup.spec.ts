import { test, expect } from '@playwright/test';
import { execFileSync, spawn } from 'node:child_process';
import { once } from 'node:events';
import path from 'node:path';

test('a clean IDE-style launch builds assets and calculates through the real API', async ({ page }) => {
  test.setTimeout(90_000);
  const failures: string[] = [];
  page.on('pageerror', error => failures.push(error.message));
  const response = await page.goto('/calculate');
  expect(response?.status()).toBe(200);
  await expect(page.getByRole('heading', { name: 'Calculate', exact: true })).toBeVisible();
  await page.locator('#expressionTextArea textarea').fill('2x');
  for (const [input, result] of [['21', '42'], ['9', '18']]) {
    await page.getByRole('spinbutton', { name: 'x', exact: true }).fill(input);
    // The first actual Roslyn invocation is deliberately cold in this suite.
    // Await and inspect the real HTTP response before checking the rendered value;
    // a pending request must not be confused with a successful calculation.
    const calculation = page.waitForResponse(response => response.url().endsWith('/api/v2/execute') &&
      response.request().method() === 'POST', { timeout: 30_000 });
    await page.getByRole('button', { name: 'Calculate!' }).click();
    const reply = await calculation;
    expect(reply.ok(), await reply.text()).toBe(true);
    expect(await reply.json()).toMatchObject({ result });
    await expect(page.getByRole('status')).toHaveText(result);
  }
  await page.reload();
  await expect(page.locator('#expressionTextArea')).toBeVisible();
  expect(failures).toEqual([]);
});

test('fresh startup serves actual JS/CSS while unknown APIs remain 404', async ({ request, page }) => {
  const requests: string[] = [];
  page.on('response', response => { if (response.status() === 404) requests.push(response.url()); });
  await page.goto('/scripting');
  await expect(page.locator('#scriptTextArea')).toBeVisible();
  expect(requests).toEqual([]);
  expect((await request.get('/api/does-not-exist')).status()).toBe(404);
});

test('an IDE restart replaces assets built for another API profile', async ({ page, request }) => {
  test.setTimeout(300_000);
  // A package/real-API build replaces dist but leaves the IDE source stamp intact.
  // Starting another source host must restore the direct localhost API profile.
  execFileSync(process.execPath, ['node_modules/vite/bin/vite.js', 'build'], {
    cwd: process.cwd(), env: { ...process.env, VITE_API_BASE_URL: '/api' }, timeout: 60_000
  });
  const host = spawn('dotnet', [path.resolve('bin/Release/net10.0/Computator.NET.WebClient.dll'),
    '--contentRoot', process.cwd(), '--urls', 'http://localhost:63784'], {
    cwd: process.cwd(), windowsHide: true,
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Development', DOTNET_ENVIRONMENT: 'Development' }
  });
  let output = '';
  host.stdout.on('data', chunk => { output = (output + chunk).slice(-32_000); });
  host.stderr.on('data', chunk => { output = (output + chunk).slice(-32_000); });
  const exited = once(host, 'exit');
  try {
    await expect.poll(async () => {
      if (host.exitCode !== null) throw new Error(`Source host exited: ${output}`);
      try { return (await request.get('http://localhost:63784', { timeout: 1000 })).status(); }
      catch { return 0; }
    }, { timeout: 240_000, intervals: [500] }).toBe(200);
    await page.goto('http://localhost:63784/calculate');
    await page.locator('#expressionTextArea textarea').fill('40+2');
    const calculation = page.waitForResponse(response => response.url().endsWith('/api/v2/execute') &&
      response.request().method() === 'POST', { timeout: 30_000 });
    await page.getByRole('button', { name: 'Calculate!' }).click();
    const reply = await calculation;
    expect(reply.url()).toContain('localhost:60996/');
    expect(reply.ok(), await reply.text()).toBe(true);
    await expect(page.getByRole('status')).toHaveText('42');
  } finally {
    if (host.exitCode === null && host.pid) {
      // This suite is Windows-only; also terminate a still-running Cake child.
      execFileSync('taskkill', ['/pid', String(host.pid), '/T', '/F'], { windowsHide: true });
    }
    await exited;
  }
});
