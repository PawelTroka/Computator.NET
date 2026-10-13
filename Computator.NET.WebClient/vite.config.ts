import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  publicDir: 'wwwroot',
  build: { outDir: 'dist', emptyOutDir: true, target: 'es2022' },
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api/SampleData': 'http://localhost:63785',
      '/api': 'http://localhost:60996'
    }
  }
});
