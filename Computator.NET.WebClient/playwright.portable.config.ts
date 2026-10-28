import { defineConfig, devices } from '@playwright/test';
import { resolve } from 'node:path';

// The separate directory keeps real portable tests out of the existing mocked
// frontend suite and keeps its Windows-only real-stack acceptance unchanged.
const published = process.env.COMPUTATOR_PORTABLE_PUBLISH_DIRECTORY;
if (!published) throw new Error('Set COMPUTATOR_PORTABLE_PUBLISH_DIRECTORY to a built portable publication.');
const dotnet = process.env.COMPUTATOR_DOTNET ?? 'dotnet';
const directory = resolve(published);
const baseURL = 'http://127.0.0.1:5099';

export default defineConfig({
  testDir: './portable-tests',
  outputDir: 'test-results/portable-local',
  forbidOnly: true,
  retries: 0,
  workers: 1,
  timeout: 90_000,
  expect: { timeout: 10_000 },
  reporter: [['list'], ['junit', { outputFile: 'test-results/portable-local.xml' }]],
  use: { baseURL, headless: true, trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  projects: [
    { name: 'portable-chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'portable-webkit', use: { ...devices['Desktop Safari'] } }
  ],
  webServer: {
    command: `"${dotnet}" Computator.NET.PortableWeb.dll --port 5099 --execution-timeout-seconds 30`,
    cwd: directory,
    url: `${baseURL}/api/portable/status`,
    reuseExistingServer: false,
    timeout: 60_000,
    env: { ASPNETCORE_ENVIRONMENT: 'Production', DOTNET_ENVIRONMENT: 'Production' }
  }
});
