import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { SessionStatusBadge } from '../components/SessionStatusBadge';
import { getErrorMessage } from '../lib/api';
import type { CourseResponse, TeachingNeedResponse } from '../types/needs';
import type { SessionResponse } from '../types/sessions';

export function SessionCoursesPage() {
  const { sessionId } = useParams();
  const id = Number(sessionId);
  const { apiFetch, user } = useAuth();

  const [session, setSession] = useState<SessionResponse | null>(null);
  const [courses, setCourses] = useState<CourseResponse[]>([]);
  const [needs, setNeeds] = useState<TeachingNeedResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  const isTeacher = user?.role === 'professor' || user?.role === 'course_instructor';

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
        apiFetch<CourseResponse[]>('/courses'),
        apiFetch<TeachingNeedResponse[]>(`/sessions/${id}/needs`),
      ]);

      setSession(sessionData);
      setCourses(coursesData);
      setNeeds(needsData);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les données.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, id]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const courseNeedCounts = useMemo(() => {
    const map = new Map<number, number>();
    for (const need of needs) {
      map.set(need.courseId, (map.get(need.courseId) ?? 0) + 1);
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
              {session ? <SessionStatusBadge status={session.status} /> : null}
            </div>

            {isTeacher && session?.status !== 'Open' ? (
              <div className="mt-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
                Cette session n&apos;accepte pas de besoins actuellement. Vous pouvez consulter les ressources des cours en lecture seule.
              </div>
            ) : null}
          </section>

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
                {search ? 'Aucun cours ne correspond à votre recherche.' : 'Aucun cours disponible.'}
              </div>
            ) : (
              <div className="grid gap-3 p-6 lg:grid-cols-2 xl:grid-cols-3">
                {filteredCourses.map((course) => {
                  const needCount = courseNeedCounts.get(course.id) ?? 0;

                  return (
                    <Link
                      key={course.id}
                      to={`/sessions/${id}/courses/${course.id}`}
                      className="group rounded-2xl border border-stone-200 bg-white/80 p-4 transition hover:border-[var(--ets-primary)]/30 hover:bg-[rgba(220,4,44,0.04)] hover:shadow-sm"
                    >
                      <div className="flex items-start justify-between gap-2">
                        <div className="min-w-0">
                          <p className="text-lg font-semibold text-stone-950 group-hover:text-[var(--ets-primary)]">{course.code}</p>
                          {course.name ? <p className="mt-1 text-sm text-stone-600">{course.name}</p> : null}
                        </div>
                        {needCount > 0 ? (
                          <span className="shrink-0 rounded-xl border border-blue-200 bg-blue-50 px-2 py-0.5 text-xs font-medium text-blue-700">
                            {needCount} besoin{needCount > 1 ? 's' : ''}
                          </span>
                        ) : null}
                      </div>
                      <p className="mt-3 text-xs text-stone-400 group-hover:text-[var(--ets-primary)]/60">
                        Voir les ressources &rarr;
                      </p>
                    </Link>
                  );
                })}
              </div>
            )}
          </section>
        </>
      )}
    </div>
  );
}
