export type TeachingNeedStatus = 'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected';

export interface CourseResponse {
  id: number;
  code: string;
  name?: string;
}

export interface SoftwareVersionResponse {
  id: number;
  softwareId: number;
  osId: number;
  versionNumber: string;
  installationDetails?: string;
  notes?: string;
}

export interface SoftwareResponse {
  id: number;
  name: string;
  softwareVersions?: SoftwareVersionResponse[];
}

export type NeedItemType =
  | 'saas'
  | 'software'
  | 'configuration'
  | 'virtual_machine'
  | 'physical_server'
  | 'equipment_loan'
  | 'other';

export interface TeachingNeedItemResponse {
  id: number;
  itemType: NeedItemType;
  softwareId?: number;
  softwareName?: string;
  softwareVersionId?: number;
  softwareVersionNumber?: string;
  osId?: number;
  osName?: string;
  quantity?: number;
  description?: string;
  notes?: string;
  detailsJson?: string | null;
}

export interface TeachingNeedResponse {
  id: number;
  sessionId: number;
  personnelId: number;
  personnelFullName: string;
  courseId: number;
  courseCode: string;
  courseName?: string;
  status: TeachingNeedStatus;
  createdAt: string;
  submittedAt?: string;
  reviewedAt?: string;
  reviewedByUserId?: number;
  rejectionReason?: string;
  notes?: string;
  expectedStudents?: number;
  hasTechNeeds?: boolean;
  foundAllCourses?: boolean;
  desiredModifications?: string;
  allowsUpdates?: boolean;
  additionalComments?: string;
  items: TeachingNeedItemResponse[];
}

export interface CreateTeachingNeedRequest {
  courseId: number;
  personnelId?: number;
  notes?: string;
  expectedStudents?: number;
  hasTechNeeds?: boolean;
  foundAllCourses?: boolean;
  desiredModifications?: string;
  allowsUpdates?: boolean;
  additionalComments?: string;
}

export interface AddNeedItemRequest {
  itemType?: NeedItemType;
  softwareId?: number;
  softwareVersionId?: number;
  osId?: number;
  quantity?: number;
  description?: string;
  notes?: string;
  detailsJson?: string | null;
}

export interface SubmitTeachingNeedResponse {
  need: TeachingNeedResponse;
  warnings: string[];
}

export interface MyNeedResponse {
  id: number;
  sessionId: number;
  sessionTitle: string;
  courseId: number;
  courseCode: string;
  courseName?: string;
  status: TeachingNeedStatus;
  createdAt: string;
  submittedAt?: string;
  reviewedAt?: string;
  rejectionReason?: string;
  notes?: string;
}
