import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type { SessionResponse, SessionStatus } from '../types/sessions';

function SessionStatusBadge({ status }: { status: SessionStatus }) {
  const styles: Record<SessionStatus, string> = {
    Draft: 'bg-slate-100 text-slate-700 border-slate-200',
    Open: 'bg-emerald-100 text-emerald-700 border-emerald-200',
    Closed: 'bg-amber-100 text-amber-700 border-amber-200',
    Archived: 'bg-stone-200 text-stone-700 border-stone-300',
  };

  return <span className={`inline-flex rounded-xl border px-2.5 py-0.5 text-xs font-medium ${styles[status]}`}>{status}</span>;
}

export function NeedsPage() {
  const { apiFetch, user } = useAuth();
  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const isTeacher = user?.role === 'professor' || user?.role === 'course_instructor';
  const canAccessSessionNeeds = isTeacher || user?.role === 'lab_instructor' || user?.role === 'admin';

  const loadSessions = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const path = isTeacher ? '/sessions?active=true' : '/sessions';
      const data = await apiFetch<SessionResponse[]>(path);
      setSessions(data);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les sessions pour la saisie des besoins.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, isTeacher]);

  useEffect(() => {
    void loadSessions();
  }, [loadSessions]);

  return (
    <div className="space-y-6">
      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Besoins</p>
        <h1 className="mt-3 text-3xl font-semibold text-stone-950">Soumission et validation par session</h1>
        <p className="mt-3 max-w-3xl text-sm leading-7 text-stone-600 sm:text-base">
          Sélectionnez une session pour accéder à la vue enseignant (soumission) ou responsable technique
          (approbation/rejet), selon votre rôle.
        </p>
      </section>

      {error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : null}

      <section className="surface-card p-0">
        <div className="flex items-center justify-between border-b border-stone-200 px-6 py-4">
          <h2 className="text-base font-semibold text-stone-950">Sessions disponibles</h2>
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
          <div className="px-6 py-10 text-center text-sm text-stone-500">Aucune session disponible.</div>
        ) : (
          <div className="grid gap-4 p-6 lg:grid-cols-2">
            {sessions.map((session) => (
              <article key={session.id} className="rounded-2xl border border-stone-200 bg-white/80 p-4">
                <div className="flex items-center justify-between gap-3">
                  <p className="text-lg font-semibold text-stone-950">{session.title}</p>
                  <SessionStatusBadge status={session.status} />
                </div>
                <p className="mt-2 text-sm text-stone-600">
                  {new Date(session.startDate).toLocaleDateString('fr-FR')} - {new Date(session.endDate).toLocaleDateString('fr-FR')}
                </p>

                <div className="mt-4">
                  {canAccessSessionNeeds ? (
                    <div className="flex flex-wrap gap-2">
                      <Link
                        to={`/sessions/${session.id}/needs`}
                        className="inline-flex rounded-xl border-2 border-[var(--ets-primary)]/40 bg-[rgba(220,4,44,0.08)] px-3 py-1.5 text-xs font-medium text-[var(--ets-primary)] transition hover:bg-[rgba(220,4,44,0.14)]"
                      >
                        Ouvrir le workflow besoins
                      </Link>
                      {isTeacher ? (
                        <Link
                          to={`/sessions/${session.id}/needs?create=1`}
                          className="inline-flex rounded-xl bg-[var(--ets-primary)] px-3 py-1.5 text-xs font-medium !text-white transition hover:bg-[var(--ets-primary-hover)] hover:!text-white"
                        >
                          Créer un besoin
                        </Link>
                      ) : null}
                    </div>
                  ) : (
                    <span className="inline-flex rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-500">
                      Non accessible pour votre rôle
                    </span>
                  )}
                </div>
              </article>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
