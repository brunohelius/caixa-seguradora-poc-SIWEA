/**
 * T124 [US6] - Overview Cards Component
 * Displays key metrics in card format
 * Migrated to shadcn/ui components
 */

import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { Badge } from '@/components/ui/badge';

interface OverviewCardsProps {
  overview?: {
    percentualCompleto: number;
    userStoriesCompletas: number;
    totalUserStories: number;
    requisitosCompletos: number;
    requisitosTotal: number;
    testesAprovados: number;
    testesTotal: number;
    coberturaCodigo: number;
  };
}

const OverviewCards: React.FC<OverviewCardsProps> = ({ overview }) => {
  if (!overview) return null;

  const passRate = overview.testesTotal > 0 ? (overview.testesAprovados / overview.testesTotal) * 100 : 0;

  const getPassRateVariant = (): "default" | "secondary" | "destructive" | "outline" => {
    if (passRate >= 90) return "default";
    if (passRate >= 70) return "secondary";
    return "destructive";
  };

  const CircularProgress = ({ percentage, size = 80 }: { percentage: number; size?: number }) => {
    const radius = (size - 8) / 2;
    const circumference = 2 * Math.PI * radius;
    const strokeDashoffset = circumference - (percentage / 100) * circumference;

    return (
      <svg width={size} height={size} className="mx-auto">
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="hsl(var(--muted))"
          strokeWidth="8"
        />
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="hsl(var(--primary))"
          strokeWidth="8"
          strokeDasharray={circumference}
          strokeDashoffset={strokeDashoffset}
          transform={`rotate(-90 ${size / 2} ${size / 2})`}
          style={{ transition: 'stroke-dashoffset 0.5s ease' }}
        />
        <text
          x="50%"
          y="50%"
          textAnchor="middle"
          dy=".3em"
          fontSize="20"
          fontWeight="bold"
          fill="hsl(var(--foreground))"
        >
          {Math.round(percentage)}%
        </text>
      </svg>
    );
  };

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      {/* Overall Progress Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium text-muted-foreground">
            Progresso Geral
          </CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col items-center">
          <CircularProgress percentage={overview.percentualCompleto} />
          <p className="mt-4 text-sm text-muted-foreground">
            <strong className="text-foreground">
              {overview.userStoriesCompletas}/{overview.totalUserStories}
            </strong>{' '}
            User Stories
          </p>
        </CardContent>
      </Card>

      {/* Requirements Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium text-muted-foreground">
            Requisitos
          </CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col items-center justify-center">
          <div className="text-4xl font-bold text-primary my-4">
            {overview.requisitosCompletos}/{overview.requisitosTotal}
          </div>
          <Progress
            value={(overview.requisitosCompletos / overview.requisitosTotal) * 100}
            className="w-full h-2"
          />
        </CardContent>
      </Card>

      {/* Tests Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium text-muted-foreground">
            Testes
          </CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col items-center justify-center">
          <div className="text-4xl font-bold my-4">
            <Badge variant={getPassRateVariant()} className="text-2xl px-4 py-2">
              {overview.testesAprovados}/{overview.testesTotal}
            </Badge>
          </div>
          <p className="text-sm text-muted-foreground">
            Taxa de aprovação:{' '}
            <Badge variant={getPassRateVariant()} className="ml-1">
              {passRate.toFixed(1)}%
            </Badge>
          </p>
        </CardContent>
      </Card>

      {/* Code Coverage Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-sm font-medium text-muted-foreground">
            Cobertura de Código
          </CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col items-center">
          <CircularProgress percentage={overview.coberturaCodigo} size={80} />
          <p className="mt-4 text-xs text-muted-foreground">
            Meta: 80%
          </p>
        </CardContent>
      </Card>
    </div>
  );
};

export default OverviewCards;
