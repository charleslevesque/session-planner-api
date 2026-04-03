import { type FormEvent } from 'react';
import { createPortal } from 'react-dom';
import type { FieldDef } from '../../types/admin';
import { FieldRenderer } from '../FieldRenderer';

interface FormModalProps {
  open: boolean;
  title: string;
  fields: readonly FieldDef[];
  values: Record<string, string>;
  onChange: (name: string, value: string) => void;
  onSubmit: () => void;
  onClose: () => void;
  saving: boolean;
  error: string;
}

export function FormModal({
  open,
  title,
  fields,
  values,
  onChange,
  onSubmit,
  onClose,
  saving,
  error,
}: FormModalProps) {
  if (!open) return null;

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    onSubmit();
  }

  return createPortal(
    <div
      className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/50 px-4"
      onClick={onClose}
    >
      <div
        className="w-full max-w-lg max-h-[85vh] overflow-y-auto rounded-2xl border border-stone-200 bg-white p-6 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-stone-950">{title}</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-1 text-stone-400 transition hover:bg-stone-100 hover:text-stone-600"
            aria-label="Fermer"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {error ? (
          <div className="mt-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        ) : null}

        <form className="mt-4 space-y-4" onSubmit={handleSubmit}>
          {fields.map((field) => (
            <label key={field.name} className="block">
              <span className="mb-1.5 block text-sm font-medium text-stone-700">
                {field.label}
                {field.required ? <span className="ml-0.5 text-rose-500">*</span> : null}
              </span>
              <FieldRenderer field={field} value={values[field.name] ?? ''} onChange={onChange} />
            </label>
          ))}

          <div className="flex justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-xl border border-stone-200 px-4 py-2 text-sm text-stone-700 transition hover:bg-stone-50"
            >
              Annuler
            </button>
            <button
              type="submit"
              disabled={saving}
              className="primary-button"
            >
              {saving ? 'Enregistrement...' : 'Enregistrer'}
            </button>
          </div>
        </form>
      </div>
    </div>,
    document.body,
  );
}
