import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { readFileSync, createReadStream } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { join } from 'node:path';

// The committed equations and scientific diagrams are shared with the desktop
// package. No mathematics renderer or duplicated image catalogue runs at startup.
const referenceAssets = [
  { directory: 'reference-equations', collection: 'equations' },
  { directory: 'reference-figures', collection: 'figures' }
].map(asset => ({ ...asset, path: fileURLToPath(new URL(`../Computator.NET.Core/Static/${asset.directory}/`, import.meta.url)) }));
function offlineReference() {
  return {
    name: 'computator-offline-reference-assets',
    generateBundle(this: { emitFile(asset: { type: 'asset'; fileName: string; source: Buffer }): void }) {
      for (const asset of referenceAssets) {
        const manifest = JSON.parse(readFileSync(join(asset.path, 'manifest.json'), 'utf8'));
        this.emitFile({ type: 'asset', fileName: `${asset.directory}/NOTICE.txt`, source: readFileSync(join(asset.path, 'NOTICE.txt')) });
        for (const id of Object.keys(manifest[asset.collection])) {
          if (!/^[a-f0-9]{64}$/.test(id)) throw Error('Invalid reference asset identity');
          this.emitFile({ type: 'asset', fileName: `${asset.directory}/${id}.png`, source: readFileSync(join(asset.path, `${id}.png`)) });
        }
      }
    },
    configureServer(server: { middlewares: { use(handler: (req: import('node:http').IncomingMessage, res: import('node:http').ServerResponse, next: () => void) => void): void } }) {
      server.middlewares.use((req, res, next) => {
        const match = /^\/(reference-equations|reference-figures)\/([a-f0-9]{64})\.png$/.exec(req.url ?? '');
        if (!match) return next();
        const asset = referenceAssets.find(asset => asset.directory === match[1])!;
        const stream = createReadStream(join(asset.path, `${match[2]}.png`));
        stream.on('error', () => { res.statusCode = 404; res.end(); });
        res.setHeader('Content-Type', 'image/png'); stream.pipe(res);
      });
    }
  };
}

export default defineConfig({
  plugins: [react(), offlineReference()],
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
