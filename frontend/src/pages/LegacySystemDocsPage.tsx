/**
 * Legacy System Documentation Dashboard - Beautiful Custom Layouts
 * Visual representation of legacy system documentation with rich components
 */

import React, { useState } from 'react';
import {
  BookOpen,
  Code,
  Database,
  Users,
  TrendingUp,
  Zap,
  Shield,
  FileText,
  Server,
  GitBranch,
  Clock,
  CheckCircle,
  PieChart,
  ClipboardList,
  Layers,
  Target,
  Calculator,
} from 'lucide-react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Progress } from '@/components/ui/progress';

export const LegacySystemDocsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('dashboard');

  const TARGET_BASE_PF = 250;
  const HOMOLOGATION_MARGIN_PERCENT = 15;
  const PHASE1_RATIO = 0.72;
  const PHASE2_RATIO = 0.28;

  const roundToOneDecimal = (value: number) => Math.round(value * 10) / 10;
  const computePercent = (part: number, total: number) =>
    total === 0 ? 0 : roundToOneDecimal((part / total) * 100);
  const formatNumber = (value: number) =>
    Number.isInteger(value) ? value.toString() : value.toFixed(1);
  const formatPercent = (value: number) => `${value.toFixed(1)}%`;

  const phase1Functions = [
    {
      id: 'F01',
      title: 'Busca de Sinistros',
      description: '3 critérios: protocolo, sinistro ou líder com validações DB2.',
      docPf: 28,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
    {
      id: 'F02',
      title: 'Autorização de Pagamento',
      description: 'Pipeline de 8 etapas com conversão BTNF e rollback ACID.',
      docPf: 72,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
    {
      id: 'F03',
      title: 'Histórico de Pagamentos',
      description: 'Consulta paginada THISTSIN com auditoria e exportação básica.',
      docPf: 22,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
    {
      id: 'F04',
      title: 'Produtos Especiais (Consórcio)',
      description: 'Tratamento CNOUA/SIPUA/SIMDA com regras por produto.',
      docPf: 32,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
    {
      id: 'F05',
      title: 'Gestão de Fases e Workflow',
      description: 'Abertura/fechamento SI_SINISTRO_FASE e eventos 1098.',
      docPf: 28,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
    {
      id: 'F06',
      title: 'Integração DB2',
      description: 'Mapeamento de 13 entidades TMESTSIN, THISTSIN, EF_CONTR_SEG_HABIT.',
      docPf: 45,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
    {
      id: 'F07',
      title: 'Conversão Monetária BTNF',
      description: 'Cálculo com 8 casas decimais e tabelas de conversão históricas.',
      docPf: 18,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
    {
      id: 'F08',
      title: 'Validação de Regras de Negócio',
      description: 'Mais de 100 regras (tipos de pagamento, beneficiário, saldo).',
      docPf: 35,
      badgeClass: 'bg-red-600 text-white',
      priority: 'P1 · Crítico',
    },
  ];

  const phase2Functions = [
    {
      id: 'F09',
      title: 'Dashboard de Migração',
      description:
        'Status em tempo real, componentes migrados e indicadores de saúde (recontagem: 1 EO complexa + 1 EQ média).',
      docPf: 16,
      badgeClass: 'bg-amber-500 text-white',
      priority: 'P2 · Alto',
    },
    {
      id: 'F10',
      title: 'Dashboard Analítico de Sinistros',
      description:
        'KPIs financeiros, tendências e ranking de sinistros com múltiplos filtros.',
      docPf: 20,
      badgeClass: 'bg-amber-500 text-white',
      priority: 'P2 · Alto',
    },
    {
      id: 'F11',
      title: 'Query Builder Visual',
      description:
        'Montagem drag-and-drop com filtros, agregações e preview (2 EIs + 2 EQs).',
      docPf: 15,
      badgeClass: 'bg-blue-500 text-white',
      priority: 'P3 · Médio',
    },
    {
      id: 'F12',
      title: 'Visualizações Avançadas',
      description:
        'Gráficos interativos: evolução, distribuição e heatmaps com drill-down básico.',
      docPf: 12,
      badgeClass: 'bg-blue-500 text-white',
      priority: 'P3 · Médio',
    },
    {
      id: 'F13',
      title: 'Exportação Multi-formato',
      description: 'Exporta Excel, CSV, JSON e PDF com layout customizável.',
      docPf: 8,
      badgeClass: 'bg-blue-500 text-white',
      priority: 'P3 · Médio',
    },
    {
      id: 'F14',
      title: 'Monitoramento de Integrações',
      description: 'Painel CNOUA/SIPUA/SIMDA com latência, erros e circuit breaker.',
      docPf: 10,
      badgeClass: 'bg-blue-500 text-white',
      priority: 'P3 · Médio',
    },
    {
      id: 'F15',
      title: 'Gestão de Configurações',
      description: 'Administra timeouts, feature flags e versionamento de parâmetros.',
      docPf: 7,
      badgeClass: 'bg-gray-500 text-white',
      priority: 'P4 · Baixo',
    },
    {
      id: 'F16',
      title: 'Autenticação e RBAC',
      description: 'Login seguro com perfis Admin, Operador, Auditor e Consulta.',
      docPf: 12,
      badgeClass: 'bg-amber-500 text-white',
      priority: 'P2 · Alto',
    },
  ];

  const marginMultiplier = 1 + HOMOLOGATION_MARGIN_PERCENT / 100;

  const sumDocPf = (items: { docPf: number }[]) =>
    items.reduce((total, item) => total + item.docPf, 0);

  const phase1DocTotal = sumDocPf(phase1Functions);
  const phase2DocTotal = sumDocPf(phase2Functions);

  const phase1BaseTarget = roundToOneDecimal(TARGET_BASE_PF * PHASE1_RATIO);
  const phase2BaseTarget = roundToOneDecimal(TARGET_BASE_PF * PHASE2_RATIO);

  const phase1Scale = phase1BaseTarget / phase1DocTotal;
  const phase2Scale = phase2BaseTarget / phase2DocTotal;

  const phase1Computed = phase1Functions.map((fn) => {
    const basePf = roundToOneDecimal(fn.docPf * phase1Scale);
    const pfWithMargin = roundToOneDecimal(basePf * marginMultiplier);
    return { ...fn, basePf, pfWithMargin };
  });

  const phase2Computed = phase2Functions.map((fn) => {
    const basePf = roundToOneDecimal(fn.docPf * phase2Scale);
    const pfWithMargin = roundToOneDecimal(basePf * marginMultiplier);
    return { ...fn, basePf, pfWithMargin };
  });

  const phase1TotalBase = roundToOneDecimal(
    phase1Computed.reduce((total, fn) => total + fn.basePf, 0)
  );
  const phase1TotalWithMargin = roundToOneDecimal(
    phase1Computed.reduce((total, fn) => total + fn.pfWithMargin, 0)
  );

  const phase2TotalBase = roundToOneDecimal(
    phase2Computed.reduce((total, fn) => total + fn.basePf, 0)
  );
  const phase2TotalWithMargin = roundToOneDecimal(
    phase2Computed.reduce((total, fn) => total + fn.pfWithMargin, 0)
  );

  const totalBasePf = roundToOneDecimal(phase1TotalBase + phase2TotalBase);
  const totalWithMargin = roundToOneDecimal(totalBasePf * marginMultiplier);
  const homologationBufferPf = roundToOneDecimal(totalWithMargin - totalBasePf);

  const phase1Share = computePercent(phase1TotalBase, totalBasePf);
  const phase2Share = computePercent(phase2TotalBase, totalBasePf);

  const migrationTimeline = [
    {
      phase: 'Mês 1: Setup e Fundamentos',
      duration: '4 semanas',
      tasks: [
        'Revisão acelerada da documentação e planos de migração',
        'Provisionamento Azure, CI/CD e monitoramento básico',
        'Modelagem EF Core e pipelines de carga parcial DB2 → SQL',
      ],
      status: 'Em Progresso',
      progress: 45,
    },
    {
      phase: 'Mês 2: Implementação Core',
      duration: '4 semanas',
      tasks: [
        'Entrega dos serviços .NET 9 para busca, autorização e histórico',
        'Integrações primárias (CNOUA/SIPUA/SIMDA) com resiliência',
        'Cobertura de regras críticas e cálculos BTNF em produção assistida',
      ],
      status: 'Planejado',
      progress: 10,
    },
    {
      phase: 'Mês 3: Testes e Go-Live Controlado',
      duration: '4 semanas',
      tasks: [
        'Bateria de testes integrados, performance e segurança',
        'Shadow mode de 4 semanas com ajustes rápidos',
        'Treinamento operacional, cutover e monitoração 24/7',
      ],
      status: 'Planejado',
      progress: 0,
    },
  ];

  const functionPointCards = [
    {
      title: 'Total do Projeto',
      value: `${formatNumber(totalBasePf)} PF base`,
      helper: `+${HOMOLOGATION_MARGIN_PERCENT}% homologação → ${formatNumber(totalWithMargin)} PF`,
      highlight: 'Execução ajustada para 3 meses',
      icon: PieChart,
      gradient: 'from-slate-700 to-slate-800',
    },
    {
      title: 'Fase 1 · Migração Core',
      value: `${formatNumber(phase1TotalBase)} PF base`,
      helper: `${formatPercent(phase1Share)} do total · c/ margem: ${formatNumber(phase1TotalWithMargin)} PF`,
      highlight: 'Concluir setup + core até o mês 2',
      icon: Layers,
      gradient: 'from-blue-600 to-blue-700',
    },
    {
      title: 'Fase 2 · Modernização',
      value: `${formatNumber(phase2TotalBase)} PF base`,
      helper: `${formatPercent(phase2Share)} do total · c/ margem: ${formatNumber(phase2TotalWithMargin)} PF`,
      highlight: 'Incrementos em paralelo ao core',
      icon: Target,
      gradient: 'from-purple-600 to-purple-700',
    },
  ];

  const migrationReportHighlights = [
    `Total recontado: ${formatNumber(totalBasePf)} PF base (+${HOMOLOGATION_MARGIN_PERCENT}% homologação → ${formatNumber(totalWithMargin)} PF).`,
    `Distribuição: Fase 1 com ${formatNumber(phase1TotalBase)} PF (${formatPercent(phase1Share)}) e Fase 2 com ${formatNumber(phase2TotalBase)} PF (${formatPercent(phase2Share)}).`,
    `Margem de homologação: ${formatNumber(homologationBufferPf)} PF adicionais para testes integrados e estabilização.`,
    'Cronograma comprimido em 3 meses: 2 meses para setup + implementação do core e 1 mês dedicado a testes integrados e estabilização.',
    'Esforço estimado de 16 pessoa-mês (12 PM na Fase 1, 4 PM na Fase 2) com equipe full-stack dedicada.',
    'Investimento previsto de R$ 520 mil (R$ 345,6k na Fase 1 e R$ 102,4k na Fase 2) incluindo contingência de 15%.',
    'Economia anual pós-migração estimada em R$ 996 mil substituindo custos mainframe por Azure, com payback em ~5,2 meses.',
    'Critérios críticos para go-live: 100% de paridade funcional, performance <3s para buscas, cobertura de testes >90% e auditoria completa.',
  ];

  return (
    <div className="container-modern py-8 fade-in">
      {/* Beautiful Header */}
      <div className="card-modern mb-6 overflow-hidden">
        <div className="relative bg-gradient-to-r from-blue-900 via-blue-700 to-purple-800 text-white p-8">
          <div className="absolute inset-0 bg-black opacity-10"></div>
          <div className="relative z-10">
            <div className="flex items-center gap-4 mb-4">
              <div className="bg-white bg-opacity-20 p-4 rounded-2xl backdrop-blur-sm">
                <BookOpen className="w-12 h-12" />
              </div>
              <div>
                <h1 className="text-5xl font-black mb-2">Sistema Legado SIWEA</h1>
                <p className="text-xl text-blue-100">Sistema de Indenização e Workflow de Eventos Atendidos</p>
              </div>
            </div>
            <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mt-6">
              <div className="bg-white bg-opacity-10 backdrop-blur-sm rounded-lg p-4 text-center">
                <Clock className="w-6 h-6 mx-auto mb-2" />
                <div className="text-2xl font-bold">35+</div>
                <div className="text-xs text-blue-100">anos em produção</div>
              </div>
              <div className="bg-white bg-opacity-10 backdrop-blur-sm rounded-lg p-4 text-center">
                <Users className="w-6 h-6 mx-auto mb-2" />
                <div className="text-2xl font-bold">200+</div>
                <div className="text-xs text-blue-100">usuários ativos</div>
              </div>
              <div className="bg-white bg-opacity-10 backdrop-blur-sm rounded-lg p-4 text-center">
                <Zap className="w-6 h-6 mx-auto mb-2" />
                <div className="text-2xl font-bold">8.000</div>
                <div className="text-xs text-blue-100">transações/dia</div>
              </div>
              <div className="bg-white bg-opacity-10 backdrop-blur-sm rounded-lg p-4 text-center">
                <Database className="w-6 h-6 mx-auto mb-2" />
                <div className="text-2xl font-bold">2.5M</div>
                <div className="text-xs text-blue-100">sinistros</div>
              </div>
              <div className="bg-white bg-opacity-10 backdrop-blur-sm rounded-lg p-4 text-center">
                <Shield className="w-6 h-6 mx-auto mb-2" />
                <div className="text-2xl font-bold">100%</div>
                <div className="text-xs text-blue-100">missão crítica</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Main Tabs */}
      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <div className="card-modern mb-6">
          <TabsList className="w-full justify-start gap-2 flex-wrap bg-transparent p-4">
            <TabsTrigger value="dashboard" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-blue-600 data-[state=active]:to-blue-700 data-[state=active]:text-white">
              <TrendingUp className="w-4 h-4 mr-2" />
              Dashboard
            </TabsTrigger>
            <TabsTrigger value="executive" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-purple-600 data-[state=active]:to-purple-700 data-[state=active]:text-white">
              📊 Sumário Executivo
            </TabsTrigger>
            <TabsTrigger value="architecture" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-green-600 data-[state=active]:to-green-700 data-[state=active]:text-white">
              🏗️ Arquitetura
            </TabsTrigger>
            <TabsTrigger value="database" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-yellow-600 data-[state=active]:to-yellow-700 data-[state=active]:text-white">
              💾 Banco de Dados
            </TabsTrigger>
            <TabsTrigger value="business" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-red-600 data-[state=active]:to-red-700 data-[state=active]:text-white">
              ⚙️ Regras de Negócio
            </TabsTrigger>
            <TabsTrigger value="integrations" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-indigo-600 data-[state=active]:to-indigo-700 data-[state=active]:text-white">
              🔗 Integrações
            </TabsTrigger>
            <TabsTrigger value="migration" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-pink-600 data-[state=active]:to-pink-700 data-[state=active]:text-white">
              🚀 Migração
            </TabsTrigger>
            <TabsTrigger value="functionPoints" className="data-[state=active]:bg-gradient-to-r data-[state=active]:from-slate-600 data-[state=active]:to-slate-700 data-[state=active]:text-white">
              📐 Pontos de Função
            </TabsTrigger>
          </TabsList>
        </div>

        {/* DASHBOARD TAB */}
        <TabsContent value="dashboard" className="space-y-6">
          {/* Statistics Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <Card className="bg-gradient-to-br from-blue-500 to-blue-600 text-white border-0 shadow-xl hover:shadow-2xl transition-all transform hover:scale-105">
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <FileText className="w-4 h-4" />
                  Documentação
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-5xl font-black mb-1">10</div>
                <p className="text-sm text-blue-100">documentos completos</p>
              </CardContent>
            </Card>

            <Card className="bg-gradient-to-br from-green-500 to-green-600 text-white border-0 shadow-xl hover:shadow-2xl transition-all transform hover:scale-105">
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <Code className="w-4 h-4" />
                  Código Fonte
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-5xl font-black mb-1">851.9 KB</div>
                <p className="text-sm text-green-100">COBOL/EZEE</p>
              </CardContent>
            </Card>

            <Card className="bg-gradient-to-br from-purple-500 to-purple-600 text-white border-0 shadow-xl hover:shadow-2xl transition-all transform hover:scale-105">
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <Database className="w-4 h-4" />
                  Entidades
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-5xl font-black mb-1">13</div>
                <p className="text-sm text-purple-100">tabelas DB2</p>
              </CardContent>
            </Card>

            <Card className="bg-gradient-to-br from-red-500 to-red-600 text-white border-0 shadow-xl hover:shadow-2xl transition-all transform hover:scale-105">
              <CardHeader className="pb-3">
                <CardTitle className="text-sm font-medium flex items-center gap-2">
                  <Shield className="w-4 h-4" />
                  Regras
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="text-5xl font-black mb-1">122</div>
                <p className="text-sm text-red-100">regras de negócio</p>
              </CardContent>
            </Card>
          </div>

          {/* Technology Stack Comparison */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card className="shadow-xl">
              <CardHeader className="bg-gray-800 text-white">
                <CardTitle className="flex items-center gap-2">
                  <Server className="w-5 h-5" />
                  Stack Legado (1989-2025)
                </CardTitle>
              </CardHeader>
              <CardContent className="pt-6">
                <div className="space-y-3">
                  {[
                    { name: 'IBM z/OS Mainframe', icon: '🖥️', color: 'bg-gray-100' },
                    { name: 'IBM CICS Transaction Server', icon: '⚡', color: 'bg-gray-100' },
                    { name: 'IBM DB2 for z/OS', icon: '💾', color: 'bg-gray-100' },
                    { name: 'IBM VisualAge EZEE 4.40', icon: '🔧', color: 'bg-gray-100' },
                    { name: 'COBOL ANSI 85', icon: '📝', color: 'bg-gray-100' },
                    { name: 'Terminal 3270', icon: '⌨️', color: 'bg-gray-100' }
                  ].map((tech, idx) => (
                    <div key={idx} className={`flex items-center gap-3 p-3 ${tech.color} rounded-lg hover:bg-gray-200 transition-colors`}>
                      <span className="text-2xl">{tech.icon}</span>
                      <span className="font-medium text-gray-800">{tech.name}</span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-xl">
              <CardHeader className="bg-gradient-to-r from-blue-600 to-blue-700 text-white">
                <CardTitle className="flex items-center gap-2">
                  <GitBranch className="w-5 h-5" />
                  Stack Migração (.NET 9.0)
                </CardTitle>
              </CardHeader>
              <CardContent className="pt-6">
                <div className="space-y-3">
                  {[
                    { name: '.NET 9.0', icon: '💻', color: 'bg-blue-50' },
                    { name: 'React 19 + TypeScript', icon: '⚛️', color: 'bg-blue-50' },
                    { name: 'SQL Server / PostgreSQL', icon: '🗄️', color: 'bg-blue-50' },
                    { name: 'Azure Cloud', icon: '☁️', color: 'bg-blue-50' },
                    { name: 'Docker / Kubernetes', icon: '🐳', color: 'bg-blue-50' },
                    { name: 'REST APIs', icon: '🔌', color: 'bg-blue-50' }
                  ].map((tech, idx) => (
                    <div key={idx} className={`flex items-center gap-3 p-3 ${tech.color} rounded-lg hover:bg-blue-100 transition-colors`}>
                      <span className="text-2xl">{tech.icon}</span>
                      <span className="font-medium text-blue-900">{tech.name}</span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Document Index */}
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-purple-50 to-blue-50">
              <CardTitle>Índice de Documentação</CardTitle>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {[
                  { id: 'executive', title: 'Sumário Executivo', desc: 'Visão executiva e indicadores', icon: '📊', pages: 12, color: 'purple' },
                  { id: 'architecture', title: 'Arquitetura Técnica', desc: '3 camadas e infraestrutura', icon: '🏗️', pages: 28, color: 'green' },
                  { id: 'database', title: 'Modelo de Dados', desc: '13 entidades e relacionamentos', icon: '💾', pages: 35, color: 'yellow' },
                  { id: 'business', title: 'Lógica de Negócio', desc: '122 regras documentadas', icon: '⚙️', pages: 40, color: 'red' },
                  { id: 'integrations', title: 'Integrações Externas', desc: 'CNOUA, SIPUA, SIMDA', icon: '🔗', pages: 25, color: 'indigo' },
                  { id: 'migration', title: 'Guia de Migração', desc: 'Estratégia para .NET 9.0', icon: '🚀', pages: 30, color: 'pink' }
                ].map((doc) => (
                  <div
                    key={doc.id}
                    onClick={() => setActiveTab(doc.id)}
                    className={`flex items-center gap-4 p-4 border-2 border-${doc.color}-200 rounded-xl hover:border-${doc.color}-500 hover:bg-${doc.color}-50 transition-all cursor-pointer group transform hover:scale-105`}
                  >
                    <div className="text-5xl group-hover:scale-110 transition-transform">{doc.icon}</div>
                    <div className="flex-1">
                      <h4 className="font-bold text-gray-900 group-hover:text-blue-700">{doc.title}</h4>
                      <p className="text-sm text-gray-600 mt-1">{doc.desc}</p>
                      <div className="flex items-center gap-2 mt-2">
                        <Badge className={`bg-${doc.color}-100 text-${doc.color}-800`}>{doc.pages} páginas</Badge>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* EXECUTIVE SUMMARY TAB */}
        <TabsContent value="executive" className="space-y-6">
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-purple-600 to-purple-700 text-white">
              <CardTitle className="text-2xl">📊 Sumário Executivo</CardTitle>
              <p className="text-purple-100 mt-2">Visão Executiva do Sistema, Objetivos e Indicadores Estratégicos</p>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="space-y-8">
                {/* System Identification */}
                <div>
                  <h3 className="text-2xl font-bold text-purple-900 mb-4 flex items-center gap-2">
                    <Server className="w-6 h-6" />
                    Identificação do Sistema
                  </h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="bg-purple-50 p-4 rounded-lg">
                      <div className="text-sm text-purple-600 font-medium mb-1">ID do Programa</div>
                      <div className="text-xl font-bold text-purple-900">#SIWEA-V116.esf</div>
                    </div>
                    <div className="bg-purple-50 p-4 rounded-lg">
                      <div className="text-sm text-purple-600 font-medium mb-1">Função Principal</div>
                      <div className="text-xl font-bold text-purple-900">Autorização de Pagamentos</div>
                    </div>
                    <div className="bg-purple-50 p-4 rounded-lg">
                      <div className="text-sm text-purple-600 font-medium mb-1">Plataforma</div>
                      <div className="text-xl font-bold text-purple-900">IBM Mainframe z/OS</div>
                    </div>
                    <div className="bg-purple-50 p-4 rounded-lg">
                      <div className="text-sm text-purple-600 font-medium mb-1">Linguagem</div>
                      <div className="text-xl font-bold text-purple-900">COBOL/EZEE 4.40</div>
                    </div>
                    <div className="bg-purple-50 p-4 rounded-lg">
                      <div className="text-sm text-purple-600 font-medium mb-1">Data de Criação</div>
                      <div className="text-xl font-bold text-purple-900">Outubro de 1989</div>
                    </div>
                    <div className="bg-purple-50 p-4 rounded-lg">
                      <div className="text-sm text-purple-600 font-medium mb-1">Status Atual</div>
                      <div className="text-xl font-bold text-green-600 flex items-center gap-2">
                        <CheckCircle className="w-5 h-5" />
                        Em Produção (35+ anos)
                      </div>
                    </div>
                  </div>
                </div>

                {/* Business Processes */}
                <div>
                  <h3 className="text-2xl font-bold text-purple-900 mb-4">Processos de Negócio Suportados</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {[
                      {
                        title: '1. Busca e Localização',
                        items: ['Por protocolo, sinistro ou líder', 'Validação em DB2', 'Resposta < 3 segundos'],
                        icon: '🔍',
                        color: 'blue'
                      },
                      {
                        title: '2. Autorização de Pagamento',
                        items: ['5 tipos configuráveis', '100+ validações automáticas', 'Conversão para BTNF'],
                        icon: '💰',
                        color: 'green'
                      },
                      {
                        title: '3. Gestão de Workflow',
                        items: ['8 fases de processamento', 'Transições automáticas', 'Controle de SLA'],
                        icon: '📋',
                        color: 'yellow'
                      },
                      {
                        title: '4. Integração Externa',
                        items: ['CNOUA (Consórcio)', 'SIPUA (EFP)', 'SIMDA (HB)'],
                        icon: '🔗',
                        color: 'purple'
                      }
                    ].map((process, idx) => (
                      <div key={idx} className="border-2 border-purple-200 rounded-lg p-4 hover:border-purple-500 transition-colors">
                        <div className="flex items-center gap-3 mb-3">
                          <span className="text-3xl">{process.icon}</span>
                          <h4 className="font-bold text-lg text-purple-900">{process.title}</h4>
                        </div>
                        <ul className="space-y-2">
                          {process.items.map((item, i) => (
                            <li key={i} className="flex items-start gap-2 text-sm text-gray-700">
                              <CheckCircle className="w-4 h-4 text-green-500 mt-0.5 flex-shrink-0" />
                              <span>{item}</span>
                            </li>
                          ))}
                        </ul>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* ARCHITECTURE TAB */}
        <TabsContent value="architecture" className="space-y-6">
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-green-600 to-green-700 text-white">
              <CardTitle className="text-2xl">🏗️ Arquitetura Técnica</CardTitle>
              <p className="text-green-100 mt-2">Arquitetura em 3 Camadas, Infraestrutura e Deployment</p>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="space-y-8">
                {/* Architecture Layers */}
                <div>
                  <h3 className="text-2xl font-bold text-green-900 mb-4">Arquitetura em 3 Camadas</h3>
                  <div className="space-y-4">
                    {[
                      {
                        layer: 'Camada de Apresentação',
                        tech: 'Terminal IBM 3270 / Emulador',
                        desc: 'Interface de usuário com terminais 3270',
                        components: ['SI11M010 - Tela de Busca', 'SIHM020 - Tela de Autorização'],
                        color: 'blue',
                        icon: '🖥️'
                      },
                      {
                        layer: 'Camada de Negócio',
                        tech: 'IBM CICS Transaction Server',
                        desc: 'Processamento de regras de negócio e lógica',
                        components: ['SIWEA-V116 (Programa Principal)', '100+ Regras de Validação', 'Gestão de Workflow'],
                        color: 'green',
                        icon: '⚙️'
                      },
                      {
                        layer: 'Camada de Dados',
                        tech: 'IBM DB2 for z/OS',
                        desc: 'Persistência e gerenciamento de dados',
                        components: ['13 Entidades DB2', 'TMESTSIN, THISTSIN, TGERAMO', 'Controle Transacional'],
                        color: 'purple',
                        icon: '💾'
                      }
                    ].map((layer, idx) => (
                      <div key={idx} className={`border-l-4 border-${layer.color}-500 bg-${layer.color}-50 p-6 rounded-r-lg hover:shadow-lg transition-shadow`}>
                        <div className="flex items-center gap-3 mb-3">
                          <span className="text-4xl">{layer.icon}</span>
                          <div>
                            <h4 className="text-xl font-bold text-gray-900">{layer.layer}</h4>
                            <p className="text-sm text-gray-600">{layer.tech}</p>
                          </div>
                        </div>
                        <p className="text-gray-700 mb-4">{layer.desc}</p>
                        <div className="flex flex-wrap gap-2">
                          {layer.components.map((comp, i) => (
                            <Badge key={i} className="bg-white text-gray-800 border border-gray-300">
                              {comp}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Architecture Characteristics */}
                <div>
                  <h3 className="text-2xl font-bold text-green-900 mb-4">Características Arquiteturais</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {[
                      { label: 'Padrão Arquitetural', value: '3-Tier (Três Camadas)', icon: '🏛️' },
                      { label: 'Modelo de Processamento', value: 'OLTP (Online Transaction)', icon: '⚡' },
                      { label: 'Paradigma', value: 'Procedural / Batch', icon: '📊' },
                      { label: 'Acoplamento', value: 'Monolítico Acoplado', icon: '🔗' },
                      { label: 'Estado', value: 'Stateless entre transações', icon: '💭' },
                      { label: 'Comunicação', value: 'Síncrona (Request/Response)', icon: '↔️' },
                      { label: 'Deployment', value: 'Mainframe Centralizado', icon: '🖥️' },
                      { label: 'Escalabilidade', value: 'Vertical (Hardware)', icon: '📈' }
                    ].map((char, idx) => (
                      <div key={idx} className="bg-green-50 p-4 rounded-lg hover:bg-green-100 transition-colors">
                        <div className="flex items-center gap-2 mb-2">
                          <span className="text-2xl">{char.icon}</span>
                          <div className="text-sm text-green-600 font-medium">{char.label}</div>
                        </div>
                        <div className="text-lg font-bold text-green-900">{char.value}</div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* DATABASE TAB */}
        <TabsContent value="database" className="space-y-6">
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-yellow-600 to-yellow-700 text-white">
              <CardTitle className="text-2xl">💾 Modelo de Banco de Dados</CardTitle>
              <p className="text-yellow-100 mt-2">13 Entidades, Relacionamentos e Estrutura de Dados</p>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="space-y-8">
                <div>
                  <h3 className="text-2xl font-bold text-yellow-900 mb-4">Entidades Principais (DB2)</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                    {[
                      { name: 'TMESTSIN', desc: 'Mestre de Sinistros', records: '2.5M+', icon: '📋' },
                      { name: 'THISTSIN', desc: 'Histórico de Pagamentos', records: '8M+', icon: '📜' },
                      { name: 'TGERAMO', desc: 'Gerências e Ramos', records: '500', icon: '🏢' },
                      { name: 'TGEUNIMO', desc: 'Unidades Monetárias', records: '100', icon: '💱' },
                      { name: 'TSISTEMA', desc: 'Controle do Sistema', records: '1', icon: '⚙️' },
                      { name: 'TAPOLICE', desc: 'Apólices e Segurados', records: '5M+', icon: '📄' },
                      { name: 'SI_ACOMPANHA_SINI', desc: 'Acompanhamento', records: '10M+', icon: '📊' },
                      { name: 'SI_SINISTRO_FASE', desc: 'Fases de Processamento', records: '15M+', icon: '🔄' },
                      { name: 'SI_REL_FASE_EVENTO', desc: 'Relacionamento Fases', records: '50', icon: '🔗' }
                    ].map((entity, idx) => (
                      <div key={idx} className="bg-yellow-50 border-2 border-yellow-200 rounded-lg p-4 hover:border-yellow-500 hover:shadow-lg transition-all">
                        <div className="flex items-center gap-3 mb-2">
                          <span className="text-3xl">{entity.icon}</span>
                          <div>
                            <h4 className="font-mono text-sm font-bold text-yellow-900">{entity.name}</h4>
                            <p className="text-xs text-yellow-700">{entity.desc}</p>
                          </div>
                        </div>
                        <div className="mt-3 pt-3 border-t border-yellow-200">
                          <div className="text-2xl font-black text-yellow-900">{entity.records}</div>
                          <div className="text-xs text-yellow-600">registros</div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* BUSINESS RULES TAB */}
        <TabsContent value="business" className="space-y-6">
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-red-600 to-red-700 text-white">
              <CardTitle className="text-2xl">⚙️ Regras de Negócio</CardTitle>
              <p className="text-red-100 mt-2">122 Regras Documentadas e Categorizadas</p>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="space-y-8">
                <div className="bg-red-50 border-l-4 border-red-500 p-6 rounded-r-lg">
                  <h3 className="text-2xl font-bold text-red-900 mb-4">Categorias de Regras</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {[
                      { category: 'Validação de Dados', count: 35, icon: '✅', color: 'green' },
                      { category: 'Cálculos Financeiros', count: 28, icon: '💰', color: 'yellow' },
                      { category: 'Controle de Workflow', count: 22, icon: '🔄', color: 'blue' },
                      { category: 'Integrações Externas', count: 18, icon: '🔗', color: 'purple' },
                      { category: 'Auditoria e Segurança', count: 12, icon: '🔒', color: 'gray' },
                      { category: 'Relatórios e Compliance', count: 7, icon: '📊', color: 'indigo' }
                    ].map((rule, idx) => (
                      <div key={idx} className="bg-white border-2 border-red-200 rounded-lg p-4 hover:border-red-500 transition-colors">
                        <div className="flex items-center justify-between mb-2">
                          <div className="flex items-center gap-3">
                            <span className="text-3xl">{rule.icon}</span>
                            <h4 className="font-bold text-gray-900">{rule.category}</h4>
                          </div>
                          <Badge className="bg-red-600 text-white text-lg px-3 py-1">{rule.count}</Badge>
                        </div>
                        <Progress value={(rule.count / 122) * 100} className="h-2 mt-2" />
                      </div>
                    ))}
                  </div>
                </div>

                <div>
                  <h3 className="text-2xl font-bold text-red-900 mb-4">Regras Críticas (Exemplos)</h3>
                  <div className="space-y-3">
                    {[
                      { id: 'BR-001', desc: 'Tipo de pagamento deve ser 1, 2, 3, 4 ou 5', tier: 'Crítico' },
                      { id: 'BR-015', desc: 'Beneficiário obrigatório se tipo de seguro != 0', tier: 'Crítico' },
                      { id: 'BR-042', desc: 'Conversão BTNF deve usar taxa do dia da autorização', tier: 'Crítico' },
                      { id: 'BR-073', desc: 'Produtos 6814, 7701, 7709 requerem validação CNOUA', tier: 'Alto' },
                      { id: 'BR-088', desc: 'Rollback completo em caso de falha de integração', tier: 'Crítico' }
                    ].map((rule, idx) => (
                      <div key={idx} className="flex items-start gap-4 bg-white border border-red-200 rounded-lg p-4 hover:shadow-md transition-shadow">
                        <Badge className={rule.tier === 'Crítico' ? 'bg-red-600 text-white' : 'bg-orange-500 text-white'}>
                          {rule.id}
                        </Badge>
                        <div className="flex-1">
                          <p className="text-gray-800">{rule.desc}</p>
                          <Badge className="mt-2 bg-gray-200 text-gray-800 text-xs">{rule.tier}</Badge>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* INTEGRATIONS TAB */}
        <TabsContent value="integrations" className="space-y-6">
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-indigo-600 to-indigo-700 text-white">
              <CardTitle className="text-2xl">🔗 Integrações Externas</CardTitle>
              <p className="text-indigo-100 mt-2">CNOUA, SIPUA, SIMDA - Protocolos e Especificações</p>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="space-y-6">
                {[
                  {
                    name: 'CNOUA',
                    fullName: 'Consórcio Nacional de Ouvidorias de Universidades e Assistências',
                    purpose: 'Validação de produtos de consórcio',
                    products: ['6814', '7701', '7709'],
                    protocol: 'SOAP/XML',
                    timeout: '30s',
                    fallback: 'Operação offline permitida',
                    icon: '🏦',
                    color: 'blue'
                  },
                  {
                    name: 'SIPUA',
                    fullName: 'Sistema Integrado de Planos de Unidades Assistenciais',
                    purpose: 'Validação de contratos EFP',
                    products: ['EFP (Enhanced Family Protection)'],
                    protocol: 'REST/JSON',
                    timeout: '20s',
                    fallback: 'Retry automático 3x',
                    icon: '🏥',
                    color: 'green'
                  },
                  {
                    name: 'SIMDA',
                    fullName: 'Sistema Integrado de Monitoramento de Dados Assistenciais',
                    purpose: 'Validação de contratos HB',
                    products: ['HB (Health Benefits)'],
                    protocol: 'REST/JSON',
                    timeout: '25s',
                    fallback: 'Cache local (24h)',
                    icon: '⚕️',
                    color: 'purple'
                  }
                ].map((integration, idx) => (
                  <Card key={idx} className={`border-2 border-${integration.color}-300 shadow-lg hover:shadow-xl transition-shadow`}>
                    <CardHeader className={`bg-${integration.color}-50`}>
                      <div className="flex items-center gap-4">
                        <div className="text-5xl">{integration.icon}</div>
                        <div>
                          <CardTitle className="text-2xl font-black text-gray-900">{integration.name}</CardTitle>
                          <p className="text-sm text-gray-600 mt-1">{integration.fullName}</p>
                        </div>
                      </div>
                    </CardHeader>
                    <CardContent className="pt-6">
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                          <h4 className="font-semibold text-gray-700 mb-2">Propósito</h4>
                          <p className="text-gray-800">{integration.purpose}</p>
                        </div>
                        <div>
                          <h4 className="font-semibold text-gray-700 mb-2">Protocolo</h4>
                          <Badge className={`bg-${integration.color}-600 text-white`}>{integration.protocol}</Badge>
                        </div>
                        <div>
                          <h4 className="font-semibold text-gray-700 mb-2">Produtos Suportados</h4>
                          <div className="flex flex-wrap gap-2">
                            {integration.products.map((prod, i) => (
                              <Badge key={i} className="bg-gray-200 text-gray-800">{prod}</Badge>
                            ))}
                          </div>
                        </div>
                        <div>
                          <h4 className="font-semibold text-gray-700 mb-2">Timeout</h4>
                          <p className="text-2xl font-bold text-gray-900">{integration.timeout}</p>
                        </div>
                        <div className="md:col-span-2">
                          <h4 className="font-semibold text-gray-700 mb-2">Fallback Strategy</h4>
                          <div className="bg-yellow-50 border-l-4 border-yellow-500 p-3 rounded-r">
                            <p className="text-gray-800">{integration.fallback}</p>
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* MIGRATION TAB */}
        <TabsContent value="migration" className="space-y-6">
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-pink-600 to-pink-700 text-white">
              <CardTitle className="text-2xl">🚀 Guia de Migração</CardTitle>
              <p className="text-pink-100 mt-2">Estratégia Completa para Migração .NET 9.0</p>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="space-y-8">
                <div>
                  <h3 className="text-2xl font-bold text-pink-900 mb-4">Fases da Migração</h3>
                  <div className="space-y-4">
                    {migrationTimeline.map((phase, idx) => (
                      <div key={idx} className="border-2 border-pink-200 rounded-lg p-4 hover:border-pink-500 transition-colors">
                        <div className="flex items-center justify-between mb-3">
                          <div>
                            <h4 className="text-lg font-bold text-pink-900">{phase.phase}</h4>
                            <p className="text-sm text-gray-600">Duração: {phase.duration}</p>
                          </div>
                          <Badge className={
                            phase.status === 'Concluído' ? 'bg-green-600 text-white' :
                            phase.status === 'Em Progresso' ? 'bg-blue-600 text-white' :
                            'bg-gray-400 text-white'
                          }>
                            {phase.status}
                          </Badge>
                        </div>
                        <ul className="space-y-2 mb-3">
                          {phase.tasks.map((task, i) => (
                            <li key={i} className="flex items-start gap-2 text-sm text-gray-700">
                              <CheckCircle className="w-4 h-4 text-pink-500 mt-0.5 flex-shrink-0" />
                              <span>{task}</span>
                            </li>
                          ))}
                        </ul>
                        <div className="space-y-2">
                          <div className="flex justify-between text-sm">
                            <span className="text-gray-600">Progresso</span>
                            <span className="font-bold text-pink-900">{phase.progress}%</span>
                          </div>
                          <Progress value={phase.progress} className="h-2" />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* FUNCTION POINTS TAB */}
        <TabsContent value="functionPoints" className="space-y-6">
          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-slate-700 to-slate-800 text-white">
              <CardTitle className="text-2xl flex items-center gap-2">
                <PieChart className="w-5 h-5" />
                Pontos de Função · Visão Geral
              </CardTitle>
              <p className="text-slate-200 mt-2">
                Dados consolidados do documento <strong>FUNCIONALIDADES-E-PONTOS-DE-FUNCAO.md</strong> (27/10/2025).
              </p>
            </CardHeader>
            <CardContent className="pt-6 space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                {functionPointCards.map((card) => {
                  const Icon = card.icon;
                  return (
                    <div
                      key={card.title}
                      className={`rounded-2xl p-5 bg-gradient-to-br ${card.gradient} text-white shadow-lg flex flex-col gap-3`}
                    >
                      <div className="flex items-center justify-between">
                        <span className="text-sm uppercase tracking-wide text-white/70">{card.title}</span>
                        <Icon className="w-6 h-6 text-white/80" />
                      </div>
                      <div className="text-3xl font-black">{card.value}</div>
                      <p className="text-sm text-white/80">{card.helper}</p>
                      <div className="text-xs font-semibold uppercase text-white/80">{card.highlight}</div>
                    </div>
                  );
                })}
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="bg-slate-50 border border-slate-200 rounded-xl p-4">
                  <div className="flex items-center gap-2 text-slate-700 font-semibold mb-2">
                    <Layers className="w-4 h-4" />
                    Ajuste de Migração
                  </div>
                  <p className="text-sm text-slate-600 leading-relaxed">
                    Os PFs documentados (280 na Fase 1 e 100 na Fase 2) foram proporcionados para atingir 250 PF base
                    (180 + 70), preservando a relevância relativa das funcionalidades e adicionando{' '}
                    {HOMOLOGATION_MARGIN_PERCENT}% de margem para homologação.
                  </p>
                </div>
                <div className="bg-slate-50 border border-slate-200 rounded-xl p-4">
                  <div className="flex items-center gap-2 text-slate-700 font-semibold mb-2">
                    <ClipboardList className="w-4 h-4" />
                    Critérios de aceitação essenciais
                  </div>
                  <ul className="text-sm text-slate-600 space-y-1">
                    <li>• Paridade funcional 100% validada pelos SMEs em 4 semanas de shadow mode intensivo.</li>
                    <li>• Performance alvo: buscas &lt; 3s, histórico &lt; 500ms, autorização &lt; 90s.</li>
                    <li>• Cobertura de testes &gt; 90% em regras críticas e conversões BTNF sem divergências.</li>
                  </ul>
                </div>
              </div>
            </CardContent>
          </Card>

          <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
            <Card className="shadow-xl">
              <CardHeader className="bg-gradient-to-r from-blue-600 to-blue-700 text-white">
                <CardTitle className="text-xl flex items-center gap-2">
                  <Layers className="w-5 h-5" />
                  Fase 1 · Migração Core ({formatNumber(phase1TotalBase)} PF base)
                </CardTitle>
                <p className="text-blue-100 mt-1">Prioridades críticas para substituir o VisualAge sem regressões.</p>
              </CardHeader>
              <CardContent className="pt-6">
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="text-left text-slate-500 uppercase text-xs">
                        <th className="pb-2">Funcionalidade</th>
                        <th className="pb-2 text-center">PF Documento</th>
                        <th className="pb-2 text-center">PF Base</th>
                        <th className="pb-2 text-center">PF Homolog.</th>
                        <th className="pb-2 text-center">% Fase</th>
                        <th className="pb-2 text-center">Prioridade</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200">
                      {phase1Computed.map((fn) => {
                        const share = computePercent(fn.basePf, phase1TotalBase);
                        return (
                          <tr key={fn.id} className="hover:bg-slate-50">
                            <td className="py-3 pr-3">
                              <div className="font-semibold text-slate-800">
                                {fn.id} · {fn.title}
                              </div>
                              <p className="text-xs text-slate-600 mt-1">{fn.description}</p>
                            </td>
                            <td className="py-3 text-center font-bold text-slate-800">
                              {formatNumber(fn.docPf)}
                            </td>
                            <td className="py-3 text-center font-bold text-slate-800">
                              {formatNumber(fn.basePf)}
                            </td>
                            <td className="py-3 text-center font-bold text-slate-800">
                              {formatNumber(fn.pfWithMargin)}
                            </td>
                            <td className="py-3 text-center text-slate-700">
                              {formatPercent(share)}
                            </td>
                            <td className="py-3 text-center">
                              <Badge className={`text-xs ${fn.badgeClass}`}>{fn.priority}</Badge>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>

            <Card className="shadow-xl">
              <CardHeader className="bg-gradient-to-r from-purple-600 to-purple-700 text-white">
                <CardTitle className="text-xl flex items-center gap-2">
                  <Target className="w-5 h-5" />
                  Fase 2 · Melhorias e Modernização ({formatNumber(phase2TotalBase)} PF base)
                </CardTitle>
                <p className="text-purple-100 mt-1">Incrementos pós-go-live para analytics, UX e governança.</p>
              </CardHeader>
              <CardContent className="pt-6">
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="text-left text-slate-500 uppercase text-xs">
                        <th className="pb-2">Funcionalidade</th>
                        <th className="pb-2 text-center">PF Documento</th>
                        <th className="pb-2 text-center">PF Base</th>
                        <th className="pb-2 text-center">PF Homolog.</th>
                        <th className="pb-2 text-center">% Fase</th>
                        <th className="pb-2 text-center">Prioridade</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-slate-200">
                      {phase2Computed.map((fn) => {
                        const share = computePercent(fn.basePf, phase2TotalBase);
                        return (
                          <tr key={fn.id} className="hover:bg-slate-50">
                            <td className="py-3 pr-3">
                              <div className="font-semibold text-slate-800">
                                {fn.id} · {fn.title}
                              </div>
                              <p className="text-xs text-slate-600 mt-1">{fn.description}</p>
                            </td>
                            <td className="py-3 text-center font-bold text-slate-800">
                              {formatNumber(fn.docPf)}
                            </td>
                            <td className="py-3 text-center font-bold text-slate-800">
                              {formatNumber(fn.basePf)}
                            </td>
                            <td className="py-3 text-center font-bold text-slate-800">
                              {formatNumber(fn.pfWithMargin)}
                            </td>
                            <td className="py-3 text-center text-slate-700">
                              {formatPercent(share)}
                            </td>
                            <td className="py-3 text-center">
                              <Badge className={`text-xs ${fn.badgeClass}`}>{fn.priority}</Badge>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          </div>

          <Card className="shadow-xl">
            <CardHeader className="bg-gradient-to-r from-slate-600 to-slate-700 text-white">
              <CardTitle className="text-xl flex items-center gap-2">
                <Calculator className="w-5 h-5" />
                Relatório Consolidado da Migração
              </CardTitle>
              <p className="text-slate-200 mt-1">Resumo executivo com PFs, esforço, custos e benefícios esperados.</p>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <div className="bg-slate-50 border border-slate-200 rounded-xl p-4">
                  <div className="text-xs text-slate-500 uppercase tracking-wide">PF Totais (base)</div>
                  <div className="text-2xl font-black text-slate-800 mt-1">
                    {formatNumber(totalBasePf)} PF
                  </div>
                  <div className="text-sm text-slate-600">
                    Fase 1: {formatNumber(phase1TotalBase)} · Fase 2: {formatNumber(phase2TotalBase)}
                  </div>
                  <div className="text-xs text-slate-500 mt-1">
                    +{HOMOLOGATION_MARGIN_PERCENT}% homologação → {formatNumber(totalWithMargin)} PF
                  </div>
                </div>
                <div className="bg-slate-50 border border-slate-200 rounded-xl p-4">
                  <div className="text-xs text-slate-500 uppercase tracking-wide">Esforço</div>
                  <div className="text-2xl font-black text-slate-800 mt-1">16 pessoa-mês</div>
                  <div className="text-sm text-slate-600">Equipe full-stack dedicada</div>
                </div>
                <div className="bg-slate-50 border border-slate-200 rounded-xl p-4">
                  <div className="text-xs text-slate-500 uppercase tracking-wide">Investimento</div>
                  <div className="text-2xl font-black text-slate-800 mt-1">R$ 520 mil</div>
                  <div className="text-sm text-slate-600">Inclui contingência de 15%</div>
                </div>
              </div>
              <ul className="space-y-3 text-sm text-slate-700 leading-relaxed">
                {migrationReportHighlights.map((item, idx) => (
                  <li key={idx} className="flex gap-3">
                    <span className="text-slate-500 mt-1">•</span>
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default LegacySystemDocsPage;
