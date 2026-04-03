import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { FieldRenderer } from '../components/FieldRenderer';
import { getErrorMessage } from '../lib/api';
import {
  createNeedItemPayload,
  getNeedItemSchema,
  isNeedItemValid,
  parseDetailsJson,
  TEACHER_NEED_ITEM_OPTIONS,
  summarizeNeedItem,
  type NeedItemDraft,
  type NeedItemLookups,
  type TeacherNeedItemType,
} from '../lib/needItemSchemas';
import type { CourseResponse, TeachingNeedResponse, TeachingNeedStatus } from '../types/needs';
import type { SessionResponse } from '../types/sessions';
import type { OSResponse, LaboratoryLookupResponse, PhysicalServerResponse, SoftwareResponse } from '../types/admin';

const EMPTY_LOOKUPS: NeedItemLookups = {
  softwareNames: [],
  osOptions: [],
  laboratoryOptions: [],
  serverOptions: [],
};

const EDITABLE_ITEM_TYPES = new Set<string>([
  'saas', 'software', 'configuration', 'virtual_machine', 'physical_server', 'equipment_loan',
]);

export function CreateNeedPage() {
  const { sessionId, courseId, needId } = useParams();
  const sId = Number(sessionId);
  const cId = Number(courseId);
  const nId = Number(needId);
  const isEditMode = !!needId && Number.isFinite(nId);

  const navigate = useNavigate();
  const { apiFetch } = useAuth();

  const [session, setSession] = useState<SessionResponse | null>(null);
  const [course, setCourse] = useState<CourseResponse | null>(null);
  const [lookups, setLookups] = useState<NeedItemLookups>(EMPTY_LOOKUPS);
  const [existingStatus, setExistingStatus] = useState<TeachingNeedStatus | null>(null);
  const [originalItemIds, setOriginalItemIds] = useState<Set<number>>(new Set());

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [items, setItems] = useState<NeedItemDraft[]>([]);
  const [selectedType, setSelectedType] = useState<TeacherNeedItemType>('software');
  const [draftValues, setDraftValues] = useState<Record<string, string>>(() => {
    return getNeedItemSchema('software', EMPTY_LOOKUPS).defaultValues;
  });

  const currentSchema = useMemo(() => getNeedItemSchema(selectedType, lookups), [selectedType, lookups]);
  const canAddItem = useMemo(() => isNeedItemValid(currentSchema, draftValues), [currentSchema, draftValues]);

  // In edit mode, submit is only possible from Draft or Rejected statuses.
  const canSubmit = !isEditMode || existingStatus === 'Draft' || existingStatus === 'Rejected';

  const loadData = useCallback(async () => {
    if (!Number.isFinite(sId) || !Number.isFinite(cId)) {
      setError('Paramètres invalides.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const baseRequests = [
        apiFetch<SessionResponse>(`/sessions/${sId}`),
        apiFetch<CourseResponse>(`/courses/${cId}`),
        apiFetch<SoftwareResponse[]>('/softwares'),
        apiFetch<OSResponse[]>('/operatingsystems'),
        apiFetch<LaboratoryLookupResponse[]>('/laboratories'),
        apiFetch<PhysicalServerResponse[]>('/physicalservers'),
      ] as const;

      const [sessionData, courseData, softwaresData, osData, laboratoriesData, serversData] =
        await Promise.all(baseRequests);

      if (sessionData.status !== 'Open') {
        setError("Cette session n'accepte pas de besoins actuellement.");
        setLoading(false);
        return;
      }

      setSession(sessionData);
      setCourse(courseData);

      const resolvedLookups: NeedItemLookups = {
        softwareNames: softwaresData.map((s) => s.name),
        osOptions: osData.map((os) => ({ value: String(os.id), label: os.name })),
        laboratoryOptions: laboratoriesData.map((lab) => ({ value: String(lab.id), label: lab.name })),
        serverOptions: serversData.map((server) => ({ value: String(server.id), label: server.hostname })),
      };
      setLookups(resolvedLookups);

      if (isEditMode) {
        const needData = await apiFetch<TeachingNeedResponse>(`/sessions/${sId}/needs/${nId}`);
        setExistingStatus(needData.status);
        setOriginalItemIds(new Set(needData.items.map((i) => i.id)));

        const loadedItems: NeedItemDraft[] = needData.items
          .filter((item) => EDITABLE_ITEM_TYPES.has(item.itemType))
          .map((item) => ({
            id: `existing-${item.id}`,
            existingApiId: item.id,
            itemType: item.itemType as TeacherNeedItemType,
            values: parseDetailsJson(item.detailsJson),
          }));
        setItems(loadedItems);
      }
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les données.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sId, cId, nId, isEditMode]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  useEffect(() => {
    setDraftValues(getNeedItemSchema(selectedType, lookups).defaultValues);
  }, [selectedType, lookups]);

  function addItem() {
    if (!canAddItem) return;

    setItems((prev) => [
      ...prev,
      {
        id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
        itemType: selectedType,
        values: { ...draftValues },
      },
    ]);

    setDraftValues(getNeedItemSchema(selectedType, lookups).defaultValues);
  }

  function removeItem(itemId: string) {
    setItems((prev) => prev.filter((item) => item.id !== itemId));
  }

  async function persistNeed(mode: 'draft' | 'submit') {
    setSaving(true);
    setError('');
    setSuccess('');

    try {
      if (isEditMode) {
        const currentApiIds = new Set(
          items.filter((i) => i.existingApiId != null).map((i) => i.existingApiId!),
        );

        const toDelete = [...originalItemIds].filter((id) => !currentApiIds.has(id));
        const toAdd = items.filter((i) => i.existingApiId == null);

        await Promise.all(
          toDelete.map((id) =>
            apiFetch(`/sessions/${sId}/needs/${nId}/items/${id}`, { method: 'DELETE' }),
          ),
        );

        await Promise.all(
          toAdd.map((item) =>
            apiFetch(`/sessions/${sId}/needs/${nId}/items`, {
              method: 'POST',
              body: JSON.stringify(createNeedItemPayload(item.itemType, item.values)),
            }),
          ),
        );

        if (mode === 'submit') {
          if (existingStatus === 'Rejected') {
            await apiFetch(`/sessions/${sId}/needs/${nId}/revise`, { method: 'POST' });
          }
          await apiFetch(`/sessions/${sId}/needs/${nId}/submit`, { method: 'POST' });
        }

        setSuccess(mode === 'submit' ? 'Besoin re-soumis avec succès.' : 'Modifications sauvegardées.');
      } else {
        const need = await apiFetch<TeachingNeedResponse>(`/sessions/${sId}/needs`, {
          method: 'POST',
          body: JSON.stringify({ courseId: cId }),
        });

        await Promise.all(
          items.map((item) =>
            apiFetch(`/sessions/${sId}/needs/${need.id}/items`, {
              method: 'POST',
              body: JSON.stringify(createNeedItemPayload(item.itemType, item.values)),
            }),
          ),
        );

        if (mode === 'submit') {
          await apiFetch(`/sessions/${sId}/needs/${need.id}/submit`, { method: 'POST' });
        }

        setSuccess(mode === 'submit' ? 'Besoin soumis avec succès.' : 'Brouillon sauvegardé.');
      }

      setTimeout(() => {
        void navigate(isEditMode ? '/mes-demandes' : `/sessions/${sId}/courses/${cId}`);
      }, 1200);
    } catch (err) {
      setError(getErrorMessage(err, "Impossible d'enregistrer ce besoin."));
    } finally {
      setSaving(false);
    }
  }

  const backUrl = isEditMode ? '/mes-demandes' : `/sessions/${sId}/courses/${cId}`;
  const backLabel = isEditMode ? 'Retour aux demandes' : 'Retour aux ressources du cours';

  return (
    <div className="space-y-6">
      <Link
        to={backUrl}
        className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]"
      >
        &larr; {backLabel}
      </Link>

      {loading ? (
        <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>
      ) : error && !session ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : (
        <>
          <section className="surface-card p-6 sm:p-8">
            <p className="text-xs uppercase tracking-[0.3em] text-stone-500">
              {isEditMode ? 'Modifier le besoin' : 'Nouveau besoin'}
            </p>
            <h1 className="mt-2 text-2xl font-semibold text-stone-950">
              {course?.code}{course?.name ? ` — ${course.name}` : ''}
            </h1>
            <p className="mt-2 text-sm text-stone-600">Session: {session?.title}</p>
          </section>

          {error ? <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div> : null}
          {success ? <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</div> : null}

          <section className="surface-card p-6 sm:p-8">
            {/*
              noValidate disables browser-native HTML5 validation so that the
              required attributes on sub-form fields (FieldRenderer) do not
              block the final "Soumettre" button when those fields are empty
              but items have already been added to the list.
              JS-side gates (canAddItem / items.length > 0) handle validation.
            */}
            <form
              noValidate
              className="grid gap-4"
              onSubmit={(event: FormEvent<HTMLFormElement>) => {
                event.preventDefault();
                if (canSubmit && items.length > 0) {
                  void persistNeed('submit');
                }
              }}
            >
              <div className="rounded-2xl border border-stone-200 bg-stone-50/70 p-4">
                <p className="mb-3 text-sm font-semibold text-stone-800">Ajouter des besoins</p>

                <div className="grid gap-4">
                  <label className="block">
                    <span className="mb-1 block text-xs font-medium text-stone-600">Type de besoin</span>
                    <select
                      value={selectedType}
                      onChange={(event) => setSelectedType(event.target.value as TeacherNeedItemType)}
                      className="input-field max-w-xs"
                    >
                      {TEACHER_NEED_ITEM_OPTIONS.map((option) => (
                        <option key={option.value} value={option.value}>{option.label}</option>
                      ))}
                    </select>
                  </label>

                  <div className="grid gap-4 md:grid-cols-2">
                    {currentSchema.fields.map((field) => (
                      <label key={field.name} className="block md:col-span-1">
                        <span className="mb-1 block text-xs font-medium text-stone-600">
                          {field.label}
                          {field.required ? <span className="ml-0.5 text-rose-500">*</span> : null}
                        </span>
                        <FieldRenderer
                          field={field}
                          value={draftValues[field.name] ?? ''}
                          onChange={(name, value) => setDraftValues((prev) => ({ ...prev, [name]: value }))}
                        />
                      </label>
                    ))}
                  </div>

                  <div>
                    <button
                      type="button"
                      onClick={addItem}
                      disabled={!canAddItem}
                      className="rounded-2xl border border-stone-300 px-4 py-3 text-sm text-stone-700 transition hover:bg-stone-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      Ajouter
                    </button>
                  </div>
                </div>
              </div>

              {items.length > 0 ? (
                <div className="rounded-2xl border border-stone-200 bg-stone-50/70 p-4">
                  <p className="text-sm font-medium text-stone-800">Besoins ajoutés</p>
                  <ul className="mt-3 space-y-2">
                    {items.map((item) => {
                      const { label, summary } = summarizeNeedItem(
                        { id: item.existingApiId ?? 0, itemType: item.itemType, detailsJson: JSON.stringify(item.values) },
                        lookups,
                      );

                      return (
                        <li key={item.id} className="flex items-center justify-between gap-4 rounded-xl bg-white px-3 py-2 text-sm">
                          <span className="min-w-0">
                            <span className="mr-2 inline-flex rounded-md border border-stone-200 bg-stone-100 px-1.5 py-0.5 text-[10px] font-medium text-stone-600">
                              {label}
                            </span>
                            {summary}
                          </span>
                          <button
                            type="button"
                            onClick={() => removeItem(item.id)}
                            className="text-xs text-rose-600 hover:text-rose-700"
                          >
                            Retirer
                          </button>
                        </li>
                      );
                    })}
                  </ul>
                </div>
              ) : null}

              <div className="flex flex-wrap gap-3">
                <button
                  type="button"
                  disabled={saving}
                  onClick={() => void persistNeed('draft')}
                  className="rounded-2xl border border-stone-300 px-4 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 disabled:opacity-50"
                >
                  {saving ? 'Enregistrement...' : isEditMode ? 'Sauvegarder les modifications' : 'Sauvegarder brouillon'}
                </button>

                {canSubmit ? (
                  <button
                    type="submit"
                    disabled={saving || items.length === 0}
                    className="primary-button disabled:opacity-50"
                  >
                    {saving ? 'Soumission...' : 'Soumettre'}
                  </button>
                ) : null}
              </div>
            </form>
          </section>
        </>
      )}
    </div>
  );
}
