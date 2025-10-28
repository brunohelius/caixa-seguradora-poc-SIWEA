import React, { useState } from 'react';
import type { PhaseRecord } from './ClaimPhasesComponent';

interface PhaseTimelineProps {
  phases: PhaseRecord[];
}

const PhaseTimeline: React.FC<PhaseTimelineProps> = ({ phases }) => {
  const [expandedPhase, setExpandedPhase] = useState<number | null>(null);

  if (!phases || phases.length === 0) {
    return null;
  }

  // Sort phases chronologically by opening date
  const sortedPhases = [...phases].sort(
    (a, b) => new Date(a.dataAberturaSifa).getTime() - new Date(b.dataAberturaSifa).getTime()
  );

  const toggleExpand = (index: number) => {
    setExpandedPhase(expandedPhase === index ? null : index);
  };

  const formatDate = (date: Date | string | null): string => {
    if (!date) return 'Em Aberto';
    const d = new Date(date);
    return d.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  };

  return (
    <div className="phase-timeline">
      <div className="timeline-line"></div>
      {sortedPhases.map((phase, index) => {
        const isOpen = phase.status === 'Aberta';
        const isExpanded = expandedPhase === index;

        return (
          <div key={index} className="timeline-item">
            <div
              className={`timeline-node ${isOpen ? 'node-open' : 'node-closed'}`}
              onClick={() => toggleExpand(index)}
              role="button"
              tabIndex={0}
              aria-label={`${phase.nomeFase} - ${phase.status}`}
            >
              {isOpen ? (
                <div className="node-pulsing"></div>
              ) : (
                <span className="checkmark">✓</span>
              )}
            </div>

            <div className="timeline-content">
              <div
                className="timeline-header"
                onClick={() => toggleExpand(index)}
                style={{ cursor: 'pointer' }}
              >
                <h4 className="phase-name">
                  {phase.nomeFase || `Fase ${phase.codFase}`}
                  <span className={`phase-badge ${isOpen ? 'badge-open' : 'badge-closed'}`}>
                    {phase.status}
                  </span>
                </h4>
                <p className="phase-event">
                  Evento: {phase.nomeEvento || `${phase.codEvento}`}
                </p>
                <div className="phase-dates">
                  <span>Abertura: {formatDate(phase.dataAberturaSifa)}</span>
                  {!isOpen && phase.dataFechaSifa && (
                    <span> | Fechamento: {formatDate(phase.dataFechaSifa)}</span>
                  )}
                </div>
                <div className="phase-duration">
                  <span className={`duration-badge ${
                    phase.diasAberta < 30 ? 'duration-green' :
                    phase.diasAberta <= 60 ? 'duration-yellow' :
                    'duration-red'
                  }`}>
                    {phase.diasAberta} dias {isOpen ? 'aberta' : 'de duração'}
                  </span>
                </div>
              </div>

              {isExpanded && (
                <div className="timeline-details">
                  <div className="detail-grid">
                    <div className="detail-item">
                      <strong>Código da Fase:</strong> {phase.codFase}
                    </div>
                    <div className="detail-item">
                      <strong>Código do Evento:</strong> {phase.codEvento}
                    </div>
                    <div className="detail-item">
                      <strong>Num. Ocorrência:</strong> {phase.numOcorrSiniaco}
                    </div>
                    <div className="detail-item">
                      <strong>Data Início Vigência:</strong> {formatDate(phase.dataInivigRefaev)}
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        );
      })}

      <style>{`
        .phase-timeline {
          position: relative;
          padding: 20px 0;
        }

        .timeline-line {
          position: absolute;
          left: 20px;
          top: 40px;
          bottom: 40px;
          width: 2px;
          background: #ddd;
        }

        .timeline-item {
          display: flex;
          margin-bottom: 30px;
          position: relative;
        }

        .timeline-node {
          width: 40px;
          height: 40px;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          flex-shrink: 0;
          cursor: pointer;
          z-index: 1;
          transition: transform 0.2s;
        }

        .timeline-node:hover {
          transform: scale(1.1);
        }

        .node-open {
          background: #4CAF50;
          box-shadow: 0 0 0 4px rgba(76, 175, 80, 0.2);
        }

        .node-closed {
          background: #9e9e9e;
          box-shadow: 0 0 0 4px rgba(158, 158, 158, 0.2);
        }

        .node-pulsing {
          width: 12px;
          height: 12px;
          background: white;
          border-radius: 50%;
          animation: pulse 2s infinite;
        }

        @keyframes pulse {
          0%, 100% {
            transform: scale(1);
            opacity: 1;
          }
          50% {
            transform: scale(1.5);
            opacity: 0.7;
          }
        }

        .checkmark {
          color: white;
          font-size: 20px;
          font-weight: bold;
        }

        .timeline-content {
          flex: 1;
          margin-left: 20px;
          padding: 15px;
          background: #fff;
          border-radius: 4px;
          box-shadow: 0 2px 4px rgba(0,0,0,0.1);
          border-left: 3px solid #ddd;
        }

        .timeline-header:hover {
          background: #fafafa;
        }

        .phase-name {
          margin: 0 0 8px 0;
          color: #333;
          font-size: 18px;
          display: flex;
          align-items: center;
          gap: 10px;
        }

        .phase-badge {
          font-size: 12px;
          padding: 4px 8px;
          border-radius: 3px;
          font-weight: 600;
        }

        .badge-open {
          background: #4CAF50;
          color: white;
        }

        .badge-closed {
          background: #9e9e9e;
          color: white;
        }

        .phase-event {
          margin: 5px 0;
          color: #666;
          font-size: 14px;
        }

        .phase-dates {
          margin: 8px 0;
          font-size: 14px;
          color: #555;
        }

        .phase-duration {
          margin-top: 10px;
        }

        .duration-badge {
          padding: 4px 8px;
          border-radius: 3px;
          font-size: 13px;
          font-weight: 600;
        }

        .duration-green {
          background: #E8F5E9;
          color: #2E7D32;
        }

        .duration-yellow {
          background: #FFF3E0;
          color: #E65100;
        }

        .duration-red {
          background: #FFEBEE;
          color: #C62828;
        }

        .timeline-details {
          margin-top: 15px;
          padding-top: 15px;
          border-top: 1px solid #eee;
          animation: fadeIn 0.3s;
        }

        @keyframes fadeIn {
          from {
            opacity: 0;
            transform: translateY(-10px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }

        .detail-grid {
          display: grid;
          grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
          gap: 12px;
        }

        .detail-item {
          padding: 8px;
          background: #f9f9f9;
          border-radius: 3px;
        }

        .detail-item strong {
          display: block;
          margin-bottom: 4px;
          color: #333;
          font-size: 12px;
        }

        /* Mobile responsive */
        @media (max-width: 850px) {
          .timeline-line {
            left: 15px;
          }

          .timeline-node {
            width: 30px;
            height: 30px;
          }

          .checkmark {
            font-size: 16px;
          }

          .timeline-content {
            margin-left: 15px;
            padding: 12px;
          }

          .phase-name {
            font-size: 16px;
            flex-direction: column;
            align-items: flex-start;
          }

          .detail-grid {
            grid-template-columns: 1fr;
          }
        }

        /* Tablet responsive */
        @media (max-width: 768px) and (min-width: 481px) {
          .detail-grid {
            grid-template-columns: repeat(2, 1fr);
          }
        }
      `}</style>
    </div>
  );
};

export default PhaseTimeline;
