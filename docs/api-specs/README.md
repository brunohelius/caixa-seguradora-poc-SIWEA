# Especificações de API - SIWEA

**Versão:** 1.0.0
**Data:** 2025-10-27
**Projeto:** Migração SIWEA - Visual Age para .NET 9.0

---

## Índice

1. [Visão Geral](#visão-geral)
2. [Especificação OpenAPI (REST)](#especificação-openapi-rest)
3. [Especificações WSDL (SOAP)](#especificações-wsdl-soap)
4. [Como Usar](#como-usar)
5. [Autenticação](#autenticação)
6. [Versionamento](#versionamento)
7. [Rate Limiting](#rate-limiting)
8. [Códigos de Erro](#códigos-de-erro)
9. [Ambientes](#ambientes)

---

## Visão Geral

Esta pasta contém todas as especificações de API do sistema SIWEA migrado:

| Arquivo | Tipo | Descrição | Linhas |
|---------|------|-----------|--------|
| **openapi-siwea-v1.yaml** | OpenAPI 3.0.3 | API REST completa | ~1.800 |
| **wsdl-autenticacao-v1.wsdl** | WSDL 1.1 | Serviço SOAP de autenticação | ~400 |
| **wsdl-solicitacao-v1.wsdl** | WSDL 1.1 | Serviço SOAP de solicitações | ~350 |
| **wsdl-assunto-v1.wsdl** | WSDL 1.1 | Serviço SOAP de assuntos | ~300 |

**Total:** ~2.850 linhas de especificações

---

## Especificação OpenAPI (REST)

**Arquivo:** `openapi-siwea-v1.yaml`

### Endpoints Documentados (18)

#### Claims Search (3 endpoints)
- `POST /claims/search/protocol` - Buscar por protocolo
- `POST /claims/search/number` - Buscar por número
- `POST /claims/search/leader` - Buscar por código líder

#### Payment Authorization (1 endpoint)
- `POST /payments/authorize` ⭐ - **Autorizar pagamento (crítico)**

#### Payment History (1 endpoint)
- `GET /claims/{claimId}/payments` - Histórico de pagamentos

#### Phase Management (1 endpoint)
- `GET /claims/{claimId}/phases` - Consultar fases do sinistro

#### Dashboard (2 endpoints)
- `GET /dashboard/migration-status` - Status da migração
- `GET /dashboard/performance-metrics` - Métricas de performance

#### Health Checks (2 endpoints)
- `GET /health` - Health check básico
- `GET /health/detailed` - Health check detalhado

### Schemas Definidos (30+)

**Request Schemas:**
- SearchByProtocolRequest
- SearchByClaimNumberRequest
- SearchByLeaderCodeRequest
- PaymentAuthorizationRequest

**Response Schemas:**
- ClaimDetailsResponse
- PaymentAuthorizationResponse
- PaymentHistoryResponse
- ClaimPhasesResponse
- MigrationStatusResponse
- PerformanceMetricsResponse
- HealthCheckResponse
- DetailedHealthCheckResponse

**Error Schemas:**
- ErrorResponse (genérico)
- ValidationErrorResponse (campo inválido)
- BusinessRuleErrorResponse (regra de negócio violada)

### Visualizar OpenAPI

#### Opção 1: Swagger UI Online

1. Acesse: https://editor.swagger.io/
2. Arquivo → Importar arquivo
3. Selecione `openapi-siwea-v1.yaml`
4. Visualize documentação interativa

#### Opção 2: Swagger UI Local

```bash
# Via Docker
docker run -p 8080:8080 \
  -e SWAGGER_JSON=/api-specs/openapi-siwea-v1.yaml \
  -v $(pwd):/api-specs \
  swaggerapi/swagger-ui

# Acesse: http://localhost:8080
```

#### Opção 3: VS Code

**Extensão:** OpenAPI (Swagger) Editor

```bash
# Instalar extensão
code --install-extension 42Crunch.vscode-openapi

# Abrir arquivo
code openapi-siwea-v1.yaml

# Preview: Ctrl+Shift+P → "OpenAPI: Show Preview"
```

#### Opção 4: Redoc

```bash
# Via NPX
npx @redocly/cli preview-docs openapi-siwea-v1.yaml

# Acesse: http://localhost:8080
```

### Gerar Cliente

#### C# (.NET)

```bash
# NSwag
nswag openapi2csclient /input:openapi-siwea-v1.yaml /output:SiweaApiClient.cs /namespace:CaixaSeguradora.Api.Client

# OpenAPI Generator
openapi-generator-cli generate \
  -i openapi-siwea-v1.yaml \
  -g csharp-netcore \
  -o ./generated/csharp
```

#### TypeScript (React)

```bash
# OpenAPI Generator
openapi-generator-cli generate \
  -i openapi-siwea-v1.yaml \
  -g typescript-axios \
  -o ./generated/typescript

# Orval (recomendado para React)
npx orval --input openapi-siwea-v1.yaml --output ./src/api
```

#### Python

```bash
openapi-generator-cli generate \
  -i openapi-siwea-v1.yaml \
  -g python \
  -o ./generated/python
```

### Validar Especificação

```bash
# Swagger CLI
swagger-cli validate openapi-siwea-v1.yaml

# Redocly CLI
npx @redocly/cli lint openapi-siwea-v1.yaml

# Spectral (OpenAPI linter)
npx @stoplight/spectral-cli lint openapi-siwea-v1.yaml
```

---

## Especificações WSDL (SOAP)

### Serviços SOAP Documentados (3)

#### 1. Autenticação (WSDL)

**Arquivo:** `wsdl-autenticacao-v1.wsdl`

**Operações:**
- `autenticar` - Autenticar usuário e obter sessionId
- `renovarSessao` - Renovar sessão expirada
- `logout` - Encerrar sessão

**Endpoint:** `https://api.caixaseguradora.com.br/siwea/v1/soap/autenticacao`

**Namespace:** `http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1`

#### 2. Solicitação (WSDL)

**Arquivo:** `wsdl-solicitacao-v1.wsdl`

**Operações:**
- `criarSolicitacao` - Criar nova solicitação
- `consultarSolicitacao` - Consultar solicitação por ID
- `atualizarSolicitacao` - Atualizar dados de solicitação

**Endpoint:** `https://api.caixaseguradora.com.br/siwea/v1/soap/solicitacao`

**Namespace:** `http://ls.caixaseguradora.com.br/LS1134WSV0007_Solicitacao/v1`

#### 3. Assunto (WSDL)

**Arquivo:** `wsdl-assunto-v1.wsdl`

**Operações:**
- `listarAssuntos` - Listar todos os assuntos
- `buscarAssuntoPorCodigo` - Buscar assunto específico

**Endpoint:** `https://api.caixaseguradora.com.br/siwea/v1/soap/assunto`

**Namespace:** `http://ls.caixaseguradora.com.br/LS1134WSV0006_Assunto/v1`

### Visualizar WSDL

#### Navegador

Abra o arquivo `.wsdl` diretamente no navegador:
```
file:///path/to/wsdl-autenticacao-v1.wsdl
```

#### SoapUI

1. Abra SoapUI
2. File → Import Project
3. Selecione arquivo WSDL
4. Gere requests de teste automaticamente

#### Postman

1. Abra Postman
2. Import → File → Selecione WSDL
3. Postman gera collection automaticamente

### Gerar Cliente SOAP

#### C# (.NET)

```bash
# dotnet-svcutil
dotnet-svcutil wsdl-autenticacao-v1.wsdl \
  --namespace "*,CaixaSeguradora.Soap.Autenticacao" \
  --outputDir ./generated/soap

# WCF Connected Service (Visual Studio)
# Add → Connected Service → WCF Web Service → Select WSDL file
```

#### Java

```bash
# Apache CXF
wsdl2java -d ./generated/java -p com.caixaseguradora.soap wsdl-autenticacao-v1.wsdl

# JAX-WS
wsimport -keep -d ./generated/java wsdl-autenticacao-v1.wsdl
```

#### Python

```bash
# Zeep
pip install zeep
python -c "from zeep import Client; client = Client('wsdl-autenticacao-v1.wsdl'); print(client.service)"
```

### Testar SOAP com curl

```bash
# Autenticação
curl -X POST https://api.caixaseguradora.com.br/siwea/v1/soap/autenticacao \
  -H "Content-Type: text/xml; charset=utf-8" \
  -H "SOAPAction: autenticar" \
  -d @- <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:aut="http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1">
  <soapenv:Header/>
  <soapenv:Body>
    <aut:autenticarRequest>
      <aut:codUsuario>teste.usuario</aut:codUsuario>
      <aut:desSenha>senha123</aut:desSenha>
      <aut:codSistema>S1</aut:codSistema>
    </aut:autenticarRequest>
  </soapenv:Body>
</soapenv:Envelope>
EOF
```

---

## Como Usar

### Fluxo Típico de Uso

#### 1. Autenticação (REST)

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "operador123",
  "password": "senha123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 28800,
  "refreshToken": "..."
}
```

#### 2. Buscar Sinistro

```http
POST /api/v1/claims/search/protocol
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "source": 1,
  "protocolNumber": 123456,
  "dac": 7
}

Response:
{
  "claimId": "1-10-20-789012",
  "protocol": "001/0123456-7",
  "pendingValue": 40000.00,
  ...
}
```

#### 3. Autorizar Pagamento

```http
POST /api/v1/payments/authorize
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "claimId": "1-10-20-789012",
  "paymentType": 1,
  "policyType": 1,
  "principalValue": 25000.00,
  "correctionValue": 500.00,
  "beneficiaryName": "João da Silva"
}

Response:
{
  "success": true,
  "transactionId": "TXN-20241027-001",
  "authorizedAmount": 31481.48,
  "message": "PAGAMENTO AUTORIZADO COM SUCESSO"
}
```

#### 4. Consultar Histórico

```http
GET /api/v1/claims/1-10-20-789012/payments?page=1&pageSize=20
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Response:
{
  "claimId": "1-10-20-789012",
  "totalRecords": 6,
  "payments": [...]
}
```

---

## Autenticação

### JWT Bearer Token

Todas as rotas (exceto `/health` e `/swagger`) requerem autenticação via JWT.

**Header:**
```
Authorization: Bearer <token>
```

**Obter Token:**
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "operador123",
  "password": "senha123"
}
```

**Token Expiration:**
- Tempo de vida: 8 horas
- Refresh: Use `/auth/refresh` com refreshToken

**Payload do Token:**
```json
{
  "sub": "operador123",
  "userId": "OP12345",
  "roles": ["operator", "authorizer"],
  "exp": 1698422122,
  "iat": 1698393322
}
```

---

## Versionamento

### Estratégia de Versionamento

**Path-Based Versioning:**
```
https://api.caixaseguradora.com.br/siwea/v1/...
https://api.caixaseguradora.com.br/siwea/v2/... (futuro)
```

**Header de Versão (Opcional):**
```
X-API-Version: 1
```

### Política de Depreciação

- **v1**: Suportada indefinidamente (compatibilidade com legado)
- **v2**: Futuro - breaking changes permitidas
- **Aviso de depreciação**: 6 meses antes de remover versão

---

## Rate Limiting

### Limites

| Cliente | Limite | Janela |
|---------|--------|--------|
| **IP único** | 100 requests | 1 minuto |
| **Usuário autenticado** | 500 requests | 1 minuto |
| **Burst** | 10 requests | 1 segundo |

### Headers de Resposta

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 85
X-RateLimit-Reset: 1698422122
```

### Erro de Rate Limit

```http
HTTP/1.1 429 Too Many Requests
Retry-After: 60
Content-Type: application/json

{
  "error": {
    "code": "RATE_LIMIT_EXCEEDED",
    "message": "Limite de requisições excedido. Tente novamente em 60 segundos.",
    "timestamp": "2024-10-27T14:35:22Z",
    "traceId": "abc123"
  }
}
```

---

## Códigos de Erro

### Códigos HTTP

| Código | Significado | Quando Ocorre |
|--------|-------------|---------------|
| **200** | OK | Requisição bem-sucedida |
| **400** | Bad Request | Validação de entrada falhou |
| **401** | Unauthorized | Token ausente/inválido |
| **403** | Forbidden | Sem permissão para recurso |
| **404** | Not Found | Recurso não encontrado |
| **409** | Conflict | Conflito de concorrência |
| **422** | Unprocessable Entity | Regra de negócio violada |
| **429** | Too Many Requests | Rate limit excedido |
| **500** | Internal Server Error | Erro de sistema |
| **503** | Service Unavailable | Serviço externo indisponível |
| **504** | Gateway Timeout | Timeout em serviço externo |

### Códigos de Erro Customizados

| Código | Descrição |
|--------|-----------|
| `CLAIM_NOT_FOUND` | Sinistro não encontrado |
| `VALIDATION_ERROR` | Erro de validação de entrada |
| `BUSINESS_RULE_VIOLATION` | Regra de negócio violada |
| `CURRENCY_RATE_UNAVAILABLE` | Taxa BTNF não disponível |
| `PRODUCT_VALIDATION_FAILED` | Validação externa falhou |
| `TRANSACTION_FAILED` | Erro na transação de banco |
| `CONCURRENCY_CONFLICT` | Conflito de concorrência |
| `SERVICE_UNAVAILABLE` | Circuit breaker aberto |
| `GATEWAY_TIMEOUT` | Timeout em validação |

### Estrutura de Erro Padrão

```json
{
  "error": {
    "code": "BUSINESS_RULE_VIOLATION",
    "message": "VALOR EXCEDE SALDO PENDENTE",
    "timestamp": "2024-10-27T14:35:22Z",
    "traceId": "abc123",
    "businessRule": {
      "ruleId": "BR-013",
      "ruleName": "Validação de Saldo Disponível",
      "details": {
        "requestedAmount": 50000.00,
        "pendingBalance": 40000.00
      }
    }
  }
}
```

---

## Ambientes

### Produção

```
Base URL: https://api.caixaseguradora.com.br/siwea/v1
SOAP Endpoint: https://api.caixaseguradora.com.br/siwea/v1/soap
Swagger UI: https://api.caixaseguradora.com.br/siwea/v1/swagger
```

### Homologação

```
Base URL: https://api-hom.caixaseguradora.com.br/siwea/v1
SOAP Endpoint: https://api-hom.caixaseguradora.com.br/siwea/v1/soap
Swagger UI: https://api-hom.caixaseguradora.com.br/siwea/v1/swagger
```

### Desenvolvimento Local

```
Base URL: https://localhost:5001/api/v1
SOAP Endpoint: https://localhost:5001/soap
Swagger UI: https://localhost:5001/swagger
```

---

## Exemplo de Integração Completa

### C# (.NET)

```csharp
using System.Net.Http.Json;
using System.Net.Http.Headers;

// 1. Autenticação
var client = new HttpClient();
var loginResponse = await client.PostAsJsonAsync(
    "https://api.caixaseguradora.com.br/siwea/v1/auth/login",
    new { username = "operador123", password = "senha123" }
);
var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

// 2. Configurar token
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", auth.Token);

// 3. Buscar sinistro
var searchResponse = await client.PostAsJsonAsync(
    "https://api.caixaseguradora.com.br/siwea/v1/claims/search/protocol",
    new { source = 1, protocolNumber = 123456, dac = 7 }
);
var claim = await searchResponse.Content.ReadFromJsonAsync<ClaimDetailsResponse>();

// 4. Autorizar pagamento
var authzResponse = await client.PostAsJsonAsync(
    "https://api.caixaseguradora.com.br/siwea/v1/payments/authorize",
    new {
        claimId = claim.ClaimId,
        paymentType = 1,
        policyType = 1,
        principalValue = 25000.00,
        beneficiaryName = "João da Silva"
    }
);
var payment = await authzResponse.Content.ReadFromJsonAsync<PaymentAuthorizationResponse>();

Console.WriteLine($"Pagamento autorizado: {payment.TransactionId}");
```

### TypeScript (React)

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://api.caixaseguradora.com.br/siwea/v1'
});

// 1. Autenticação
const { data: auth } = await api.post('/auth/login', {
  username: 'operador123',
  password: 'senha123'
});

// 2. Configurar token
api.defaults.headers.common['Authorization'] = `Bearer ${auth.token}`;

// 3. Buscar sinistro
const { data: claim } = await api.post('/claims/search/protocol', {
  source: 1,
  protocolNumber: 123456,
  dac: 7
});

// 4. Autorizar pagamento
const { data: payment } = await api.post('/payments/authorize', {
  claimId: claim.claimId,
  paymentType: 1,
  policyType: 1,
  principalValue: 25000.00,
  beneficiaryName: 'João da Silva'
});

console.log('Pagamento autorizado:', payment.transactionId);
```

---

## Suporte e Contato

**Equipe de Desenvolvimento:**
- Email: siwea-dev@caixaseguradora.com.br
- Slack: #siwea-api-support

**Documentação Adicional:**
- Guia de Migração: `docs/migration-guide.md`
- Regras de Negócio: `docs/SISTEMA_LEGADO_REGRAS_NEGOCIO.md`
- Arquitetura: `docs/SISTEMA_LEGADO_ARQUITETURA.md`

**Reportar Problemas:**
- Issues: https://github.com/caixaseguradora/siwea/issues
- Email: siwea-bugs@caixaseguradora.com.br

---

**FIM DO README - API SPECS**
