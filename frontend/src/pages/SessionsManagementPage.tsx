import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type { SessionResponse, SessionStatus } from '../types/sessions';

type ApiSessionResponse = Omit<SessionResponse, 'status'> & {
  status: SessionStatus | number | string;
};

function normalizeSessionStatus(status: SessionStatus | number | string): SessionStatus {
  if (typeof status === 'number') {
    const byValue: Record<number, SessionStatus> = { 1: 'Draft', 2: 'Open', 3: 'Closed', 4: 'Archived' };
    return byValue[status] ?? 'Draft';
  }

  const normalized = String(status).trim();
  const map: Record<string, SessionStatus> = {
    draft: 'Draft',
    open: 'Open',
    closed: 'Closed',
    archived: 'Archived',
  };

  return map[normalized.toLowerCase()] ?? (normalized as SessionStatus) ?? 'Draft';
}

function toDateInput(value: string) {
  return new Date(value).toISOString().slice(0, 10);
}

function getTransitionAction(from: SessionStatus, to: SessionStatus): 'open' | 'close' | 'archive' | null {
  if (from === 'Draft' && to === 'Open') return 'open';
  if (from === 'Open' && to === 'Closed') return 'close';
  if (from === 'Closed' && to === 'Archived') return 'archive';
  return null;
}

function getValidTargets(current: SessionStatus): SessionStatus[] {
  switch (current) {
    case 'Draft': return ['Draft', 'Open'];
    case 'Open': return ['Open', 'Closed'];
    case 'Closed': return ['Closed', 'Archived'];
    default: return [current];
  }
}

function SessionStatusBadge({ status }: { status: SessionStatus }) {
  const styles: Record<SessionStatus, string> = {
    Draft: 'bg-slate-100 text-slate-700 border-slate-200',
    Open: 'bg-emerald-100 text-emerald-700 border-emerald-200',
    Closed: 'bg-amber-100 text-amber-700 border-amber-200',
    Archived: 'bg-stone-200 text-stone-700 border-stone-300',
  };

  return <span className={`inline-flex rounded-xl border px-2.5 py-0.5 text-xs font-medium ${styles[status]}`}>{status}</span>;
}

export function SessionsManagementPage() {
  const { apiFetch, user } = useAuth();
  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [targetStatusById, setTargetStatusById] = useState<Record<number, SessionStatus>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [savingId, setSavingId] = useState<number | null>(null);
  const [editId, setEditId] = useState<number | null>(null);

  const [createForm, setCreateForm] = useState({
    title: '',
    startDate: '',
    endDate: '',
  });

  const [editForm, setEditForm] = useState({
    title: '',
    startDate: '',
    endDate: '',
  });

  const canCreateSessions = user?.role === 'admin';
  const canUpdateSessions = user?.role === 'admin' || user?.role === 'lab_instructor';
  const canDeleteSessions = user?.role === 'admin';

  const loadSessions = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const rawData = await apiFetch<ApiSessionResponse[]>('/sessions');
      const data: SessionResponse[] = rawData.map((session) => ({
        ...session,
        status: normalizeSessionStatus(session.status),
      }));
      setSessions(data);
      setTargetStatusById(Object.fromEntries(data.map((session) => [session.id, session.status])));
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les sessions.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch]);

  useEffect(() => {
    void loadSessions();
  }, [loadSessions]);

  const summary = useMemo(() => {
    return {
      total: sessions.length,
      open: sessions.filter((s) => s.status === 'Open').length,
      draft: sessions.filter((s) => s.status === 'Draft').length,
    };
  }, [sessions]);

  async function createSession(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');

    if (createForm.endDate <= createForm.startDate) {
      setError('La date de fin doit être après la date de début.');
      return;
    }

    try {
      await apiFetch('/sessions', {
        method: 'POST',
        body: JSON.stringify(createForm),
      });

      setCreateForm({ title: '', startDate: '', endDate: '' });
      await loadSessions();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de créer la session.'));
    }
  }

  function startEdit(session: SessionResponse) {
    setEditId(session.id);
    setEditForm({
      title: session.title,
      startDate: toDateInput(session.startDate),
      endDate: toDateInput(session.endDate),
    });
  }

  async function saveEdit(sessionId: number) {
    setSavingId(sessionId);
    setError('');

    try {
      await apiFetch(`/sessions/${sessionId}`, {
        method: 'PUT',
        body: JSON.stringify(editForm),
      });
      setEditId(null);
      await loadSessions();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de mettre à jour la session.'));
    } finally {
      setSavingId(null);
    }
  }

  async function transition(sessionId: number, action: 'open' | 'close' | 'archive') {
    setSavingId(sessionId);
    setError('');

    try {
      const updated = await apiFetch<ApiSessionResponse>(`/sessions/${sessionId}/${action}`, {
        method: 'POST',
      });
      const normalized: SessionResponse = { ...updated, status: normalizeSessionStatus(updated.status) };
      setSessions((prev) => prev.map((s) => (s.id === sessionId ? normalized : s)));
      setTargetStatusById((prev) => ({ ...prev, [sessionId]: normalized.status }));
    } catch (err) {
      setError(getErrorMessage(err, 'Transition impossible pour cette session.'));
    } finally {
      setSavingId(null);
    }
  }

  async function applyStatus(sessionId: number) {
    const session = sessions.find((s) => s.id === sessionId);
    if (!session) return;

    const target = targetStatusById[sessionId];
    if (!target || target === session.status) return;

    const action = getTransitionAction(session.status, target);
    if (!action) {
      setError('Transition invalide.');
      return;
    }

    setSavingId(sessionId);
    setError('');

    try {
      const updated = await apiFetch<ApiSessionResponse>(`/sessions/${sessionId}/${action}`, {
        method: 'POST',
      });
      const normalized: SessionResponse = { ...updated, status: normalizeSessionStatus(updated.status) };
      setSessions((prev) => prev.map((s) => (s.id === sessionId ? normalized : s)));
      setTargetStatusById((prev) => ({ ...prev, [sessionId]: normalized.status }));
      setEditId(null);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de changer le statut de la session.'));
    } finally {
      setSavingId(null);
    }
  }

  async function removeSession(sessionId: number) {
    setSavingId(sessionId);
    setError('');

    try {
      await apiFetch(`/sessions/${sessionId}`, {
        method: 'DELETE',
      });
      await loadSessions();
    } catch (err) {
      setError(getErrorMessage(err, 'Suppression impossible pour cette session.'));
    } finally {
      setSavingId(null);
    }
  }

  return (
    <div className="space-y-6">
      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Sessions</p>
        <h1 className="mt-3 text-3xl font-semibold text-stone-950">Gestion des sessions</h1>
        <p className="mt-2 text-sm text-stone-600">Ouvrez les sessions pour débloquer la soumission de besoins.</p>

        <div className="mt-4 grid gap-3 sm:grid-cols-3">
          <div className="rounded-xl border border-stone-200 bg-stone-50 p-3 text-sm text-stone-700">Total: {summary.total}</div>
          <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-3 text-sm text-emerald-700">Open: {summary.open}</div>
          <div className="rounded-xl border border-slate-200 bg-slate-50 p-3 text-sm text-slate-700">Draft: {summary.draft}</div>
        </div>
      </section>

      {canCreateSessions ? (
        <section className="surface-card p-6 sm:p-8">
          <h2 className="text-base font-semibold text-stone-950">Créer une session</h2>
          <form className="mt-4 grid gap-4 md:grid-cols-[1.4fr_1fr_1fr_auto]" onSubmit={createSession}>
            <input
              value={createForm.title}
              onChange={(event) => setCreateForm((prev) => ({ ...prev, title: event.target.value }))}
              className="input-field"
              placeholder="Titre"
              required
            />
            <input
              type="date"
              value={createForm.startDate}
              onChange={(event) => setCreateForm((prev) => ({ ...prev, startDate: event.target.value }))}
              className="input-field"
              required
            />
            <input
              type="date"
              value={createForm.endDate}
              onChange={(event) => setCreateForm((prev) => ({ ...prev, endDate: event.target.value }))}
              className="input-field"
              required
            />
            <button type="submit" className="primary-button">Créer</button>
          </form>
        </section>
      ) : null}

      {error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : null}

      <section className="surface-card p-0">
        <div className="flex items-center justify-between border-b border-stone-200 px-6 py-4">
          <h2 className="text-base font-semibold text-stone-950">Liste</h2>
          <button
            type="button"
            onClick={() => void loadSessions()}
            className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
          >
            Rafraîchir
          </button>
        </div>

        {loading ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
        ) : sessions.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Aucune session.</div>
        ) : (
          <div className="grid gap-4 p-6 lg:grid-cols-2">
            {sessions.map((session) => (
              <article key={session.id} className="rounded-2xl border border-stone-200 bg-white/80 p-4">
                {editId === session.id ? (
                  <div className="space-y-3">
                    <input
                      value={editForm.title}
                      onChange={(event) => setEditForm((prev) => ({ ...prev, title: event.target.value }))}
                      className="input-field"
                    />
                    <div className="grid gap-3 sm:grid-cols-2">
                      <input
                        type="date"
                        value={editForm.startDate}
                        onChange={(event) => setEditForm((prev) => ({ ...prev, startDate: event.target.value }))}
                        className="input-field"
                      />
                      <input
                        type="date"
                        value={editForm.endDate}
                        onChange={(event) => setEditForm((prev) => ({ ...prev, endDate: event.target.value }))}
                        className="input-field"
                      />
                    </div>
                    <div className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-center">
                      <select
                        value={targetStatusById[session.id] ?? session.status}
                        onChange={(event) =>
                          setTargetStatusById((prev) => ({
                            ...prev,
                            [session.id]: event.target.value as SessionStatus,
                          }))
                        }
                        disabled={savingId === session.id}
                        className="rounded-xl border border-stone-200 bg-white px-3 py-2 text-sm text-stone-700 outline-none focus:border-[var(--ets-primary)] focus:ring-2 focus:ring-[rgba(220,4,44,0.15)] disabled:opacity-50"
                      >
                        {getValidTargets(session.status).map((s) => (
                          <option key={s} value={s}>{s}</option>
                        ))}
                      </select>
                      <button
                        type="button"
                        onClick={() => void applyStatus(session.id)}
                        disabled={savingId === session.id || (targetStatusById[session.id] ?? session.status) === session.status}
                        className="rounded-xl border-2 border-[var(--ets-primary)] bg-[rgba(220,4,44,0.08)] px-3 py-2 text-xs font-medium text-[var(--ets-primary)] hover:bg-[rgba(220,4,44,0.14)] disabled:opacity-50"
                      >
                        Appliquer statut
                      </button>
                    </div>
                    <div className="flex gap-2">
                      <button
                        type="button"
                        onClick={() => void saveEdit(session.id)}
                        disabled={savingId === session.id}
                        className="rounded-xl bg-stone-950 px-3 py-1.5 text-xs font-medium text-white hover:bg-stone-800 disabled:opacity-50"
                      >
                        Sauvegarder
                      </button>
                      <button
                        type="button"
                        onClick={() => setEditId(null)}
                        className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-700 hover:bg-stone-100"
                      >
                        Annuler
                      </button>
                    </div>
                  </div>
                ) : (
                  <>
                    <div className="flex items-center justify-between gap-3">
                      <h3 className="text-lg font-semibold text-stone-950">{session.title}</h3>
                      <SessionStatusBadge status={session.status} />
                    </div>
                    <p className="mt-2 text-sm text-stone-600">
                      {new Date(session.startDate).toLocaleDateString('fr-FR')} - {new Date(session.endDate).toLocaleDateString('fr-FR')}
                    </p>

                    <div className="mt-4 flex flex-wrap gap-2">
                      {canUpdateSessions ? (
                        <button
                          type="button"
                          onClick={() => startEdit(session)}
                          className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-700 hover:bg-stone-100"
                        >
                          Éditer
                        </button>
                      ) : null}

                      {canUpdateSessions && session.status === 'Draft' ? (
                        <button
                          type="button"
                          onClick={() => void transition(session.id, 'open')}
                          disabled={savingId === session.id}
                          className="rounded-xl bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
                        >
                          Open
                        </button>
                      ) : null}

                      {canUpdateSessions && session.status === 'Open' ? (
                        <button
                          type="button"
                          onClick={() => void transition(session.id, 'close')}
                          disabled={savingId === session.id}
                          className="rounded-xl bg-[var(--ets-primary)] px-3 py-1.5 text-xs font-medium text-white hover:bg-[var(--ets-primary-hover)] disabled:opacity-50"
                        >
                          Close
                        </button>
                      ) : null}

                      {canUpdateSessions && session.status === 'Closed' ? (
                        <button
                          type="button"
                          onClick={() => void transition(session.id, 'archive')}
                          disabled={savingId === session.id}
                          className="rounded-xl bg-stone-700 px-3 py-1.5 text-xs font-medium text-white hover:bg-stone-800 disabled:opacity-50"
                        >
                          Archive
                        </button>
                      ) : null}

                      {canDeleteSessions ? (
                        <button
                          type="button"
                          onClick={() => void removeSession(session.id)}
                          disabled={savingId === session.id}
                          className="rounded-xl border border-rose-200 px-3 py-1.5 text-xs text-rose-600 hover:bg-rose-50 disabled:opacity-50"
                        >
                          Supprimer
                        </button>
                      ) : null}
                    </div>
                  </>
                )}
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
