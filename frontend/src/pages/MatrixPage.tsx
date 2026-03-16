export function MatrixPage() {
  return (
    <div className="grid gap-6 xl:grid-cols-[0.9fr_1.1fr]">
      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Matrice</p>
        <h1 className="mt-3 text-3xl font-semibold text-stone-950">Zone de projection des affectations.</h1>
        <p className="mt-4 text-sm leading-7 text-stone-600 sm:text-base">
          Le layout est deja pret pour accueillir un tableau dense, des filtres et une lecture transversale des
          arbitrages par session, laboratoire et configuration.
        </p>
      </section>

      <section className="surface-card overflow-hidden p-0">
        <div className="grid grid-cols-4 border-b border-stone-200 bg-stone-950/95 text-xs uppercase tracking-[0.2em] text-white/70">
          <div className="px-4 py-3">Cours</div>
          <div className="px-4 py-3">Lab</div>
          <div className="px-4 py-3">OS</div>
          <div className="px-4 py-3">Statut</div>
        </div>
        {[
          ['LOG430', 'B-204', 'Windows 11', 'A venir'],
          ['LOG320', 'C-102', 'Ubuntu', 'A arbitrer'],
          ['INF155', 'A-015', 'macOS', 'Placeholder'],
        ].map((row) => (
          <div key={row.join('-')} className="grid grid-cols-4 border-b border-stone-100 text-sm text-stone-700">
            {row.map((cell) => (
              <div key={cell} className="px-4 py-4">
                {cell}
              </div>
            ))}
          </div>
        ))}
      </section>
    </div>
  );
}
