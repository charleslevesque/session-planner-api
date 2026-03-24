export class ApiError extends Error {
  readonly status: number;
  readonly details: unknown;

  constructor(message: string, status: number, details?: unknown) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

const API_PREFIX = '/api/v1';

async function parseResponse<T>(response: Response): Promise<T> {
  const contentType = response.headers.get('content-type') ?? '';
  const isJson = contentType.includes('application/json');
  const payload = isJson ? await response.json() : await response.text();

  if (!response.ok) {
    const message =
      isJson && typeof payload === 'object' && payload !== null && 'error' in payload
        ? String(payload.error)
        : response.statusText || 'Une erreur est survenue.';

    throw new ApiError(message, response.status, payload);
  }

  return payload as T;
}

export async function apiRequest<T>(
  path: string,
  init: RequestInit = {},
  accessToken?: string,
): Promise<T> {
  const headers = new Headers(init.headers);

  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json');
  }

  if (init.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json');
  }

  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const response = await fetch(`${API_PREFIX}${path}`, {
    ...init,
    headers,
  });

  if (response.status === 204) {
    return undefined as T;
  }

  return parseResponse<T>(response);
}

export function apiGet<T>(path: string, accessToken?: string) {
  return apiRequest<T>(path, { method: 'GET' }, accessToken);
}

export function apiPost<TRequest, TResponse>(
  path: string,
  body: TRequest,
  accessToken?: string,
) {
  return apiRequest<TResponse>(
    path,
    {
      method: 'POST',
      body: JSON.stringify(body),
    },
    accessToken,
  );
}

export function getErrorMessage(error: unknown, fallback: string) {
  if (error instanceof ApiError) {
    return error.message;
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return fallback;
}
