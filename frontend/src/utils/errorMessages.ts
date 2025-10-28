/**
 * T136 [UI Polish] - Frontend Error Message Mapping
 * Maps API error codes to user-friendly Portuguese messages
 */

export interface ApiError {
  statusCode: number;
  errorCode?: string;
  message?: string;
  details?: string[];
}

/**
 * Maps API error codes to Portuguese user-friendly messages
 */
export const mapApiError = (
  statusCode: number,
  errorCode?: string,
  defaultMessage?: string
): string => {
  // Network errors
  if (statusCode === 0) {
    return 'Erro de conexão. Verifique sua internet e tente novamente.';
  }

  // Timeout errors
  if (statusCode === 408 || statusCode === 504) {
    return 'A requisição demorou muito. Tente novamente.';
  }

  // Specific error code mapping
  const errorCodeMessages: Record<string, string> = {
    // Claim errors
    'SINISTRO_NAO_ENCONTRADO': 'Sinistro não encontrado. Verifique os dados informados.',
    'DOCUMENTO_NAO_CADASTRADO': 'Documento não cadastrado no sistema.',
    'CRITERIOS_INVALIDOS': 'Critérios de busca inválidos. Preencha pelo menos um conjunto completo.',

    // Payment errors
    'VALIDACAO_EXTERNA_FALHOU': 'Validação externa falhou. Entre em contato com o suporte.',
    'SALDO_INSUFICIENTE': 'Valor total excede o saldo pendente do sinistro.',
    'FAVORECIDO_OBRIGATORIO': 'Favorecido é obrigatório para este tipo de seguro.',
    'TIPO_PAGAMENTO_INVALIDO': 'Tipo de pagamento deve ser 1, 2, 3, 4 ou 5.',
    'CONVERSAO_MOEDA_FALHOU': 'Falha na conversão de moeda. Taxa não disponível para a data.',

    // History errors
    'HISTORICO_NAO_ENCONTRADO': 'Nenhum registro de histórico encontrado.',

    // Phase errors
    'FASES_NAO_ENCONTRADAS': 'Nenhuma fase encontrada para este sinistro.',

    // Authorization errors
    'NAO_AUTORIZADO': 'Você não tem permissão para acessar este recurso.',
    'TOKEN_EXPIRADO': 'Sua sessão expirou. Faça login novamente.',
    'TOKEN_INVALIDO': 'Token de autenticação inválido.',

    // Generic errors
    'ERRO_INTERNO': 'Erro interno do servidor. Tente novamente mais tarde.',
    'PARAMETROS_INVALIDOS': 'Parâmetros inválidos. Verifique os dados informados.',
    'RECURSO_NAO_ENCONTRADO': 'Recurso não encontrado.',
  };

  // Return specific error code message if available
  if (errorCode && errorCodeMessages[errorCode]) {
    return errorCodeMessages[errorCode];
  }

  // Status code based messages
  const statusCodeMessages: Record<number, string> = {
    400: 'Requisição inválida. Verifique os dados informados.',
    401: 'Não autorizado. Faça login para continuar.',
    403: 'Acesso negado. Você não tem permissão para esta operação.',
    404: 'Recurso não encontrado.',
    409: 'Conflito. O recurso foi modificado por outro usuário.',
    422: 'Dados não processáveis. Verifique as informações fornecidas.',
    429: 'Muitas requisições. Aguarde alguns segundos e tente novamente.',
    500: 'Erro interno do servidor. Tente novamente mais tarde.',
    502: 'Serviço temporariamente indisponível.',
    503: 'Serviço em manutenção. Tente novamente mais tarde.',
  };

  if (statusCodeMessages[statusCode]) {
    return statusCodeMessages[statusCode];
  }

  // Return default message or generic fallback
  return defaultMessage || 'Ocorreu um erro inesperado. Tente novamente.';
};

/**
 * Formats validation errors from API into readable Portuguese messages
 */
export const formatValidationErrors = (details?: string[]): string => {
  if (!details || details.length === 0) {
    return '';
  }

  if (details.length === 1) {
    return details[0];
  }

  return details.map((error, index) => `${index + 1}. ${error}`).join('\n');
};

/**
 * Creates a formatted error message for display
 */
export const createErrorMessage = (error: ApiError): string => {
  const mainMessage = mapApiError(error.statusCode, error.errorCode, error.message);

  if (error.details && error.details.length > 0) {
    const detailsFormatted = formatValidationErrors(error.details);
    return `${mainMessage}\n\n${detailsFormatted}`;
  }

  return mainMessage;
};

/**
 * Error display component helper
 */
export const getErrorClassNames = (errorCode?: string): string => {
  const baseClasses = 'error-message';

  // Add specific classes based on error severity
  if (errorCode?.includes('INTERNO') || errorCode?.includes('FATAL')) {
    return `${baseClasses} error-message--critical`;
  }

  if (errorCode?.includes('NAO_ENCONTRADO') || errorCode?.includes('NOT_FOUND')) {
    return `${baseClasses} error-message--warning`;
  }

  return `${baseClasses} error-message--error`;
};

export default {
  mapApiError,
  formatValidationErrors,
  createErrorMessage,
  getErrorClassNames,
};
