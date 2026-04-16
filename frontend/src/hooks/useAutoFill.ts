import { useCallback, useEffect, useRef, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';

interface AutoFillSuggestion {
  value: string;
  reason: string;
  confidence: number;
}

interface AutoFillResponse {
  suggestions: Record<string, AutoFillSuggestion>;
  source: string;
}

interface UseAutoFillOptions {
  sessionId: number;
  courseId: number;
  itemType: string;
  currentValues: Record<string, string>;
  enabled?: boolean;
  debounceMs?: number;
}

export interface FieldSuggestion {
  value: string;
  reason: string;
  confidence: number;
  fieldName: string;
}

export function useAutoFill({
  sessionId,
  courseId,
  itemType,
  currentValues,
  enabled = true,
  debounceMs = 500,
}: UseAutoFillOptions) {
  const { apiFetch } = useAuth();
  const [suggestions, setSuggestions] = useState<Record<string, AutoFillSuggestion>>({});
  const [source, setSource] = useState('');
  const [loading, setLoading] = useState(false);
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const lastRequestRef = useRef('');

  const fetchSuggestions = useCallback(async (values: Record<string, string>) => {
    const requestKey = `${sessionId}-${courseId}-${itemType}-${JSON.stringify(values)}`;
    if (requestKey === lastRequestRef.current) return;
    lastRequestRef.current = requestKey;

    setLoading(true);
    try {
      const result = await apiFetch<AutoFillResponse>('/ai/auto-fill', {
        method: 'POST',
        body: JSON.stringify({ sessionId, courseId, itemType, currentValues: values }),
      });
      setSuggestions(result.suggestions);
      setSource(result.source);
    } catch {
      setSuggestions({});
      setSource('');
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sessionId, courseId, itemType]);

  useEffect(() => {
    if (!enabled) {
      setSuggestions({});
      return;
    }

    const hasAnyValue = Object.values(currentValues).some((v) => v.trim().length > 0);
    if (!hasAnyValue) {
      setSuggestions({});
      setSource('');
      return;
    }

    if (timerRef.current) clearTimeout(timerRef.current);
    timerRef.current = setTimeout(() => {
      void fetchSuggestions(currentValues);
    }, debounceMs);

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [currentValues, enabled, debounceMs, fetchSuggestions]);

  const acceptSuggestion = useCallback((fieldName: string): string | null => {
    const suggestion = suggestions[fieldName];
    if (!suggestion) return null;
    setSuggestions((prev) => {
      const next = { ...prev };
      delete next[fieldName];
      return next;
    });
    return suggestion.value;
  }, [suggestions]);

  const dismissSuggestion = useCallback((fieldName: string) => {
    setSuggestions((prev) => {
      const next = { ...prev };
      delete next[fieldName];
      return next;
    });
  }, []);

  return {
    suggestions,
    source,
    loading,
    acceptSuggestion,
    dismissSuggestion,
    hasSuggestions: Object.keys(suggestions).length > 0,
  };
}
