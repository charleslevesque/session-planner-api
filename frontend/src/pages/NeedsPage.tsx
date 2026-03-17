import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type { SessionResponse } from '../types/sessions';

export function NeedsPage() {
  const { apiFetch, user } = useAuth();
  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadSessions = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const data = await apiFetch<SessionResponse[]>('/sessions');
      setSessions(data);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les sessions pour la saisie des besoins.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch]);

  useEffect(() => {
    void loadSessions();
  }, [loadSessions]);

  const canAccessSessionNeeds = user?.role === 'teacher' || user?.role === 'technician' || user?.role === 'admin';
  const isTeacher = user?.role === 'teacher';

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
                <p className="text-lg font-semibold text-stone-950">{session.title}</p>
                <p className="mt-2 text-sm text-stone-600">
                  {new Date(session.startDate).toLocaleDateString('fr-FR')} - {new Date(session.endDate).toLocaleDateString('fr-FR')}
                </p>

                <div className="mt-4">
                  {canAccessSessionNeeds ? (
                    <div className="flex flex-wrap gap-2">
                      <Link
                        to={`/sessions/${session.id}/needs`}
                        className="inline-flex rounded-xl border border-amber-300 bg-amber-50 px-3 py-1.5 text-xs font-medium text-amber-800 transition hover:bg-amber-100"
                      >
                        Ouvrir le workflow besoins
                      </Link>
                      {isTeacher ? (
                        <Link
                          to={`/sessions/${session.id}/needs?create=1`}
                          className="inline-flex rounded-xl bg-stone-950 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-stone-800"
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
