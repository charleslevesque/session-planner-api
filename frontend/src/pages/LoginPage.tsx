import { useState, type FormEvent } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { getErrorMessage } from '../lib/api';
import { useAuth } from '../contexts/AuthContext';
import type { LoginRequest } from '../types/auth';

interface LoginLocationState {
  from?: {
    pathname?: string;
  };
  message?: string;
}

const initialForm: LoginRequest = {
  email: '',
  password: '',
};

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isBusy } = useAuth();
  const [form, setForm] = useState<LoginRequest>(initialForm);
  const [error, setError] = useState('');

  const state = (location.state ?? null) as LoginLocationState | null;
  const nextPath = state?.from?.pathname ?? '/dashboard';

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');

    try {
      await login(form);
      navigate(nextPath, { replace: true });
    } catch (submitError) {
      setError(getErrorMessage(submitError, 'Connexion impossible.'));
    }
  }

  return (
    <div className="auth-shell px-4 py-10 sm:px-6 lg:px-8">
      <div className="mx-auto grid max-w-6xl overflow-hidden rounded-[2rem] border border-white/60 bg-white/70 shadow-[0_30px_80px_rgba(79,53,24,0.18)] backdrop-blur lg:grid-cols-[0.95fr_1.05fr]">
        <section className="auth-hero px-6 py-10 text-white sm:px-10 sm:py-12">
          <p className="text-xs uppercase tracking-[0.35em] text-amber-100/75">Session Planner</p>
          <h1 className="mt-6 max-w-lg text-4xl font-semibold sm:text-5xl">
            Authentification du portail de planification.
          </h1>
          <p className="mt-6 max-w-xl text-sm leading-7 text-amber-50/80 sm:text-base">
            Ce premier lot connecte le frontend a l&apos;API et pose les fondations pour le dashboard, les besoins et la matrice.
          </p>
        </section>

        <section className="px-6 py-10 sm:px-10 sm:py-12">
          <div className="mx-auto max-w-md">
            <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Login</p>
            <h2 className="mt-3 text-3xl font-semibold text-stone-950">Connectez-vous</h2>
            <p className="mt-3 text-sm leading-7 text-stone-600">
              Utilisez votre compte existant pour acceder au socle du planner.
            </p>

            {state?.message ? (
              <div className="mt-6 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
                {state.message}
              </div>
            ) : null}

            {error ? (
              <div className="mt-6 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {error}
              </div>
            ) : null}

            <form className="mt-8 space-y-5" onSubmit={handleSubmit}>
              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Email</span>
                <input
                  type="email"
                  value={form.email}
                  onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
                  className="input-field"
                  placeholder="nom@organisation.com"
                  autoComplete="email"
                  required
                />
              </label>

              <label className="block">
                <span className="mb-2 block text-sm font-medium text-stone-700">Password</span>
                <input
                  type="password"
                  value={form.password}
                  onChange={(event) => setForm((current) => ({ ...current, password: event.target.value }))}
                  className="input-field"
                  placeholder="Minimum 8 caracteres"
                  autoComplete="current-password"
                  required
                  minLength={8}
                />
              </label>

              <button type="submit" className="primary-button w-full" disabled={isBusy}>
                {isBusy ? 'Connexion...' : 'Se connecter'}
              </button>
            </form>

            <p className="mt-6 text-sm text-stone-600">
              Pas encore de compte ?{' '}
              <Link to="/register" className="font-medium text-stone-950 underline decoration-amber-500 underline-offset-4">
                Creer un compte
              </Link>
            </p>
          </div>
        </section>
      </div>
    </div>
  );
}
