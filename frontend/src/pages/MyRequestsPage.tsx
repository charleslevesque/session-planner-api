import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import { summarizeNeedItem, type NeedItemLookups } from '../lib/needItemSchemas';
import {
  MINE_STATUS_FILTER_OPTIONS,
  teachingNeedStatusBadgeClass,
  teachingNeedStatusLabelFr,
  type UiNeedStatusFilter,
} from '../lib/needStatusMapping';
import type { MyNeedResponse, TeachingNeedResponse, TeachingNeedStatus } from '../types/needs';
import type { SessionResponse } from '../types/sessions';
import type { LaboratoryLookupResponse, OSResponse, PhysicalServerResponse, SoftwareResponse } from '../types/admin';

type FilterValue = 'all';

function canEdit(status: TeachingNeedStatus) {
  return status === 'Draft' || status === 'Submitted' || status === 'Rejected';
}

function canDelete(status: TeachingNeedStatus) {
  return status !== 'Approved';
}

const EMPTY_LOOKUPS: NeedItemLookups = {
  softwareNames: [],
  osOptions: [],
  laboratoryOptions: [],
  serverOptions: [],
};

function formatDateTime(iso: string | undefined): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('fr-CA', { dateStyle: 'medium', timeStyle: 'short' });
}

function ChevronIcon({ expanded }: { expanded: boolean }) {
  return (
    <svg
      className={`h-4 w-4 text-stone-400 transition-transform ${expanded ? 'rotate-180' : ''}`}
      fill="none"
      viewBox="0 0 24 24"
      strokeWidth={2}
      stroke="currentColor"
    >
      <path strokeLinecap="round" strokeLinejoin="round" d="M19.5 8.25l-7.5 7.5-7.5-7.5" />
    </svg>
  );
}

export function MyRequestsPage() {
  const { apiFetch } = useAuth();
  const [searchParams] = useSearchParams();

  const [sessions, setSessions] = useState<SessionResponse[]>([]);
  const [requests, setRequests] = useState<MyNeedResponse[]>([]);
  const [requestDetails, setRequestDetails] = useState<Record<number, TeachingNeedResponse>>({});
  const [expandedIds, setExpandedIds] = useState<Set<number>>(new Set());
  const [detailLoadingId, setDetailLoadingId] = useState<number | null>(null);
  const [lookups, setLookups] = useState<NeedItemLookups>(EMPTY_LOOKUPS);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [selectedSessionId, setSelectedSessionId] = useState<number | FilterValue>('all');
  const [selectedCourseId, setSelectedCourseId] = useState<number | FilterValue>('all');
  const [selectedStatus, setSelectedStatus] = useState<UiNeedStatusFilter>('all');

  const [confirmDeleteId, setConfirmDeleteId] = useState<number | null>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [actionError, setActionError] = useState('');

  const loadSessions = useCallback(async () => {
    try {
      const data = await apiFetch<SessionResponse[]>('/sessions?active=true');
      setSessions(data);
      return data;
    } catch {
      return [] as SessionResponse[];
    }
  }, [apiFetch]);

  const loadRequests = useCallback(async () => {
    try {
      const data = await apiFetch<MyNeedResponse[]>('/needs/mine');
      setRequests(data);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger vos demandes.'));
    }
  }, [apiFetch]);

  const loadLookups = useCallback(async () => {
    try {
      const [softwaresData, osData, laboratoriesData, serversData] = await Promise.all([
        apiFetch<SoftwareResponse[]>('/softwares'),
        apiFetch<OSResponse[]>('/operatingsystems'),
        apiFetch<LaboratoryLookupResponse[]>('/laboratories'),
        apiFetch<PhysicalServerResponse[]>('/physicalservers'),
      ]);

      setLookups({
        softwareNames: softwaresData.map((s) => s.name),
        osOptions: osData.map((os) => ({ value: String(os.id), label: os.name })),
        laboratoryOptions: laboratoriesData.map((lab) => ({ value: String(lab.id), label: lab.name })),
        serverOptions: serversData.map((server) => ({ value: String(server.id), label: server.hostname })),
      });
    } catch {
      setLookups(EMPTY_LOOKUPS);
    }
  }, [apiFetch]);

  useEffect(() => {
    let active = true;

    const init = async () => {
      setLoading(true);
      setError('');

      const [loadedSessions] = await Promise.all([loadSessions(), loadRequests(), loadLookups()]);

      if (!active) return;

      const paramSessionId = searchParams.get('sessionId');
      if (paramSessionId) {
        const parsed = Number(paramSessionId);
        if (Number.isFinite(parsed)) {
          setSelectedSessionId(parsed);
          setLoading(false);
          return;
        }
      }

      if (loadedSessions.length > 0) {
        setSelectedSessionId(loadedSessions[0].id);
      }

      setLoading(false);
    };

    void init();
    return () => { active = false; };
  }, [loadSessions, loadRequests, loadLookups, searchParams]);

  const toggleExpand = useCallback(async (request: MyNeedResponse) => {
    const isExpanded = expandedIds.has(request.id);
    if (isExpanded) {
      setExpandedIds((prev) => {
        const next = new Set(prev);
        next.delete(request.id);
        return next;
      });
      return;
    }

    setExpandedIds((prev) => {
      const next = new Set(prev);
      next.add(request.id);
      return next;
    });

    if (requestDetails[request.id]) {
      return;
    }

    setDetailLoadingId(request.id);
    try {
      const detail = await apiFetch<TeachingNeedResponse>(`/sessions/${request.sessionId}/needs/${request.id}`);
      setRequestDetails((prev) => ({ ...prev, [request.id]: detail }));
    } catch (err) {
      setActionError(getErrorMessage(err, 'Impossible de charger le détail de la demande.'));
    } finally {
      setDetailLoadingId(null);
    }
  }, [apiFetch, expandedIds, requestDetails]);

  const handleDelete = useCallback(async (r: MyNeedResponse) => {
    setDeletingId(r.id);
    setActionError('');
    try {
      await apiFetch(`/sessions/${r.sessionId}/needs/${r.id}`, { method: 'DELETE' });
      setRequests((prev) => prev.filter((x) => x.id !== r.id));
      setRequestDetails((prev) => {
        const next = { ...prev };
        delete next[r.id];
        return next;
      });
      setExpandedIds((prev) => {
        const next = new Set(prev);
        next.delete(r.id);
        return next;
      });
      setConfirmDeleteId(null);
    } catch (err) {
      setActionError(getErrorMessage(err, 'Impossible de supprimer la demande.'));
    } finally {
      setDeletingId(null);
    }
  }, [apiFetch]);

  const courseOptions = useMemo(() => {
    const map = new Map<number, { id: number; code: string; name?: string }>();
    for (const r of requests) {
      if (!map.has(r.courseId)) {
        map.set(r.courseId, { id: r.courseId, code: r.courseCode, name: r.courseName });
      }
    }
    return Array.from(map.values()).sort((a, b) => a.code.localeCompare(b.code));
  }, [requests]);

  const filtered = useMemo(() => {
    return requests.filter((r) => {
      if (selectedSessionId !== 'all' && r.sessionId !== selectedSessionId) return false;
      if (selectedCourseId !== 'all' && r.courseId !== selectedCourseId) return false;
      if (selectedStatus !== 'all' && r.status !== selectedStatus) return false;
      return true;
    });
  }, [requests, selectedSessionId, selectedCourseId, selectedStatus]);

  return (
    <div className="space-y-6">
      <Link to="/besoins" className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]">
        &larr; Retour aux sessions
      </Link>

      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Besoins</p>
        <h1 className="mt-3 text-3xl font-semibold text-stone-950">Mes demandes</h1>
        <p className="mt-3 max-w-3xl text-sm leading-7 text-stone-600 sm:text-base">
          Retrouvez l&apos;ensemble de vos demandes de besoins, toutes sessions confondues.
        </p>
      </section>

      {error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : null}

      {actionError ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{actionError}</div>
      ) : null}

      <section className="surface-card p-0">
        <div className="flex flex-wrap items-center gap-3 border-b border-stone-200 px-6 py-4">
          <h2 className="mr-auto text-base font-semibold text-stone-950">Demandes</h2>

          <select
            value={selectedSessionId}
            onChange={(e) => setSelectedSessionId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
            className="input-field max-w-[200px] text-xs"
          >
            <option value="all">Toutes les sessions</option>
            {sessions.map((s) => (
              <option key={s.id} value={s.id}>{s.title}</option>
            ))}
          </select>

          <select
            value={selectedCourseId}
            onChange={(e) => setSelectedCourseId(e.target.value === 'all' ? 'all' : Number(e.target.value))}
            className="input-field max-w-[200px] text-xs"
          >
            <option value="all">Tous les cours</option>
            {courseOptions.map((c) => (
              <option key={c.id} value={c.id}>{c.code}{c.name ? ` – ${c.name}` : ''}</option>
            ))}
          </select>

          <select
            value={selectedStatus}
            onChange={(e) => setSelectedStatus(e.target.value as UiNeedStatusFilter)}
            className="input-field max-w-[200px] text-xs"
          >
            {MINE_STATUS_FILTER_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>

        {loading ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
        ) : filtered.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Aucune demande.</div>
        ) : (
          <div className="divide-y divide-stone-100">
            {filtered.map((r) => {
              const st = r.status as TeachingNeedStatus;
              const isConfirming = confirmDeleteId === r.id;
              const isDeleting = deletingId === r.id;
              const isExpanded = expandedIds.has(r.id);
              const detail = requestDetails[r.id];

              return (
                <div key={r.id} className="px-6 py-4">
                  <div className="flex flex-wrap items-center gap-4">
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-semibold text-stone-950">
                        {r.courseCode}{r.courseName ? ` – ${r.courseName}` : ''}
                      </p>
                      <p className="mt-0.5 text-xs text-stone-500">{r.sessionTitle}</p>
                      {r.status === 'Rejected' && r.rejectionReason ? (
                        <p className="mt-1 line-clamp-2 text-xs text-rose-600">
                          Motif&nbsp;: {r.rejectionReason}
                        </p>
                      ) : null}
                    </div>

                    <span className={`shrink-0 rounded-xl border px-2.5 py-0.5 text-xs font-medium ${teachingNeedStatusBadgeClass(st)}`}>
                      {teachingNeedStatusLabelFr(st)}
                    </span>

                    <p className="shrink-0 text-xs text-stone-400">
                      {new Date(r.createdAt).toLocaleDateString('fr-FR')}
                    </p>

                    <button
                      type="button"
                      onClick={() => void toggleExpand(r)}
                      className="inline-flex h-7 w-7 items-center justify-center rounded-lg border border-stone-200 text-stone-500 transition hover:bg-stone-50"
                      aria-label={isExpanded ? 'Réduire le détail' : 'Afficher le détail'}
                    >
                      <ChevronIcon expanded={isExpanded} />
                    </button>

                    <div className="flex shrink-0 items-center gap-2">
                      {canEdit(r.status) ? (
                        <Link
                          to={`/sessions/${r.sessionId}/courses/${r.courseId}/needs/${r.id}/edit`}
                          className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs font-medium text-stone-600 transition hover:bg-stone-50"
                        >
                          Modifier
                        </Link>
                      ) : null}

                      {canDelete(r.status) ? (
                        <button
                          type="button"
                          onClick={() => {
                            setActionError('');
                            setConfirmDeleteId(r.id);
                          }}
                          className="rounded-xl border border-rose-200 px-3 py-1.5 text-xs font-medium text-rose-600 transition hover:bg-rose-50"
                        >
                          Annuler
                        </button>
                      ) : null}
                    </div>
                  </div>

                  {isConfirming ? (
                    <div className="mt-3 flex items-center gap-3 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3">
                      <p className="flex-1 text-xs text-rose-700">
                        Supprimer définitivement cette demande&nbsp;?
                      </p>
                      <button
                        type="button"
                        disabled={isDeleting}
                        onClick={() => void handleDelete(r)}
                        className="rounded-xl bg-rose-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-rose-700 disabled:opacity-50"
                      >
                        {isDeleting ? 'Suppression…' : 'Confirmer'}
                      </button>
                      <button
                        type="button"
                        disabled={isDeleting}
                        onClick={() => setConfirmDeleteId(null)}
                        className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs font-medium text-stone-600 transition hover:bg-stone-50 disabled:opacity-50"
                      >
                        Retour
                      </button>
                    </div>
                  ) : null}

                  {isExpanded ? (
                    <div className="mt-3 rounded-2xl border border-stone-200 bg-stone-50/70 px-4 py-3">
                      {detailLoadingId === r.id ? (
                        <p className="text-sm text-stone-500">Chargement du détail...</p>
                      ) : detail ? (
                        <>
                          <div className="flex flex-wrap gap-x-4 gap-y-0.5 text-[11px] text-stone-500">
                            <span>Créé: {formatDateTime(detail.createdAt)}</span>
                            <span>Soumis: {formatDateTime(detail.submittedAt)}</span>
                            <span>Révisé: {formatDateTime(detail.reviewedAt)}</span>
                          </div>

                          <p className="mt-3 text-sm font-semibold text-stone-800">Besoins ({detail.items.length})</p>
                          {detail.items.length > 0 ? (
                            <ul className="mt-2 space-y-1.5">
                              {detail.items.map((item) => {
                                const { label, summary } = summarizeNeedItem(item, lookups);
                                return (
                                  <li key={item.id} className="rounded-xl bg-white px-3 py-2 text-sm text-stone-700">
                                    <span className="mr-2 inline-flex rounded-md border border-stone-200 bg-stone-100 px-1.5 py-0.5 text-[10px] font-medium text-stone-500">
                                      {label}
                                    </span>
                                    {summary}
                                  </li>
                                );
                              })}
                            </ul>
                          ) : (
                            <p className="mt-2 text-sm text-stone-500">Aucun besoin spécifique.</p>
                          )}
                        </>
                      ) : (
                        <p className="text-sm text-stone-500">Détail indisponible pour cette demande.</p>
                      )}
                    </div>
                  ) : null}
                </div>
              );
            })}
          </div>
        )}
      </section>
    </div>
  );
}
