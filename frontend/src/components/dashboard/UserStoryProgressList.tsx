/** T125 [US6] - User Story Progress List */
import React from 'react';

const UserStoryProgressList: React.FC<{ stories?: any[] }> = ({ stories = [] }) => (
  <div className="card shadow-sm">
    <div className="card-body">
      <h5 className="card-title mb-3">User Stories</h5>
      {stories.map((story, idx) => {
        const statusColors: Record<string, string> = {
          NotStarted: '#6c757d', InProgress: '#007bff', Completed: '#28a745', Blocked: '#dc3545'
        };
        return (
          <div key={idx} className="mb-3 pb-3 border-bottom">
            <div className="d-flex justify-content-between align-items-center mb-2">
              <div>
                <strong>{story.codigo}</strong> - {story.nome}
                <span className="badge ms-2" style={{ backgroundColor: statusColors[story.status] || '#6c757d' }}>
                  {story.status}
                </span>
              </div>
              <span className="text-muted">{story.responsavel}</span>
            </div>
            <div className="progress mb-1" style={{ height: '6px' }}>
              <div className="progress-bar" style={{ width: `${story.percentualCompleto}%` }} />
            </div>
            <div className="d-flex justify-content-between" style={{ fontSize: '11px', color: '#666' }}>
              <span>Requisitos: {story.requisitosCompletos}/{story.requisitosTotal}</span>
              <span>Testes: {story.testesAprovados}/{story.testesTotal}</span>
            </div>
            {story.bloqueios && <div className="alert alert-danger p-1 mt-2" style={{ fontSize: '11px' }}>{story.bloqueios}</div>}
          </div>
        );
      })}
    </div>
  </div>
);

export default UserStoryProgressList;
