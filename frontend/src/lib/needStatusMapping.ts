import type { TeachingNeedStatus } from '../types/needs';

/** UI filter / display aligned with backend `TeachingNeedStatus` (no merged Approved/Rejected). */
export type UiNeedStatusFilter = TeachingNeedStatus | 'all';

export const TEACHING_NEED_STATUS_LABEL_FR: Record<TeachingNeedStatus, string> = {
  Draft: 'Brouillon',
  Submitted: 'Soumis',
  UnderReview: 'En révision',
  Approved: 'Approuvé',
  Rejected: 'Rejeté',
};

/** @deprecated Prefer TEACHING_NEED_STATUS_LABEL_FR and backend status; kept for gradual migration. */
export type UiNeedStatus = 'draft' | 'open' | 'under review' | 'closed';

const BACKEND_TO_LEGACY_UI: Record<TeachingNeedStatus, UiNeedStatus> = {
  Draft: 'draft',
  Submitted: 'open',
  UnderReview: 'under review',
  Approved: 'closed',
  Rejected: 'closed',
};

/** Legacy mapping (Approved/Rejected both map to closed). Prefer filtering on `TeachingNeedStatus` directly. */
export function toUiStatus(backendStatus: TeachingNeedStatus): UiNeedStatus {
  return BACKEND_TO_LEGACY_UI[backendStatus] ?? 'draft';
}

export const MINE_STATUS_FILTER_OPTIONS: { value: UiNeedStatusFilter; label: string }[] = [
  { value: 'all', label: 'Tous les statuts' },
  { value: 'Draft', label: TEACHING_NEED_STATUS_LABEL_FR.Draft },
  { value: 'Submitted', label: TEACHING_NEED_STATUS_LABEL_FR.Submitted },
  { value: 'UnderReview', label: TEACHING_NEED_STATUS_LABEL_FR.UnderReview },
  { value: 'Approved', label: TEACHING_NEED_STATUS_LABEL_FR.Approved },
  { value: 'Rejected', label: TEACHING_NEED_STATUS_LABEL_FR.Rejected },
];

export const STATUS_BADGE_CLASSES: Record<TeachingNeedStatus, string> = {
  Draft: 'border-stone-300 bg-stone-50 text-stone-600',
  Submitted: 'border-blue-200 bg-blue-50 text-blue-700',
  UnderReview: 'border-amber-200 bg-amber-50 text-amber-700',
  Approved: 'border-emerald-200 bg-emerald-50 text-emerald-700',
  Rejected: 'border-rose-200 bg-rose-50 text-rose-700',
};

export function teachingNeedStatusBadgeClass(status: TeachingNeedStatus): string {
  return STATUS_BADGE_CLASSES[status] ?? STATUS_BADGE_CLASSES.Draft;
}

export function teachingNeedStatusLabelFr(status: TeachingNeedStatus): string {
  return TEACHING_NEED_STATUS_LABEL_FR[status] ?? status;
}
