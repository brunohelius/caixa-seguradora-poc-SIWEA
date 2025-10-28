/**
 * Statistics Card for Legacy System Dashboard
 */

import React from 'react';
import { Users, Activity, Database, Zap, TrendingUp } from 'lucide-react';
import type { SystemStatistics } from '../../services/legacyDocsService';

interface StatisticsCardProps {
  statistics: SystemStatistics;
}

export const StatisticsCard: React.FC<StatisticsCardProps> = ({ statistics }) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      {/* Active Users */}
      <div className="card-modern">
        <div className="card-body">
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <p className="text-sm text-gray-600 font-semibold mb-2">Usuários Ativos</p>
              <p className="text-4xl font-black text-blue-900">
                {statistics.activeUsers.toLocaleString('pt-BR')}
              </p>
              <p className="text-xs text-gray-500 mt-1">Operadores certificados</p>
            </div>
            <Users className="w-16 h-16 text-blue-200" />
          </div>
        </div>
      </div>

      {/* Daily Transactions */}
      <div className="card-modern">
        <div className="card-body">
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <p className="text-sm text-gray-600 font-semibold mb-2">Transações/Dia</p>
              <p className="text-4xl font-black text-green-900">
                {statistics.dailyTransactions.toLocaleString('pt-BR')}
              </p>
              <p className="text-xs text-gray-500 mt-1">Operações processadas</p>
            </div>
            <Activity className="w-16 h-16 text-green-200" />
          </div>
        </div>
      </div>

      {/* Data Volume */}
      <div className="card-modern">
        <div className="card-body">
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <p className="text-sm text-gray-600 font-semibold mb-2">Volume de Dados</p>
              <p className="text-4xl font-black text-purple-900">
                {statistics.totalDataVolume.toLocaleString('pt-BR')}
              </p>
              <p className="text-xs text-gray-500 mt-1">{statistics.dataVolumeUnit}</p>
            </div>
            <Database className="w-16 h-16 text-purple-200" />
          </div>
        </div>
      </div>

      {/* Average Response Time */}
      <div className="card-modern">
        <div className="card-body">
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <p className="text-sm text-gray-600 font-semibold mb-2">Tempo Médio de Resposta</p>
              <p className="text-4xl font-black text-yellow-900">
                {statistics.averageResponseTime.toLocaleString('pt-BR', {
                  minimumFractionDigits: 1,
                  maximumFractionDigits: 1,
                })}s
              </p>
              <p className="text-xs text-gray-500 mt-1">Performance de consulta</p>
            </div>
            <Zap className="w-16 h-16 text-yellow-200" />
          </div>
        </div>
      </div>

      {/* Availability */}
      <div className="card-modern">
        <div className="card-body">
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <p className="text-sm text-gray-600 font-semibold mb-2">Disponibilidade</p>
              <p className="text-4xl font-black text-green-900">
                {statistics.availability.toLocaleString('pt-BR', {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2,
                })}%
              </p>
              <p className="text-xs text-gray-500 mt-1">Uptime anual</p>
            </div>
            <TrendingUp className="w-16 h-16 text-green-200" />
          </div>
        </div>
      </div>

      {/* Criticality Level */}
      <div className="card-modern bg-gradient-to-br from-red-50 to-orange-50 border-2 border-red-300">
        <div className="card-body">
          <div className="flex items-center justify-between">
            <div className="flex-1">
              <p className="text-sm text-red-700 font-semibold mb-2">Nível de Criticidade</p>
              <p className="text-2xl font-black text-red-900">
                {statistics.criticalityLevel}
              </p>
              <p className="text-xs text-red-700 mt-1">Sistema essencial</p>
            </div>
            <div className="w-16 h-16 rounded-full bg-red-200 flex items-center justify-center">
              <span className="text-3xl">⚠️</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default StatisticsCard;
