import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="flex min-h-screen items-center justify-center px-6 py-10">
      <div className="surface-card max-w-xl p-8 text-center sm:p-10">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">404</p>
        <h1 className="mt-4 text-3xl font-semibold text-stone-950">Page introuvable</h1>
        <p className="mt-4 text-sm leading-7 text-stone-600 sm:text-base">
          La route demandee n&apos;existe pas encore dans cette premiere iteration du frontend.
        </p>
        <Link
          to="/"
          className="mt-6 inline-flex items-center justify-center rounded-2xl bg-stone-950 px-5 py-3 text-sm font-medium text-white transition hover:bg-stone-800"
        >
          Revenir a l&apos;application
        </Link>
      </div>
    </div>
  );
}
