import { useCallback, useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { ResourceTabs } from '../components/ResourceTabs';
import { ResourceTable, type Column } from '../components/ResourceTable';
import { NeedActionPanel } from '../components/NeedActionPanel';
import { getErrorMessage } from '../lib/api';
import type { CourseResponse } from '../types/needs';
import type { SessionResponse } from '../types/sessions';
import type {
  CourseResourcesResponse,
  CourseSaaSResponse,
  CourseSoftwareResponse,
  CourseConfigurationResponse,
  CourseVmResponse,
  CourseServerResponse,
  CourseEquipmentResponse,
  ResourceTab,
} from '../types/courseResources';

const SAAS_COLUMNS: Column<CourseSaaSResponse>[] = [
  { key: 'name', label: 'Nom', render: (item) => item.name },
  { key: 'accounts', label: 'Comptes', render: (item) => item.numberOfAccounts ?? '—' },
  { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
];

const SOFTWARE_COLUMNS: Column<CourseSoftwareResponse>[] = [
  { key: 'name', label: 'Nom', render: (item) => item.name },
  { key: 'install', label: 'Commande d\'installation', render: (item) => item.installCommand ? (
    <code className="rounded bg-stone-100 px-1.5 py-0.5 text-xs font-mono text-stone-700">{item.installCommand}</code>
  ) : '—' },
];

const CONFIG_COLUMNS: Column<CourseConfigurationResponse>[] = [
  { key: 'title', label: 'Titre', render: (item) => item.title },
  { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
];

const VM_COLUMNS: Column<CourseVmResponse>[] = [
  { key: 'qty', label: 'Qté', render: (item) => item.quantity },
  { key: 'cpu', label: 'CPU', render: (item) => `${item.cpuCores} cœurs` },
  { key: 'ram', label: 'RAM', render: (item) => `${item.ramGb} Go` },
  { key: 'storage', label: 'Stockage', render: (item) => `${item.storageGb} Go` },
  { key: 'access', label: 'Accès', render: (item) => item.accessType },
  { key: 'os', label: 'OS', render: (item) => item.osName },
  { key: 'host', label: 'Serveur hôte', render: (item) => item.hostServerHostname ?? '—' },
  { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
];

const SERVER_COLUMNS: Column<CourseServerResponse>[] = [
  { key: 'hostname', label: 'Hostname', render: (item) => (
    <code className="rounded bg-stone-100 px-1.5 py-0.5 text-xs font-mono text-stone-700">{item.hostname}</code>
  ) },
  { key: 'cpu', label: 'CPU', render: (item) => `${item.cpuCores} cœurs` },
  { key: 'ram', label: 'RAM', render: (item) => `${item.ramGb} Go` },
  { key: 'storage', label: 'Stockage', render: (item) => `${item.storageGb} Go` },
  { key: 'access', label: 'Accès', render: (item) => item.accessType },
  { key: 'os', label: 'OS', render: (item) => item.osName },
  { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
];

const EQUIPMENT_COLUMNS: Column<CourseEquipmentResponse>[] = [
  { key: 'name', label: 'Nom', render: (item) => item.name },
  { key: 'qty', label: 'Quantité', render: (item) => item.quantity },
  { key: 'accessories', label: 'Accessoires', render: (item) => item.defaultAccessories ?? '—' },
  { key: 'notes', label: 'Notes', render: (item) => item.notes ?? '—', className: 'max-w-xs truncate' },
];

export function CourseResourcesPage() {
  const { sessionId, courseId } = useParams();
  const sId = Number(sessionId);
  const cId = Number(courseId);
  const { apiFetch, user } = useAuth();

  const [session, setSession] = useState<SessionResponse | null>(null);
  const [course, setCourse] = useState<CourseResponse | null>(null);
  const [resources, setResources] = useState<CourseResourcesResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<ResourceTab>('saas');

  const isTeacher = user?.role === 'professor' || user?.role === 'course_instructor';

  const loadData = useCallback(async () => {
    if (!Number.isFinite(sId) || !Number.isFinite(cId)) {
      setError('Paramètres invalides.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');

    try {
      const [sessionData, courseData, resourcesData] = await Promise.all([
        apiFetch<SessionResponse>(`/sessions/${sId}`),
        apiFetch<CourseResponse>(`/courses/${cId}`),
        apiFetch<CourseResourcesResponse>(`/courses/${cId}/resources`),
      ]);

      setSession(sessionData);
      setCourse(courseData);
      setResources(resourcesData);
    } catch (err) {
      setError(getErrorMessage(err, 'Impossible de charger les ressources du cours.'));
    } finally {
      setLoading(false);
    }
  }, [apiFetch, sId, cId]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  const counts = useMemo(() => ({
    saas: resources?.saaS?.length ?? 0,
    softwares: resources?.softwares?.length ?? 0,
    configurations: resources?.configurations?.length ?? 0,
    vms: resources?.virtualMachines?.length ?? 0,
    servers: resources?.physicalServers?.length ?? 0,
    equipment: resources?.equipment?.length ?? 0,
  }), [resources]);

  const totalCount = useMemo(() => Object.values(counts).reduce((sum, n) => sum + n, 0), [counts]);

  function renderActiveTable() {
    if (!resources) return null;

    switch (activeTab) {
      case 'saas':
        return <ResourceTable data={resources.saaS} columns={SAAS_COLUMNS} emptyMessage="Aucun produit SaaS associé à ce cours." keyExtractor={(item) => item.id} />;
      case 'softwares':
        return <ResourceTable data={resources.softwares} columns={SOFTWARE_COLUMNS} emptyMessage="Aucun logiciel associé à ce cours." keyExtractor={(item) => item.id} />;
      case 'configurations':
        return <ResourceTable data={resources.configurations} columns={CONFIG_COLUMNS} emptyMessage="Aucune configuration associée à ce cours." keyExtractor={(item) => item.id} />;
      case 'vms':
        return <ResourceTable data={resources.virtualMachines} columns={VM_COLUMNS} emptyMessage="Aucune machine virtuelle associée à ce cours." keyExtractor={(item) => item.id} />;
      case 'servers':
        return <ResourceTable data={resources.physicalServers} columns={SERVER_COLUMNS} emptyMessage="Aucun serveur physique associé à ce cours." keyExtractor={(item) => item.id} />;
      case 'equipment':
        return <ResourceTable data={resources.equipment} columns={EQUIPMENT_COLUMNS} emptyMessage="Aucun équipement associé à ce cours." keyExtractor={(item) => item.id} />;
    }
  }

  return (
    <div className="space-y-6">
      <Link
        to={`/sessions/${sId}/courses`}
        className="inline-flex text-sm text-[var(--ets-primary)] hover:text-[var(--ets-primary-hover)]"
      >
        &larr; Retour aux cours
      </Link>

      {loading ? (
        <div className="rounded-2xl border border-stone-200 bg-white/70 px-4 py-6 text-sm text-stone-600">Chargement...</div>
      ) : error ? (
        <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">{error}</div>
      ) : (
        <>
          <section className="surface-card p-6 sm:p-8">
            <p className="text-xs uppercase tracking-[0.3em] text-stone-500">Cours</p>
            <h1 className="mt-2 text-2xl font-semibold text-stone-950 sm:text-3xl">
              {course?.code}{course?.name ? ` — ${course.name}` : ''}
            </h1>
            <p className="mt-2 text-sm text-stone-600">
              {totalCount} ressource{totalCount !== 1 ? 's' : ''} associée{totalCount !== 1 ? 's' : ''} à ce cours
            </p>
            <p className="mt-1 text-xs text-stone-400">
              Ces ressources proviennent des demandes approuvées pour ce cours. Elles sont indépendantes du catalogue global.
            </p>
          </section>

          {session ? (
            <NeedActionPanel
              session={session}
              isTeacher={isTeacher}
              createNeedUrl={`/sessions/${sId}/courses/${cId}/create-need`}
            />
          ) : null}

          <section className="surface-card p-0">
            <div className="border-b border-stone-200 px-6 py-4">
              <ResourceTabs activeTab={activeTab} onTabChange={setActiveTab} counts={counts} />
            </div>
            {renderActiveTable()}
          </section>
        </>
      )}
    </div>
  );
}
