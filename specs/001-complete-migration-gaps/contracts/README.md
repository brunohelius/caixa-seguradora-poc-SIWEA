# API Contracts: Complete SIWEA Migration - Gaps Analysis and Implementation

**Feature**: Complete 100% SIWEA Migration
**Branch**: 001-complete-migration-gaps
**Date**: 2025-10-27

## Overview

This directory contains API contract specifications for the migration gap implementations. Contracts define request/response structures, validation rules, error codes, and API endpoints.

## Contract Files

### REST API Contracts (OpenAPI 3.0)

1. **payment-authorization-api.yaml** - Enhanced payment authorization endpoint with external validation
2. **external-validation-api.yaml** - Contract for CNOUA REST API integration
3. **claim-search-api.yaml** - Existing endpoint (reference only, no changes)
4. **dashboard-api.yaml** - Existing endpoint (reference only, no changes)

### SOAP API Contracts (WSDL 1.1)

5. **sipua-service.wsdl** - EFP contract validation SOAP service
6. **simda-service.wsdl** - HB contract validation SOAP service
7. **solicitacao-service.wsdl** - Existing SOAP endpoint (reference only, no changes)

### JSON Schema Contracts

8. **external-validation-request.schema.json** - Request payload for CNOUA/SIPUA/SIMDA
9. **external-validation-response.schema.json** - Response payload from validation services
10. **transaction-context.schema.json** - Transaction state tracking object
11. **business-rule-violation.schema.json** - Business rule validation error format

## API Endpoints Summary

### Enhanced Endpoints

#### POST /api/claims/authorize-payment
**Purpose**: Authorize claim payment with external validation

**Request Body** (application/json):
```json
{
  "claimId": "string (composite key)",
  "tipoPagamento": 1-5 (integer),
  "valorPrincipal": 0.00 (decimal, 2 places),
  "valorCorrecao": 0.00 (decimal, 2 places),
  "tipoApolice": 1-2 (integer),
  "beneficiario": "string (max 255 chars, required if tpSegu != 0)",
  "currencyCode": "BRL",
  "targetCurrencyCode": "BTNF",
  "externalValidationRequired": true (boolean),
  "routingService": "CNOUA|SIPUA|SIMDA|null" (string, nullable)
}
```

**Success Response** (200 OK):
```json
{
  "transactionId": "guid",
  "authorizationId": "string",
  "sucesso": true,
  "mensagem": "Pagamento autorizado com sucesso",
  "erros": [],
  "validationResults": [
    {
      "ezert8": "00000000",
      "errorMessage": null,
      "isSuccess": true,
      "validationService": "CNOUA",
      "requestTimestamp": "2025-10-27T10:30:00Z",
      "responseTimestamp": "2025-10-27T10:30:02Z",
      "elapsedMilliseconds": 2000
    }
  ],
  "rollbackOccurred": false,
  "failedStep": null
}
```

**Error Response** (400 Bad Request - Validation Failure):
```json
{
  "transactionId": "guid",
  "authorizationId": "string",
  "sucesso": false,
  "mensagem": "Validação falhou",
  "erros": [
    {
      "ruleId": "BR-019",
      "ruleName": "Beneficiary Required",
      "errorCode": "VAL-007",
      "errorMessage": "Favorecido é obrigatório para este tipo de seguro",
      "failedValue": null,
      "expectedValue": "string (not null)",
      "severity": "Critical"
    }
  ],
  "validationResults": [],
  "rollbackOccurred": false,
  "failedStep": null
}
```

**Error Response** (500 Internal Server Error - Transaction Rollback):
```json
{
  "transactionId": "guid",
  "authorizationId": "string",
  "sucesso": false,
  "mensagem": "Erro ao processar fases",
  "erros": [
    {
      "ruleId": "SYS-004",
      "ruleName": "Phase Update Failure",
      "errorCode": "SYS-004",
      "errorMessage": "Erro ao processar fases",
      "failedValue": "Phase 10",
      "expectedValue": "Phase opened successfully",
      "severity": "Critical"
    }
  ],
  "validationResults": [
    {
      "ezert8": "00000000",
      "errorMessage": null,
      "isSuccess": true,
      "validationService": "CNOUA",
      "requestTimestamp": "2025-10-27T10:30:00Z",
      "responseTimestamp": "2025-10-27T10:30:02Z",
      "elapsedMilliseconds": 2000
    }
  ],
  "rollbackOccurred": true,
  "failedStep": "Phases"
}
```

**Error Response** (503 Service Unavailable - External Service Down):
```json
{
  "transactionId": "guid",
  "authorizationId": "string",
  "sucesso": false,
  "mensagem": "Serviço de validação indisponível",
  "erros": [
    {
      "ruleId": "SYS-005",
      "ruleName": "External Service Unavailable",
      "errorCode": "SYS-005",
      "errorMessage": "Serviço de validação indisponível",
      "failedValue": "CNOUA",
      "expectedValue": "Service available",
      "severity": "Critical"
    }
  ],
  "validationResults": [],
  "rollbackOccurred": false,
  "failedStep": null
}
```

### New Internal Endpoints (Not Exposed Externally)

These endpoints exist only for integration testing and are not exposed via API Gateway:

#### POST /internal/external-validation/cnoua
**Purpose**: Call CNOUA REST API for consortium product validation (products 6814, 7701, 7709)

#### POST /internal/external-validation/sipua
**Purpose**: Call SIPUA SOAP service for EFP contract validation (NUM_CONTRATO > 0)

#### POST /internal/external-validation/simda
**Purpose**: Call SIMDA SOAP service for HB contract validation (NUM_CONTRATO = 0 or not found)

## External Service Contracts

### CNOUA REST API

**Endpoint**: `POST https://cnoua-service.caixaseguradora.com.br/api/v1/validate`

**Authentication**: Bearer token (JWT)

**Request Headers**:
```
Content-Type: application/json
Authorization: Bearer {token}
X-Request-ID: {guid}
```

**Request Body**:
```json
{
  "fonte": 1,
  "protocolNumber": 123456,
  "dac": 7,
  "productCode": 6814,
  "claimNumber": "01/05/123456",
  "contractNumber": null,
  "principalAmount": 1000.00,
  "beneficiary": "João Silva"
}
```

**Response Body** (Success):
```json
{
  "ezert8": "00000000",
  "message": "Validation successful",
  "timestamp": "2025-10-27T10:30:02Z"
}
```

**Response Body** (Validation Failure):
```json
{
  "ezert8": "EZERT8001",
  "message": "Contrato de consórcio inválido",
  "timestamp": "2025-10-27T10:30:02Z"
}
```

**Timeout**: 10 seconds

**Retry Policy**: 3 retries with exponential backoff (2s, 4s, 8s)

**Circuit Breaker**: Open after 5 consecutive failures, 30-second break

### SIPUA SOAP Service

**Endpoint**: `https://sipua-service.caixaseguradora.com.br/services/ContractValidation`

**WSDL**: `https://sipua-service.caixaseguradora.com.br/services/ContractValidation?wsdl`

**SOAP Action**: `http://caixaseguradora.com.br/sipua/ValidateContract`

**Request Envelope**:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:sip="http://caixaseguradora.com.br/sipua">
  <soapenv:Header/>
  <soapenv:Body>
    <sip:ValidateContractRequest>
      <sip:ContractNumber>12345</sip:ContractNumber>
      <sip:ClaimNumber>01/05/123456</sip:ClaimNumber>
      <sip:PolicyType>1</sip:PolicyType>
      <sip:PrincipalAmount>1000.00</sip:PrincipalAmount>
    </sip:ValidateContractRequest>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response Envelope** (Success):
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:sip="http://caixaseguradora.com.br/sipua">
  <soapenv:Body>
    <sip:ValidateContractResponse>
      <sip:Ezert8>00000000</sip:Ezert8>
      <sip:Message>Validation successful</sip:Message>
    </sip:ValidateContractResponse>
  </soapenv:Body>
</soapenv:Envelope>
```

**Response Envelope** (Validation Failure):
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:sip="http://caixaseguradora.com.br/sipua">
  <soapenv:Body>
    <sip:ValidateContractResponse>
      <sip:Ezert8>EZERT8002</sip:Ezert8>
      <sip:Message>Contrato cancelado</sip:Message>
    </sip:ValidateContractResponse>
  </soapenv:Body>
</soapenv:Envelope>
```

**Timeout**: 10 seconds

**Retry Policy**: Same as CNOUA (3 retries, exponential backoff)

**Circuit Breaker**: Same as CNOUA (5 failures, 30s break)

### SIMDA SOAP Service

**Endpoint**: `https://simda-service.caixaseguradora.com.br/services/HBValidation`

**WSDL**: `https://simda-service.caixaseguradora.com.br/services/HBValidation?wsdl`

**SOAP Action**: `http://caixaseguradora.com.br/simda/ValidateHBContract`

**Request/Response Format**: Same structure as SIPUA (different namespace and endpoint only)

## Error Code Reference

### Validation Errors (VAL-001 through VAL-008)

| Code | Message (Portuguese) | Trigger Condition |
|------|----------------------|-------------------|
| VAL-001 | Protocolo {fonte}/{protsini}-{dac} NAO ENCONTRADO | Protocol search returns no results |
| VAL-002 | SINISTRO {orgsin}/{rmosin}/{numsin} NAO ENCONTRADO | Claim number search returns no results |
| VAL-003 | LIDER {codlider}-{sinlid} NAO ENCONTRADO | Leader code search returns no results |
| VAL-004 | Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5 | TipoPagamento not in {1,2,3,4,5} |
| VAL-005 | Valor Superior ao Saldo Pendente | ValorPrincipal > (SDOPAG - TOTPAG) |
| VAL-006 | Tipo de Apólice deve ser 1 ou 2 | TipoApolice not in {1,2} |
| VAL-007 | Favorecido é obrigatório para este tipo de seguro | Beneficiario null when TPSEGU != 0 |
| VAL-008 | Taxa de conversão não disponível para a data | No VLCRUZAD rate found in TGEUNIMO for DTMOVABE |

### Consortium Validation Errors (CONS-001 through CONS-005)

| Code | Message (Portuguese) | Trigger Condition |
|------|----------------------|-------------------|
| CONS-001 | Contrato de consórcio inválido | EZERT8 = 'EZERT8001' from CNOUA/SIPUA/SIMDA |
| CONS-002 | Contrato cancelado | EZERT8 = 'EZERT8002' |
| CONS-003 | Grupo encerrado | EZERT8 = 'EZERT8003' |
| CONS-004 | Cota não contemplada | EZERT8 = 'EZERT8004' |
| CONS-005 | Beneficiário não autorizado | EZERT8 = 'EZERT8005' |

### System Errors (SYS-001 through SYS-005)

| Code | Message (Portuguese) | Trigger Condition |
|------|----------------------|-------------------|
| SYS-001 | Erro ao buscar dados do sinistro | Exception during claim retrieval |
| SYS-002 | Erro ao inserir histórico de pagamento | Exception during THISTSIN insert |
| SYS-003 | Erro ao atualizar saldo do sinistro | Exception during TMESTSIN update |
| SYS-004 | Erro ao processar fases | Exception during SI_SINISTRO_FASE update |
| SYS-005 | Serviço de validação indisponível | External service timeout/circuit breaker open |

## Contract Testing Strategy

### Unit Tests (Contract Validation)
- Validate request/response schema compliance using JSON Schema validators
- Test error code mapping (EZERT8 → Portuguese error messages)
- Verify Polly retry/circuit breaker behavior with mocked external services

### Integration Tests (WireMock Stubs)
- Stub CNOUA REST API with WireMock returning success/failure responses
- Stub SIPUA/SIMDA SOAP services with SoapUI test mocks
- Verify request payloads match contract specifications
- Verify timeout handling (10-second limit)
- Verify circuit breaker behavior (5 failures → 30s break)

### Contract Tests (Pact/Spring Cloud Contract)
- Consumer-driven contracts for CNOUA/SIPUA/SIMDA services
- Verify API changes don't break existing consumers
- Generate contracts from integration test scenarios

## Implementation Notes

### Request Validation Order
1. FluentValidation rules (required fields, ranges, formats)
2. Business rule validators (BR-001 through BR-099)
3. External service validation (CNOUA/SIPUA/SIMDA)
4. Transaction execution (4-step pipeline)

### Response Generation Order
1. Validation failures → 400 Bad Request with BusinessRuleViolation[]
2. External service failures → 503 Service Unavailable with SYS-005 error
3. Transaction failures → 500 Internal Server Error with rollback details
4. Success → 200 OK with TransactionContext details

### Polly Policy Configuration
```csharp
// Retry policy: 3 attempts with exponential backoff
var retryPolicy = Policy
  .Handle<HttpRequestException>()
  .Or<TaskCanceledException>()
  .WaitAndRetryAsync(3, retryAttempt =>
    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // 2s, 4s, 8s

// Circuit breaker: Open after 5 failures for 30 seconds
var circuitBreakerPolicy = Policy
  .Handle<HttpRequestException>()
  .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// Timeout policy: 10 seconds per request
var timeoutPolicy = Policy.TimeoutAsync(10);

// Combined policy
var combinedPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
```

---

**Contracts Status**: COMPLETE (specifications defined, implementation files to be created)
**Next Step**: Phase 1 - Generate quickstart.md
**Dependencies**: External service endpoints and WSDLs must be available for integration testing
