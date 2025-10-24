/**
 * T080: HistoryTable Component
 * Reusable table component with sorting, sticky header, and export functionality
 * Migrated to shadcn/ui components
 */

import React, { useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { ArrowUpDown, ArrowUp, ArrowDown, Download, Loader2 } from "lucide-react";

interface HistoryRecord {
  tipseg: number;
  orgsin: number;
  rmosin: number;
  numsin: number;
  ocorhist: number;
  operacao: number;
  dtmovto: string;
  horaoper: string;
  dataHoraFormatada: string;
  valpri: number;
  crrmon: number;
  nomfav: string;
  tipcrr: string;
  valpribt: number;
  crrmonbt: number;
  valtotbt: number;
  sitcontb: string;
  situacao: string;
  ezeusrid: string;
}

interface Column {
  key: keyof HistoryRecord;
  label: string;
  sortable?: boolean;
  format?: (value: any) => string;
}

interface HistoryTableProps {
  records: HistoryRecord[];
  loading?: boolean;
  columns?: Column[];
}

type SortDirection = 'asc' | 'desc' | null;

const defaultColumns: Column[] = [
  { key: 'dataHoraFormatada', label: 'Data/Hora', sortable: true },
  { key: 'ocorhist', label: 'Ocorrência', sortable: true },
  { key: 'operacao', label: 'Operação', sortable: true },
  { key: 'valpri', label: 'Valor Principal', sortable: true },
  { key: 'crrmon', label: 'Correção', sortable: true },
  { key: 'valtotbt', label: 'Valor Total BTNF', sortable: true },
  { key: 'nomfav', label: 'Favorecido', sortable: true },
  { key: 'ezeusrid', label: 'Operador', sortable: true },
];

const HistoryTable: React.FC<HistoryTableProps> = ({
  records,
  loading = false,
  columns = defaultColumns,
}) => {
  const [sortColumn, setSortColumn] = useState<keyof HistoryRecord | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>(null);

  const handleSort = (columnKey: keyof HistoryRecord) => {
    if (sortColumn === columnKey) {
      // Cycle through: asc -> desc -> null
      if (sortDirection === 'asc') {
        setSortDirection('desc');
      } else if (sortDirection === 'desc') {
        setSortDirection(null);
        setSortColumn(null);
      }
    } else {
      setSortColumn(columnKey);
      setSortDirection('asc');
    }
  };

  const getSortedRecords = (): HistoryRecord[] => {
    if (!sortColumn || !sortDirection) {
      return records;
    }

    return [...records].sort((a, b) => {
      const aValue = a[sortColumn];
      const bValue = b[sortColumn];

      // Handle null/undefined values
      if (aValue === null || aValue === undefined) return 1;
      if (bValue === null || bValue === undefined) return -1;

      // Compare values
      let comparison = 0;
      if (typeof aValue === 'number' && typeof bValue === 'number') {
        comparison = aValue - bValue;
      } else {
        comparison = String(aValue).localeCompare(String(bValue));
      }

      return sortDirection === 'asc' ? comparison : -comparison;
    });
  };

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(value);
  };

  const formatValue = (column: Column, value: any): string => {
    if (column.format) {
      return column.format(value);
    }

    // Auto-format currency fields
    if (
      ['valpri', 'crrmon', 'valpribt', 'crrmonbt', 'valtotbt'].includes(column.key) &&
      typeof value === 'number'
    ) {
      return formatCurrency(value);
    }

    // Handle null/undefined
    if (value === null || value === undefined) {
      return '-';
    }

    return String(value);
  };

  const exportToCSV = () => {
    const headers = columns.map((col) => col.label).join(',');
    const rows = records.map((record) =>
      columns
        .map((col) => {
          const value = formatValue(col, record[col.key]);
          // Escape commas and quotes in CSV
          return `"${String(value).replace(/"/g, '""')}"`;
        })
        .join(',')
    );

    const csv = [headers, ...rows].join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', `historico-pagamentos-${Date.now()}.csv`);
    link.style.visibility = 'hidden';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const getSortIcon = (columnKey: keyof HistoryRecord) => {
    if (sortColumn !== columnKey) {
      return <ArrowUpDown className="ml-2 h-4 w-4" />;
    }
    return sortDirection === 'asc' ? (
      <ArrowUp className="ml-2 h-4 w-4" />
    ) : (
      <ArrowDown className="ml-2 h-4 w-4" />
    );
  };

  const sortedRecords = getSortedRecords();

  if (loading) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-20">
          <Loader2 className="h-10 w-10 animate-spin text-primary mb-4" />
          <p className="text-muted-foreground">Carregando registros...</p>
        </CardContent>
      </Card>
    );
  }

  if (records.length === 0) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-20">
          <p className="text-muted-foreground">Nenhum registro encontrado.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={exportToCSV} variant="outline" size="sm">
          <Download className="mr-2 h-4 w-4" />
          Exportar CSV
        </Button>
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="max-h-[600px] overflow-auto">
            <Table>
              <TableHeader className="sticky top-0 bg-muted z-10">
                <TableRow>
                  {columns.map((column) => (
                    <TableHead
                      key={String(column.key)}
                      className={column.sortable ? 'cursor-pointer select-none hover:bg-muted/50' : ''}
                      onClick={() => column.sortable && handleSort(column.key)}
                    >
                      <div className="flex items-center">
                        <span>{column.label}</span>
                        {column.sortable && getSortIcon(column.key)}
                      </div>
                    </TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {sortedRecords.map((record, index) => (
                  <TableRow
                    key={`${record.ocorhist}-${index}`}
                    className="hover:bg-muted/50"
                  >
                    {columns.map((column) => (
                      <TableCell key={String(column.key)}>
                        {formatValue(column, record[column.key])}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default HistoryTable;
