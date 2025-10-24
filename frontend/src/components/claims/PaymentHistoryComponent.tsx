import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import axios from 'axios';

/**
 * T079: PaymentHistoryComponent
 * Displays paginated payment history for a claim with sortable columns
 * and loading/empty states.
 */

interface ClaimKey {
  tipseg: number;
  orgsin: number;
  rmosin: number;
  numsin: number;
}

interface HistoryRecord {
  tipseg: number;
  orgsin: number;
  rmosin: number;
  numsin: number;
  ocorhist: number;
  operacao: number;
  dtmovto: string;
  horaoper: string;
  dataHoraFormatada: string;
  valpri: number;
  crrmon: number;
  nomfav: string;
  tipcrr: string;
  valpribt: number;
  crrmonbt: number;
  valtotbt: number;
  sitcontb: string;
  situacao: string;
  ezeusrid: string;
}

interface ClaimHistoryResponse {
  sucesso: boolean;
  totalRegistros: number;
  paginaAtual: number;
  tamanhoPagina: number;
  totalPaginas: number;
  historico: HistoryRecord[];
}

interface PaymentHistoryComponentProps {
  claimKey: ClaimKey;
}

const PaymentHistoryComponent: React.FC<PaymentHistoryComponentProps> = ({
  claimKey,
}) => {
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  const fetchHistory = async (page: number): Promise<ClaimHistoryResponse> => {
    const baseUrl =
      import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001';
    const url = `${baseUrl}/api/claims/${claimKey.tipseg}/${claimKey.orgsin}/${claimKey.rmosin}/${claimKey.numsin}/history?page=${page}&pageSize=${pageSize}`;

    const response = await axios.get<ClaimHistoryResponse>(url);
    return response.data;
  };

  const {
    data,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: ['claimHistory', claimKey, currentPage],
    queryFn: () => fetchHistory(currentPage),
    staleTime: 0, // Always refetch
    refetchOnWindowFocus: false,
  });

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value);
  };

  const handlePreviousPage = () => {
    if (currentPage > 1) {
      setCurrentPage(currentPage - 1);
    }
  };

  const handleNextPage = () => {
    if (data && currentPage < data.totalPaginas) {
      setCurrentPage(currentPage + 1);
    }
  };

  const handlePageClick = (page: number) => {
    setCurrentPage(page);
  };

  const renderPaginationNumbers = () => {
    if (!data || data.totalPaginas <= 1) return null;

    const pages: number[] = [];
    const maxPagesToShow = 5;
    const startPage = Math.max(
      1,
      currentPage - Math.floor(maxPagesToShow / 2)
    );
    const endPage = Math.min(data.totalPaginas, startPage + maxPagesToShow - 1);

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return (
      <div className="pagination-numbers">
        {pages.map((page) => (
          <button
            key={page}
            className={`page-number ${page === currentPage ? 'active' : ''}`}
            onClick={() => handlePageClick(page)}
          >
            {page}
          </button>
        ))}
      </div>
    );
  };

  if (isLoading) {
    return (
      <div className="payment-history-container">
        <h3>Histórico de Pagamentos</h3>
        <div className="loading-skeleton">
          <div className="skeleton-row"></div>
          <div className="skeleton-row"></div>
          <div className="skeleton-row"></div>
          <div className="skeleton-row"></div>
          <div className="skeleton-row"></div>
        </div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="payment-history-container">
        <h3>Histórico de Pagamentos</h3>
        <div className="error-message">
          Erro ao carregar histórico: {error instanceof Error ? error.message : 'Erro desconhecido'}
          <button onClick={() => refetch()} className="retry-button">
            Tentar Novamente
          </button>
        </div>
      </div>
    );
  }

  if (!data || data.historico.length === 0) {
    return (
      <div className="payment-history-container">
        <h3>Histórico de Pagamentos</h3>
        <div className="empty-state">
          <p>Nenhum registro de histórico encontrado.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="payment-history-container">
      <div className="history-header">
        <h3>Histórico de Pagamentos</h3>
        <button onClick={() => refetch()} className="refresh-button">
          Atualizar
        </button>
      </div>

      <div className="table-container">
        <table className="history-table">
          <thead>
            <tr>
              <th>Data/Hora</th>
              <th>Ocorrência</th>
              <th>Operação</th>
              <th>Valor Principal</th>
              <th>Correção</th>
              <th>Valor Total BTNF</th>
              <th>Favorecido</th>
              <th>Operador</th>
            </tr>
          </thead>
          <tbody>
            {data.historico.map((record, index) => (
              <tr key={`${record.ocorhist}-${index}`}>
                <td>{record.dataHoraFormatada}</td>
                <td>{record.ocorhist}</td>
                <td>{record.operacao}</td>
                <td className="currency">{formatCurrency(record.valpri)}</td>
                <td className="currency">{formatCurrency(record.crrmon)}</td>
                <td className="currency">{formatCurrency(record.valtotbt)}</td>
                <td>{record.nomfav || '-'}</td>
                <td>{record.ezeusrid}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="pagination-container">
        <div className="pagination-info">
          Mostrando {data.historico.length} de {data.totalRegistros} registros
          (Página {data.paginaAtual} de {data.totalPaginas})
        </div>

        <div className="pagination-controls">
          <button
            onClick={handlePreviousPage}
            disabled={currentPage === 1}
            className="pagination-button"
          >
            Anterior
          </button>

          {renderPaginationNumbers()}

          <button
            onClick={handleNextPage}
            disabled={currentPage >= data.totalPaginas}
            className="pagination-button"
          >
            Próxima
          </button>
        </div>
      </div>
    </div>
  );
};

export default PaymentHistoryComponent;
