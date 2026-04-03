import { RESOURCE_TABS, RESOURCE_TAB_LABELS, type ResourceTab } from '../types/courseResources';

interface ResourceTabsProps {
  activeTab: ResourceTab;
  onTabChange: (tab: ResourceTab) => void;
  counts: Record<ResourceTab, number>;
}

export function ResourceTabs({ activeTab, onTabChange, counts }: ResourceTabsProps) {
  return (
    <div className="flex flex-wrap gap-1.5">
      {RESOURCE_TABS.map((tab) => {
        const isActive = tab === activeTab;
        const count = counts[tab];

        return (
          <button
            key={tab}
            type="button"
            onClick={() => onTabChange(tab)}
            className={[
              'rounded-xl border px-3 py-1.5 text-xs font-medium transition',
              isActive
                ? 'border-[var(--ets-primary)]/40 bg-[rgba(220,4,44,0.08)] text-[var(--ets-primary)]'
                : 'border-stone-200 text-stone-600 hover:border-stone-300 hover:bg-stone-50',
            ].join(' ')}
          >
            {RESOURCE_TAB_LABELS[tab]}
            <span className={`ml-1.5 inline-flex min-w-[1.25rem] items-center justify-center rounded-md px-1 py-0.5 text-[10px] font-semibold ${
              isActive ? 'bg-[var(--ets-primary)] text-white' : 'bg-stone-100 text-stone-500'
            }`}>
              {count}
            </span>
          </button>
        );
      })}
    </div>
  );
}
