// Editorial-only scientific diagrams. Sample the reviewed mathematical models,
// draw level curves and a labelled cross-section, and rasterize through the same
// pinned MathJax/resvg dependencies as the equation pipeline. No runtime renderer.
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { createRequire } = require('node:module');
const dependencies = createRequire(path.resolve(__dirname, '../../Computator.NET.WebClient/package.json'));
const hash = value => crypto.createHash('sha256').update(value).digest('hex');
const source = 'https://github.com/mathnet/mathnet-numerics/blob/v5.0.0/src/Numerics/SpecialFunctions/TestFunctions.cs';
const models = {
  Ackley: {
    value: (x, y) => -20 * Math.exp(-0.2 * Math.sqrt((x*x + y*y) / 2)) - Math.exp((Math.cos(2*Math.PI*x) + Math.cos(2*Math.PI*y)) / 2) + 20 + Math.E,
    x: [-4, 4], y: [-4, 4], z: [0, 12], levels: [1, 3, 5, 7, 9, 11], minima: [[0, 0]],
    slice: () => 0, sliceTex: 'y=0',
    caption: 'The two-coordinate landscape has a narrow central basin and repeated shallow basins around it. Coloured curves join equal objective values. The orange line is the y=0 cross-section shown on the right; the marked global minimum is at (0,0).',
    alt: 'Ackley in two coordinates, x and y from -4 to 4: nested and rippled contours surround the global minimum at (0,0). The y=0 slice has its deepest value zero at x=0, with smaller repeated local basins on either side.'
  },
  Rastrigin: {
    value: (x, y) => 20 + x*x + y*y - 10*Math.cos(2*Math.PI*x) - 10*Math.cos(2*Math.PI*y),
    x: [-3, 3], y: [-3, 3], z: [0, 30], levels: [5, 10, 20, 30, 40, 50], minima: [[0, 0]],
    slice: () => 0, sliceTex: 'y=0',
    caption: 'The repeated contour islands are local basins inside the larger quadratic bowl. The y=0 slice shows the periodic barriers between them. Only the marked origin attains the global value zero; the nearby basins remain higher.',
    alt: 'Rastrigin in two coordinates from -3 to 3: a grid of local basins surrounds the unique global minimum at the origin. The y=0 slice oscillates above zero except at x=0.'
  },
  Bohachevsky1: {
    // Match the current application, including Math.NET 5.0's missing +0.7.
    value: (x, y) => x*x + 2*y*y - 0.3*Math.cos(3*Math.PI*x) - 0.4*Math.cos(4*Math.PI*y),
    x: [-2, 2], y: [-2, 2], z: [-1, 4], levels: [-0.5, 0, 1, 2, 4, 8], minima: [[0, 0]],
    slice: () => 0, sliceTex: 'y=0',
    caption: 'This drawing follows the value returned by Bohachevsky1, whose minimum is -0.7 at the origin. Adding 0.7 produces the standard definition above: every height changes, while the contours and minimizing coordinates keep their shapes and positions.',
    alt: 'Bohachevsky1 as returned by the application, without the standard +0.7 offset: contours of an oscillatory bowl and its y=0 cross-section. The marked origin has value -0.7, not zero.'
  },
  Himmelblau: {
    value: (x, y) => (x*x + y - 11)**2 + (x + y*y - 7)**2,
    x: [-4.5, 4.5], y: [-4.5, 4.5], z: [0, 200], levels: [1, 10, 30, 60, 120, 240],
    minima: [[3, 2], [-2.805118, 3.131312], [-3.779310, -3.283186], [3.584428, -1.848126]],
    slice: () => 2, sliceTex: 'y=2',
    caption: 'Four separate basins attain the same global value zero; all four minimizing points are marked. The y=2 slice passes through (3,2) but misses the other three minima. A one-dimensional slice alone cannot establish the number of minima of a surface.',
    alt: 'Himmelblau level curves show four distinct equal-depth basins. Four global minimizers are marked. The horizontal y=2 slice passes through the minimum (3,2), where both squared terms vanish.'
  },
  Matyas: {
    value: (x, y) => 0.26*(x*x + y*y) - 0.48*x*y,
    x: [-3, 3], y: [-3, 3], z: [0, 0.4], levels: [0.05, 0.2, 0.5, 1, 2, 4], minima: [[0, 0]],
    slice: x => x, sliceTex: 'y=x',
    caption: 'The long contour ellipses follow the diagonal y=x: changing both coordinates together moves along the shallow valley. Along that orange diagonal the function reduces to 0.04x². Perpendicular to it, along y=-x, the curvature is much stronger.',
    alt: 'Matyas has long elliptical contours aligned with y=x and a unique minimum at the origin. The plotted diagonal slice is 0.04x squared, demonstrating the shallow direction of the valley.'
  },
  Rosenbrock: {
    value: (x, y) => (1 - x)**2 + 100*(y - x*x)**2,
    x: [-2, 2], y: [-1, 5], z: [0, 9], levels: [0.1, 1, 5, 20, 100, 400], minima: [[1, 1]],
    slice: x => x*x, sliceTex: 'y=x^2',
    caption: 'The orange path follows the curved valley y=x², rather than a straight cross-section. On this path the large squared penalty vanishes and only (1-x)² remains, reaching zero at the marked point (1,1). The close contour spacing across the valley shows why movement across it is costly.',
    alt: 'Rosenbrock contours bend around the parabola y=x squared. The marked global minimum is (1,1). The right panel follows that curved path and plots (1-x) squared, with its minimum at x=1.'
  },
  dropWave: {
    value: (x, y) => -(1 + Math.cos(12*Math.sqrt(x*x + y*y))) / (2 + (x*x + y*y)/2),
    x: [-2, 2], y: [-2, 2], z: [-1, 0.1], levels: [-0.9, -0.7, -0.5, -0.3, -0.1], minima: [[0, 0]],
    slice: () => 0, sliceTex: 'y=0',
    caption: 'Concentric rings reflect the dependence on distance from the origin. The y=0 slice shows repeated negative troughs, with the deepest value -1 at the centre. The intervening peaks approach zero, which is not the minimizing value.',
    alt: 'Drop-wave has concentric oscillatory rings around the origin. Its y=0 slice repeatedly drops below zero and reaches its deepest trough, -1, at x=0.'
  },
  sixHumpCamel: {
    value: (x, y) => (4 - 2.1*x*x + x**4/3)*x*x + x*y + (-4 + 4*y*y)*y*y,
    x: [-2, 2], y: [-1.1, 1.1], z: [-1.2, 6], levels: [-1, -0.8, -0.4, 0, 1, 3],
    minima: [[0.089842, -0.712656], [-0.089842, 0.712656]],
    slice: () => -0.712656, sliceTex: 'y=-0.712656',
    caption: 'Two symmetry-related global basins are marked. The horizontal slice uses the rounded y-coordinate of the lower basin and passes close to its minimum, near x=0.089842. Other closed contours correspond to higher basins; a stationary point need not be globally best.',
    alt: 'Six-hump camel contours show two symmetry-related deepest basins near (0.089842,-0.712656) and (-0.089842,0.712656). The plotted lower horizontal slice reaches approximately -1.031628 near x=0.089842.'
  }
};

// Marching squares on a uniformly sampled grid, with a centre-value decision
// for saddle cells. Return data-space segments so numerical tests do not rely
// on pixels or merely mirror the SVG strings.
function contours(value, xr, yr, level, cells = 160) {
  const grid = Array.from({ length: cells + 1 }, (_, j) => Array.from({ length: cells + 1 }, (_, i) =>
    value(xr[0] + i*(xr[1]-xr[0])/cells, yr[0] + j*(yr[1]-yr[0])/cells)));
  const result = [];
  for (let j = 0; j < cells; j++) for (let i = 0; i < cells; i++) {
    const points = [[i,j], [i+1,j], [i+1,j+1], [i,j+1]];
    const values = [grid[j][i], grid[j][i+1], grid[j+1][i+1], grid[j+1][i]];
    const crossings = [];
    for (let edge = 0; edge < 4; edge++) {
      const next = (edge + 1) % 4;
      if ((values[edge] >= level) === (values[next] >= level)) continue;
      const t = (level - values[edge]) / (values[next] - values[edge]);
      const px = points[edge][0] + t*(points[next][0] - points[edge][0]);
      const py = points[edge][1] + t*(points[next][1] - points[edge][1]);
      crossings.push([xr[0] + px*(xr[1]-xr[0])/cells, yr[0] + py*(yr[1]-yr[0])/cells]);
    }
    if (crossings.length === 2) result.push(crossings);
    else if (crossings.length === 4) {
      const centre = value(xr[0] + (i+0.5)*(xr[1]-xr[0])/cells, yr[0] + (j+0.5)*(yr[1]-yr[0])/cells);
      const pairs = (values[0] >= level) === (centre >= level) ? [[0,1],[2,3]] : [[0,3],[1,2]];
      for (const [a,b] of pairs) result.push([crossings[a],crossings[b]]);
    }
  }
  return result;
}

async function render(output, check) {
  const { mathjax } = dependencies('@mathjax/src/js/mathjax.js');
  const { TeX } = dependencies('@mathjax/src/js/input/tex.js');
  const { SVG } = dependencies('@mathjax/src/js/output/svg.js');
  const { liteAdaptor } = dependencies('@mathjax/src/js/adaptors/liteAdaptor.js');
  const { RegisterHTMLHandler } = dependencies('@mathjax/src/js/handlers/html.js');
  dependencies('@mathjax/src/js/util/asyncLoad/node.js');
  dependencies('@mathjax/src/js/input/tex/base/BaseConfiguration.js');
  const { Resvg } = dependencies('@resvg/resvg-js');
  if (mathjax.version !== '4.1.3') throw Error('Expected reviewed MathJax 4.1.3');
  const adaptor = liteAdaptor(); RegisterHTMLHandler(adaptor);
  const document = mathjax.document('', { InputJax: new TeX({ packages: ['base'], formatError(_jax,e) { throw Error(e.message); } }),
    OutputJax: new SVG({ fontCache: 'none', linebreaks: { inline: false } }) });
  const labels = new Map();
  async function label(tex, x, y, size = 16, align = 'middle') {
    if (!labels.has(tex)) {
      const node = adaptor.firstChild(await document.convertPromise(tex, { display: false }));
      const box = adaptor.getAttribute(node, 'viewBox').split(/\s+/).map(Number);
      function clean(node) {
        if (adaptor.kind(node) === '#text') return;
        for (const {name} of adaptor.allAttributes(node)) if (name.startsWith('data-')) adaptor.removeAttribute(node,name);
        for (const child of adaptor.childNodes(node)) clean(child);
      }
      clean(node);
      labels.set(tex, { box, content: adaptor.innerHTML(node) });
    }
    const {box,content} = labels.get(tex);
    const w = box[2]*size/1000, h = box[3]*size/1000;
    return `<svg x="${x-(align === 'middle' ? w/2 : align === 'end' ? w : 0)}" y="${y-h/2}" width="${w}" height="${h}" viewBox="${box.join(' ')}" color="#26364b">${content}</svg>`;
  }
  const colors = ['#315585','#347b9e','#369d96','#79ad66','#c2a342','#ad714c'];
  const number = x => Number(x.toFixed(3)).toString();
  const manifest = { schema: 1, renderer: 'Sampled contours and sections; MathJax 4.1.3 / resvg 2.6.2; 3x pixels', figures: {} };
  if (!check) fs.mkdirSync(output, { recursive: true });
  for (const [name,model] of Object.entries(models)) {
    const width = 600, height = 374;
    const panels = [{ x: 46, y: 48, w: 224, h: 224, yr: model.y }, { x: 352, y: 48, w: 224, h: 224, yr: model.z }];
    const point = (panel,x,y) => [panel.x + (x-model.x[0])/(model.x[1]-model.x[0])*panel.w,
      panel.y + panel.h - (y-panel.yr[0])/(panel.yr[1]-panel.yr[0])*panel.h];
    let svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}"><rect width="600" height="374" fill="white"/>`;
    svg += '<defs>' + panels.map((p,i) => `<clipPath id="panel${i}"><rect x="${p.x}" y="${p.y}" width="${p.w}" height="${p.h}"/></clipPath>`).join('') + '</defs>';
    svg += await label('\\text{Level curves}',158,19,18) + await label(model.sliceTex,464,19,18);
    for (const [index,panel] of panels.entries()) {
      svg += `<rect x="${panel.x}" y="${panel.y}" width="${panel.w}" height="${panel.h}" fill="#fafcff" stroke="#93a0b1"/>`;
      for (let tick = 0; tick <= 4; tick++) {
        const x = model.x[0] + tick*(model.x[1]-model.x[0])/4;
        const y = panel.yr[0] + tick*(panel.yr[1]-panel.yr[0])/4;
        const px = point(panel,x,0)[0], py = point(panel,0,y)[1];
        svg += `<path d="M${number(px)},${panel.y}v${panel.h}M${panel.x},${number(py)}h${panel.w}" stroke="#e3e8ef" stroke-width="0.7"/>`;
        svg += await label(number(x),px,287,14) + await label(number(y),panel.x-8,py,14,'end');
      }
      svg += await label('x',panel.x+panel.w/2,308,17);
      svg += await label(index === 0 ? 'y' : 'f',panel.x-28,30,17);
    }
    for (const [index,level] of model.levels.entries()) {
      const segments = contours(model.value,model.x,model.y,level);
      const d = segments.map(segment => segment.map((xy,i) => (i ? 'L' : 'M')+point(panels[0],...xy).map(number).join(',')).join('')).join('');
      svg += `<path d="${d}" fill="none" stroke="${colors[index]}" stroke-width="1.05" clip-path="url(#panel0)"/>`;
      const lx = 47 + index*47;
      svg += `<path d="M${lx},333h10" stroke="${colors[index]}" stroke-width="2"/>`;
      svg += await label(number(level),lx+14,333,12,'start');
    }
    for (const [index,panel] of panels.entries()) {
      const d = Array.from({ length: 1001 }, (_,i) => {
        const x = model.x[0] + i*(model.x[1]-model.x[0])/1000;
        const y = index === 0 ? model.slice(x) : model.value(x,model.slice(x));
        return (i ? 'L' : 'M')+point(panel,x,y).map(number).join(',');
      }).join('');
      svg += `<path d="${d}" fill="none" stroke="#d57a22" stroke-width="2.1"${index === 0 ? ' stroke-dasharray="5 4"' : ''} clip-path="url(#panel${index})"/>`;
    }
    for (const [x,y] of model.minima) {
      const [px,py] = point(panels[0],x,y);
      svg += `<circle cx="${number(px)}" cy="${number(py)}" r="4.2" fill="#253851" stroke="white" stroke-width="1.3"/>`;
      if (Math.abs(y-model.slice(x)) < 1e-6) {
        const [sx,sy] = point(panels[1],x,model.value(x,y));
        svg += `<circle cx="${number(sx)}" cy="${number(sy)}" r="4.2" fill="#253851" stroke="white" stroke-width="1.3"/>`;
      }
    }
    svg += await label('\\text{Contour values}',46,354,13,'start');
    svg += '<circle cx="362" cy="333" r="4.2" fill="#253851"/>' + await label('\\text{Global minimum}',373,333,13,'start');
    svg += '<path d="M354,354h16" stroke="#d57a22" stroke-width="2" stroke-dasharray="4 3"/>' + await label('\\text{Path sampled at right}',377,354,13,'start');
    svg += '</svg>';
    if (/<(?:text|image|script|foreignObject)\b|(?:href|src)\s*=/i.test(svg)) throw Error('Plot must contain only self-contained vector outlines');
    const id = hash(svg);
    const png = new Resvg(svg, { fitTo: { mode: 'zoom', value: 3 }, font: { loadSystemFonts: false } }).render().asPng();
    if (png.length > 512000) throw Error(`${name} exceeds the PNG byte budget`);
    manifest.figures[id] = { name, width, height, sha256: hash(png), source,
      caption: model.caption, alt: model.alt, x: model.x, y: model.y, levels: model.levels,
      minima: model.minima, sliceTex: model.sliceTex };
    const file = path.join(output,id+'.png');
    if (check) { if (!fs.existsSync(file) || !fs.readFileSync(file).equals(png)) throw Error(`${name} plot differs`); }
    else fs.writeFileSync(file,png);
  }
  const json = JSON.stringify(manifest,null,2)+'\n';
  const file = path.join(output,'manifest.json');
  if (check) { if (fs.readFileSync(file,'utf8') !== json) throw Error('Plot manifest differs'); }
  else {
    fs.writeFileSync(file,json);
    for (const file of fs.readdirSync(output)) if (/^[a-f0-9]{64}\.png$/.test(file) && !manifest.figures[file.slice(0,-4)]) fs.unlinkSync(path.join(output,file));
  }
  console.log(`${check ? 'Verified' : 'Rendered'} ${Object.keys(models).length} offline benchmark diagrams.`);
}
module.exports = { models, contours };
if (require.main === module) {
  const args = process.argv.slice(2);
  if (!args.includes('--output')) throw Error('Expected --output directory [--check]');
  render(args[args.indexOf('--output')+1],args.includes('--check')).catch(error => { console.error(error); process.exitCode = 1; });
}
