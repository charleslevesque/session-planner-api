import type { TeachingNeedStatus } from '../types/needs';

export type UiNeedStatus = 'draft' | 'open' | 'under review' | 'closed';

const BACKEND_TO_UI: Record<TeachingNeedStatus, UiNeedStatus> = {
  Draft: 'draft',
  Submitted: 'open',
  UnderReview: 'under review',
  Approved: 'closed',
  Rejected: 'closed',
};

export function toUiStatus(backendStatus: TeachingNeedStatus): UiNeedStatus {
  return BACKEND_TO_UI[backendStatus] ?? 'draft';
}

export const UI_STATUS_OPTIONS: { value: UiNeedStatus; label: string }[] = [
  { value: 'draft', label: 'Brouillon' },
  { value: 'open', label: 'Soumis' },
  { value: 'under review', label: 'En révision' },
  { value: 'closed', label: 'Clôturé' },
];

const UI_STATUS_STYLES: Record<UiNeedStatus, string> = {
  draft: 'border-stone-300 bg-stone-50 text-stone-600',
  open: 'border-blue-200 bg-blue-50 text-blue-700',
  'under review': 'border-amber-200 bg-amber-50 text-amber-700',
  closed: 'border-emerald-200 bg-emerald-50 text-emerald-700',
};

export function uiStatusStyle(status: UiNeedStatus): string {
  return UI_STATUS_STYLES[status] ?? UI_STATUS_STYLES.draft;
}

export function uiStatusLabel(status: UiNeedStatus): string {
  return UI_STATUS_OPTIONS.find((o) => o.value === status)?.label ?? status;
}
