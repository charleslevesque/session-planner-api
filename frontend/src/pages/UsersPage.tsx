import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { getErrorMessage } from '../lib/api';
import {
  ALL_ROLES,
  ROLE_LABELS,
  type CreateUserRequest,
  type RoleName,
  type UpdateUserPasswordRequest,
  type UserResponse,
} from '../types/users';

/* ── Types for activity endpoint ── */

interface UserTeachingNeedItemDetail {
  id: number;
  itemType: string;
  softwareName: string | null;
  versionNumber: string | null;
  osName: string | null;
  quantity: number | null;
  description: string | null;
  notes: string | null;
}

interface UserTeachingNeedDetail {
  id: number;
  courseName: string;
  sessionName: string;
  status: string;
  createdAt: string;
  submittedAt: string | null;
  reviewedAt: string | null;
  rejectionReason: string | null;
  notes: string | null;
  expectedStudents: number | null;
  desiredModifications: string | null;
  additionalComments: string | null;
  isFastTrack: boolean;
  items: UserTeachingNeedItemDetail[];
}

interface UserActivityResponse {
  userId: number;
  username: string;
  fullName: string | null;
  role: string;
  isActive: boolean;
  teachingNeeds: UserTeachingNeedDetail[];
}

/* ── Constants ── */

const STATUS_LABELS: Record<string, string> = {
  Draft: 'Brouillon',
  Submitted: 'Soumis',
  Approved: 'Approuvé',
  Rejected: 'Rejeté',
};

const STATUS_COLORS: Record<string, string> = {
  Approved: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  Rejected: 'bg-rose-50 text-rose-700 border-rose-200',
  Submitted: 'bg-blue-50 text-blue-700 border-blue-200',
  Draft: 'bg-stone-50 text-stone-600 border-stone-200',
};

const ITEM_TYPE_LABELS: Record<string, string> = {
  software: 'Logiciel',
  hardware: 'Matériel',
  other: 'Autre',
};

const initialForm: CreateUserRequest = {
  username: '',
  password: '',
  roleName: 'professor',
};

/* ── Small components ── */

function RoleBadge({ role }: { role: string }) {
  const colors: Record<string, string> = {
    admin: 'bg-rose-100 text-rose-700 border-rose-200',
    professor: 'bg-amber-100 text-amber-700 border-amber-200',
    lab_instructor: 'bg-blue-100 text-blue-700 border-blue-200',
    course_instructor: 'bg-violet-100 text-violet-700 border-violet-200',
  };

  const label = ROLE_LABELS[role as RoleName] ?? role;
  const cls = colors[role] ?? 'bg-stone-100 text-stone-600 border-stone-200';

  return (
    <span className={`inline-flex items-center rounded-xl border px-2.5 py-0.5 text-xs font-medium ${cls}`}>
      {label}
    </span>
  );
}

function StatusBadge({ isActive }: { isActive: boolean }) {
  if (isActive) {
    return (
      <span className="inline-flex items-center rounded-xl border border-emerald-200 bg-emerald-50 px-2.5 py-0.5 text-xs font-medium text-emerald-700">
        Actif
      </span>
    );
  }
  return (
    <span className="inline-flex items-center rounded-xl border border-stone-300 bg-stone-100 px-2.5 py-0.5 text-xs font-medium text-stone-500">
      Désactivé
    </span>
  );
}

function NeedStatusBadge({ status }: { status: string }) {
  const cls = STATUS_COLORS[status] ?? STATUS_COLORS.Draft;
  return (
    <span className={`inline-flex items-center rounded-lg border px-2 py-0.5 text-xs font-medium ${cls}`}>
      {STATUS_LABELS[status] ?? status}
    </span>
  );
}

/* ── Modal ── */

function ActivityModal({
  activity,
  onClose,
}: {
  activity: UserActivityResponse;
  onClose: () => void;
}) {
  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/40 p-4 pt-[5vh]" onClick={onClose}>
      <div
        className="relative w-full max-w-3xl rounded-2xl border border-stone-200 bg-white shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-start justify-between border-b border-stone-200 px-6 py-5">
          <div>
            <h2 className="text-lg font-semibold text-stone-950">
              {activity.fullName ?? activity.username}
            </h2>
            <div className="mt-1 flex flex-wrap items-center gap-2 text-sm text-stone-500">
              <span>{activity.username}</span>
              <RoleBadge role={activity.role} />
              <StatusBadge isActive={activity.isActive} />
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-500 transition hover:bg-stone-50"
          >
            Fermer
          </button>
        </div>

        {/* Body */}
        <div className="max-h-[70vh] overflow-y-auto px-6 py-5">
          {activity.teachingNeeds.length === 0 ? (
            <p className="py-8 text-center text-sm text-stone-400">
              Aucune demande d&apos;enseignement enregistrée pour ce compte.
            </p>
          ) : (
            <div className="space-y-5">
              <p className="text-sm text-stone-500">
                {activity.teachingNeeds.length} demande{activity.teachingNeeds.length !== 1 ? 's' : ''} d&apos;enseignement
              </p>

              {activity.teachingNeeds.map((need) => (
                <div key={need.id} className="rounded-xl border border-stone-200 bg-stone-50/50">
                  {/* Need header */}
                  <div className="border-b border-stone-200 px-4 py-3">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="text-sm font-semibold text-stone-900">{need.courseName}</span>
                      <span className="text-stone-300">·</span>
                      <span className="text-sm text-stone-600">{need.sessionName}</span>
                      <NeedStatusBadge status={need.status} />
                      {need.isFastTrack && (
                        <span className="rounded-lg bg-violet-50 px-2 py-0.5 text-xs font-medium text-violet-700 border border-violet-200">
                          FastTrack
                        </span>
                      )}
                    </div>
                    <div className="mt-2 flex flex-wrap gap-x-5 gap-y-1 text-xs text-stone-400">
                      <span>Créée le {new Date(need.createdAt).toLocaleDateString('fr-CA')}</span>
                      {need.submittedAt && <span>Soumise le {new Date(need.submittedAt).toLocaleDateString('fr-CA')}</span>}
                      {need.reviewedAt && <span>Révisée le {new Date(need.reviewedAt).toLocaleDateString('fr-CA')}</span>}
                      {need.expectedStudents != null && <span>{need.expectedStudents} étudiants attendus</span>}
                    </div>
                  </div>

                  {/* Need metadata */}
                  {(need.notes || need.desiredModifications || need.additionalComments || need.rejectionReason) && (
                    <div className="border-b border-stone-200 px-4 py-3 space-y-2">
                      {need.rejectionReason && (
                        <div className="rounded-lg bg-rose-50 px-3 py-2 text-sm">
                          <span className="font-medium text-rose-700">Raison du rejet : </span>
                          <span className="text-rose-600">{need.rejectionReason}</span>
                        </div>
                      )}
                      {need.notes && (
                        <div className="text-sm">
                          <span className="font-medium text-stone-700">Notes : </span>
                          <span className="text-stone-600">{need.notes}</span>
                        </div>
                      )}
                      {need.desiredModifications && (
                        <div className="text-sm">
                          <span className="font-medium text-stone-700">Modifications souhaitées : </span>
                          <span className="text-stone-600">{need.desiredModifications}</span>
                        </div>
                      )}
                      {need.additionalComments && (
                        <div className="text-sm">
                          <span className="font-medium text-stone-700">Commentaires : </span>
                          <span className="text-stone-600">{need.additionalComments}</span>
                        </div>
                      )}
                    </div>
                  )}

                  {/* Items */}
                  {need.items.length > 0 && (
                    <div className="px-4 py-3">
                      <h4 className="mb-2 text-xs font-semibold uppercase tracking-wide text-stone-400">
                        Éléments ({need.items.length})
                      </h4>
                      <div className="divide-y divide-stone-100 rounded-lg border border-stone-200 bg-white">
                        {need.items.map((item) => (
                          <div key={item.id} className="px-3 py-2.5">
                            <div className="flex flex-wrap items-center gap-2">
                              <span className="rounded bg-stone-100 px-1.5 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-stone-500">
                                {ITEM_TYPE_LABELS[item.itemType] ?? item.itemType}
                              </span>
                              {item.softwareName && (
                                <span className="text-sm font-medium text-stone-900">{item.softwareName}</span>
                              )}
                              {item.versionNumber && (
                                <span className="text-sm text-stone-500">v{item.versionNumber}</span>
                              )}
                              {item.osName && (
                                <span className="rounded bg-sky-50 px-1.5 py-0.5 text-xs text-sky-700">{item.osName}</span>
                              )}
                              {item.quantity != null && (
                                <span className="text-xs text-stone-400">Qté : {item.quantity}</span>
                              )}
                            </div>
                            {(item.description || item.notes) && (
                              <div className="mt-1 space-y-0.5 text-xs text-stone-500">
                                {item.description && <p>{item.description}</p>}
                                {item.notes && <p className="italic">{item.notes}</p>}
                              </div>
                            )}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  {need.items.length === 0 && (
                    <div className="px-4 py-3 text-xs text-stone-400">Aucun élément dans cette demande.</div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

/* ── Main page ── */

export function UsersPage() {
  const { apiFetch } = useAuth();
  const [users, setUsers] = useState<UserResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState('');

  const [roleEdits, setRoleEdits] = useState<Record<number, string>>({});
  const [roleUpdating, setRoleUpdating] = useState<Record<number, boolean>>({});
  const [passwordEdits, setPasswordEdits] = useState<Record<number, string>>({});
  const [passwordUpdating, setPasswordUpdating] = useState<Record<number, boolean>>({});

  const [passwordResetSuccess, setPasswordResetSuccess] = useState<Record<number, string>>({});

  const [deactivateConfirm, setDeactivateConfirm] = useState<number | null>(null);
  const [deactivating, setDeactivating] = useState<Record<number, boolean>>({});
  const [reactivating, setReactivating] = useState<Record<number, boolean>>({});

  const [modalActivity, setModalActivity] = useState<UserActivityResponse | null>(null);
  const [activityLoading, setActivityLoading] = useState<Record<number, boolean>>({});

  const [form, setForm] = useState<CreateUserRequest>(initialForm);
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState('');
  const [createSuccess, setCreateSuccess] = useState('');

  const [showInactive, setShowInactive] = useState(true);

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setPageError('');
    try {
      const data = await apiFetch<UserResponse[]>('/users?includeInactive=true');
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

  async function handleDeactivate(userId: number) {
    setDeactivating((prev) => ({ ...prev, [userId]: true }));
    try {
      await apiFetch(`/users/${userId}/deactivate`, { method: 'POST' });
      setUsers((prev) =>
        prev.map((u) => (u.id === userId ? { ...u, isActive: false } : u)),
      );
      setDeactivateConfirm(null);
    } catch (err) {
      setPageError(getErrorMessage(err, 'Impossible de désactiver le compte.'));
    } finally {
      setDeactivating((prev) => ({ ...prev, [userId]: false }));
    }
  }

  async function handleReactivate(userId: number) {
    setReactivating((prev) => ({ ...prev, [userId]: true }));
    try {
      await apiFetch(`/users/${userId}/reactivate`, { method: 'POST' });
      setUsers((prev) =>
        prev.map((u) => (u.id === userId ? { ...u, isActive: true } : u)),
      );
      setModalActivity(null);
    } catch (err) {
      setPageError(getErrorMessage(err, 'Impossible de réactiver le compte.'));
    } finally {
      setReactivating((prev) => ({ ...prev, [userId]: false }));
    }
  }

  async function handleOpenActivity(userId: number) {
    setActivityLoading((prev) => ({ ...prev, [userId]: true }));
    try {
      const data = await apiFetch<UserActivityResponse>(`/users/${userId}/activity`);
      setModalActivity(data);
    } catch (err) {
      setPageError(getErrorMessage(err, 'Impossible de charger l\'activité.'));
    } finally {
      setActivityLoading((prev) => ({ ...prev, [userId]: false }));
    }
  }

  async function handlePasswordReset(userId: number) {
    const newPassword = (passwordEdits[userId] ?? '').trim();
    if (newPassword.length < 8) {
      setPageError('Le mot de passe temporaire doit contenir au moins 8 caractères.');
      return;
    }

    setPasswordUpdating((prev) => ({ ...prev, [userId]: true }));
    setPasswordResetSuccess((prev) => ({ ...prev, [userId]: '' }));
    setPageError('');

    try {
      const body: UpdateUserPasswordRequest = { newPassword };
      await apiFetch(`/users/${userId}/password`, {
        method: 'PUT',
        body: JSON.stringify(body),
      });
      setPasswordEdits((prev) => ({ ...prev, [userId]: '' }));
      setPasswordResetSuccess((prev) => ({ ...prev, [userId]: 'Mot de passe réinitialisé.' }));
      setTimeout(() => {
        setPasswordResetSuccess((prev) => ({ ...prev, [userId]: '' }));
      }, 4000);
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
      setCreateSuccess(`Compte "${form.username}" créé avec le rôle ${ROLE_LABELS[form.roleName as RoleName] ?? form.roleName}.`);
      setForm(initialForm);
      await loadUsers();
    } catch (err) {
      setCreateError(getErrorMessage(err, 'Impossible de créer l\'utilisateur.'));
    } finally {
      setCreating(false);
    }
  }

  const filteredUsers = showInactive ? users : users.filter((u) => u.isActive);
  const activeCount = users.filter((u) => u.isActive).length;
  const inactiveCount = users.filter((u) => !u.isActive).length;

  return (
    <div className="space-y-8">
      {/* Modal */}
      {modalActivity && (
        <ActivityModal
          activity={modalActivity}
          onClose={() => setModalActivity(null)}
        />
      )}

      {/* Header */}
      <section className="rounded-[2rem] bg-[radial-gradient(circle_at_top_left,_rgba(255,255,255,0.12),_transparent_28%),linear-gradient(135deg,_#682a36_0%,_#dc042c_50%,_#c00328_100%)] px-6 py-8 text-white sm:px-8">
        <p className="text-xs uppercase tracking-[0.35em] text-white/90">ÉTS · Administration</p>
        <h1 className="mt-4 text-3xl font-semibold sm:text-4xl">Gestion des utilisateurs</h1>
        <p className="mt-3 text-sm leading-7 text-white/85">
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
          <div className="flex items-center gap-3">
            <h2 className="text-base font-semibold text-stone-950">Utilisateurs</h2>
            <span className="text-xs text-stone-500">
              {activeCount} actif{activeCount !== 1 ? 's' : ''}
              {inactiveCount > 0 && ` · ${inactiveCount} désactivé${inactiveCount !== 1 ? 's' : ''}`}
            </span>
          </div>
          <div className="flex items-center gap-3">
            <label className="flex items-center gap-2 text-xs text-stone-600">
              <input
                type="checkbox"
                checked={showInactive}
                onChange={(e) => setShowInactive(e.target.checked)}
                className="rounded border-stone-300"
              />
              Afficher désactivés
            </label>
            <button
              type="button"
              onClick={() => void loadUsers()}
              className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 transition hover:bg-stone-50"
            >
              Rafraîchir
            </button>
          </div>
        </div>

        {loading ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Chargement...</div>
        ) : filteredUsers.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-stone-500">Aucun utilisateur trouvé.</div>
        ) : (
          <div className="divide-y divide-stone-100">
            {filteredUsers.map((user) => {
              const pendingRole = roleEdits[user.id] ?? user.roles;
              const roleChanged = pendingRole !== user.roles;
              const isUpdating = roleUpdating[user.id] ?? false;
              const isPasswordUpdating = passwordUpdating[user.id] ?? false;
              const pendingPassword = passwordEdits[user.id] ?? '';
              const isDeactivating = deactivating[user.id] ?? false;
              const isReactivating = reactivating[user.id] ?? false;
              const isProtectedAdmin = user.roles === 'admin';
              const passwordSuccess = passwordResetSuccess[user.id];
              const isActivityLoading = activityLoading[user.id] ?? false;

              return (
                <div
                  key={user.id}
                  className={`px-5 py-5 transition sm:px-6 ${user.isActive ? 'hover:bg-stone-50/60' : 'bg-stone-50/40'}`}
                >
                  {/* Row 1: email + badges */}
                  <div className="flex flex-wrap items-center gap-2">
                    <span className={`text-sm font-medium break-all ${user.isActive ? 'text-stone-950' : 'text-stone-500'}`}>
                      {user.username}
                    </span>
                    <RoleBadge role={user.roles} />
                    <StatusBadge isActive={user.isActive} />
                  </div>

                  {/* Row 2: actions */}
                  {user.isActive ? (
                    <div className="mt-3 grid gap-3 sm:grid-cols-[1fr_1fr_auto]">
                      {/* Role change */}
                      <div className="flex items-center gap-2">
                        <select
                          value={pendingRole}
                          onChange={(e) =>
                            setRoleEdits((prev) => ({ ...prev, [user.id]: e.target.value }))
                          }
                          className="min-w-0 flex-1 rounded-xl border border-stone-200 bg-white px-3 py-1.5 text-sm text-stone-700 outline-none focus:border-[var(--ets-primary)] focus:ring-2 focus:ring-[rgba(220,4,44,0.15)]"
                        >
                          {ALL_ROLES.map((r) => (
                            <option key={r} value={r}>{ROLE_LABELS[r]}</option>
                          ))}
                        </select>
                        {roleChanged && (
                          <button
                            type="button"
                            onClick={() => void handleRoleUpdate(user.id)}
                            disabled={isUpdating}
                            className="shrink-0 rounded-xl bg-stone-950 px-3 py-1.5 text-xs font-medium text-white transition hover:bg-stone-700 disabled:opacity-50"
                          >
                            {isUpdating ? '...' : 'Sauvegarder'}
                          </button>
                        )}
                      </div>

                      {/* Password reset */}
                      <div className="flex items-center gap-2">
                        <input
                          type="password"
                          value={pendingPassword}
                          onChange={(event) =>
                            setPasswordEdits((prev) => ({ ...prev, [user.id]: event.target.value }))
                          }
                          className="input-field min-w-0 flex-1"
                          placeholder="Nouveau mot de passe"
                          minLength={8}
                        />
                        <button
                          type="button"
                          onClick={() => void handlePasswordReset(user.id)}
                          disabled={isPasswordUpdating || pendingPassword.trim().length < 8}
                          className="shrink-0 rounded-xl bg-[var(--ets-primary)] px-3 py-1.5 text-xs font-medium text-white transition hover:bg-[var(--ets-primary-hover)] disabled:opacity-50"
                        >
                          {isPasswordUpdating ? '...' : 'Reset'}
                        </button>
                      </div>

                      {/* Deactivate / protected */}
                      <div className="flex items-center">
                        {isProtectedAdmin ? (
                          <span className="text-xs text-stone-400">Compte protégé</span>
                        ) : deactivateConfirm === user.id ? (
                          <div className="flex items-center gap-2">
                            <span className="text-xs text-stone-600">Désactiver ?</span>
                            <button
                              type="button"
                              onClick={() => void handleDeactivate(user.id)}
                              disabled={isDeactivating}
                              className="rounded-xl bg-amber-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-amber-700 disabled:opacity-50"
                            >
                              {isDeactivating ? '...' : 'Oui'}
                            </button>
                            <button
                              type="button"
                              onClick={() => setDeactivateConfirm(null)}
                              className="rounded-xl border border-stone-200 px-3 py-1.5 text-xs text-stone-600 hover:bg-stone-50"
                            >
                              Non
                            </button>
                          </div>
                        ) : (
                          <button
                            type="button"
                            onClick={() => setDeactivateConfirm(user.id)}
                            className="rounded-xl border border-amber-200 px-3 py-1.5 text-xs text-amber-600 transition hover:bg-amber-50"
                          >
                            Désactiver
                          </button>
                        )}
                      </div>
                    </div>
                  ) : (
                    <div className="mt-3 flex flex-wrap items-center gap-3">
                      <button
                        type="button"
                        onClick={() => void handleOpenActivity(user.id)}
                        disabled={isActivityLoading}
                        className="rounded-xl border border-stone-300 bg-white px-4 py-1.5 text-xs font-medium text-stone-700 transition hover:bg-stone-50 disabled:opacity-50"
                      >
                        {isActivityLoading ? 'Chargement...' : 'Consulter le compte'}
                      </button>
                      <button
                        type="button"
                        onClick={() => void handleReactivate(user.id)}
                        disabled={isReactivating}
                        className="rounded-xl bg-emerald-600 px-4 py-1.5 text-xs font-medium text-white transition hover:bg-emerald-700 disabled:opacity-50"
                      >
                        {isReactivating ? 'Réactivation...' : 'Réactiver le compte'}
                      </button>
                      <span className="text-xs text-stone-400">
                        Les données et l&apos;historique sont conservés.
                      </span>
                    </div>
                  )}

                  {/* Password reset success feedback */}
                  {passwordSuccess ? (
                    <p className="mt-2 text-xs text-emerald-600">{passwordSuccess}</p>
                  ) : null}
                </div>
              );
            })}
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
                <option key={r} value={r}>{ROLE_LABELS[r]}</option>
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
