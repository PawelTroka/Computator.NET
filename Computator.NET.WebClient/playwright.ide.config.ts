import { defineConfig, devices } from '@playwright/test';
export default defineConfig({
  testDir: './ide-e2e',
  outputDir: 'test-results/ide',
  forbidOnly: Boolean(process.env.CI), retries: 0, workers: 1,
  reporter: [['list'], ['junit', { outputFile: 'test-results/ide-startup.xml' }]],
  use: { baseURL: 'http://localhost:63785', trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: 'dotnet run --no-build --no-launch-profile -c Release --project ../Computator.NET.WebApi -- --urls http://localhost:60996',
      url: 'http://localhost:60996/api/numerical-calculations/integral/list-methods', reuseExistingServer: false, timeout: 120_000,
      // Match the deliberately direct local IDE profile. The later protected
      // package requires authentication in Production, not this explicit dev mode.
      env: { ASPNETCORE_ENVIRONMENT: 'Development', DOTNET_ENVIRONMENT: 'Development', ExecutionAccess__AllowUnauthenticatedDevelopment: 'true' }
    },
    {
      command: 'dotnet run --no-build --no-launch-profile -c Release --project . -- --urls http://localhost:63785',
      url: 'http://localhost:63785', reuseExistingServer: false, timeout: 300_000,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development', DOTNET_ENVIRONMENT: 'Development',
        PATH: process.env.COMPUTATOR_IDE_PATH ?? process.env.PATH ?? ''
      }
    }
  ]
});
