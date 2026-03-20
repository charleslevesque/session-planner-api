import { useCallback, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type {
  LaboratoryBasic,
  LaboratorySoftwareEntry,
  OSBasic,
  SoftwareForMatrix,
} from '../types/matrix';

type ViewMode = 'matrix' | 'lab';

const STATUS_COLORS: Record<string, string> = {
  W: 'bg-blue-100 text-blue-700 border-blue-200',
  M: 'bg-stone-100 text-stone-500 border-stone-200',
  L: 'bg-amber-100 text-amber-700 border-amber-200',
};

const STATUS_LABELS: Record<string, string> = {
  W: 'Working',
  M: 'Missing',
  L: 'Limited',
};

function StatusBadge({ status }: { status: string }) {
  const parts = status.split(',').map((s) => s.trim()).filter(Boolean);

  if (parts.length === 0) return <span className="text-xs text-stone-400">—</span>;

  return (
    <span className="inline-flex flex-wrap gap-1">
      {parts.map((code) => {
        const cls = STATUS_COLORS[code] ?? 'bg-stone-100 text-stone-500 border-stone-200';
        return (
          <span
            key={code}
            className={`inline-flex items-center rounded-xl border px-2 py-0.5 text-[11px] font-medium leading-none ${cls}`}
            title={STATUS_LABELS[code] ?? code}
          >
            {code}
          </span>
        );
      })}
    </span>
  );
}

export function MatrixPage() {
  const { apiFetch } = useAuth();

  const [laboratories, setLaboratories] = useState<LaboratoryBasic[]>([]);
  const [softwares, setSoftwares] = useState<SoftwareForMatrix[]>([]);
  const [entries, setEntries] = useState<LaboratorySoftwareEntry[]>([]);
  const [osList, setOsList] = useState<OSBasic[]>([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [viewMode, setViewMode] = useState<ViewMode>('matrix');
  const [search, setSearch] = useState('');
  const [filterLab, setFilterLab] = useState('');
  const [filterOS, setFilterOS] = useState('');
  const [selectedLabId, setSelectedLabId] = useState<number | ''>('');

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [labsData, softwaresData, entriesData, osData] = await Promise.all([
        apiFetch<LaboratoryBasic[]>('/laboratories'),
        apiFetch<SoftwareForMatrix[]>('/softwares'),
        apiFetch<LaboratorySoftwareEntry[]>('/laboratorysoftwares'),
        apiFetch<OSBasic[]>('/operatingsystems'),
      ]);
      setLaboratories(labsData);
      setSoftwares(softwaresData);
      setEntries(entriesData);
      setOsList(osData);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les données de la matrice.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const osMap = useMemo(() => {
    const map = new Map<number, string>();
    for (const os of osList) map.set(os.id, os.name);
    return map;
  }, [osList]);

  const entryMap = useMemo(() => {
    const map = new Map<string, string>();
    for (const e of entries) map.set(`${e.softwareId}-${e.laboratoryId}`, e.status);
    return map;
  }, [entries]);

  const softwareOsNames = useMemo(() => {
    const map = new Map<number, string>();
    for (const sw of softwares) {
      if (sw.softwareVersions && sw.softwareVersions.length > 0) {
        const uniqueOsIds = [...new Set(sw.softwareVersions.map((v) => v.osId))];
        const names = uniqueOsIds.map((id) => osMap.get(id) ?? `OS#${id}`);
        map.set(sw.id, names.join(', '));
      }
    }
    return map;
  }, [softwares, osMap]);

  const softwareVersionStr = useMemo(() => {
    const map = new Map<number, string>();
    for (const sw of softwares) {
      if (sw.softwareVersions && sw.softwareVersions.length > 0) {
        const versions = [...new Set(sw.softwareVersions.map((v) => v.versionNumber))];
        map.set(sw.id, versions.join(', '));
      }
    }
    return map;
  }, [softwares]);

  const softwareNotes = useMemo(() => {
    const map = new Map<number, string>();
    for (const sw of softwares) {
      if (sw.softwareVersions && sw.softwareVersions.length > 0) {
        const notes = sw.softwareVersions
          .map((v) => v.notes)
          .filter((n): n is string => !!n);
        if (notes.length > 0) map.set(sw.id, [...new Set(notes)].join('; '));
      }
    }
    return map;
  }, [softwares]);

  const filteredSoftwares = useMemo(() => {
    let result = softwares;

    if (search) {
      const q = search.toLowerCase();
      result = result.filter((sw) => sw.name.toLowerCase().includes(q));
    }

    if (filterOS) {
      const osId = Number(filterOS);
      result = result.filter(
        (sw) => sw.softwareVersions?.some((v) => v.osId === osId),
      );
    }

    if (filterLab) {
      const labId = Number(filterLab);
      result = result.filter((sw) =>
        entries.some((e) => e.softwareId === sw.id && e.laboratoryId === labId),
      );
    }

    return result;
  }, [softwares, search, filterOS, filterLab, entries]);

  const filteredLabs = useMemo(() => {
    if (filterLab) {
      return laboratories.filter((l) => l.id === Number(filterLab));
    }
    return laboratories;
  }, [laboratories, filterLab]);

  function resetFilters() {
    setSearch('');
    setFilterLab('');
    setFilterOS('');
  }

  const hasFilters = search || filterLab || filterOS;

  const labSoftwares = useMemo(() => {
    if (selectedLabId === '') return [];
    return entries
      .filter((e) => e.laboratoryId === selectedLabId)
      .map((e) => {
        const sw = softwares.find((s) => s.id === e.softwareId);
        return {
          ...e,
          version: sw ? softwareVersionStr.get(sw.id) ?? '—' : '—',
          osNames: sw ? softwareOsNames.get(sw.id) ?? '—' : '—',
          installCommand: sw?.installCommand ?? '—',
          notes: sw ? softwareNotes.get(sw.id) ?? '—' : '—',
        };
      })
      .filter((e) => {
        if (search && !e.softwareName.toLowerCase().includes(search.toLowerCase())) return false;
        return true;
      });
  }, [selectedLabId, entries, softwares, softwareVersionStr, softwareOsNames, softwareNotes, search]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <section className="rounded-[2rem] bg-[radial-gradient(circle_at_top_left,_rgba(255,255,255,0.12),_transparent_28%),linear-gradient(135deg,_#682a36_0%,_#dc042c_50%,_#c00328_100%)] px-6 py-8 text-white sm:px-8">
        <p className="text-xs uppercase tracking-[0.35em] text-white/90">ÉTS · Matrice</p>
        <h1 className="mt-4 text-3xl font-semibold sm:text-4xl">Matrice d&apos;installation</h1>
        <p className="mt-3 text-sm leading-7 text-white/85">
          Vue transversale des logiciels par laboratoire. Statuts W (Working), M (Missing), L (Limited).
        </p>
      </section>

      {error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {error}
        </div>
      ) : null}

      {/* Filters */}
      <section className="surface-card p-6 sm:p-8">
        <div className="flex flex-wrap items-end gap-3">
          <label className="block min-w-[180px] flex-1">
            <span className="mb-1.5 block text-xs font-medium text-stone-600">Recherche logiciel</span>
            <input
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="input-field"
              placeholder="Nom du logiciel..."
            />
          </label>

          <label className="block min-w-[160px]">
            <span className="mb-1.5 block text-xs font-medium text-stone-600">Laboratoire</span>
            <select
              value={filterLab}
              onChange={(e) => setFilterLab(e.target.value)}
              className="input-field"
            >
              <option value="">Tous</option>
              {laboratories.map((l) => (
                <option key={l.id} value={l.id}>{l.name}</option>
              ))}
            </select>
          </label>

          <label className="block min-w-[140px]">
            <span className="mb-1.5 block text-xs font-medium text-stone-600">OS</span>
            <select
              value={filterOS}
              onChange={(e) => setFilterOS(e.target.value)}
              className="input-field"
            >
              <option value="">Tous</option>
              {osList.map((os) => (
                <option key={os.id} value={os.id}>{os.name}</option>
              ))}
            </select>
          </label>



          {hasFilters ? (
            <button
              type="button"
              onClick={resetFilters}
              className="rounded-xl border border-stone-200 px-3 py-3 text-xs text-stone-600 transition hover:bg-stone-50"
            >
              Réinitialiser
            </button>
          ) : null}
        </div>
      </section>

      {/* View toggle */}
      <div className="flex gap-2">
        <button
          type="button"
          onClick={() => setViewMode('matrix')}
          className={[
            'rounded-2xl border px-4 py-2 text-sm font-medium transition',
            viewMode === 'matrix'
              ? 'border-[var(--ets-primary)]/40 bg-[rgba(220,4,44,0.08)] text-[var(--ets-primary)] shadow-lg shadow-[rgba(220,4,44,0.12)]'
              : 'border-stone-200 bg-white/75 text-stone-600 hover:border-[var(--ets-primary)]/30 hover:bg-[rgba(220,4,44,0.04)]',
          ].join(' ')}
        >
          Vue matrice
        </button>
        <button
          type="button"
          onClick={() => setViewMode('lab')}
          className={[
            'rounded-2xl border px-4 py-2 text-sm font-medium transition',
            viewMode === 'lab'
              ? 'border-[var(--ets-primary)]/40 bg-[rgba(220,4,44,0.08)] text-[var(--ets-primary)] shadow-lg shadow-[rgba(220,4,44,0.12)]'
              : 'border-stone-200 bg-white/75 text-stone-600 hover:border-[var(--ets-primary)]/30 hover:bg-[rgba(220,4,44,0.04)]',
          ].join(' ')}
        >
          Vue par laboratoire
        </button>
      </div>

      {/* Content */}
      {loading ? (
        <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
      ) : viewMode === 'matrix' ? (
        <MatrixView
          softwares={filteredSoftwares}
          laboratories={filteredLabs}
          entryMap={entryMap}
          softwareVersionStr={softwareVersionStr}
          softwareOsNames={softwareOsNames}
          softwareNotes={softwareNotes}
          onRefresh={() => void loadData()}
        />
      ) : (
        <LabDetailView
          laboratories={laboratories}
          selectedLabId={selectedLabId}
          onSelectLab={setSelectedLabId}
          labSoftwares={labSoftwares}
          onRefresh={() => void loadData()}
        />
      )}
    </div>
  );
}

interface MatrixViewProps {
  softwares: SoftwareForMatrix[];
  laboratories: LaboratoryBasic[];
  entryMap: Map<string, string>;
  softwareVersionStr: Map<number, string>;
  softwareOsNames: Map<number, string>;
  softwareNotes: Map<number, string>;
  onRefresh: () => void;
}

function MatrixView({
  softwares,
  laboratories,
  entryMap,
  softwareVersionStr,
  softwareOsNames,
  softwareNotes,
  onRefresh,
}: MatrixViewProps) {
  return (
    <section className="surface-card overflow-hidden p-0">
      <div className="flex items-center justify-between border-b border-stone-200 px-6 py-4">
        <h2 className="text-base font-semibold text-stone-950">
          Matrice logiciels / laboratoires
          <span className="ml-2 text-xs font-normal text-stone-500">
            {softwares.length} logiciel(s) × {laboratories.length} lab(s)
          </span>
        </h2>
        <button
          type="button"
          onClick={onRefresh}
          className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
        >
          Rafraîchir
        </button>
      </div>

      {softwares.length === 0 ? (
        <div className="px-6 py-10 text-center text-sm text-stone-500">
          Aucun logiciel à afficher.
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-sm">
            <thead>
              <tr className="bg-stone-950/95 text-xs uppercase tracking-[0.15em] text-white/70">
                <th className="sticky left-0 z-20 bg-stone-950/95 px-4 py-3 text-left font-medium">
                  Logiciel
                </th>
                <th className="px-4 py-3 text-left font-medium">Version</th>
                <th className="px-4 py-3 text-left font-medium">OS</th>
                <th className="px-4 py-3 text-left font-medium">Paquets</th>
                <th className="px-4 py-3 text-left font-medium">Notes</th>
                {laboratories.map((lab) => (
                  <th key={lab.id} className="px-3 py-3 text-center font-medium whitespace-nowrap">
                    {lab.name}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {softwares.map((sw, idx) => (
                <tr
                  key={sw.id}
                  className={idx % 2 === 0 ? 'bg-white/60' : 'bg-stone-50/40'}
                >
                  <td className="sticky left-0 z-10 border-b border-stone-100 px-4 py-3 font-medium text-stone-900 whitespace-nowrap"
                    style={{ backgroundColor: idx % 2 === 0 ? 'rgba(255,255,255,0.95)' : 'rgba(250,250,249,0.95)' }}
                  >
                    {sw.name}
                  </td>
                  <td className="border-b border-stone-100 px-4 py-3 text-stone-600 whitespace-nowrap">
                    {softwareVersionStr.get(sw.id) ?? '—'}
                  </td>
                  <td className="border-b border-stone-100 px-4 py-3 text-stone-600 whitespace-nowrap">
                    {softwareOsNames.get(sw.id) ?? '—'}
                  </td>
                  <td className="border-b border-stone-100 px-4 py-3 text-stone-600 max-w-[200px] truncate" title={sw.installCommand ?? undefined}>
                    {sw.installCommand ?? '—'}
                  </td>
                  <td className="border-b border-stone-100 px-4 py-3 text-stone-600 max-w-[180px] truncate" title={softwareNotes.get(sw.id)}>
                    {softwareNotes.get(sw.id) ?? '—'}
                  </td>
                  {laboratories.map((lab) => {
                    const status = entryMap.get(`${sw.id}-${lab.id}`);
                    return (
                      <td
                        key={lab.id}
                        className="border-b border-stone-100 px-3 py-3 text-center"
                      >
                        {status ? <StatusBadge status={status} /> : <span className="text-stone-300">—</span>}
                      </td>
                    );
                  })}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}

interface LabDetailEntry {
  laboratoryId: number;
  laboratoryName: string;
  softwareId: number;
  softwareName: string;
  status: string;
  version: string;
  osNames: string;
  installCommand: string;
  notes: string;
}

interface LabDetailViewProps {
  laboratories: LaboratoryBasic[];
  selectedLabId: number | '';
  onSelectLab: (id: number | '') => void;
  labSoftwares: LabDetailEntry[];
  onRefresh: () => void;
}

function LabDetailView({
  laboratories,
  selectedLabId,
  onSelectLab,
  labSoftwares,
  onRefresh,
}: LabDetailViewProps) {
  return (
    <section className="surface-card p-6 sm:p-8">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-stone-950">Vue par laboratoire</h2>
          <p className="mt-1 text-sm text-stone-600">
            Sélectionnez un laboratoire pour voir ses logiciels et statuts.
          </p>
        </div>
        <button
          type="button"
          onClick={onRefresh}
          className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
        >
          Rafraîchir
        </button>
      </div>

      <div className="mt-5">
        <label className="block max-w-sm">
          <span className="mb-1.5 block text-xs font-medium text-stone-600">Laboratoire</span>
          <select
            value={selectedLabId}
            onChange={(e) => onSelectLab(e.target.value ? Number(e.target.value) : '')}
            className="input-field"
          >
            <option value="">— Choisir un laboratoire —</option>
            {laboratories.map((l) => (
              <option key={l.id} value={l.id}>
                {l.name} ({l.building})
              </option>
            ))}
          </select>
        </label>
      </div>

      {selectedLabId === '' ? (
        <div className="mt-8 text-center text-sm text-stone-500">
          Sélectionnez un laboratoire ci-dessus.
        </div>
      ) : labSoftwares.length === 0 ? (
        <div className="mt-8 text-center text-sm text-stone-500">
          Aucun logiciel associé à ce laboratoire.
        </div>
      ) : (
        <div className="mt-6 divide-y divide-stone-100">
          {labSoftwares.map((entry) => (
            <div key={`${entry.softwareId}-${entry.laboratoryId}`} className="flex flex-wrap items-center gap-x-6 gap-y-1 py-4 first:pt-0">
              <div className="min-w-[180px]">
                <p className="text-sm font-medium text-stone-900">{entry.softwareName}</p>
              </div>
              <div className="flex flex-wrap items-center gap-x-5 gap-y-1 text-xs text-stone-500">
                <span title="Version">{entry.version}</span>
                <span title="OS">{entry.osNames}</span>
                <span className="max-w-[200px] truncate" title={entry.installCommand}>{entry.installCommand}</span>
              </div>
              <div className="ml-auto">
                <StatusBadge status={entry.status} />
              </div>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
