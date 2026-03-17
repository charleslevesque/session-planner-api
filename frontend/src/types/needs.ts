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

export interface TeachingNeedItemResponse {
  id: number;
  softwareId?: number;
  softwareName?: string;
  softwareVersionId?: number;
  softwareVersionNumber?: string;
  osId?: number;
  osName?: string;
  quantity?: number;
  notes?: string;
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
  items: TeachingNeedItemResponse[];
}

export interface CreateTeachingNeedRequest {
  courseId: number;
  personnelId?: number;
  notes?: string;
}

export interface AddNeedItemRequest {
  softwareId?: number;
  softwareVersionId?: number;
  osId?: number;
  quantity?: number;
  notes?: string;
}
