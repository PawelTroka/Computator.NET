import { WEB_API_BASE_URL } from '../config';

interface AutocompleteItem {
  text: string;
  menuText: string;
  details: { title?: string; description?: string; category?: string; type?: string };
}
interface Completion {
  caption: string; value: string; score: number; meta: string; docText: string;
}

export class CustomCompleter {
  private items: AutocompleteItem[] = [];
  private readonly controller = new AbortController();

  public constructor(mode: 'expression' | 'scripting') {
    void fetch(`${WEB_API_BASE_URL}/autocomplete/${mode}`, { signal: this.controller.signal })
      .then(response => { if (!response.ok) throw new Error('Autocomplete is unavailable.'); return response.json(); })
      .then((items: unknown) => {
        if (Array.isArray(items)) this.items = items.filter(item => item && typeof item.text === 'string' && item.details);
      }).catch(() => { this.items = []; });
  }

  public dispose() { this.controller.abort(); }

  public getCompletions(_editor: unknown, _session: unknown, _position: unknown, prefix: string,
    callback: (error: null, items: Completion[]) => void): void {
    callback(null, this.items.map(item => ({
      caption: item.menuText || item.text,
      value: item.text,
      score: item.text.includes(prefix) ? prefix.length / Math.max(item.text.length, 1) : 0,
      meta: item.details.category?.trim() || item.details.type || '',
      // API descriptions are data, never executable HTML in the editor tooltip.
      docText: `${item.details.title || item.text}\n${item.details.description || ''}`
    })));
  }
}
