import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import { NEED_ITEM_LABELS, summarizeNeedItem, type NeedItemLookups } from '../lib/needItemSchemas';
import { teachingNeedStatusBadgeClass, teachingNeedStatusLabelFr } from '../lib/needStatusMapping';
import type {
  AddNeedItemRequest,
  CourseResponse,
  NeedItemType,
  SoftwareResponse,
  TeachingNeedResponse,
  TeachingNeedStatus,
} from '../types/needs';
import type { SessionResponse } from '../types/sessions';
import type { LaboratoryLookupResponse, OSResponse, PhysicalServerResponse } from '../types/admin';
import { AiReviewPanel } from '../components/AiReviewPanel';

interface LocalNeedItem {
  id: string;
  itemType: NeedItemType;
  softwareId?: number;
  softwareName?: string;
  softwareVersion?: string;
  description?: string;
}

function formatDateTime(iso: string | undefined | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('fr-CA', { dateStyle: 'medium', timeStyle: 'short' });
}

function NeedTimestamps({ need }: { need: TeachingNeedResponse }) {
  return (
    <div className="mt-2 flex flex-wrap gap-x-4 gap-y-0.5 text-[11px] text-stone-400">
      <span>Créé: {formatDateTime(need.createdAt)}</span>
      {need.submittedAt ? <span>Soumis: {formatDateTime(need.submittedAt)}</span> : null}
      {need.reviewedAt ? <span>Révisé: {formatDateTime(need.reviewedAt)}</span> : null}
    </div>
  );
}

function ChevronIcon({ expanded }: { expanded: boolean }) {
  return (
    <svg
      className={`h-4 w-4 text-stone-400 transition-transform ${expanded ? 'rotate-180' : ''}`}
      fill="none"
      viewBox="0 0 24 24"
      strokeWidth={2}
      stroke="currentColor"
    >
      <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5" />
    </svg>
  );
}


interface NeedDetailPanelProps {
  need: TeachingNeedResponse;
  lookups: NeedItemLookups;
}

function NeedDetailPanel({ need, lookups }: NeedDetailPanelProps) {
  return (
    <div className="mt-3">
      <p className="text-sm font-semibold text-stone-800 mb-2">
        Besoins ({need.items.length})
      </p>
      {need.items.length > 0 ? (
        <ul className="space-y-1.5">
          {need.items.map((item) => {
            const { label, summary } = summarizeNeedItem(item, lookups);
            return (
              <li key={item.id} className="rounded-xl bg-stone-50 px-3 py-2 text-sm text-stone-700 flex items-center gap-2 flex-wrap">
                <span className="inline-flex rounded-md border border-stone-200 bg-stone-100 px-1.5 py-0.5 text-[10px] font-medium text-stone-500">
                  {label}
                </span>
                {summary}
                {item.alreadyInstalledInLabs === true ? (
                  <span className="inline-flex rounded-md border border-emerald-200 bg-emerald-50 px-1.5 py-0.5 text-[10px] font-medium text-emerald-700">
                    Déjà installé
                  </span>
                ) : null}
              </li>
            );
          })}
        </ul>
      ) : (
        <p className="text-sm text-stone-500">Aucun besoin spécifique.</p>
      )}

      {need.rejectionReason ? (
        <p className="mt-3 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
          Motif de rejet&nbsp;: {need.rejectionReason}
        </p>
      ) : null}
    </div>
  );
}

function NeedStatusBadge({ status, isFastTrack }: { status: TeachingNeedStatus; isFastTrack?: boolean }) {
  return (
    <div className="flex items-center gap-1.5">
      {isFastTrack ? (
        <span className="inline-flex rounded-xl border border-violet-200 bg-violet-50 px-2.5 py-0.5 text-xs font-medium text-violet-700" title="Tous les items correspondent à une demande précédemment approuvée">
          ⚡ FastTrack
        </span>
      ) : null}
      <span className={`inline-flex rounded-xl border px-2.5 py-0.5 text-xs font-medium ${teachingNeedStatusBadgeClass(status)}`}>
        {teachingNeedStatusLabelFr(status)}
      </span>
    </div>
  );
}

function TeacherNeedsView({ sessionId, startInCreateMode = false }: { sessionId: number; startInCreateMode?: boolean }) {
  const { apiFetch } = useAuth();
  const [courses, setCourses] = useState<CourseResponse[]>([]);
  const [softwares, setSoftwares] = useState<SoftwareResponse[]>([]);
  const [osOptions, setOsOptions] = useState<{ value: string; label: string }[]>([]);
  const [laboratoryOptions, setLaboratoryOptions] = useState<{ value: string; label: string }[]>([]);
  const [serverOptions, setServerOptions] = useState<{ value: string; label: string }[]>([]);
  const [needs, setNeeds] = useState<TeachingNeedResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [submittingNeedId, setSubmittingNeedId] = useState<number | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(startInCreateMode);

  const [courseCode, setCourseCode] = useState('');
  const [courseName, setCourseName] = useState('');
  const [expectedStudents, setExpectedStudents] = useState<string>('');
  const [hasTechNeeds, setHasTechNeeds] = useState<boolean | null>(null);
  const [foundAllCourses, setFoundAllCourses] = useState<boolean | null>(null);
  const [desiredModifications, setDesiredModifications] = useState('');
  const [allowsUpdates, setAllowsUpdates] = useState<boolean | null>(null);
  const [additionalComments, setAdditionalComments] = useState('');

  const [softwareNameInput, setSoftwareNameInput] = useState('');
  const [softwareVersionInput, setSoftwareVersionInput] = useState('');
  const [newItemType, setNewItemType] = useState<NeedItemType>('software');
  const [newItemDescription, setNewItemDescription] = useState('');
  const [items, setItems] = useState<LocalNeedItem[]>([]);

  const courseCodeSuggestions = useMemo(() => courses.map((course) => course.code), [courses]);
  const softwareSuggestions = useMemo(() => softwares.map((software) => software.name), [softwares]);
  const needLookups = useMemo<NeedItemLookups>(
    () => ({
      softwareNames: softwareSuggestions,
      osOptions,
      laboratoryOptions,
      serverOptions,
    }),
    [softwareSuggestions, osOptions, laboratoryOptions, serverOptions],
  );

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const [coursesData, softwaresData, osData, laboratoriesData, serversData, needsData] = await Promise.all([
        apiFetch<CourseResponse[]>('/courses'),
        apiFetch<SoftwareResponse[]>('/softwares'),
        apiFetch<OSResponse[]>('/operatingsystems'),
        apiFetch<LaboratoryLookupResponse[]>('/laboratories'),
        apiFetch<PhysicalServerResponse[]>('/physicalservers'),
        apiFetch<TeachingNeedResponse[]>(`/sessions/${sessionId}/needs`),
      ]);

      setCourses(coursesData);
      setSoftwares(softwaresData);
      setOsOptions(osData.map((os) => ({ value: String(os.id), label: os.name })));
      setLaboratoryOptions(laboratoriesData.map((lab) => ({ value: String(lab.id), label: lab.name })));
      setServerOptions(serversData.map((server) => ({ value: String(server.id), label: server.hostname })));
      setNeeds(needsData);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger la saisie des besoins.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sessionId]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  useEffect(() => {
    if (startInCreateMode) {
      setShowCreateForm(true);
    }
  }, [startInCreateMode]);

  function addItemToDraft() {
    if (newItemType === 'software') {
      const softwareName = softwareNameInput.trim();
      if (!softwareName) return;

      const version = softwareVersionInput.trim();
      const dedupeKey = `${softwareName.toLowerCase()}::${version.toLowerCase()}`;

      setItems((prev) => {
        if (prev.some((entry) => entry.itemType === 'software' && `${(entry.softwareName ?? '').toLowerCase()}::${(entry.softwareVersion ?? '').toLowerCase()}` === dedupeKey)) {
          return prev;
        }
        return [
          ...prev,
          {
            id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
            itemType: 'software',
            softwareName,
            softwareVersion: version || undefined,
          },
        ];
      });

      setSoftwareNameInput('');
      setSoftwareVersionInput('');
    } else {
      const desc = newItemDescription.trim();
      if (!desc) return;

      setItems((prev) => [
        ...prev,
        {
          id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          itemType: newItemType,
          description: desc,
        },
      ]);

      setNewItemDescription('');
    }
  }

  function removeItem(itemId: string) {
    setItems((prev) => prev.filter((entry) => entry.id !== itemId));
  }

  async function resolveCourseId(): Promise<number> {
    const normalizedCode = courseCode.trim();
    if (!normalizedCode) {
      throw new Error('Veuillez renseigner un code de cours.');
    }

    const newName = courseName.trim() || null;
    const existing = courses.find((course) => course.code.toLowerCase() === normalizedCode.toLowerCase());

    if (existing) {
      if (newName && newName !== (existing.name ?? '')) {
        await apiFetch(`/courses/${existing.id}`, {
          method: 'PUT',
          body: JSON.stringify({ code: existing.code, name: newName }),
        });
        setCourses((prev) => prev.map((c) => (c.id === existing.id ? { ...c, name: newName } : c)));
      }
      return existing.id;
    }

    const created = await apiFetch<CourseResponse>('/courses', {
      method: 'POST',
      body: JSON.stringify({ code: normalizedCode, name: newName }),
    });

    setCourses((prev) => [created, ...prev]);
    return created.id;
  }

  async function resolveSoftwareId(name: string): Promise<number> {
    const existing = softwares.find((software) => software.name.toLowerCase() === name.toLowerCase());
    if (existing) {
      return existing.id;
    }

    const created = await apiFetch<SoftwareResponse>('/softwares', {
      method: 'POST',
      body: JSON.stringify({ name }),
    });

    setSoftwares((prev) => [created, ...prev]);
    return created.id;
  }

  async function persistNeed(mode: 'draft' | 'submit') {
    setSaving(true);
    setError('');
    setSuccess('');

    try {
      const resolvedCourseId = await resolveCourseId();

      const need = await apiFetch<TeachingNeedResponse>(`/sessions/${sessionId}/needs`, {
        method: 'POST',
        body: JSON.stringify({
          courseId: resolvedCourseId,
          expectedStudents: expectedStudents ? Number(expectedStudents) : undefined,
          hasTechNeeds: hasTechNeeds ?? undefined,
          foundAllCourses: foundAllCourses ?? undefined,
          desiredModifications: desiredModifications.trim() || undefined,
          allowsUpdates: allowsUpdates ?? undefined,
          additionalComments: additionalComments.trim() || undefined,
        }),
      });

      for (const item of items) {
        let payload: AddNeedItemRequest;

        if (item.itemType === 'software' && item.softwareName) {
          const softwareId = await resolveSoftwareId(item.softwareName);
          const itemNotes = item.softwareVersion ? `Version demandee: ${item.softwareVersion}` : undefined;
          payload = { itemType: 'software', softwareId, quantity: 1, notes: itemNotes };
        } else {
          payload = { itemType: item.itemType, description: item.description };
        }

        await apiFetch(`/sessions/${sessionId}/needs/${need.id}/items`, {
          method: 'POST',
          body: JSON.stringify(payload),
        });
      }

      if (mode === 'submit') {
        await apiFetch(`/sessions/${sessionId}/needs/${need.id}/submit`, {
          method: 'POST',
        });
      }

      setCourseCode('');
      setCourseName('');
      setExpectedStudents('');
      setHasTechNeeds(null);
      setFoundAllCourses(null);
      setDesiredModifications('');
      setAllowsUpdates(null);
      setAdditionalComments('');
      setSoftwareNameInput('');
      setSoftwareVersionInput('');
      setNewItemType('software');
      setNewItemDescription('');
      setItems([]);
      setSuccess(mode === 'submit' ? 'Besoin soumis.' : 'Brouillon sauvegardé.');
      if (mode === 'submit') {
        setShowCreateForm(false);
      }

      await loadData();
    } catch (err) {
      setError(getErrorMessage(err, "Impossible d'enregistrer ce besoin."));
    } finally {
      setSaving(false);
    }
  }

  async function submitExistingNeed(need: TeachingNeedResponse) {
    setSubmittingNeedId(need.id);
    setError('');
    setSuccess('');

    try {
      if (need.status === 'Rejected') {
        await apiFetch(`/sessions/${sessionId}/needs/${need.id}/revise`, { method: 'POST' });
      }
      await apiFetch(`/sessions/${sessionId}/needs/${need.id}/submit`, { method: 'POST' });
      setSuccess('Besoin soumis.');
      await loadData();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de soumettre ce besoin.'));
    } finally {
      setSubmittingNeedId(null);
    }
  }

  const [addItemNeedId, setAddItemNeedId] = useState<number | null>(null);
  const [inlineItemType, setInlineItemType] = useState<NeedItemType>('software');
  const [inlineSoftwareName, setInlineSoftwareName] = useState('');
  const [inlineVersionInput, setInlineVersionInput] = useState('');
  const [inlineDescription, setInlineDescription] = useState('');
  const [itemBusy, setItemBusy] = useState(false);

  async function addItemToExistingNeed(needId: number) {
    setItemBusy(true);
    setError('');

    try {
      let payload: AddNeedItemRequest;

      if (inlineItemType === 'software') {
        const name = inlineSoftwareName.trim();
        if (!name) return;
        const softwareId = await resolveSoftwareId(name);
        const itemNotes = inlineVersionInput.trim() ? `Version demandee: ${inlineVersionInput.trim()}` : undefined;
        payload = { itemType: 'software', softwareId, quantity: 1, notes: itemNotes };
      } else {
        const desc = inlineDescription.trim();
        if (!desc) return;
        payload = { itemType: inlineItemType, description: desc };
      }

      await apiFetch(`/sessions/${sessionId}/needs/${needId}/items`, {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setInlineSoftwareName('');
      setInlineVersionInput('');
      setInlineDescription('');
      setInlineItemType('software');
      setAddItemNeedId(null);
      await loadData();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible d\'ajouter cet élément.'));
    } finally {
      setItemBusy(false);
    }
  }

  async function removeItemFromNeed(needId: number, itemId: number) {
    setItemBusy(true);
    setError('');

    try {
      await apiFetch(`/sessions/${sessionId}/needs/${needId}/items/${itemId}`, { method: 'DELETE' });
      await loadData();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de retirer ce logiciel.'));
    } finally {
      setItemBusy(false);
    }
  }

  const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());
  function toggleExpand(id: number) {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  if (loading) {
    return <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>;
  }

  const editableNeeds = needs.filter(
    (need) => need.status === 'Draft' || need.status === 'Submitted' || need.status === 'Rejected',
  );
  const lockedNeeds = needs.filter(
    (need) => need.status !== 'Draft' && need.status !== 'Submitted' && need.status !== 'Rejected',
  );

  return (
    <div className="space-y-6">
      {error ? <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div> : null}
      {success ? <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</div> : null}

      <section className="surface-card p-6 sm:p-8">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-stone-950">Soumission besoins</h2>
            <p className="mt-1 text-sm text-stone-600">Un enseignant peut créer un besoin, le sauvegarder en brouillon, puis le soumettre.</p>
          </div>
          <button
            type="button"
            onClick={() => setShowCreateForm((prev) => !prev)}
            className="rounded-2xl border-2 border-[var(--ets-primary)] bg-[rgba(220,4,44,0.08)] px-4 py-2 text-sm font-semibold text-[var(--ets-primary)] hover:bg-[rgba(220,4,44,0.14)]"
          >
            {showCreateForm ? 'Fermer le formulaire' : 'Créer un besoin'}
          </button>
        </div>

        {showCreateForm ? (
          <form
            className="mt-5 grid gap-4"
            onSubmit={(event: FormEvent<HTMLFormElement>) => {
              event.preventDefault();
              void persistNeed('submit');
            }}
          >
            <div className="grid gap-4 md:grid-cols-2">
              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Quel(s) cours donnez-vous ?</span>
                <input
                  value={courseCode}
                  onChange={(event) => setCourseCode(event.target.value)}
                  className="input-field"
                  list="course-code-suggestions"
                  placeholder="Ex: INF101"
                  required
                />
                <datalist id="course-code-suggestions">
                  {courseCodeSuggestions.map((code) => (
                    <option key={code} value={code} />
                  ))}
                </datalist>
              </label>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Nom du cours (optionnel)</span>
                <input
                  value={courseName}
                  onChange={(event) => setCourseName(event.target.value)}
                  className="input-field"
                  placeholder="Ex: Introduction à la programmation"
                />
              </label>
            </div>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Combien d&apos;étudiant·e·s attendez-vous pour ce cours ?</span>
              <input
                type="number"
                min={0}
                value={expectedStudents}
                onChange={(event) => setExpectedStudents(event.target.value)}
                className="input-field max-w-xs"
                placeholder="Ex: 35"
              />
            </label>

            <fieldset>
              <legend className="mb-2 text-sm font-medium text-stone-700">Avez-vous des besoins technologiques pour cette session ?</legend>
              <div className="flex gap-4">
                <label className="flex items-center gap-2 text-sm">
                  <input type="radio" name="hasTechNeeds" checked={hasTechNeeds === true} onChange={() => setHasTechNeeds(true)} /> Oui
                </label>
                <label className="flex items-center gap-2 text-sm">
                  <input type="radio" name="hasTechNeeds" checked={hasTechNeeds === false} onChange={() => setHasTechNeeds(false)} /> Non
                </label>
              </div>
            </fieldset>

            <fieldset>
              <legend className="mb-2 text-sm font-medium text-stone-700">Est-ce que vous avez trouvé l&apos;ensemble de vos cours dans la liste ?</legend>
              <div className="flex gap-4">
                <label className="flex items-center gap-2 text-sm">
                  <input type="radio" name="foundAllCourses" checked={foundAllCourses === true} onChange={() => setFoundAllCourses(true)} /> Oui
                </label>
                <label className="flex items-center gap-2 text-sm">
                  <input type="radio" name="foundAllCourses" checked={foundAllCourses === false} onChange={() => setFoundAllCourses(false)} /> Non
                </label>
              </div>
            </fieldset>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Souhaitez-vous apporter des modifications ?</span>
              <textarea
                value={desiredModifications}
                onChange={(event) => setDesiredModifications(event.target.value)}
                rows={2}
                className="input-field"
                placeholder="Décrivez les modifications souhaitées"
              />
            </label>

            <fieldset>
              <legend className="mb-2 text-sm font-medium text-stone-700">Autorisez-vous l&apos;équipe technique à faire la mise à jour des logiciels et des systèmes d&apos;exploitation vers des versions subséquentes le cas échéant ?</legend>
              <div className="flex gap-4">
                <label className="flex items-center gap-2 text-sm">
                  <input type="radio" name="allowsUpdates" checked={allowsUpdates === true} onChange={() => setAllowsUpdates(true)} /> Oui
                </label>
                <label className="flex items-center gap-2 text-sm">
                  <input type="radio" name="allowsUpdates" checked={allowsUpdates === false} onChange={() => setAllowsUpdates(false)} /> Non
                </label>
              </div>
            </fieldset>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Commentaires supplémentaires</span>
              <textarea
                value={additionalComments}
                onChange={(event) => setAdditionalComments(event.target.value)}
                rows={2}
                className="input-field"
                placeholder="Précisions ou commentaires"
              />
            </label>

            <div className="rounded-2xl border border-stone-200 bg-stone-50/70 p-4">
              <p className="text-sm font-semibold text-stone-800 mb-3">Ajouter des besoins</p>

              <div className="grid gap-3">
                <label className="block">
                  <span className="mb-1 block text-xs font-medium text-stone-600">Type de besoin</span>
                  <select value={newItemType} onChange={(e) => setNewItemType(e.target.value as NeedItemType)} className="input-field max-w-xs">
                    <option value="software">Logiciel</option>
                    <option value="virtual_machine">Machine virtuelle</option>
                    <option value="physical_server">Serveur physique</option>
                    <option value="equipment_loan">Prêt d&apos;équipement</option>
                    <option value="other">Autre besoin technologique</option>
                  </select>
                </label>

                {newItemType === 'software' ? (
                  <div className="grid gap-3 md:grid-cols-[1fr_1fr_auto]">
                    <input
                      value={softwareNameInput}
                      onChange={(event) => setSoftwareNameInput(event.target.value)}
                      className="input-field"
                      list="software-name-suggestions"
                      placeholder="Nom logiciel"
                    />
                    <datalist id="software-name-suggestions">
                      {softwareSuggestions.map((name) => (
                        <option key={name} value={name} />
                      ))}
                    </datalist>

                    <input
                      value={softwareVersionInput}
                      onChange={(event) => setSoftwareVersionInput(event.target.value)}
                      className="input-field"
                      placeholder="Version (ex: 2022.3)"
                    />

                    <button
                      type="button"
                      onClick={addItemToDraft}
                      className="rounded-2xl border border-stone-300 px-4 py-3 text-sm text-stone-700 transition hover:bg-stone-100"
                    >
                      Ajouter
                    </button>
                  </div>
                ) : (
                  <div className="grid gap-3 md:grid-cols-[1fr_auto]">
                    <textarea
                      value={newItemDescription}
                      onChange={(event) => setNewItemDescription(event.target.value)}
                      rows={2}
                      className="input-field"
                      placeholder={
                        newItemType === 'virtual_machine' ? 'Décrivez vos besoins en machines virtuelles' :
                        newItemType === 'physical_server' ? 'Décrivez vos besoins en serveurs physiques' :
                        newItemType === 'equipment_loan' ? 'Quel équipement faudrait-il prêter aux étudiant·e·s ?' :
                        'Décrivez vos autres besoins technologiques'
                      }
                    />
                    <button
                      type="button"
                      onClick={addItemToDraft}
                      className="rounded-2xl border border-stone-300 px-4 py-3 text-sm text-stone-700 transition hover:bg-stone-100 self-end"
                    >
                      Ajouter
                    </button>
                  </div>
                )}
              </div>
            </div>

            {items.length > 0 ? (
              <div className="rounded-2xl border border-stone-200 bg-stone-50/70 p-4">
                <p className="text-sm font-medium text-stone-800">Besoins ajoutés</p>
                <ul className="mt-3 space-y-2">
                  {items.map((item) => (
                    <li key={item.id} className="flex items-center justify-between rounded-xl bg-white px-3 py-2 text-sm">
                      <span>
                        <span className="inline-flex rounded-md border border-stone-200 bg-stone-100 px-1.5 py-0.5 text-[10px] font-medium text-stone-600 mr-2">{NEED_ITEM_LABELS[item.itemType] ?? 'Logiciel'}</span>
                        {item.itemType === 'software'
                          ? <>{item.softwareName}{item.softwareVersion ? ` - ${item.softwareVersion}` : ''}</>
                          : item.description}
                      </span>
                      <button
                        type="button"
                        onClick={() => removeItem(item.id)}
                        className="text-xs text-rose-600 hover:text-rose-700"
                      >
                        Retirer
                      </button>
                    </li>
                  ))}
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
                {saving ? 'Enregistrement...' : 'Sauvegarder brouillon'}
              </button>
              <button type="submit" disabled={saving} className="primary-button disabled:opacity-50">
                {saving ? 'Soumission...' : 'Soumettre'}
              </button>
            </div>
          </form>
        ) : null}
      </section>

      <section className="surface-card p-6 sm:p-8">
        <h2 className="text-lg font-semibold text-stone-950">Mes demandes</h2>
        <p className="mt-1 text-sm text-stone-600">Vous voyez uniquement vos propres demandes et leur statut.</p>
        <div className="mt-4 grid gap-3">
          {[...editableNeeds, ...lockedNeeds].length === 0 ? (
            <p className="text-sm text-stone-500">Aucun besoin pour cette session.</p>
          ) : (
            [...editableNeeds, ...lockedNeeds].map((need) => {
              const isEditable = need.status === 'Draft' || need.status === 'Submitted' || need.status === 'Rejected';
              const isExpanded = expandedIds.has(need.id);

              return (
                <article key={need.id} className="rounded-2xl border border-stone-200 bg-white/80 overflow-hidden">
                  <button
                    type="button"
                    onClick={() => toggleExpand(need.id)}
                    className="flex w-full items-center justify-between gap-3 p-4 text-left hover:bg-stone-50/50 transition"
                  >
                    <div className="min-w-0">
                      <p className="font-medium text-stone-900">{need.courseCode}{need.courseName ? ` - ${need.courseName}` : ''}</p>
                      <p className="text-sm text-stone-600">{need.items.length} besoin(s)</p>
                      <NeedTimestamps need={need} />
                    </div>
                    <div className="flex shrink-0 items-center gap-2">
                      <NeedStatusBadge status={need.status} isFastTrack={need.isFastTrack} />
                      <ChevronIcon expanded={isExpanded} />
                    </div>
                  </button>

                  {isExpanded ? (
                    <div className="border-t border-stone-100 px-4 pb-4">
                      {need.rejectionReason ? (
                        <p className="mt-3 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">Motif: {need.rejectionReason}</p>
                      ) : null}

                      <NeedDetailPanel
                        need={need}
                        lookups={needLookups}
                      />

                      {isEditable ? (
                        <>
                          {need.items.length > 0 ? (
                            <ul className="mt-3 space-y-1.5">
                              {need.items.map((item) => (
                                <li key={item.id} className="flex items-center justify-between rounded-xl bg-stone-50 px-3 py-2 text-sm">
                                  <span className="text-stone-700">
                                    {(() => {
                                      const { label, summary } = summarizeNeedItem(item, needLookups);
                                      return (
                                        <>
                                          <span className="inline-flex rounded-md border border-stone-200 bg-stone-100 px-1.5 py-0.5 text-[10px] font-medium text-stone-500 mr-2">{label}</span>
                                          {summary}
                                        </>
                                      );
                                    })()}
                                  </span>
                                  <button type="button" onClick={() => void removeItemFromNeed(need.id, item.id)} disabled={itemBusy} className="text-xs text-rose-600 hover:text-rose-700 disabled:opacity-50">Retirer</button>
                                </li>
                              ))}
                            </ul>
                          ) : null}
                        </>
                      ) : null}

                  {isEditable ? (
                    <div className="mt-3">
                      {addItemNeedId === need.id ? (
                        <div className="grid gap-2">
                          <select value={inlineItemType} onChange={(e) => setInlineItemType(e.target.value as NeedItemType)} className="input-field max-w-xs">
                            <option value="software">Logiciel</option>
                            <option value="virtual_machine">Machine virtuelle</option>
                            <option value="physical_server">Serveur physique</option>
                            <option value="equipment_loan">Prêt d&apos;équipement</option>
                            <option value="other">Autre</option>
                          </select>
                          {inlineItemType === 'software' ? (
                            <div className="grid gap-2 sm:grid-cols-[1fr_1fr_auto_auto]">
                              <input
                                value={inlineSoftwareName}
                                onChange={(e) => setInlineSoftwareName(e.target.value)}
                                className="input-field"
                                list="software-name-suggestions"
                                placeholder="Nom logiciel"
                              />
                              <input
                                value={inlineVersionInput}
                                onChange={(e) => setInlineVersionInput(e.target.value)}
                                className="input-field"
                                placeholder="Version (optionnel)"
                              />
                              <button
                                type="button"
                                onClick={() => void addItemToExistingNeed(need.id)}
                                disabled={itemBusy || !inlineSoftwareName.trim()}
                                className="rounded-xl bg-stone-950 px-3 py-1.5 text-xs font-medium text-white hover:bg-stone-800 disabled:opacity-50"
                              >
                                {itemBusy ? '...' : 'Ajouter'}
                              </button>
                              <button
                                type="button"
                                onClick={() => { setAddItemNeedId(null); setInlineSoftwareName(''); setInlineVersionInput(''); setInlineDescription(''); setInlineItemType('software'); }}
                                className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-600 hover:bg-stone-100"
                              >
                                Annuler
                              </button>
                            </div>
                          ) : (
                            <div className="grid gap-2 sm:grid-cols-[1fr_auto_auto]">
                              <input
                                value={inlineDescription}
                                onChange={(e) => setInlineDescription(e.target.value)}
                                className="input-field"
                                placeholder="Description"
                              />
                              <button
                                type="button"
                                onClick={() => void addItemToExistingNeed(need.id)}
                                disabled={itemBusy || !inlineDescription.trim()}
                                className="rounded-xl bg-stone-950 px-3 py-1.5 text-xs font-medium text-white hover:bg-stone-800 disabled:opacity-50"
                              >
                                {itemBusy ? '...' : 'Ajouter'}
                              </button>
                              <button
                                type="button"
                                onClick={() => { setAddItemNeedId(null); setInlineDescription(''); setInlineItemType('software'); }}
                                className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-600 hover:bg-stone-100"
                              >
                                Annuler
                              </button>
                            </div>
                          )}
                        </div>
                      ) : (
                        <button
                          type="button"
                          onClick={() => setAddItemNeedId(need.id)}
                          className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-600 hover:bg-stone-100"
                        >
                          + Ajouter un besoin
                        </button>
                      )}
                    </div>
                  ) : null}

                      {need.status === 'Draft' || need.status === 'Rejected' ? (
                        <div className="mt-3">
                          <button type="button" onClick={() => void submitExistingNeed(need)} disabled={submittingNeedId === need.id} className="rounded-xl bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50">
                            {submittingNeedId === need.id ? 'Soumission...' : need.status === 'Rejected' ? 'Resoumettre' : 'Soumettre'}
                          </button>
                        </div>
                      ) : null}
                    </div>
                  ) : null}
                </article>
              );
            })
          )}
        </div>
      </section>
    </div>
  );
}

function TechnicianReviewView({ sessionId }: { sessionId: number }) {
  const { apiFetch, token } = useAuth();
  const [needs, setNeeds] = useState<TeachingNeedResponse[]>([]);
  const [reviewLookups, setReviewLookups] = useState<NeedItemLookups>({
    softwareNames: [],
    osOptions: [],
    laboratoryOptions: [],
    serverOptions: [],
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState<Record<number, string>>({});
  const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());
  const [exporting, setExporting] = useState(false);
  function toggleExpand(id: number) {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id); else next.add(id);
      return next;
    });
  }

  const loadNeeds = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const [data, osData, laboratoriesData, serversData] = await Promise.all([
        apiFetch<TeachingNeedResponse[]>(`/sessions/${sessionId}/needs`),
        apiFetch<OSResponse[]>('/operatingsystems'),
        apiFetch<LaboratoryLookupResponse[]>('/laboratories'),
        apiFetch<PhysicalServerResponse[]>('/physicalservers'),
      ]);
      setNeeds(data);
      setReviewLookups({
        softwareNames: [],
        osOptions: osData.map((os) => ({ value: String(os.id), label: os.name })),
        laboratoryOptions: laboratoriesData.map((lab) => ({ value: String(lab.id), label: lab.name })),
        serverOptions: serversData.map((server) => ({ value: String(server.id), label: server.hostname })),
      });
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les besoins.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sessionId]);
  async function setUnderReview(need: TeachingNeedResponse) {
    setBusyId(need.id);
    setError('');

    try {
      await ensureUnderReview(need);
      await loadNeeds();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de changer le statut en révision.'));
    } finally {
      setBusyId(null);
    }
  }


  useEffect(() => {
    void loadNeeds();
  }, [loadNeeds]);

  async function ensureUnderReview(need: TeachingNeedResponse) {
    if (need.status !== 'Submitted') return;
    await apiFetch(`/sessions/${sessionId}/needs/${need.id}/review`, { method: 'POST' });
  }

  async function approve(need: TeachingNeedResponse) {
    setBusyId(need.id);
    setError('');

    try {
      await ensureUnderReview(need);
      await apiFetch(`/sessions/${sessionId}/needs/${need.id}/approve`, { method: 'POST' });
      await loadNeeds();
    } catch (err) {
      setError(getErrorMessage(err, "Impossible d'approuver ce besoin."));
    } finally {
      setBusyId(null);
    }
  }

  async function reject(need: TeachingNeedResponse) {
    const reason = (rejectReason[need.id] ?? '').trim();
    if (!reason) {
      setError('Le motif est requis pour rejeter un besoin.');
      return;
    }

    setBusyId(need.id);
    setError('');

    try {
      await ensureUnderReview(need);
      await apiFetch(`/sessions/${sessionId}/needs/${need.id}/reject`, {
        method: 'POST',
        body: JSON.stringify({ reason }),
      });
      await loadNeeds();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de rejeter ce besoin.'));
    } finally {
      setBusyId(null);
    }
  }

  if (loading) {
    return <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>;
  }

  return (
    <section className="surface-card p-6 sm:p-8">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-stone-950">Approbation besoins</h2>
          <p className="mt-1 text-sm text-stone-600">Vue complète des besoins. Vous pouvez changer le statut, approuver ou rejeter.</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={async () => {
              setExporting(true);
              try {
                const resp = await fetch(`/api/v1/sessions/${sessionId}/export`, {
                  headers: { Authorization: `Bearer ${token}` },
                });
                if (!resp.ok) throw new Error('Export failed');
                const blob = await resp.blob();
                const url = URL.createObjectURL(blob);
                const disposition = resp.headers.get('content-disposition') ?? '';
                const match = disposition.match(/filename="?([^";\n]+)"?/);
                const fileName = match?.[1] ?? `installations_session_${sessionId}.csv`;
                const a = document.createElement('a');
                a.href = url;
                a.download = fileName;
                a.click();
                URL.revokeObjectURL(url);
              } catch (err) {
                setError(getErrorMessage(err, 'Impossible d\'exporter la matrice d\'installations.'));
              } finally {
                setExporting(false);
              }
            }}
            disabled={exporting}
            className="rounded-xl border border-emerald-200 bg-emerald-50 px-3 py-1.5 text-xs font-medium text-emerald-700 transition hover:bg-emerald-100 disabled:opacity-50"
          >
            {exporting ? 'Export...' : 'Exporter matrice d\'installations'}
          </button>
          <button
            type="button"
            onClick={() => void loadNeeds()}
            className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
          >
            Rafraîchir
          </button>
        </div>
      </div>

      {error ? <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div> : null}

      <div className="mt-5 grid gap-3">
        {needs.length === 0 ? (
          <p className="text-sm text-stone-500">Aucun besoin.</p>
        ) : (
          needs.map((need) => {
            const isExpanded = expandedIds.has(need.id);
            return (
              <article key={need.id} className="rounded-2xl border border-stone-200 bg-white/80 overflow-hidden">
                <button
                  type="button"
                  onClick={() => toggleExpand(need.id)}
                  className="flex w-full items-center justify-between gap-3 p-4 text-left hover:bg-stone-50/50 transition"
                >
                  <div className="min-w-0">
                    <p className="font-medium text-stone-900">{need.personnelFullName}</p>
                    <p className="text-sm text-stone-600">{need.courseCode}{need.courseName ? ` - ${need.courseName}` : ''} &middot; {need.items.length} besoin(s)</p>
                    <NeedTimestamps need={need} />
                  </div>
                  <div className="flex shrink-0 items-center gap-2">
                    <NeedStatusBadge status={need.status} />
                    <ChevronIcon expanded={isExpanded} />
                  </div>
                </button>

                {isExpanded ? (
                  <div className="border-t border-stone-100 px-4 pb-4">
                    <NeedDetailPanel
                      need={need}
                      lookups={reviewLookups}
                    />

                    {need.status === 'Submitted' || need.status === 'UnderReview' ? (
                      <AiReviewPanel
                        sessionId={sessionId}
                        needId={need.id}
                        onUseRejectReason={(reason) => setRejectReason((prev) => ({ ...prev, [need.id]: reason }))}
                      />
                    ) : null}

                    {need.status === 'Submitted' ? (
                      <div className="mt-3">
                        <button type="button" onClick={() => void setUnderReview(need)} disabled={busyId === need.id} className="rounded-xl border border-violet-300 bg-violet-50 px-3 py-1.5 text-xs font-medium text-violet-700 hover:bg-violet-100 disabled:opacity-50">Passer en révision</button>
                      </div>
                    ) : null}

                    {need.status === 'Submitted' || need.status === 'UnderReview' ? (
                      <div className="mt-4 grid gap-2 md:grid-cols-[1fr_auto_auto]">
                        <input className="input-field" placeholder="Motif (obligatoire en cas de rejet)" value={rejectReason[need.id] ?? ''} onChange={(event) => setRejectReason((prev) => ({ ...prev, [need.id]: event.target.value }))} />
                        <button type="button" onClick={() => void approve(need)} disabled={busyId === need.id} className="rounded-xl bg-emerald-600 px-3 py-2 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50">Approuver</button>
                        <button type="button" onClick={() => void reject(need)} disabled={busyId === need.id} className="rounded-xl bg-rose-600 px-3 py-2 text-xs font-medium text-white hover:bg-rose-700 disabled:opacity-50">Rejeter</button>
                      </div>
                    ) : null}
                  </div>
                ) : null}
              </article>
            );
          })
        )}
      </div>
    </section>
  );
}

export function SessionNeedsPage() {
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const sessionId = Number(id);
  const { apiFetch, user } = useAuth();
  const [session, setSession] = useState<SessionResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const isTeacher = user?.role === 'professor' || user?.role === 'course_instructor';
  const isReviewer = user?.role === 'lab_instructor' || user?.role === 'admin';
  const openCreateForm = searchParams.get('create') === '1';

  useEffect(() => {
    if (!Number.isFinite(sessionId)) {
      setLoading(false);
      setError('Session invalide.');
      return;
    }

    let alive = true;

    async function run() {
      setLoading(true);
      setError('');
      try {
        const data = await apiFetch<SessionResponse>(`/sessions/${sessionId}`);
        if (alive) setSession(data);
      } catch (err) {
        if (alive) setError(getErrorMessage(err, 'Impossible de charger la session.'));
      } finally {
        if (alive) setLoading(false);
      }
    }

    void run();
    return () => {
      alive = false;
    };
  }, [apiFetch, sessionId]);

  if (isTeacher && !openCreateForm) {
    return <Navigate to={`/sessions/${sessionId}/courses`} replace />;
  }

  return (
    <div className="space-y-6">
      <Link to="/besoins" className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]">&larr; Retour aux sessions</Link>

      {loading ? (
        <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>
      ) : error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : (
        <section className="surface-card p-6 sm:p-8">
          <p className="text-xs uppercase tracking-[0.3em] text-stone-500">Session</p>
          <h1 className="mt-2 text-2xl font-semibold text-stone-950 sm:text-3xl">{session?.title}</h1>
          <p className="mt-2 text-sm text-stone-600">
            {session ? `${new Date(session.startDate).toLocaleDateString('fr-FR')} - ${new Date(session.endDate).toLocaleDateString('fr-FR')}` : ''}
          </p>
        </section>
      )}

      {!loading && !error ? (
        isTeacher && session?.status !== 'Open' ? (
          <div className="rounded-2xl border border-[var(--ets-primary)]/30 bg-[rgba(220,4,44,0.06)] px-4 py-3 text-sm text-[var(--ets-primary-dark)]">
            Cette session n&apos;accepte pas de besoins actuellement. Seules les sessions ouvertes permettent la soumission.
          </div>
        ) : isTeacher ? (
          <TeacherNeedsView sessionId={sessionId} startInCreateMode={openCreateForm} />
        ) : isReviewer ? (
          <TechnicianReviewView sessionId={sessionId} />
        ) : (
          <div className="rounded-2xl border border-[var(--ets-primary)]/30 bg-[rgba(220,4,44,0.06)] px-4 py-3 text-sm text-[var(--ets-primary-dark)]">
            Cette vue n&apos;est pas accessible pour votre rôle.
          </div>
        )
      ) : null}
    </div>
  );
}
