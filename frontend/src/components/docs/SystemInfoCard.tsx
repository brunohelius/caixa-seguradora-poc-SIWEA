/**
 * System Information Card for Legacy System Dashboard
 */

import React from 'react';
import { Server, Code, Database, Calendar } from 'lucide-react';
import type { LegacySystemInfo } from '../../services/legacyDocsService';

interface SystemInfoCardProps {
  systemInfo: LegacySystemInfo;
}

export const SystemInfoCard: React.FC<SystemInfoCardProps> = ({ systemInfo }) => {
  return (
    <div className="card-modern">
      <div className="card-header bg-gradient-caixa text-white">
        <div className="flex items-center gap-3">
          <Server className="w-6 h-6" />
          <h2 className="text-2xl font-bold">Informações do Sistema Legado</h2>
        </div>
      </div>
      <div className="card-body">
        <div className="space-y-6">
          {/* Program ID and Name */}
          <div>
            <p className="text-sm font-semibold uppercase tracking-wide mb-2 text-gray-600">
              Programa:
            </p>
            <p className="text-3xl font-black text-blue-900 mb-2">
              {systemInfo.programId}
            </p>
            <p className="text-base text-gray-700">
              {systemInfo.programName}
            </p>
          </div>

          {/* Key Info Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="p-4 rounded-lg bg-blue-50 border-2 border-blue-200">
              <div className="flex items-center gap-2 mb-2">
                <Code className="w-5 h-5 text-blue-700" />
                <p className="text-sm font-bold text-blue-700">Linguagem</p>
              </div>
              <p className="text-lg font-semibold text-blue-900">{systemInfo.language}</p>
            </div>

            <div className="p-4 rounded-lg bg-purple-50 border-2 border-purple-200">
              <div className="flex items-center gap-2 mb-2">
                <Database className="w-5 h-5 text-purple-700" />
                <p className="text-sm font-bold text-purple-700">Banco de Dados</p>
              </div>
              <p className="text-lg font-semibold text-purple-900">{systemInfo.database}</p>
            </div>

            <div className="p-4 rounded-lg bg-green-50 border-2 border-green-200">
              <div className="flex items-center gap-2 mb-2">
                <Calendar className="w-5 h-5 text-green-700" />
                <p className="text-sm font-bold text-green-700">Tempo em Produção</p>
              </div>
              <p className="text-2xl font-black text-green-900">
                {systemInfo.yearsInProduction} anos
              </p>
              <p className="text-xs text-green-700 mt-1">
                Desde {new Date(systemInfo.creationDate).getFullYear()}
              </p>
            </div>

            <div className="p-4 rounded-lg bg-yellow-50 border-2 border-yellow-200">
              <div className="flex items-center gap-2 mb-2">
                <Server className="w-5 h-5 text-yellow-700" />
                <p className="text-sm font-bold text-yellow-700">Plataforma</p>
              </div>
              <p className="text-lg font-semibold text-yellow-900">{systemInfo.platform}</p>
            </div>
          </div>

          {/* Additional Details */}
          <div className="border-t pt-4">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm">
              <div>
                <p className="text-gray-600 font-semibold mb-1">Tamanho do Código</p>
                <p className="text-gray-900 font-mono">{systemInfo.codeSize}</p>
              </div>
              <div>
                <p className="text-gray-600 font-semibold mb-1">Versão Atual</p>
                <p className="text-gray-900">{systemInfo.currentVersion}</p>
              </div>
              <div>
                <p className="text-gray-600 font-semibold mb-1">Última Manutenção</p>
                <p className="text-gray-900">{systemInfo.lastMaintenance}</p>
              </div>
            </div>
          </div>

          {/* Team Info */}
          <div className="p-4 rounded-lg bg-gray-50">
            <p className="text-sm font-bold text-gray-700 mb-2">Equipe Original</p>
            <div className="grid grid-cols-2 gap-2 text-sm">
              <div>
                <span className="text-gray-600">Analista:</span>{' '}
                <span className="font-semibold text-gray-900">{systemInfo.originalAnalyst}</span>
              </div>
              <div>
                <span className="text-gray-600">Programadora:</span>{' '}
                <span className="font-semibold text-gray-900">{systemInfo.originalProgrammer}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SystemInfoCard;
