import { defineConfig, devices } from '@playwright/test';
export default defineConfig({
  outputDir: 'test-results/local-package',
  testDir: './local-package-e2e', forbidOnly: Boolean(process.env.CI), retries: 0, workers: 1,
  timeout: 60_000, expect: { timeout: 15_000 },
  reporter: [['list'], ['junit', { outputFile: 'test-results/local-package.xml' }]],
  use: { baseURL: 'http://127.0.0.1:4188', trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }, { name: 'firefox', use: { ...devices['Desktop Firefox'] } }],
  webServer: {
    // 4190 is reserved for Sieve and blocked by Firefox. Use a browser-safe test
    // port rather than weakening the browser's protected-port configuration.
    command: 'cd ../artifacts/local-web-review/webclient && Computator.NET.WebClient.exe --urls http://127.0.0.1:4188 --LocalWeb:OpenBrowser=false --LocalWeb:ApiPort=60999',
    url: 'http://127.0.0.1:4188', reuseExistingServer: false, timeout: 90_000,
    env: { ASPNETCORE_ENVIRONMENT: 'Production', DOTNET_ENVIRONMENT: 'Production' }
  }
});
