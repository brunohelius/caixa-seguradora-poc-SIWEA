/**
 * ClaimInfoCard Component
 * T047 [US1] - Reusable card component for displaying claim field groups
 * Migrated to shadcn/ui components
 */

import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';

export interface ClaimField {
  label: string;
  value: any;
  format?: 'currency' | 'date' | 'percentage' | 'text';
}

interface ClaimInfoCardProps {
  title: string;
  fields: ClaimField[];
  className?: string;
}

/**
 * Format value based on specified format type
 */
function formatValue(value: any, format?: ClaimField['format']): string {
  if (value === null || value === undefined) {
    return '-';
  }

  switch (format) {
    case 'currency':
      return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL',
      }).format(value);

    case 'date':
      return new Intl.DateTimeFormat('pt-BR').format(new Date(value));

    case 'percentage':
      return `${value}%`;

    case 'text':
    default:
      return String(value);
  }
}

export const ClaimInfoCard: React.FC<ClaimInfoCardProps> = ({
  title,
  fields,
  className = ''
}) => {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="text-lg">{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <dl className="space-y-3">
          {fields.map((field, index) => (
            <div key={index}>
              {index > 0 && <Separator className="my-3" />}
              <div className="grid grid-cols-3 gap-4">
                <dt className="text-sm font-medium text-muted-foreground">
                  {field.label}:
                </dt>
                <dd className="col-span-2 text-sm font-semibold">
                  {formatValue(field.value, field.format)}
                </dd>
              </div>
            </div>
          ))}
        </dl>
      </CardContent>
    </Card>
  );
};
