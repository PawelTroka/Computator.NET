import { WEB_API_BASE_URL } from '../config';
import { referenceText } from './referenceText';
interface Completion { caption: string; value: string; score: number; meta: string; docText: string; }
export class CustomCompleter {
  private items: Completion[] = [];
  private readonly controller = new AbortController();
  public constructor(mode: 'expression' | 'scripting') {
    void fetch(`${WEB_API_BASE_URL}/autocomplete/${mode}`, { signal: this.controller.signal })
      .then(response => { if (!response.ok) throw new Error('Autocomplete is unavailable.'); return response.json(); })
      .then((items: unknown) => {
        if (!Array.isArray(items)) return;
        this.items = items.filter(item => item && typeof item.text === 'string' && item.details)
          .map(item => ({ caption: typeof item.menuText === 'string' ? item.menuText : item.text,
            value: item.text, score: 0,
            meta: typeof item.details.category === 'string' && item.details.category.trim()
              ? item.details.category.trim() : typeof item.details.type === 'string' ? item.details.type : '',
            docText: `${typeof item.details.title === 'string' ? item.details.title : item.text}\n${referenceText(item.details.description)}` }));
      }).catch(() => { this.items = []; });
  }
  public dispose() { this.controller.abort(); }
  public getCompletions(_editor: unknown, _session: unknown, _position: unknown, prefix: string,
    callback: (error: null, items: Completion[]) => void): void {
    callback(null, this.items.map(item => ({ ...item,
      score: item.value.includes(prefix) ? prefix.length / Math.max(item.value.length, 1) : 0 })));
  }
}
