/** T126 [US6] - Components Grid */
import React from 'react';

const ComponentsGrid: React.FC<{ components?: any }> = ({ components }) => {
  const quadrants = [
    { title: 'Telas', key: 'telas', icon: '🖥️' },
    { title: 'Regras de Negócio', key: 'regrasNegocio', icon: '⚙️' },
    { title: 'Integrações BD', key: 'integracoesBD', icon: '🗄️' },
    { title: 'Serviços Externos', key: 'servicosExternos', icon: '🌐' }
  ];

  return (
    <div className="card shadow-sm mb-4">
      <div className="card-body">
        <h5 className="card-title mb-3">Componentes Migrados</h5>
        <div className="row g-3">
          {quadrants.map(q => {
            const data = components?.[q.key] || { total: 0, completas: 0, emProgresso: 0, percentual: 0 };
            return (
              <div key={q.key} className="col-md-3">
                <div className="card bg-light">
                  <div className="card-body text-center">
                    <div style={{ fontSize: '32px' }}>{q.icon}</div>
                    <h6 className="mt-2">{q.title}</h6>
                    <div style={{ fontSize: '24px', fontWeight: 'bold' }}>{data.completas}/{data.total}</div>
                    <div className="progress mt-2" style={{ height: '6px' }}>
                      <div className="progress-bar bg-success" style={{ width: `${data.percentual}%` }} />
                    </div>
                    <small className="text-muted">{data.emProgresso} em progresso</small>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
};

export default ComponentsGrid;
