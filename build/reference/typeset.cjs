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
  // MathJax 4 otherwise emits separate SVG siblings at inline break points.
  // Each asset must contain the complete expression, including unary signs.
  const svg = new SVG({ fontCache: 'none', mtextInheritFont: false, linebreaks: { inline: false } });
  const document = mathjax.document('', { InputJax: new TeX({ packages: ['base', 'ams'],
    formatError(_jax, error) { throw Error(error.message); } }), OutputJax: svg });
  const manifest = { schema: 2, renderer: 'MathJax 4.1.3 / resvg 2.6.2; 20px math, 3x pixels; inline depth in 1/1000 em', equations: {} };
  if (!check) fs.mkdirSync(output, { recursive: true });
  for (const [id, source] of Object.entries(equations).sort(([a], [b]) => a.localeCompare(b))) {
    const inline = typeof source === 'object' && source !== null && source.mode === 'inline';
    const tex = inline ? source.tex : source;
    if (typeof tex !== 'string' || id !== hash((inline ? 'inline\0' : '') + tex) || tex.length > 6000)
      throw Error(`Invalid equation identity/length: ${id}`);
    if (/\\(?:href|url|style|class|cssId|includegraphics|require|html)\b/.test(tex)) throw Error(`Active TeX command: ${id}`);
    try {
      const result = await document.convertPromise(tex, { display: !inline, em: 20, ex: 10, containerWidth: 560 });
      if (adaptor.childNodes(result).length !== 1 || adaptor.kind(adaptor.firstChild(result)) !== 'svg')
        throw Error('Expected one complete SVG; refusing to discard inline equation segments');
      const node = adaptor.firstChild(result);
      const box = adaptor.getAttribute(node, 'viewBox').split(/\s+/).map(Number);
      if (box.length !== 4 || box.some(n => !Number.isFinite(n)) || box[2] <= 0 || box[3] <= 0) throw Error('Invalid SVG box');
      const [x, y, w, h] = box;
      // SVG's y=0 is the mathematical baseline. Tight inline padding avoids
      // inserting display-equation whitespace into prose; keep the display
      // geometry untouched so all existing PNGs retain their identities.
      const padding = inline ? 25 : 200;
      const width = Math.ceil((w + 2 * padding) / 50), height = Math.ceil((h + 2 * padding) / 50);
      if (width > (inline ? 320 : 2400) || height > (inline ? 100 : 800))
        throw Error('Equation needs editorial line breaks or display math');
      // Match the inline viewBox aspect ratio to the integer pixel dimensions.
      // Otherwise SVG's default "meet" scaling can add letterboxing that moves
      // the glyph baseline independently of the measured image-box depth.
      // 50 SVG units = 1px at 20px/em; center the extra rounding padding.
      const boxWidth = inline ? width * 50 : w + 2 * padding;
      const boxHeight = inline ? height * 50 : h + 2 * padding;
      const boxX = inline ? x - (boxWidth - w) / 2 : x - padding;
      const boxY = inline ? y - (boxHeight - h) / 2 : y - padding;
      const depth = Math.round(boxY + boxHeight); // 1 SVG unit = 1/1000 em
      if (inline && (depth < -1000 || depth >= height * 50)) throw Error('Invalid inline baseline');
      adaptor.setAttribute(node, 'viewBox', `${boxX} ${boxY} ${boxWidth} ${boxHeight}`);
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
      if (inline) Object.assign(manifest.equations[id], { mode: 'inline', depth });
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
