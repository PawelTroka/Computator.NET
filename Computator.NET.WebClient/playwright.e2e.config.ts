import { defineConfig, devices } from '@playwright/test';
import { randomBytes } from 'node:crypto';
import { resolve } from 'node:path';

if (process.platform !== 'win32') throw new Error('Real API E2E requires Windows, not a mocked Linux substitute.');
if (!process.env.COMPUTATOR_DATA_DIRECTORY) throw new Error('Run through Cake Web-E2E or Web-CustomFunctions to isolate the user profile.');
const token = randomBytes(32).toString('hex');
const api = 'http://127.0.0.1:60997';
const ui = 'http://127.0.0.1:4174';
const apiDirectory = resolve('../artifacts/api');
const uiDirectory = resolve('../artifacts/webclient');

export default defineConfig({
  testDir: './e2e', forbidOnly: true, retries: 0, workers: 1, timeout: 60_000,
  outputDir: 'test-results/e2e',
  reporter: [['list'], ['junit', { outputFile: 'test-results/e2e.xml' }]],
  use: { baseURL: ui, trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  projects: [
    { name: 'real-chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'real-firefox', use: { ...devices['Desktop Firefox'] } }
  ],
  webServer: [
    { command: 'dotnet Computator.NET.WebApi.dll --urls ' + api, cwd: apiDirectory,
      url: api + '/api/numerical-calculations/integral/list-methods', reuseExistingServer: false, timeout: 90_000,
      env: { ExecutionAccess__Token: token, ASPNETCORE_ENVIRONMENT: 'Production' } },
    { command: 'dotnet Computator.NET.WebClient.dll --urls ' + ui, cwd: uiDirectory,
      url: ui + '/calculate', reuseExistingServer: false, timeout: 90_000,
      env: { MathematicalApi__BaseUrl: api, MathematicalApi__Token: token, ASPNETCORE_ENVIRONMENT: 'Production' } }
  ]
});
