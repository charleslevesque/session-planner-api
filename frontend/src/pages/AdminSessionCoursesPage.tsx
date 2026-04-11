import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { SessionStatusBadge } from '../components/SessionStatusBadge';
import { getErrorMessage } from '../lib/api';
import type { CourseResponse } from '../types/needs';
import type { SessionCourseResponse, SessionResponse, SessionStatus } from '../types/sessions';

type FilterMode = 'all' | 'associated' | 'not-associated';

function normalizeStatus(status: SessionStatus | number | string): SessionStatus {
  if (typeof status === 'number') {
    const byValue: Record<number, SessionStatus> = { 1: 'Draft', 2: 'Open', 3: 'Closed', 4: 'Archived' };
    return byValue[status] ?? 'Draft';
  }
  const map: Record<string, SessionStatus> = { draft: 'Draft', open: 'Open', closed: 'Closed', archived: 'Archived' };
  return map[String(status).toLowerCase()] ?? (String(status) as SessionStatus) ?? 'Draft';
}

export function AdminSessionCoursesPage() {
  const { sessionId } = useParams();
  const id = Number(sessionId);
  const { apiFetch, user } = useAuth();

  const [session, setSession] = useState<SessionResponse | null>(null);
  const [allCourses, setAllCourses] = useState<CourseResponse[]>([]);
  const [associatedCourses, setAssociatedCourses] = useState<SessionCourseResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [filterMode, setFilterMode] = useState<FilterMode>('all');
  const [togglingId, setTogglingId] = useState<number | null>(null);

  const [showCopyModal, setShowCopyModal] = useState(false);
  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [copySourceId, setCopySourceId] = useState<number | null>(null);
  const [copying, setCopying] = useState(false);

  const canEdit = user?.role === 'admin' || user?.role === 'lab_instructor';
  const sessionStatus = session ? normalizeStatus(session.status) : null;
  const canModifyCourses = canEdit && (sessionStatus === 'Draft' || sessionStatus === 'Open');

  const associatedIds = useMemo(
    () => new Set(associatedCourses.map((c) => c.id)),
    [associatedCourses],
  );

  const loadData = useCallback(async () => {
    if (!Number.isFinite(id)) {
      setError('Session invalide.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const [sessionData, coursesData, assocData] = await Promise.all([
        apiFetch<SessionResponse>(`/sessions/${id}`),
        apiFetch<CourseResponse[]>('/courses'),
        apiFetch<SessionCourseResponse[]>(`/sessions/${id}/courses`),
      ]);

      setSession(sessionData);
      setAllCourses(coursesData);
      setAssociatedCourses(assocData);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les données.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, id]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const filteredCourses = useMemo(() => {
    let result = allCourses;

    if (filterMode === 'associated') {
      result = result.filter((c) => associatedIds.has(c.id));
    } else if (filterMode === 'not-associated') {
      result = result.filter((c) => !associatedIds.has(c.id));
    }

    const q = search.trim().toLowerCase();
    if (q) {
      result = result.filter(
        (c) => c.code.toLowerCase().includes(q) || (c.name?.toLowerCase().includes(q) ?? false),
      );
    }

    return result;
  }, [allCourses, filterMode, associatedIds, search]);

  const associatedCount = associatedCourses.length;

  async function toggleAssociation(courseId: number, isAssociated: boolean) {
    if (!canModifyCourses) return;

    setTogglingId(courseId);
    setError('');

    try {
      const newIds = isAssociated
        ? [...associatedIds].filter((cid) => cid !== courseId)
        : [...associatedIds, courseId];

      const result = await apiFetch<SessionCourseResponse[]>(`/sessions/${id}/courses`, {
        method: 'PUT',
        body: JSON.stringify({ courseIds: newIds }),
      });

      setAssociatedCourses(result);
    } catch (err) {
      setError(getErrorMessage(err, "Erreur lors de la modification de l'association."));
    } finally {
      setTogglingId(null);
    }
  }

  async function openCopyModal() {
    setShowCopyModal(true);
    setCopySourceId(null);
    try {
      const allSessions = await apiFetch<SessionResponse[]>('/sessions');
      setSessions(allSessions.filter((s) => s.id !== id));
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les sessions.'));
      setShowCopyModal(false);
    }
  }

  async function handleCopyCourses() {
    if (!copySourceId) return;

    setCopying(true);
    setError('');

    try {
      const sourceCourses = await apiFetch<SessionCourseResponse[]>(`/sessions/${copySourceId}/courses`);
      const mergedIds = [...new Set([...associatedIds, ...sourceCourses.map((c) => c.id)])];

      const result = await apiFetch<SessionCourseResponse[]>(`/sessions/${id}/courses`, {
        method: 'PUT',
        body: JSON.stringify({ courseIds: mergedIds }),
      });

      setAssociatedCourses(result);
      setShowCopyModal(false);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de copier les cours.'));
    } finally {
      setCopying(false);
    }
  }

  return (
    <div className="space-y-6">
      <Link to="/sessions/manage" className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]">
        &larr; Retour aux sessions
      </Link>

      {loading ? (
        <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>
      ) : error && !session ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : (
        <>
          <section className="surface-card p-6 sm:p-8">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="text-xs uppercase tracking-[0.3em] text-stone-500">Session</p>
                <h1 className="mt-2 text-2xl font-semibold text-stone-950 sm:text-3xl">{session?.title}</h1>
                <p className="mt-2 text-sm text-stone-600">
                  {session
                    ? `${new Date(session.startDate).toLocaleDateString('fr-FR')} – ${new Date(session.endDate).toLocaleDateString('fr-FR')}`
                    : ''}
                </p>
                <p className="mt-2 text-sm text-stone-600">
                  Gérez les ressources associées à cette session. Utilisez les boutons{' '}
                  <span className="font-semibold text-emerald-600">Associer</span> /{' '}
                  <span className="font-semibold text-emerald-600">Dissocier</span> pour lier ou retirer
                  des cours de cette session.
                </p>
              </div>
              {session ? <SessionStatusBadge status={normalizeStatus(session.status)} /> : null}
            </div>

            {!canModifyCourses && canEdit ? (
              <div className="mt-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
                La modification des cours est uniquement autorisée pour les sessions Draft et Open.
              </div>
            ) : null}
          </section>

          {error ? (
            <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
          ) : null}

          <section className="surface-card overflow-hidden p-0">
            <div className="border-b border-stone-200 px-6 py-4">
              <h2 className="text-base font-semibold text-stone-950">Cours associés</h2>
              <p className="mt-1 text-xs text-stone-500">
                Cliquez sur <span className="font-semibold text-emerald-600">Associer</span> ou{' '}
                <span className="font-semibold text-emerald-600">✓ Dissocier</span> pour gérer les
                liens entre cette session et ses cours.{' '}
                <span className="text-stone-400">
                  Dissocier retire uniquement l&apos;association à la session — la ressource reste dans le catalogue global.
                </span>
              </p>
            </div>

            <div className="space-y-3 border-b border-stone-200 px-6 py-3">
              <div className="flex items-center justify-between">
                <span className="text-sm text-stone-600">
                  {filteredCourses.length}/{allCourses.length} cours
                  <span className="ml-2 text-emerald-600">
                    · {associatedCount} associé{associatedCount !== 1 ? 's' : ''}
                  </span>
                </span>
                <div className="flex gap-2">
                  <button
                    type="button"
                    onClick={() => void loadData()}
                    className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
                  >
                    Rafraîchir
                  </button>
                  {canModifyCourses ? (
                    <button
                      type="button"
                      onClick={() => void openCopyModal()}
                      className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
                    >
                      Copier depuis une session
                    </button>
                  ) : null}
                </div>
              </div>

              <div className="flex flex-wrap items-center gap-2">
                <div className="flex rounded-xl border border-stone-200 p-0.5">
                  {(['all', 'associated', 'not-associated'] as FilterMode[]).map((mode) => {
                    const labels: Record<FilterMode, string> = {
                      all: 'Tous',
                      associated: 'Associés',
                      'not-associated': 'Non associés',
                    };
                    return (
                      <button
                        key={mode}
                        type="button"
                        onClick={() => setFilterMode(mode)}
                        className={[
                          'rounded-lg px-3 py-1 text-xs font-medium transition',
                          filterMode === mode
                            ? 'bg-stone-900 text-white'
                            : 'text-stone-600 hover:bg-stone-50',
                        ].join(' ')}
                      >
                        {labels[mode]}
                      </button>
                    );
                  })}
                </div>

                <input
                  type="text"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  placeholder="Rechercher par code ou nom..."
                  className="input-field max-w-xs !py-1.5 text-xs"
                />
              </div>
            </div>

            {filteredCourses.length === 0 ? (
              <div className="px-6 py-10 text-center text-sm text-stone-500">
                {search
                  ? 'Aucun cours ne correspond à votre recherche.'
                  : filterMode === 'associated'
                    ? 'Aucun cours associé à cette session.'
                    : 'Aucun cours disponible.'}
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead>
                    <tr className="border-b border-stone-200 bg-stone-50/50">
                      <th className="w-32 px-6 py-3 text-xs font-medium uppercase tracking-wider text-stone-500">Association</th>
                      <th className="px-6 py-3 text-xs font-medium uppercase tracking-wider text-stone-500">Code</th>
                      <th className="px-6 py-3 text-xs font-medium uppercase tracking-wider text-stone-500">Nom</th>
                      <th className="px-6 py-3 text-xs font-medium uppercase tracking-wider text-stone-500" />
                    </tr>
                  </thead>
                  <tbody>
                    {filteredCourses.map((course) => {
                      const linked = associatedIds.has(course.id);
                      const toggling = togglingId === course.id;

                      return (
                        <tr key={course.id} className="border-b border-stone-100 hover:bg-stone-50/40">
                          <td className="px-6 py-3">
                            {canModifyCourses ? (
                              <button
                                type="button"
                                disabled={toggling}
                                onClick={() => void toggleAssociation(course.id, linked)}
                                className={[
                                  'rounded-xl border px-3 py-1 text-xs font-medium transition',
                                  linked
                                    ? 'border-emerald-200 bg-emerald-50 text-emerald-700 hover:bg-emerald-100'
                                    : 'border-stone-200 bg-white text-stone-600 hover:bg-stone-50',
                                  toggling ? 'opacity-50' : '',
                                ].join(' ')}
                              >
                                {toggling ? '...' : linked ? '✓ Dissocier' : 'Associer'}
                              </button>
                            ) : linked ? (
                              <span className="inline-flex items-center rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-semibold text-emerald-700">✓</span>
                            ) : (
                              <span className="text-stone-300">—</span>
                            )}
                          </td>
                          <td className="px-6 py-3 font-medium text-stone-950">{course.code}</td>
                          <td className="px-6 py-3 text-stone-600">{course.name ?? '—'}</td>
                          <td className="px-6 py-3 text-right">
                            {linked ? (
                              <Link
                                to={`/sessions/${id}/courses/${course.id}`}
                                className="text-xs text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]"
                              >
                                Voir les ressources &rarr;
                              </Link>
                            ) : null}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </section>

          {showCopyModal ? (
            <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/30">
              <div className="mx-4 w-full max-w-md rounded-2xl border border-stone-200 bg-white p-6 shadow-xl">
                <h3 className="text-lg font-semibold text-stone-950">Copier les cours d&apos;une session</h3>
                <p className="mt-2 text-sm text-stone-600">
                  Les cours de la session source seront ajoutés aux cours déjà associés. Il s&apos;agit d&apos;une copie par valeur.
                </p>

                <select
                  value={copySourceId ?? ''}
                  onChange={(e) => setCopySourceId(e.target.value ? Number(e.target.value) : null)}
                  className="mt-4 w-full rounded-xl border border-stone-200 bg-white px-3 py-2 text-sm text-stone-700 outline-none focus:border-[var(--ets-primary)] focus:ring-2 focus:ring-[rgba(220,4,44,0.15)]"
                >
                  <option value="">Sélectionner une session...</option>
                  {sessions.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.title} ({normalizeStatus(s.status)})
                    </option>
                  ))}
                </select>

                <div className="mt-6 flex justify-end gap-2">
                  <button
                    type="button"
                    onClick={() => setShowCopyModal(false)}
                    className="rounded-xl border border-stone-300 px-4 py-2 text-xs text-stone-700 hover:bg-stone-100"
                  >
                    Annuler
                  </button>
                  <button
                    type="button"
                    onClick={() => void handleCopyCourses()}
                    disabled={!copySourceId || copying}
                    className="rounded-xl bg-[#DC0C2C] px-4 py-2 text-xs font-medium text-white hover:bg-[#b80a24] disabled:opacity-50"
                  >
                    {copying ? 'Copie...' : 'Copier'}
                  </button>
                </div>
              </div>
            </div>
          ) : null}
        </>
      )}
    </div>
  );
}
