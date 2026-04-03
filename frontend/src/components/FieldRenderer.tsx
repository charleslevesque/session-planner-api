import type { FieldDef } from '../types/admin';

interface FieldRendererProps {
  field: FieldDef;
  value: string;
  onChange: (name: string, value: string) => void;
}

export function FieldRenderer({ field, value, onChange }: FieldRendererProps) {
  switch (field.type) {
    case 'textarea':
      return (
        <textarea
          value={value}
          onChange={(event) => onChange(field.name, event.target.value)}
          className="input-field min-h-[5rem] resize-y"
          placeholder={field.placeholder}
          required={field.required}
          rows={3}
        />
      );

    case 'select':
      if (field.multiple) {
        const selectedValues = value ? value.split(',').filter(Boolean) : [];
        const selectedSet = new Set(selectedValues);

        function toggleValue(nextValue: string) {
          const next = new Set(selectedSet);
          if (next.has(nextValue)) {
            next.delete(nextValue);
          } else {
            next.add(nextValue);
          }
          onChange(field.name, Array.from(next).join(','));
        }

        return (
          <div className="space-y-2 rounded-2xl border border-stone-200 bg-white p-2">
            <div className="max-h-44 space-y-1 overflow-y-auto">
              {field.options?.map((option) => {
                const selected = selectedSet.has(option.value);
                return (
                  <button
                    key={option.value}
                    type="button"
                    onClick={() => toggleValue(option.value)}
                    className={[
                      'flex w-full items-center justify-between rounded-xl px-3 py-2 text-left text-sm transition',
                      selected
                        ? 'bg-[rgba(220,4,44,0.08)] text-[var(--ets-primary)]'
                        : 'text-stone-700 hover:bg-stone-50',
                    ].join(' ')}
                  >
                    <span>{option.label}</span>
                    <span
                      className={[
                        'inline-flex h-5 w-5 items-center justify-center rounded border text-xs font-semibold',
                        selected
                          ? 'border-[var(--ets-primary)] bg-[var(--ets-primary)] text-white'
                          : 'border-stone-300 bg-white text-transparent',
                      ].join(' ')}
                    >
                      ✓
                    </span>
                  </button>
                );
              })}
            </div>
          </div>
        );
      }

      return (
        <select
          value={value}
          onChange={(event) => onChange(field.name, event.target.value)}
          className="input-field"
          required={field.required}
        >
          <option value="">— Sélectionner —</option>
          {field.options?.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      );

    case 'number':
      return (
        <input
          type="number"
          value={value}
          onChange={(event) => onChange(field.name, event.target.value)}
          className="input-field"
          placeholder={field.placeholder}
          required={field.required}
          min={field.min}
        />
      );

    default:
      return (
        <>
          <input
            type="text"
            value={value}
            onChange={(event) => onChange(field.name, event.target.value)}
            className="input-field"
            placeholder={field.placeholder}
            required={field.required}
            list={field.suggestions?.length ? `${field.name}-suggestions` : undefined}
          />
          {field.suggestions?.length ? (
            <datalist id={`${field.name}-suggestions`}>
              {field.suggestions.map((suggestion) => (
                <option key={suggestion} value={suggestion} />
              ))}
            </datalist>
          ) : null}
        </>
      );
  }
}