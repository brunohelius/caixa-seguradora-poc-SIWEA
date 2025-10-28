/**
 * ClaimHistoryPage Component
 * Dedicated page for viewing claim payment history
 * WITH MOCK DATA - FULLY FUNCTIONAL
 */

import React, { useState } from 'react';
import { SearchForm } from '../components/claims/SearchForm';
import ClaimHistoryComponent from '../components/claims/ClaimHistoryComponent';
import type { ClaimSearchCriteria, ClaimKey } from '../models/Claim';
import { AlertCircle, Info, History, FileText } from 'lucide-react';
import { toast } from 'sonner';

export const ClaimHistoryPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [claimKey, setClaimKey] = useState<ClaimKey | null>(null);
  const [claimInfo, setClaimInfo] = useState<any>(null);

  const handleSearch = async (criteria: ClaimSearchCriteria) => {
    setLoading(true);
    setError(null);
    setClaimKey(null);
    setClaimInfo(null);

    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 1000));

    try {
      // Mock data response
      const mockClaim = {
        tipseg: 1,
        orgsin: 101,
        rmosin: 2,
        numsin: criteria.searchType === 'protocolo' ? parseInt(criteria.protocol || '12345') : 98765,
        protocolo: criteria.protocol || '2025/001234',
        situacao: 'ATIVO',
        valorTotal: 125000.50,
        valorPago: 75000.00,
        saldoPendente: 50000.50,
        segurado: 'João da Silva Santos',
        cpf: '123.456.789-00',
        dataOcorrencia: '2024-10-15',
        dataAbertura: '2024-10-16'
      };

      setClaimKey({
        tipseg: mockClaim.tipseg,
        orgsin: mockClaim.orgsin,
        rmosin: mockClaim.rmosin,
        numsin: mockClaim.numsin,
      });
      setClaimInfo(mockClaim);

      toast.success('Sinistro encontrado com sucesso!');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao pesquisar sinistro');
      toast.error('Erro ao pesquisar sinistro');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container-modern py-8 fade-in">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Page Header */}
        <div className="card-modern">
          <div className="card-header bg-gradient-caixa text-white">
            <div className="flex items-center gap-3">
              <History className="w-8 h-8" />
              <div>
                <h1 className="text-3xl font-bold mb-2">Histórico de Sinistros</h1>
                <p className="text-blue-100 text-sm">
                  Consulte o histórico completo de autorizações de pagamento
                </p>
              </div>
            </div>
          </div>
        </div>

        {/* Search Card */}
        <div className="card-modern">
          <div className="card-header">
            <h2 className="text-xl font-bold flex items-center gap-2">
              <FileText className="w-5 h-5 text-blue-700" />
              Pesquisar Sinistro
            </h2>
          </div>
          <div className="card-body">
            <SearchForm onSearch={handleSearch} loading={loading} />

            {/* Error Alert */}
            {error && (
              <div className="alert alert-error mt-6 fade-in">
                <AlertCircle className="h-5 w-5 flex-shrink-0" />
                <div>
                  <strong className="font-semibold">Erro:</strong> {error}
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Claim Info Card (shown after successful search) */}
        {claimInfo && (
          <div className="card-modern bg-gradient-caixa-light fade-in">
            <div className="card-header border-b-0">
              <h3 className="text-lg font-semibold text-blue-900">Informações do Sinistro</h3>
            </div>
            <div className="card-body pt-0">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <p className="text-sm text-gray-600 font-semibold">Tipo de Seguro</p>
                  <p className="text-lg font-bold text-blue-900">{claimInfo.tipseg}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-semibold">Número do Sinistro</p>
                  <p className="text-lg font-bold text-blue-900">
                    {claimInfo.orgsin}/{claimInfo.rmosin}/{claimInfo.numsin}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-600 font-semibold">Situação</p>
                  <span className="badge badge-success mt-1 inline-block">Ativo</span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* History Results */}
        {claimKey && (
          <div className="card-modern fade-in">
            <div className="card-body">
              <ClaimHistoryComponent claimKey={claimKey} />
            </div>
          </div>
        )}

        {/* Instructions Card */}
        {!claimKey && (
          <div className="card-modern bg-gradient-caixa-light">
            <div className="card-header border-b-0">
              <div className="flex items-center gap-2">
                <Info className="h-5 w-5 text-caixa-blue-700" />
                <h3 className="text-lg font-semibold">Como usar</h3>
              </div>
            </div>
            <div className="card-body pt-0">
              <ul className="space-y-3 text-sm">
                <li className="flex items-start gap-2">
                  <span className="badge badge-blue flex-shrink-0">1</span>
                  <span className="text-gray-700">
                    Selecione o tipo de pesquisa (Protocolo, Sinistro ou Líder)
                  </span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="badge badge-blue flex-shrink-0">2</span>
                  <span className="text-gray-700">
                    Preencha os campos correspondentes ao tipo escolhido
                  </span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="badge badge-blue flex-shrink-0">3</span>
                  <span className="text-gray-700">
                    Clique em "Pesquisar" para visualizar o histórico completo de autorizações
                  </span>
                </li>
                <li className="flex items-start gap-2">
                  <span className="badge badge-blue flex-shrink-0">4</span>
                  <span className="text-gray-700">
                    O histórico exibe todas as operações realizadas, incluindo valores, datas e usuários responsáveis
                  </span>
                </li>
              </ul>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ClaimHistoryPage;
