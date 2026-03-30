import { Link } from 'react-router-dom';
import { SessionStatusBadge } from './SessionStatusBadge';
import type { SessionResponse } from '../types/sessions';

interface NeedActionPanelProps {
  session: SessionResponse;
  isTeacher: boolean;
  createNeedUrl: string;
}

export function NeedActionPanel({ session, isTeacher, createNeedUrl }: NeedActionPanelProps) {
  const isOpen = session.status === 'Open';

  if (!isTeacher) {
    return null;
  }

  return (
    <div className={`rounded-2xl border p-4 ${
      isOpen
        ? 'border-emerald-200 bg-emerald-50/70'
        : 'border-stone-200 bg-stone-50/70'
    }`}>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <SessionStatusBadge status={session.status} />
          <p className="text-sm text-stone-700">
            {isOpen
              ? 'Cette session accepte les demandes de besoins.'
              : 'Cette session n\'accepte pas de besoins actuellement.'}
          </p>
        </div>

        {isOpen ? (
          <Link
            to={createNeedUrl}
            className="inline-flex rounded-2xl bg-[var(--ets-primary)] px-4 py-2 text-sm font-semibold !text-white transition hover:bg-[var(--ets-primary-hover)] hover:!text-white"
          >
            Faire une demande
          </Link>
        ) : null}
      </div>
    </div>
  );
}
