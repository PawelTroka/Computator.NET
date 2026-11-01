import { test, expect, type Page } from '@playwright/test';
import { execFileSync, spawn } from 'node:child_process';
import { once } from 'node:events';
import { writeFile } from 'node:fs/promises';
import path from 'node:path';

async function calculate(page: Page, result: string) {
  // The worker has a 30-second execution deadline. Allow its bounded failure
  // response to arrive, then reject it explicitly instead of hiding a 504 behind
  // a browser timeout. The first calculation remains cold and is never retried.
  const [reply] = await Promise.all([
    page.waitForResponse(response => {
      const url = new URL(response.url());
      const request = response.request();
      return url.origin === 'http://localhost:5080' && url.pathname === '/api/v2/execute' &&
        request.method() === 'POST' && request.postDataJSON()?.operation === 'calculate';
    }, { timeout: 35_000 }),
    page.getByRole('button', { name: 'Calculate!', exact: true }).click()
  ]);
  expect(reply.status(), await reply.text()).toBe(200);
  expect(await reply.json()).toMatchObject({ result });
  await expect(page.getByRole('status')).toHaveText(result);
}

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
    await calculate(page, result);
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

test('an IDE restart replaces assets built for another API profile', async ({ page, request }, testInfo) => {
  test.setTimeout(300_000);
  // A package/real-API build replaces dist but leaves the IDE source stamp intact.
  // Starting another source host must restore the direct localhost API profile.
  execFileSync(process.execPath, ['node_modules/vite/bin/vite.js', 'build'], {
    cwd: process.cwd(), env: { ...process.env, VITE_API_BASE_URL: '/api' }, timeout: 60_000
  });
  const host = spawn('dotnet', [path.resolve('bin/Release/net10.0/Computator.NET.WebClient.dll'),
    '--contentRoot', process.cwd(), '--urls', 'http://localhost:5082'], {
    cwd: process.cwd(), windowsHide: true,
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Development', DOTNET_ENVIRONMENT: 'Development' }
  });
  let output = '';
  host.stdout.on('data', chunk => { output = (output + chunk).slice(-32_000); });
  host.stderr.on('data', chunk => { output = (output + chunk).slice(-32_000); });
  // 'close' follows process exit and drains both redirected streams.
  const closed = once(host, 'close');
  try {
    await expect.poll(async () => {
      if (host.exitCode !== null) throw new Error(`Source host exited: ${output}`);
      try { return (await request.get('http://localhost:5082', { timeout: 1000 })).status(); }
      catch { return 0; }
    }, { timeout: 240_000, intervals: [500] }).toBe(200);
    await page.goto('http://localhost:5082/calculate');
    await page.locator('#expressionTextArea textarea').fill('40+2');
    await calculate(page, '42');
  } finally {
    try {
      if (host.exitCode === null && host.pid) {
        // This suite is Windows-only; also terminate a still-running Cake child.
        execFileSync('taskkill', ['/pid', String(host.pid), '/T', '/F'], { windowsHide: true });
      }
      await closed;
    } finally {
      // The list/JUnit reporters do not persist in-memory attachments on a
      // passing test. Keep a file in outputDir for the workflow artifact too.
      const log = testInfo.outputPath('restarted-host.log');
      await writeFile(log, output, 'utf8');
      await testInfo.attach('restarted-host.log', { path: log, contentType: 'text/plain' });
    }
  }
});
