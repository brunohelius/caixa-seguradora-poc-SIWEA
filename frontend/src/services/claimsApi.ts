/**
 * Claims API Service
 * T043 [US1] - Axios service for claim operations
 */

import axios, { AxiosError } from 'axios';
import type { ClaimSearchCriteria, ClaimDetailResponse, ClaimSearchResponse } from '../models/Claim';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001';

// Create axios instance with base configuration
const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 seconds
});

// Request interceptor for JWT token injection
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('jwt_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ mensagem?: string; codigoErro?: string }>) => {
    const errorMessage = mapErrorToPortuguese(error);
    return Promise.reject(new Error(errorMessage));
  }
);

/**
 * Maps HTTP error status codes to user-friendly Portuguese messages
 */
function mapErrorToPortuguese(error: AxiosError<{ mensagem?: string; codigoErro?: string }>): string {
  if (error.response?.data?.mensagem) {
    return error.response.data.mensagem;
  }

  const status = error.response?.status;
  const errorCode = error.response?.data?.codigoErro;

  switch (status) {
    case 400:
      return 'Dados inválidos. Verifique as informações fornecidas.';
    case 401:
      return 'Não autorizado. Faça login novamente.';
    case 403:
      return 'Acesso negado. Você não tem permissão para esta operação.';
    case 404:
      if (errorCode === 'SINISTRO_NAO_ENCONTRADO') {
        return 'NAO CADASTRADO';
      }
      return 'Sinistro não encontrado.';
    case 409:
      return 'Conflito de concorrência. O registro foi modificado por outro usuário. Recarregue e tente novamente.';
    case 422:
      if (errorCode === 'VALIDACAO_EXTERNA_FALHOU') {
        return 'Falha na validação externa. Verifique os dados do sinistro.';
      }
      return 'Não foi possível processar a solicitação.';
    case 500:
      return 'Erro interno no servidor. Tente novamente mais tarde.';
    case 503:
      return 'Serviço temporariamente indisponível. Tente novamente em alguns instantes.';
    default:
      if (error.code === 'ECONNABORTED') {
        return 'Tempo de resposta esgotado. Verifique sua conexão.';
      }
      if (error.code === 'ERR_NETWORK') {
        return 'Erro de rede. Verifique sua conexão com a internet.';
      }
      return `Erro inesperado: ${error.message}`;
  }
}

/**
 * Search for a claim using various criteria
 */
export async function searchClaim(criteria: ClaimSearchCriteria): Promise<ClaimSearchResponse> {
  const response = await apiClient.post<ClaimSearchResponse>('/claims/search', criteria);
  return response.data;
}

/**
 * Get claim details by ID
 */
export async function getClaimById(
  tipseg: number,
  orgsin: number,
  rmosin: number,
  numsin: number
): Promise<ClaimDetailResponse> {
  const response = await apiClient.get<ClaimDetailResponse>(
    `/claims/${tipseg}/${orgsin}/${rmosin}/${numsin}`
  );
  return response.data;
}

/**
 * Authorize payment for a claim
 */
export async function authorizePayment(
  tipseg: number,
  orgsin: number,
  rmosin: number,
  numsin: number,
  paymentData: {
    tipoPagamento: number;
    valorPrincipal: number;
    valorCorrecao?: number;
    favorecido?: string;
    tipoApolice: string;
    observacoes?: string;
  }
): Promise<any> {
  const response = await apiClient.post(
    `/claims/${tipseg}/${orgsin}/${rmosin}/${numsin}/authorize-payment`,
    paymentData
  );
  return response.data;
}

/**
 * Get payment history for a claim
 */
export async function getClaimHistory(
  tipseg: number,
  orgsin: number,
  rmosin: number,
  numsin: number,
  page: number = 1,
  pageSize: number = 20
): Promise<any> {
  const response = await apiClient.get(
    `/claims/${tipseg}/${orgsin}/${rmosin}/${numsin}/history`,
    {
      params: { page, pageSize }
    }
  );
  return response.data;
}

/**
 * Get claim phases by protocol
 */
export async function getClaimPhases(
  fonte: number,
  protsini: number,
  dac: number
): Promise<any> {
  const response = await apiClient.get(
    `/claims/${fonte}/${protsini}/${dac}/phases`
  );
  return response.data;
}

export default {
  searchClaim,
  getClaimById,
  authorizePayment,
  getClaimHistory,
  getClaimPhases,
};
