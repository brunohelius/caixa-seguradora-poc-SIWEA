/**
 * ClaimSearchPage Component
 * T044 [US1] - Main search page for claims
 */

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { SearchForm } from '../components/claims/SearchForm';
import { searchClaim } from '../services/claimsApi';
import type { ClaimSearchCriteria } from '../models/Claim';

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
    <div className="container mt-4">
      <div className="row justify-content-center">
        <div className="col-lg-10">
          <div className="card shadow">
            <div className="card-header bg-primary text-white">
              <h3 className="mb-0">Pesquisa de Sinistros</h3>
            </div>
            <div className="card-body">
              <p className="text-muted mb-4">
                Selecione o tipo de pesquisa e preencha os campos correspondentes.
              </p>

              <SearchForm onSearch={handleSearch} loading={loading} />

              {error && (
                <div className="alert alert-danger mt-4" role="alert">
                  <i className="bi bi-exclamation-triangle-fill me-2"></i>
                  {error}
                </div>
              )}
            </div>
          </div>

          <div className="mt-4 p-3 bg-light rounded">
            <h6>Instruções:</h6>
            <ul className="mb-0 small">
              <li><strong>Por Protocolo:</strong> Informe Fonte, Protocolo e DAC</li>
              <li><strong>Por Número do Sinistro:</strong> Informe Tipo de Seguro, Origem, Ramo e Número</li>
              <li><strong>Por Código Líder:</strong> Informe Código Líder e Sinistro Líder</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};
