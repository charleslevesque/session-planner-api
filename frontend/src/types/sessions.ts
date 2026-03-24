export type SessionStatus = 'Draft' | 'Open' | 'Closed' | 'Archived';

export interface SessionResponse {
  id: number;
  title: string;
  status: SessionStatus;
  startDate: string;
  endDate: string;
  createdAt: string;
  openedAt?: string;
  closedAt?: string;
  archivedAt?: string;
  createdByUserId?: number;
}
