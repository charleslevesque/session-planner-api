import { useCallback, useEffect, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type { AiStatusResponse, AiSuggestedItem, AiSuggestResponse } from '../types/ai';

interface AiSuggestionsPanelProps {
  sessionId: number;
  courseId: number;
  itemType?: string;
  onApplySuggestion: (suggestion: AiSuggestedItem) => void;
}

export function AiSuggestionsPanel({ sessionId, courseId, itemType, onApplySuggestion }: AiSuggestionsPanelProps) {
  const { apiFetch } = useAuth();
  const [available, setAvailable] = useState<boolean | null>(null);
  const [loading, setLoading] = useState(false);
  const [suggestions, setSuggestions] = useState<AiSuggestedItem[]>([]);
  const [summary, setSummary] = useState<string | null>(null);
  const [error, setError] = useState('');
  const [appliedIds, setAppliedIds] = useState<Set<number>>(new Set());

  useEffect(() => {
    apiFetch<AiStatusResponse>('/ai/status')
      .then((res) => setAvailable(res.available))
      .catch(() => setAvailable(false));
  }, [apiFetch]);

  const fetchSuggestions = useCallback(async () => {
    setLoading(true);
    setError('');
    setSuggestions([]);
    setSummary(null);
    setAppliedIds(new Set());
    try {
      const result = await apiFetch<AiSuggestResponse>('/ai/suggest-items', {
        method: 'POST',
        body: JSON.stringify({ sessionId, courseId, itemType: itemType || null }),
      });
      setSuggestions(result.suggestions);
      setSummary(result.summary ?? null);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les suggestions IA.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sessionId, courseId, itemType]);

  if (available === false) return null;
  if (available === null) return null;

  return (
    <div className="rounded-2xl border border-violet-200 bg-gradient-to-br from-violet-50/80 to-white p-4 sm:p-5">
      <div className="flex items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-violet-100 text-violet-600">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" className="h-4 w-4">
              <path d="M15.98 1.804a1 1 0 0 0-1.96 0l-.24 1.192a1 1 0 0 1-.784.785l-1.192.238a1 1 0 0 0 0 1.962l1.192.238a1 1 0 0 1 .785.785l.238 1.192a1 1 0 0 0 1.962 0l.238-1.192a1 1 0 0 1 .785-.785l1.192-.238a1 1 0 0 0 0-1.962l-1.192-.238a1 1 0 0 1-.785-.785l-.238-1.192ZM6.949 5.684a1 1 0 0 0-1.898 0l-.683 2.051a1 1 0 0 1-.633.633l-2.051.683a1 1 0 0 0 0 1.898l2.051.683a1 1 0 0 1 .633.633l.683 2.051a1 1 0 0 0 1.898 0l.683-2.051a1 1 0 0 1 .633-.633l2.051-.683a1 1 0 0 0 0-1.898l-2.051-.683a1 1 0 0 1-.633-.633L6.95 5.684ZM13.949 13.684a1 1 0 0 0-1.898 0l-.184.551a1 1 0 0 1-.633.633l-.551.183a1 1 0 0 0 0 1.898l.551.183a1 1 0 0 1 .633.633l.183.551a1 1 0 0 0 1.898 0l.184-.551a1 1 0 0 1 .632-.633l.551-.183a1 1 0 0 0 0-1.898l-.551-.184a1 1 0 0 1-.633-.632l-.183-.551Z" />
            </svg>
          </span>
          <div>
            <p className="text-sm font-semibold text-violet-900">Suggestions IA</p>
            <p className="text-[11px] text-violet-600/80">Basées sur l&apos;historique du cours et le catalogue</p>
          </div>
        </div>
        <button
          type="button"
          onClick={() => void fetchSuggestions()}
          disabled={loading}
          className="rounded-xl border border-violet-300 bg-white px-3 py-1.5 text-xs font-medium text-violet-700 transition hover:bg-violet-50 disabled:opacity-50"
        >
          {loading ? 'Analyse...' : suggestions.length > 0 ? 'Relancer' : 'Suggérer'}
        </button>
      </div>

      {error ? (
        <p className="mt-3 text-xs text-rose-600">{error}</p>
      ) : null}

      {loading ? (
        <div className="mt-4 flex items-center gap-2 text-xs text-violet-600">
          <span className="inline-block h-3.5 w-3.5 animate-spin rounded-full border-2 border-violet-300 border-t-violet-600" />
          Analyse du cours en cours...
        </div>
      ) : null}

      {summary && !loading ? (
        <p className="mt-3 text-xs leading-relaxed text-violet-800/90 bg-violet-100/50 rounded-xl px-3 py-2">{summary}</p>
      ) : null}

      {suggestions.length > 0 && !loading ? (
        <ul className="mt-3 space-y-2">
          {suggestions.map((s, i) => {
            const isApplied = appliedIds.has(i);
            return (
              <li
                key={i}
                className={`group flex items-start justify-between gap-3 rounded-xl border px-3 py-2.5 transition ${
                  isApplied
                    ? 'border-emerald-200 bg-emerald-50/60'
                    : 'border-violet-100 bg-white hover:border-violet-200'
                }`}
              >
                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-1.5">
                    <span className="inline-flex rounded-md border border-violet-200 bg-violet-50 px-1.5 py-0.5 text-[10px] font-medium text-violet-700">
                      {s.itemType}
                    </span>
                    <span className="text-sm font-medium text-stone-900">{s.label}</span>
                    {s.version ? <span className="text-xs text-stone-500">v{s.version}</span> : null}
                    {s.os ? <span className="text-[10px] text-stone-400">({s.os})</span> : null}
                  </div>
                  <p className="mt-0.5 text-[11px] text-stone-500">{s.reason}</p>
                  {s.installCommand ? (
                    <p className="mt-0.5 text-[10px] font-mono text-stone-400">{s.installCommand}</p>
                  ) : null}
                </div>
                <button
                  type="button"
                  onClick={() => {
                    onApplySuggestion(s);
                    setAppliedIds((prev) => new Set(prev).add(i));
                  }}
                  disabled={isApplied}
                  className={`shrink-0 rounded-lg px-2.5 py-1 text-xs font-medium transition ${
                    isApplied
                      ? 'border border-emerald-200 bg-emerald-50 text-emerald-600 cursor-default'
                      : 'border border-violet-200 bg-violet-50 text-violet-700 hover:bg-violet-100'
                  }`}
                >
                  {isApplied ? 'Ajouté' : 'Appliquer'}
                </button>
              </li>
            );
          })}
        </ul>
      ) : null}
    </div>
  );
}
