/**
 * T125 [US6] - User Story Progress List Component
 * Displays all 6 user stories with detailed progress information
 */

import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';
import { AlertCircle, CheckCircle2, Clock, XCircle } from 'lucide-react';

interface UserStory {
  codigo: string;
  nome: string;
  status: string;
  percentualCompleto: number;
  requisitosCompletos: number;
  requisitosTotal: number;
  testesAprovados: number;
  testesTotal: number;
  responsavel: string;
  dataEstimada: string;
  dataConclusao?: string;
  bloqueios?: string;
}

interface UserStoryProgressListProps {
  stories?: UserStory[];
}

const UserStoryProgressList: React.FC<UserStoryProgressListProps> = ({ stories = [] }) => {
  const getStatusConfig = (status: string) => {
    const configs: Record<string, { variant: 'default' | 'secondary' | 'destructive' | 'outline', icon: React.ReactNode, label: string }> = {
      COMPLETED: {
        variant: 'default',
        icon: <CheckCircle2 className="w-3 h-3" />,
        label: 'Concluída'
      },
      IN_PROGRESS: {
        variant: 'secondary',
        icon: <Clock className="w-3 h-3" />,
        label: 'Em Progresso'
      },
      PENDING: {
        variant: 'outline',
        icon: <Clock className="w-3 h-3" />,
        label: 'Pendente'
      },
      BLOCKED: {
        variant: 'destructive',
        icon: <XCircle className="w-3 h-3" />,
        label: 'Bloqueada'
      }
    };
    return configs[status] || configs.PENDING;
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  };

  if (!stories || stories.length === 0) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>User Stories</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Nenhuma user story encontrada.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>User Stories ({stories.length})</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {stories.map((story) => {
          const statusConfig = getStatusConfig(story.status);
          return (
            <div
              key={story.codigo}
              className="border rounded-lg p-4 hover:bg-muted/50 transition-colors cursor-pointer"
            >
              {/* Header */}
              <div className="flex justify-between items-start mb-3">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-semibold text-sm">{story.codigo}</span>
                    <Badge variant={statusConfig.variant} className="flex items-center gap-1">
                      {statusConfig.icon}
                      <span>{statusConfig.label}</span>
                    </Badge>
                  </div>
                  <h4 className="font-medium">{story.nome}</h4>
                </div>
                <div className="text-right text-xs text-muted-foreground">
                  <div>{story.responsavel}</div>
                  {story.dataConclusao ? (
                    <div className="text-green-600 font-medium">
                      Concluída: {formatDate(story.dataConclusao)}
                    </div>
                  ) : (
                    <div>Prev: {formatDate(story.dataEstimada)}</div>
                  )}
                </div>
              </div>

              {/* Progress Bar */}
              <div className="mb-3">
                <div className="flex justify-between items-center mb-1">
                  <span className="text-xs font-medium">Progresso</span>
                  <span className="text-xs font-semibold">{Math.round(story.percentualCompleto)}%</span>
                </div>
                <Progress value={story.percentualCompleto} className="h-2" />
              </div>

              {/* Metrics */}
              <div className="flex gap-4 text-xs">
                <div className="flex items-center gap-1">
                  <span className="text-muted-foreground">Requisitos:</span>
                  <span className="font-semibold">
                    {story.requisitosCompletos}/{story.requisitosTotal}
                  </span>
                </div>
                <div className="flex items-center gap-1">
                  <span className="text-muted-foreground">Testes:</span>
                  <span className="font-semibold">
                    {story.testesAprovados}/{story.testesTotal}
                  </span>
                  <span className="text-muted-foreground">
                    ({story.testesTotal > 0 ? Math.round((story.testesAprovados / story.testesTotal) * 100) : 0}%)
                  </span>
                </div>
              </div>

              {/* Blockers Alert */}
              {story.bloqueios && (
                <div className="mt-3 flex items-start gap-2 p-2 bg-destructive/10 border border-destructive/20 rounded text-xs text-destructive">
                  <AlertCircle className="w-4 h-4 mt-0.5 flex-shrink-0" />
                  <span>{story.bloqueios}</span>
                </div>
              )}
            </div>
          );
        })}
      </CardContent>
    </Card>
  );
};

export default UserStoryProgressList;
