import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { useAdminCrud } from '../../hooks/useAdminCrud';
import { getErrorMessage } from '../../lib/api';
import { ResourceTable, type Column } from '../ResourceTable';
import { ConfirmDeleteModal } from './ConfirmDeleteModal';
import { FormModal } from './FormModal';
import type {
  CourseResponse,
  CreateCourseRequest,
  UpdateCourseRequest,
  FieldDef,
} from '../../types/admin';

const COURSE_EDIT_FIELDS: FieldDef[] = [
  { name: 'code', label: 'Code', type: 'text', required: true, placeholder: 'Ex: INF1120' },
  { name: 'name', label: 'Nom', type: 'text', placeholder: 'Nom du cours (optionnel)' },
];

const initialForm = { code: '', name: '' };

function getCourseLabel(c: CourseResponse): string {
  return c.name ? `${c.code} ${c.name}` : c.code;
}

export function CoursesAdminSection() {
  const { items: courses, loading, error, saving, load, create, update, remove, setError } =
    useAdminCrud<CourseResponse>('/courses');

  const [form, setForm] = useState(initialForm);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState('');

  const [editingCourse, setEditingCourse] = useState<CourseResponse | null>(null);
  const [editValues, setEditValues] = useState<Record<string, string>>({});
  const [editError, setEditError] = useState('');

  const [deletingCourse, setDeletingCourse] = useState<CourseResponse | null>(null);

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    setCreateError('');
    setCreating(true);
    try {
      const body: CreateCourseRequest = {
        code: form.code.trim(),
        name: form.name.trim() || null,
      };
      await create(body);
      setForm(initialForm);
    } catch (err) {
      setCreateError(getErrorMessage(err, 'Impossible de créer le cours.'));
    } finally {
      setCreating(false);
    }
  }

  function startEdit(course: CourseResponse) {
    setEditingCourse(course);
    setEditValues({ code: course.code, name: course.name ?? '' });
    setEditError('');
  }

  async function handleUpdate() {
    if (!editingCourse) return;
    setEditError('');
    try {
      const body: UpdateCourseRequest = {
        code: editValues.code.trim(),
        name: editValues.name.trim() || null,
      };
      await update(editingCourse.id, body);
      setEditingCourse(null);
    } catch (err) {
      setEditError(getErrorMessage(err, 'Impossible de modifier le cours.'));
    }
  }

  async function handleDelete() {
    if (!deletingCourse) return;
    try {
      await remove(deletingCourse.id);
      setDeletingCourse(null);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de supprimer le cours.'));
      setDeletingCourse(null);
    }
  }

  const columns: Column<CourseResponse>[] = [
    {
      key: 'code',
      label: 'Code',
      render: (c) => (
        <Link
          to={`/admin/courses-resources/${c.id}`}
          className="font-medium text-[var(--ets-primary)] hover:underline"
        >
          {c.code}
        </Link>
      ),
    },
    {
      key: 'name',
      label: 'Nom',
      render: (c) => c.name ?? '—',
    },
    {
      key: 'actions',
      label: '',
      render: (course) => (
        <div className="flex justify-end gap-2">
          <Link
            to={`/admin/courses-resources/${course.id}`}
            className="rounded-xl border border-[var(--ets-primary)]/30 px-3 py-1 text-xs font-medium text-[var(--ets-primary)] transition hover:bg-[rgba(220,4,44,0.06)]"
          >
            Ressources
          </Link>
          <button
            type="button"
            onClick={() => startEdit(course)}
            className="rounded-xl border border-stone-200 px-3 py-1 text-xs text-stone-600 transition hover:bg-stone-50"
          >
            Éditer
          </button>
          <button
            type="button"
            onClick={() => setDeletingCourse(course)}
            className="rounded-xl border border-rose-200 px-3 py-1 text-xs text-rose-600 transition hover:bg-rose-50"
          >
            Supprimer
          </button>
        </div>
      ),
    },
  ];

  return (
    <>
      <section className="surface-card p-0">
        <div className="flex items-center justify-between border-b border-stone-200 px-6 py-4">
          <h2 className="text-base font-semibold text-stone-950">Cours ({courses.length})</h2>
          <button
            type="button"
            onClick={() => void load()}
            className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
          >
            Rafraîchir
          </button>
        </div>

        {error ? (
          <div className="mx-6 mt-4 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        ) : null}

        {loading ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
        ) : (
          <ResourceTable
            data={courses}
            columns={columns}
            emptyMessage="Aucun cours enregistré."
            keyExtractor={(c) => c.id}
          />
        )}

        {/* Create form */}
        <div className="border-t border-stone-200 px-6 py-5">
          <h3 className="text-sm font-semibold text-stone-950">Ajouter un cours</h3>

          {createError ? (
            <div className="mt-2 rounded-xl border border-rose-200 bg-rose-50 px-4 py-2 text-sm text-rose-700">
              {createError}
            </div>
          ) : null}

          <form className="mt-3 grid gap-3 sm:grid-cols-[1fr_1fr_auto]" onSubmit={handleCreate}>
            <input
              value={form.code}
              onChange={(e) => setForm((p) => ({ ...p, code: e.target.value }))}
              className="input-field"
              placeholder="Code (ex: INF1120)"
              required
            />
            <input
              value={form.name}
              onChange={(e) => setForm((p) => ({ ...p, name: e.target.value }))}
              className="input-field"
              placeholder="Nom (optionnel)"
            />
            <button type="submit" className="primary-button" disabled={creating || saving}>
              {creating ? 'Création...' : 'Créer'}
            </button>
          </form>
        </div>
      </section>

      {/* Edit modal */}
      <FormModal
        open={editingCourse !== null}
        title={`Modifier le cours ${editingCourse?.code ?? ''}`}
        fields={COURSE_EDIT_FIELDS}
        values={editValues}
        onChange={(name, value) => setEditValues((p) => ({ ...p, [name]: value }))}
        onSubmit={() => void handleUpdate()}
        onClose={() => setEditingCourse(null)}
        saving={saving}
        error={editError}
      />

      {/* Delete confirmation */}
      <ConfirmDeleteModal
        open={deletingCourse !== null}
        itemLabel={deletingCourse ? getCourseLabel(deletingCourse) : ''}
        expectedText={deletingCourse ? getCourseLabel(deletingCourse) : ''}
        saving={saving}
        onConfirm={() => void handleDelete()}
        onCancel={() => setDeletingCourse(null)}
      />
    </>
  );
}
