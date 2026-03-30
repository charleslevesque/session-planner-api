import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type {
  AddNeedItemRequest,
  CourseResponse,
  NeedItemType,
  SoftwareResponse,
  TeachingNeedResponse,
} from '../types/needs';
import type { SessionResponse } from '../types/sessions';

const ITEM_TYPE_LABELS: Record<NeedItemType, string> = {
  software: 'Logiciel',
  virtual_machine: 'Machine virtuelle',
  physical_server: 'Serveur physique',
  equipment_loan: 'Prêt d\'équipement',
  other: 'Autre besoin',
};

interface LocalNeedItem {
  id: string;
  itemType: NeedItemType;
  softwareId?: number;
  softwareName?: string;
  softwareVersion?: string;
  description?: string;
}

export function CreateNeedPage() {
  const { sessionId, courseId } = useParams();
  const sId = Number(sessionId);
  const cId = Number(courseId);
  const navigate = useNavigate();
  const { apiFetch } = useAuth();

  const [session, setSession] = useState<SessionResponse | null>(null);
  const [course, setCourse] = useState<CourseResponse | null>(null);
  const [softwares, setSoftwares] = useState<SoftwareResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const [expectedStudents, setExpectedStudents] = useState('');
  const [hasTechNeeds, setHasTechNeeds] = useState<boolean | null>(null);
  const [foundAllCourses, setFoundAllCourses] = useState<boolean | null>(null);
  const [desiredModifications, setDesiredModifications] = useState('');
  const [allowsUpdates, setAllowsUpdates] = useState<boolean | null>(null);
  const [additionalComments, setAdditionalComments] = useState('');

  const [newItemType, setNewItemType] = useState<NeedItemType>('software');
  const [softwareNameInput, setSoftwareNameInput] = useState('');
  const [softwareVersionInput, setSoftwareVersionInput] = useState('');
  const [newItemDescription, setNewItemDescription] = useState('');
  const [items, setItems] = useState<LocalNeedItem[]>([]);

  const softwareSuggestions = useMemo(() => softwares.map((s) => s.name), [softwares]);

  const loadData = useCallback(async () => {
    if (!Number.isFinite(sId) || !Number.isFinite(cId)) {
      setError('Paramètres invalides.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const [sessionData, courseData, softwaresData] = await Promise.all([
        apiFetch<SessionResponse>(`/sessions/${sId}`),
        apiFetch<CourseResponse>(`/courses/${cId}`),
        apiFetch<SoftwareResponse[]>('/softwares'),
      ]);

      if (sessionData.status !== 'Open') {
        setError('Cette session n\'accepte pas de besoins actuellement.');
        setLoading(false);
        return;
      }

      setSession(sessionData);
      setCourse(courseData);
      setSoftwares(softwaresData);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les données.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sId, cId]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  function addItem() {
    if (newItemType === 'software') {
      const name = softwareNameInput.trim();
      if (!name) return;

      const version = softwareVersionInput.trim();
      const dedupeKey = `${name.toLowerCase()}::${version.toLowerCase()}`;

      setItems((prev) => {
        if (prev.some((e) => e.itemType === 'software' && `${(e.softwareName ?? '').toLowerCase()}::${(e.softwareVersion ?? '').toLowerCase()}` === dedupeKey)) {
          return prev;
        }
        return [...prev, {
          id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
          itemType: 'software',
          softwareName: name,
          softwareVersion: version || undefined,
        }];
      });

      setSoftwareNameInput('');
      setSoftwareVersionInput('');
    } else {
      const desc = newItemDescription.trim();
      if (!desc) return;

      setItems((prev) => [...prev, {
        id: `${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
        itemType: newItemType,
        description: desc,
      }]);

      setNewItemDescription('');
    }
  }

  function removeItem(itemId: string) {
    setItems((prev) => prev.filter((e) => e.id !== itemId));
  }

  async function resolveSoftwareId(name: string): Promise<number> {
    const existing = softwares.find((s) => s.name.toLowerCase() === name.toLowerCase());
    if (existing) return existing.id;

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
      const need = await apiFetch<TeachingNeedResponse>(`/sessions/${sId}/needs`, {
        method: 'POST',
        body: JSON.stringify({
          courseId: cId,
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

        await apiFetch(`/sessions/${sId}/needs/${need.id}/items`, {
          method: 'POST',
          body: JSON.stringify(payload),
        });
      }

      if (mode === 'submit') {
        await apiFetch(`/sessions/${sId}/needs/${need.id}/submit`, { method: 'POST' });
      }

      setSuccess(mode === 'submit' ? 'Besoin soumis avec succès.' : 'Brouillon sauvegardé.');

      setTimeout(() => {
        void navigate(`/sessions/${sId}/courses/${cId}`);
      }, 1200);
    } catch (err) {
      setError(getErrorMessage(err, "Impossible d'enregistrer ce besoin."));
    } finally {
      setSaving(false);
    }
  }

  const resourcesUrl = `/sessions/${sId}/courses/${cId}`;

  return (
    <div className="space-y-6">
      <Link
        to={resourcesUrl}
        className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]"
      >
        &larr; Retour aux ressources du cours
      </Link>

      {loading ? (
        <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>
      ) : error && !session ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : (
        <>
          <section className="surface-card p-6 sm:p-8">
            <p className="text-xs uppercase tracking-[0.3em] text-stone-500">Nouveau besoin</p>
            <h1 className="mt-2 text-2xl font-semibold text-stone-950">
              {course?.code}{course?.name ? ` — ${course.name}` : ''}
            </h1>
            <p className="mt-2 text-sm text-stone-600">Session: {session?.title}</p>
          </section>

          {error ? <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div> : null}
          {success ? <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{success}</div> : null}

          <section className="surface-card p-6 sm:p-8">
            <form
              className="grid gap-4"
              onSubmit={(event: FormEvent<HTMLFormElement>) => {
                event.preventDefault();
                void persistNeed('submit');
              }}
            >
              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Combien d&apos;étudiant·e·s attendez-vous pour ce cours ?</span>
                <input
                  type="number"
                  min={0}
                  value={expectedStudents}
                  onChange={(e) => setExpectedStudents(e.target.value)}
                  className="input-field max-w-xs"
                  placeholder="Ex: 35"
                />
              </label>

              <fieldset>
                <legend className="mb-2 text-sm font-medium text-stone-700">Avez-vous des besoins technologiques pour cette session ?</legend>
                <div className="flex gap-4">
                  <label className="flex items-center gap-2 text-sm"><input type="radio" name="hasTechNeeds" checked={hasTechNeeds === true} onChange={() => setHasTechNeeds(true)} /> Oui</label>
                  <label className="flex items-center gap-2 text-sm"><input type="radio" name="hasTechNeeds" checked={hasTechNeeds === false} onChange={() => setHasTechNeeds(false)} /> Non</label>
                </div>
              </fieldset>

              <fieldset>
                <legend className="mb-2 text-sm font-medium text-stone-700">Est-ce que vous avez trouvé l&apos;ensemble de vos cours dans la liste ?</legend>
                <div className="flex gap-4">
                  <label className="flex items-center gap-2 text-sm"><input type="radio" name="foundAllCourses" checked={foundAllCourses === true} onChange={() => setFoundAllCourses(true)} /> Oui</label>
                  <label className="flex items-center gap-2 text-sm"><input type="radio" name="foundAllCourses" checked={foundAllCourses === false} onChange={() => setFoundAllCourses(false)} /> Non</label>
                </div>
              </fieldset>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Souhaitez-vous apporter des modifications ?</span>
                <textarea
                  value={desiredModifications}
                  onChange={(e) => setDesiredModifications(e.target.value)}
                  rows={2}
                  className="input-field"
                  placeholder="Décrivez les modifications souhaitées"
                />
              </label>

              <fieldset>
                <legend className="mb-2 text-sm font-medium text-stone-700">Autorisez-vous l&apos;équipe technique à faire la mise à jour des logiciels et des systèmes d&apos;exploitation vers des versions subséquentes le cas échéant ?</legend>
                <div className="flex gap-4">
                  <label className="flex items-center gap-2 text-sm"><input type="radio" name="allowsUpdates" checked={allowsUpdates === true} onChange={() => setAllowsUpdates(true)} /> Oui</label>
                  <label className="flex items-center gap-2 text-sm"><input type="radio" name="allowsUpdates" checked={allowsUpdates === false} onChange={() => setAllowsUpdates(false)} /> Non</label>
                </div>
              </fieldset>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Commentaires supplémentaires</span>
                <textarea
                  value={additionalComments}
                  onChange={(e) => setAdditionalComments(e.target.value)}
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
                        onChange={(e) => setSoftwareNameInput(e.target.value)}
                        className="input-field"
                        list="create-need-sw-suggestions"
                        placeholder="Nom logiciel"
                      />
                      <datalist id="create-need-sw-suggestions">
                        {softwareSuggestions.map((name) => (
                          <option key={name} value={name} />
                        ))}
                      </datalist>
                      <input
                        value={softwareVersionInput}
                        onChange={(e) => setSoftwareVersionInput(e.target.value)}
                        className="input-field"
                        placeholder="Version (ex: 2022.3)"
                      />
                      <button type="button" onClick={addItem} className="rounded-2xl border border-stone-300 px-4 py-3 text-sm text-stone-700 transition hover:bg-stone-100">
                        Ajouter
                      </button>
                    </div>
                  ) : (
                    <div className="grid gap-3 md:grid-cols-[1fr_auto]">
                      <textarea
                        value={newItemDescription}
                        onChange={(e) => setNewItemDescription(e.target.value)}
                        rows={2}
                        className="input-field"
                        placeholder={
                          newItemType === 'virtual_machine' ? 'Décrivez vos besoins en machines virtuelles' :
                          newItemType === 'physical_server' ? 'Décrivez vos besoins en serveurs physiques' :
                          newItemType === 'equipment_loan' ? 'Quel équipement faudrait-il prêter aux étudiant·e·s ?' :
                          'Décrivez vos autres besoins technologiques'
                        }
                      />
                      <button type="button" onClick={addItem} className="rounded-2xl border border-stone-300 px-4 py-3 text-sm text-stone-700 transition hover:bg-stone-100 self-end">
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
                          <span className="inline-flex rounded-md border border-stone-200 bg-stone-100 px-1.5 py-0.5 text-[10px] font-medium text-stone-600 mr-2">
                            {ITEM_TYPE_LABELS[item.itemType]}
                          </span>
                          {item.itemType === 'software'
                            ? <>{item.softwareName}{item.softwareVersion ? ` - ${item.softwareVersion}` : ''}</>
                            : item.description}
                        </span>
                        <button type="button" onClick={() => removeItem(item.id)} className="text-xs text-rose-600 hover:text-rose-700">
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
          </section>
        </>
      )}
    </div>
  );
}
