import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { PAGE_ACCESS, hasRoleAccess } from '../lib/access';

const navigation = [
  { to: '/dashboard', label: 'Dashboard', hint: 'Vue generale', allowedRoles: PAGE_ACCESS.dashboard },
  { to: '/sessions/manage', label: 'Sessions', hint: 'Gestion cycle de vie', allowedRoles: PAGE_ACCESS.sessionsManage },
  { to: '/besoins', label: 'Besoins', hint: 'Saisie et suivi', allowedRoles: PAGE_ACCESS.besoins },
  { to: '/matrice', label: 'Matrice', hint: 'Affectations', allowedRoles: PAGE_ACCESS.matrice },
  { to: '/compte/securite', label: 'Securite', hint: 'Changer mon mot de passe', allowedRoles: PAGE_ACCESS.security },
];

const adminNavigation = [
  { to: '/admin/users', label: 'Utilisateurs', hint: 'Gestion des comptes', allowedRoles: PAGE_ACCESS.users },
];

interface SidebarProps {
  onNavigate?: () => void;
}

function NavItem({ to, label, hint, onNavigate }: { to: string; label: string; hint: string; onNavigate?: () => void }) {
  return (
    <NavLink
      to={to}
      onClick={onNavigate}
      className={({ isActive }) =>
        [
          'group block rounded-2xl border px-4 py-3 transition',
          isActive
            ? 'border-[var(--ets-primary)]/40 bg-[rgba(220,4,44,0.08)] text-stone-900 shadow-lg shadow-[rgba(220,4,44,0.12)]'
            : 'border-stone-200 bg-white/75 text-stone-700 hover:border-[var(--ets-primary)]/30 hover:bg-[rgba(220,4,44,0.04)]',
        ].join(' ')
      }
    >
      <div className="text-base font-medium">{label}</div>
      <div className="mt-1 text-sm text-current/70">{hint}</div>
    </NavLink>
  );
}

export function Sidebar({ onNavigate }: SidebarProps) {
  const { user } = useAuth();
  const role = user?.role;
  const [logoError, setLogoError] = useState(false);

  const visibleNavigation = navigation.filter((item) => hasRoleAccess(role, item.allowedRoles));
  const visibleAdminNavigation = adminNavigation.filter((item) => hasRoleAccess(role, item.allowedRoles));

  return (
    <nav className="flex h-full flex-col gap-6">
      <div>
        <a href="https://www.etsmtl.ca/" target="_blank" rel="noopener noreferrer" className="block">
          {logoError ? (
            <span className="inline-flex items-center rounded-xl bg-[var(--ets-primary)] px-4 py-2.5 text-lg font-bold italic text-white">ÉTS</span>
          ) : (
            <img src="/ets-logo.png" alt="ÉTS - École de technologie supérieure" className="h-14 w-auto object-contain" onError={() => setLogoError(true)} />
          )}
        </a>
        <p className="mt-4 text-xs uppercase tracking-[0.35em] text-[var(--ets-primary)]/80">Planificateur de sessions</p>
        <h1 className="mt-2 text-xl font-semibold text-stone-950">Pilotage des sessions</h1>
        <p className="mt-2 text-sm leading-6 text-stone-600">
          Orchestrer besoins, ressources et arbitrages.
        </p>
      </div>

      <div className="space-y-2">
        {visibleNavigation.map((item) => (
          <NavItem key={item.to} {...item} onNavigate={onNavigate} />
        ))}
      </div>

      {visibleAdminNavigation.length > 0 ? (
        <div className="space-y-2">
          <p className="px-1 text-xs uppercase tracking-[0.3em] text-stone-400">Administration</p>
          {visibleAdminNavigation.map((item) => (
            <NavItem key={item.to} {...item} onNavigate={onNavigate} />
          ))}
        </div>
      ) : null}
    </nav>
  );
}
