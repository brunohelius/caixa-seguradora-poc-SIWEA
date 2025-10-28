/**
 * ReportsPage Component
 * Comprehensive reports dashboard for claims and system analytics
 * WITH MOCK DATA AND DOWNLOAD FUNCTIONALITY
 */

import React, { useState } from 'react';
import { BarChart, FileText, TrendingUp, Users, Calendar, Download, Filter, CheckCircle } from 'lucide-react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';

// Mock data for reports
interface ReportData {
  id: string;
  name: string;
  type: string;
  date: string;
  size: string;
  records: number;
  data?: any[];
}

const generateMockData = (reportType: string, recordCount: number = 100): any[] => {
  const data = [];
  for (let i = 0; i < recordCount; i++) {
    if (reportType === 'claims') {
      data.push({
        sinistro: `${Math.floor(Math.random() * 9000) + 1000}/${Math.floor(Math.random() * 90) + 10}/${Math.floor(Math.random() * 90000) + 10000}`,
        data: new Date(2025, Math.floor(Math.random() * 10), Math.floor(Math.random() * 28) + 1).toLocaleDateString('pt-BR'),
        tipo: ['Morte', 'Invalidez', 'Doença Grave', 'Desemprego'][Math.floor(Math.random() * 4)],
        valor: (Math.random() * 50000 + 5000).toFixed(2),
        status: ['Aberto', 'Em Análise', 'Aprovado', 'Pago'][Math.floor(Math.random() * 4)]
      });
    } else if (reportType === 'authorizations') {
      data.push({
        autorizacao: `AUTH${(i + 1).toString().padStart(6, '0')}`,
        data: new Date(2025, 9, Math.floor(Math.random() * 28) + 1).toLocaleDateString('pt-BR'),
        usuario: `USER${Math.floor(Math.random() * 50) + 1}`,
        sinistro: `${Math.floor(Math.random() * 9000) + 1000}/${Math.floor(Math.random() * 90) + 10}/${Math.floor(Math.random() * 90000) + 10000}`,
        valor: (Math.random() * 30000 + 1000).toFixed(2),
        tempo: `${Math.floor(Math.random() * 120) + 30}s`
      });
    } else if (reportType === 'payments') {
      data.push({
        pagamento: `PAG${(i + 1).toString().padStart(6, '0')}`,
        data: new Date(2025, 9, Math.floor(Math.random() * 28) + 1).toLocaleDateString('pt-BR'),
        beneficiario: `Beneficiário ${i + 1}`,
        cpf: `${Math.floor(Math.random() * 90000000000) + 10000000000}`.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4'),
        valorPrincipal: (Math.random() * 20000 + 1000).toFixed(2),
        correcao: (Math.random() * 500).toFixed(2),
        total: (Math.random() * 20500 + 1000).toFixed(2)
      });
    }
  }
  return data;
};

const downloadCSV = (data: any[], filename: string) => {
  if (!data || data.length === 0) return;

  const headers = Object.keys(data[0]);
  const csvContent = [
    headers.join(','),
    ...data.map(row => headers.map(header => `"${row[header]}"`).join(','))
  ].join('\n');

  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = `${filename}.csv`;
  link.click();
};

const downloadExcel = (data: any[], filename: string) => {
  // Simulated Excel download (would use library like xlsx in production)
  const csvContent = data.map(row => Object.values(row).join('\t')).join('\n');
  const blob = new Blob([csvContent], { type: 'application/vnd.ms-excel' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = `${filename}.xls`;
  link.click();
};

const downloadPDF = (reportName: string, data: any[]) => {
  // Simulated PDF download (would use library like jsPDF in production)
  toast.info('Download PDF', {
    description: `Gerando PDF: ${reportName} (${data.length} registros)...`,
    duration: 3000,
  });

  setTimeout(() => {
    toast.success('PDF Gerado!', {
      description: `${reportName} foi baixado com sucesso.`,
      duration: 3000,
    });
  }, 1500);
};

export const ReportsPage: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview');
  const [generatingReport, setGeneratingReport] = useState<string | null>(null);

  // Mock data for recent reports
  const [recentReports, setRecentReports] = useState<ReportData[]>([
    {
      id: '1',
      name: 'Sinistros por Período - Outubro 2025',
      type: 'claims',
      date: '2025-10-27',
      size: '2.4 MB',
      records: 1523,
      data: generateMockData('claims', 1523)
    },
    {
      id: '2',
      name: 'Autorizações por Usuário - Semana 43',
      type: 'authorizations',
      date: '2025-10-20',
      size: '1.1 MB',
      records: 892,
      data: generateMockData('authorizations', 892)
    },
    {
      id: '3',
      name: 'Valores Pagos - 3º Trimestre',
      type: 'payments',
      date: '2025-09-30',
      size: '3.8 MB',
      records: 4521,
      data: generateMockData('payments', 4521)
    },
  ]);

  const overviewStats = {
    totalClaims: 12458,
    totalAuthorizations: 8942,
    totalPaid: 45678912.50,
    averageProcessingTime: 2.3,
  };

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  const formatNumber = (value: number): string => {
    return new Intl.NumberFormat('pt-BR').format(value);
  };

  const getReportTypeColor = (type: string): string => {
    switch (type) {
      case 'claims': return 'badge-blue';
      case 'authorizations': return 'badge-yellow';
      case 'payments': return 'badge-success';
      default: return 'badge-blue';
    }
  };

  const getReportTypeName = (type: string): string => {
    switch (type) {
      case 'claims': return 'Sinistros';
      case 'authorizations': return 'Autorizações';
      case 'payments': return 'Pagamentos';
      default: return type;
    }
  };

  const handleDownload = (report: ReportData, format: 'csv' | 'excel' | 'pdf') => {
    const filename = report.name.replace(/\s+/g, '_');

    switch (format) {
      case 'csv':
        downloadCSV(report.data || [], filename);
        toast.success('Download CSV', {
          description: `${report.name} baixado com sucesso!`,
          duration: 3000,
        });
        break;
      case 'excel':
        downloadExcel(report.data || [], filename);
        toast.success('Download Excel', {
          description: `${report.name} baixado com sucesso!`,
          duration: 3000,
        });
        break;
      case 'pdf':
        downloadPDF(report.name, report.data || []);
        break;
    }
  };

  const handleGenerateReport = (reportName: string, reportType: string, recordCount: number = 1000) => {
    setGeneratingReport(reportName);

    toast.info('Gerando Relatório', {
      description: `Processando ${recordCount} registros...`,
      duration: 2000,
    });

    setTimeout(() => {
      const newReport: ReportData = {
        id: Date.now().toString(),
        name: reportName,
        type: reportType,
        date: new Date().toISOString().split('T')[0],
        size: `${(recordCount * 0.0015).toFixed(1)} MB`,
        records: recordCount,
        data: generateMockData(reportType, recordCount)
      };

      setRecentReports(prev => [newReport, ...prev]);
      setGeneratingReport(null);

      toast.success('Relatório Gerado!', {
        description: `${reportName} com ${recordCount} registros está pronto.`,
        duration: 3000,
        action: {
          label: 'Baixar CSV',
          onClick: () => handleDownload(newReport, 'csv')
        }
      });
    }, 2000);
  };

  const handleDownloadAll = () => {
    toast.info('Download em Lote', {
      description: `Preparando ${recentReports.length} relatórios para download...`,
      duration: 3000,
    });

    setTimeout(() => {
      recentReports.forEach((report, index) => {
        setTimeout(() => {
          downloadCSV(report.data || [], report.name.replace(/\s+/g, '_'));
        }, index * 500);
      });

      toast.success('Downloads Completos!', {
        description: `${recentReports.length} relatórios baixados com sucesso.`,
        duration: 3000,
      });
    }, 1000);
  };

  return (
    <div className="container-modern py-8 fade-in">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Page Header */}
        <div className="card-modern">
          <div className="card-header bg-gradient-caixa text-white">
            <div className="flex items-center justify-between flex-wrap gap-4">
              <div className="flex items-center gap-3">
                <BarChart className="w-8 h-8" />
                <div>
                  <h1 className="text-3xl font-bold mb-2">Relatórios e Análises</h1>
                  <p className="text-blue-100 text-sm">
                    Gere relatórios detalhados e acompanhe indicadores do sistema
                  </p>
                </div>
              </div>
              <Button
                className="bg-white text-blue-700 hover:bg-blue-50"
                onClick={handleDownloadAll}
              >
                <Download className="w-4 h-4 mr-2" />
                Exportar Todos
              </Button>
            </div>
          </div>
        </div>

        {/* Overview Stats */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="card-modern">
            <div className="card-body">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600 font-semibold">Total de Sinistros</p>
                  <p className="text-3xl font-bold text-blue-900 mt-1">
                    {formatNumber(overviewStats.totalClaims)}
                  </p>
                </div>
                <FileText className="w-12 h-12 text-blue-200" />
              </div>
            </div>
          </div>

          <div className="card-modern">
            <div className="card-body">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600 font-semibold">Autorizações</p>
                  <p className="text-3xl font-bold text-yellow-700 mt-1">
                    {formatNumber(overviewStats.totalAuthorizations)}
                  </p>
                </div>
                <Users className="w-12 h-12 text-yellow-200" />
              </div>
            </div>
          </div>

          <div className="card-modern">
            <div className="card-body">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600 font-semibold">Total Pago</p>
                  <p className="text-2xl font-bold text-green-700 mt-1">
                    {formatCurrency(overviewStats.totalPaid)}
                  </p>
                </div>
                <TrendingUp className="w-12 h-12 text-green-200" />
              </div>
            </div>
          </div>

          <div className="card-modern">
            <div className="card-body">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-600 font-semibold">Tempo Médio (dias)</p>
                  <p className="text-3xl font-bold text-purple-700 mt-1">
                    {overviewStats.averageProcessingTime}
                  </p>
                </div>
                <Calendar className="w-12 h-12 text-purple-200" />
              </div>
            </div>
          </div>
        </div>

        {/* Reports Tabs */}
        <div className="card-modern">
          <Tabs value={activeTab} onValueChange={setActiveTab}>
            <div className="card-header border-b">
              <TabsList className="bg-transparent w-full justify-start gap-2 flex-wrap">
                <TabsTrigger value="overview" className="gap-2">
                  <BarChart className="w-4 h-4" />
                  Visão Geral
                </TabsTrigger>
                <TabsTrigger value="claims" className="gap-2">
                  <FileText className="w-4 h-4" />
                  Sinistros
                </TabsTrigger>
                <TabsTrigger value="authorizations" className="gap-2">
                  <Users className="w-4 h-4" />
                  Autorizações
                </TabsTrigger>
                <TabsTrigger value="payments" className="gap-2">
                  <TrendingUp className="w-4 h-4" />
                  Pagamentos
                </TabsTrigger>
              </TabsList>
            </div>

            <div className="card-body">
              {/* Overview Tab */}
              <TabsContent value="overview" className="space-y-6">
                <div>
                  <h3 className="text-xl font-bold mb-4 flex items-center gap-2">
                    <FileText className="w-5 h-5 text-blue-700" />
                    Relatórios Recentes
                  </h3>
                  <div className="space-y-3">
                    {recentReports.map((report) => (
                      <div
                        key={report.id}
                        className="flex items-center justify-between p-4 border rounded-lg hover:bg-gray-50 transition-colors"
                      >
                        <div className="flex items-center gap-4 flex-1">
                          <FileText className="w-8 h-8 text-gray-400" />
                          <div className="flex-1">
                            <h4 className="font-semibold text-gray-900">{report.name}</h4>
                            <div className="flex items-center gap-3 mt-1 flex-wrap">
                              <span className={`badge ${getReportTypeColor(report.type)}`}>
                                {getReportTypeName(report.type)}
                              </span>
                              <span className="text-sm text-gray-600">
                                {formatNumber(report.records)} registros
                              </span>
                              <span className="text-sm text-gray-600">{report.size}</span>
                              <span className="text-sm text-gray-600">
                                {new Date(report.date).toLocaleDateString('pt-BR')}
                              </span>
                            </div>
                          </div>
                        </div>
                        <div className="flex gap-2 ml-4 flex-shrink-0">
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleDownload(report, 'csv')}
                          >
                            CSV
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleDownload(report, 'excel')}
                          >
                            Excel
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleDownload(report, 'pdf')}
                          >
                            PDF
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </TabsContent>

              {/* Claims Reports Tab */}
              <TabsContent value="claims" className="space-y-6">
                <div>
                  <h3 className="text-xl font-bold mb-4">Relatórios de Sinistros</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Sinistros por Período</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Listagem completa de sinistros registrados em um período específico
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Sinistros por Período - ' + new Date().toLocaleDateString('pt-BR'), 'claims', 850)}
                          disabled={generatingReport === 'sinistros-periodo'}
                        >
                          {generatingReport === 'sinistros-periodo' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Sinistros por Ramo</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Análise de sinistros agrupados por ramo de seguro
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Sinistros por Ramo - ' + new Date().toLocaleDateString('pt-BR'), 'claims', 620)}
                          disabled={generatingReport === 'sinistros-ramo'}
                        >
                          {generatingReport === 'sinistros-ramo' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Sinistros por Situação</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Relatório de sinistros classificados por situação atual
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Sinistros por Situação - ' + new Date().toLocaleDateString('pt-BR'), 'claims', 740)}
                          disabled={generatingReport === 'sinistros-situacao'}
                        >
                          {generatingReport === 'sinistros-situacao' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Análise Comparativa</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Comparação de volumes e valores entre períodos
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Análise Comparativa - ' + new Date().toLocaleDateString('pt-BR'), 'claims', 950)}
                          disabled={generatingReport === 'analise-comparativa'}
                        >
                          {generatingReport === 'analise-comparativa' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>
                  </div>
                </div>
              </TabsContent>

              {/* Authorizations Reports Tab */}
              <TabsContent value="authorizations" className="space-y-6">
                <div>
                  <h3 className="text-xl font-bold mb-4">Relatórios de Autorizações</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Autorizações por Usuário</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Listagem de autorizações realizadas por cada usuário
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Autorizações por Usuário - ' + new Date().toLocaleDateString('pt-BR'), 'authorizations', 680)}
                          disabled={generatingReport === 'auth-usuario'}
                        >
                          {generatingReport === 'auth-usuario' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Performance de Autorizações</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Análise de tempo médio de processamento de autorizações
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Performance de Autorizações - ' + new Date().toLocaleDateString('pt-BR'), 'authorizations', 530)}
                          disabled={generatingReport === 'auth-performance'}
                        >
                          {generatingReport === 'auth-performance' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Autorizações Rejeitadas</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Relatório de autorizações que falharam com motivo da rejeição
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Autorizações Rejeitadas - ' + new Date().toLocaleDateString('pt-BR'), 'authorizations', 450)}
                          disabled={generatingReport === 'auth-rejeitadas'}
                        >
                          {generatingReport === 'auth-rejeitadas' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Auditoria de Operações</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Log completo de todas as operações do sistema
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Auditoria de Operações - ' + new Date().toLocaleDateString('pt-BR'), 'authorizations', 1200)}
                          disabled={generatingReport === 'auth-auditoria'}
                        >
                          {generatingReport === 'auth-auditoria' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>
                  </div>
                </div>
              </TabsContent>

              {/* Payments Reports Tab */}
              <TabsContent value="payments" className="space-y-6">
                <div>
                  <h3 className="text-xl font-bold mb-4">Relatórios de Pagamentos</h3>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Valores Pagos por Período</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Sumário de valores pagos em um período específico
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Valores Pagos por Período - ' + new Date().toLocaleDateString('pt-BR'), 'payments', 920)}
                          disabled={generatingReport === 'pay-periodo'}
                        >
                          {generatingReport === 'pay-periodo' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Pagamentos por Beneficiário</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Listagem de pagamentos agrupados por beneficiário
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Pagamentos por Beneficiário - ' + new Date().toLocaleDateString('pt-BR'), 'payments', 780)}
                          disabled={generatingReport === 'pay-beneficiario'}
                        >
                          {generatingReport === 'pay-beneficiario' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Conversão Monetária</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Análise de valores em BTNF e conversões aplicadas
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Conversão Monetária - ' + new Date().toLocaleDateString('pt-BR'), 'payments', 650)}
                          disabled={generatingReport === 'pay-conversao'}
                        >
                          {generatingReport === 'pay-conversao' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>

                    <div className="card-modern bg-gradient-caixa-light">
                      <div className="card-body">
                        <h4 className="font-semibold mb-2">Análise Financeira</h4>
                        <p className="text-sm text-gray-600 mb-4">
                          Relatório consolidado com totais e médias financeiras
                        </p>
                        <Button
                          className="w-full"
                          onClick={() => handleGenerateReport('Análise Financeira - ' + new Date().toLocaleDateString('pt-BR'), 'payments', 850)}
                          disabled={generatingReport === 'pay-financeira'}
                        >
                          {generatingReport === 'pay-financeira' ? (
                            <>
                              <div className="spinner mr-2"></div>
                              Gerando...
                            </>
                          ) : (
                            <>
                              <Filter className="w-4 h-4 mr-2" />
                              Gerar Relatório
                            </>
                          )}
                        </Button>
                      </div>
                    </div>
                  </div>
                </div>
              </TabsContent>
            </div>
          </Tabs>
        </div>

        {/* Help Card */}
        <div className="card-modern bg-gradient-caixa-light">
          <div className="card-header border-b-0">
            <h3 className="text-lg font-semibold">Sobre os Relatórios</h3>
          </div>
          <div className="card-body pt-0">
            <ul className="space-y-2 text-sm text-gray-700">
              <li className="flex items-start gap-2">
                <CheckCircle className="w-4 h-4 text-blue-700 mt-0.5 flex-shrink-0" />
                <span>
                  Todos os relatórios podem ser exportados nos formatos PDF, Excel e CSV
                </span>
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle className="w-4 h-4 text-blue-700 mt-0.5 flex-shrink-0" />
                <span>
                  Os dados são gerados com mock data para demonstração
                </span>
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle className="w-4 h-4 text-blue-700 mt-0.5 flex-shrink-0" />
                <span>
                  Relatórios gerados ficam disponíveis na aba "Visão Geral"
                </span>
              </li>
              <li className="flex items-start gap-2">
                <CheckCircle className="w-4 h-4 text-blue-700 mt-0.5 flex-shrink-0" />
                <span>
                  Clique em "Gerar Relatório" para criar um novo relatório com dados atualizados
                </span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ReportsPage;
