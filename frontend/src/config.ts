const DEFAULT_API_BASE_URL = 'http://localhost:5000';

function normalizeBaseUrl(url: string): string {
  return url.trim().replace(/\/+$/, '');
}

const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL as string | undefined;

export const API_BASE_URL = normalizeBaseUrl(configuredBaseUrl ?? DEFAULT_API_BASE_URL);
