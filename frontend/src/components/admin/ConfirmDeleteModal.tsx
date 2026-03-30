import { useState } from 'react';
import { createPortal } from 'react-dom';

interface ConfirmDeleteModalProps {
  open: boolean;
  itemLabel: string;
  expectedText: string;
  saving: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmDeleteModal({
  open,
  itemLabel,
  expectedText,
  saving,
  onConfirm,
  onCancel,
}: ConfirmDeleteModalProps) {
  const [typed, setTyped] = useState('');

  if (!open) return null;

  const canConfirm = typed === expectedText;

  function handleCancel() {
    setTyped('');
    onCancel();
  }

  return createPortal(
    <div
      className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/50 px-4"
      onClick={handleCancel}
    >
      <div
        className="w-full max-w-md rounded-2xl border border-stone-200 bg-white p-6 shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-stone-950">Confirmer la suppression</h2>
          <button
            type="button"
            onClick={handleCancel}
            className="rounded-lg p-1 text-stone-400 transition hover:bg-stone-100 hover:text-stone-600"
            aria-label="Fermer"
          >
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <div className="mt-3 rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          Cette action est <strong>irréversible</strong>. L&apos;élément{' '}
          <strong>{itemLabel}</strong> sera définitivement supprimé.
        </div>

        <p className="mt-4 text-sm text-stone-700">
          Pour confirmer, saisissez exactement&nbsp;:
        </p>
        <p className="mt-1 select-all rounded-lg bg-stone-100 px-3 py-1.5 font-mono text-sm text-stone-900">
          {expectedText}
        </p>

        <input
          type="text"
          value={typed}
          onChange={(e) => setTyped(e.target.value)}
          className="input-field mt-3"
          placeholder="Saisissez le texte ci-dessus"
          autoFocus
        />

        <div className="mt-5 flex justify-end gap-2">
          <button
            type="button"
            onClick={handleCancel}
            className="rounded-xl border border-stone-200 px-4 py-2 text-sm text-stone-700 transition hover:bg-stone-50"
          >
            Annuler
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={!canConfirm || saving}
            className="rounded-xl bg-rose-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-rose-700 disabled:opacity-50"
          >
            {saving ? 'Suppression...' : 'Supprimer définitivement'}
          </button>
        </div>
      </div>
    </div>,
    document.body,
  );
}
