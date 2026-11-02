import { defineConfig, devices } from '@playwright/test';
import { mkdirSync } from 'node:fs';
// The runner already has its pinned Node executable. Source hosts must see
// the tools inherited by the original IDE, including on restart.
if (process.env.COMPUTATOR_IDE_PATH) process.env.PATH = process.env.COMPUTATOR_IDE_PATH;
// Keep server startup and request-completion evidence even if a server never
// becomes ready. This is outside outputDir, which Playwright cleans at startup.
mkdirSync('test-results', { recursive: true });
export default defineConfig({
  testDir: './ide-e2e',
  outputDir: 'test-results/ide',
  forbidOnly: Boolean(process.env.CI), retries: 0, workers: 1,
  reporter: [['list'], ['junit', { outputFile: 'test-results/ide-startup.xml' }]],
  use: { baseURL: 'http://localhost:5081', trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: [
    {
      command: 'dotnet run --no-build --no-launch-profile -c Release --project ../Computator.NET.WebApi > test-results/ide-api.log 2>&1',
      url: 'http://localhost:5080/api/numerical-calculations/integral/list-methods', reuseExistingServer: false, timeout: 120_000,
      // Exercise the API's default address with the direct local IDE access mode.
      // Packaged launch requires authentication in Production, not this explicit dev mode.
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development', DOTNET_ENVIRONMENT: 'Development',
        ExecutionAccess__AllowUnauthenticatedDevelopment: 'true',
        // appsettings.json sets the Console provider to Warning. Override that
        // provider for this suite so worker 504s include request timing in logs.
        Logging__Console__LogLevel__Default: 'Information'
      }
    },
    {
      command: 'dotnet run --no-build --no-launch-profile -c Release --project . -- --urls http://localhost:5081 > test-results/ide-client.log 2>&1',
      url: 'http://localhost:5081', reuseExistingServer: false, timeout: 300_000,
      env: { ASPNETCORE_ENVIRONMENT: 'Development', DOTNET_ENVIRONMENT: 'Development' }
    }
  ]
});
