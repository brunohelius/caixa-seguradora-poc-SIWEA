import React from 'react';
import { useQuery } from '@tanstack/react-query';
import * as claimsApi from '../../services/claimsApi';
import PhaseTimeline from './PhaseTimeline';

export interface PhaseRecord {
  codFase: number;
  nomeFase: string;
  codEvento: number;
  nomeEvento: string;
  numOcorrSiniaco: number;
  dataInivigRefaev: Date;
  dataAberturaSifa: Date;
  dataFechaSifa: Date | null;
  status: 'Aberta' | 'Fechada';
  diasAberta: number;
}

export interface ProtocolKey {
  fonte: number;
  protsini: number;
  dac: number;
}

interface ClaimPhasesComponentProps {
  protocolKey: ProtocolKey;
  autoRefresh?: boolean;
}

const ClaimPhasesComponent: React.FC<ClaimPhasesComponentProps> = ({
  protocolKey,
  autoRefresh = false
}) => {
  const { data, isLoading, isError, error, refetch } = useQuery({
    queryKey: ['claimPhases', protocolKey],
    queryFn: () => claimsApi.getClaimPhases(
      protocolKey.fonte,
      protocolKey.protsini,
      protocolKey.dac
    ),
    staleTime: autoRefresh ? 0 : 60000, // Always refetch if autoRefresh is true
    refetchInterval: autoRefresh ? 30000 : false, // Auto-refresh every 30 seconds if enabled
  });

  const handleRefresh = () => {
    refetch();
  };

  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner"></div>
        <p>Carregando fases do sinistro...</p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="error-container">
        <p className="error-message">
          Erro ao carregar fases: {error instanceof Error ? error.message : 'Erro desconhecido'}
        </p>
        <button onClick={handleRefresh} className="button-primary">
          Tentar Novamente
        </button>
      </div>
    );
  }

  if (!data || !data.fases || data.fases.length === 0) {
    return (
      <div className="empty-state">
        <p>Nenhuma fase encontrada para este sinistro.</p>
      </div>
    );
  }

  // Color-code days open: green < 30 days, yellow 30-60 days, red > 60 days
  const getDaysOpenColor = (days: number): string => {
    if (days < 30) return 'green';
    if (days <= 60) return 'yellow';
    return 'red';
  };

  return (
    <div className="claim-phases-container">
      <div className="phases-header">
        <h3>Fases do Sinistro</h3>
        <p className="protocol-info">
          Protocolo: {data.protocolo} | Total de Fases: {data.totalFases}
        </p>
        <button onClick={handleRefresh} className="button-secondary refresh-button">
          🔄 Atualizar
        </button>
      </div>

      <PhaseTimeline phases={data.fases} />

      {/* Detailed table view */}
      <div className="phases-table-container">
        <table className="phases-table">
          <thead>
            <tr>
              <th>Fase</th>
              <th>Evento</th>
              <th>Abertura</th>
              <th>Fechamento</th>
              <th>Status</th>
              <th>Dias Aberta</th>
            </tr>
          </thead>
          <tbody>
            {data.fases.map((phase: PhaseRecord, index: number) => (
              <tr key={index} className={phase.status === 'Aberta' ? 'phase-open' : 'phase-closed'}>
                <td>
                  <strong>{phase.codFase}</strong>
                  <br />
                  <small>{phase.nomeFase}</small>
                </td>
                <td>
                  <strong>{phase.codEvento}</strong>
                  <br />
                  <small>{phase.nomeEvento}</small>
                </td>
                <td>{new Date(phase.dataAberturaSifa).toLocaleDateString('pt-BR')}</td>
                <td>
                  {phase.dataFechaSifa
                    ? new Date(phase.dataFechaSifa).toLocaleDateString('pt-BR')
                    : 'Em Aberto'}
                </td>
                <td>
                  <span className={`status-badge status-${phase.status.toLowerCase()}`}>
                    {phase.status}
                  </span>
                </td>
                <td>
                  <span className={`days-open days-${getDaysOpenColor(phase.diasAberta)}`}>
                    {phase.diasAberta} dias
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <style>{`
        .claim-phases-container {
          margin: 20px 0;
          padding: 20px;
          background: #fff;
          border-radius: 4px;
          box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .phases-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          margin-bottom: 20px;
          flex-wrap: wrap;
        }

        .phases-header h3 {
          margin: 0;
          color: #333;
        }

        .protocol-info {
          color: #666;
          margin: 5px 0;
        }

        .refresh-button {
          padding: 8px 16px;
          cursor: pointer;
        }

        .loading-container, .error-container, .empty-state {
          text-align: center;
          padding: 40px 20px;
        }

        .loading-spinner {
          border: 4px solid #f3f3f3;
          border-top: 4px solid #3498db;
          border-radius: 50%;
          width: 40px;
          height: 40px;
          animation: spin 1s linear infinite;
          margin: 0 auto 20px;
        }

        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }

        .phases-table-container {
          overflow-x: auto;
          margin-top: 30px;
        }

        .phases-table {
          width: 100%;
          border-collapse: collapse;
        }

        .phases-table th {
          background: #f5f5f5;
          padding: 12px;
          text-align: left;
          font-weight: 600;
          border-bottom: 2px solid #ddd;
        }

        .phases-table td {
          padding: 12px;
          border-bottom: 1px solid #eee;
        }

        .phases-table tbody tr:hover {
          background: #fafafa;
        }

        .phase-open {
          background: #f0f8ff;
        }

        .status-badge {
          padding: 4px 8px;
          border-radius: 3px;
          font-size: 12px;
          font-weight: 600;
        }

        .status-aberta {
          background: #4CAF50;
          color: white;
        }

        .status-fechada {
          background: #9e9e9e;
          color: white;
        }

        .days-open {
          font-weight: 600;
        }

        .days-green {
          color: #4CAF50;
        }

        .days-yellow {
          color: #FF9800;
        }

        .days-red {
          color: #f44336;
        }

        /* Mobile responsive */
        @media (max-width: 850px) {
          .phases-header {
            flex-direction: column;
            align-items: flex-start;
          }

          .refresh-button {
            margin-top: 10px;
          }

          .phases-table {
            font-size: 14px;
          }

          .phases-table th, .phases-table td {
            padding: 8px;
          }
        }
      `}</style>
    </div>
  );
};

export default ClaimPhasesComponent;
