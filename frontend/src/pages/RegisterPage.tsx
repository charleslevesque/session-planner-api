import { useState, type FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import type { RegisterRequest } from '../types/auth';

const initialForm: RegisterRequest = {
  email: '',
  password: '',
  firstName: '',
  lastName: '',
};

export function RegisterPage() {
  const navigate = useNavigate();
  const { register } = useAuth();
  const [form, setForm] = useState<RegisterRequest>(initialForm);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsSubmitting(true);
    setError('');

    try {
      await register(form);
      navigate('/login', {
        replace: true,
        state: {
          message: 'Compte créé. Vous pouvez maintenant vous connecter.',
        },
      });
    } catch (submitError) {
      setError(getErrorMessage(submitError, 'Impossible de créer le compte.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="auth-shell px-4 py-10 sm:px-6 lg:px-8">
      <div className="mx-auto grid max-w-6xl overflow-hidden rounded-[2rem] border border-white/60 bg-white/70 shadow-[0_30px_80px_rgba(79,53,24,0.18)] backdrop-blur lg:grid-cols-[1.02fr_0.98fr]">
        <section className="px-6 py-10 sm:px-10 sm:py-12">
          <div className="mx-auto max-w-md">
            <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Inscription</p>
            <h1 className="mt-3 text-3xl font-semibold text-stone-950">Créer un compte enseignant</h1>
            <p className="mt-3 text-sm leading-7 text-stone-600">
              Inscrivez-vous pour accéder au planificateur de sessions et soumettre vos besoins technologiques.
            </p>

            {error ? (
              <div className="mt-6 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {error}
              </div>
            ) : null}

            <form className="mt-8 grid gap-5" onSubmit={handleSubmit}>
              <div className="grid gap-5 sm:grid-cols-2">
                <label className="block">
                  <span className="mb-2 block text-sm font-medium text-stone-700">Prénom</span>
                  <input
                    type="text"
                    value={form.firstName}
                    onChange={(event) => setForm((current) => ({ ...current, firstName: event.target.value }))}
                    className="input-field"
                    autoComplete="given-name"
                    maxLength={100}
                    required
                  />
                </label>

                <label className="block">
                  <span className="mb-2 block text-sm font-medium text-stone-700">Nom</span>
                  <input
                    type="text"
                    value={form.lastName}
                    onChange={(event) => setForm((current) => ({ ...current, lastName: event.target.value }))}
                    className="input-field"
                    autoComplete="family-name"
                    maxLength={100}
                    required
                  />
                </label>
              </div>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Courriel</span>
                <input
                  type="email"
                  value={form.email}
                  onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
                  className="input-field"
                  autoComplete="email"
                  required
                />
              </label>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Mot de passe</span>
                <input
                  type="password"
                  value={form.password}
                  onChange={(event) => setForm((current) => ({ ...current, password: event.target.value }))}
                  className="input-field"
                  autoComplete="new-password"
                  minLength={8}
                  required
                />
              </label>

              <button type="submit" className="primary-button w-full" disabled={isSubmitting}>
                {isSubmitting ? 'Création...' : 'Créer mon compte'}
              </button>
            </form>

            <p className="mt-6 text-sm text-stone-600">
              Déjà inscrit ?{' '}
              <Link to="/login" className="font-medium text-stone-950 underline decoration-[var(--ets-primary)] underline-offset-4 hover:text-[var(--ets-primary)]">
                Se connecter
              </Link>
            </p>
          </div>
        </section>

        <section className="auth-panel px-6 py-10 sm:px-10 sm:py-12">
          <div className="rounded-[2rem] border border-stone-900/10 bg-white/55 p-8 shadow-[0_20px_50px_rgba(54,36,17,0.08)] backdrop-blur-sm">
            <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Planificateur de sessions</p>
            <h2 className="mt-4 text-3xl font-semibold text-stone-950">Soumettez vos besoins technologiques.</h2>
            <div className="mt-6 space-y-4 text-sm leading-7 text-stone-600">
              <p>Déclarez vos logiciels, machines virtuelles et équipements pour chaque session d&apos;enseignement.</p>
              <p>Sauvegardez en brouillon, modifiez à tout moment, puis soumettez lorsque vous êtes prêt.</p>
              <p>L&apos;équipe technique traite vos demandes et vous tient informé du statut.</p>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
