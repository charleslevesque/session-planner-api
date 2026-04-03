// ── Courses ──

export interface CourseResponse {
  id: number;
  code: string;
  name: string | null;
}

export interface CreateCourseRequest {
  code: string;
  name: string | null;
}

export interface UpdateCourseRequest {
  code: string;
  name: string | null;
}

// ── SaaS Products ──

export interface SaaSProductResponse {
  id: number;
  name: string;
  numberOfAccounts: number | null;
  notes: string | null;
}

export interface CreateSaaSProductRequest {
  name: string;
  numberOfAccounts: number | null;
  notes: string | null;
}

export interface UpdateSaaSProductRequest {
  name: string;
  numberOfAccounts: number | null;
  notes: string | null;
}

// ── Softwares ──

export interface SoftwareResponse {
  id: number;
  name: string;
  installCommand: string | null;
}

export interface CreateSoftwareRequest {
  name: string;
}

export interface UpdateSoftwareRequest {
  name: string;
}

// ── Software Versions ──

export interface SoftwareVersionResponse {
  id: number;
  softwareId: number;
  osId: number;
  versionNumber: string;
  installationDetails: string | null;
  notes: string | null;
}

export interface CreateSoftwareVersionRequest {
  softwareId: number;
  osId: number;
  versionNumber: string;
  installationDetails: string | null;
  notes: string | null;
}

export interface UpdateSoftwareVersionRequest {
  osId: number;
  versionNumber: string;
  installationDetails: string | null;
  notes: string | null;
}

export interface SoftwareVersionRow {
  versionId: number;
  softwareId: number;
  softwareName: string;
  osId: number;
  osName: string;
  versionNumber: string;
  installationDetails: string | null;
  notes: string | null;
}

// ── Configurations ──

export interface ConfigurationResponse {
  id: number;
  title: string;
  osIds: number[];
  laboratoryIds: number[];
  notes: string | null;
}

export interface CreateConfigurationRequest {
  title: string;
  osIds: number[];
  laboratoryIds: number[];
  notes: string | null;
}

export interface UpdateConfigurationRequest {
  title: string;
  osIds: number[];
  laboratoryIds: number[];
  notes: string | null;
}

// ── Virtual Machines ──

export interface VirtualMachineResponse {
  id: number;
  quantity: number;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  notes: string | null;
  osId: number;
  osName: string;
  hostServerId: number | null;
  hostServerHostname: string | null;
}

export interface CreateVirtualMachineRequest {
  quantity: number;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  notes: string | null;
  osId: number;
  hostServerId: number | null;
}

export interface UpdateVirtualMachineRequest {
  quantity: number;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  notes: string | null;
  osId: number;
  hostServerId: number | null;
}

// ── Physical Servers ──

export interface PhysicalServerResponse {
  id: number;
  hostname: string;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  notes: string | null;
  osId: number;
  osName: string;
}

export interface CreatePhysicalServerRequest {
  hostname: string;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  notes: string | null;
  osId: number;
}

export interface UpdatePhysicalServerRequest {
  hostname: string;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  notes: string | null;
  osId: number;
}

// ── Equipment Models ──

export interface EquipmentModelResponse {
  id: number;
  name: string;
  quantity: number;
  defaultAccessories: string | null;
  notes: string | null;
}

export interface CreateEquipmentModelRequest {
  name: string;
  quantity: number;
  defaultAccessories: string | null;
  notes: string | null;
}

export interface UpdateEquipmentModelRequest {
  name: string;
  quantity: number;
  defaultAccessories: string | null;
  notes: string | null;
}

// ── Operating Systems (lookup) ──

export interface OSResponse {
  id: number;
  name: string;
}

export interface LaboratoryLookupResponse {
  id: number;
  name: string;
}

// ── Admin resource tabs ──

export type AdminResourceTab = 'saas' | 'softwares' | 'configurations' | 'vms' | 'servers' | 'equipment';

export const ADMIN_RESOURCE_TAB_LABELS: Record<AdminResourceTab, string> = {
  saas: 'SaaS',
  softwares: 'Logiciels',
  configurations: 'Configurations',
  vms: 'Machines virtuelles',
  servers: 'Serveurs physiques',
  equipment: 'Prêts d\'équipement',
};

export const ADMIN_RESOURCE_TABS: AdminResourceTab[] = [
  'saas',
  'softwares',
  'configurations',
  'vms',
  'servers',
  'equipment',
];

// ── Form field definition ──

export interface FieldDef {
  name: string;
  label: string;
  type: 'text' | 'number' | 'textarea' | 'select';
  multiple?: boolean;
  required?: boolean;
  placeholder?: string;
  min?: number;
  options?: readonly { value: string; label: string }[];
  suggestions?: readonly string[];
}
