import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ROLE_LABELS, type RoleName } from '../types/users';
import { Sidebar } from './Sidebar';

function formatRole(role: string) {
  if (!role) {
    return 'Utilisateur';
  }

  return ROLE_LABELS[role as RoleName] ?? role;
}

export function AppLayout() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const { logout, user } = useAuth();

  return (
    <div className="min-h-screen">
      <div className="mx-auto flex min-h-screen max-w-7xl gap-6 px-4 py-4 sm:px-6 lg:px-8">
        <aside className="hidden w-80 shrink-0 rounded-[2rem] border border-white/60 bg-white/65 p-6 shadow-[0_20px_60px_rgba(120,88,38,0.12)] backdrop-blur xl:block">
          <Sidebar />
        </aside>

        {mobileOpen ? (
          <button
            type="button"
            className="fixed inset-0 z-30 bg-stone-950/30 xl:hidden"
            aria-label="Fermer le menu"
            onClick={() => setMobileOpen(false)}
          />
        ) : null}

        <aside
          className={[
            'fixed inset-y-0 left-0 z-40 w-80 max-w-[85vw] border-r border-white/60 bg-[var(--ets-bg-warm)] p-6 shadow-[0_20px_60px_rgba(65,63,73,0.15)] transition-transform xl:hidden',
            mobileOpen ? 'translate-x-0' : '-translate-x-full',
          ].join(' ')}
        >
          <Sidebar onNavigate={() => setMobileOpen(false)} />
        </aside>

        <div className="flex min-w-0 flex-1 flex-col gap-6">
          <header className="flex flex-col gap-4 rounded-[2rem] border border-white/60 bg-white/70 px-5 py-4 shadow-[0_20px_50px_rgba(120,88,38,0.1)] backdrop-blur sm:px-6 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex items-center gap-3">
              <button
                type="button"
                className="inline-flex h-11 w-11 items-center justify-center rounded-2xl border border-stone-200 bg-white text-stone-900 xl:hidden"
                onClick={() => setMobileOpen((current) => !current)}
                aria-label="Ouvrir le menu"
              >
                <span className="text-lg">≡</span>
              </button>

              <div>
                <p className="text-xs uppercase tracking-[0.35em] text-[var(--ets-primary)]/80">ÉTS · Planificateur de sessions</p>
                <h2 className="mt-1 text-2xl font-semibold text-stone-950">Pilotage des sessions</h2>
              </div>
            </div>

            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="rounded-2xl bg-[var(--ets-primary)] px-4 py-3 text-white shadow-lg shadow-[rgba(220,4,44,0.25)]">
                <p className="text-sm font-medium">{user?.name ?? 'Utilisateur connecte'}</p>
                <p className="mt-1 text-sm text-white/70">{formatRole(user?.role ?? '')}</p>
              </div>

              <button
                type="button"
                className="inline-flex items-center justify-center rounded-2xl border-2 border-[var(--ets-primary)] px-4 py-3 text-sm font-medium text-[var(--ets-primary)] transition hover:bg-[var(--ets-primary)] hover:text-white"
                onClick={() => {
                  setMobileOpen(false);
                  void logout();
                }}
              >
                Déconnexion
              </button>
            </div>
          </header>

          <main className="flex-1 rounded-[2rem] border border-white/60 bg-white/70 p-5 shadow-[0_20px_50px_rgba(120,88,38,0.08)] backdrop-blur sm:p-6">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  );
}
