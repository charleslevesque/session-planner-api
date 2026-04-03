import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import {
  toUiStatus,
  uiStatusLabel,
  uiStatusStyle,
  UI_STATUS_OPTIONS,
  type UiNeedStatus,
} from '../lib/needStatusMapping';
import type { MyNeedResponse, TeachingNeedStatus } from '../types/needs';
import type { SessionResponse } from '../types/sessions';

type FilterValue = 'all';

function canEdit(status: TeachingNeedStatus) {
  return status === 'Draft' || status === 'Submitted' || status === 'Rejected';
}

function canDelete(status: TeachingNeedStatus) {
  return status !== 'Approved';
}

export function MyRequestsPage() {
  const { apiFetch } = useAuth();
  const [searchParams] = useSearchParams();

  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [requests, setRequests] = useState<MyNeedResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedSessionId, setSelectedSessionId] = useState<number | FilterValue>('all');
  const [selectedCourseId, setSelectedCourseId] = useState<number | FilterValue>('all');
  const [selectedUiStatus, setSelectedUiStatus] = useState<UiNeedStatus | FilterValue>('all');

  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [actionError, setActionError] = useState('');

  const loadSessions = useCallback(async () => {
    try {
      const data = await apiFetch<SessionResponse[]>('/sessions?active=true');
      setSessions(data);
      return data;
    } catch {
      return [] as SessionResponse[];
    }
  }, [apiFetch]);

  const loadRequests = useCallback(async () => {
    try {
      const data = await apiFetch<MyNeedResponse[]>('/needs/mine');
      setRequests(data);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger vos demandes.'));
    }
  }, [apiFetch]);

  useEffect(() => {
    let active = true;

    const init = async () => {
      setLoading(true);
      setError('');

      const [loadedSessions] = await Promise.all([loadSessions(), loadRequests()]);

      if (!active) return;

      const paramSessionId = searchParams.get('sessionId');
      if (paramSessionId) {
        const parsed = Number(paramSessionId);
        if (Number.isFinite(parsed)) {
          setSelectedSessionId(parsed);
          setLoading(false);
          return;
        }
      }

      if (loadedSessions.length > 0) {
        setSelectedSessionId(loadedSessions[0].id);
      }

      setLoading(false);
    };

    void init();
    return () => { active = false; };
  }, [loadSessions, loadRequests, searchParams]);

  const handleDelete = useCallback(async (r: MyNeedResponse) => {
    setDeletingId(r.id);
    setActionError('');
    try {
      await apiFetch(`/sessions/${r.sessionId}/needs/${r.id}`, { method: 'DELETE' });
      setRequests((prev) => prev.filter((x) => x.id !== r.id));
      setConfirmDeleteId(null);
    } catch (err) {
      setActionError(getErrorMessage(err, 'Impossible de supprimer la demande.'));
    } finally {
      setDeletingId(null);
    }
  }, [apiFetch]);

  const courseOptions = useMemo(() => {
    const map = new Map<number, { id: number; code: string; name?: string }>();
    for (const r of requests) {
      if (!map.has(r.courseId)) {
        map.set(r.courseId, { id: r.courseId, code: r.courseCode, name: r.courseName });
      }
    }
    return Array.from(map.values()).sort((a, b) => a.code.localeCompare(b.code));
  }, [requests]);

  const filtered = useMemo(() => {
    return requests.filter((r) => {
      if (selectedSessionId !== 'all' && r.sessionId !== selectedSessionId) return false;
      if (selectedCourseId !== 'all' && r.courseId !== selectedCourseId) return false;
      if (selectedUiStatus !== 'all' && toUiStatus(r.status) !== selectedUiStatus) return false;
      return true;
    });
  }, [requests, selectedSessionId, selectedCourseId, selectedUiStatus]);

  return (
    <div className="space-y-6">
      <Link to="/besoins" className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]">
        &larr; Retour aux sessions
      </Link>

      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Besoins</p>
        <h1 className="mt-3 text-3xl font-semibold text-stone-950">Mes demandes</h1>
        <p className="mt-3 max-w-3xl text-sm leading-7 text-stone-600 sm:text-base">
          Retrouvez l&apos;ensemble de vos demandes de besoins, toutes sessions confondues.
        </p>
      </section>

      {error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : null}

      {actionError ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{actionError}</div>
      ) : null}

      <section className="surface-card p-0">
        <div className="flex flex-wrap items-center gap-3 border-b border-stone-200 px-6 py-4">
          <h2 className="mr-auto text-base font-semibold text-stone-950">Demandes</h2>

          <select
            value={selectedSessionId}
            onChange={(e) => setSelectedSessionId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
            className="input-field max-w-[200px] text-xs"
          >
            <option value="all">Toutes les sessions</option>
            {sessions.map((s) => (
              <option key={s.id} value={s.id}>{s.title}</option>
            ))}
          </select>

          <select
            value={selectedCourseId}
            onChange={(e) => setSelectedCourseId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
            className="input-field max-w-[200px] text-xs"
          >
            <option value="all">Tous les cours</option>
            {courseOptions.map((c) => (
              <option key={c.id} value={c.id}>{c.code}{c.name ? ` – ${c.name}` : ''}</option>
            ))}
          </select>

          <select
            value={selectedUiStatus}
            onChange={(e) => setSelectedUiStatus(e.target.value as UiNeedStatus | FilterValue)}
            className="input-field max-w-[180px] text-xs"
          >
            <option value="all">Tous les statuts</option>
            {UI_STATUS_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>

        {loading ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
        ) : filtered.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Aucune demande.</div>
        ) : (
          <div className="divide-y divide-stone-100">
            {filtered.map((r) => {
              const ui = toUiStatus(r.status);
              const isConfirming = confirmDeleteId === r.id;
              const isDeleting = deletingId === r.id;

              return (
                <div key={r.id} className="px-6 py-4">
                  <div className="flex flex-wrap items-center gap-4">
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-semibold text-stone-950">
                        {r.courseCode}{r.courseName ? ` – ${r.courseName}` : ''}
                      </p>
                      <p className="mt-0.5 text-xs text-stone-500">{r.sessionTitle}</p>
                    </div>

                    <span className={`shrink-0 rounded-xl border px-2.5 py-0.5 text-xs font-medium ${uiStatusStyle(ui)}`}>
                      {uiStatusLabel(ui)}
                    </span>

                    <p className="shrink-0 text-xs text-stone-400">
                      {new Date(r.createdAt).toLocaleDateString('fr-FR')}
                    </p>

                    <div className="flex shrink-0 items-center gap-2">
                      {canEdit(r.status) ? (
                        <Link
                          to={`/sessions/${r.sessionId}/courses/${r.courseId}/needs/${r.id}/edit`}
                          className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs font-medium text-stone-600 transition hover:bg-stone-50"
                        >
                          Modifier
                        </Link>
                      ) : null}

                      {canDelete(r.status) ? (
                        <button
                          type="button"
                          onClick={() => {
                            setActionError('');
                            setConfirmDeleteId(r.id);
                          }}
                          className="rounded-xl border border-rose-200 px-3 py-1.5 text-xs font-medium text-rose-600 transition hover:bg-rose-50"
                        >
                          Annuler
                        </button>
                      ) : null}
                    </div>
                  </div>

                  {isConfirming ? (
                    <div className="mt-3 flex items-center gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3">
                      <p className="flex-1 text-xs text-rose-700">
                        Supprimer définitivement cette demande&nbsp;?
                      </p>
                      <button
                        type="button"
                        disabled={isDeleting}
                        onClick={() => void handleDelete(r)}
                        className="rounded-xl bg-rose-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-rose-700 disabled:opacity-50"
                      >
                        {isDeleting ? 'Suppression…' : 'Confirmer'}
                      </button>
                      <button
                        type="button"
                        disabled={isDeleting}
                        onClick={() => setConfirmDeleteId(null)}
                        className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs font-medium text-stone-600 transition hover:bg-stone-50 disabled:opacity-50"
                      >
                        Retour
                      </button>
                    </div>
                  ) : null}
                </div>
              );
            })}
          </div>
        )}
      </section>
    </div>
  );
}
