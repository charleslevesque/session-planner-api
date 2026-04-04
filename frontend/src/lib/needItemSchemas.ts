import type { FieldDef } from '../types/admin';
import type { NeedItemType, TeachingNeedItemResponse } from '../types/needs';

export type TeacherNeedItemType = Exclude<NeedItemType, 'other'>;

export interface LookupOption {
  value: string;
  label: string;
}

export interface NeedItemLookups {
  softwareNames: string[];
  osOptions: LookupOption[];
  laboratoryOptions: LookupOption[];
  serverOptions: LookupOption[];
}

export interface NeedItemSchema {
  type: TeacherNeedItemType;
  label: string;
  fields: FieldDef[];
  defaultValues: Record<string, string>;
}

export interface NeedItemDraft {
  id: string;
  /** Set when the item was loaded from the API (edit mode). */
  existingApiId?: number;
  /**
   * Snapshot of the values at load time (edit mode only).
   * Used to detect field-level changes so that modified items
   * are deleted + re-created rather than silently kept as-is.
   */
  originalValues?: Record<string, string>;
  itemType: TeacherNeedItemType;
  values: Record<string, string>;
}

export const TEACHER_NEED_ITEM_OPTIONS: Array<{ value: TeacherNeedItemType; label: string }> = [
  { value: 'saas', label: 'SaaS' },
  { value: 'software', label: 'Logiciels' },
  { value: 'configuration', label: 'Configurations' },
  { value: 'virtual_machine', label: 'Machines virtuelles' },
  { value: 'physical_server', label: 'Serveurs physiques' },
  { value: 'equipment_loan', label: 'Prêts d\'équipement' },
];

export const NEED_ITEM_LABELS: Record<NeedItemType, string> = {
  saas: 'SaaS',
  software: 'Logiciels',
  configuration: 'Configurations',
  virtual_machine: 'Machine virtuelle',
  physical_server: 'Serveur physique',
  equipment_loan: 'Prêt d\'équipement',
  other: 'Autre besoin',
};

const SAAS_DEFAULTS = {
  name: '',
  numberOfAccounts: '',
  notes: '',
};

const SOFTWARE_DEFAULTS = {
  softwareName: '',
  versionNumber: '',
  osId: '',
  installationDetails: '',
  notes: '',
};

const CONFIGURATION_DEFAULTS = {
  title: '',
  osIds: '',
  laboratoryIds: '',
  notes: '',
};

const VM_DEFAULTS = {
  quantity: '1',
  cpuCores: '',
  ramGb: '',
  storageGb: '',
  accessType: '',
  osId: '',
  hostServerId: '',
  notes: '',
};

const SERVER_DEFAULTS = {
  hostname: '',
  cpuCores: '',
  ramGb: '',
  storageGb: '',
  accessType: '',
  osId: '',
  notes: '',
};

const EQUIPMENT_DEFAULTS = {
  name: '',
  quantity: '1',
  defaultAccessories: '',
  notes: '',
};

function labelsFromIds(ids: string, options: LookupOption[]): string {
  return ids
    .split(',')
    .map((value) => value.trim())
    .filter(Boolean)
    .map((value) => options.find((option) => option.value === value)?.label ?? value)
    .join(', ');
}

export function parseDetailsJson(detailsJson: string | null | undefined): Record<string, string> {
  if (!detailsJson) return {};

  try {
    const parsed = JSON.parse(detailsJson) as unknown;
    if (!parsed || typeof parsed !== 'object') return {};

    return Object.fromEntries(
      Object.entries(parsed as Record<string, unknown>).map(([key, value]) => [key, value == null ? '' : String(value)]),
    );
  } catch {
    return {};
  }
}

export function getNeedItemSchema(type: TeacherNeedItemType, lookups: NeedItemLookups): NeedItemSchema {
  switch (type) {
    case 'saas':
      return {
        type,
        label: 'SaaS',
        fields: [
          { name: 'name', label: 'Nom', type: 'text', required: true },
          { name: 'numberOfAccounts', label: 'Nombre de comptes', type: 'number', min: 0 },
          { name: 'notes', label: 'Notes', type: 'textarea' },
        ],
        defaultValues: SAAS_DEFAULTS,
      };

    case 'software':
      return {
        type,
        label: 'Logiciels',
        fields: [
          { name: 'softwareName', label: 'Nom du logiciel', type: 'text', required: true, suggestions: lookups.softwareNames },
          { name: 'versionNumber', label: 'Version', type: 'text', required: true },
          { name: 'osId', label: 'Système d\'exploitation', type: 'select', required: true, options: lookups.osOptions },
          { name: 'installationDetails', label: 'Paquets / Détails d\'installation', type: 'text', placeholder: 'Ex: choco install vscode, apt install git' },
          { name: 'notes', label: 'Notes', type: 'textarea' },
        ],
        defaultValues: SOFTWARE_DEFAULTS,
      };

    case 'configuration':
      return {
        type,
        label: 'Configurations',
        fields: [
          { name: 'title', label: 'Titre', type: 'text', required: true },
          { name: 'osIds', label: 'Systèmes d\'exploitation', type: 'select', multiple: true, required: true, options: lookups.osOptions },
          { name: 'laboratoryIds', label: 'Laboratoires', type: 'select', multiple: true, required: true, options: lookups.laboratoryOptions },
          { name: 'notes', label: 'Notes', type: 'textarea' },
        ],
        defaultValues: CONFIGURATION_DEFAULTS,
      };

    case 'virtual_machine':
      return {
        type,
        label: 'Machines virtuelles',
        fields: [
          { name: 'quantity', label: 'Quantité', type: 'number', required: true, min: 1 },
          { name: 'cpuCores', label: 'CPU (cœurs)', type: 'number', required: true, min: 1 },
          { name: 'ramGb', label: 'RAM (Go)', type: 'number', required: true, min: 1 },
          { name: 'storageGb', label: 'Stockage (Go)', type: 'number', required: true, min: 1 },
          { name: 'accessType', label: 'Type d\'accès', type: 'text', required: true, placeholder: 'Ex: SSH, RDP, VNC' },
          { name: 'osId', label: 'Système d\'exploitation', type: 'select', required: true, options: lookups.osOptions },
          { name: 'hostServerId', label: 'Serveur hôte', type: 'select', options: lookups.serverOptions },
          { name: 'notes', label: 'Notes', type: 'textarea' },
        ],
        defaultValues: VM_DEFAULTS,
      };

    case 'physical_server':
      return {
        type,
        label: 'Serveurs physiques',
        fields: [
          { name: 'hostname', label: 'Hostname', type: 'text', required: true },
          { name: 'cpuCores', label: 'CPU (cœurs)', type: 'number', required: true, min: 1 },
          { name: 'ramGb', label: 'RAM (Go)', type: 'number', required: true, min: 1 },
          { name: 'storageGb', label: 'Stockage (Go)', type: 'number', required: true, min: 1 },
          { name: 'accessType', label: 'Type d\'accès', type: 'text', required: true, placeholder: 'Ex: SSH, IPMI' },
          { name: 'osId', label: 'Système d\'exploitation', type: 'select', required: true, options: lookups.osOptions },
          { name: 'notes', label: 'Notes', type: 'textarea' },
        ],
        defaultValues: SERVER_DEFAULTS,
      };

    case 'equipment_loan':
      return {
        type,
        label: 'Prêts d\'équipement',
        fields: [
          { name: 'name', label: 'Nom', type: 'text', required: true },
          { name: 'quantity', label: 'Quantité', type: 'number', required: true, min: 0 },
          { name: 'defaultAccessories', label: 'Accessoires par défaut', type: 'text' },
          { name: 'notes', label: 'Notes', type: 'textarea' },
        ],
        defaultValues: EQUIPMENT_DEFAULTS,
      };
  }
}

export function isNeedItemValid(schema: NeedItemSchema, values: Record<string, string>): boolean {
  return schema.fields.every((field) => {
    if (!field.required) return true;
    const val = values[field.name] ?? '';
    if (field.type === 'select' && field.multiple) return val.split(',').some(Boolean);
    return val.trim().length > 0;
  });
}

export function createNeedItemPayload(type: TeacherNeedItemType, values: Record<string, string>) {
  return {
    itemType: type,
    detailsJson: JSON.stringify(values),
  };
}

export function summarizeNeedItem(
  item: TeachingNeedItemResponse,
  lookups: NeedItemLookups,
): { label: string; summary: string } {
  if (item.itemType === 'other') {
    return {
      label: NEED_ITEM_LABELS.other,
      summary: item.description ?? item.notes ?? 'Autre besoin',
    };
  }

  const values = parseDetailsJson(item.detailsJson);
  const schema = getNeedItemSchema(item.itemType, lookups);

  let summary = item.description ?? item.notes ?? '';

  switch (item.itemType) {
    case 'saas':
      summary = [values.name, values.numberOfAccounts ? `${values.numberOfAccounts} comptes` : '']
        .filter(Boolean)
        .join(' · ') || summary;
      break;

    case 'software':
      summary = [values.softwareName, values.versionNumber ? `v${values.versionNumber}` : '', values.osId ? labelsFromIds(values.osId, lookups.osOptions) : '']
        .filter(Boolean)
        .join(' · ') || summary;
      break;

    case 'configuration':
      summary = [
        values.title,
        values.osIds ? labelsFromIds(values.osIds, lookups.osOptions) : '',
        values.laboratoryIds ? labelsFromIds(values.laboratoryIds, lookups.laboratoryOptions) : '',
      ].filter(Boolean).join(' · ') || summary;
      break;

    case 'virtual_machine':
      summary = [
        values.quantity ? `${values.quantity}×` : '',
        values.cpuCores ? `${values.cpuCores} cœurs` : '',
        values.ramGb ? `${values.ramGb} Go RAM` : '',
        values.storageGb ? `${values.storageGb} Go stockage` : '',
        values.osId ? labelsFromIds(values.osId, lookups.osOptions) : '',
      ].filter(Boolean).join(' · ') || summary;
      break;

    case 'physical_server':
      summary = [
        values.hostname,
        values.cpuCores ? `${values.cpuCores} cœurs` : '',
        values.ramGb ? `${values.ramGb} Go RAM` : '',
        values.storageGb ? `${values.storageGb} Go stockage` : '',
        values.osId ? labelsFromIds(values.osId, lookups.osOptions) : '',
      ].filter(Boolean).join(' · ') || summary;
      break;

    case 'equipment_loan':
      summary = [values.name, values.quantity ? `x${values.quantity}` : '', values.defaultAccessories || '']
        .filter(Boolean)
        .join(' · ') || summary;
      break;
  }

  return {
    label: schema.label,
    summary: summary || item.notes || item.description || schema.label,
  };
}