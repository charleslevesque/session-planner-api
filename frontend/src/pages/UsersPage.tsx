import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import {
  ALL_ROLES,
  type CreateUserRequest,
  type RoleName,
  type UpdateUserPasswordRequest,
  type UserResponse,
} from '../types/users';

const initialForm: CreateUserRequest = {
  username: '',
  password: '',
  roleName: 'teacher',
};

function RoleBadge({ role }: { role: string }) {
  const colors: Record<string, string> = {
    admin: 'bg-rose-100 text-rose-700 border-rose-200',
    planner: 'bg-violet-100 text-violet-700 border-violet-200',
    technician: 'bg-blue-100 text-blue-700 border-blue-200',
    teacher: 'bg-amber-100 text-amber-700 border-amber-200',
    management: 'bg-emerald-100 text-emerald-700 border-emerald-200',
  };

  const cls = colors[role] ?? 'bg-stone-100 text-stone-600 border-stone-200';

  return (
    <span className={`inline-flex items-center rounded-xl border px-2.5 py-0.5 text-xs font-medium ${cls}`}>
      {role}
    </span>
  );
}

export function UsersPage() {
  const { apiFetch } = useAuth();
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState('');

  const [roleEdits, setRoleEdits] = useState<Record<number, string>>({});
  const [roleUpdating, setRoleUpdating] = useState<Record<number, boolean>>({});
  const [passwordEdits, setPasswordEdits] = useState<Record<number, string>>({});
  const [passwordUpdating, setPasswordUpdating] = useState<Record<number, boolean>>({});

  const [deleteConfirm, setDeleteConfirm] = useState<number | null>(null);
  const [deleting, setDeleting] = useState<Record<number, boolean>>({});

  const [form, setForm] = useState<CreateUserRequest>(initialForm);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState('');
  const [createSuccess, setCreateSuccess] = useState('');

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setPageError('');
    try {
      const data = await apiFetch<UserResponse[]>('/users');
      setUsers(data);
      const edits: Record<number, string> = {};
      for (const u of data) {
        edits[u.id] = u.roles;
      }
      setRoleEdits(edits);
    } catch (err) {
      setPageError(getErrorMessage(err, 'Impossible de charger les utilisateurs.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch]);

  useEffect(() => {
    void loadUsers();
  }, [loadUsers]);

  async function handleRoleUpdate(userId: number) {
    const roleName = roleEdits[userId];
    setRoleUpdating((prev) => ({ ...prev, [userId]: true }));
    try {
      await apiFetch(`/users/${userId}/role`, {
        method: 'PUT',
        body: JSON.stringify({ roleName }),
      });
      setUsers((prev) =>
        prev.map((u) => (u.id === userId ? { ...u, roles: roleName } : u)),
      );
    } catch (err) {
      setPageError(getErrorMessage(err, 'Impossible de modifier le rôle.'));
    } finally {
      setRoleUpdating((prev) => ({ ...prev, [userId]: false }));
    }
  }

  async function handleDelete(userId: number) {
    setDeleting((prev) => ({ ...prev, [userId]: true }));
    try {
      await apiFetch(`/users/${userId}`, { method: 'DELETE' });
      setUsers((prev) => prev.filter((u) => u.id !== userId));
      setDeleteConfirm(null);
    } catch (err) {
      setPageError(getErrorMessage(err, 'Impossible de désactiver l\'utilisateur.'));
    } finally {
      setDeleting((prev) => ({ ...prev, [userId]: false }));
    }
  }

  async function handlePasswordReset(userId: number) {
    const newPassword = (passwordEdits[userId] ?? '').trim();
    if (newPassword.length < 8) {
      setPageError('Le mot de passe temporaire doit contenir au moins 8 caractères.');
      return;
    }

    setPasswordUpdating((prev) => ({ ...prev, [userId]: true }));
    setPageError('');

    try {
      const body: UpdateUserPasswordRequest = { newPassword };
      await apiFetch(`/users/${userId}/password`, {
        method: 'PUT',
        body: JSON.stringify(body),
      });
      setPasswordEdits((prev) => ({ ...prev, [userId]: '' }));
    } catch (err) {
      setPageError(getErrorMessage(err, 'Impossible de réinitialiser le mot de passe.'));
    } finally {
      setPasswordUpdating((prev) => ({ ...prev, [userId]: false }));
    }
  }

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setCreating(true);
    setCreateError('');
    setCreateSuccess('');
    try {
      await apiFetch<UserResponse>('/users', {
        method: 'POST',
        body: JSON.stringify(form),
      });
      setCreateSuccess(`Compte "${form.username}" créé avec le rôle ${form.roleName}.`);
      setForm(initialForm);
      await loadUsers();
    } catch (err) {
      setCreateError(getErrorMessage(err, 'Impossible de créer l\'utilisateur.'));
    } finally {
      setCreating(false);
    }
  }

  return (
    <div className="space-y-8">
      {/* Header */}
      <section className="rounded-[2rem] bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.28),_transparent_32%),linear-gradient(135deg,_#20150d_0%,_#3f2b1d_45%,_#8a5a31_100%)] px-6 py-8 text-white sm:px-8">
        <p className="text-xs uppercase tracking-[0.35em] text-amber-100/80">Administration</p>
        <h1 className="mt-4 text-3xl font-semibold sm:text-4xl">Gestion des utilisateurs</h1>
        <p className="mt-3 text-sm leading-7 text-amber-50/75">
          Consultez, modifiez les rôles et désactivez les comptes. Créez de nouveaux utilisateurs avec le rôle de votre choix.
        </p>
      </section>

      {pageError ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
          {pageError}
        </div>
      ) : null}

      {/* Users table */}
      <section className="surface-card overflow-hidden p-0">
        <div className="flex items-center justify-between border-b border-stone-200 px-6 py-4">
          <h2 className="text-base font-semibold text-stone-950">Utilisateurs actifs</h2>
          <button
            type="button"
            onClick={() => void loadUsers()}
            className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
          >
            Rafraîchir
          </button>
        </div>

        {loading ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
        ) : users.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Aucun utilisateur trouvé.</div>
        ) : (
          <div>
            <table className="w-full table-auto text-sm">
              <thead>
                <tr className="border-b border-stone-100 bg-stone-50/70 text-xs uppercase tracking-[0.15em] text-stone-500">
                  <th className="w-[24%] px-6 py-4 text-left font-medium">Email</th>
                  <th className="w-[12%] px-6 py-4 text-left font-medium">Rôle</th>
                  <th className="w-[20%] px-6 py-4 text-left font-medium">Changer le rôle</th>
                  <th className="w-[30%] px-6 py-4 text-left font-medium">Mot de passe temp.</th>
                  <th className="w-[14%] px-6 py-4 text-left font-medium">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-stone-100">
                {users.map((user) => {
                  const pendingRole = roleEdits[user.id] ?? user.roles;
                  const roleChanged = pendingRole !== user.roles;
                  const isUpdating = roleUpdating[user.id] ?? false;
                  const isPasswordUpdating = passwordUpdating[user.id] ?? false;
                  const pendingPassword = passwordEdits[user.id] ?? '';
                  const isDeleting = deleting[user.id] ?? false;
                  const isProtectedAdmin = user.username.toLowerCase() === 'admin@local.dev';

                  return (
                    <tr key={user.id} className="transition hover:bg-stone-50/60">
                      <td className="truncate px-6 py-5 font-medium text-stone-950">{user.username}</td>
                      <td className="px-6 py-5">
                        <RoleBadge role={user.roles} />
                      </td>
                      <td className="px-6 py-5">
                        <div className="flex items-center gap-2">
                          <select
                            value={pendingRole}
                            onChange={(e) =>
                              setRoleEdits((prev) => ({ ...prev, [user.id]: e.target.value }))
                            }
                            className="rounded-xl border border-stone-200 bg-white px-3 py-1.5 text-sm text-stone-700 outline-none focus:border-amber-400 focus:ring-2 focus:ring-amber-100"
                          >
                            {ALL_ROLES.map((r) => (
                              <option key={r} value={r}>{r}</option>
                            ))}
                          </select>
                          {roleChanged && (
                            <button
                              type="button"
                              onClick={() => void handleRoleUpdate(user.id)}
                              disabled={isUpdating}
                              className="rounded-xl bg-stone-950 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-stone-700 disabled:opacity-50"
                            >
                              {isUpdating ? '...' : 'Sauvegarder'}
                            </button>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-5">
                        <div className="flex items-center gap-2">
                          <input
                            type="password"
                            value={pendingPassword}
                            onChange={(event) =>
                              setPasswordEdits((prev) => ({ ...prev, [user.id]: event.target.value }))
                            }
                            className="input-field min-w-0"
                            placeholder="Nouveau mot de passe"
                            minLength={8}
                          />
                          <button
                            type="button"
                            onClick={() => void handlePasswordReset(user.id)}
                            disabled={isPasswordUpdating || pendingPassword.trim().length < 8}
                            className="rounded-xl bg-amber-600 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-amber-700 disabled:opacity-50"
                          >
                            {isPasswordUpdating ? '...' : 'Reset'}
                          </button>
                        </div>
                      </td>
                      <td className="px-6 py-5">
                        {isProtectedAdmin ? (
                          <span className="text-xs text-stone-400">Compte protégé</span>
                        ) : deleteConfirm === user.id ? (
                          <div className="flex items-center gap-2">
                            <span className="text-xs text-stone-600">Confirmer ?</span>
                            <button
                              type="button"
                              onClick={() => void handleDelete(user.id)}
                              disabled={isDeleting}
                              className="rounded-xl bg-rose-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-rose-700 disabled:opacity-50"
                            >
                              {isDeleting ? '...' : 'Oui'}
                            </button>
                            <button
                              type="button"
                              onClick={() => setDeleteConfirm(null)}
                              className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 hover:bg-stone-50"
                            >
                              Non
                            </button>
                          </div>
                        ) : (
                          <button
                            type="button"
                            onClick={() => setDeleteConfirm(user.id)}
                            className="rounded-xl border border-rose-200 px-3 py-1.5 text-xs text-rose-600 transition hover:bg-rose-50"
                          >
                            Désactiver
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {/* Create user form */}
      <section className="surface-card p-6 sm:p-8">
        <h2 className="text-base font-semibold text-stone-950">Créer un compte</h2>
        <p className="mt-1 text-sm text-stone-600">
          L&apos;email doit être unique. Le mot de passe doit faire au moins 8 caractères.
        </p>

        {createError ? (
          <div className="mt-4 rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {createError}
          </div>
        ) : null}

        {createSuccess ? (
          <div className="mt-4 rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
            {createSuccess}
          </div>
        ) : null}

        <form className="mt-6 grid gap-5 sm:grid-cols-2 lg:grid-cols-4" onSubmit={handleCreate}>
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-stone-700">Email</span>
            <input
              type="email"
              value={form.username}
              onChange={(e) => setForm((prev) => ({ ...prev, username: e.target.value }))}
              className="input-field"
              placeholder="nom@organisation.com"
              required
            />
          </label>

          <label className="block">
            <span className="mb-2 block text-sm font-medium text-stone-700">Mot de passe</span>
            <input
              type="password"
              value={form.password}
              onChange={(e) => setForm((prev) => ({ ...prev, password: e.target.value }))}
              className="input-field"
              placeholder="Min. 8 caractères"
              minLength={8}
              required
            />
          </label>

          <label className="block">
            <span className="mb-2 block text-sm font-medium text-stone-700">Rôle</span>
            <select
              value={form.roleName}
              onChange={(e) => setForm((prev) => ({ ...prev, roleName: e.target.value as RoleName }))}
              className="input-field"
            >
              {ALL_ROLES.map((r) => (
                <option key={r} value={r}>{r}</option>
              ))}
            </select>
          </label>

          <div className="flex items-end">
            <button type="submit" className="primary-button w-full" disabled={creating}>
              {creating ? 'Création...' : 'Créer'}
            </button>
          </div>
        </form>
      </section>
    </div>
  );
}
