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

// Caixa Seguradora logo as base64 PNG (placeholder - replace with actual logo)
const LOGO_BASE64 = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==';

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
      <div className="container mt-4">
        <div className="alert alert-warning">
          Nenhum sinistro selecionado.
          <button className="btn btn-link" onClick={() => navigate('/claims/search')}>
            Voltar para pesquisa
          </button>
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
    <div className="container mt-4">
      {/* Header with Logo */}
      <div className="card shadow mb-4">
        <div className="card-body text-center">
          <img
            src={LOGO_BASE64}
            alt="Caixa Seguradora"
            className="mb-3"
            style={{ maxHeight: '80px' }}
          />
          <h2>Detalhes do Sinistro</h2>
          <p className="text-muted mb-0">{claim.numeroSinistro}</p>
        </div>
      </div>

      {/* Success Message */}
      {successMessage && (
        <div className="alert alert-success d-flex align-items-center mb-4" role="alert">
          <i className="bi bi-check-circle-fill me-3 fs-4"></i>
          <div>
            {successMessage}
            {authorizationDetails && (
              <div style={{ marginTop: '8px', fontSize: '13px' }}>
                <strong>Detalhes:</strong><br />
                Data: {authorizationDetails.dataMovimento} {authorizationDetails.horaOperacao}<br />
                Valor Total BTNF: R$ {authorizationDetails.valorTotalBTNF.toFixed(2)}
              </div>
            )}
          </div>
        </div>
      )}

      {/* Action Buttons */}
      <div className="d-flex gap-2 mb-4">
        <button
          className="btn btn-secondary"
          onClick={() => navigate('/claims/search')}
        >
          <i className="bi bi-arrow-left me-2"></i>
          Voltar para Pesquisa
        </button>

        {claim.valorPendente > 0 && !showPaymentForm && (
          <button
            className="btn btn-success"
            onClick={handleAuthorizePayment}
          >
            <i className="bi bi-check-circle me-2"></i>
            Autorizar Pagamento
          </button>
        )}
      </div>

      {/* Payment Authorization Form */}
      {showPaymentForm && (
        <div className="card shadow mb-4">
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
        <div className="alert alert-warning d-flex align-items-center mb-4" role="alert">
          <i className="bi bi-exclamation-triangle-fill me-3 fs-4"></i>
          <div>
            <strong>Valor Pendente:</strong> {new Intl.NumberFormat('pt-BR', {
              style: 'currency',
              currency: 'BRL',
            }).format(claim.valorPendente)}
            <br />
            <small>Este sinistro possui valores pendentes de autorização.</small>
          </div>
        </div>
      )}

      {/* Tabs Navigation */}
      <ul className="nav nav-tabs mb-4">
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'details' ? 'active' : ''}`}
            onClick={() => setActiveTab('details')}
          >
            <i className="bi bi-info-circle me-2"></i>
            Detalhes
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'history' ? 'active' : ''}`}
            onClick={() => setActiveTab('history')}
          >
            <i className="bi bi-clock-history me-2"></i>
            Histórico de Pagamentos
          </button>
        </li>
        <li className="nav-item">
          <button
            className={`nav-link ${activeTab === 'phases' ? 'active' : ''}`}
            onClick={() => setActiveTab('phases')}
          >
            <i className="bi bi-diagram-3 me-2"></i>
            Fases do Sinistro
          </button>
        </li>
      </ul>

      {/* Tab Content */}
      {activeTab === 'details' && (
        <div className="row g-3">
          <div className="col-md-6">
            <ClaimInfoCard title="Informações do Protocolo" fields={protocolFields} />
          </div>
          <div className="col-md-6">
            <ClaimInfoCard title="Informações Financeiras" fields={financialFields} />
          </div>
          <div className="col-md-6">
            <ClaimInfoCard title="Informações da Apólice" fields={policyFields} />
          </div>
          <div className="col-md-6">
            <ClaimInfoCard title="Informações do Fluxo" fields={workflowFields} />
          </div>
          {(claim.codlider || claim.sinlid) && (
            <div className="col-md-6">
              <ClaimInfoCard title="Informações do Líder (Resseguro)" fields={leaderFields} />
            </div>
          )}
        </div>
      )}

      {activeTab === 'history' && (
        <div className="card shadow">
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
        <div className="card shadow">
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
