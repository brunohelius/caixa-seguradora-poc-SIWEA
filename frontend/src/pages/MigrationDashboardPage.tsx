/**
 * T123 [US6] - Migration Dashboard Page
 * Main dashboard displaying migration progress and metrics
 */

import React from 'react';
import { useQuery } from '@tanstack/react-query';
import OverviewCards from '../components/dashboard/OverviewCards';
import UserStoryProgressList from '../components/dashboard/UserStoryProgressList';
import ComponentsGrid from '../components/dashboard/ComponentsGrid';
import PerformanceCharts from '../components/dashboard/PerformanceCharts';
import ActivitiesTimeline from '../components/dashboard/ActivitiesTimeline';
import SystemHealthIndicators from '../components/dashboard/SystemHealthIndicators';

const LOGO_BASE64 = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==';

const MigrationDashboardPage: React.FC = () => {
  const { data: overview, isLoading, error } = useQuery({
    queryKey: ['dashboardOverview'],
    queryFn: async () => {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/dashboard/overview`);
      if (!response.ok) throw new Error('Failed to fetch dashboard data');
      return response.json();
    },
    refetchInterval: 30000 // 30 seconds auto-refresh (SC-014)
  });

  if (error) {
    return (
      <div className="alert alert-danger">
        Erro ao carregar dashboard: {(error as Error).message}
      </div>
    );
  }

  return (
    <div className="container-fluid" style={{ maxWidth: '1400px', margin: '0 auto', padding: '20px' }}>
      <div className="card shadow mb-4">
        <div className="card-body text-center">
          <img src={LOGO_BASE64} alt="Caixa Seguradora" style={{ maxHeight: '60px', marginBottom: '12px' }} />
          <h2>Painel de Migração - Visual Age para .NET 9</h2>
          <p className="text-muted mb-0">Sistema de Sinistros (SIWEA)</p>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-5">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Carregando...</span>
          </div>
        </div>
      ) : (
        <>
          <SystemHealthIndicators health={overview?.saudeDoSistema} />
          <OverviewCards overview={overview?.progressoGeral} />

          <div className="row g-3 mb-4">
            <div className="col-lg-8">
              <UserStoryProgressList stories={overview?.statusUserStories} />
            </div>
            <div className="col-lg-4">
              <ActivitiesTimeline />
            </div>
          </div>

          <ComponentsGrid components={overview?.componentesMigrados} />
          <PerformanceCharts />

          <div className="text-center text-muted mt-4" style={{ fontSize: '12px' }}>
            Última atualização: {overview?.ultimaAtualizacao ? new Date(overview.ultimaAtualizacao).toLocaleString('pt-BR') : '-'}
          </div>
        </>
      )}
    </div>
  );
};

export default MigrationDashboardPage;
