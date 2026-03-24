import { useState, type FormEvent } from 'react';
import { getErrorMessage } from '../lib/api';
import { useAuth } from '../contexts/AuthContext';
import type { UpdateCurrentUserEmailRequest } from '../types/auth';

interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

const initialPayload: ChangePasswordPayload = {
  currentPassword: '',
  newPassword: '',
};

export function SecurityPage() {
  const { apiFetch, user, refreshCurrentUser } = useAuth();
  const [payload, setPayload] = useState<ChangePasswordPayload>(initialPayload);
  const [confirmPassword, setConfirmPassword] = useState('');
  const [saving, setSaving] = useState(false);
  const [passwordError, setPasswordError] = useState('');
  const [passwordSuccess, setPasswordSuccess] = useState('');
  const [newEmail, setNewEmail] = useState('');
  const [currentEmailPassword, setCurrentEmailPassword] = useState('');
  const [emailSaving, setEmailSaving] = useState(false);
  const [emailError, setEmailError] = useState('');
  const [emailSuccess, setEmailSuccess] = useState('');

  const canManageEmail = user?.role === 'admin';

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setPasswordError('');
    setPasswordSuccess('');

    if (payload.newPassword !== confirmPassword) {
      setPasswordError('La confirmation du nouveau mot de passe ne correspond pas.');
      return;
    }

    if (payload.newPassword.length < 8) {
      setPasswordError('Le nouveau mot de passe doit contenir au moins 8 caractères.');
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
      setPasswordSuccess('Mot de passe mis à jour avec succès.');
    } catch (submitError) {
      setPasswordError(getErrorMessage(submitError, 'Impossible de changer le mot de passe.'));
    } finally {
      setSaving(false);
    }
  }

  async function handleEmailSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setEmailError('');
    setEmailSuccess('');

    const normalizedNewEmail = newEmail.trim().toLowerCase();
    const normalizedCurrentEmail = (user?.email ?? '').trim().toLowerCase();

    if (!normalizedNewEmail) {
      setEmailError('Le nouveau courriel est requis.');
      return;
    }

    const isValidEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(normalizedNewEmail);
    if (!isValidEmail) {
      setEmailError('Veuillez saisir un courriel valide.');
      return;
    }

    if (normalizedNewEmail === normalizedCurrentEmail) {
      setEmailError('Le nouveau courriel doit être différent du courriel actuel.');
      return;
    }

    if (!currentEmailPassword.trim()) {
      setEmailError('Le mot de passe actuel est requis.');
      return;
    }

    setEmailSaving(true);

    try {
      const body: UpdateCurrentUserEmailRequest = {
        newEmail: normalizedNewEmail,
        currentPassword: currentEmailPassword,
      };

      await apiFetch('/users/me/email', {
        method: 'PUT',
        body: JSON.stringify(body),
      });

      const didRefreshProfile = await refreshCurrentUser();

      if (!didRefreshProfile) {
        throw new Error('Votre profil n\'a pas pu être rafraîchi.');
      }

      setCurrentEmailPassword('');
      setNewEmail('');
      setEmailSuccess('Courriel mis à jour avec succès.');
    } catch (submitError) {
      setEmailError(getErrorMessage(submitError, 'Impossible de changer le courriel.'));
    } finally {
      setEmailSaving(false);
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
        {passwordError ? (
          <div className="mb-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {passwordError}
          </div>
        ) : null}

        {passwordSuccess ? (
          <div className="mb-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            {passwordSuccess}
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

      {canManageEmail ? (
        <section className="surface-card p-6 sm:p-8">
          <h2 className="text-xl font-semibold text-stone-950">Changer le courriel</h2>
          <p className="mt-2 text-sm leading-7 text-stone-600 sm:text-base">
            Mettez à jour l&apos;adresse courriel associée à votre compte administrateur.
          </p>

          {emailError ? (
            <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
              {emailError}
            </div>
          ) : null}

          {emailSuccess ? (
            <div className="mt-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
              {emailSuccess}
            </div>
          ) : null}

          <form className="mt-6 grid gap-5 sm:max-w-xl" onSubmit={handleEmailSubmit}>
            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Courriel actuel</span>
              <input
                type="email"
                value={user?.email ?? ''}
                className="input-field"
                readOnly
              />
            </label>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Nouveau courriel</span>
              <input
                type="email"
                value={newEmail}
                onChange={(event) => setNewEmail(event.target.value)}
                className="input-field"
                required
              />
            </label>

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-stone-700">Mot de passe actuel</span>
              <input
                type="password"
                value={currentEmailPassword}
                onChange={(event) => setCurrentEmailPassword(event.target.value)}
                className="input-field"
                minLength={8}
                required
              />
            </label>

            <button type="submit" className="primary-button w-full sm:w-auto" disabled={emailSaving}>
              {emailSaving ? 'Mise à jour...' : 'Mettre à jour le courriel'}
            </button>
          </form>
        </section>
      ) : null}
    </div>
  );
}
