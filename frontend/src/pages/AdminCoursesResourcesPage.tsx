import { CoursesAdminSection } from '../components/admin/CoursesAdminSection';

export function AdminCoursesResourcesPage() {
  return (
    <div className="space-y-8">
      <section className="rounded-[2rem] bg-[radial-gradient(circle_at_top_left,_rgba(255,255,255,0.12),_transparent_28%),linear-gradient(135deg,_#682a36_0%,_#dc042c_50%,_#c00328_100%)] px-6 py-8 text-white sm:px-8">
        <p className="text-xs uppercase tracking-[0.35em] text-white/90">
          ÉTS · Administration
        </p>
        <h1 className="mt-4 text-3xl font-semibold sm:text-4xl">Cours et ressources</h1>
        <p className="mt-3 text-sm leading-7 text-white/85">
          Gérez le catalogue de cours. Sélectionnez un cours pour voir et gérer ses ressources.
        </p>
      </section>

      <CoursesAdminSection />
    </div>
  );
}
