/**
 * T129 [US6] - System Health Indicators Component
 * Shows service status with refresh button
 * 5 service indicators: API, Database, CNOUA, SIPUA, SIMDA
 * Status icons: green checkmark (available), red X (unavailable), yellow warning (degraded)
 */
import React, { useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';

interface SystemHealthIndicatorsProps {
  health?: {
    apiDisponivel: boolean;
    bancoConectado: boolean; // Changed from databaseDisponivel to match API spec
    cnouaDisponivel: boolean;
    sipuaDisponivel: boolean;
    simdaDisponivel: boolean;
    ultimaVerificacao: string;
  };
}

const SystemHealthIndicators: React.FC<SystemHealthIndicatorsProps> = ({ health }) => {
  const queryClient = useQueryClient();
  const [isRefreshing, setIsRefreshing] = useState(false);

  if (!health) return null;

  // T129: 5 service indicators with proper mapping
  const services = [
    { name: 'API', status: health.apiDisponivel, icon: '🌐' },
    { name: 'Database', status: health.bancoConectado, icon: '💾' },
    { name: 'CNOUA', status: health.cnouaDisponivel, icon: '🔗' },
    { name: 'SIPUA', status: health.sipuaDisponivel, icon: '🔗' },
    { name: 'SIMDA', status: health.simdaDisponivel, icon: '🔗' }
  ];

  // T129: Refresh button to force health check update
  const handleRefresh = async () => {
    setIsRefreshing(true);
    await queryClient.invalidateQueries({ queryKey: ['dashboardOverview'] });
    setTimeout(() => setIsRefreshing(false), 1000);
  };

  // Calculate overall health status
  const availableCount = services.filter(s => s.status).length;
  const totalCount = services.length;
  const allHealthy = availableCount === totalCount;
  const mostlyHealthy = availableCount >= totalCount - 1;

  return (
    <div
      className={`alert border mb-4 ${
        allHealthy ? 'alert-success' : mostlyHealthy ? 'alert-warning' : 'alert-danger'
      }`}
      style={{ backgroundColor: allHealthy ? '#d4edda' : mostlyHealthy ? '#fff3cd' : '#f8d7da' }}
    >
      <div className="d-flex justify-content-between align-items-center flex-wrap">
        <div className="d-flex align-items-center mb-2 mb-md-0">
          <strong className="me-3" style={{ fontSize: '14px' }}>
            Saúde do Sistema:
          </strong>
          <div className="d-flex gap-3 flex-wrap">
            {/* T129: 5 service indicators with status icons */}
            {services.map(service => (
              <div
                key={service.name}
                className="d-flex align-items-center"
                title={`${service.name}: ${service.status ? 'Disponível' : 'Indisponível'}`}
                style={{ cursor: 'help' }}
              >
                {/* T129: Status icon - green checkmark if available, red X if unavailable */}
                <span
                  className={`badge ${service.status ? 'bg-success' : 'bg-danger'} me-2`}
                  style={{ fontSize: '12px', minWidth: '24px' }}
                >
                  {service.status ? '✓' : '✗'}
                </span>
                <span style={{ fontSize: '13px', fontWeight: service.status ? 'normal' : 'bold' }}>
                  <span className="me-1">{service.icon}</span>
                  {service.name}
                </span>
              </div>
            ))}
          </div>
        </div>

        <div className="d-flex align-items-center gap-3">
          {/* T129: Hover tooltip with ultimaVerificacao timestamp */}
          <small
            className="text-muted"
            title={`Última verificação: ${new Date(health.ultimaVerificacao).toLocaleString('pt-BR')}`}
            style={{ fontSize: '11px', cursor: 'help' }}
          >
            🕒 {new Date(health.ultimaVerificacao).toLocaleTimeString('pt-BR')}
          </small>

          {/* T129: Refresh button */}
          <button
            className="btn btn-sm btn-outline-secondary"
            onClick={handleRefresh}
            disabled={isRefreshing}
            title="Atualizar status dos serviços"
            style={{ fontSize: '11px', padding: '4px 8px' }}
          >
            {isRefreshing ? (
              <span className="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
            ) : (
              '↻ '
            )}
            Atualizar
          </button>
        </div>
      </div>

      {/* Overall status summary */}
      <div className="mt-2 pt-2 border-top" style={{ fontSize: '12px' }}>
        <span className="badge bg-secondary me-2">{availableCount}/{totalCount}</span>
        {allHealthy ? (
          <span className="text-success">✓ Todos os serviços operacionais</span>
        ) : mostlyHealthy ? (
          <span className="text-warning">⚠ Serviço com falha detectado</span>
        ) : (
          <span className="text-danger">✗ Múltiplos serviços indisponíveis</span>
        )}
      </div>
    </div>
  );
};

export default SystemHealthIndicators;
