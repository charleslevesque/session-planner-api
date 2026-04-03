import type { ReactNode } from 'react';

export interface Column<T> {
  key: string;
  label: string;
  render: (item: T) => ReactNode;
  className?: string;
}

interface ResourceTableProps<T> {
  data: T[];
  columns: Column<T>[];
  emptyMessage: string;
  keyExtractor: (item: T) => string | number;
}

export function ResourceTable<T>({ data, columns, emptyMessage, keyExtractor }: ResourceTableProps<T>) {
  if (data.length === 0) {
    return (
      <div className="px-4 py-8 text-center text-sm text-stone-500">
        {emptyMessage}
      </div>
    );
  }

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b border-stone-200">
            {columns.map((col) => (
              <th key={col.key} className={`px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-stone-500 ${col.className ?? ''}`}>
                {col.label}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((item) => (
            <tr key={keyExtractor(item)} className="border-b border-stone-100 last:border-0 hover:bg-stone-50/50 transition">
              {columns.map((col) => (
                <td key={col.key} className={`px-4 py-3 text-stone-700 ${col.className ?? ''}`}>
                  {col.render(item)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
