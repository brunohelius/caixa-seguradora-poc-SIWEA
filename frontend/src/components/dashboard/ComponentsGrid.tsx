/**
 * T126 [US6] - Components Grid Component
 * Display 4 quadrants: Telas, Regras de Negócio, Integrações BD, Serviços Externos
 */

import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Monitor, Settings, Database, Globe } from 'lucide-react';

interface ComponentCount {
  total: number;
  completas: number;
  emProgresso: number;
  bloqueadas: number;
  percentual: number;
}

interface ComponentsGridProps {
  components?: {
    telas?: ComponentCount;
    regrasNegocio?: ComponentCount;
    integracoesBD?: ComponentCount;
    servicosExternos?: ComponentCount;
  };
}

const ComponentsGrid: React.FC<ComponentsGridProps> = ({ components }) => {
  const quadrants = [
    {
      title: 'Telas',
      key: 'telas' as const,
      icon: <Monitor className="w-8 h-8 text-blue-600" />,
      description: 'Interfaces migradas'
    },
    {
      title: 'Regras de Negócio',
      key: 'regrasNegocio' as const,
      icon: <Settings className="w-8 h-8 text-purple-600" />,
      description: 'Lógica de negócio'
    },
    {
      title: 'Integrações BD',
      key: 'integracoesBD' as const,
      icon: <Database className="w-8 h-8 text-green-600" />,
      description: 'Entidades de dados'
    },
    {
      title: 'Serviços Externos',
      key: 'servicosExternos' as const,
      icon: <Globe className="w-8 h-8 text-orange-600" />,
      description: 'APIs externas'
    }
  ];

  const getDefaultCount = (): ComponentCount => ({
    total: 0,
    completas: 0,
    emProgresso: 0,
    bloqueadas: 0,
    percentual: 0
  });

  return (
    <Card className="mb-6">
      <CardHeader>
        <CardTitle>Componentes Migrados</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {quadrants.map((quadrant) => {
            const data = components?.[quadrant.key] || getDefaultCount();
            const percentage = data.percentual || 0;

            return (
              <div
                key={quadrant.key}
                className="border rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer bg-card"
              >
                {/* Icon and Title */}
                <div className="flex items-center gap-3 mb-3">
                  <div className="flex-shrink-0">{quadrant.icon}</div>
                  <div className="flex-1 min-w-0">
                    <h3 className="font-semibold text-sm truncate">{quadrant.title}</h3>
                    <p className="text-xs text-muted-foreground truncate">{quadrant.description}</p>
                  </div>
                </div>

                {/* Count */}
                <div className="text-center mb-3">
                  <div className="text-3xl font-bold">
                    <span className="text-primary">{data.completas}</span>
                    <span className="text-muted-foreground text-xl">/{data.total}</span>
                  </div>
                </div>

                {/* Progress Bar */}
                <div className="mb-3">
                  <div className="flex justify-between items-center mb-1">
                    <span className="text-xs text-muted-foreground">Progresso</span>
                    <span className="text-xs font-semibold">{Math.round(percentage)}%</span>
                  </div>
                  <Progress value={percentage} className="h-2" />
                </div>

                {/* Status Breakdown */}
                <div className="grid grid-cols-2 gap-2 text-xs">
                  <div className="flex items-center justify-between">
                    <span className="text-muted-foreground">Em progresso:</span>
                    <span className="font-medium text-blue-600">{data.emProgresso}</span>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-muted-foreground">Bloqueadas:</span>
                    <span className="font-medium text-destructive">{data.bloqueadas}</span>
                  </div>
                </div>
              </div>
            );
          })}
        </div>

        {/* Summary Section */}
        {components && (
          <div className="mt-6 pt-4 border-t">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
              <div>
                <div className="text-2xl font-bold text-primary">
                  {Object.values(components).reduce((sum, c) => sum + (c?.completas || 0), 0)}
                </div>
                <div className="text-xs text-muted-foreground">Total Completo</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-blue-600">
                  {Object.values(components).reduce((sum, c) => sum + (c?.emProgresso || 0), 0)}
                </div>
                <div className="text-xs text-muted-foreground">Em Progresso</div>
              </div>
              <div>
                <div className="text-2xl font-bold text-destructive">
                  {Object.values(components).reduce((sum, c) => sum + (c?.bloqueadas || 0), 0)}
                </div>
                <div className="text-xs text-muted-foreground">Bloqueadas</div>
              </div>
              <div>
                <div className="text-2xl font-bold">
                  {Object.values(components).reduce((sum, c) => sum + (c?.total || 0), 0)}
                </div>
                <div className="text-xs text-muted-foreground">Total Geral</div>
              </div>
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export default ComponentsGrid;
