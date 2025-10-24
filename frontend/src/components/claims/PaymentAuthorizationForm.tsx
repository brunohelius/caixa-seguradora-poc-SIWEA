import React, { useState } from 'react';
import CurrencyInput from '../common/CurrencyInput';
import type { Claim } from '../../models/Claim';

interface PaymentAuthorizationFormProps {
  claim: Claim;
  onSuccess: (authorizationDetails: any) => void;
  onCancel?: () => void;
}

interface FormData {
  tipoPagamento: number;
  valorPrincipal: number;
  valorCorrecao: number;
  favorecido: string;
  tipoApolice: string;
  observacoes: string;
}

interface FormErrors {
  [key: string]: string;
}

/**
 * Payment Authorization Form Component
 * Handles payment creation with validation and user feedback
 */
const PaymentAuthorizationForm: React.FC<PaymentAuthorizationFormProps> = ({
  claim,
  onSuccess,
  onCancel
}) => {
  const [formData, setFormData] = useState<FormData>({
    tipoPagamento: 1,
    valorPrincipal: 0,
    valorCorrecao: 0,
    favorecido: '',
    tipoApolice: '1',
    observacoes: ''
  });

  const [errors, setErrors] = useState<FormErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [apiError, setApiError] = useState<string>('');

  // Payment type options
  const paymentTypes = [
    { value: 1, label: '1 - Pagamento Principal' },
    { value: 2, label: '2 - Correção Monetária' },
    { value: 3, label: '3 - Pagamento Parcial' },
    { value: 4, label: '4 - Pagamento Total' },
    { value: 5, label: '5 - Reembolso' }
  ];

  // Validation
  const validateForm = (): boolean => {
    const newErrors: FormErrors = {};

    if (formData.tipoPagamento < 1 || formData.tipoPagamento > 5) {
      newErrors.tipoPagamento = 'Tipo de pagamento deve ser 1, 2, 3, 4 ou 5';
    }

    if (formData.valorPrincipal <= 0) {
      newErrors.valorPrincipal = 'Valor principal deve ser maior que zero';
    }

    if (formData.valorCorrecao < 0) {
      newErrors.valorCorrecao = 'Valor de correção não pode ser negativo';
    }

    // Check if total exceeds pending value
    const totalValue = formData.valorPrincipal + formData.valorCorrecao;
    if (totalValue > claim.valorPendente) {
      newErrors.valorPrincipal = `Valor total (R$ ${totalValue.toFixed(2)}) excede o saldo pendente (R$ ${claim.valorPendente.toFixed(2)})`;
    }

    // Favorecido is required if tpsegu != 0
    if (claim.tpsegu !== 0 && !formData.favorecido.trim()) {
      newErrors.favorecido = 'Favorecido é obrigatório para este tipo de seguro';
    }

    if (formData.favorecido.length > 100) {
      newErrors.favorecido = 'Nome do favorecido deve ter no máximo 100 caracteres';
    }

    if (!/^[12]$/.test(formData.tipoApolice)) {
      newErrors.tipoApolice = 'Tipo de apólice deve ser 1 ou 2';
    }

    if (formData.observacoes.length > 500) {
      newErrors.observacoes = 'Observações devem ter no máximo 500 caracteres';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setApiError('');

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);

    try {
      // Call API (this would be imported from services)
      const response = await fetch(
        `${import.meta.env.VITE_API_BASE_URL}/api/claims/${claim.tipseg}/${claim.orgsin}/${claim.rmosin}/${claim.numsin}/authorize-payment`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({
            tipoPagamento: formData.tipoPagamento,
            valorPrincipal: formData.valorPrincipal,
            valorCorrecao: formData.valorCorrecao,
            favorecido: formData.favorecido || null,
            tipoApolice: formData.tipoApolice,
            observacoes: formData.observacoes || null
          })
        }
      );

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.mensagem || 'Erro ao autorizar pagamento');
      }

      const result = await response.json();
      onSuccess(result);

      // Reset form
      setFormData({
        tipoPagamento: 1,
        valorPrincipal: 0,
        valorCorrecao: 0,
        favorecido: '',
        tipoApolice: '1',
        observacoes: ''
      });
    } catch (error: any) {
      setApiError(error.message || 'Erro ao processar solicitação');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleFieldChange = (field: keyof FormData, value: any) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error for this field
    if (errors[field]) {
      setErrors(prev => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  return (
    <div className="payment-authorization-form">
      <h3 style={{ marginBottom: '20px', color: '#333' }}>Autorização de Pagamento</h3>

      {apiError && (
        <div className="error-message" style={{
          backgroundColor: '#fee',
          border: '1px solid #e80c4d',
          padding: '12px',
          borderRadius: '4px',
          marginBottom: '16px',
          color: '#e80c4d'
        }}>
          {apiError}
        </div>
      )}

      <form onSubmit={handleSubmit}>
        {/* Payment Type */}
        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label htmlFor="tipoPagamento" className="form-label">
            Tipo de Pagamento <span style={{ color: '#e80c4d' }}>*</span>
          </label>
          <select
            id="tipoPagamento"
            className={`input-field ${errors.tipoPagamento ? 'input-error' : ''}`}
            value={formData.tipoPagamento}
            onChange={(e) => handleFieldChange('tipoPagamento', parseInt(e.target.value))}
            style={{
              width: '100%',
              padding: '8px 12px',
              border: errors.tipoPagamento ? '1px solid #e80c4d' : '1px solid #ccc',
              borderRadius: '4px'
            }}
          >
            {paymentTypes.map(type => (
              <option key={type.value} value={type.value}>
                {type.label}
              </option>
            ))}
          </select>
          {errors.tipoPagamento && (
            <span className="error-message" style={{ color: '#e80c4d', fontSize: '12px' }}>
              {errors.tipoPagamento}
            </span>
          )}
        </div>

        {/* Principal Value */}
        <CurrencyInput
          label="Valor Principal"
          value={formData.valorPrincipal}
          onChange={(value) => handleFieldChange('valorPrincipal', value)}
          required
          error={errors.valorPrincipal}
          min={0.01}
          max={claim.valorPendente}
        />

        {/* Correction Value */}
        <CurrencyInput
          label="Valor de Correção"
          value={formData.valorCorrecao}
          onChange={(value) => handleFieldChange('valorCorrecao', value)}
          error={errors.valorCorrecao}
          min={0}
        />

        {/* Beneficiary */}
        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label htmlFor="favorecido" className="form-label">
            Favorecido
            {claim.tpsegu !== 0 && <span style={{ color: '#e80c4d' }}> *</span>}
          </label>
          <input
            id="favorecido"
            type="text"
            className={`input-field ${errors.favorecido ? 'input-error' : ''}`}
            value={formData.favorecido}
            onChange={(e) => handleFieldChange('favorecido', e.target.value)}
            maxLength={100}
            placeholder="Nome do beneficiário"
            style={{
              width: '100%',
              padding: '8px 12px',
              border: errors.favorecido ? '1px solid #e80c4d' : '1px solid #ccc',
              borderRadius: '4px'
            }}
          />
          {errors.favorecido && (
            <span className="error-message" style={{ color: '#e80c4d', fontSize: '12px' }}>
              {errors.favorecido}
            </span>
          )}
        </div>

        {/* Policy Type */}
        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label className="form-label">
            Tipo de Apólice <span style={{ color: '#e80c4d' }}>*</span>
          </label>
          <div style={{ display: 'flex', gap: '16px' }}>
            <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }}>
              <input
                type="radio"
                name="tipoApolice"
                value="1"
                checked={formData.tipoApolice === '1'}
                onChange={(e) => handleFieldChange('tipoApolice', e.target.value)}
                style={{ marginRight: '8px' }}
              />
              Tipo 1
            </label>
            <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }}>
              <input
                type="radio"
                name="tipoApolice"
                value="2"
                checked={formData.tipoApolice === '2'}
                onChange={(e) => handleFieldChange('tipoApolice', e.target.value)}
                style={{ marginRight: '8px' }}
              />
              Tipo 2
            </label>
          </div>
        </div>

        {/* Observations */}
        <div className="form-group" style={{ marginBottom: '16px' }}>
          <label htmlFor="observacoes" className="form-label">
            Observações
          </label>
          <textarea
            id="observacoes"
            className={`input-field ${errors.observacoes ? 'input-error' : ''}`}
            value={formData.observacoes}
            onChange={(e) => handleFieldChange('observacoes', e.target.value)}
            maxLength={500}
            rows={3}
            placeholder="Observações adicionais (opcional)"
            style={{
              width: '100%',
              padding: '8px 12px',
              border: errors.observacoes ? '1px solid #e80c4d' : '1px solid #ccc',
              borderRadius: '4px',
              fontFamily: 'inherit',
              resize: 'vertical'
            }}
          />
          <span style={{ fontSize: '11px', color: '#666' }}>
            {formData.observacoes.length}/500 caracteres
          </span>
          {errors.observacoes && (
            <span className="error-message" style={{ color: '#e80c4d', fontSize: '12px', display: 'block' }}>
              {errors.observacoes}
            </span>
          )}
        </div>

        {/* Action Buttons */}
        <div style={{ display: 'flex', gap: '12px', marginTop: '24px' }}>
          <button
            type="submit"
            className="button-primary"
            disabled={isSubmitting}
            style={{
              flex: 1,
              padding: '10px 20px',
              backgroundColor: isSubmitting ? '#ccc' : '#007bff',
              color: '#fff',
              border: 'none',
              borderRadius: '4px',
              cursor: isSubmitting ? 'not-allowed' : 'pointer',
              fontSize: '14px',
              fontWeight: 'bold'
            }}
          >
            {isSubmitting ? 'Processando...' : 'Autorizar Pagamento'}
          </button>

          {onCancel && (
            <button
              type="button"
              onClick={onCancel}
              className="button-secondary"
              disabled={isSubmitting}
              style={{
                flex: 1,
                padding: '10px 20px',
                backgroundColor: '#fff',
                color: '#333',
                border: '1px solid #ccc',
                borderRadius: '4px',
                cursor: isSubmitting ? 'not-allowed' : 'pointer',
                fontSize: '14px'
              }}
            >
              Cancelar
            </button>
          )}
        </div>
      </form>
    </div>
  );
};

export default PaymentAuthorizationForm;
