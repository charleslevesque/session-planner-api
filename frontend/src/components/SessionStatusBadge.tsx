import type { SessionStatus } from '../types/sessions';

const STYLES: Record<SessionStatus, string> = {
  Draft: 'bg-slate-100 text-slate-700 border-slate-200',
  Open: 'bg-emerald-100 text-emerald-700 border-emerald-200',
  Closed: 'bg-amber-100 text-amber-700 border-amber-200',
  Archived: 'bg-stone-200 text-stone-700 border-stone-300',
};

export function SessionStatusBadge({ status }: { status: SessionStatus }) {
  return (
    <span className={`inline-flex rounded-xl border px-2.5 py-0.5 text-xs font-medium ${STYLES[status]}`}>
      {status}
    </span>
  );
}
