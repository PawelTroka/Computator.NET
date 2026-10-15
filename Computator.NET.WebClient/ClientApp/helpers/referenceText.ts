// Ace's docText must stay text. Parse the shipped HTML in an inert template,
// never attach it to the document and never pass it as executable docHTML.
export function referenceText(fragment: unknown): string {
  if (typeof fragment !== 'string') return '';
  const template = document.createElement('template');
  template.innerHTML = fragment;
  const excluded = new Set(['SCRIPT', 'STYLE', 'TEMPLATE', 'IFRAME', 'OBJECT', 'SVG', 'MATH']);
  const blocks = new Set(['P', 'DIV', 'PRE', 'LI', 'BR', 'H1', 'H2', 'H3']);
  function text(node: Node): string {
    if (node.nodeType === Node.TEXT_NODE) return node.textContent ?? '';
    if (node.nodeType === Node.ELEMENT_NODE) {
      const name = (node as Element).tagName;
      if (excluded.has(name)) return '';
      const children = Array.from(node.childNodes, text).join('');
      return blocks.has(name) ? '\n' + children + '\n' : children;
    }
    return Array.from(node.childNodes, text).join('');
  }
  return text(template.content).replace(/[ \t]+\n/g, '\n').replace(/\n{3,}/g, '\n\n').trim();
}
