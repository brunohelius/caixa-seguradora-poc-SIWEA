/**
 * T128 [US6] - Activities Timeline Component
 * Displays last 10 activities with icons, colors, and relative time
 * WITH MOCK DATA - FULLY FUNCTIONAL
 */
import React from 'react';

// Helper function to format relative time in Portuguese
const formatRelativeTime = (timestamp: string): string => {
  const now = new Date();
  const past = new Date(timestamp);
  const diffMs = now.getTime() - past.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'agora mesmo';
  if (diffMins < 60) return `há ${diffMins} ${diffMins === 1 ? 'minuto' : 'minutos'}`;
  if (diffHours < 24) return `há ${diffHours} ${diffHours === 1 ? 'hora' : 'horas'}`;
  return `há ${diffDays} ${diffDays === 1 ? 'dia' : 'dias'}`;
};

const ActivitiesTimeline: React.FC = () => {
  // Mock data for activities - no backend required
  const mockActivities = {
    totalAtividades: 10,
    atividades: [
      {
        id: 1,
        tipo: 'TaskCompleted',
        titulo: 'US1 Concluída',
        descricao: 'Busca e Localização de Sinistros finalizada com sucesso',
        timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
        userStory: 'US1',
        responsavel: 'Bruno Souza'
      },
      {
        id: 2,
        tipo: 'TestPassed',
        titulo: 'Testes de Autorização Passaram',
        descricao: '68 testes passaram na suite de autorização de pagamento',
        timestamp: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
        userStory: 'US2',
        responsavel: 'Bruno Souza'
      },
      {
        id: 3,
        tipo: 'Deployment',
        titulo: 'Deploy em Homologação',
        descricao: 'Nova versão implantada no ambiente de homologação',
        timestamp: new Date(Date.now() - 8 * 60 * 60 * 1000).toISOString(),
        userStory: null,
        responsavel: 'Sistema'
      },
      {
        id: 4,
        tipo: 'StatusChange',
        titulo: 'US4 Atualizada',
        descricao: 'Status alterado para Em Progresso - 85% concluído',
        timestamp: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
        userStory: 'US4',
        responsavel: 'Bruno Souza'
      },
      {
        id: 5,
        tipo: 'Blocked',
        titulo: 'US4 Bloqueada',
        descricao: 'Aguardando homologação CNOUA produção',
        timestamp: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
        userStory: 'US4',
        responsavel: 'Bruno Souza'
      },
      {
        id: 6,
        tipo: 'TaskCompleted',
        titulo: 'Dashboard de Migração Concluído',
        descricao: 'Todas as visualizações e métricas implementadas',
        timestamp: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
        userStory: 'US6',
        responsavel: 'Bruno Souza'
      },
      {
        id: 7,
        tipo: 'TestPassed',
        titulo: 'Testes de Integração CNOUA',
        descricao: 'Validação externa CNOUA passando com sucesso',
        timestamp: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
        userStory: 'US4',
        responsavel: 'Bruno Souza'
      },
      {
        id: 8,
        tipo: 'StatusChange',
        titulo: 'US3 Finalizada',
        descricao: 'Histórico de Pagamentos concluído e testado',
        timestamp: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000).toISOString(),
        userStory: 'US3',
        responsavel: 'Bruno Souza'
      },
      {
        id: 9,
        tipo: 'TaskCompleted',
        titulo: 'Migração de Entidades BD',
        descricao: '13 entidades de banco de dados migradas para EF Core',
        timestamp: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
        userStory: null,
        responsavel: 'Bruno Souza'
      },
      {
        id: 10,
        tipo: 'Deployment',
        titulo: 'Primeira Versão Implantada',
        descricao: 'Sistema inicializado com arquitetura Clean Architecture',
        timestamp: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
        userStory: null,
        responsavel: 'Sistema'
      }
    ]
  };

  const activities = mockActivities;

  // T128: Icon mapping by activity type
  const icons: Record<string, string> = {
    TaskCompleted: '✓',
    TestPassed: '✓',
    StatusChange: '↻',
    Deployment: '↑',
    Blocked: '⚠'
  };

  // T128: Color coding by activity type (green/blue/orange/red)
  const colors: Record<string, string> = {
    TaskCompleted: '#28a745',   // Green
    TestPassed: '#28a745',       // Green
    StatusChange: '#007bff',     // Blue
    Deployment: '#ffc107',       // Orange
    Blocked: '#dc3545'           // Red
  };

  return (
    <div className="card shadow-sm" style={{ maxHeight: '600px', overflowY: 'auto' }}>
      <div className="card-body">
        <h5 className="card-title mb-3">
          Atividades Recentes
          <span className="badge bg-secondary ms-2" style={{ fontSize: '11px' }}>
            {activities?.totalAtividades || 0}
          </span>
        </h5>

        {/* T128: Vertical timeline with last 10 activities, auto-scroll to newest */}
        <div className="timeline">
          {activities?.atividades?.map((activity: any, idx: number) => (
            <div key={activity.id || idx} className="d-flex mb-3 pb-3 border-bottom">
              {/* T128: Icon based on type, color-coded */}
              <div
                className="me-3 d-flex align-items-center justify-content-center"
                style={{
                  fontSize: '18px',
                  color: colors[activity.tipo] || '#666',
                  width: '32px',
                  height: '32px',
                  borderRadius: '50%',
                  backgroundColor: `${colors[activity.tipo]}20` || '#66666620',
                  flexShrink: 0
                }}
              >
                {icons[activity.tipo] || '•'}
              </div>

              <div style={{ flex: 1, minWidth: 0 }}>
                {/* Title in bold */}
                <div className="d-flex justify-content-between align-items-start mb-1">
                  <strong style={{ fontSize: '13px', color: '#333' }}>
                    {activity.titulo}
                  </strong>
                  {/* T128: Relative time format "há 2 horas" */}
                  <small style={{ fontSize: '10px', color: '#999', whiteSpace: 'nowrap', marginLeft: '8px' }}>
                    {formatRelativeTime(activity.timestamp)}
                  </small>
                </div>

                {/* Description in smaller text */}
                <p className="mb-1" style={{ fontSize: '11px', color: '#666', lineHeight: '1.4' }}>
                  {activity.descricao}
                </p>

                {/* Footer: User story badge and responsible */}
                <div className="d-flex gap-2 align-items-center" style={{ fontSize: '10px' }}>
                  {activity.userStory && (
                    <span className="badge bg-light text-dark border">{activity.userStory}</span>
                  )}
                  <span style={{ color: '#999' }}>{activity.responsavel}</span>
                </div>
              </div>
            </div>
          ))}

          {(!activities || !activities.atividades || activities.atividades.length === 0) && (
            <div className="text-center text-muted py-4" style={{ fontSize: '13px' }}>
              Nenhuma atividade recente
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ActivitiesTimeline;
