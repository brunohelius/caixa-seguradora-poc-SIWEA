/**
 * SearchForm Component
 * T045 [US1] - Form component for claim search
 * Migrated to shadcn/ui components
 */

import React, { useState } from 'react';
import type { ClaimSearchCriteria } from '../../models/Claim';
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Card, CardContent } from "@/components/ui/card";
import { Loader2 } from "lucide-react";

interface SearchFormProps {
  onSearch: (criteria: ClaimSearchCriteria) => void;
  loading?: boolean;
}

type SearchType = 'protocol' | 'claim' | 'leader';

export const SearchForm: React.FC<SearchFormProps> = ({ onSearch, loading = false }) => {
  const [searchType, setSearchType] = useState<SearchType>('protocol');

  // Protocol fields
  const [fonte, setFonte] = useState<string>('');
  const [protsini, setProtsini] = useState<string>('');
  const [dac, setDac] = useState<string>('');

  // Claim number fields
  const [tipseg, setTipseg] = useState<string>('');
  const [orgsin, setOrgsin] = useState<string>('');
  const [rmosin, setRmosin] = useState<string>('');
  const [numsin, setNumsin] = useState<string>('');

  // Leader fields
  const [codlider, setCodlider] = useState<string>('');
  const [sinlid, setSinlid] = useState<string>('');

  // Validation errors
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (searchType === 'protocol') {
      if (!fonte) newErrors.fonte = 'Fonte é obrigatório';
      if (!protsini) newErrors.protsini = 'Protocolo é obrigatório';
      if (!dac) newErrors.dac = 'DAC é obrigatório';
      else if (!/^[0-9]$/.test(dac)) newErrors.dac = 'DAC deve ser um dígito de 0-9';
    } else if (searchType === 'claim') {
      if (!tipseg) newErrors.tipseg = 'Tipo de seguro é obrigatório';
      if (!orgsin) newErrors.orgsin = 'Origem é obrigatória';
      if (!rmosin) newErrors.rmosin = 'Ramo é obrigatório';
      if (!numsin) newErrors.numsin = 'Número do sinistro é obrigatório';
    } else if (searchType === 'leader') {
      if (!codlider) newErrors.codlider = 'Código líder é obrigatório';
      if (!sinlid) newErrors.sinlid = 'Sinistro líder é obrigatório';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) {
      return;
    }

    const criteria: ClaimSearchCriteria = {};

    if (searchType === 'protocol') {
      criteria.fonte = parseInt(fonte);
      criteria.protsini = parseInt(protsini);
      criteria.dac = parseInt(dac);
    } else if (searchType === 'claim') {
      criteria.tipseg = parseInt(tipseg);
      criteria.orgsin = parseInt(orgsin);
      criteria.rmosin = parseInt(rmosin);
      criteria.numsin = parseInt(numsin);
    } else if (searchType === 'leader') {
      criteria.codlider = parseInt(codlider);
      criteria.sinlid = parseInt(sinlid);
    }

    onSearch(criteria);
  };

  const handleClear = () => {
    setFonte('');
    setProtsini('');
    setDac('');
    setTipseg('');
    setOrgsin('');
    setRmosin('');
    setNumsin('');
    setCodlider('');
    setSinlid('');
    setErrors({});
  };

  return (
    <Card>
      <CardContent className="pt-6">
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Search Type Selector */}
          <div>
            <Label className="text-base font-semibold mb-3 block">Tipo de Pesquisa</Label>
            <RadioGroup
              value={searchType}
              onValueChange={(value) => setSearchType(value as SearchType)}
              className="flex flex-wrap gap-4"
            >
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="protocol" id="searchProtocol" />
                <Label htmlFor="searchProtocol" className="cursor-pointer font-normal">
                  Por Protocolo
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="claim" id="searchClaim" />
                <Label htmlFor="searchClaim" className="cursor-pointer font-normal">
                  Por Número do Sinistro
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="leader" id="searchLeader" />
                <Label htmlFor="searchLeader" className="cursor-pointer font-normal">
                  Por Código Líder
                </Label>
              </div>
            </RadioGroup>
          </div>

          {/* Protocol Fields */}
          {searchType === 'protocol' && (
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-2">
                <Label htmlFor="fonte">Fonte *</Label>
                <Input
                  type="number"
                  id="fonte"
                  value={fonte}
                  onChange={(e) => setFonte(e.target.value)}
                  placeholder="Ex: 1"
                  className={errors.fonte ? 'border-destructive' : ''}
                  aria-label="Fonte"
                  aria-required="true"
                />
                {errors.fonte && (
                  <p className="text-sm text-destructive">{errors.fonte}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="protsini">Número do Protocolo *</Label>
                <Input
                  type="number"
                  id="protsini"
                  value={protsini}
                  onChange={(e) => setProtsini(e.target.value)}
                  placeholder="Ex: 123456"
                  className={errors.protsini ? 'border-destructive' : ''}
                  aria-label="Número do Protocolo"
                />
                {errors.protsini && (
                  <p className="text-sm text-destructive">{errors.protsini}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="dac">DAC *</Label>
                <Input
                  type="number"
                  id="dac"
                  value={dac}
                  onChange={(e) => setDac(e.target.value)}
                  placeholder="0-9"
                  min="0"
                  max="9"
                  className={errors.dac ? 'border-destructive' : ''}
                  aria-label="DAC"
                  aria-required="true"
                />
                {errors.dac && (
                  <p className="text-sm text-destructive">{errors.dac}</p>
                )}
              </div>
            </div>
          )}

          {/* Claim Number Fields */}
          {searchType === 'claim' && (
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div className="space-y-2">
                <Label htmlFor="tipseg">Tipo Seguro *</Label>
                <Input
                  type="number"
                  id="tipseg"
                  value={tipseg}
                  onChange={(e) => setTipseg(e.target.value)}
                  className={errors.tipseg ? 'border-destructive' : ''}
                />
                {errors.tipseg && (
                  <p className="text-sm text-destructive">{errors.tipseg}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="orgsin">Origem *</Label>
                <Input
                  type="number"
                  id="orgsin"
                  value={orgsin}
                  onChange={(e) => setOrgsin(e.target.value)}
                  className={errors.orgsin ? 'border-destructive' : ''}
                />
                {errors.orgsin && (
                  <p className="text-sm text-destructive">{errors.orgsin}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="rmosin">Ramo *</Label>
                <Input
                  type="number"
                  id="rmosin"
                  value={rmosin}
                  onChange={(e) => setRmosin(e.target.value)}
                  className={errors.rmosin ? 'border-destructive' : ''}
                />
                {errors.rmosin && (
                  <p className="text-sm text-destructive">{errors.rmosin}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="numsin">Número *</Label>
                <Input
                  type="number"
                  id="numsin"
                  value={numsin}
                  onChange={(e) => setNumsin(e.target.value)}
                  className={errors.numsin ? 'border-destructive' : ''}
                />
                {errors.numsin && (
                  <p className="text-sm text-destructive">{errors.numsin}</p>
                )}
              </div>
            </div>
          )}

          {/* Leader Fields */}
          {searchType === 'leader' && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="codlider">Código Líder *</Label>
                <Input
                  type="number"
                  id="codlider"
                  value={codlider}
                  onChange={(e) => setCodlider(e.target.value)}
                  className={errors.codlider ? 'border-destructive' : ''}
                />
                {errors.codlider && (
                  <p className="text-sm text-destructive">{errors.codlider}</p>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="sinlid">Sinistro Líder *</Label>
                <Input
                  type="number"
                  id="sinlid"
                  value={sinlid}
                  onChange={(e) => setSinlid(e.target.value)}
                  className={errors.sinlid ? 'border-destructive' : ''}
                />
                {errors.sinlid && (
                  <p className="text-sm text-destructive">{errors.sinlid}</p>
                )}
              </div>
            </div>
          )}

          {/* Action Buttons */}
          <div className="flex gap-3">
            <Button type="submit" disabled={loading}>
              {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {loading ? 'Pesquisando...' : 'Pesquisar'}
            </Button>
            <Button type="button" variant="outline" onClick={handleClear} disabled={loading}>
              Limpar
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
};
