export interface AiSuggestedItem {
  itemType: string;
  label: string;
  softwareName?: string | null;
  version?: string | null;
  os?: string | null;
  installCommand?: string | null;
  notes?: string | null;
  reason: string;
}

export interface AiSuggestResponse {
  suggestions: AiSuggestedItem[];
  summary?: string | null;
}

export interface AiStatusResponse {
  available: boolean;
}
