import { useAuth } from '../contexts/AuthContext';
import { AdminSessionCoursesPage } from './AdminSessionCoursesPage';
import { TeacherSessionCoursesPage } from './TeacherSessionCoursesPage';

export function SessionCoursesPage() {
  const { user } = useAuth();

  const isAdmin = user?.role === 'admin' || user?.role === 'lab_instructor';

  return isAdmin ? <AdminSessionCoursesPage /> : <TeacherSessionCoursesPage />;
}
