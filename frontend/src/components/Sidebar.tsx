import { NavLink } from 'react-router-dom';

const navigation = [
  { to: '/dashboard', label: 'Dashboard', hint: 'Vue generale' },
  { to: '/besoins', label: 'Besoins', hint: 'Saisie et suivi' },
  { to: '/matrice', label: 'Matrice', hint: 'Affectations' },
];

interface SidebarProps {
  onNavigate?: () => void;
}

export function Sidebar({ onNavigate }: SidebarProps) {
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
        {navigation.map((item) => (
          <NavLink
            key={item.to}
            to={item.to}
            onClick={onNavigate}
            className={({ isActive }) =>
              [
                'group block rounded-2xl border px-4 py-3 transition',
                isActive
                  ? 'border-stone-950 bg-stone-950 text-white shadow-lg shadow-stone-900/10'
                  : 'border-stone-200 bg-white/75 text-stone-700 hover:border-amber-300 hover:bg-amber-50/80',
              ].join(' ')
            }
          >
            <div className="text-base font-medium">{item.label}</div>
            <div className="mt-1 text-sm text-current/70">{item.hint}</div>
          </NavLink>
        ))}
      </div>
    </nav>
  );
}
