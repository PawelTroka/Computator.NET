// Editorial-only TeX -> MathJax SVG -> high-resolution PNG. The application
// ships the PNGs; no browser JavaScript, web fonts or network math service runs.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { createRequire } = require('node:module');
const dependencies = createRequire(path.resolve(__dirname, '../../Computator.NET.WebClient/package.json'));
const { mathjax } = dependencies('@mathjax/src/js/mathjax.js');
const { TeX } = dependencies('@mathjax/src/js/input/tex.js');
const { SVG } = dependencies('@mathjax/src/js/output/svg.js');
const { liteAdaptor } = dependencies('@mathjax/src/js/adaptors/liteAdaptor.js');
const { RegisterHTMLHandler } = dependencies('@mathjax/src/js/handlers/html.js');
dependencies('@mathjax/src/js/util/asyncLoad/node.js');
dependencies('@mathjax/src/js/input/tex/base/BaseConfiguration.js');
dependencies('@mathjax/src/js/input/tex/ams/AmsConfiguration.js');
const { Resvg } = dependencies('@resvg/resvg-js');
const hash = value => crypto.createHash('sha256').update(value).digest('hex');
const args = process.argv.slice(2);
const input = args[args.indexOf('--input') + 1];
const output = args[args.indexOf('--output') + 1];
const check = args.includes('--check');
if (!args.includes('--input') || !args.includes('--output')) throw Error('Expected --input JSON --output directory [--check]');

(async () => {
  if (mathjax.version !== '4.1.3') throw Error(`Expected reviewed MathJax 4.1.3, got ${mathjax.version}`);
  const equations = JSON.parse(fs.readFileSync(input, 'utf8'));
  const adaptor = liteAdaptor({ fontSize: 20 });
  RegisterHTMLHandler(adaptor);
  const svg = new SVG({ fontCache: 'none', mtextInheritFont: false });
  const document = mathjax.document('', { InputJax: new TeX({ packages: ['base', 'ams'],
    formatError(_jax, error) { throw Error(error.message); } }), OutputJax: svg });
  const manifest = { schema: 1, renderer: 'MathJax 4.1.3 / resvg 2.6.2; 20px math, 3x pixels', equations: {} };
  if (!check) fs.mkdirSync(output, { recursive: true });
  for (const [id, tex] of Object.entries(equations).sort(([a], [b]) => a.localeCompare(b))) {
    if (id !== hash(tex) || tex.length > 6000) throw Error(`Invalid equation identity/length: ${id}`);
    if (/\\(?:href|url|style|class|cssId|includegraphics|require|html)\b/.test(tex)) throw Error(`Active TeX command: ${id}`);
    try {
      const result = await document.convertPromise(tex, { display: true, em: 20, ex: 10, containerWidth: 560 });
      const node = adaptor.firstChild(result);
      const box = adaptor.getAttribute(node, 'viewBox').split(/\s+/).map(Number);
      if (box.length !== 4 || box.some(n => !Number.isFinite(n)) || box[2] <= 0 || box[3] <= 0) throw Error('Invalid SVG box');
      const [x, y, w, h] = box;
      const width = Math.ceil((w + 400) / 50), height = Math.ceil((h + 400) / 50);
      if (width > 2400 || height > 800) throw Error('Equation needs editorial line breaks');
      adaptor.setAttribute(node, 'viewBox', `${x - 200} ${y - 200} ${w + 400} ${h + 400}`);
      adaptor.setAttribute(node, 'width', `${width}`); adaptor.setAttribute(node, 'height', `${height}`);
      adaptor.setAttribute(node, 'style', 'color:#202630');
      // MathJax's diagnostic data-latex attributes contain literal TeX, which
      // can include '<'. They are not SVG geometry and are not needed in PNGs.
      function stripDiagnostics(element) {
        if (adaptor.kind(element) === '#text') return;
        for (const {name} of adaptor.allAttributes(element))
          if (name.startsWith('data-')) adaptor.removeAttribute(element, name);
        for (const child of adaptor.childNodes(element)) stripDiagnostics(child);
      }
      stripDiagnostics(node);
      const serialized = adaptor.outerHTML(node);
      // All glyphs must be local vector outlines before rasterization. No OS
      // font substitution, external image or browser-executable markup is used.
      if (/<(?:text|image|script|foreignObject)\b|(?:href|src)\s*=/i.test(serialized)) throw Error('Non-self-contained SVG');
      const png = new Resvg(serialized, { fitTo: { mode: 'zoom', value: 3 }, font: { loadSystemFonts: false } }).render().asPng();
      if (png.length > 512000) throw Error('Equation exceeds the image byte budget');
      manifest.equations[id] = { tex, width, height, sha256: hash(png) };
      const file = path.join(output, `${id}.png`);
      if (check) {
        if (!fs.existsSync(file) || !fs.readFileSync(file).equals(png)) throw Error('PNG differs from authored TeX');
      } else fs.writeFileSync(file, png);
    } catch (error) { throw Error(`Equation ${id}: ${tex}\n${error.message}`); }
  }
  const json = JSON.stringify(manifest, null, 2) + '\n';
  const manifestPath = path.join(output, 'manifest.json');
  if (check) {
    if (fs.readFileSync(manifestPath, 'utf8') !== json) throw Error('Equation manifest differs');
  } else {
    fs.writeFileSync(manifestPath, json);
    for (const file of fs.readdirSync(output)) {
      if (/^[a-f0-9]{64}\.png$/.test(file) && !manifest.equations[file.slice(0, -4)]) fs.unlinkSync(path.join(output, file));
    }
  }
  console.log(`${check ? 'Verified' : 'Typeset'} ${Object.keys(equations).length} offline TeX equations.`);
})().catch(error => { console.error(error); process.exitCode = 1; });
