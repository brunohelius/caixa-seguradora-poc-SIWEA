import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, test, expect, vi, beforeEach } from 'vitest';
import PaymentHistoryComponent from '../../src/components/claims/PaymentHistoryComponent';
import * as claimsApi from '../../src/services/claimsApi';

// Mock the API
vi.mock('../../src/services/claimsApi');

const mockHistoryData = {
  sucesso: true,
  totalRegistros: 3,
  paginaAtual: 1,
  tamanhoPagina: 20,
  totalPaginas: 1,
  historico: [
    {
      tipseg: 1,
      orgsin: 1,
      rmosin: 1,
      numsin: 12345,
      ocorhist: 3,
      operacao: 1098,
      dtmovto: '2025-10-23',
      horaoper: '14:30:00',
      dataHoraFormatada: '23/10/2025 14:30:00',
      valpri: 10000.00,
      crrmon: 500.00,
      nomfav: 'João Silva',
      tipcrr: '5',
      valpribt: 11000.00,
      crrmonbt: 550.00,
      valtotbt: 11550.00,
      sitcontb: '0',
      situacao: '0',
      ezeusrid: 'operator1'
    },
    {
      tipseg: 1,
      orgsin: 1,
      rmosin: 1,
      numsin: 12345,
      ocorhist: 2,
      operacao: 1098,
      dtmovto: '2025-10-22',
      horaoper: '10:15:00',
      dataHoraFormatada: '22/10/2025 10:15:00',
      valpri: 5000.00,
      crrmon: 250.00,
      nomfav: 'Maria Santos',
      tipcrr: '5',
      valpribt: 5500.00,
      crrmonbt: 275.00,
      valtotbt: 5775.00,
      sitcontb: '0',
      situacao: '0',
      ezeusrid: 'operator2'
    },
    {
      tipseg: 1,
      orgsin: 1,
      rmosin: 1,
      numsin: 12345,
      ocorhist: 1,
      operacao: 1098,
      dtmovto: '2025-10-21',
      horaoper: '09:00:00',
      dataHoraFormatada: '21/10/2025 09:00:00',
      valpri: 15000.00,
      crrmon: 750.00,
      nomfav: 'Pedro Costa',
      tipcrr: '5',
      valpribt: 16500.00,
      crrmonbt: 825.00,
      valtotbt: 17325.00,
      sitcontb: '0',
      situacao: '0',
      ezeusrid: 'operator3'
    }
  ]
};

const mockPaginatedHistoryData = {
  sucesso: true,
  totalRegistros: 50,
  paginaAtual: 1,
  tamanhoPagina: 20,
  totalPaginas: 3,
  historico: Array.from({ length: 20 }, (_, i) => ({
    ...mockHistoryData.historico[0],
    ocorhist: 50 - i,
    dataHoraFormatada: `${20 + Math.floor(i / 10)}/10/2025 ${10 + i}:00:00`
  }))
};

const mockEmptyHistoryData = {
  sucesso: true,
  totalRegistros: 0,
  paginaAtual: 1,
  tamanhoPagina: 20,
  totalPaginas: 0,
  historico: []
};

describe('PaymentHistoryComponent', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false }
      }
    });
    vi.clearAllMocks();
  });

  const renderComponent = (claimKey = { tipseg: 1, orgsin: 1, rmosin: 1, numsin: 12345 }) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <PaymentHistoryComponent claimKey={claimKey} />
      </QueryClientProvider>
    );
  };

  test('renders loading state', () => {
    vi.mocked(claimsApi.getClaimHistory).mockImplementation(() =>
      new Promise(() => {}) // Never resolves to keep loading state
    );

    renderComponent();

    expect(screen.getByText(/Carregando histórico.../i)).toBeInTheDocument();
  });

  test('renders history records correctly', async () => {
    vi.mocked(claimsApi.getClaimHistory).mockResolvedValue(mockHistoryData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('23/10/2025 14:30:00')).toBeInTheDocument();
      expect(screen.getByText('João Silva')).toBeInTheDocument();
      expect(screen.getByText('R$ 10.000,00')).toBeInTheDocument();
    });

    // Check if all 3 records are displayed
    const rows = screen.getAllByRole('row');
    // +1 for header row
    expect(rows).toHaveLength(4);

    // Verify second record
    expect(screen.getByText('Maria Santos')).toBeInTheDocument();
    expect(screen.getByText('22/10/2025 10:15:00')).toBeInTheDocument();

    // Verify third record
    expect(screen.getByText('Pedro Costa')).toBeInTheDocument();
    expect(screen.getByText('21/10/2025 09:00:00')).toBeInTheDocument();
  });

  test('handles pagination clicks', async () => {
    const getClaimHistoryMock = vi.mocked(claimsApi.getClaimHistory);
    getClaimHistoryMock.mockResolvedValue(mockPaginatedHistoryData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText(/Página 1 de 3/i)).toBeInTheDocument();
    });

    // Click next page
    const nextButton = screen.getByRole('button', { name: /Próxima/i });
    fireEvent.click(nextButton);

    await waitFor(() => {
      expect(getClaimHistoryMock).toHaveBeenCalledTimes(2);
      expect(getClaimHistoryMock).toHaveBeenLastCalledWith(
        { tipseg: 1, orgsin: 1, rmosin: 1, numsin: 12345 },
        2,
        20
      );
    });

    // Mock page 2 response
    getClaimHistoryMock.mockResolvedValue({
      ...mockPaginatedHistoryData,
      paginaAtual: 2,
      historico: Array.from({ length: 20 }, (_, i) => ({
        ...mockHistoryData.historico[0],
        ocorhist: 30 - i,
        dataHoraFormatada: `${15 + Math.floor(i / 10)}/10/2025 ${10 + i}:00:00`
      }))
    });

    await waitFor(() => {
      expect(screen.getByText(/Página 2 de 3/i)).toBeInTheDocument();
    });
  });

  test('displays empty state when no records', async () => {
    vi.mocked(claimsApi.getClaimHistory).mockResolvedValue(mockEmptyHistoryData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText(/Nenhum registro de histórico encontrado/i)).toBeInTheDocument();
    });

    // Should not display table
    expect(screen.queryByRole('table')).not.toBeInTheDocument();
  });

  test('handles error state', async () => {
    const errorMessage = 'Erro ao carregar histórico';
    vi.mocked(claimsApi.getClaimHistory).mockRejectedValue(new Error(errorMessage));

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText(/Erro ao carregar histórico/i)).toBeInTheDocument();
    });

    // Should show retry button
    const retryButton = screen.getByRole('button', { name: /Tentar novamente/i });
    expect(retryButton).toBeInTheDocument();

    // Click retry
    vi.mocked(claimsApi.getClaimHistory).mockResolvedValue(mockHistoryData);
    fireEvent.click(retryButton);

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });
  });

  test('formats currency values correctly', async () => {
    vi.mocked(claimsApi.getClaimHistory).mockResolvedValue(mockHistoryData);

    renderComponent();

    await waitFor(() => {
      // Check Brazilian currency format
      expect(screen.getByText('R$ 10.000,00')).toBeInTheDocument();
      expect(screen.getByText('R$ 500,00')).toBeInTheDocument();
      expect(screen.getByText('R$ 11.550,00')).toBeInTheDocument();
    });
  });

  test('shows correct operation code', async () => {
    vi.mocked(claimsApi.getClaimHistory).mockResolvedValue(mockHistoryData);

    renderComponent();

    await waitFor(() => {
      // All should show operation 1098 (payment authorization)
      const operationCells = screen.getAllByText('1098');
      expect(operationCells).toHaveLength(3);
    });
  });

  test('displays operator ID', async () => {
    vi.mocked(claimsApi.getClaimHistory).mockResolvedValue(mockHistoryData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('operator1')).toBeInTheDocument();
      expect(screen.getByText('operator2')).toBeInTheDocument();
      expect(screen.getByText('operator3')).toBeInTheDocument();
    });
  });

  test('refresh button reloads data', async () => {
    const getClaimHistoryMock = vi.mocked(claimsApi.getClaimHistory);
    getClaimHistoryMock.mockResolvedValue(mockHistoryData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    // Click refresh
    const refreshButton = screen.getByRole('button', { name: /Atualizar/i });
    fireEvent.click(refreshButton);

    await waitFor(() => {
      expect(getClaimHistoryMock).toHaveBeenCalledTimes(2);
    });
  });

  test('export to CSV functionality', async () => {
    vi.mocked(claimsApi.getClaimHistory).mockResolvedValue(mockHistoryData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    // Look for export button
    const exportButton = screen.getByRole('button', { name: /Exportar CSV/i });
    expect(exportButton).toBeInTheDocument();

    // Mock URL.createObjectURL and link click
    const createObjectURLMock = vi.fn();
    global.URL.createObjectURL = createObjectURLMock;
    createObjectURLMock.mockReturnValue('blob:mock-url');

    // Click export
    fireEvent.click(exportButton);

    // Should create blob and trigger download
    expect(createObjectURLMock).toHaveBeenCalled();
  });
});