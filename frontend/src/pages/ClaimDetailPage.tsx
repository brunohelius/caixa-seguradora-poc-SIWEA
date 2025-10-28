/**
 * ClaimDetailPage Component
 * T046 [US1] - Claim detail display page
 * T069 [US2] - Updated with PaymentAuthorizationForm integration
 * T081 [US3] - Updated with PaymentHistoryComponent integration
 * T103 [US5] - Updated with ClaimPhasesComponent integration
 */

import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { ClaimInfoCard, type ClaimField } from '../components/claims/ClaimInfoCard';
import PaymentAuthorizationForm from '../components/claims/PaymentAuthorizationForm';
import PaymentHistoryComponent from '../components/claims/PaymentHistoryComponent';
import ClaimPhasesComponent from '../components/claims/ClaimPhasesComponent';
import type { Claim } from '../models/Claim';

export const ClaimDetailPage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const claim = location.state?.claim as Claim | undefined;

  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string>('');
  const [authorizationDetails, setAuthorizationDetails] = useState<any>(null);
  const [activeTab, setActiveTab] = useState<'details' | 'history' | 'phases'>('details');
  const [refreshHistory, setRefreshHistory] = useState(0);
  const [refreshPhases, setRefreshPhases] = useState(0);

  if (!claim) {
    return (
      <div className="container-modern py-8">
        <div className="alert alert-warning fade-in">
          <svg className="w-5 h-5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <div>
            <strong className="font-semibold">Nenhum sinistro selecionado.</strong>
            <button className="btn btn-ghost mt-2" onClick={() => navigate('/claims/search')}>
              Voltar para pesquisa
            </button>
          </div>
        </div>
      </div>
    );
  }

  const protocolFields: ClaimField[] = [
    { label: 'Número do Protocolo', value: claim.numeroProtocolo },
    { label: 'Número do Sinistro', value: claim.numeroSinistro },
    { label: 'Fonte', value: claim.fonte },
    { label: 'DAC', value: claim.dac },
  ];

  const policyFields: ClaimField[] = [
    { label: 'Número da Apólice', value: claim.numeroApolice },
    { label: 'Seguradora', value: claim.nomeSeguradora },
    { label: 'Ramo', value: claim.nomeRamo },
    { label: 'Tipo de Seguro', value: claim.tpsegu },
    { label: 'Tipo de Registro', value: claim.tipreg },
    { label: 'Produto', value: claim.codprodu },
    { label: 'É Consórcio?', value: claim.ehConsorcio ? 'Sim' : 'Não' },
  ];

  const financialFields: ClaimField[] = [
    { label: 'Saldo a Pagar (Sdopag)', value: claim.sdopag, format: 'currency' },
    { label: 'Total Pago (Totpag)', value: claim.totpag, format: 'currency' },
    {
      label: 'Valor Pendente',
      value: claim.valorPendente,
      format: 'currency'
    },
  ];

  const workflowFields: ClaimField[] = [
    { label: 'Ocorrência Histórico', value: claim.ocorhist },
    { label: 'Criado Por', value: claim.createdBy },
    { label: 'Criado Em', value: claim.createdAt, format: 'date' },
    { label: 'Atualizado Por', value: claim.updatedBy },
    { label: 'Atualizado Em', value: claim.updatedAt, format: 'date' },
  ];

  const leaderFields: ClaimField[] = [
    { label: 'Código Líder', value: claim.codlider },
    { label: 'Sinistro Líder', value: claim.sinlid },
  ];

  const handleAuthorizePayment = () => {
    setShowPaymentForm(true);
    setSuccessMessage('');
    setAuthorizationDetails(null);
  };

  const handlePaymentSuccess = (details: any) => {
    setAuthorizationDetails(details);
    setSuccessMessage(
      `Pagamento autorizado com sucesso! Ocorrência: ${details.ocorhist}. ` +
      `Valor pendente atualizado: R$ ${details.valorPendenteAtualizado.toFixed(2)}`
    );
    setShowPaymentForm(false);

    // Refresh history to show new payment
    setRefreshHistory(prev => prev + 1);

    // Refresh phases to show updated phase status
    setRefreshPhases(prev => prev + 1);

    // Switch to history tab to show the new record
    setActiveTab('history');
  };

  const handlePaymentCancel = () => {
    setShowPaymentForm(false);
  };

  return (
    <div className="container-modern py-8 fade-in">
      {/* Header Card */}
      <div className="card-modern mb-6 text-center">
        <div className="card-body">
          <div className="section-header">
            <h1 className="section-title">Detalhes do Sinistro</h1>
            <p className="section-subtitle">
              <span className="badge badge-blue text-lg">{claim.numeroSinistro}</span>
            </p>
          </div>
        </div>
      </div>

      {/* Success Message */}
      {successMessage && (
        <div className="alert alert-success mb-6 fade-in">
          <svg className="w-6 h-6 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <strong className="font-semibold">{successMessage}</strong>
            {authorizationDetails && (
              <div className="mt-2 text-sm">
                <strong>Detalhes:</strong><br />
                Data: {authorizationDetails.dataMovimento} {authorizationDetails.horaOperacao}<br />
                Valor Total BTNF: R$ {authorizationDetails.valorTotalBTNF.toFixed(2)}
              </div>
            )}
          </div>
        </div>
      )}

      {/* Action Buttons */}
      <div className="flex flex-wrap gap-3 mb-6">
        <button
          className="btn btn-ghost"
          onClick={() => navigate('/claims/search')}
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          Voltar para Pesquisa
        </button>

        {claim.valorPendente > 0 && !showPaymentForm && (
          <button
            className="btn btn-primary"
            onClick={handleAuthorizePayment}
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Autorizar Pagamento
          </button>
        )}
      </div>

      {/* Payment Authorization Form */}
      {showPaymentForm && (
        <div className="card-modern mb-6 fade-in">
          <div className="card-body">
            <PaymentAuthorizationForm
              claim={claim}
              onSuccess={handlePaymentSuccess}
              onCancel={handlePaymentCancel}
            />
          </div>
        </div>
      )}

      {/* Financial Summary - Highlighted if pending */}
      {claim.valorPendente > 0 && (
        <div className="alert alert-warning mb-6">
          <svg className="w-6 h-6 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <div>
            <strong className="font-semibold">Valor Pendente:</strong> {new Intl.NumberFormat('pt-BR', {
              style: 'currency',
              currency: 'BRL',
            }).format(claim.valorPendente)}
            <br />
            <small className="text-sm">Este sinistro possui valores pendentes de autorização.</small>
          </div>
        </div>
      )}

      {/* Tabs Navigation */}
      <div className="border-b border-gray-200 mb-6">
        <nav className="flex gap-2" aria-label="Tabs">
          <button
            className={`px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'details'
                ? 'border-caixa-blue-700 text-caixa-blue-700'
                : 'border-transparent text-gray-600 hover:text-gray-900 hover:border-gray-300'
            }`}
            onClick={() => setActiveTab('details')}
          >
            <svg className="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Detalhes
          </button>
          <button
            className={`px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'history'
                ? 'border-caixa-blue-700 text-caixa-blue-700'
                : 'border-transparent text-gray-600 hover:text-gray-900 hover:border-gray-300'
            }`}
            onClick={() => setActiveTab('history')}
          >
            <svg className="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            Histórico de Pagamentos
          </button>
          <button
            className={`px-4 py-3 text-sm font-medium border-b-2 transition-colors ${
              activeTab === 'phases'
                ? 'border-caixa-blue-700 text-caixa-blue-700'
                : 'border-transparent text-gray-600 hover:text-gray-900 hover:border-gray-300'
            }`}
            onClick={() => setActiveTab('phases')}
          >
            <svg className="w-4 h-4 inline-block mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
            Fases do Sinistro
          </button>
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'details' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 fade-in">
          <ClaimInfoCard title="Informações do Protocolo" fields={protocolFields} />
          <ClaimInfoCard title="Informações Financeiras" fields={financialFields} />
          <ClaimInfoCard title="Informações da Apólice" fields={policyFields} />
          <ClaimInfoCard title="Informações do Fluxo" fields={workflowFields} />
          {(claim.codlider || claim.sinlid) && (
            <ClaimInfoCard title="Informações do Líder (Resseguro)" fields={leaderFields} />
          )}
        </div>
      )}

      {activeTab === 'history' && (
        <div className="card-modern fade-in">
          <div className="card-body">
            <PaymentHistoryComponent
              key={refreshHistory}
              claimKey={{
                tipseg: claim.tipseg,
                orgsin: claim.orgsin,
                rmosin: claim.rmosin,
                numsin: claim.numsin,
              }}
            />
          </div>
        </div>
      )}

      {activeTab === 'phases' && (
        <div className="card-modern fade-in">
          <div className="card-body">
            <ClaimPhasesComponent
              key={refreshPhases}
              protocolKey={{
                fonte: claim.fonte,
                protsini: claim.protsini,
                dac: claim.dac
              }}
              autoRefresh={true}
            />
          </div>
        </div>
      )}
    </div>
  );
};
