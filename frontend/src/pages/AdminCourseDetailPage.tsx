import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import { ResourcesAdminSection } from '../components/admin/ResourcesAdminSection';
import type { CourseResponse } from '../types/admin';

export function AdminCourseDetailPage() {
  const { courseId } = useParams();
  const cId = Number(courseId);
  const { apiFetch } = useAuth();

  const [course, setCourse] = useState<CourseResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadCourse = useCallback(async () => {
    if (!Number.isFinite(cId)) {
      setError('Identifiant de cours invalide.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const data = await apiFetch<CourseResponse>(`/courses/${cId}`);
      setCourse(data);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger le cours.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, cId]);

  useEffect(() => {
    void loadCourse();
  }, [loadCourse]);

  if (loading) {
    return (
      <div className="space-y-6">
        <Link
          to="/admin/courses-resources"
          className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]"
        >
          &larr; Retour aux cours
        </Link>
        <div className="surface-card px-6 py-10 text-center text-sm text-stone-500">
          Chargement...
        </div>
      </div>
    );
  }

  if (error || !course) {
    return (
      <div className="space-y-6">
        <Link
          to="/admin/courses-resources"
          className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]"
        >
          &larr; Retour aux cours
        </Link>
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {error || 'Cours introuvable.'}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <Link
        to="/admin/courses-resources"
        className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]"
      >
        &larr; Retour aux cours
      </Link>

      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.3em] text-stone-500">Cours</p>
        <h1 className="mt-2 text-2xl font-semibold text-stone-950 sm:text-3xl">
          {course.code}
          {course.name ? ` — ${course.name}` : ''}
        </h1>
        <p className="mt-2 text-sm text-stone-600">
          Gérez les ressources associées à ce cours. Utilisez les boutons{' '}
          <span className="font-semibold text-emerald-600">Associer</span> /{' '}
          <span className="font-semibold text-emerald-600">Dissocier</span> pour lier ou retirer
          des ressources du catalogue.
        </p>
      </section>

      <ResourcesAdminSection courseId={cId} />
    </div>
  );
}
