/**
 * ClaimSearchPage Component
 * T044 [US1] - Main search page for claims
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { SearchForm } from '../components/claims/SearchForm';
import { searchClaim } from '../services/claimsApi';
import type { ClaimSearchCriteria } from '../models/Claim';
import { AlertCircle, Info } from 'lucide-react';

export const ClaimSearchPage: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSearch = async (criteria: ClaimSearchCriteria) => {
    setLoading(true);
    setError(null);

    try {
      const response = await searchClaim(criteria);

      if (response.sucesso && response.claim) {
        // Navigate to detail page with claim data
        navigate('/claims/detail', {
          state: { claim: response.claim },
        });
      } else {
        setError(response.mensagem || 'Sinistro não encontrado');
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao pesquisar sinistro');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container-modern py-8 fade-in">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Main Search Card */}
        <div className="card-modern">
          <div className="card-header bg-gradient-caixa text-white">
            <h2 className="text-2xl font-bold mb-2">Pesquisa de Sinistros</h2>
            <p className="text-blue-100 text-sm">
              Selecione o tipo de pesquisa e preencha os campos correspondentes.
            </p>
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

        {/* Instructions Card */}
        <div className="card-modern bg-gradient-caixa-light">
          <div className="card-header border-b-0">
            <div className="flex items-center gap-2">
              <Info className="h-5 w-5 text-caixa-blue-700" />
              <h3 className="text-lg font-semibold">Instruções de Pesquisa</h3>
            </div>
          </div>
          <div className="card-body pt-0">
            <ul className="space-y-3 text-sm">
              <li className="flex items-start gap-2">
                <span className="badge badge-blue flex-shrink-0">Protocolo</span>
                <span className="text-gray-700">Informe Fonte, Protocolo e DAC</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="badge badge-blue flex-shrink-0">Sinistro</span>
                <span className="text-gray-700">Informe Tipo de Seguro, Origem, Ramo e Número</span>
              </li>
              <li className="flex items-start gap-2">
                <span className="badge badge-blue flex-shrink-0">Líder</span>
                <span className="text-gray-700">Informe Código Líder e Sinistro Líder</span>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};
