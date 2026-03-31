export interface CourseSaaSResponse {
  id: number;
  name: string;
  numberOfAccounts?: number;
  notes?: string;
}

export interface CourseSoftwareResponse {
  id: number;
  name: string;
  installCommand?: string;
}

export interface CourseConfigurationResponse {
  id: number;
  title: string;
  osIds?: number[];
  laboratoryIds?: number[];
  notes?: string;
}

export interface CourseVmResponse {
  id: number;
  quantity: number;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  osName: string;
  hostServerHostname?: string;
  notes?: string;
}

export interface CourseServerResponse {
  id: number;
  hostname: string;
  cpuCores: number;
  ramGb: number;
  storageGb: number;
  accessType: string;
  osName: string;
  notes?: string;
}

export interface CourseEquipmentResponse {
  id: number;
  name: string;
  quantity: number;
  defaultAccessories?: string;
  notes?: string;
}

export interface CourseResourcesResponse {
  saaS: CourseSaaSResponse[];
  softwares: CourseSoftwareResponse[];
  configurations: CourseConfigurationResponse[];
  virtualMachines: CourseVmResponse[];
  physicalServers: CourseServerResponse[];
  equipment: CourseEquipmentResponse[];
  softwareVersionIds: number[];
}

export type ResourceTab = 'saas' | 'softwares' | 'configurations' | 'vms' | 'servers' | 'equipment';

export const RESOURCE_TAB_LABELS: Record<ResourceTab, string> = {
  saas: 'SaaS',
  softwares: 'Logiciels',
  configurations: 'Configurations',
  vms: 'Machines virtuelles',
  servers: 'Serveurs physiques',
  equipment: 'Prêts d\'équipement',
};

export const RESOURCE_TABS: ResourceTab[] = ['saas', 'softwares', 'configurations', 'vms', 'servers', 'equipment'];
