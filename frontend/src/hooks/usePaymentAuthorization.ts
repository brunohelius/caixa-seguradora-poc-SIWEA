import { useState, useCallback } from 'react';

interface PaymentAuthorizationRequest {
  tipoPagamento: number;
  valorPrincipal: number;
  valorCorrecao: number;
  favorecido?: string | null;
  tipoApolice: string;
  observacoes?: string | null;
}

interface PaymentAuthorizationResponse {
  sucesso: boolean;
  ocorhist: number;
  operacao: number;
  dataMovimento: string;
  horaOperacao: string;
  valorPrincipalBTNF: number;
  valorCorrecaoBTNF: number;
  valorTotalBTNF: number;
  valorPendenteAtualizado: number;
  mensagemErro?: string;
  codigoErro?: string;
}

interface UsePaymentAuthorizationReturn {
  authorizePayment: (
    tipseg: number,
    orgsin: number,
    rmosin: number,
    numsin: number,
    request: PaymentAuthorizationRequest
  ) => Promise<PaymentAuthorizationResponse>;
  loading: boolean;
  error: string | null;
  success: boolean;
  retryCount: number;
  resetState: () => void;
}

/**
 * Custom hook for payment authorization operations
 * T070 [US2] - Encapsulates payment submission logic with:
 * - Loading state
 * - Error state with message mapping
 * - Success state
 * - Automatic retry on 409 conflict
 * - Error message mapping (Portuguese)
 */
export const usePaymentAuthorization = (): UsePaymentAuthorizationReturn => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [retryCount, setRetryCount] = useState(0);

  const MAX_RETRIES = 3;

  /**
   * Map HTTP status codes and error codes to Portuguese messages
   */
  const mapErrorMessage = (statusCode: number, errorCode?: string, defaultMessage?: string): string => {
    // Error code specific messages
    if (errorCode) {
      const errorMessages: Record<string, string> = {
        'SINISTRO_NAO_ENCONTRADO': 'Sinistro não encontrado no sistema',
        'VALIDACAO_EXTERNA_FALHOU': 'Validação externa falhou. Verifique os dados do contrato',
        'VALIDACAO_FALHOU': 'Erro de validação. Verifique os dados informados',
        'SALDO_INSUFICIENTE': 'Valor total excede o saldo pendente do sinistro',
        'TAXA_CONVERSAO_NAO_ENCONTRADA': 'Taxa de conversão não encontrada para a data atual',
        'FAVORECIDO_OBRIGATORIO': 'O campo Favorecido é obrigatório para este tipo de seguro',
        'PRODUTO_CONSORCIO_INVALIDO': 'Validação de produto consórcio falhou'
      };

      if (errorMessages[errorCode]) {
        return errorMessages[errorCode];
      }
    }

    // HTTP status code messages
    switch (statusCode) {
      case 400:
        return 'Dados inválidos. Verifique os campos do formulário';
      case 404:
        return 'Sinistro não encontrado';
      case 409:
        return 'Conflito detectado. Outro usuário pode ter atualizado este sinistro. Tentando novamente...';
      case 422:
        return 'Falha na validação externa. ' + (defaultMessage || 'Verifique os dados do contrato');
      case 500:
        return 'Erro interno do servidor. Tente novamente mais tarde';
      case 503:
        return 'Serviço temporariamente indisponível. Tente novamente em alguns instantes';
      default:
        return defaultMessage || 'Erro ao processar a solicitação';
    }
  };

  /**
   * Reset hook state
   */
  const resetState = useCallback(() => {
    setLoading(false);
    setError(null);
    setSuccess(false);
    setRetryCount(0);
  }, []);

  /**
   * Authorize payment with automatic retry on conflict
   */
  const authorizePayment = useCallback(
    async (
      tipseg: number,
      orgsin: number,
      rmosin: number,
      numsin: number,
      request: PaymentAuthorizationRequest
    ): Promise<PaymentAuthorizationResponse> => {
      setLoading(true);
      setError(null);
      setSuccess(false);

      const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001';
      const url = `${apiBaseUrl}/api/claims/${tipseg}/${orgsin}/${rmosin}/${numsin}/authorize-payment`;

      let currentRetry = 0;

      while (currentRetry <= MAX_RETRIES) {
        try {
          const response = await fetch(url, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json'
            },
            body: JSON.stringify(request)
          });

          if (!response.ok) {
            const errorData = await response.json().catch(() => null);

            // Handle 409 Conflict with automatic retry
            if (response.status === 409 && currentRetry < MAX_RETRIES) {
              currentRetry++;
              setRetryCount(currentRetry);
              setError(`Conflito detectado. Tentativa ${currentRetry} de ${MAX_RETRIES}...`);

              // Wait before retrying (exponential backoff)
              await new Promise(resolve => setTimeout(resolve, 1000 * currentRetry));
              continue;
            }

            // Other errors - no retry
            const errorMessage = mapErrorMessage(
              response.status,
              errorData?.codigoErro,
              errorData?.mensagem
            );

            setError(errorMessage);
            setLoading(false);
            throw new Error(errorMessage);
          }

          // Success
          const data: PaymentAuthorizationResponse = await response.json();
          setSuccess(true);
          setLoading(false);
          return data;

        } catch (err: any) {
          // Network errors
          if (err.name === 'TypeError' && err.message.includes('fetch')) {
            const networkError = 'Erro de conexão. Verifique sua internet.';
            setError(networkError);
            setLoading(false);
            throw new Error(networkError);
          }

          // Timeout errors
          if (err.name === 'AbortError') {
            const timeoutError = 'A requisição demorou muito. Tente novamente.';
            setError(timeoutError);
            setLoading(false);
            throw new Error(timeoutError);
          }

          // Re-throw if already handled
          if (error) {
            throw err;
          }

          // Generic error
          const genericError = err.message || 'Erro desconhecido ao processar a solicitação';
          setError(genericError);
          setLoading(false);
          throw new Error(genericError);
        }
      }

      // Max retries exceeded
      const maxRetriesError = 'Número máximo de tentativas excedido. Tente novamente mais tarde.';
      setError(maxRetriesError);
      setLoading(false);
      throw new Error(maxRetriesError);
    },
    [error]
  );

  return {
    authorizePayment,
    loading,
    error,
    success,
    retryCount,
    resetState
  };
};

export default usePaymentAuthorization;
