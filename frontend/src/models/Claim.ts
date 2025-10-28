/**
 * Claim TypeScript interface matching ClaimDetailDto
 * T042 [US1] - Frontend model for claim details
 * Updated: Added ClaimKey interface for history page
 */

// ClaimKey interface for history component
export interface ClaimKey {
  tipseg: number;
  orgsin: number;
  rmosin: number;
  numsin: number;
}

export interface Claim {
  // Primary Key
  tipseg: number;
  orgsin: number;
  rmosin: number;
  numsin: number;

  // Protocol Information
  fonte: number;
  protsini: number;
  dac: number;
  numeroProtocolo: string; // "fonte/protsini-dac" (e.g., "001/0123456-7")
  numeroSinistro: string;  // "orgsin/rmosin/numsin" (e.g., "001/001/0000123")

  // Policy Information
  orgapo: number;
  rmoapo: number;
  numapol: number;
  numeroApolice: string; // "orgapo/rmoapo/numapol"
  nomeSeguradora: string | null;
  nomeRamo: string | null;

  // Product Information
  codprodu: number;
  ehConsorcio: boolean; // consortium products: 6814, 7701, 7709

  // Financial Information
  sdopag: number;    // Expected Reserve Amount
  totpag: number;    // Total Payments Made
  valorPendente: number; // Pending Value (Sdopag - Totpag)

  // Leader Information (reinsurance)
  codlider: number | null;
  sinlid: number | null;

  // Workflow Information
  ocorhist: number; // History Occurrence Counter
  tipreg: string;   // Policy Type (1 or 2)
  tpsegu: number;   // Insurance Type (0 = optional beneficiary, != 0 = mandatory)

  // Audit Fields
  createdBy: string | null;
  createdAt: string | null;
  updatedBy: string | null;
  updatedAt: string | null;
}

export interface ClaimSearchCriteria {
  // Protocol search (mutual exclusive group 1)
  fonte?: number;
  protsini?: number;
  dac?: number;
  protocol?: string;
  searchType?: string;

  // Claim number search (mutual exclusive group 2)
  orgsin?: number;
  rmosin?: number;
  numsin?: number;
  tipseg?: number;

  // Leader search (mutual exclusive group 3)
  codlider?: number;
  sinlid?: number;
}

export interface ClaimDetailResponse {
  sucesso: boolean;
  mensagem: string;
  claim: Claim;
}

export interface ClaimSearchResponse {
  sucesso: boolean;
  mensagem: string;
  claim: Claim;
}
 
