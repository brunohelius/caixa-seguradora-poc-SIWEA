/**
 * Service for accessing legacy system documentation API
 */

import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

export interface LegacySystemInfo {
  programId: string;
  programName: string;
  systemType: string;
  platform: string;
  language: string;
  database: string;
  codeSize: string;
  creationDate: string;
  lastUpdate: string;
  yearsInProduction: number;
  currentVersion: string;
  originalProgrammer: string;
  originalAnalyst: string;
  lastMaintenance: string;
}

export interface SystemStatistics {
  activeUsers: number;
  dailyTransactions: number;
  totalDataVolume: number;
  dataVolumeUnit: string;
  criticalityLevel: string;
  averageResponseTime: number;
  availability: number;
}

export interface DocumentationMetrics {
  totalDocuments: number;
  totalPages: number;
  businessRules: number;
  databaseTables: number;
  externalIntegrations: number;
  screenMaps: number;
  workflowPhases: number;
  dataStructures: number;
}

export interface DocumentDto {
  id: string;
  title: string;
  description: string;
  fileName: string;
  pages: number;
  category: string;
  lastModified: string;
}

export interface TechnologyStack {
  legacyTechnologies: string[];
  migrationTechnologies: string[];
}

export interface BusinessProcess {
  name: string;
  description: string;
  features: string[];
  performanceTarget: string;
}

export interface LegacySystemDashboard {
  systemInfo: LegacySystemInfo;
  statistics: SystemStatistics;
  documentation: DocumentationMetrics;
  availableDocuments: DocumentDto[];
  technologyStack: TechnologyStack;
  businessProcesses: BusinessProcess[];
  lastAnalyzed: string;
}

class LegacyDocsService {
  /**
   * Gets comprehensive dashboard data for the legacy system
   */
  async getDashboard(): Promise<LegacySystemDashboard> {
    const response = await axios.get<LegacySystemDashboard>(`${API_BASE_URL}/api/LegacyDocs/dashboard`);
    return response.data;
  }

  /**
   * Gets list of all available documentation files
   */
  async getDocuments(): Promise<DocumentDto[]> {
    const response = await axios.get<DocumentDto[]>(`${API_BASE_URL}/api/LegacyDocs/documents`);
    return response.data;
  }

  /**
   * Gets content of a specific documentation file
   */
  async getDocumentContent(documentId: string): Promise<string> {
    const response = await axios.get<{ content: string }>(`${API_BASE_URL}/api/LegacyDocs/documents/${documentId}`);
    return response.data.content;
  }
}

export default new LegacyDocsService();
