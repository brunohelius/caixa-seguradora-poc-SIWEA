/**
 * Phase Service
 * T101 [US5] - API client for claim phase operations
 */

import axios from 'axios';
import type { PhaseResponse, ProtocolKey } from '../models/Phase';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001';

export const phaseService = {
  /**
   * Get all phases for a claim protocol
   * GET /api/claims/{fonte}/{protsini}/{dac}/phases
   */
  async getClaimPhases(protocolKey: ProtocolKey): Promise<PhaseResponse> {
    const { fonte, protsini, dac } = protocolKey;
    const response = await axios.get<PhaseResponse>(
      `${API_BASE_URL}/api/claims/${fonte}/${protsini}/${dac}/phases`
    );
    return response.data;
  }
};
