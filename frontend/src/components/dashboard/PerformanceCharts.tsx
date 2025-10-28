/**
 * T127 [US6] - Performance Charts Component
 * Chart 1: Bar chart comparing legacy vs new system (5 metrics)
 * Chart 2: Line chart showing improvement percentages over time
 * Chart 3: Table view with detailed metrics
 * WITH MOCK DATA - FULLY FUNCTIONAL
 */

import React, { useState } from 'react';
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer
} from 'recharts';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { CheckCircle2, XCircle } from 'lucide-react';

type Period = 'ultima-hora' | 'ultimo-dia' | 'ultima-semana' | 'ultimo-mes';

const PerformanceCharts: React.FC = () => {
  const [period, setPeriod] = useState<Period>('ultimo-dia');

  // Transform data for Bar Chart (Legacy vs New comparison)
  const comparisonData = [
    {
      name: 'Tempo Resposta',
      Legacy: 850,
      Novo: 125,
      unit: 'ms',
      improvement: 85.3
    },
    {
      name: 'Throughput',
      Legacy: 450,
      Novo: 1850,
      unit: 'req/min',
      improvement: 311.1
    },
    {
      name: 'Usuários Concorrentes',
      Legacy: 50,
      Novo: 500,
      unit: 'users',
      improvement: 900.0
    },
    {
      name: 'Uso de Memória',
      Legacy: 2048,
      Novo: 512,
      unit: 'MB',
      improvement: 75.0
    },
    {
      name: 'Taxa de Erro',
      Legacy: 5.8,
      Novo: 0.3,
      unit: '%',
      improvement: 94.8
    }
  ];

  // Transform data for Line Chart (improvement over time)
  const improvementTrendData = Array.from({ length: 30 }, (_, i) => ({
    day: `Dia ${i + 1}`,
    'Tempo Resposta': 70 + Math.random() * 20,
    Throughput: 200 + Math.random() * 50,
    'Taxa de Erro': 90 + Math.random() * 5
  }));

  // Detailed metrics table data
  const detailedMetrics = [
    {
      cenario: 'Busca de Sinistro por Protocolo',
      metrica: 'Tempo de Resposta',
      valorLegado: '850ms',
      valorNovo: '125ms',
      melhoria: 85.3,
      aprovado: true
    },
    {
      cenario: 'Autorização de Pagamento',
      metrica: 'Tempo de Processamento',
      valorLegado: '2.5s',
      valorNovo: '0.8s',
      melhoria: 68.0,
      aprovado: true
    },
    {
      cenario: 'Validação Externa (CNOUA)',
      metrica: 'Latência',
      valorLegado: '1200ms',
      valorNovo: '350ms',
      melhoria: 70.8,
      aprovado: true
    },
    {
      cenario: 'Conversão de Moeda (BTNF)',
      metrica: 'Precisão',
      valorLegado: '99.5%',
      valorNovo: '99.99%',
      melhoria: 0.49,
      aprovado: true
    },
    {
      cenario: 'Atualização de Fase',
      metrica: 'Taxa de Sucesso',
      valorLegado: '94.2%',
      valorNovo: '99.7%',
      melhoria: 5.84,
      aprovado: true
    }
  ];

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-card border rounded-lg p-3 shadow-lg">
          <p className="font-semibold mb-1">{payload[0].payload.name}</p>
          {payload.map((entry: any, index: number) => (
            <p key={index} className="text-sm" style={{ color: entry.color }}>
              {entry.name}: {entry.value} {payload[0].payload.unit}
            </p>
          ))}
          <p className="text-sm font-medium text-green-600 mt-1">
            Melhoria: {payload[0].payload.improvement}%
          </p>
        </div>
      );
    }
    return null;
  };

  return (
    <Card className="mb-6">
      <CardHeader>
        <div className="flex justify-between items-center">
          <CardTitle>Análise de Performance: Legacy vs .NET 9.0</CardTitle>
          <Select value={period} onValueChange={(value) => setPeriod(value as Period)}>
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Selecione o período" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="ultima-hora">Última Hora</SelectItem>
              <SelectItem value="ultimo-dia">Último Dia</SelectItem>
              <SelectItem value="ultima-semana">Última Semana</SelectItem>
              <SelectItem value="ultimo-mes">Último Mês</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </CardHeader>
      <CardContent>
        <Tabs defaultValue="comparison" className="w-full">
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="comparison">Comparação</TabsTrigger>
            <TabsTrigger value="trend">Tendência</TabsTrigger>
            <TabsTrigger value="details">Detalhes</TabsTrigger>
          </TabsList>

          {/* Chart 1: Bar Chart Comparison */}
          <TabsContent value="comparison" className="space-y-4">
            <div className="text-sm text-muted-foreground mb-4">
              Comparação de métricas chave entre o sistema legacy (Visual Age) e o novo sistema (.NET 9.0)
            </div>
            <ResponsiveContainer width="100%" height={400}>
              <BarChart data={comparisonData}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="name" stroke="hsl(var(--foreground))" fontSize={12} />
                <YAxis stroke="hsl(var(--foreground))" fontSize={12} />
                <Tooltip content={<CustomTooltip />} />
                <Legend />
                <Bar dataKey="Legacy" fill="hsl(var(--destructive))" name="Visual Age (Legacy)" />
                <Bar dataKey="Novo" fill="hsl(var(--primary))" name=".NET 9.0 (Novo)" />
              </BarChart>
            </ResponsiveContainer>

            {/* Summary Cards */}
            <div className="grid grid-cols-2 md:grid-cols-5 gap-3 mt-6">
              {comparisonData.map((metric) => (
                <div key={metric.name} className="border rounded-lg p-3 bg-card">
                  <div className="text-xs text-muted-foreground mb-1">{metric.name}</div>
                  <div className="text-lg font-bold text-green-600">
                    +{metric.improvement.toFixed(1)}%
                  </div>
                  <div className="text-xs text-muted-foreground mt-1">melhoria</div>
                </div>
              ))}
            </div>
          </TabsContent>

          {/* Chart 2: Line Chart Improvement Trend */}
          <TabsContent value="trend" className="space-y-4">
            <div className="text-sm text-muted-foreground mb-4">
              Evolução das melhorias de performance ao longo do tempo
            </div>
            <ResponsiveContainer width="100%" height={400}>
              <LineChart data={improvementTrendData}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="day" stroke="hsl(var(--foreground))" fontSize={10} />
                <YAxis stroke="hsl(var(--foreground))" fontSize={12} label={{ value: 'Melhoria (%)', angle: -90, position: 'insideLeft' }} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: 'hsl(var(--card))',
                    border: '1px solid hsl(var(--border))',
                    borderRadius: '8px'
                  }}
                />
                <Legend />
                <Line
                  type="monotone"
                  dataKey="Tempo Resposta"
                  stroke="hsl(var(--primary))"
                  strokeWidth={2}
                  dot={{ r: 2 }}
                />
                <Line
                  type="monotone"
                  dataKey="Throughput"
                  stroke="hsl(var(--chart-2))"
                  strokeWidth={2}
                  dot={{ r: 2 }}
                />
                <Line
                  type="monotone"
                  dataKey="Taxa de Erro"
                  stroke="hsl(var(--chart-3))"
                  strokeWidth={2}
                  dot={{ r: 2 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </TabsContent>

          {/* Chart 3: Detailed Metrics Table */}
          <TabsContent value="details" className="space-y-4">
            <div className="text-sm text-muted-foreground mb-4">
              Métricas detalhadas por cenário de teste
            </div>
            <div className="overflow-x-auto">
              <table className="w-full border-collapse">
                <thead>
                  <tr className="border-b bg-muted/50">
                    <th className="text-left p-3 text-sm font-semibold">Cenário de Teste</th>
                    <th className="text-left p-3 text-sm font-semibold">Métrica</th>
                    <th className="text-center p-3 text-sm font-semibold">Legacy</th>
                    <th className="text-center p-3 text-sm font-semibold">Novo</th>
                    <th className="text-center p-3 text-sm font-semibold">Melhoria</th>
                    <th className="text-center p-3 text-sm font-semibold">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {detailedMetrics.map((row, idx) => (
                    <tr key={idx} className="border-b hover:bg-muted/30 transition-colors">
                      <td className="p-3 text-sm">{row.cenario}</td>
                      <td className="p-3 text-sm text-muted-foreground">{row.metrica}</td>
                      <td className="p-3 text-sm text-center font-mono text-destructive">
                        {row.valorLegado}
                      </td>
                      <td className="p-3 text-sm text-center font-mono text-primary font-semibold">
                        {row.valorNovo}
                      </td>
                      <td className="p-3 text-sm text-center">
                        <span className="inline-flex items-center gap-1 text-green-600 font-semibold">
                          {row.melhoria > 0 ? '+' : ''}{row.melhoria.toFixed(1)}%
                        </span>
                      </td>
                      <td className="p-3 text-center">
                        {row.aprovado ? (
                          <CheckCircle2 className="w-5 h-5 text-green-600 inline-block" />
                        ) : (
                          <XCircle className="w-5 h-5 text-destructive inline-block" />
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </TabsContent>
        </Tabs>
      </CardContent>
    </Card>
  );
};

export default PerformanceCharts;
