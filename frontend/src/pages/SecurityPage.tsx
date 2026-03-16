import { useState, type FormEvent } from 'react';
import { getErrorMessage } from '../lib/api';
import { useAuth } from '../contexts/AuthContext';

interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

const initialPayload: ChangePasswordPayload = {
  currentPassword: '',
  newPassword: '',
};

export function SecurityPage() {
  const { apiFetch } = useAuth();
  const [payload, setPayload] = useState<ChangePasswordPayload>(initialPayload);
  const [confirmPassword, setConfirmPassword] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setSuccess('');

    if (payload.newPassword !== confirmPassword) {
      setError('La confirmation du nouveau mot de passe ne correspond pas.');
      return;
    }

    if (payload.newPassword.length < 8) {
      setError('Le nouveau mot de passe doit contenir au moins 8 caractères.');
      return;
    }

    setSaving(true);

    try {
      await apiFetch('/auth/change-password', {
        method: 'POST',
        body: JSON.stringify(payload),
      });

      setPayload(initialPayload);
      setConfirmPassword('');
      setSuccess('Mot de passe mis à jour avec succès.');
    } catch (submitError) {
      setError(getErrorMessage(submitError, 'Impossible de changer le mot de passe.'));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="space-y-8">
      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Mon compte</p>
        <h1 className="mt-3 text-3xl font-semibold text-stone-950">Sécurité du compte</h1>
        <p className="mt-3 max-w-2xl text-sm leading-7 text-stone-600 sm:text-base">
          Changez votre mot de passe temporaire après votre première connexion.
        </p>
      </section>

      <section className="surface-card p-6 sm:p-8">
        {error ? (
          <div className="mb-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        ) : null}

        {success ? (
          <div className="mb-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            {success}
          </div>
        ) : null}

        <form className="grid gap-5 sm:max-w-xl" onSubmit={handleSubmit}>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-stone-700">Mot de passe actuel</span>
            <input
              type="password"
              value={payload.currentPassword}
              onChange={(event) =>
                setPayload((current) => ({ ...current, currentPassword: event.target.value }))
              }
              className="input-field"
              minLength={8}
              required
            />
          </label>

          <label className="block">
            <span className="mb-2 block text-sm font-medium text-stone-700">Nouveau mot de passe</span>
            <input
              type="password"
              value={payload.newPassword}
              onChange={(event) =>
                setPayload((current) => ({ ...current, newPassword: event.target.value }))
              }
              className="input-field"
              minLength={8}
              required
            />
          </label>

          <label className="block">
            <span className="mb-2 block text-sm font-medium text-stone-700">Confirmer le nouveau mot de passe</span>
            <input
              type="password"
              value={confirmPassword}
              onChange={(event) => setConfirmPassword(event.target.value)}
              className="input-field"
              minLength={8}
              required
            />
          </label>

          <button type="submit" className="primary-button w-full sm:w-auto" disabled={saving}>
            {saving ? 'Mise à jour...' : 'Mettre à jour le mot de passe'}
          </button>
        </form>
      </section>
    </div>
  );
}
