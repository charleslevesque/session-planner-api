const stats = [
  { label: 'Sessions actives', value: '03', detail: 'Une vue synthese pour lancer les prochains modules.' },
  { label: 'Besoins ouverts', value: '14', detail: 'Le suivi mettra ici les demandes en attente de validation.' },
  { label: 'Configurations critiques', value: '05', detail: 'Cette tuile servira aux ecarts de capacite et de compatibilite.' },
];

export function DashboardPage() {
  return (
    <div className="space-y-8">
      <section className="rounded-[2rem] bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.32),_transparent_32%),linear-gradient(135deg,_#20150d_0%,_#3f2b1d_45%,_#8a5a31_100%)] px-6 py-8 text-white sm:px-8">
        <p className="text-xs uppercase tracking-[0.35em] text-amber-100/80">Dashboard</p>
        <h1 className="mt-4 max-w-2xl text-3xl font-semibold sm:text-4xl">
          Le socle UI est pret pour brancher les futurs modules metier.
        </h1>
        <p className="mt-4 max-w-2xl text-sm leading-7 text-amber-50/80 sm:text-base">
          Cette premiere page valide le layout authentifie, la navigation laterale et l&apos;etat de session.
        </p>
      </section>

      <section className="grid gap-4 xl:grid-cols-3">
        {stats.map((item) => (
          <article key={item.label} className="surface-card p-6">
            <p className="text-sm uppercase tracking-[0.3em] text-stone-500">{item.label}</p>
            <p className="mt-4 text-4xl font-semibold text-stone-950">{item.value}</p>
            <p className="mt-4 text-sm leading-6 text-stone-600">{item.detail}</p>
          </article>
        ))}
      </section>
    </div>
  );
}
