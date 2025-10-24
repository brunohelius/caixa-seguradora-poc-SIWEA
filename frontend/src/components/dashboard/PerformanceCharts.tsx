/** T127 [US6] - Performance Charts (using Recharts) */
import React, { useState } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { useQuery } from '@tanstack/react-query';

const PerformanceCharts: React.FC = () => {
  const [period, setPeriod] = useState('ultimo-dia');

  const { data: metrics } = useQuery({
    queryKey: ['performance', period],
    queryFn: async () => {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/dashboard/performance?periodo=${period}`);
      if (!response.ok) throw new Error('Failed to fetch performance data');
      return response.json();
    }
  });

  const chartData = metrics?.metricas?.map((m: any) => ({
    name: m.tipoMetrica,
    Legacy: m.valorLegado,
    Novo: m.valorNovo,
    Melhoria: m.percentualMelhoria
  })) || [];

  return (
    <div className="card shadow-sm mb-4">
      <div className="card-body">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h5 className="card-title mb-0">Performance: Legacy vs Novo</h5>
          <select className="form-select form-select-sm" style={{ width: '150px' }} value={period} onChange={e => setPeriod(e.target.value)}>
            <option value="ultima-hora">Última Hora</option>
            <option value="ultimo-dia">Último Dia</option>
            <option value="ultima-semana">Última Semana</option>
            <option value="ultimo-mes">Último Mês</option>
          </select>
        </div>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <Tooltip />
            <Legend />
            <Bar dataKey="Legacy" fill="#dc3545" />
            <Bar dataKey="Novo" fill="#28a745" />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
};

export default PerformanceCharts;
