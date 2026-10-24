// Match the API development listener unless deployment explicitly overrides it.
export const WEB_API_BASE_URL: string = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5080/api').replace(/\/+$/, '');
