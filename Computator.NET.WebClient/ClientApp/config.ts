// Keep the existing local API endpoint unless deployment explicitly overrides it.
export const WEB_API_BASE_URL: string = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:60996/api').replace(/\/+$/, '');
