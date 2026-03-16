import { NavLink } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { PAGE_ACCESS, hasRoleAccess } from '../lib/access';

const navigation = [
  { to: '/dashboard', label: 'Dashboard', hint: 'Vue generale', allowedRoles: PAGE_ACCESS.dashboard },
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
            ? 'border-amber-300 bg-amber-100/90 text-stone-900 shadow-lg shadow-amber-200/40'
            : 'border-stone-200 bg-white/75 text-stone-700 hover:border-amber-300 hover:bg-amber-50/80',
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

  const visibleNavigation = navigation.filter((item) => hasRoleAccess(role, item.allowedRoles));
  const visibleAdminNavigation = adminNavigation.filter((item) => hasRoleAccess(role, item.allowedRoles));

  return (
    <nav className="flex h-full flex-col gap-6">
      <div>
        <p className="text-xs uppercase tracking-[0.35em] text-amber-700/70">Session Planner</p>
        <h1 className="mt-3 text-2xl font-semibold text-stone-950">Pilotage des sessions</h1>
        <p className="mt-2 text-sm leading-6 text-stone-600">
          Une base de travail pour orchestrer besoins, ressources et arbitrages.
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
