import { useCallback, useEffect, useMemo, useState, type FormEvent } from 'react';
import { createPortal } from 'react-dom';
import { useAuth } from '../../contexts/AuthContext';
import { getErrorMessage } from '../../lib/api';
import { ResourceTable, type Column } from '../ResourceTable';
import { ConfirmDeleteModal } from './ConfirmDeleteModal';
import type {
  SoftwareResponse,
  SoftwareVersionResponse,
  SoftwareVersionRow,
} from '../../types/admin';

type SelectOption = { value: string; label: string };

interface SoftwareAdminPanelProps {
  osOptions: SelectOption[];
  associatedIds?: Set<number>;
  courseId?: number;
  onAssociationChange?: () => void;
}

const EMPTY_FORM = {
  softwareName: '',
  versionNumber: '',
  osId: '',
  installationDetails: '',
  notes: '',
};

export function SoftwareAdminPanel({ osOptions, associatedIds, courseId, onAssociationChange }: SoftwareAdminPanelProps) {
  const { apiFetch } = useAuth();

  const [softwares, setSoftwares] = useState<SoftwareResponse[]>([]);
  const [versions, setVersions] = useState<SoftwareVersionResponse[]>([]);
  const [osLookup, setOsLookup] = useState<Map<number, string>>(new Map());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [createForm, setCreateForm] = useState(EMPTY_FORM);
  const [createError, setCreateError] = useState('');

  const [editingRow, setEditingRow] = useState<SoftwareVersionRow | null>(null);
  const [editForm, setEditForm] = useState(EMPTY_FORM);
  const [editError, setEditError] = useState('');

  const [deletingRow, setDeletingRow] = useState<SoftwareVersionRow | null>(null);
  const [togglingId, setTogglingId] = useState<number | null>(null);

  const [filterMode, setFilterMode] = useState<'associated' | 'all'>(courseId ? 'associated' : 'all');
  const [searchQuery, setSearchQuery] = useState('');

  async function toggleAssociation(versionId: number, isAssociated: boolean) {
    if (!courseId) return;
    setTogglingId(versionId);
    try {
      const method = isAssociated ? 'DELETE' : 'POST';
      await apiFetch(`/courses/${courseId}/softwareversions/${versionId}`, { method });
      onAssociationChange?.();
    } catch (err) {
      setError(getErrorMessage(err, 'Erreur lors de la modification de l\'association.'));
    } finally {
      setTogglingId(null);
    }
  }

  useEffect(() => {
    const map = new Map<number, string>();
    for (const opt of osOptions) map.set(Number(opt.value), opt.label);
    setOsLookup(map);
  }, [osOptions]);

  const loadAll = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [sw, sv] = await Promise.all([
        apiFetch<SoftwareResponse[]>('/softwares'),
        apiFetch<SoftwareVersionResponse[]>('/softwareversions'),
      ]);
      setSoftwares(sw);
      setVersions(sv);
    } catch (err) {
      setError(getErrorMessage(err, 'Erreur de chargement.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch]);

  useEffect(() => {
    void loadAll();
  }, [loadAll]);

  const softwareMap = useMemo(() => {
    const m = new Map<number, string>();
    for (const s of softwares) m.set(s.id, s.name);
    return m;
  }, [softwares]);

  const rows: SoftwareVersionRow[] = useMemo(
    () =>
      versions.map((v) => ({
        versionId: v.id,
        softwareId: v.softwareId,
        softwareName: softwareMap.get(v.softwareId) ?? 'Inconnu',
        osId: v.osId,
        osName: osLookup.get(v.osId) ?? 'Inconnu',
        versionNumber: v.versionNumber,
        installationDetails: v.installationDetails,
        notes: v.notes,
      })),
    [versions, softwareMap, osLookup],
  );

  const filteredRows = useMemo(() => {
    let result = rows;

    if (courseId && associatedIds && filterMode === 'associated') {
      result = result.filter((r) => associatedIds.has(r.versionId));
    }

    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase();
      result = result.filter(
        (r) =>
          r.softwareName.toLowerCase().includes(q) ||
          r.versionNumber.toLowerCase().includes(q) ||
          r.osName.toLowerCase().includes(q) ||
          (r.installationDetails?.toLowerCase().includes(q) ?? false),
      );
    }

    return result;
  }, [rows, courseId, associatedIds, filterMode, searchQuery]);

  const associatedCount = useMemo(
    () => (associatedIds ? rows.filter((r) => associatedIds.has(r.versionId)).length : 0),
    [rows, associatedIds],
  );

  // ── Create ──

  function openCreate() {
    setCreateForm({ ...EMPTY_FORM });
    setCreateError('');
    setShowCreateModal(true);
  }

  async function handleCreate(e: FormEvent) {
    e.preventDefault();
    setCreateError('');
    setSaving(true);
    try {
      const name = createForm.softwareName.trim();
      if (!name) {
        setCreateError('Le nom du logiciel est requis.');
        setSaving(false);
        return;
      }

      const existing = softwares.find(
        (s) => s.name.toLowerCase() === name.toLowerCase(),
      );

      let softwareId: number;
      if (existing) {
        softwareId = existing.id;
      } else {
        const created = await apiFetch<SoftwareResponse>('/softwares', {
          method: 'POST',
          body: JSON.stringify({ name }),
        });
        softwareId = created.id;
      }

      await apiFetch('/softwareversions', {
        method: 'POST',
        body: JSON.stringify({
          softwareId,
          osId: Number(createForm.osId),
          versionNumber: createForm.versionNumber,
          installationDetails: createForm.installationDetails.trim() || null,
          notes: createForm.notes.trim() || null,
        }),
      });

      setShowCreateModal(false);
      await loadAll();
    } catch (err) {
      setCreateError(getErrorMessage(err, 'Impossible de créer.'));
    } finally {
      setSaving(false);
    }
  }

  // ── Edit ──

  function startEdit(row: SoftwareVersionRow) {
    setEditingRow(row);
    setEditForm({
      softwareName: row.softwareName,
      versionNumber: row.versionNumber,
      osId: String(row.osId),
      installationDetails: row.installationDetails ?? '',
      notes: row.notes ?? '',
    });
    setEditError('');
  }

  async function handleEdit(e: FormEvent) {
    e.preventDefault();
    if (!editingRow) return;
    setEditError('');
    setSaving(true);
    try {
      await apiFetch(`/softwareversions/${editingRow.versionId}`, {
        method: 'PUT',
        body: JSON.stringify({
          osId: Number(editForm.osId),
          versionNumber: editForm.versionNumber,
          installationDetails: editForm.installationDetails.trim() || null,
          notes: editForm.notes.trim() || null,
        }),
      });

      setEditingRow(null);
      await loadAll();
    } catch (err) {
      setEditError(getErrorMessage(err, 'Impossible de modifier.'));
    } finally {
      setSaving(false);
    }
  }

  // ── Delete ──

  async function handleDelete() {
    if (!deletingRow) return;
    setSaving(true);
    try {
      await apiFetch(`/softwareversions/${deletingRow.versionId}`, { method: 'DELETE' });
      setDeletingRow(null);
      await loadAll();
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de supprimer.'));
      setDeletingRow(null);
    } finally {
      setSaving(false);
    }
  }

  // ── Columns ──

  const columns: Column<SoftwareVersionRow>[] = useMemo(() => {
    const cols: Column<SoftwareVersionRow>[] = [];

    if (courseId && associatedIds) {
      cols.push({
        key: 'associated',
        label: 'Association',
        className: 'w-28 text-center',
        render: (r) => {
          const linked = associatedIds.has(r.versionId);
          const toggling = togglingId === r.versionId;
          return (
            <button
              type="button"
              disabled={toggling}
              onClick={() => void toggleAssociation(r.versionId, linked)}
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
          );
        },
      });
    }

    cols.push(
      { key: 'software', label: 'Logiciel', render: (r) => <span className="font-medium">{r.softwareName}</span> },
      { key: 'version', label: 'Version', render: (r) => r.versionNumber },
      { key: 'os', label: 'OS', render: (r) => r.osName },
      {
        key: 'install',
        label: 'Paquets / Installation',
        render: (r) =>
          r.installationDetails ? (
            <code className="rounded bg-stone-100 px-1.5 py-0.5 text-xs font-mono text-stone-700">{r.installationDetails}</code>
          ) : (
            '—'
          ),
        className: 'max-w-xs truncate',
      },
      { key: 'notes', label: 'Notes', render: (r) => r.notes ?? '—', className: 'max-w-xs truncate' },
      {
        key: 'actions',
        label: '',
        render: (r) => (
          <div className="flex justify-end gap-2">
            <button type="button" onClick={() => startEdit(r)} className="rounded-xl border border-stone-200 px-3 py-1 text-xs text-stone-600 transition hover:bg-stone-50">
              Éditer
            </button>
            <button type="button" onClick={() => setDeletingRow(r)} className="rounded-xl border border-rose-200 px-3 py-1 text-xs text-rose-600 transition hover:bg-rose-50">
              Supprimer
            </button>
          </div>
        ),
      },
    );

    return cols;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [associatedIds, courseId, togglingId]);

  // ── Form fields renderer ──

  function renderCreateFormFields(
    form: typeof EMPTY_FORM,
    setForm: React.Dispatch<React.SetStateAction<typeof EMPTY_FORM>>,
  ) {
    return (
      <div className="space-y-4">
        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">
            Nom du logiciel<span className="ml-0.5 text-rose-500">*</span>
          </span>
          <input
            type="text"
            list="software-names"
            value={form.softwareName}
            onChange={(e) => setForm((p) => ({ ...p, softwareName: e.target.value }))}
            className="input-field"
            placeholder="Ex: FileZilla, Visual Studio Code"
            required
          />
          <datalist id="software-names">
            {softwares.map((s) => (
              <option key={s.id} value={s.name} />
            ))}
          </datalist>
          <p className="mt-1 text-xs text-stone-400">
            Si le logiciel existe déjà, il sera réutilisé automatiquement.
          </p>
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">
            Version<span className="ml-0.5 text-rose-500">*</span>
          </span>
          <input
            type="text"
            value={form.versionNumber}
            onChange={(e) => setForm((p) => ({ ...p, versionNumber: e.target.value }))}
            className="input-field"
            placeholder="Ex: 1.0.0, 2024, latest"
            required
          />
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">
            Système d&apos;exploitation<span className="ml-0.5 text-rose-500">*</span>
          </span>
          <select
            value={form.osId}
            onChange={(e) => setForm((p) => ({ ...p, osId: e.target.value }))}
            className="input-field"
            required
          >
            <option value="">— Sélectionner —</option>
            {osOptions.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">Paquets / Détails d&apos;installation</span>
          <input
            type="text"
            value={form.installationDetails}
            onChange={(e) => setForm((p) => ({ ...p, installationDetails: e.target.value }))}
            className="input-field"
            placeholder="Ex: choco install vscode, apt install git"
          />
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">Notes</span>
          <textarea
            value={form.notes}
            onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))}
            className="input-field min-h-[5rem] resize-y"
            rows={2}
          />
        </label>
      </div>
    );
  }

  function renderEditFormFields(
    form: typeof EMPTY_FORM,
    setForm: React.Dispatch<React.SetStateAction<typeof EMPTY_FORM>>,
  ) {
    return (
      <div className="space-y-4">
        <div>
          <span className="mb-1.5 block text-sm font-medium text-stone-700">Logiciel</span>
          <input type="text" value={form.softwareName} disabled className="input-field opacity-60" />
        </div>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">
            Version<span className="ml-0.5 text-rose-500">*</span>
          </span>
          <input
            type="text"
            value={form.versionNumber}
            onChange={(e) => setForm((p) => ({ ...p, versionNumber: e.target.value }))}
            className="input-field"
            placeholder="Ex: 1.0.0, 2024, latest"
            required
          />
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">
            Système d&apos;exploitation<span className="ml-0.5 text-rose-500">*</span>
          </span>
          <select
            value={form.osId}
            onChange={(e) => setForm((p) => ({ ...p, osId: e.target.value }))}
            className="input-field"
            required
          >
            <option value="">— Sélectionner —</option>
            {osOptions.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">Paquets / Détails d&apos;installation</span>
          <input
            type="text"
            value={form.installationDetails}
            onChange={(e) => setForm((p) => ({ ...p, installationDetails: e.target.value }))}
            className="input-field"
            placeholder="Ex: choco install vscode, apt install git"
          />
        </label>

        <label className="block">
          <span className="mb-1.5 block text-sm font-medium text-stone-700">Notes</span>
          <textarea
            value={form.notes}
            onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))}
            className="input-field min-h-[5rem] resize-y"
            rows={2}
          />
        </label>
      </div>
    );
  }

  // ── Modal wrapper ──

  function renderModal(
    open: boolean,
    title: string,
    modalError: string,
    onClose: () => void,
    onSubmit: (e: FormEvent) => void,
    children: React.ReactNode,
  ) {
    if (!open) return null;
    return createPortal(
      <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/50 px-4" onClick={onClose}>
        <div className="w-full max-w-lg max-h-[85vh] overflow-y-auto rounded-2xl border border-stone-200 bg-white p-6 shadow-2xl" onClick={(e) => e.stopPropagation()}>
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-stone-950">{title}</h2>
            <button type="button" onClick={onClose} className="rounded-lg p-1 text-stone-400 transition hover:bg-stone-100 hover:text-stone-600" aria-label="Fermer">
              <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
          {modalError ? (
            <div className="mt-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{modalError}</div>
          ) : null}
          <form className="mt-4" onSubmit={onSubmit}>
            {children}
            <div className="mt-5 flex justify-end gap-2">
              <button type="button" onClick={onClose} className="rounded-xl border border-stone-200 px-4 py-2 text-sm text-stone-700 transition hover:bg-stone-50">
                Annuler
              </button>
              <button type="submit" disabled={saving} className="primary-button">
                {saving ? 'Enregistrement...' : 'Enregistrer'}
              </button>
            </div>
          </form>
        </div>
      </div>,
      document.body,
    );
  }

  return (
    <>
      <div className="space-y-3 border-b border-stone-200 px-6 py-3">
        <div className="flex items-center justify-between">
          <span className="text-sm text-stone-600">
            {filteredRows.length}/{rows.length} version{rows.length !== 1 ? 's' : ''}
            {associatedIds ? (
              <span className="ml-2 text-emerald-600">
                · {associatedCount} associée{associatedCount !== 1 ? 's' : ''}
              </span>
            ) : null}
          </span>
          <div className="flex gap-2">
            <button type="button" onClick={() => void loadAll()} className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50">
              Rafraîchir
            </button>
            <button type="button" onClick={openCreate} className="primary-button text-xs">
              Ajouter
            </button>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {courseId && associatedIds ? (
            <div className="flex rounded-xl border border-stone-200 p-0.5">
              <button
                type="button"
                onClick={() => setFilterMode('associated')}
                className={[
                  'rounded-lg px-3 py-1 text-xs font-medium transition',
                  filterMode === 'associated'
                    ? 'bg-stone-900 text-white'
                    : 'text-stone-600 hover:bg-stone-50',
                ].join(' ')}
              >
                Associées
              </button>
              <button
                type="button"
                onClick={() => setFilterMode('all')}
                className={[
                  'rounded-lg px-3 py-1 text-xs font-medium transition',
                  filterMode === 'all'
                    ? 'bg-stone-900 text-white'
                    : 'text-stone-600 hover:bg-stone-50',
                ].join(' ')}
              >
                Toutes
              </button>
            </div>
          ) : null}

          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Rechercher par nom, version, OS..."
            className="input-field max-w-xs !py-1.5 text-xs"
          />
        </div>
      </div>

      {error ? (
        <div className="mx-6 mt-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : null}

      {loading ? (
        <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
      ) : (
        <ResourceTable
          data={filteredRows}
          columns={columns}
          emptyMessage={
            filterMode === 'associated' && searchQuery === ''
              ? 'Aucune version associée à ce cours.'
              : 'Aucun résultat.'
          }
          keyExtractor={(r) => r.versionId}
        />
      )}

      {renderModal(showCreateModal, 'Ajouter un logiciel', createError, () => setShowCreateModal(false), handleCreate,
        renderCreateFormFields(createForm, setCreateForm),
      )}

      {renderModal(editingRow !== null, 'Modifier la version', editError, () => setEditingRow(null), handleEdit,
        renderEditFormFields(editForm, setEditForm),
      )}

      <ConfirmDeleteModal
        open={deletingRow !== null}
        itemLabel={deletingRow ? `${deletingRow.softwareName} ${deletingRow.versionNumber}` : ''}
        expectedText={deletingRow ? `${deletingRow.softwareName} ${deletingRow.versionNumber}` : ''}
        saving={saving}
        onConfirm={() => void handleDelete()}
        onCancel={() => setDeletingRow(null)}
      />
    </>
  );
}
