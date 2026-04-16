import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useParams, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { SessionStatusBadge } from '../components/SessionStatusBadge';
import { getErrorMessage } from '../lib/api';
import type { TeachingNeedResponse, TeachingNeedItemResponse } from '../types/needs';
import type { SessionCourseResponse, SessionResponse, SessionStatus } from '../types/sessions';

interface RenewableCourse {
  courseId: number;
  courseCode: string;
  courseName: string | null;
  sourceNeedId: number;
  sourceSessionId: number;
  sourceSessionTitle: string;
  itemCount: number;
}

interface RenewResult {
  need: TeachingNeedResponse;
  changes: string[];
}

interface RenewAllResult {
  renewed: RenewResult[];
  totalCourses: number;
  totalItems: number;
}

function normalizeStatus(status: SessionStatus | number | string): SessionStatus {
  if (typeof status === 'number') {
    const byValue: Record<number, SessionStatus> = { 1: 'Draft', 2: 'Open', 3: 'Closed', 4: 'Archived' };
    return byValue[status] ?? 'Draft';
  }
  const map: Record<string, SessionStatus> = { draft: 'Draft', open: 'Open', closed: 'Closed', archived: 'Archived' };
  return map[String(status).toLowerCase()] ?? (String(status) as SessionStatus) ?? 'Draft';
}

function itemLabel(item: TeachingNeedItemResponse): string {
  if (item.softwareName) {
    const ver = item.softwareVersionNumber ? ` (${item.softwareVersionNumber})` : '';
    return `${item.softwareName}${ver}`;
  }
  return item.description ?? item.itemType;
}

export function TeacherSessionCoursesPage() {
  const { sessionId } = useParams();
  const id = Number(sessionId);
  const { apiFetch } = useAuth();
  const navigate = useNavigate();

  const [session, setSession] = useState<SessionResponse | null>(null);
  const [courses, setCourses] = useState<SessionCourseResponse[]>([]);
  const [needs, setNeeds] = useState<TeachingNeedResponse[]>([]);
  const [renewables, setRenewables] = useState<RenewableCourse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  const [renewingCourseId, setRenewingCourseId] = useState<number | null>(null);
  const [renewingAll, setRenewingAll] = useState(false);
  const [renewResult, setRenewResult] = useState<RenewResult | null>(null);
  const [renewAllResult, setRenewAllResult] = useState<RenewAllResult | null>(null);
  const [renewError, setRenewError] = useState('');

  const loadData = useCallback(async () => {
    if (!Number.isFinite(id)) {
      setError('Session invalide.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const [sessionData, coursesData, needsData] = await Promise.all([
        apiFetch<SessionResponse>(`/sessions/${id}`),
        apiFetch<SessionCourseResponse[]>(`/sessions/${id}/courses`),
        apiFetch<TeachingNeedResponse[]>(`/sessions/${id}/needs`),
      ]);

      setSession(sessionData);
      setCourses(coursesData);
      setNeeds(needsData);

      if (normalizeStatus(sessionData.status) === 'Open') {
        try {
          const renewable = await apiFetch<RenewableCourse[]>(`/sessions/${id}/needs/renewable-courses`);
          setRenewables(renewable);
        } catch {
          setRenewables([]);
        }
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les données.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, id]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const renewableMap = useMemo(() => {
    const map = new Map<number, RenewableCourse>();
    for (const r of renewables) map.set(r.courseId, r);
    return map;
  }, [renewables]);

  const courseNeedCounts = useMemo(() => {
    const map = new Map<number, number>();
    for (const need of needs) {
      if (need.status !== 'Approved') {
        map.set(need.courseId, (map.get(need.courseId) ?? 0) + 1);
      }
    }
    return map;
  }, [needs]);

  const filteredCourses = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return courses;
    return courses.filter(
      (c) => c.code.toLowerCase().includes(q) || (c.name?.toLowerCase().includes(q) ?? false),
    );
  }, [courses, search]);

  const sessionStatus = session ? normalizeStatus(session.status) : null;
  const isBusy = renewingCourseId !== null || renewingAll;

  const handleRenew = async (courseId: number) => {
    setRenewingCourseId(courseId);
    setRenewError('');
    try {
      const result = await apiFetch<RenewResult>(`/sessions/${id}/needs/renew/${courseId}`, { method: 'POST' });
      setRenewResult(result);
      void loadData();
    } catch (err) {
      setRenewError(getErrorMessage(err, 'Impossible de renouveler cette demande.'));
    } finally {
      setRenewingCourseId(null);
    }
  };

  const handleRenewAll = async () => {
    setRenewingAll(true);
    setRenewError('');
    try {
      const result = await apiFetch<RenewAllResult>(`/sessions/${id}/needs/renew-all`, { method: 'POST' });
      setRenewAllResult(result);
      void loadData();
    } catch (err) {
      setRenewError(getErrorMessage(err, 'Impossible de renouveler les demandes.'));
    } finally {
      setRenewingAll(false);
    }
  };

  const closeModal = () => {
    setRenewResult(null);
    setRenewAllResult(null);
  };

  const goToEdit = () => {
    if (!renewResult) return;
    const n = renewResult.need;
    navigate(`/sessions/${id}/courses/${n.courseId}/needs/${n.id}/edit`);
    setRenewResult(null);
  };

  return (
    <div className="space-y-6">
      <Link to="/besoins" className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]">
        &larr; Retour aux sessions
      </Link>

      {loading ? (
        <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>
      ) : error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : (
        <>
          <section className="surface-card p-6 sm:p-8">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="text-xs uppercase tracking-[0.3em] text-stone-500">Session</p>
                <h1 className="mt-2 text-2xl font-semibold text-stone-950 sm:text-3xl">{session?.title}</h1>
                <p className="mt-2 text-sm text-stone-600">
                  {session ? `${new Date(session.startDate).toLocaleDateString('fr-FR')} – ${new Date(session.endDate).toLocaleDateString('fr-FR')}` : ''}
                </p>
              </div>
              {session ? <SessionStatusBadge status={normalizeStatus(session.status)} /> : null}
            </div>

            {sessionStatus !== 'Open' ? (
              <div className="mt-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
                Cette session n&apos;accepte pas de besoins actuellement. Vous pouvez consulter les ressources des cours en lecture seule.
              </div>
            ) : null}
          </section>

          {renewables.length > 0 && sessionStatus === 'Open' ? (
            <section className="surface-card p-0">
              <div className="flex flex-wrap items-center justify-between gap-3 border-b border-stone-200 px-6 py-4">
                <div>
                  <h2 className="text-base font-semibold text-stone-950">Renouvellement rapide</h2>
                  <p className="mt-1 text-xs text-stone-500">
                    Ces cours avaient des demandes approuvées lors d&apos;une session précédente.
                    Les cours seront automatiquement associés à cette session.
                  </p>
                </div>
                <button
                  type="button"
                  disabled={isBusy}
                  onClick={() => void handleRenewAll()}
                  className="shrink-0 rounded-xl border-2 border-emerald-400/60 bg-emerald-500 px-4 py-2 text-xs font-semibold text-white transition hover:bg-emerald-600 disabled:opacity-50"
                >
                  {renewingAll ? 'Renouvellement...' : `Tout renouveler (${renewables.length})`}
                </button>
              </div>

              {renewError ? (
                <div className="mx-6 mt-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-2.5 text-sm text-rose-700">{renewError}</div>
              ) : null}

              <div className="grid gap-3 p-6 lg:grid-cols-2 xl:grid-cols-3">
                {renewables.map((r) => (
                  <div
                    key={r.courseId}
                    className="rounded-2xl border border-emerald-200 bg-emerald-50/50 p-4"
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div className="min-w-0">
                        <p className="text-sm font-semibold text-stone-950">{r.courseCode}</p>
                        {r.courseName ? <p className="mt-0.5 text-xs text-stone-600">{r.courseName}</p> : null}
                      </div>
                      <span className="shrink-0 rounded-lg border border-emerald-300 bg-emerald-100 px-2 py-0.5 text-[10px] font-medium text-emerald-800">
                        {r.itemCount} item{r.itemCount > 1 ? 's' : ''}
                      </span>
                    </div>
                    <p className="mt-2 text-xs text-stone-500">
                      Depuis : {r.sourceSessionTitle}
                    </p>
                    <button
                      type="button"
                      disabled={isBusy}
                      onClick={() => void handleRenew(r.courseId)}
                      className="mt-3 w-full rounded-xl border-2 border-emerald-400/60 bg-emerald-500 px-3 py-2 text-xs font-semibold text-white transition hover:bg-emerald-600 disabled:opacity-50"
                    >
                      {renewingCourseId === r.courseId ? 'Renouvellement...' : 'Renouveler'}
                    </button>
                  </div>
                ))}
              </div>
            </section>
          ) : null}

          <section className="surface-card p-0">
            <div className="flex flex-wrap items-center justify-between gap-3 border-b border-stone-200 px-6 py-4">
              <h2 className="text-base font-semibold text-stone-950">Cours disponibles</h2>
              <input
                type="search"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Rechercher un cours..."
                className="input-field max-w-xs"
              />
            </div>

            {filteredCourses.length === 0 ? (
              <div className="px-6 py-10 text-center text-sm text-stone-500">
                {search ? 'Aucun cours ne correspond à votre recherche.' : 'Aucun cours disponible pour cette session.'}
              </div>
            ) : (
              <div className="grid gap-3 p-6 lg:grid-cols-2 xl:grid-cols-3">
                {filteredCourses.map((course) => {
                  const needCount = courseNeedCounts.get(course.id) ?? 0;
                  const renewable = renewableMap.get(course.id);

                  return (
                    <div
                      key={course.id}
                      className="group rounded-2xl border border-stone-200 bg-white/80 p-4 transition hover:border-[var(--ets-primary)]/30 hover:bg-[rgba(220,4,44,0.04)] hover:shadow-sm"
                    >
                      <Link to={`/sessions/${id}/courses/${course.id}`}>
                        <div className="flex items-start justify-between gap-2">
                          <div className="min-w-0">
                            <p className="text-lg font-semibold text-stone-950 group-hover:text-[var(--ets-primary)]">{course.code}</p>
                            {course.name ? <p className="mt-1 text-sm text-stone-600">{course.name}</p> : null}
                          </div>
                          <div className="flex shrink-0 flex-col items-end gap-1">
                            {needCount > 0 ? (
                              <span className="rounded-xl border border-blue-200 bg-blue-50 px-2 py-0.5 text-xs font-medium text-blue-700">
                                {needCount} demande{needCount > 1 ? 's' : ''} active{needCount > 1 ? 's' : ''}
                              </span>
                            ) : null}
                            {renewable ? (
                              <span className="rounded-xl border border-emerald-200 bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700">
                                Renouvelable
                              </span>
                            ) : null}
                          </div>
                        </div>
                        <p className="mt-3 text-xs text-stone-400 group-hover:text-[var(--ets-primary)]/60">
                          Voir les ressources &rarr;
                        </p>
                      </Link>
                    </div>
                  );
                })}
              </div>
            )}
          </section>
        </>
      )}

      {/* Single renewal modal */}
      {renewResult ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-lg rounded-2xl bg-white shadow-xl">
            <div className="border-b border-stone-200 px-6 py-4">
              <h3 className="text-lg font-semibold text-stone-950">Demande renouvelée</h3>
              <p className="mt-1 text-sm text-stone-600">
                {renewResult.need.courseCode}{renewResult.need.courseName ? ` — ${renewResult.need.courseName}` : ''}
              </p>
            </div>

            <div className="max-h-[60vh] overflow-y-auto px-6 py-4">
              {renewResult.changes.length > 0 ? (
                <div className="space-y-2">
                  <p className="text-xs font-semibold uppercase tracking-wider text-stone-500">Changements</p>
                  <ul className="space-y-1.5">
                    {renewResult.changes.map((c, i) => (
                      <li key={i} className="flex items-start gap-2 text-sm text-stone-700">
                        <span className="mt-0.5 shrink-0 text-emerald-500">&#10003;</span>
                        {c}
                      </li>
                    ))}
                  </ul>
                </div>
              ) : null}

              <div className="mt-4">
                <p className="text-xs font-semibold uppercase tracking-wider text-stone-500">Items inclus</p>
                <ul className="mt-2 space-y-1">
                  {renewResult.need.items.map((item) => (
                    <li key={item.id} className="flex items-center gap-2 text-sm text-stone-700">
                      <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-stone-400" />
                      {itemLabel(item)}
                    </li>
                  ))}
                </ul>
              </div>
            </div>

            <div className="flex items-center justify-end gap-3 border-t border-stone-200 px-6 py-4">
              <button
                type="button"
                onClick={closeModal}
                className="rounded-xl border border-stone-200 px-4 py-2 text-sm text-stone-600 transition hover:bg-stone-50"
              >
                Fermer
              </button>
              <button
                type="button"
                onClick={goToEdit}
                className="rounded-xl border-2 border-[var(--ets-primary)]/40 bg-[rgba(220,4,44,0.08)] px-4 py-2 text-sm font-medium text-[var(--ets-primary)] transition hover:bg-[rgba(220,4,44,0.14)]"
              >
                Modifier la demande
              </button>
            </div>
          </div>
        </div>
      ) : null}

      {/* Bulk renewal modal */}
      {renewAllResult ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-2xl rounded-2xl bg-white shadow-xl">
            <div className="border-b border-stone-200 px-6 py-4">
              <h3 className="text-lg font-semibold text-stone-950">Renouvellement complet</h3>
              <p className="mt-1 text-sm text-stone-600">
                {renewAllResult.totalCourses} cours renouvelé{renewAllResult.totalCourses > 1 ? 's' : ''} &middot; {renewAllResult.totalItems} item{renewAllResult.totalItems > 1 ? 's' : ''} au total
              </p>
            </div>

            <div className="max-h-[60vh] overflow-y-auto px-6 py-4 space-y-5">
              {renewAllResult.renewed.map((r) => (
                <div key={r.need.id} className="rounded-xl border border-stone-200 p-4">
                  <p className="text-sm font-semibold text-stone-950">
                    {r.need.courseCode}{r.need.courseName ? ` — ${r.need.courseName}` : ''}
                  </p>

                  {r.changes.length > 0 ? (
                    <ul className="mt-2 space-y-1">
                      {r.changes.map((c, i) => (
                        <li key={i} className="flex items-start gap-2 text-xs text-stone-600">
                          <span className="mt-0.5 shrink-0 text-emerald-500">&#10003;</span>
                          {c}
                        </li>
                      ))}
                    </ul>
                  ) : null}

                  <div className="mt-2 flex flex-wrap gap-1.5">
                    {r.need.items.map((item) => (
                      <span key={item.id} className="rounded-lg border border-stone-200 bg-stone-50 px-2 py-0.5 text-[10px] text-stone-600">
                        {itemLabel(item)}
                      </span>
                    ))}
                  </div>
                </div>
              ))}
            </div>

            <div className="flex items-center justify-end gap-3 border-t border-stone-200 px-6 py-4">
              <button
                type="button"
                onClick={closeModal}
                className="rounded-xl border border-stone-200 px-4 py-2 text-sm text-stone-600 transition hover:bg-stone-50"
              >
                Fermer
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
