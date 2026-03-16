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
import { SecurityPage } from './pages/SecurityPage';
import { UsersPage } from './pages/UsersPage';

function RootRedirect() {
  const { isAuthenticated } = useAuth();

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
