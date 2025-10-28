import React from 'react';

/**
 * T132 [UI Polish] - Caixa Seguradora Logo Component
 * Displays the logo from base64 PNG data
 */

// Base64 PNG logo data (placeholder - replace with actual logo from spec.md)
const LOGO_BASE64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

interface LogoProps {
  maxHeight?: string;
  alt?: string;
  className?: string;
}

export const Logo: React.FC<LogoProps> = ({
  maxHeight = '60px',
  alt = 'Caixa Seguradora',
  className = ''
}) => {
  return (
    <img
      src={LOGO_BASE64}
      alt={alt}
      style={{ maxHeight, width: 'auto' }}
      className={`caixa-logo ${className}`}
      onError={(e) => {
        // Fallback to text if image fails to load
        const target = e.target as HTMLImageElement;
        target.style.display = 'none';
        const fallback = target.nextElementSibling as HTMLSpanElement;
        if (fallback) {
          fallback.style.display = 'inline-block';
        }
      }}
    />
  );
};

// Fallback text component
export const LogoFallback: React.FC<{ text?: string }> = ({ text = 'Caixa Seguradora' }) => {
  return (
    <span
      className="caixa-logo-fallback"
      style={{
        display: 'none',
        fontWeight: 'bold',
        fontSize: '1.5rem',
        color: '#00A859'
      }}
    >
      {text}
    </span>
  );
};

export default Logo;
