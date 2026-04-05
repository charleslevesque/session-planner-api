import { useCallback, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../../contexts/AuthContext';
import { getErrorMessage } from '../../lib/api';
import { useAdminCrud } from '../../hooks/useAdminCrud';
import { ResourceTable, type Column } from '../ResourceTable';
import { FormModal } from './FormModal';
import { ConfirmDeleteModal } from './ConfirmDeleteModal';
import {
  ADMIN_RESOURCE_TABS,
  ADMIN_RESOURCE_TAB_LABELS,
  type AdminResourceTab,
  type FieldDef,
  type SaaSProductResponse,
  type ConfigurationResponse,
  type VirtualMachineResponse,
  type PhysicalServerResponse,
  type EquipmentModelResponse,
  type OSResponse,
  type LaboratoryLookupResponse,
} from '../../types/admin';
import type { CourseResourcesResponse } from '../../types/courseResources';
import { SoftwareAdminPanel } from './SoftwareAdminPanel';

// ── Data columns (per resource type) ──

const SAAS_COLUMNS: Column<SaaSProductResponse>[] = [
  { key: 'name', label: 'Nom', render: (item) => <span className="font-medium">{item.name}</span> },
  { key: 'accounts', label: 'Comptes', render: (item) => item.numberOfAccounts ?? '—' },
  { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
];

const VM_COLUMNS: Column<VirtualMachineResponse>[] = [
  { key: 'qty', label: 'Qté', render: (item) => item.quantity },
  { key: 'cpu', label: 'CPU', render: (item) => `${item.cpuCores} cœurs` },
  { key: 'ram', label: 'RAM', render: (item) => `${item.ramGb} Go` },
  { key: 'storage', label: 'Stockage', render: (item) => `${item.storageGb} Go` },
  { key: 'access', label: 'Accès', render: (item) => item.accessType },
  { key: 'os', label: 'OS', render: (item) => item.osName },
  { key: 'host', label: 'Hôte', render: (item) => item.hostServerHostname ?? '—' },
];

const SERVER_COLUMNS: Column<PhysicalServerResponse>[] = [
  {
    key: 'hostname',
    label: 'Hostname',
    render: (item) => (
      <code className="rounded bg-stone-100 px-1.5 py-0.5 text-xs font-mono text-stone-700">
        {item.hostname}
      </code>
    ),
  },
  { key: 'cpu', label: 'CPU', render: (item) => `${item.cpuCores} cœurs` },
  { key: 'ram', label: 'RAM', render: (item) => `${item.ramGb} Go` },
  { key: 'storage', label: 'Stockage', render: (item) => `${item.storageGb} Go` },
  { key: 'access', label: 'Accès', render: (item) => item.accessType },
  { key: 'os', label: 'OS', render: (item) => item.osName },
];

const EQUIPMENT_COLUMNS: Column<EquipmentModelResponse>[] = [
  { key: 'name', label: 'Nom', render: (item) => <span className="font-medium">{item.name}</span> },
  { key: 'qty', label: 'Quantité', render: (item) => item.quantity },
  { key: 'accessories', label: 'Accessoires', render: (item) => item.defaultAccessories ?? '—' },
  { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
];

// ── Field definitions ──

type SelectOption = { value: string; label: string };

const SAAS_FIELDS: FieldDef[] = [
  { name: 'name', label: 'Nom', type: 'text', required: true },
  { name: 'numberOfAccounts', label: 'Nombre de comptes', type: 'number', min: 0 },
  { name: 'notes', label: 'Notes', type: 'textarea' },
];

function getConfigFields(osOpts: SelectOption[], laboratoryOpts: SelectOption[]): FieldDef[] {
  return [
    { name: 'title', label: 'Titre', type: 'text', required: true },
    { name: 'osIds', label: 'Systèmes d\'exploitation', type: 'select', multiple: true, required: true, options: osOpts },
    { name: 'laboratoryIds', label: 'Laboratoires', type: 'select', multiple: true, required: true, options: laboratoryOpts },
    { name: 'notes', label: 'Notes', type: 'textarea' },
  ];
}

function getVmFields(osOpts: SelectOption[], serverOpts: SelectOption[]): FieldDef[] {
  return [
    { name: 'quantity', label: 'Quantité', type: 'number', required: true, min: 1 },
    { name: 'cpuCores', label: 'CPU (cœurs)', type: 'number', required: true, min: 1 },
    { name: 'ramGb', label: 'RAM (Go)', type: 'number', required: true, min: 1 },
    { name: 'storageGb', label: 'Stockage (Go)', type: 'number', required: true, min: 1 },
    { name: 'accessType', label: 'Type d\'accès', type: 'text', required: true, placeholder: 'Ex: SSH, RDP, VNC' },
    { name: 'osId', label: 'Système d\'exploitation', type: 'select', required: true, options: osOpts },
    { name: 'hostServerId', label: 'Serveur hôte', type: 'select', options: serverOpts },
    { name: 'notes', label: 'Notes', type: 'textarea' },
  ];
}

function getServerFields(osOpts: SelectOption[]): FieldDef[] {
  return [
    { name: 'hostname', label: 'Hostname', type: 'text', required: true },
    { name: 'cpuCores', label: 'CPU (cœurs)', type: 'number', required: true, min: 1 },
    { name: 'ramGb', label: 'RAM (Go)', type: 'number', required: true, min: 1 },
    { name: 'storageGb', label: 'Stockage (Go)', type: 'number', required: true, min: 1 },
    { name: 'accessType', label: 'Type d\'accès', type: 'text', required: true, placeholder: 'Ex: SSH, IPMI' },
    { name: 'osId', label: 'Système d\'exploitation', type: 'select', required: true, options: osOpts },
    { name: 'notes', label: 'Notes', type: 'textarea' },
  ];
}

const EQUIPMENT_FIELDS: FieldDef[] = [
  { name: 'name', label: 'Nom', type: 'text', required: true },
  { name: 'quantity', label: 'Quantité', type: 'number', required: true, min: 0 },
  { name: 'defaultAccessories', label: 'Accessoires par défaut', type: 'text' },
  { name: 'notes', label: 'Notes', type: 'textarea' },
];

// ── Value converters ──

function toStringVal(v: string | number | null | undefined): string {
  if (v === null || v === undefined) return '';
  return String(v);
}

function nullableStr(v: string): string | null {
  return v.trim() || null;
}

function nullableNum(v: string): number | null {
  if (!v.trim()) return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
}

function requiredNum(v: string): number {
  return Number(v) || 0;
}

function idsFromCsv(v: string): number[] {
  return v
    .split(',')
    .map((part) => Number(part.trim()))
    .filter((id) => Number.isFinite(id) && id > 0);
}

function csvFromIds(ids: number[] | null | undefined): string {
  if (!ids || ids.length === 0) return '';
  return ids.join(',');
}

// ── Generic Resource Panel ──

interface ResourcePanelProps<T extends { id: number }> {
  apiPath: string;
  singularLabel: string;
  dataColumns: Column<T>[];
  fields: FieldDef[];
  defaultValues: Record<string, string>;
  itemToValues: (item: T) => Record<string, string>;
  valuesToBody: (values: Record<string, string>) => Record<string, unknown>;
  getLabel: (item: T) => string;
  getSearchText: (item: T) => string;
  associatedIds?: Set<number>;
  courseId?: number;
  courseResourcePath?: string;
  onAssociationChange?: () => void;
}

function ResourcePanel<T extends { id: number }>({
  apiPath,
  singularLabel,
  dataColumns,
  fields,
  defaultValues,
  itemToValues,
  valuesToBody,
  getLabel,
  getSearchText,
  associatedIds,
  courseId,
  courseResourcePath,
  onAssociationChange,
}: ResourcePanelProps<T>) {
  const { apiFetch } = useAuth();
  const { items, loading, error, saving, load, create, update, remove, setError } =
    useAdminCrud<T>(apiPath);

  const [showCreateForm, setShowCreateForm] = useState(false);
  const [createValues, setCreateValues] = useState<Record<string, string>>({ ...defaultValues });
  const [createError, setCreateError] = useState('');

  const [editingItem, setEditingItem] = useState<T | null>(null);
  const [editValues, setEditValues] = useState<Record<string, string>>({});
  const [editError, setEditError] = useState('');

  const [deletingItem, setDeletingItem] = useState<T | null>(null);
  const [togglingId, setTogglingId] = useState<number | null>(null);

  const [filterMode, setFilterMode] = useState<'associated' | 'all'>(courseId ? 'associated' : 'all');
  const [searchQuery, setSearchQuery] = useState('');

  const filteredItems = useMemo(() => {
    let result = items;

    if (courseId && associatedIds && filterMode === 'associated') {
      result = result.filter((item) => associatedIds.has(item.id));
    }

    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase();
      result = result.filter((item) => getSearchText(item).toLowerCase().includes(q));
    }

    return result;
  }, [items, courseId, associatedIds, filterMode, searchQuery, getSearchText]);

  const associatedCount = useMemo(
    () => (associatedIds ? items.filter((i) => associatedIds.has(i.id)).length : 0),
    [items, associatedIds],
  );

  async function toggleAssociation(itemId: number, isAssociated: boolean) {
    if (!courseId || !courseResourcePath) return;
    setTogglingId(itemId);
    try {
      const method = isAssociated ? 'DELETE' : 'POST';
      await apiFetch(`/courses/${courseId}/${courseResourcePath}/${itemId}`, { method });
      onAssociationChange?.();
    } catch (err) {
      setError(getErrorMessage(err, 'Erreur lors de la modification de l\'association.'));
    } finally {
      setTogglingId(null);
    }
  }

  function startCreate() {
    setCreateValues({ ...defaultValues });
    setCreateError('');
    setShowCreateForm(true);
  }

  async function handleCreate() {
    setCreateError('');
    try {
      await create(valuesToBody(createValues));
      setShowCreateForm(false);
    } catch (err) {
      setCreateError(getErrorMessage(err, 'Impossible de créer.'));
    }
  }

  function startEdit(item: T) {
    setEditingItem(item);
    setEditValues(itemToValues(item));
    setEditError('');
  }

  async function handleUpdate() {
    if (!editingItem) return;
    setEditError('');
    try {
      await update(editingItem.id, valuesToBody(editValues));
      setEditingItem(null);
    } catch (err) {
      setEditError(getErrorMessage(err, 'Impossible de modifier.'));
    }
  }

  async function handleDelete() {
    if (!deletingItem) return;
    try {
      await remove(deletingItem.id);
      setDeletingItem(null);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de supprimer.'));
      setDeletingItem(null);
    }
  }

  const allColumns = useMemo(() => {
    const cols: Column<T>[] = [];

    if (courseId && courseResourcePath && associatedIds) {
      cols.push({
        key: 'associated',
        label: 'Association',
        className: 'w-28 text-center',
        render: (item) => {
          const linked = associatedIds.has(item.id);
          const toggling = togglingId === item.id;
          return (
            <button
              type="button"
              disabled={toggling}
              onClick={() => void toggleAssociation(item.id, linked)}
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
    } else if (associatedIds) {
      cols.push({
        key: 'associated',
        label: 'Lié',
        className: 'w-12 text-center',
        render: (item) =>
          associatedIds.has(item.id) ? (
            <span className="inline-flex items-center justify-center rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-semibold text-emerald-700">
              ✓
            </span>
          ) : (
            <span className="text-stone-300">—</span>
          ),
      });
    }

    cols.push(...dataColumns);

    cols.push({
      key: 'actions',
      label: '',
      render: (item) => (
        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={() => startEdit(item)}
            className="rounded-xl border border-stone-200 px-3 py-1 text-xs text-stone-600 transition hover:bg-stone-50"
          >
            Éditer
          </button>
          <button
            type="button"
            onClick={() => setDeletingItem(item)}
            className="rounded-xl border border-rose-200 px-3 py-1 text-xs text-rose-600 transition hover:bg-rose-50"
          >
            Supprimer
          </button>
        </div>
      ),
    });

    return cols;
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dataColumns, associatedIds, courseId, courseResourcePath, togglingId]);

  return (
    <>
      <div className="space-y-3 border-b border-stone-200 px-6 py-3">
        <div className="flex items-center justify-between">
          <span className="text-sm text-stone-600">
            {filteredItems.length}/{items.length} élément{items.length !== 1 ? 's' : ''}
            {associatedIds ? (
              <span className="ml-2 text-emerald-600">
                · {associatedCount} associé{associatedCount !== 1 ? 's' : ''}
              </span>
            ) : null}
          </span>
          <div className="flex gap-2">
            <button
              type="button"
              onClick={() => void load()}
              className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
            >
              Rafraîchir
            </button>
            <button type="button" onClick={startCreate} className="primary-button text-xs">
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
            placeholder="Rechercher..."
            className="input-field max-w-xs !py-1.5 text-xs"
          />
        </div>
      </div>

      {error ? (
        <div className="mx-6 mt-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {error}
        </div>
      ) : null}

      {loading ? (
        <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
      ) : (
        <ResourceTable
          data={filteredItems}
          columns={allColumns}
          emptyMessage={
            filterMode === 'associated' && searchQuery === ''
              ? `Aucun ${singularLabel.toLowerCase()} associé à ce cours.`
              : `Aucun ${singularLabel.toLowerCase()} trouvé.`
          }
          keyExtractor={(item) => item.id}
        />
      )}

      <FormModal
        open={showCreateForm}
        title={`Ajouter un ${singularLabel.toLowerCase()}`}
        fields={fields}
        values={createValues}
        onChange={(name, value) => setCreateValues((p) => ({ ...p, [name]: value }))}
        onSubmit={() => void handleCreate()}
        onClose={() => setShowCreateForm(false)}
        saving={saving}
        error={createError}
      />

      <FormModal
        open={editingItem !== null}
        title={`Modifier le ${singularLabel.toLowerCase()}`}
        fields={fields}
        values={editValues}
        onChange={(name, value) => setEditValues((p) => ({ ...p, [name]: value }))}
        onSubmit={() => void handleUpdate()}
        onClose={() => setEditingItem(null)}
        saving={saving}
        error={editError}
      />

      <ConfirmDeleteModal
        open={deletingItem !== null}
        itemLabel={deletingItem ? getLabel(deletingItem) : ''}
        expectedText={deletingItem ? getLabel(deletingItem) : ''}
        saving={saving}
        onConfirm={() => void handleDelete()}
        onCancel={() => setDeletingItem(null)}
      />
    </>
  );
}

// ── Main section ──

interface ResourcesAdminSectionProps {
  courseId?: number;
}

export function ResourcesAdminSection({ courseId }: ResourcesAdminSectionProps) {
  const { apiFetch } = useAuth();
  const [activeTab, setActiveTab] = useState<AdminResourceTab>('saas');

  const [osOptions, setOsOptions] = useState<SelectOption[]>([]);
  const [laboratoryOptions, setLaboratoryOptions] = useState<SelectOption[]>([]);
  const [serverOptions, setServerOptions] = useState<SelectOption[]>([]);

  // Association data (when viewing a specific course)
  const [associationMap, setAssociationMap] = useState<Record<AdminResourceTab, Set<number>>>({
    saas: new Set(),
    softwares: new Set(),
    configurations: new Set(),
    vms: new Set(),
    servers: new Set(),
    equipment: new Set(),
  });
  const [softwareVersionAssocIds, setSoftwareVersionAssocIds] = useState<Set<number>>(new Set());
  const [associationsLoaded, setAssociationsLoaded] = useState(false);

  const loadLookups = useCallback(async () => {
    try {
      const [osList, laboratoryList, serverList] = await Promise.all([
        apiFetch<OSResponse[]>('/operatingsystems'),
        apiFetch<LaboratoryLookupResponse[]>('/laboratories'),
        apiFetch<PhysicalServerResponse[]>('/physicalservers'),
      ]);
      setOsOptions(osList.map((os) => ({ value: String(os.id), label: os.name })));
      setLaboratoryOptions(laboratoryList.map((lab) => ({ value: String(lab.id), label: lab.name })));
      setServerOptions(serverList.map((s) => ({ value: String(s.id), label: s.hostname })));
    } catch {
      /* Lookups are best-effort */
    }
  }, [apiFetch]);

  const loadAssociations = useCallback(async () => {
    if (!courseId) return;
    try {
      const data = await apiFetch<CourseResourcesResponse>(`/courses/${courseId}/resources`);
      setAssociationMap({
        saas: new Set(data.saaS.map((r) => r.id)),
        softwares: new Set(data.softwares.map((r) => r.id)),
        configurations: new Set(data.configurations.map((r) => r.id)),
        vms: new Set(data.virtualMachines.map((r) => r.id)),
        servers: new Set(data.physicalServers.map((r) => r.id)),
        equipment: new Set(data.equipment.map((r) => r.id)),
      });
      setSoftwareVersionAssocIds(new Set(data.softwareVersionIds ?? []));
      setAssociationsLoaded(true);
    } catch {
      setAssociationsLoaded(true);
    }
  }, [apiFetch, courseId]);

  useEffect(() => {
    void loadLookups();
  }, [loadLookups]);

  useEffect(() => {
    void loadAssociations();
  }, [loadAssociations]);

  const vmFields = useMemo(() => getVmFields(osOptions, serverOptions), [osOptions, serverOptions]);
  const serverFields = useMemo(() => getServerFields(osOptions), [osOptions]);
  const configFields = useMemo(() => getConfigFields(osOptions, laboratoryOptions), [osOptions, laboratoryOptions]);

  const osNameById = useMemo(
    () => new Map(osOptions.map((opt) => [Number(opt.value), opt.label])),
    [osOptions],
  );
  const laboratoryNameById = useMemo(
    () => new Map(laboratoryOptions.map((opt) => [Number(opt.value), opt.label])),
    [laboratoryOptions],
  );
  const configColumns = useMemo<Column<ConfigurationResponse>[]>(
    () => [
      { key: 'title', label: 'Titre', render: (item) => <span className="font-medium">{item.title}</span> },
      {
        key: 'os',
        label: 'OS',
        render: (item) =>
          item.osIds.length > 0
            ? item.osIds.map((id) => osNameById.get(id) ?? `OS #${id}`).join(', ')
            : '—',
      },
      {
        key: 'laboratory',
        label: 'Laboratoire',
        render: (item) =>
          item.laboratoryIds.length > 0
            ? item.laboratoryIds.map((id) => laboratoryNameById.get(id) ?? `Lab #${id}`).join(', ')
            : '—',
      },
      { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
    ],
    [osNameById, laboratoryNameById],
  );

  const currentAssociatedIds = courseId && associationsLoaded ? associationMap[activeTab] : undefined;

  function renderActivePanel() {
    switch (activeTab) {
      case 'saas':
        return (
          <ResourcePanel<SaaSProductResponse>
            key="saas"
            apiPath="/saasproducts"
            singularLabel="Produit SaaS"
            dataColumns={SAAS_COLUMNS}
            fields={SAAS_FIELDS}
            defaultValues={{ name: '', numberOfAccounts: '', notes: '' }}
            itemToValues={(item) => ({
              name: item.name,
              numberOfAccounts: toStringVal(item.numberOfAccounts),
              notes: item.notes ?? '',
            })}
            valuesToBody={(v) => ({
              name: v.name,
              numberOfAccounts: nullableNum(v.numberOfAccounts),
              notes: nullableStr(v.notes),
            })}
            getLabel={(item) => item.name}
            getSearchText={(item) => `${item.name} ${item.notes ?? ''}`}
            associatedIds={currentAssociatedIds}
            courseId={courseId}
            courseResourcePath="saas"
            onAssociationChange={() => void loadAssociations()}
          />
        );

      case 'softwares':
        return (
          <SoftwareAdminPanel
            key="softwares"
            osOptions={osOptions}
            associatedIds={courseId && associationsLoaded ? softwareVersionAssocIds : undefined}
            courseId={courseId}
            onAssociationChange={() => void loadAssociations()}
          />
        );

      case 'configurations':
        return (
          <ResourcePanel<ConfigurationResponse>
            key="configurations"
            apiPath="/configurations"
            singularLabel="Configuration"
            dataColumns={configColumns}
            fields={configFields}
            defaultValues={{ title: '', osIds: '', laboratoryIds: '', notes: '' }}
            itemToValues={(item) => ({
              title: item.title,
              osIds: csvFromIds(item.osIds),
              laboratoryIds: csvFromIds(item.laboratoryIds),
              notes: item.notes ?? '',
            })}
            valuesToBody={(v) => ({
              title: v.title,
              osIds: idsFromCsv(v.osIds),
              laboratoryIds: idsFromCsv(v.laboratoryIds),
              notes: nullableStr(v.notes),
            })}
            getLabel={(item) => item.title}
            getSearchText={(item) =>
              `${item.title} ${item.notes ?? ''} ${item.osIds.map((id) => osNameById.get(id) ?? '').join(' ')} ${item.laboratoryIds.map((id) => laboratoryNameById.get(id) ?? '').join(' ')}`}
            associatedIds={currentAssociatedIds}
            courseId={courseId}
            courseResourcePath="configurations"
            onAssociationChange={() => void loadAssociations()}
          />
        );

      case 'vms':
        return (
          <ResourcePanel<VirtualMachineResponse>
            key="vms"
            apiPath="/virtualmachines"
            singularLabel="Machine virtuelle"
            dataColumns={VM_COLUMNS}
            fields={vmFields}
            defaultValues={{
              quantity: '1',
              cpuCores: '',
              ramGb: '',
              storageGb: '',
              accessType: '',
              osId: '',
              hostServerId: '',
              notes: '',
            }}
            itemToValues={(item) => ({
              quantity: String(item.quantity),
              cpuCores: String(item.cpuCores),
              ramGb: String(item.ramGb),
              storageGb: String(item.storageGb),
              accessType: item.accessType,
              osId: String(item.osId),
              hostServerId: toStringVal(item.hostServerId),
              notes: item.notes ?? '',
            })}
            valuesToBody={(v) => ({
              quantity: requiredNum(v.quantity),
              cpuCores: requiredNum(v.cpuCores),
              ramGb: requiredNum(v.ramGb),
              storageGb: requiredNum(v.storageGb),
              accessType: v.accessType,
              notes: nullableStr(v.notes),
              osId: requiredNum(v.osId),
              hostServerId: nullableNum(v.hostServerId),
            })}
            getLabel={(item) => `VM ${item.osName} (${item.quantity}×)`}
            getSearchText={(item) => `${item.osName} ${item.accessType} ${item.hostServerHostname ?? ''} ${item.notes ?? ''}`}
            associatedIds={currentAssociatedIds}
            courseId={courseId}
            courseResourcePath="vms"
            onAssociationChange={() => void loadAssociations()}
          />
        );

      case 'servers':
        return (
          <ResourcePanel<PhysicalServerResponse>
            key="servers"
            apiPath="/physicalservers"
            singularLabel="Serveur physique"
            dataColumns={SERVER_COLUMNS}
            fields={serverFields}
            defaultValues={{
              hostname: '',
              cpuCores: '',
              ramGb: '',
              storageGb: '',
              accessType: '',
              osId: '',
              notes: '',
            }}
            itemToValues={(item) => ({
              hostname: item.hostname,
              cpuCores: String(item.cpuCores),
              ramGb: String(item.ramGb),
              storageGb: String(item.storageGb),
              accessType: item.accessType,
              osId: String(item.osId),
              notes: item.notes ?? '',
            })}
            valuesToBody={(v) => ({
              hostname: v.hostname,
              cpuCores: requiredNum(v.cpuCores),
              ramGb: requiredNum(v.ramGb),
              storageGb: requiredNum(v.storageGb),
              accessType: v.accessType,
              notes: nullableStr(v.notes),
              osId: requiredNum(v.osId),
            })}
            getLabel={(item) => item.hostname}
            getSearchText={(item) => `${item.hostname} ${item.osName} ${item.accessType} ${item.notes ?? ''}`}
            associatedIds={currentAssociatedIds}
            courseId={courseId}
            courseResourcePath="servers"
            onAssociationChange={() => void loadAssociations()}
          />
        );

      case 'equipment':
        return (
          <ResourcePanel<EquipmentModelResponse>
            key="equipment"
            apiPath="/equipmentmodels"
            singularLabel="Modèle d'équipement"
            dataColumns={EQUIPMENT_COLUMNS}
            fields={EQUIPMENT_FIELDS}
            defaultValues={{ name: '', quantity: '', defaultAccessories: '', notes: '' }}
            itemToValues={(item) => ({
              name: item.name,
              quantity: String(item.quantity),
              defaultAccessories: item.defaultAccessories ?? '',
              notes: item.notes ?? '',
            })}
            valuesToBody={(v) => ({
              name: v.name,
              quantity: requiredNum(v.quantity),
              defaultAccessories: nullableStr(v.defaultAccessories),
              notes: nullableStr(v.notes),
            })}
            getLabel={(item) => item.name}
            getSearchText={(item) => `${item.name} ${item.defaultAccessories ?? ''} ${item.notes ?? ''}`}
            associatedIds={currentAssociatedIds}
            courseId={courseId}
            courseResourcePath="equipment"
            onAssociationChange={() => void loadAssociations()}
          />
        );
    }
  }

  return (
    <section className="surface-card overflow-hidden p-0">
      <div className="border-b border-stone-200 px-6 py-4">
        <h2 className="text-base font-semibold text-stone-950">
          {courseId ? 'Ressources' : 'Catalogue de ressources'}
        </h2>

        {courseId ? (
          <p className="mt-1 text-xs text-stone-500">
            Cliquez sur <span className="font-semibold text-emerald-600">Associer</span> ou{' '}
            <span className="font-semibold text-emerald-600">✓ Dissocier</span> pour gérer les
            liens entre ce cours et ses ressources.{' '}
            <span className="text-stone-400">
              Dissocier retire uniquement l&apos;association au cours — la ressource reste dans le catalogue global.
            </span>
          </p>
        ) : null}

        <div className="mt-3 flex flex-wrap gap-1.5">
          {ADMIN_RESOURCE_TABS.map((tab) => {
            const isActive = tab === activeTab;
            return (
              <button
                key={tab}
                type="button"
                onClick={() => setActiveTab(tab)}
                className={[
                  'rounded-xl border px-3 py-1.5 text-xs font-medium transition',
                  isActive
                    ? 'border-[var(--ets-primary)]/40 bg-[rgba(220,4,44,0.08)] text-[var(--ets-primary)]'
                    : 'border-stone-200 text-stone-600 hover:border-stone-300 hover:bg-stone-50',
                ].join(' ')}
              >
                {ADMIN_RESOURCE_TAB_LABELS[tab]}
              </button>
            );
          })}
        </div>
      </div>

      {renderActivePanel()}
    </section>
  );
}
