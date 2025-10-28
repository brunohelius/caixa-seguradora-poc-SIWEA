# 06 - Integrações Externas: Sistema Legado SIWEA

[← Voltar ao Índice](README.md) | [→ Próximo: Guia de Operações](07-operations-guide.md)

---

## Visão Geral das Integrações

### Sistemas Integrados

| Sistema | Tipo | Protocolo | Criticidade | Volume/Dia |
|---------|------|-----------|-------------|------------|
| **CNOUA** | Validação Consórcio | SOAP/HTTP | Alta | 500 |
| **SIPUA** | Validação EFP | REST/JSON | Alta | 300 |
| **SIMDA** | Validação HB | MQ Series | Média | 200 |
| **RACF** | Autenticação | Native | Crítica | 8.000 |
| **DB2** | Banco de Dados | SQL/JDBC | Crítica | 50.000 |
| **CICS** | Transaction Manager | Native | Crítica | 10.000 |

---

## CNOUA - Sistema de Validação de Consórcio

### Especificação Técnica

| Atributo | Valor |
|----------|-------|
| **Endpoint Produção** | http://cnoua.caixaseguradora.com.br:8080/validacao |
| **Endpoint Homologação** | http://cnoua-hom.caixaseguradora.com.br:8080/validacao |
| **Protocolo** | SOAP 1.1 |
| **Autenticação** | WS-Security UsernameToken |
| **Timeout** | 30 segundos |
| **Retry Policy** | 3 tentativas, backoff exponencial |

### WSDL Simplificado

```xml
<?xml version="1.0" encoding="UTF-8"?>
<definitions xmlns="http://schemas.xmlsoap.org/wsdl/"
             targetNamespace="http://cnoua.caixaseguradora.com.br/validacao">
    
    <types>
        <schema targetNamespace="http://cnoua.caixaseguradora.com.br/validacao">
            
            <element name="ValidarConsorcioRequest">
                <complexType>
                    <sequence>
                        <element name="codigoProduto" type="int"/>
                        <element name="numeroContrato" type="string"/>
                        <element name="numeroSinistro" type="string"/>
                        <element name="valorSolicitado" type="decimal"/>
                        <element name="dataMovimento" type="date"/>
                    </sequence>
                </complexType>
            </element>
            
            <element name="ValidarConsorcioResponse">
                <complexType>
                    <sequence>
                        <element name="status" type="string"/>
                        <element name="codigo" type="string"/>
                        <element name="mensagem" type="string"/>
                        <element name="limiteDisponivel" type="decimal"/>
                        <element name="dataExpiracao" type="date"/>
                    </sequence>
                </complexType>
            </element>
            
        </schema>
    </types>
    
    <message name="ValidarConsorcioInput">
        <part name="parameters" element="ValidarConsorcioRequest"/>
    </message>
    
    <message name="ValidarConsorcioOutput">
        <part name="parameters" element="ValidarConsorcioResponse"/>
    </message>
    
    <portType name="ValidacaoConsorcioPortType">
        <operation name="validarConsorcio">
            <input message="ValidarConsorcioInput"/>
            <output message="ValidarConsorcioOutput"/>
        </operation>
    </portType>
    
    <binding name="ValidacaoConsorcioBinding" type="ValidacaoConsorcioPortType">
        <soap:binding transport="http://schemas.xmlsoap.org/soap/http"/>
        <operation name="validarConsorcio">
            <soap:operation soapAction="validarConsorcio"/>
            <input><soap:body use="literal"/></input>
            <output><soap:body use="literal"/></output>
        </operation>
    </binding>
    
    <service name="ValidacaoConsorcioService">
        <port name="ValidacaoConsorcioPort" binding="ValidacaoConsorcioBinding">
            <soap:address location="http://cnoua.caixaseguradora.com.br:8080/validacao"/>
        </port>
    </service>
    
</definitions>
```

### Exemplo de Request/Response

**Request:**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    <soap:Header>
        <wsse:Security xmlns:wsse="http://schemas.xmlsoap.org/ws/2002/07/secext">
            <wsse:UsernameToken>
                <wsse:Username>SIWEA_USER</wsse:Username>
                <wsse:Password>encrypted_password</wsse:Password>
            </wsse:UsernameToken>
        </wsse:Security>
    </soap:Header>
    <soap:Body>
        <ValidarConsorcioRequest>
            <codigoProduto>6814</codigoProduto>
            <numeroContrato>CON123456789</numeroContrato>
            <numeroSinistro>10/20/789012</numeroSinistro>
            <valorSolicitado>50000.00</valorSolicitado>
            <dataMovimento>2025-10-27</dataMovimento>
        </ValidarConsorcioRequest>
    </soap:Body>
</soap:Envelope>
```

**Response (Sucesso):**
```xml
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    <soap:Body>
        <ValidarConsorcioResponse>
            <status>APROVADO</status>
            <codigo>VAL-001</codigo>
            <mensagem>Validação aprovada com sucesso</mensagem>
            <limiteDisponivel>150000.00</limiteDisponivel>
            <dataExpiracao>2025-12-31</dataExpiracao>
        </ValidarConsorcioResponse>
    </soap:Body>
</soap:Envelope>
```

### Códigos de Retorno

| Código | Status | Descrição | Ação |
|--------|--------|-----------|------|
| VAL-001 | APROVADO | Validação aprovada | Continuar |
| VAL-002 | REJEITADO | Limite excedido | Abortar |
| VAL-003 | REJEITADO | Contrato inválido | Abortar |
| VAL-004 | PENDENTE | Análise manual necessária | Aguardar |
| ERR-001 | ERRO | Erro de sistema | Retry |
| ERR-002 | ERRO | Timeout | Retry |

---

## SIPUA - Sistema de Validação EFP

### Especificação Técnica

| Atributo | Valor |
|----------|-------|
| **Endpoint Produção** | https://sipua.caixaseguradora.com.br/api/v1/validate |
| **Endpoint Homologação** | https://sipua-hom.caixaseguradora.com.br/api/v1/validate |
| **Protocolo** | REST/JSON |
| **Autenticação** | Bearer Token (JWT) |
| **Timeout** | 20 segundos |
| **Rate Limit** | 100 requests/minute |

### OpenAPI Specification

```yaml
openapi: 3.0.0
info:
  title: SIPUA Validation API
  version: 1.0.0
  
servers:
  - url: https://sipua.caixaseguradora.com.br/api/v1
    description: Production
  - url: https://sipua-hom.caixaseguradora.com.br/api/v1
    description: Homologation

paths:
  /validate:
    post:
      summary: Validate EFP Contract
      security:
        - bearerAuth: []
      requestBody:
        required: true
        content:
          application/json:
            schema:
              type: object
              required:
                - contractType
                - contractNumber
                - amount
                - claimNumber
              properties:
                contractType:
                  type: string
                  enum: [EFP]
                contractNumber:
                  type: string
                  pattern: '^EFP[0-9]{9}$'
                amount:
                  type: number
                  format: decimal
                  minimum: 0.01
                claimNumber:
                  type: string
                requestDate:
                  type: string
                  format: date
      responses:
        '200':
          description: Validation successful
          content:
            application/json:
              schema:
                type: object
                properties:
                  validated:
                    type: boolean
                  status:
                    type: string
                    enum: [APPROVED, REJECTED, PENDING]
                  availableLimit:
                    type: number
                    format: decimal
                  expirationDate:
                    type: string
                    format: date
                  message:
                    type: string
        '400':
          description: Invalid request
        '401':
          description: Unauthorized
        '429':
          description: Rate limit exceeded
        '500':
          description: Internal server error

components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
```

### Exemplo de Integração

**Request:**
```bash
curl -X POST https://sipua.caixaseguradora.com.br/api/v1/validate \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "contractType": "EFP",
    "contractNumber": "EFP123456789",
    "amount": 25000.00,
    "claimNumber": "10/20/789012",
    "requestDate": "2025-10-27"
  }'
```

**Response:**
```json
{
  "validated": true,
  "status": "APPROVED",
  "availableLimit": 75000.00,
  "expirationDate": "2025-12-31",
  "message": "Contract validated successfully"
}
```

### Fluxo de Autenticação

```mermaid
sequenceDiagram
    participant SIWEA
    participant Auth as SIPUA Auth
    participant API as SIPUA API
    
    SIWEA->>Auth: POST /auth/token (credentials)
    Auth-->>SIWEA: JWT Token (expires in 1h)
    SIWEA->>API: POST /validate (with Bearer token)
    API-->>SIWEA: Validation result
```

---

## SIMDA - Sistema de Validação HB

### Especificação Técnica

| Atributo | Valor |
|----------|-------|
| **Queue Manager** | QM.SIMDA.PRD |
| **Request Queue** | SIMDA.VALIDATION.REQ |
| **Response Queue** | SIMDA.VALIDATION.RSP |
| **Reply Queue** | SIMDA.VALIDATION.REPLY |
| **Protocol** | IBM MQ Series |
| **Format** | Fixed-width text |
| **Encoding** | EBCDIC |
| **Timeout** | 25 segundos |

### Formato de Mensagem

#### Request Message (200 bytes)

```
Position  Length  Field           Type      Description
001-010   10      Contract#       CHAR      Contract number
011-025   15      Amount          DEC(15,2) Amount (packed)
026-035   10      ClaimNumber     CHAR      Claim reference
036-043   8       RequestDate     DATE      YYYYMMDD
044-051   8       RequestTime     TIME      HHMMSSTH
052-059   8       CorrelationID   CHAR      Unique ID
060-200   141     Filler          CHAR      Spaces
```

#### Response Message (100 bytes)

```
Position  Length  Field           Type      Description
001-001   1       Status          CHAR      A=Approved, R=Rejected
002-016   15      AvailableAmt    DEC(15,2) Available amount
017-024   8       ExpiryDate      DATE      YYYYMMDD
025-032   8       CorrelationID   CHAR      Same as request
033-082   50      Message         CHAR      Status message
083-100   18      Filler          CHAR      Spaces
```

### Código de Integração COBOL

```cobol
WORKING-STORAGE SECTION.
01  MQ-CONSTANTS.
    05  MQ-CONN-HANDLE      PIC S9(9) BINARY.
    05  MQ-OBJECT-HANDLE    PIC S9(9) BINARY.
    05  MQ-COMP-CODE        PIC S9(9) BINARY.
    05  MQ-REASON           PIC S9(9) BINARY.

01  SIMDA-REQUEST.
    05  SIMDA-CONTRACT      PIC X(10).
    05  SIMDA-AMOUNT        PIC S9(13)V99 COMP-3.
    05  SIMDA-CLAIM         PIC X(10).
    05  SIMDA-REQ-DATE      PIC 9(08).
    05  SIMDA-REQ-TIME      PIC 9(08).
    05  SIMDA-CORREL-ID     PIC X(08).
    05  FILLER              PIC X(141).

01  SIMDA-RESPONSE.
    05  SIMDA-STATUS        PIC X(01).
        88  SIMDA-APPROVED  VALUE 'A'.
        88  SIMDA-REJECTED  VALUE 'R'.
    05  SIMDA-AVAIL-AMT     PIC S9(13)V99 COMP-3.
    05  SIMDA-EXPIRY        PIC 9(08).
    05  SIMDA-RESP-CORR     PIC X(08).
    05  SIMDA-MESSAGE       PIC X(50).
    05  FILLER              PIC X(18).

PROCEDURE DIVISION.
VALIDATE-WITH-SIMDA SECTION.
    PERFORM CONNECT-TO-MQ
    PERFORM BUILD-REQUEST
    PERFORM SEND-TO-MQ
    PERFORM RECEIVE-FROM-MQ
    PERFORM PROCESS-RESPONSE
    PERFORM DISCONNECT-FROM-MQ.

CONNECT-TO-MQ.
    CALL 'MQCONN' USING QM-NAME
                        MQ-CONN-HANDLE
                        MQ-COMP-CODE
                        MQ-REASON.

SEND-TO-MQ.
    CALL 'MQPUT' USING MQ-CONN-HANDLE
                       MQ-OBJECT-HANDLE
                       MQ-MSG-DESC
                       MQ-PUT-OPTIONS
                       SIMDA-REQUEST-LENGTH
                       SIMDA-REQUEST
                       MQ-COMP-CODE
                       MQ-REASON.
```

---

## Estratégias de Resiliência

### Circuit Breaker Pattern

```
Estados:
    CLOSED: Sistema funcionando normalmente
    OPEN: Sistema com falha, requisições rejeitadas
    HALF_OPEN: Testando recuperação

Transições:
    CLOSED → OPEN: 5 falhas consecutivas
    OPEN → HALF_OPEN: Após 30 segundos
    HALF_OPEN → CLOSED: 1 sucesso
    HALF_OPEN → OPEN: 1 falha
```

### Retry Policy

```yaml
retryPolicy:
  maxAttempts: 3
  backoffStrategy: exponential
  initialDelay: 1000ms
  maxDelay: 8000ms
  multiplier: 2
  retryableErrors:
    - TIMEOUT
    - CONNECTION_REFUSED
    - SERVICE_UNAVAILABLE
  nonRetryableErrors:
    - INVALID_CONTRACT
    - INSUFFICIENT_FUNDS
    - AUTHENTICATION_FAILED
```

### Modo Contingência

```
ATIVAÇÃO:
    - Todos os sistemas externos indisponíveis
    - Autorização manual do supervisor
    - Horário de contingência (22h-6h)

COMPORTAMENTO:
    - Aprovar até R$ 10.000 automaticamente
    - Valores maiores requerem dupla aprovação
    - Flag CONTINGENCY = 'Y' no registro
    - Revalidação posterior obrigatória

DESATIVAÇÃO:
    - Sistemas voltam online
    - Processamento de backlog
    - Reconciliação de transações
```

---

## Monitoramento e Alertas

### Métricas Monitoradas

| Métrica | Threshold | Alerta | Ação |
|---------|-----------|--------|------|
| Response Time CNOUA | > 10s | Warning | Check network |
| Response Time SIPUA | > 8s | Warning | Check load |
| Response Time SIMDA | > 12s | Warning | Check queue |
| Error Rate | > 5% | Critical | Activate contingency |
| Timeout Rate | > 10% | Critical | Increase timeout |
| Queue Depth SIMDA | > 1000 | Warning | Scale consumers |

### Dashboard de Integração

```
┌─────────────────────────────────────────────┐
│         INTEGRATION DASHBOARD                │
├─────────────────────────────────────────────┤
│                                             │
│ CNOUA    [████████░░] 85% Available        │
│          Avg: 2.3s | P99: 8.5s             │
│                                             │
│ SIPUA    [██████████] 100% Available       │
│          Avg: 1.8s | P99: 5.2s             │
│                                             │
│ SIMDA    [███████░░░] 70% Available        │
│          Avg: 3.1s | P99: 12.3s            │
│          Queue: 234 messages               │
│                                             │
│ Circuit Breaker States:                     │
│   CNOUA: CLOSED                            │
│   SIPUA: CLOSED                            │
│   SIMDA: HALF_OPEN                         │
│                                             │
│ Last 24h: 1,245 validations               │
│   Success: 1,198 (96.2%)                   │
│   Failed: 47 (3.8%)                        │
└─────────────────────────────────────────────┘
```

---

## Configuração de Ambientes

### Desenvolvimento

```properties
# DEV Environment
cnoua.endpoint=http://cnoua-dev.internal:8080/validacao
cnoua.timeout=60000
cnoua.mock=true

sipua.endpoint=http://localhost:8081/mock/sipua
sipua.timeout=30000
sipua.mock=true

simda.queueManager=QM.SIMDA.DEV
simda.mock=true
```

### Homologação

```properties
# HOM Environment
cnoua.endpoint=http://cnoua-hom.caixaseguradora.com.br:8080/validacao
cnoua.timeout=30000
cnoua.mock=false

sipua.endpoint=https://sipua-hom.caixaseguradora.com.br/api/v1
sipua.timeout=20000
sipua.mock=false

simda.queueManager=QM.SIMDA.HOM
simda.mock=false
```

### Produção

```properties
# PRD Environment
cnoua.endpoint=http://cnoua.caixaseguradora.com.br:8080/validacao
cnoua.timeout=30000
cnoua.mock=false
cnoua.circuitBreaker=true

sipua.endpoint=https://sipua.caixaseguradora.com.br/api/v1
sipua.timeout=20000
sipua.mock=false
sipua.circuitBreaker=true

simda.queueManager=QM.SIMDA.PRD
simda.mock=false
simda.circuitBreaker=true
```

---

## Testes de Integração

### Cenários de Teste

1. **Happy Path**
   - Validação aprovada
   - Response time < SLA
   - Dados corretos retornados

2. **Validação Rejeitada**
   - Limite excedido
   - Contrato inválido
   - Produto não elegível

3. **Timeout**
   - Sistema não responde
   - Retry funciona
   - Circuit breaker ativa

4. **Erro de Sistema**
   - Sistema retorna erro 500
   - Mensagem de erro tratada
   - Fallback ativado

5. **Modo Contingência**
   - Todos sistemas offline
   - Aprovação manual
   - Flag contingência setada

### Scripts de Teste

```bash
#!/bin/bash
# Test CNOUA Integration

# Test successful validation
curl -X POST http://cnoua-test:8080/validacao \
  -H "Content-Type: text/xml" \
  -d @test-data/cnoua-success.xml

# Test timeout scenario
timeout 5 curl -X POST http://cnoua-test:8080/validacao \
  -H "Content-Type: text/xml" \
  -d @test-data/cnoua-timeout.xml

# Test circuit breaker
for i in {1..10}; do
  curl -X POST http://cnoua-test:8080/validacao \
    -H "Content-Type: text/xml" \
    -d @test-data/cnoua-error.xml
done
```

---

*Este documento detalha todas as integrações externas do sistema SIWEA.*

**Última Atualização:** 27/10/2025
