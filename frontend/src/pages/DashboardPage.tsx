import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type { TeachingNeedResponse } from '../types/needs';
import type { SessionResponse, SessionStatus } from '../types/sessions';

interface SessionStats {
  submitted: number;
  total: number;
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

export function DashboardPage() {
  const { apiFetch, user } = useAuth();
  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [statsBySession, setStatsBySession] = useState<Record<number, SessionStats>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [createError, setCreateError] = useState('');
  const [createSuccess, setCreateSuccess] = useState('');
  const [creatingSession, setCreatingSession] = useState(false);
  const [transitionLoading, setTransitionLoading] = useState<Record<number, boolean>>({});
  const [newSession, setNewSession] = useState({
    title: '',
    startDate: '',
    endDate: '',
  });

  const canTransitionSessions = user?.role === 'technician' || user?.role === 'admin';
  const canCreateSessions = user?.role === 'admin';
  const canAccessNeeds = user?.role === 'teacher' || user?.role === 'technician' || user?.role === 'admin';

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const sessionsData = await apiFetch<SessionResponse[]>('/sessions');
      setSessions(sessionsData);

      const statsEntries = await Promise.all(
        sessionsData.map(async (session) => {
          try {
            const needs = await apiFetch<TeachingNeedResponse[]>(`/sessions/${session.id}/needs`);
            const submitted = needs.filter((need) => need.status !== 'Draft').length;
            return [session.id, { submitted, total: needs.length }] as const;
          } catch {
            return [session.id, { submitted: 0, total: 0 }] as const;
          }
        }),
      );

      setStatsBySession(Object.fromEntries(statsEntries));
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les sessions.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const summary = useMemo(() => {
    const totalSessions = sessions.length;
    const openSessions = sessions.filter((session) => session.status === 'Open').length;
    const submitted = Object.values(statsBySession).reduce((acc, value) => acc + value.submitted, 0);
    const totalNeeds = Object.values(statsBySession).reduce((acc, value) => acc + value.total, 0);

    return { totalSessions, openSessions, submitted, totalNeeds };
  }, [sessions, statsBySession]);

  async function handleTransition(sessionId: number, action: 'open' | 'close' | 'archive') {
    setTransitionLoading((prev) => ({ ...prev, [sessionId]: true }));
    setError('');

    try {
      await apiFetch(`/sessions/${sessionId}/${action}`, { method: 'POST' });
      await loadData();
    } catch (err) {
      setError(getErrorMessage(err, 'Transition impossible pour cette session.'));
    } finally {
      setTransitionLoading((prev) => ({ ...prev, [sessionId]: false }));
    }
  }

  async function handleCreateSession(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setCreateError('');
    setCreateSuccess('');

    if (!newSession.title.trim()) {
      setCreateError('Le titre de session est requis.');
      return;
    }

    if (!newSession.startDate || !newSession.endDate) {
      setCreateError('Les dates de début et de fin sont requises.');
      return;
    }

    if (newSession.endDate <= newSession.startDate) {
      setCreateError('La date de fin doit être après la date de début.');
      return;
    }

    setCreatingSession(true);

    try {
      await apiFetch('/sessions', {
        method: 'POST',
        body: JSON.stringify({
          title: newSession.title.trim(),
          startDate: newSession.startDate,
          endDate: newSession.endDate,
        }),
      });

      setCreateSuccess('Session créée avec succès.');
      setNewSession({ title: '', startDate: '', endDate: '' });
      await loadData();
    } catch (err) {
      setCreateError(getErrorMessage(err, 'Impossible de créer la session.'));
    } finally {
      setCreatingSession(false);
    }
  }

  function renderTransitionButton(session: SessionResponse) {
    if (!canTransitionSessions) return null;

    if (session.status === 'Draft') {
      return (
        <button
          type="button"
          onClick={() => void handleTransition(session.id, 'open')}
          disabled={transitionLoading[session.id]}
          className="rounded-xl bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
        >
          Open
        </button>
      );
    }

    if (session.status === 'Open') {
      return (
        <button
          type="button"
          onClick={() => void handleTransition(session.id, 'close')}
          disabled={transitionLoading[session.id]}
          className="rounded-xl bg-amber-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-amber-700 disabled:opacity-50"
        >
          Close
        </button>
      );
    }

    if (session.status === 'Closed') {
      return (
        <button
          type="button"
          onClick={() => void handleTransition(session.id, 'archive')}
          disabled={transitionLoading[session.id]}
          className="rounded-xl bg-stone-700 px-3 py-1.5 text-xs font-medium text-white hover:bg-stone-800 disabled:opacity-50"
        >
          Archive
        </button>
      );
    }

    return null;
  }

  return (
    <div className="space-y-8">
      <section className="rounded-[2rem] bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.32),_transparent_32%),linear-gradient(135deg,_#20150d_0%,_#3f2b1d_45%,_#8a5a31_100%)] px-6 py-8 text-white sm:px-8">
        <p className="text-xs uppercase tracking-[0.35em] text-amber-100/80">Dashboard</p>
        <h1 className="mt-4 max-w-2xl text-3xl font-semibold sm:text-4xl">Sessions et suivi des besoins</h1>
        <p className="mt-4 max-w-2xl text-sm leading-7 text-amber-50/80 sm:text-base">
          Consultez le cycle de vie des sessions, puis accédez à la saisie ou la validation des besoins pédagogiques.
        </p>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <article className="surface-card p-6">
          <p className="text-sm uppercase tracking-[0.3em] text-stone-500">Sessions</p>
          <p className="mt-3 text-4xl font-semibold text-stone-950">{summary.totalSessions}</p>
        </article>
        <article className="surface-card p-6">
          <p className="text-sm uppercase tracking-[0.3em] text-stone-500">Sessions ouvertes</p>
          <p className="mt-3 text-4xl font-semibold text-stone-950">{summary.openSessions}</p>
        </article>
        <article className="surface-card p-6">
          <p className="text-sm uppercase tracking-[0.3em] text-stone-500">Besoins soumis</p>
          <p className="mt-3 text-4xl font-semibold text-stone-950">{summary.submitted}</p>
        </article>
        <article className="surface-card p-6">
          <p className="text-sm uppercase tracking-[0.3em] text-stone-500">Besoins total</p>
          <p className="mt-3 text-4xl font-semibold text-stone-950">{summary.totalNeeds}</p>
        </article>
      </section>

      {canCreateSessions ? (
        <section className="surface-card p-6 sm:p-8">
          <h2 className="text-base font-semibold text-stone-950">Créer une session</h2>
          <p className="mt-1 text-sm text-stone-600">Disponible uniquement pour l&apos;admin.</p>

          {createError ? (
            <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{createError}</div>
          ) : null}

          {createSuccess ? (
            <div className="mt-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{createSuccess}</div>
          ) : null}

          <form className="mt-5 grid gap-4 md:grid-cols-[1.4fr_1fr_1fr_auto]" onSubmit={handleCreateSession}>
            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Titre</span>
              <input
                value={newSession.title}
                onChange={(event) => setNewSession((prev) => ({ ...prev, title: event.target.value }))}
                className="input-field"
                placeholder="Session Automne"
                required
              />
            </label>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Date début</span>
              <input
                type="date"
                value={newSession.startDate}
                onChange={(event) => setNewSession((prev) => ({ ...prev, startDate: event.target.value }))}
                className="input-field"
                required
              />
            </label>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Date fin</span>
              <input
                type="date"
                value={newSession.endDate}
                onChange={(event) => setNewSession((prev) => ({ ...prev, endDate: event.target.value }))}
                className="input-field"
                required
              />
            </label>

            <div className="flex items-end">
              <button type="submit" disabled={creatingSession} className="primary-button w-full md:w-auto">
                {creatingSession ? 'Création...' : 'Créer session'}
              </button>
            </div>
          </form>
        </section>
      ) : null}

      {error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : null}

      <section className="surface-card p-0">
        <div className="flex items-center justify-between border-b border-stone-200 px-6 py-4">
          <h2 className="text-base font-semibold text-stone-950">Liste des sessions</h2>
          <button
            type="button"
            onClick={() => void loadData()}
            className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
          >
            Rafraîchir
          </button>
        </div>

        {loading ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
        ) : sessions.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Aucune session disponible.</div>
        ) : (
          <div className="grid gap-4 p-6 lg:grid-cols-2">
            {sessions.map((session) => {
              const stats = statsBySession[session.id] ?? { submitted: 0, total: 0 };

              return (
                <article key={session.id} className="rounded-2xl border border-stone-200 bg-white/80 p-5">
                  <div className="flex items-center justify-between gap-3">
                    <h3 className="text-lg font-semibold text-stone-950">{session.title}</h3>
                    <SessionStatusBadge status={session.status} />
                  </div>

                  <p className="mt-2 text-sm text-stone-600">
                    {new Date(session.startDate).toLocaleDateString('fr-FR')} - {new Date(session.endDate).toLocaleDateString('fr-FR')}
                  </p>

                  <p className="mt-4 rounded-xl bg-stone-50 px-3 py-2 text-sm text-stone-700">
                    {stats.submitted} besoins soumis / {stats.total} total
                  </p>

                  <div className="mt-4 flex flex-wrap gap-2">
                    {canAccessNeeds ? (
                      <Link
                        to={`/sessions/${session.id}/needs`}
                        className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs font-medium text-stone-700 transition hover:bg-stone-100"
                      >
                        Ouvrir besoins
                      </Link>
                    ) : null}
                    {renderTransitionButton(session)}
                  </div>
                </article>
              );
            })}
          </div>
        )}
      </section>
    </div>
  );
}
