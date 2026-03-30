import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';

interface UseAdminCrudResult<T> {
  items: T[];
  loading: boolean;
  error: string;
  saving: boolean;
  load: () => Promise<void>;
  create: (body: unknown) => Promise<void>;
  update: (id: number, body: unknown) => Promise<void>;
  remove: (id: number) => Promise<void>;
  setError: (msg: string) => void;
}

export function useAdminCrud<T>(apiPath: string): UseAdminCrudResult<T> {
  const { apiFetch } = useAuth();
  const [items, setItems] = useState<T[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const data = await apiFetch<T[]>(apiPath);
      setItems(data);
    } catch (err) {
      setError(getErrorMessage(err, 'Erreur lors du chargement.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, apiPath]);

  useEffect(() => {
    void load();
  }, [load]);

  const create = useCallback(
    async (body: unknown) => {
      setSaving(true);
      try {
        await apiFetch(apiPath, {
          method: 'POST',
          body: JSON.stringify(body),
        });
        await load();
      } finally {
        setSaving(false);
      }
    },
    [apiFetch, apiPath, load],
  );

  const update = useCallback(
    async (id: number, body: unknown) => {
      setSaving(true);
      try {
        await apiFetch(`${apiPath}/${id}`, {
          method: 'PUT',
          body: JSON.stringify(body),
        });
        await load();
      } finally {
        setSaving(false);
      }
    },
    [apiFetch, apiPath, load],
  );

  const remove = useCallback(
    async (id: number) => {
      setSaving(true);
      try {
        await apiFetch(`${apiPath}/${id}`, {
          method: 'DELETE',
        });
        await load();
      } finally {
        setSaving(false);
      }
    },
    [apiFetch, apiPath, load],
  );

  return { items, loading, error, saving, load, create, update, remove, setError };
}
