/**
 * Phase TypeScript interfaces matching PhaseRecordDto
 * T101 [US5] - Frontend model for claim phases
 */

export interface PhaseRecord {
  codFase: number;
  nomeFase: string | null;
  codEvento: number;
  nomeEvento: string | null;
  numOcorrSiniaco: number;
  dataInivigRefaev: string;
  dataAberturaSifa: string;
  dataFechaSifa: string;
  isOpen: boolean;
  status: string; // "Aberta" or "Fechada"
  diasAberta: number;
  dataAberturaFormatada: string;
  dataFechaFormatada: string;
  durationColorCode: 'green' | 'yellow' | 'red';
}

export interface PhaseResponse {
  sucesso: boolean;
  protocolo: string;
  totalFases: number;
  fases: PhaseRecord[];
}

export interface ProtocolKey {
  fonte: number;
  protsini: number;
  dac: number;
}
