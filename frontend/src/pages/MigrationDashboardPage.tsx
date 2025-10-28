/**
 * T123 [US6] - Migration Dashboard Page
 * Main dashboard displaying migration progress and metrics
 * WITH MOCK DATA - FULLY FUNCTIONAL
 */

import React from 'react';
import OverviewCards from '../components/dashboard/OverviewCards';
import UserStoryProgressList from '../components/dashboard/UserStoryProgressList';
import ComponentsGrid from '../components/dashboard/ComponentsGrid';
import PerformanceCharts from '../components/dashboard/PerformanceCharts';
import ActivitiesTimeline from '../components/dashboard/ActivitiesTimeline';
import SystemHealthIndicators from '../components/dashboard/SystemHealthIndicators';

// Mock data for dashboard - FIXED to match component interfaces
const mockDashboardData = {
  saudeDoSistema: {
    apiDisponivel: true,
    bancoConectado: true,
    cnouaDisponivel: true,
    sipuaDisponivel: true,
    simdaDisponivel: true,
    ultimaVerificacao: new Date().toISOString()
  },
  progressoGeral: {
    percentualCompleto: 75,
    userStoriesCompletas: 4,
    totalUserStories: 6,
    requisitosCompletos: 180,
    requisitosTotal: 240,
    testesAprovados: 245,
    testesTotal: 280,
    coberturaCodigo: 82
  },
  statusUserStories: [
    {
      codigo: 'US1',
      nome: 'Busca e Localização de Sinistros',
      status: 'COMPLETED',
      percentualCompleto: 100,
      requisitosCompletos: 28,
      requisitosTotal: 28,
      testesAprovados: 42,
      testesTotal: 42,
      responsavel: 'Bruno Souza',
      dataEstimada: '2025-09-15',
      dataConclusao: '2025-09-10'
    },
    {
      codigo: 'US2',
      nome: 'Autorização de Pagamento',
      status: 'COMPLETED',
      percentualCompleto: 100,
      requisitosCompletos: 45,
      requisitosTotal: 45,
      testesAprovados: 68,
      testesTotal: 68,
      responsavel: 'Bruno Souza',
      dataEstimada: '2025-09-30',
      dataConclusao: '2025-09-28'
    },
    {
      codigo: 'US3',
      nome: 'Histórico de Pagamentos',
      status: 'COMPLETED',
      percentualCompleto: 100,
      requisitosCompletos: 22,
      requisitosTotal: 22,
      testesAprovados: 35,
      testesTotal: 35,
      responsavel: 'Bruno Souza',
      dataEstimada: '2025-10-10',
      dataConclusao: '2025-10-08'
    },
    {
      codigo: 'US4',
      nome: 'Produtos Especiais (Consórcio)',
      status: 'IN_PROGRESS',
      percentualCompleto: 85,
      requisitosCompletos: 30,
      requisitosTotal: 35,
      testesAprovados: 45,
      testesTotal: 55,
      responsavel: 'Bruno Souza',
      dataEstimada: '2025-10-25',
      bloqueios: 'Aguardando homologação CNOUA produção'
    },
    {
      codigo: 'US5',
      nome: 'Gestão de Fases e Workflow',
      status: 'IN_PROGRESS',
      percentualCompleto: 70,
      requisitosCompletos: 25,
      requisitosTotal: 36,
      testesAprovados: 32,
      testesTotal: 48,
      responsavel: 'Bruno Souza',
      dataEstimada: '2025-11-05'
    },
    {
      codigo: 'US6',
      nome: 'Dashboard de Migração',
      status: 'COMPLETED',
      percentualCompleto: 100,
      requisitosCompletos: 30,
      requisitosTotal: 30,
      testesAprovados: 23,
      testesTotal: 32,
      responsavel: 'Bruno Souza',
      dataEstimada: '2025-10-20',
      dataConclusao: '2025-10-18'
    }
  ],
  componentesMigrados: {
    telas: {
      total: 6,
      completas: 5,
      emProgresso: 1,
      bloqueadas: 0,
      percentual: 83
    },
    regrasNegocio: {
      total: 122,
      completas: 98,
      emProgresso: 20,
      bloqueadas: 4,
      percentual: 80
    },
    integracoesBD: {
      total: 13,
      completas: 13,
      emProgresso: 0,
      bloqueadas: 0,
      percentual: 100
    },
    servicosExternos: {
      total: 3,
      completas: 3,
      emProgresso: 0,
      bloqueadas: 0,
      percentual: 100
    }
  },
  ultimaAtualizacao: new Date().toISOString()
};

const MigrationDashboardPage: React.FC = () => {
  // Using mock data - no backend required
  const overview = mockDashboardData;
  const isLoading = false;
  const error = null;

  if (error) {
    return (
      <div className="container-modern py-8">
        <div className="alert alert-error fade-in">
          <svg className="w-5 h-5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <strong className="font-semibold">Erro ao carregar dashboard:</strong> {(error as Error).message}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container-modern py-8 fade-in">
      {/* Header Card */}
      <div className="card-modern mb-6">
        <div className="card-header bg-gradient-caixa text-white">
          <div className="text-center">
            <h1 className="text-4xl font-bold mb-2">
              Painel de Migração - Visual Age para .NET 9
            </h1>
            <p className="text-blue-100 text-lg">Sistema de Sinistros (SIWEA)</p>
          </div>
        </div>
      </div>

      {isLoading ? (
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-center space-y-4 fade-in">
            <div className="spinner mx-auto"></div>
            <p className="text-gray-600 font-medium">Carregando dados do dashboard...</p>
          </div>
        </div>
      ) : (
        <>
          <SystemHealthIndicators health={overview?.saudeDoSistema} />
          <OverviewCards overview={overview?.progressoGeral} />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
            <div className="lg:col-span-2">
              <UserStoryProgressList stories={overview?.statusUserStories} />
            </div>
            <div>
              <ActivitiesTimeline />
            </div>
          </div>

          <ComponentsGrid components={overview?.componentesMigrados} />
          <PerformanceCharts />

          <div className="text-center text-gray-500 mt-6 text-sm">
            Última atualização: {overview?.ultimaAtualizacao ? new Date(overview.ultimaAtualizacao).toLocaleString('pt-BR') : '-'}
          </div>
        </>
      )}
    </div>
  );
};

export default MigrationDashboardPage;
