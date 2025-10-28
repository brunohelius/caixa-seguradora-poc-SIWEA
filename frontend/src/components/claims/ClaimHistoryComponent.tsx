import React, { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import * as claimsApi from '../../services/claimsApi';
import type { ClaimKey } from '../../models/Claim';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '../ui/table';
import { Button } from '../ui/button';
import { Alert, AlertDescription } from '../ui/alert';
import { Loader2, ChevronLeft, ChevronRight } from 'lucide-react';

/**
 * T076-T078 [US3] - F03: Payment History Component
 * Displays paginated claim history with 20 records per page
 * Performance optimized with server-side pagination
 */

interface ClaimHistoryProps {
  claimKey: ClaimKey;
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
  dataHoraCompleta: string;
  valpri: number;
  crrmon: number;
  nomfav: string | null;
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

export const ClaimHistoryComponent: React.FC<ClaimHistoryProps> = ({ claimKey }) => {
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  const { data, isLoading, error, refetch } = useQuery<ClaimHistoryResponse>({
    queryKey: ['claimHistory', claimKey, currentPage, pageSize],
    queryFn: async () => {
      const response = await claimsApi.getClaimHistory(
        claimKey.tipseg,
        claimKey.orgsin,
        claimKey.rmosin,
        claimKey.numsin,
        currentPage,
        pageSize
      );
      return response.data;
    },
    staleTime: 30000, // Cache for 30 seconds
  });

  // Reset to page 1 when claim changes
  useEffect(() => {
    setCurrentPage(1);
  }, [claimKey]);

  const handlePreviousPage = () => {
    setCurrentPage((prev) => Math.max(1, prev - 1));
  };

  const handleNextPage = () => {
    if (data && currentPage < data.totalPaginas) {
      setCurrentPage((prev) => prev + 1);
    }
  };

  const handlePageClick = (page: number) => {
    setCurrentPage(page);
  };

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  const formatDate = (dateTimeString: string): string => {
    if (!dateTimeString) return '-';
    try {
      const date = new Date(dateTimeString);
      return date.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
      });
    } catch {
      return dateTimeString;
    }
  };

  const getOperationName = (operacao: number): string => {
    switch (operacao) {
      case 1098:
        return 'Autorização de Pagamento';
      default:
        return `Operação ${operacao}`;
    }
  };

  const getSituationLabel = (situacao: string): string => {
    switch (situacao) {
      case '0':
        return 'Normal';
      case '1':
        return 'Processado';
      case '2':
        return 'Cancelado';
      default:
        return situacao;
    }
  };

  const renderPaginationButtons = () => {
    if (!data || data.totalPaginas <= 1) return null;

    const maxButtons = 7;
    const buttons: (number | string)[] = [];
    const totalPages = data.totalPaginas;

    if (totalPages <= maxButtons) {
      for (let i = 1; i <= totalPages; i++) {
        buttons.push(i);
      }
    } else {
      buttons.push(1);

      if (currentPage > 3) {
        buttons.push('...');
      }

      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);

      for (let i = start; i <= end; i++) {
        buttons.push(i);
      }

      if (currentPage < totalPages - 2) {
        buttons.push('...');
      }

      buttons.push(totalPages);
    }

    return (
      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          onClick={handlePreviousPage}
          disabled={currentPage === 1}
        >
          <ChevronLeft className="h-4 w-4" />
          Anterior
        </Button>

        {buttons.map((btn, idx) =>
          typeof btn === 'number' ? (
            <Button
              key={idx}
              variant={currentPage === btn ? 'default' : 'outline'}
              size="sm"
              onClick={() => handlePageClick(btn)}
              className={currentPage === btn ? 'bg-blue-600 text-white' : ''}
            >
              {btn}
            </Button>
          ) : (
            <span key={idx} className="px-2">
              {btn}
            </span>
          )
        )}

        <Button
          variant="outline"
          size="sm"
          onClick={handleNextPage}
          disabled={currentPage === totalPages}
        >
          Próxima
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>
    );
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-blue-600" />
        <span className="ml-2">Carregando histórico...</span>
      </div>
    );
  }

  if (error) {
    return (
      <Alert variant="destructive">
        <AlertDescription>
          Erro ao carregar histórico de pagamentos. Tente novamente mais tarde.
          <Button variant="outline" size="sm" onClick={() => refetch()} className="ml-4">
            Tentar Novamente
          </Button>
        </AlertDescription>
      </Alert>
    );
  }

  if (!data || data.historico.length === 0) {
    return (
      <Alert>
        <AlertDescription>
          Nenhum registro de histórico encontrado para este sinistro.
        </AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="space-y-4">
      {/* Header with summary */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-semibold">Histórico de Pagamentos</h3>
          <p className="text-sm text-gray-600">
            Total: {data.totalRegistros} registro(s) | Página {data.paginaAtual} de{' '}
            {data.totalPaginas}
          </p>
        </div>
      </div>

      {/* History Table */}
      <div className="overflow-x-auto border rounded-lg">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[80px]">Ocorrência</TableHead>
              <TableHead>Operação</TableHead>
              <TableHead>Data/Hora</TableHead>
              <TableHead className="text-right">Valor Principal</TableHead>
              <TableHead className="text-right">Correção</TableHead>
              <TableHead className="text-right">Total (BTNF)</TableHead>
              <TableHead>Beneficiário</TableHead>
              <TableHead>Situação</TableHead>
              <TableHead>Usuário</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.historico.map((record) => (
              <TableRow key={`${record.ocorhist}`}>
                <TableCell className="font-medium">{record.ocorhist}</TableCell>
                <TableCell>{getOperationName(record.operacao)}</TableCell>
                <TableCell className="text-sm">
                  {formatDate(record.dataHoraCompleta) || record.dataHoraFormatada}
                </TableCell>
                <TableCell className="text-right font-mono">
                  {formatCurrency(record.valpri)}
                </TableCell>
                <TableCell className="text-right font-mono">
                  {formatCurrency(record.crrmon)}
                </TableCell>
                <TableCell className="text-right font-mono font-semibold">
                  {formatCurrency(record.valtotbt)}
                </TableCell>
                <TableCell className="max-w-[200px] truncate" title={record.nomfav || '-'}>
                  {record.nomfav || '-'}
                </TableCell>
                <TableCell>
                  <span
                    className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${
                      record.situacao === '0'
                        ? 'bg-green-100 text-green-800'
                        : record.situacao === '2'
                        ? 'bg-red-100 text-red-800'
                        : 'bg-blue-100 text-blue-800'
                    }`}
                  >
                    {getSituationLabel(record.situacao)}
                  </span>
                </TableCell>
                <TableCell className="text-sm text-gray-600">{record.ezeusrid}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {data.totalPaginas > 1 && (
        <div className="flex items-center justify-between pt-4">
          <div className="text-sm text-gray-600">
            Exibindo {(data.paginaAtual - 1) * data.tamanhoPagina + 1} a{' '}
            {Math.min(data.paginaAtual * data.tamanhoPagina, data.totalRegistros)} de{' '}
            {data.totalRegistros} registros
          </div>
          {renderPaginationButtons()}
        </div>
      )}
    </div>
  );
};

export default ClaimHistoryComponent;
