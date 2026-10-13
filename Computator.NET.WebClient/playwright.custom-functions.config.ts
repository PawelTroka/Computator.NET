import { defineConfig } from '@playwright/test';
import realStack from './playwright.e2e.config';

export default defineConfig({
  ...realStack,
  testDir: './custom-functions-e2e', timeout: 90_000,
  reporter: [['list'], ['junit', { outputFile: 'test-results/custom-functions.xml' }]],
  outputDir: 'test-results/custom-functions'
});
