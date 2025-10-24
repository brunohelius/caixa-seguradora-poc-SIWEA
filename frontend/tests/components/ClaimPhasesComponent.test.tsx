import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { describe, test, expect, vi, beforeEach } from 'vitest';
import ClaimPhasesComponent from '../../src/components/claims/ClaimPhasesComponent';
import * as claimsApi from '../../src/services/claimsApi';

// Mock the API
vi.mock('../../src/services/claimsApi');

const mockPhasesData = {
  sucesso: true,
  protocolo: '001/0123456-7',
  totalFases: 2,
  fases: [
    {
      codFase: 1,
      nomeFase: 'Análise Inicial',
      codEvento: 100,
      nomeEvento: 'Abertura de Sinistro',
      numOcorrSiniaco: 1,
      dataInivigRefaev: '2025-01-01',
      dataAberturaSifa: '2025-10-20',
      dataFechaSifa: '9999-12-31',
      status: 'Aberta',
      diasAberta: 3,
      isOpen: true,
      daysOpen: 3,
      statusDescription: 'Aberta'
    },
    {
      codFase: 2,
      nomeFase: 'Pagamento Autorizado',
      codEvento: 200,
      nomeEvento: 'Autorização de Pagamento',
      numOcorrSiniaco: 2,
      dataInivigRefaev: '2025-01-01',
      dataAberturaSifa: '2025-10-15',
      dataFechaSifa: '2025-10-19',
      status: 'Fechada',
      diasAberta: 4,
      isOpen: false,
      daysOpen: 4,
      statusDescription: 'Fechada'
    }
  ]
};

const mockLongOpenPhase = {
  ...mockPhasesData.fases[0],
  diasAberta: 45,
  daysOpen: 45
};

describe('ClaimPhasesComponent', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false }
      }
    });
    vi.clearAllMocks();
  });

  const renderComponent = (protocolKey = { fonte: 1, protsini: 123456, dac: 7 }) => {
    return render(
      <QueryClientProvider client={queryClient}>
        <ClaimPhasesComponent protocolKey={protocolKey} />
      </QueryClientProvider>
    );
  };

  test('renders phases in timeline', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(mockPhasesData);

    renderComponent();

    await waitFor(() => {
      // Should show both phase nodes
      expect(screen.getByText('Análise Inicial')).toBeInTheDocument();
      expect(screen.getByText('Pagamento Autorizado')).toBeInTheDocument();
    });

    // Check timeline structure
    const timeline = screen.getByRole('list', { name: /timeline/i });
    expect(timeline).toBeInTheDocument();

    // Should have 2 timeline nodes
    const timelineNodes = screen.getAllByRole('listitem');
    expect(timelineNodes).toHaveLength(2);
  });

  test('distinguishes open vs closed phases', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(mockPhasesData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Análise Inicial')).toBeInTheDocument();
    });

    // Open phase should have green indicator
    const openPhase = screen.getByText('Análise Inicial').closest('[data-testid="phase-node"]');
    expect(openPhase).toHaveClass('phase-open');

    // Look for "Em Aberto" text for open phase
    expect(screen.getByText('Em Aberto')).toBeInTheDocument();

    // Closed phase should have checkmark
    const closedPhase = screen.getByText('Pagamento Autorizado').closest('[data-testid="phase-node"]');
    expect(closedPhase).toHaveClass('phase-closed');

    // Should show closing date for closed phase
    expect(screen.getByText(/19\/10\/2025/)).toBeInTheDocument();
  });

  test('calculates days open correctly', async () => {
    const modifiedData = {
      ...mockPhasesData,
      fases: [
        {
          ...mockPhasesData.fases[0],
          diasAberta: 45,
          daysOpen: 45
        },
        mockPhasesData.fases[1]
      ]
    };

    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(modifiedData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('45 dias')).toBeInTheDocument();
    });

    // Check color coding based on days open
    const longOpenPhase = screen.getByText('45 dias').closest('[data-testid="phase-duration"]');

    // Should have warning color for 30-60 days
    expect(longOpenPhase).toHaveClass('duration-warning');
  });

  test('shows loading state', () => {
    vi.mocked(claimsApi.getClaimPhases).mockImplementation(
      () => new Promise(() => {}) // Never resolves
    );

    renderComponent();

    expect(screen.getByText(/Carregando fases.../i)).toBeInTheDocument();
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  test('shows error state', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockRejectedValue(
      new Error('Erro ao carregar fases')
    );

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText(/Erro ao carregar fases/i)).toBeInTheDocument();
    });

    // Should show retry button
    expect(screen.getByRole('button', { name: /Tentar novamente/i })).toBeInTheDocument();
  });

  test('shows empty state when no phases', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue({
      sucesso: true,
      protocolo: '001/0123456-7',
      totalFases: 0,
      fases: []
    });

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText(/Nenhuma fase registrada/i)).toBeInTheDocument();
    });
  });

  test('displays event details', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(mockPhasesData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Análise Inicial')).toBeInTheDocument();
    });

    // Should show event that triggered the phase
    expect(screen.getByText('Abertura de Sinistro')).toBeInTheDocument();
    expect(screen.getByText('Autorização de Pagamento')).toBeInTheDocument();
  });

  test('displays dates correctly', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(mockPhasesData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Análise Inicial')).toBeInTheDocument();
    });

    // Check opening dates
    expect(screen.getByText(/20\/10\/2025/)).toBeInTheDocument();
    expect(screen.getByText(/15\/10\/2025/)).toBeInTheDocument();

    // Check closing date for closed phase
    expect(screen.getByText(/19\/10\/2025/)).toBeInTheDocument();
  });

  test('expandable details work', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(mockPhasesData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Análise Inicial')).toBeInTheDocument();
    });

    // Click to expand details
    const expandButton = screen.getAllByRole('button', { name: /Ver detalhes/i })[0];
    fireEvent.click(expandButton);

    // Should show additional details
    await waitFor(() => {
      expect(screen.getByText(/Número de Ocorrência: 1/i)).toBeInTheDocument();
      expect(screen.getByText(/Data Início Vigência: 01\/01\/2025/i)).toBeInTheDocument();
    });
  });

  test('color codes phases by duration', async () => {
    const phasesWithVariousDurations = {
      ...mockPhasesData,
      fases: [
        { ...mockPhasesData.fases[0], diasAberta: 15, daysOpen: 15 }, // Green
        { ...mockPhasesData.fases[0], diasAberta: 45, daysOpen: 45 }, // Yellow
        { ...mockPhasesData.fases[0], diasAberta: 90, daysOpen: 90 }  // Red
      ]
    };

    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(phasesWithVariousDurations);

    renderComponent();

    await waitFor(() => {
      const duration15 = screen.getByText('15 dias').closest('[data-testid="phase-duration"]');
      const duration45 = screen.getByText('45 dias').closest('[data-testid="phase-duration"]');
      const duration90 = screen.getByText('90 dias').closest('[data-testid="phase-duration"]');

      expect(duration15).toHaveClass('duration-ok'); // Green
      expect(duration45).toHaveClass('duration-warning'); // Yellow
      expect(duration90).toHaveClass('duration-critical'); // Red
    });
  });

  test('refresh button works', async () => {
    const getClaimPhasesMock = vi.mocked(claimsApi.getClaimPhases);
    getClaimPhasesMock.mockResolvedValue(mockPhasesData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Análise Inicial')).toBeInTheDocument();
    });

    // Click refresh
    const refreshButton = screen.getByRole('button', { name: /Atualizar/i });
    fireEvent.click(refreshButton);

    // Should call API again
    await waitFor(() => {
      expect(getClaimPhasesMock).toHaveBeenCalledTimes(2);
    });
  });

  test('timeline is chronologically ordered', async () => {
    vi.mocked(claimsApi.getClaimPhases).mockResolvedValue(mockPhasesData);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Análise Inicial')).toBeInTheDocument();
    });

    const timelineNodes = screen.getAllByRole('listitem');

    // First node should be the most recent (open phase from 20/10)
    expect(timelineNodes[0]).toHaveTextContent('Análise Inicial');

    // Second node should be the older closed phase (15/10 - 19/10)
    expect(timelineNodes[1]).toHaveTextContent('Pagamento Autorizado');
  });
});