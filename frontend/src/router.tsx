import { Navigate, createBrowserRouter } from 'react-router-dom';
import { AppLayout } from './components/AppLayout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { PublicOnlyRoute } from './components/PublicOnlyRoute';
import { RoleRoute } from './components/RoleRoute';
import { useAuth } from './contexts/AuthContext';
import { PAGE_ACCESS } from './lib/access';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';
import { MatrixPage } from './pages/MatrixPage';
import { NeedsPage } from './pages/NeedsPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { RegisterPage } from './pages/RegisterPage';
import { SessionNeedsPage } from './pages/SessionNeedsPage';
import { SessionsManagementPage } from './pages/SessionsManagementPage';
import { SecurityPage } from './pages/SecurityPage';
import { UsersPage } from './pages/UsersPage';

function RootRedirect() {
  const { isAuthenticated, isInitializing } = useAuth();

  if (isInitializing) {
    return (
      <div className="flex min-h-screen items-center justify-center px-6 text-center">
        <div className="surface-card max-w-md p-8">
          <p className="text-sm uppercase tracking-[0.3em] text-stone-500">Authentification</p>
          <h1 className="mt-3 text-2xl font-semibold text-stone-900">Connexion en cours</h1>
          <p className="mt-3 text-sm text-stone-600">Restauration de votre session.</p>
        </div>
      </div>
    );
  }

  return <Navigate to={isAuthenticated ? '/dashboard' : '/login'} replace />;
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <RootRedirect />,
  },
  {
    path: '/login',
    element: (
      <PublicOnlyRoute>
        <LoginPage />
      </PublicOnlyRoute>
    ),
  },
  {
    path: '/register',
    element: (
      <PublicOnlyRoute>
        <RegisterPage />
      </PublicOnlyRoute>
    ),
  },
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <AppLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: 'dashboard',
        element: <DashboardPage />,
      },
      {
        path: 'besoins',
        element: (
          <RoleRoute allowedRoles={PAGE_ACCESS.besoins}>
            <NeedsPage />
          </RoleRoute>
        ),
      },
      {
        path: 'sessions/manage',
        element: (
          <RoleRoute allowedRoles={PAGE_ACCESS.sessionsManage}>
            <SessionsManagementPage />
          </RoleRoute>
        ),
      },
      {
        path: 'sessions/:id/needs',
        element: (
          <RoleRoute allowedRoles={PAGE_ACCESS.sessionNeeds}>
            <SessionNeedsPage />
          </RoleRoute>
        ),
      },
      {
        path: 'matrice',
        element: (
          <RoleRoute allowedRoles={PAGE_ACCESS.matrice}>
            <MatrixPage />
          </RoleRoute>
        ),
      },
      {
        path: 'compte/securite',
        element: <SecurityPage />,
      },
      {
        path: 'admin/users',
        element: (
          <RoleRoute allowedRoles={PAGE_ACCESS.users}>
            <UsersPage />
          </RoleRoute>
        ),
      },
    ],
  },
  {
    path: '*',
    element: <NotFoundPage />,
  },
]);
