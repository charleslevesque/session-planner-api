export function NeedsPage() {
  return (
    <div className="grid gap-6 lg:grid-cols-[1.25fr_0.75fr]">
      <section className="surface-card p-6 sm:p-8">
        <p className="text-xs uppercase tracking-[0.35em] text-stone-500">Besoins</p>
        <h1 className="mt-3 text-3xl font-semibold text-stone-950">Ecran reserve au workflow de saisie.</h1>
        <p className="mt-4 max-w-2xl text-sm leading-7 text-stone-600 sm:text-base">
          Ce placeholder permet deja de valider la protection de route, la navigation et la place disponible
          pour le futur formulaire de besoins pedagogiques.
        </p>
      </section>

      <aside className="surface-card p-6">
        <p className="text-sm font-medium text-stone-900">Prochaine iteration</p>
        <ul className="mt-4 space-y-3 text-sm leading-6 text-stone-600">
          <li>Connexion avec la liste des sessions ouvertes.</li>
          <li>Creation et edition des teaching needs.</li>
          <li>Validation selon le role et le statut.</li>
        </ul>
      </aside>
    </div>
  );
}
