import React, { useState, useEffect } from 'react';

interface CurrencyInputProps {
  value: number;
  onChange: (value: number) => void;
  label: string;
  required?: boolean;
  error?: string;
  min?: number;
  max?: number;
  placeholder?: string;
  disabled?: boolean;
}

/**
 * Reusable currency input component with R$ formatting
 * Handles:
 * - Thousands separator
 * - 2 decimal precision
 * - Paste event cleaning
 * - Min/max validation
 */
const CurrencyInput: React.FC<CurrencyInputProps> = ({
  value,
  onChange,
  label,
  required = false,
  error,
  min = 0.01,
  max = 999999999.99,
  placeholder = 'R$ 0,00',
  disabled = false
}) => {
  const [displayValue, setDisplayValue] = useState('');

  // Format number to Brazilian currency string
  const formatCurrency = (num: number): string => {
    if (isNaN(num) || num === 0) return '';

    return num.toLocaleString('pt-BR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    });
  };

  // Parse Brazilian currency string to number
  const parseCurrency = (str: string): number => {
    if (!str) return 0;

    // Remove everything except digits and comma
    const cleaned = str.replace(/[^\d,]/g, '');
    // Replace comma with dot for parsing
    const normalized = cleaned.replace(',', '.');
    const parsed = parseFloat(normalized);

    return isNaN(parsed) ? 0 : parsed;
  };

  // Update display value when prop value changes
  useEffect(() => {
    setDisplayValue(formatCurrency(value));
  }, [value]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const inputValue = e.target.value;

    // Allow empty input
    if (inputValue === '') {
      setDisplayValue('');
      onChange(0);
      return;
    }

    // Parse to number
    const numericValue = parseCurrency(inputValue);

    // Validate min/max
    if (numericValue < 0) return;
    if (max && numericValue > max) return;

    // Update both display and numeric value
    setDisplayValue(formatCurrency(numericValue));
    onChange(numericValue);
  };

  const handleBlur = () => {
    // Reformat on blur to ensure consistency
    if (value > 0) {
      setDisplayValue(formatCurrency(value));
    }
  };

  const handlePaste = (e: React.ClipboardEvent<HTMLInputElement>) => {
    e.preventDefault();
    const pastedText = e.clipboardData.getData('text');

    // Clean pasted text (remove everything except digits, comma, and dot)
    const cleaned = pastedText.replace(/[^\d,\.]/g, '');
    const numericValue = parseCurrency(cleaned);

    if (numericValue >= min && numericValue <= max) {
      setDisplayValue(formatCurrency(numericValue));
      onChange(numericValue);
    }
  };

  return (
    <div className="form-group">
      <label htmlFor={`currency-${label}`} className="form-label">
        {label}
        {required && <span className="required-asterisk" style={{ color: '#e80c4d' }}> *</span>}
      </label>

      <div className="currency-input-wrapper" style={{ position: 'relative' }}>
        <span
          className="currency-symbol"
          style={{
            position: 'absolute',
            left: '10px',
            top: '50%',
            transform: 'translateY(-50%)',
            color: '#666',
            pointerEvents: 'none'
          }}
        >
          R$
        </span>

        <input
          id={`currency-${label}`}
          type="text"
          className={`input-field ${error ? 'input-error' : ''}`}
          value={displayValue}
          onChange={handleChange}
          onBlur={handleBlur}
          onPaste={handlePaste}
          placeholder={placeholder}
          disabled={disabled}
          style={{
            paddingLeft: '35px',
            width: '100%',
            border: error ? '1px solid #e80c4d' : '1px solid #ccc',
            borderRadius: '4px',
            padding: '8px 12px 8px 35px',
            fontSize: '14px'
          }}
        />
      </div>

      {error && (
        <span className="error-message" style={{ color: '#e80c4d', fontSize: '12px', marginTop: '4px', display: 'block' }}>
          {error}
        </span>
      )}

      {!error && min > 0 && (
        <span className="hint-text" style={{ fontSize: '11px', color: '#666', marginTop: '2px', display: 'block' }}>
          Valor mínimo: R$ {formatCurrency(min)}
        </span>
      )}
    </div>
  );
};

export default CurrencyInput;
