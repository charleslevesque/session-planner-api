import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type {
  AddNeedItemRequest,
  CourseResponse,
  SoftwareResponse,
  TeachingNeedResponse,
  TeachingNeedStatus,
} from '../types/needs';
import type { SessionResponse } from '../types/sessions';

interface LocalNeedItem {
  id: string;
  softwareId?: number;
  softwareName: string;
  softwareVersion?: string;
}

function NeedStatusBadge({ status }: { status: TeachingNeedStatus }) {
  const styles: Record<TeachingNeedStatus, string> = {
    Draft: 'bg-slate-100 text-slate-700 border-slate-200',
    Submitted: 'bg-blue-100 text-blue-700 border-blue-200',
    UnderReview: 'bg-violet-100 text-violet-700 border-violet-200',
    Approved: 'bg-emerald-100 text-emerald-700 border-emerald-200',
    Rejected: 'bg-rose-100 text-rose-700 border-rose-200',
  };

  return <span className={`inline-flex rounded-xl border px-2.5 py-0.5 text-xs font-medium ${styles[status]}`}>{status}</span>;
}

function TeacherNeedsView({ sessionId, startInCreateMode = false }: { sessionId: number; startInCreateMode?: boolean }) {
  const { apiFetch } = useAuth();
  const [courses, setCourses] = useState<CourseResponse[]>([]);
  const [softwares, setSoftwares] = useState<SoftwareResponse[]>([]);
  const [needs, setNeeds] = useState<TeachingNeedResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [submittingNeedId, setSubmittingNeedId] = useState<number | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(startInCreateMode);

  const [courseCode, setCourseCode] = useState('');
  const [courseName, setCourseName] = useState('');
  const [notes, setNotes] = useState('');
  const [softwareNameInput, setSoftwareNameInput] = useState('');
  const [softwareVersionInput, setSoftwareVersionInput] = useState('');
  const [items, setItems] = useState<LocalNeedItem[]>([]);

  const courseCodeSuggestions = useMemo(() => courses.map((course) => course.code), [courses]);
  const softwareSuggestions = useMemo(() => softwares.map((software) => software.name), [softwares]);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const [coursesData, softwaresData, needsData] = await Promise.all([
        apiFetch<CourseResponse[]>('/courses'),
        apiFetch<SoftwareResponse[]>('/softwares'),
        apiFetch<TeachingNeedResponse[]>(`/sessions/${sessionId}/needs`),
      ]);

      setCourses(coursesData);
      setSoftwares(softwaresData);
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
    const softwareName = softwareNameInput.trim();
    if (!softwareName) return;

    const version = softwareVersionInput.trim();
    const dedupeKey = `${softwareName.toLowerCase()}::${version.toLowerCase()}`;

    setItems((prev) => {
      if (prev.some((entry) => `${entry.softwareName.toLowerCase()}::${(entry.softwareVersion ?? '').toLowerCase()}` === dedupeKey)) {
        return prev;
      }
      return [
        ...prev,
        {
          id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          softwareName,
          softwareVersion: version || undefined,
        },
      ];
    });

    setSoftwareNameInput('');
    setSoftwareVersionInput('');
  }

  function removeItem(itemId: string) {
    setItems((prev) => prev.filter((entry) => entry.id !== itemId));
  }

  async function resolveCourseId(): Promise<number> {
    const normalizedCode = courseCode.trim();
    if (!normalizedCode) {
      throw new Error('Veuillez renseigner un code de cours.');
    }

    const existing = courses.find((course) => course.code.toLowerCase() === normalizedCode.toLowerCase());
    if (existing) {
      return existing.id;
    }

    const created = await apiFetch<CourseResponse>('/courses', {
      method: 'POST',
      body: JSON.stringify({
        code: normalizedCode,
        name: courseName.trim() || null,
      }),
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
        body: JSON.stringify({ courseId: resolvedCourseId, notes: notes.trim() || undefined }),
      });

      for (const item of items) {
        const softwareId = await resolveSoftwareId(item.softwareName);
        const itemNotes = item.softwareVersion ? `Version demandee: ${item.softwareVersion}` : undefined;

        const payload: AddNeedItemRequest = {
          softwareId,
          quantity: 1,
          notes: itemNotes,
        };

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
      setNotes('');
      setSoftwareNameInput('');
      setSoftwareVersionInput('');
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

  async function submitExistingNeed(needId: number) {
    setSubmittingNeedId(needId);
    setError('');
    setSuccess('');

    try {
      await apiFetch(`/sessions/${sessionId}/needs/${needId}/submit`, { method: 'POST' });
      setSuccess('Besoin soumis.');
      await loadData();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de soumettre ce besoin.'));
    } finally {
      setSubmittingNeedId(null);
    }
  }

  const [addItemNeedId, setAddItemNeedId] = useState<number | null>(null);
  const [inlineSoftwareName, setInlineSoftwareName] = useState('');
  const [inlineVersionInput, setInlineVersionInput] = useState('');
  const [itemBusy, setItemBusy] = useState(false);

  async function addItemToExistingNeed(needId: number) {
    const name = inlineSoftwareName.trim();
    if (!name) return;

    setItemBusy(true);
    setError('');

    try {
      const softwareId = await resolveSoftwareId(name);
      const itemNotes = inlineVersionInput.trim() ? `Version demandee: ${inlineVersionInput.trim()}` : undefined;

      await apiFetch(`/sessions/${sessionId}/needs/${needId}/items`, {
        method: 'POST',
        body: JSON.stringify({ softwareId, quantity: 1, notes: itemNotes } satisfies AddNeedItemRequest),
      });

      setInlineSoftwareName('');
      setInlineVersionInput('');
      setAddItemNeedId(null);
      await loadData();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible d\'ajouter ce logiciel.'));
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

  if (loading) {
    return <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>;
  }

  const editableNeeds = needs.filter((need) => need.status === 'Draft' || need.status === 'Submitted');
  const lockedNeeds = needs.filter((need) => need.status !== 'Draft' && need.status !== 'Submitted');

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
            className="rounded-2xl border border-amber-300 bg-amber-100 px-4 py-2 text-sm font-semibold text-stone-900 hover:bg-amber-200"
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
                <span className="mb-2 block text-sm font-medium text-stone-700">Code cours (champ libre)</span>
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
                  placeholder="Ex: Introduction a la programmation"
                />
              </label>
            </div>

            <div className="grid gap-4 md:grid-cols-[1fr_1fr_auto]">
              <input
                value={softwareNameInput}
                onChange={(event) => setSoftwareNameInput(event.target.value)}
                className="input-field"
                list="software-name-suggestions"
                placeholder="Nom logiciel (champ libre)"
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

            {items.length > 0 ? (
              <div className="rounded-2xl border border-stone-200 bg-stone-50/70 p-4">
                <p className="text-sm font-medium text-stone-800">Logiciels sélectionnés</p>
                <ul className="mt-3 space-y-2">
                  {items.map((item) => (
                    <li key={item.id} className="flex items-center justify-between rounded-xl bg-white px-3 py-2 text-sm">
                      <span>
                        {item.softwareName}{item.softwareVersion ? ` - ${item.softwareVersion}` : ''}
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

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Notes</span>
              <textarea
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
                rows={3}
                className="input-field"
                placeholder="Précisions pédagogiques"
              />
            </label>

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
              const isEditable = need.status === 'Draft' || need.status === 'Submitted';

              return (
                <article key={need.id} className="rounded-2xl border border-stone-200 bg-white/80 p-4">
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div>
                      <p className="font-medium text-stone-900">{need.courseCode}{need.courseName ? ` - ${need.courseName}` : ''}</p>
                      <p className="text-sm text-stone-600">{need.items.length} logiciel(s)</p>
                    </div>
                    <NeedStatusBadge status={need.status} />
                  </div>

                  {need.rejectionReason ? (
                    <p className="mt-3 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">Motif: {need.rejectionReason}</p>
                  ) : null}

                  {need.items.length > 0 ? (
                    <ul className="mt-3 space-y-1.5">
                      {need.items.map((item) => (
                        <li key={item.id} className="flex items-center justify-between rounded-xl bg-stone-50 px-3 py-2 text-sm">
                          <span className="text-stone-700">
                            {item.softwareName ?? 'Logiciel inconnu'}
                            {item.notes ? ` — ${item.notes}` : ''}
                          </span>
                          {isEditable ? (
                            <button
                              type="button"
                              onClick={() => void removeItemFromNeed(need.id, item.id)}
                              disabled={itemBusy}
                              className="text-xs text-rose-600 hover:text-rose-700 disabled:opacity-50"
                            >
                              Retirer
                            </button>
                          ) : null}
                        </li>
                      ))}
                    </ul>
                  ) : null}

                  {isEditable ? (
                    <div className="mt-3">
                      {addItemNeedId === need.id ? (
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
                            onClick={() => { setAddItemNeedId(null); setInlineSoftwareName(''); setInlineVersionInput(''); }}
                            className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-600 hover:bg-stone-100"
                          >
                            Annuler
                          </button>
                        </div>
                      ) : (
                        <button
                          type="button"
                          onClick={() => setAddItemNeedId(need.id)}
                          className="rounded-xl border border-stone-300 px-3 py-1.5 text-xs text-stone-600 hover:bg-stone-100"
                        >
                          + Ajouter un logiciel
                        </button>
                      )}
                    </div>
                  ) : null}

                  {need.status === 'Draft' ? (
                    <div className="mt-3">
                      <button
                        type="button"
                        onClick={() => void submitExistingNeed(need.id)}
                        disabled={submittingNeedId === need.id}
                        className="rounded-xl bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
                      >
                        {submittingNeedId === need.id ? 'Soumission...' : 'Soumettre'}
                      </button>
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
  const { apiFetch } = useAuth();
  const [needs, setNeeds] = useState<TeachingNeedResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState<number | null>(null);
  const [rejectReason, setRejectReason] = useState<Record<number, string>>({});

  const loadNeeds = useCallback(async () => {
    setLoading(true);
    setError('');

    try {
      const data = await apiFetch<TeachingNeedResponse[]>(`/sessions/${sessionId}/needs`);
      setNeeds(data);
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
        <button
          type="button"
          onClick={() => void loadNeeds()}
          className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
        >
          Rafraîchir
        </button>
      </div>

      {error ? <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div> : null}

      <div className="mt-5 grid gap-3">
        {needs.length === 0 ? (
          <p className="text-sm text-stone-500">Aucun besoin.</p>
        ) : (
          needs.map((need) => (
            <article key={need.id} className="rounded-2xl border border-stone-200 bg-white/80 p-4">
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div>
                  <p className="font-medium text-stone-900">{need.personnelFullName}</p>
                  <p className="text-sm text-stone-600">{need.courseCode}{need.courseName ? ` - ${need.courseName}` : ''}</p>
                </div>
                <NeedStatusBadge status={need.status} />
              </div>

              {need.items.length > 0 ? (
                <ul className="mt-3 space-y-1.5">
                  {need.items.map((item) => (
                    <li key={item.id} className="rounded-xl bg-stone-50 px-3 py-2 text-sm text-stone-700">
                      {item.softwareName ?? 'Logiciel inconnu'}
                      {item.notes ? ` — ${item.notes}` : ''}
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="mt-2 text-sm text-stone-500">Aucun logiciel demandé.</p>
              )}

              {need.notes ? (
                <p className="mt-2 rounded-xl bg-stone-50 px-3 py-2 text-sm text-stone-600 italic">{need.notes}</p>
              ) : null}

              {need.rejectionReason ? (
                <p className="mt-3 rounded-xl border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">Motif: {need.rejectionReason}</p>
              ) : null}

              {need.status === 'Submitted' ? (
                <div className="mt-3">
                  <button
                    type="button"
                    onClick={() => void setUnderReview(need)}
                    disabled={busyId === need.id}
                    className="rounded-xl border border-violet-300 bg-violet-50 px-3 py-1.5 text-xs font-medium text-violet-700 hover:bg-violet-100 disabled:opacity-50"
                  >
                    Passer en révision
                  </button>
                </div>
              ) : null}

              {need.status === 'Submitted' || need.status === 'UnderReview' ? (
                <div className="mt-4 grid gap-2 md:grid-cols-[1fr_auto_auto]">
                  <input
                    className="input-field"
                    placeholder="Motif (obligatoire en cas de rejet)"
                    value={rejectReason[need.id] ?? ''}
                    onChange={(event) => setRejectReason((prev) => ({ ...prev, [need.id]: event.target.value }))}
                  />
                  <button
                    type="button"
                    onClick={() => void approve(need)}
                    disabled={busyId === need.id}
                    className="rounded-xl bg-emerald-600 px-3 py-2 text-xs font-medium text-white hover:bg-emerald-700 disabled:opacity-50"
                  >
                    Approuver
                  </button>
                  <button
                    type="button"
                    onClick={() => void reject(need)}
                    disabled={busyId === need.id}
                    className="rounded-xl bg-rose-600 px-3 py-2 text-xs font-medium text-white hover:bg-rose-700 disabled:opacity-50"
                  >
                    Rejeter
                  </button>
                </div>
              ) : null}
            </article>
          ))
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

  const isTeacher = user?.role === 'professor' || user?.role === 'course_instructor';
  const isReviewer = user?.role === 'lab_instructor' || user?.role === 'admin';
  const openCreateForm = searchParams.get('create') === '1';

  return (
    <div className="space-y-6">
      <Link to="/dashboard" className="inline-flex text-sm text-amber-700 hover:text-amber-800">&larr; Retour dashboard</Link>

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
          <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-700">
            Cette session n&apos;accepte pas de besoins actuellement. Seules les sessions ouvertes permettent la soumission.
          </div>
        ) : isTeacher ? (
          <TeacherNeedsView sessionId={sessionId} startInCreateMode={openCreateForm} />
        ) : isReviewer ? (
          <TechnicianReviewView sessionId={sessionId} />
        ) : (
          <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-700">
            Cette vue n&apos;est pas accessible pour votre rôle.
          </div>
        )
      ) : null}
    </div>
  );
}
