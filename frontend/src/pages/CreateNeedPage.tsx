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
import type { CourseResponse, NeedHistoryEntry, TeachingNeedResponse, TeachingNeedStatus } from '../types/needs';
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
  const [rejectionReason, setRejectionReason] = useState<string | null>(null);
  const [originalItemIds, setOriginalItemIds] = useState<Set<number>>(new Set());

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [history, setHistory] = useState<NeedHistoryEntry[]>([]);
  const [showHistory, setShowHistory] = useState(false);
  const [cloning, setCloning] = useState(false);

  const [items, setItems] = useState<NeedItemDraft[]>([]);
  const [selectedType, setSelectedType] = useState<TeacherNeedItemType>('software');
  const [draftValues, setDraftValues] = useState<Record<string, string>>(() => {
    return getNeedItemSchema('software', EMPTY_LOOKUPS).defaultValues;
  });
  const [editingItemId, setEditingItemId] = useState<string | null>(null);

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

      if (!isEditMode) {
        try {
          const historyData = await apiFetch<NeedHistoryEntry[]>(`/courses/${cId}/needs/history`);
          setHistory(historyData);
        } catch {
          // history is optional, ignore errors
        }
      }

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
        setRejectionReason(needData.rejectionReason ?? null);
        setOriginalItemIds(new Set(needData.items.map((i) => i.id)));

        const loadedItems: NeedItemDraft[] = needData.items
          .filter((item) => EDITABLE_ITEM_TYPES.has(item.itemType))
          .map((item) => {
            const parsedValues = parseDetailsJson(item.detailsJson);
            return {
              id: `existing-${item.id}`,
              existingApiId: item.id,
              // Snapshot kept to detect field-level modifications later.
              originalValues: parsedValues,
              itemType: item.itemType as TeacherNeedItemType,
              values: parsedValues,
            };
          });
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

  function addOrUpdateItem() {
    if (!canAddItem) return;

    if (editingItemId) {
      // Update existing item
      setItems((prev) =>
        prev.map((item) =>
          item.id === editingItemId
            ? { ...item, values: { ...draftValues } }
            : item
        )
      );
      setEditingItemId(null);
    } else {
      // Add new item
      setItems((prev) => [
        ...prev,
        {
          id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          itemType: selectedType,
          values: { ...draftValues },
        },
      ]);
    }

    setDraftValues(getNeedItemSchema(selectedType, lookups).defaultValues);
  }

  function startEditItem(itemId: string) {
    const item = items.find((i) => i.id === itemId);
    if (!item) return;

    setSelectedType(item.itemType);
    setDraftValues(item.values);
    setEditingItemId(itemId);
    // Scroll to the edit form
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function cancelEditItem() {
    setEditingItemId(null);
    setDraftValues(getNeedItemSchema(selectedType, lookups).defaultValues);
  }

  function removeItem(itemId: string) {
    setItems((prev) => prev.filter((item) => item.id !== itemId));
    if (editingItemId === itemId) {
      cancelEditItem();
    }
  }

  async function persistNeed(mode: 'draft' | 'submit') {
    setSaving(true);
    setError('');
    setSuccess('');

    try {
      if (isEditMode) {
        // Compare two value maps, ignoring empty-string fields.
        function hasValueChanged(orig: Record<string, string>, curr: Record<string, string>): boolean {
          const nonEmpty = (r: Record<string, string>) =>
            Object.fromEntries(Object.entries(r).filter(([, v]) => v !== ''));
          const a = nonEmpty(orig);
          const b = nonEmpty(curr);
          const keysA = Object.keys(a).sort();
          const keysB = Object.keys(b).sort();
          if (keysA.join(',') !== keysB.join(',')) return true;
          return keysA.some((k) => a[k] !== b[k]);
        }

        // Items whose detailsJson values changed — must be deleted and re-created.
        const modifiedApiIds = new Set<number>(
          items
            .filter(
              (i) =>
                i.existingApiId != null &&
                hasValueChanged(i.originalValues ?? {}, i.values),
            )
            .map((i) => i.existingApiId!),
        );

        const currentApiIds = new Set(
          items.filter((i) => i.existingApiId != null).map((i) => i.existingApiId!),
        );

        // Delete: items removed from the list + items whose values changed.
        const toDelete = [...originalItemIds].filter(
          (id) => !currentApiIds.has(id) || modifiedApiIds.has(id),
        );

        // Add: brand-new items + modified items (re-created with updated values).
        const toAdd = items.filter(
          (i) => i.existingApiId == null || modifiedApiIds.has(i.existingApiId),
        );

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

          {!isEditMode && history.length > 0 ? (
            <section className="surface-card p-4 sm:p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-semibold text-stone-800">Réutiliser une demande précédente</p>
                  <p className="text-xs text-stone-500 mt-0.5">{history.length} demande{history.length > 1 ? 's' : ''} approuvée{history.length > 1 ? 's' : ''} pour ce cours</p>
                </div>
                <button
                  type="button"
                  onClick={() => setShowHistory((v) => !v)}
                  className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs font-medium text-stone-600 hover:bg-stone-50 transition"
                >
                  {showHistory ? 'Masquer' : 'Voir l\'historique'}
                </button>
              </div>

              {showHistory ? (
                <ul className="mt-4 space-y-2">
                  {history.map((h) => (
                    <li key={h.id} className="flex items-center justify-between gap-4 rounded-xl border border-stone-200 bg-stone-50 px-4 py-3">
                      <div className="min-w-0">
                        <p className="text-sm font-medium text-stone-800">
                          {h.items.length} besoin{h.items.length > 1 ? 's' : ''} — approuvé le{' '}
                          {new Date(h.createdAt).toLocaleDateString('fr-CA')}
                        </p>
                        <p className="text-xs text-stone-500 mt-0.5">
                          {h.items.map((i) => i.softwareName ?? i.description ?? i.itemType).filter(Boolean).join(', ')}
                        </p>
                      </div>
                      <button
                        type="button"
                        disabled={cloning}
                        onClick={async () => {
                          setCloning(true);
                          setError('');
                          try {
                            const cloned = await apiFetch<TeachingNeedResponse>(
                              `/sessions/${sId}/needs/from-template/${h.id}`,
                              { method: 'POST' }
                            );
                            void navigate(`/sessions/${sId}/courses/${cId}/needs/${cloned.id}/edit`);
                          } catch (err) {
                            setError(getErrorMessage(err, 'Impossible de réutiliser cette demande.'));
                            setCloning(false);
                          }
                        }}
                        className="shrink-0 rounded-xl bg-[var(--ets-primary)] px-3 py-1.5 text-xs font-medium text-white hover:bg-[var(--ets-primary-hover)] disabled:opacity-50 transition"
                      >
                        {cloning ? 'Chargement…' : 'Réutiliser'}
                      </button>
                    </li>
                  ))}
                </ul>
              ) : null}
            </section>
          ) : null}

          {isEditMode && existingStatus === 'Rejected' && rejectionReason ? (
            <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-800">
              <p className="font-semibold">Demande rejetée</p>
              <p className="mt-1 text-rose-700">Motif indiqué par l&apos;équipe&nbsp;: {rejectionReason}</p>
              <p className="mt-2 text-xs text-rose-600">
                Vous pouvez modifier les éléments ci-dessous, puis utiliser «&nbsp;Soumettre&nbsp;» pour renvoyer la demande (brouillon puis soumission).
              </p>
            </div>
          ) : null}

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
                <p className="mb-3 text-sm font-semibold text-stone-800">
                  {editingItemId ? 'Modifier le besoin' : 'Ajouter des besoins'}
                </p>

                <div className="grid gap-4">
                  <label className="block">
                    <span className="mb-1 block text-xs font-medium text-stone-600">Type de besoin</span>
                    <select
                      value={selectedType}
                      onChange={(event) => setSelectedType(event.target.value as TeacherNeedItemType)}
                      disabled={editingItemId != null}
                      className="input-field max-w-xs disabled:cursor-not-allowed disabled:bg-stone-200"
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

                  <div className="flex gap-2">
                    <button
                      type="button"
                      onClick={addOrUpdateItem}
                      disabled={!canAddItem}
                      className="rounded-2xl border border-stone-300 px-4 py-3 text-sm text-stone-700 transition hover:bg-stone-100 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {editingItemId ? 'Mettre à jour' : 'Ajouter'}
                    </button>
                    {editingItemId ? (
                      <button
                        type="button"
                        onClick={cancelEditItem}
                        className="rounded-2xl border border-stone-300 px-4 py-3 text-sm text-stone-600 transition hover:bg-stone-100"
                      >
                        Annuler
                      </button>
                    ) : null}
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
                      const isBeingEdited = editingItemId === item.id;

                      return (
                        <li
                          key={item.id}
                          className={[
                            'flex items-center justify-between gap-4 rounded-xl px-3 py-2 text-sm transition',
                            isBeingEdited ? 'bg-blue-50 border border-blue-200' : 'bg-white'
                          ].join(' ')}
                        >
                          <span className="min-w-0">
                            <span className="mr-2 inline-flex rounded-md border border-stone-200 bg-stone-100 px-1.5 py-0.5 text-[10px] font-medium text-stone-600">
                              {label}
                            </span>
                            {summary}
                            {isBeingEdited && (
                              <span className="ml-2 inline-flex rounded-md border border-blue-200 bg-blue-50 px-1.5 py-0.5 text-[10px] font-medium text-blue-600">
                                En édition
                              </span>
                            )}
                          </span>
                          <div className="flex gap-2">
                            <button
                              type="button"
                              onClick={() => startEditItem(item.id)}
                              className="text-xs text-blue-600 hover:text-blue-700 font-medium"
                            >
                              Éditer
                            </button>
                            <button
                              type="button"
                              onClick={() => removeItem(item.id)}
                              className="text-xs text-rose-600 hover:text-rose-700"
                            >
                              Retirer
                            </button>
                          </div>
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
