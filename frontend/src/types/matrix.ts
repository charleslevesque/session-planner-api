export interface LaboratorySoftwareEntry {
  laboratoryId: number;
  laboratoryName: string;
  softwareId: number;
  softwareName: string;
  status: string;
}

export interface LaboratoryBasic {
  id: number;
  name: string;
  building: string;
  numberOfPCs: number;
  seatingCapacity: number;
  workstations: WorkstationBasic[];
}

export interface WorkstationBasic {
  id: number;
  name: string;
  laboratoryId: number;
  osId: number;
  osName: string;
}

export interface SoftwareForMatrix {
  id: number;
  name: string;
  installCommand: string | null;
  softwareVersions?: SoftwareVersionForMatrix[];
}

export interface SoftwareVersionForMatrix {
  id: number;
  softwareId: number;
  osId: number;
  versionNumber: string;
  installationDetails?: string;
  notes?: string;
}

export interface OSBasic {
  id: number;
  name: string;
}

export interface CourseBasic {
  id: number;
  code: string;
  name?: string;
}
