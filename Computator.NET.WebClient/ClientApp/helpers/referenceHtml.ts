// Rebuild the shipped reference from inert nodes before giving HTML to Ace.
// No source attributes survive except reviewed reference classes and HTTP(S) links.
const elements = new Set(['DIV', 'P', 'BR', 'CODE', 'STRONG', 'B', 'EM', 'I', 'SUB', 'SUP', 'A']);
const excluded = new Set(['SCRIPT', 'STYLE', 'TEMPLATE', 'IFRAME', 'OBJECT', 'EMBED', 'SVG', 'MATH', 'NOSCRIPT', 'NOFRAMES']);
const classes = new Set(['function-reference', 'reference-arguments', 'reference-example', 'reference-formula',
  'reference-note', 'reference-signature', 'reference-source', 'reference-summary']);

export function referenceHtml(title: string, fragment: unknown): string {
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
    if (excluded.has(name)) return;
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
