// Rebuild the shipped reference from inert nodes before giving HTML to Ace.
// No source attributes survive except reviewed reference classes and HTTP(S) links.
const elements = new Set(['DIV', 'P', 'BR', 'PRE', 'UL', 'LI', 'CODE', 'STRONG', 'B', 'EM', 'I', 'SUB', 'SUP', 'A']);
const excluded = new Set(['SCRIPT', 'STYLE', 'TEMPLATE', 'IFRAME', 'OBJECT', 'EMBED', 'SVG', 'MATH', 'NOSCRIPT', 'NOFRAMES']);
const classes = new Set(['function-reference', 'reference-arguments', 'reference-example', 'reference-formula',
  'reference-note', 'reference-source', 'reference-summary',
  'reference-explanation', 'reference-equation', 'reference-worked-example', 'reference-variant']);

export function referenceHtml(title: string, fragment: unknown, _displayedSignature?: string): string {
  const template = document.createElement('template');
  if (typeof fragment === 'string') template.innerHTML = fragment;
  const article = document.createElement('div');
  article.className = 'computator-reference';
  const heading = document.createElement('strong');
  heading.className = 'reference-title'; heading.textContent = title;
  article.append(heading);

  function append(source: Node, target: Node) {
    if (source.nodeType === Node.TEXT_NODE) { target.appendChild(document.createTextNode(source.textContent ?? '')); return; }
    if (source.nodeType !== Node.ELEMENT_NODE) return;
    const element = source as Element;
    const name = element.tagName.toUpperCase(); // SVG/MathML names are lowercase.
    // Autocomplete owns the signatures. Remove the entire metadata block,
    // including sibling overloads and legacy Syntax headings, in every context.
    // Do not strip code from examples, formulas or argument explanations.
    if (excluded.has(name) || element.classList.contains('reference-signature')) return;
    if (name === 'IMG') {
      // Accept only packaged PNG equations, never data URLs, remote resources,
      // SVG or arbitrary same-origin paths. Reconstruct every attribute.
      const src = element.getAttribute('src') ?? '';
      const width = element.getAttribute('width') ?? '', height = element.getAttribute('height') ?? '';
      const alt = element.getAttribute('alt') ?? '';
      const imageClass = element.getAttribute('class');
      const inline = imageClass === 'reference-math-inline';
      const depthText = element.getAttribute('data-depth') ?? '';
      const depth = Number(depthText);
      if (!/^reference-equations\/[a-f0-9]{64}\.png$/.test(src) ||
          (!inline && imageClass !== 'reference-math') || !alt.trim() ||
          !/^[1-9]\d{0,3}$/.test(width) || Number(width) > (inline ? 320 : 2400) ||
          !/^[1-9]\d{0,2}$/.test(height) || Number(height) > (inline ? 100 : 800) ||
          (inline ? !/^(?:0|-?[1-9][0-9]{0,3})$/.test(depthText) || depth < -1000 || depth >= Number(height) * 50
            : element.hasAttribute('data-depth'))) return;
      const image = document.createElement('img');
      image.className = inline ? 'reference-math-inline' : 'reference-math'; image.alt = alt;
      image.width = Number(width); image.height = Number(height);
      image.src = '/' + src;
      if (inline) {
        // Scale with surrounding prose. The measured descent, in 1/1000 em,
        // aligns each expression to the text baseline instead of centering it.
        // Only bounded numeric metadata reaches these reconstructed styles.
        image.setAttribute('data-depth', String(depth));
        image.style.width = `${Number(width) / 20}em`;
        image.style.height = `${Number(height) / 20}em`;
        image.style.verticalAlign = `${-depth / 1000}em`;
      }
      target.appendChild(image);
      return;
    }
    let destination = target;
    if (elements.has(name)) {
      const clean = document.createElement(name.toLowerCase());
      const retained = Array.from(element.classList).filter(name => classes.has(name));
      if (retained.length) clean.className = retained.join(' ');
      if (name === 'A') {
        try {
          const url = new URL(element.getAttribute('href') ?? '');
          if (url.protocol === 'https:' || url.protocol === 'http:') {
            clean.setAttribute('href', url.href);
            clean.setAttribute('target', '_blank'); clean.setAttribute('rel', 'noopener noreferrer');
          }
        } catch { /* Invalid/relative links remain readable, without navigation. */ }
      }
      target.appendChild(clean); destination = clean;
    }
    for (const child of Array.from(source.childNodes)) append(child, destination);
  }
  for (const child of Array.from(template.content.childNodes)) append(child, article);
  return article.outerHTML;
}
