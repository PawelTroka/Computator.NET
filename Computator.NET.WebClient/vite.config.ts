import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { readFileSync, createReadStream } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';

// One committed set of equations is shared with the desktop package. Do not
// fetch/render math at startup or keep a second, easily stale image catalogue.
const equations = fileURLToPath(new URL('../Computator.NET.Core/Static/reference-equations/', import.meta.url));
function referenceEquations() {
  return {
    name: 'computator-offline-reference-equations',
    generateBundle(this: { emitFile(asset: { type: 'asset'; fileName: string; source: Buffer }): void }) {
      const manifest = JSON.parse(readFileSync(join(equations, 'manifest.json'), 'utf8'));
      this.emitFile({ type: 'asset', fileName: 'reference-equations/NOTICE.txt', source: readFileSync(join(equations, 'NOTICE.txt')) });
      for (const id of Object.keys(manifest.equations)) {
        if (!/^[a-f0-9]{64}$/.test(id)) throw Error('Invalid equation asset identity');
        this.emitFile({ type: 'asset', fileName: `reference-equations/${id}.png`, source: readFileSync(join(equations, `${id}.png`)) });
      }
    },
    configureServer(server: { middlewares: { use(handler: (req: import('node:http').IncomingMessage, res: import('node:http').ServerResponse, next: () => void) => void): void } }) {
      server.middlewares.use((req, res, next) => {
        const match = /^\/reference-equations\/([a-f0-9]{64})\.png$/.exec(req.url ?? '');
        if (!match) return next();
        const stream = createReadStream(join(equations, `${match[1]}.png`));
        stream.on('error', () => { res.statusCode = 404; res.end(); });
        res.setHeader('Content-Type', 'image/png'); stream.pipe(res);
      });
    }
  };
}

export default defineConfig({
  plugins: [react(), referenceEquations()],
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
