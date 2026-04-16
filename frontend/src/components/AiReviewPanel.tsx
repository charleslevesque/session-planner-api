import { useCallback, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';

interface AiReviewAnalysis {
  summary: string;
  alerts: string[];
  suggestedAction?: string | null;
  draftRejectReason?: string | null;
  historyComparisons: Array<{ sessionTitle: string; similarity: string }>;
}

interface AiReviewPanelProps {
  sessionId: number;
  needId: number;
  onUseRejectReason?: (reason: string) => void;
}

const ACTION_LABELS: Record<string, { label: string; color: string }> = {
  approve: { label: 'Approuver', color: 'text-emerald-700 bg-emerald-50 border-emerald-200' },
  review: { label: 'Demande d\'information', color: 'text-amber-700 bg-amber-50 border-amber-200' },
  reject: { label: 'Rejeter', color: 'text-rose-700 bg-rose-50 border-rose-200' },
};

export function AiReviewPanel({ sessionId, needId, onUseRejectReason }: AiReviewPanelProps) {
  const { apiFetch } = useAuth();
  const [loading, setLoading] = useState(false);
  const [analysis, setAnalysis] = useState<AiReviewAnalysis | null>(null);
  const [error, setError] = useState('');

  const analyze = useCallback(async () => {
    setLoading(true);
    setError('');
    setAnalysis(null);
    try {
      const result = await apiFetch<AiReviewAnalysis>('/ai/analyze-need', {
        method: 'POST',
        body: JSON.stringify({ sessionId, needId }),
      });
      setAnalysis(result);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible d\'analyser cette demande.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sessionId, needId]);

  return (
    <div className="mt-3 rounded-xl border border-violet-200 bg-gradient-to-br from-violet-50/60 to-white p-3">
      <div className="flex items-center justify-between gap-2">
        <div className="flex items-center gap-1.5">
          <span className="flex h-5 w-5 items-center justify-center rounded-md bg-violet-100 text-violet-600">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" className="h-3 w-3">
              <path d="M15.98 1.804a1 1 0 0 0-1.96 0l-.24 1.192a1 1 0 0 1-.784.785l-1.192.238a1 1 0 0 0 0 1.962l1.192.238a1 1 0 0 1 .785.785l.238 1.192a1 1 0 0 0 1.962 0l.238-1.192a1 1 0 0 1 .785-.785l1.192-.238a1 1 0 0 0 0-1.962l-1.192-.238a1 1 0 0 1-.785-.785l-.238-1.192ZM6.949 5.684a1 1 0 0 0-1.898 0l-.683 2.051a1 1 0 0 1-.633.633l-2.051.683a1 1 0 0 0 0 1.898l2.051.683a1 1 0 0 1 .633.633l.683 2.051a1 1 0 0 0 1.898 0l.683-2.051a1 1 0 0 1 .633-.633l2.051-.683a1 1 0 0 0 0-1.898l-2.051-.683a1 1 0 0 1-.633-.633L6.95 5.684Z" />
            </svg>
          </span>
          <span className="text-xs font-semibold text-violet-900">Analyse IA</span>
        </div>
        <button
          type="button"
          onClick={() => void analyze()}
          disabled={loading}
          className="rounded-lg border border-violet-300 bg-white px-2 py-1 text-[11px] font-medium text-violet-700 transition hover:bg-violet-50 disabled:opacity-50"
        >
          {loading ? 'Analyse...' : analysis ? 'Relancer' : 'Analyser'}
        </button>
      </div>

      {error ? <p className="mt-2 text-[11px] text-rose-600">{error}</p> : null}

      {loading ? (
        <div className="mt-3 flex items-center gap-2 text-[11px] text-violet-600">
          <span className="inline-block h-3 w-3 animate-spin rounded-full border-2 border-violet-300 border-t-violet-600" />
          Analyse de la demande en cours...
        </div>
      ) : null}

      {analysis && !loading ? (
        <div className="mt-3 space-y-2.5">
          <p className="text-xs leading-relaxed text-stone-800">{analysis.summary}</p>

          {analysis.suggestedAction ? (
            <div className="flex items-center gap-2">
              <span className="text-[10px] text-stone-500">Recommandation :</span>
              <span className={`inline-flex rounded-lg border px-2 py-0.5 text-[11px] font-medium ${ACTION_LABELS[analysis.suggestedAction]?.color ?? 'text-stone-600 bg-stone-50 border-stone-200'}`}>
                {ACTION_LABELS[analysis.suggestedAction]?.label ?? analysis.suggestedAction}
              </span>
            </div>
          ) : null}

          {analysis.alerts.length > 0 ? (
            <div className="rounded-lg border border-amber-200 bg-amber-50/70 px-2.5 py-2">
              <p className="text-[10px] font-medium text-amber-800 mb-1">Alertes</p>
              <ul className="space-y-0.5">
                {analysis.alerts.map((alert, i) => (
                  <li key={i} className="flex gap-1.5 text-[11px] text-amber-700">
                    <span className="shrink-0 mt-0.5">⚠</span>
                    <span>{alert}</span>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

          {analysis.historyComparisons.length > 0 ? (
            <div>
              <p className="text-[10px] font-medium text-stone-500 mb-1">Comparaison historique</p>
              <div className="space-y-1">
                {analysis.historyComparisons.map((c, i) => (
                  <div key={i} className="flex items-center gap-2 text-[11px]">
                    <span className="font-medium text-stone-700">{c.sessionTitle}</span>
                    <span className="text-stone-500">→ {c.similarity}</span>
                  </div>
                ))}
              </div>
            </div>
          ) : null}

          {analysis.draftRejectReason && onUseRejectReason ? (
            <button
              type="button"
              onClick={() => onUseRejectReason(analysis.draftRejectReason!)}
              className="rounded-lg border border-rose-200 bg-rose-50 px-2.5 py-1.5 text-[11px] font-medium text-rose-700 transition hover:bg-rose-100"
            >
              Utiliser la raison de rejet suggérée
            </button>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}
