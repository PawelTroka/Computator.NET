import { defineConfig, devices } from '@playwright/test';
import { resolve } from 'node:path';
if (process.platform !== 'win32' || !process.env.COMPUTATOR_DATA_DIRECTORY) throw new Error('Run Cake Web-CustomFunctions on Windows with an isolated profile.');
export default defineConfig({
  testDir: './custom-functions-e2e', forbidOnly: true, retries: 0, workers: 1, timeout: 90_000,
  reporter: [['list'], ['junit', { outputFile: 'test-results/custom-functions.xml' }]],
  outputDir: 'test-results/custom-functions',
  use: { baseURL: 'http://127.0.0.1:5173', trace: 'retain-on-failure' },
  projects: [{ name: 'chromium', use: devices['Desktop Chrome'] }, { name: 'firefox', use: devices['Desktop Firefox'] }],
  webServer: [
    { command: 'dotnet Computator.NET.WebApi.dll --urls http://localhost:60996', cwd: resolve('../artifacts/api'),
      url: 'http://localhost:60996/api/numerical-calculations/integral/list-methods', reuseExistingServer: false, timeout: 90_000 },
    { command: 'dotnet Computator.NET.WebClient.dll --urls http://127.0.0.1:5173', cwd: resolve('../artifacts/webclient'),
      url: 'http://127.0.0.1:5173/custom-functions', reuseExistingServer: false, timeout: 90_000 }
  ]
});
