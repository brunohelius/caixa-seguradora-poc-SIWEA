# API Contracts - Claims System Migration

**Project**: Visual Age to .NET 9 Migration
**Feature**: 001-visualage-dotnet-migration
**Created**: 2025-10-23
**Status**: Production-Ready

---

## Overview

This directory contains comprehensive API contracts for the Claims System (SIWEA - Sistema de Autorização de Pagamento de Indenizações de Sinistros) migration from IBM Visual Age EZEE to .NET 9 with React frontend.

All contracts follow industry standards and are ready for:
- Code generation (C# controllers, TypeScript clients)
- API documentation (Swagger UI)
- Contract testing
- Integration with SoapCore for SOAP endpoints

---

## Directory Structure

```
contracts/
├── README.md                              # This file
├── rest-api.yaml                          # OpenAPI 3.0 REST API specification
├── soap-autenticacao.wsdl                 # SOAP authentication service WSDL
├── soap-solicitacao.wsdl                  # SOAP request/solicitation service WSDL
├── soap-assunto.wsdl                      # SOAP subject/topic management WSDL
└── schemas/                               # JSON Schemas for validation
    ├── ClaimSearchRequest.json
    ├── ClaimSearchResponse.json
    ├── PaymentAuthorizationRequest.json
    ├── PaymentAuthorizationResponse.json
    ├── DashboardOverviewResponse.json
    └── ErrorResponse.json
```

---

## REST API Specification

### File: `rest-api.yaml`

**Format**: OpenAPI 3.0.3
**Base URL**: `https://api.caixaseguradora.com.br/v1`
**Authentication**: JWT Bearer Token
**Language**: Portuguese (Brazil)

#### Endpoints

##### Claims Operations
- `POST /api/claims/search` - Search claims by protocol, claim number, or leader code
- `GET /api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}` - Get claim by ID
- `POST /api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/authorize-payment` - Authorize payment
- `GET /api/claims/{tipseg}/{orgsin}/{rmosin}/{numsin}/history` - Get payment history (paginated)
- `GET /api/claims/{fonte}/{protsini}/{dac}/phases` - Get claim workflow phases

##### Dashboard Operations
- `GET /api/dashboard/overview` - Migration dashboard overview
- `GET /api/dashboard/components` - Detailed component migration status
- `GET /api/dashboard/performance` - Performance comparison metrics (legacy vs new)
- `GET /api/dashboard/activities` - Recent activities timeline

#### Features
- Comprehensive request/response schemas with Portuguese field names
- Detailed error responses with error codes and localized messages
- Validation rules (required, min/max, patterns, enums)
- Security schemes (JWT Bearer)
- Multiple example requests and responses
- Server URLs for prod, homologation, and development

#### Usage

**View in Swagger Editor**:
```bash
# Open in browser
open https://editor.swagger.io/

# Or use local Swagger UI
docker run -p 8080:8080 -e SWAGGER_JSON=/contracts/rest-api.yaml -v $(pwd):/contracts swaggerapi/swagger-ui
```

**Generate C# Client**:
```bash
# Using OpenAPI Generator
openapi-generator-cli generate \
  -i rest-api.yaml \
  -g csharp-netcore \
  -o ../backend/src/CaixaSeguradora.Api/Generated

# Using NSwag
nswag openapi2csclient /input:rest-api.yaml /output:../backend/src/CaixaSeguradora.Api/Generated/ClaimsApiClient.cs
```

**Generate TypeScript Client**:
```bash
# Using OpenAPI Generator
openapi-generator-cli generate \
  -i rest-api.yaml \
  -g typescript-axios \
  -o ../frontend/src/services/generated

# Using openapi-typescript
npx openapi-typescript rest-api.yaml --output ../frontend/src/services/generated/api.d.ts
```

---

## SOAP WSDL Specifications

### 1. Authentication Service - `soap-autenticacao.wsdl`

**Namespace**: `http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1`
**Endpoint**: `https://api.caixaseguradora.com.br/soap/autenticacao`
**Protocol**: SOAP 1.1 and SOAP 1.2

#### Operations
- `autenticar` - Authenticate user and return session token (JWT)
- `validarSessao` - Validate active session
- `encerrarSessao` - End active session (logout)

#### Usage with SoapCore (.NET)
```csharp
// Program.cs
builder.Services.AddSoapCore();
builder.Services.AddScoped<IAutenticacaoService, AutenticacaoService>();

app.UseSoapEndpoint<IAutenticacaoService>(
    "/soap/autenticacao",
    new SoapEncoderOptions(),
    SoapSerializer.DataContractSerializer
);
```

#### Test with SoapUI
1. Import `soap-autenticacao.wsdl` into SoapUI
2. Create new test request
3. Set endpoint: `https://localhost:5001/soap/autenticacao`
4. Send sample request:
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:v1="http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1">
   <soapenv:Header/>
   <soapenv:Body>
      <v1:autenticarRequest>
         <v1:codUsuario>teste.usuario</v1:codUsuario>
         <v1:desSenha>senha123</v1:desSenha>
         <v1:codSistema>SI</v1:codSistema>
      </v1:autenticarRequest>
   </soapenv:Body>
</soapenv:Envelope>
```

---

### 2. Request/Solicitation Service - `soap-solicitacao.wsdl`

**Namespace**: `http://ls.caixaseguradora.com.br/LS1134WSV0007_Solicitacao/v1`
**Endpoint**: `https://api.caixaseguradora.com.br/soap/solicitacao`

#### Operations
- `autorizarPagamento` - Authorize claim payment
- `consultarSolicitacao` - Query request status
- `cancelarSolicitacao` - Cancel pending request

#### Business Rules Implemented
- Payment type validation (1-5)
- Beneficiary requirement based on insurance type (tpsegu)
- Consortium product validation via CNOUA
- EFP contract validation via SIPUA
- HB contract validation via SIMDA
- Currency conversion to BTNF using TGEUNIMO rates
- Transaction rollback on validation failure
- Workflow phase management

---

### 3. Subject/Topic Service - `soap-assunto.wsdl`

**Namespace**: `http://ls.caixaseguradora.com.br/LS1134WSV0006_Assunto/v1`
**Endpoint**: `https://api.caixaseguradora.com.br/soap/assunto`

#### Operations
- `listarAssuntos` - List available subjects/topics
- `consultarAssunto` - Query subject details
- `associarAssuntoSinistro` - Associate subject with claim

---

## JSON Schemas

### Validation and Code Generation

All JSON schemas follow JSON Schema Draft 07 specification and include:
- Type definitions
- Validation rules (required, min/max, patterns)
- Descriptions in Portuguese
- Multiple examples
- Format specifications (date, time, date-time, email, uuid)

### Schema Files

#### 1. ClaimSearchRequest.json
Search criteria with three mutually exclusive options:
- Protocol number (fonte + protsini + dac)
- Claim number (orgsin + rmosin + numsin)
- Leader code (codlider + sinlid)

**Validation**: Uses `oneOf` to enforce exactly one complete criterion

#### 2. ClaimSearchResponse.json
Complete claim information including:
- Claim identification
- Protocol and policy numbers
- Financial summary (sdopag, totpag, valorPendente)
- Branch and insured names
- Product information
- Audit fields

#### 3. PaymentAuthorizationRequest.json
Payment authorization data:
- Payment type (1-5)
- Principal and correction amounts
- Beneficiary name (conditional)
- Policy type ('1' or '2')
- Observations

**Validation**:
- Payment type: 1-5 (enum)
- Amounts: decimal with 2 decimal places
- Beneficiary: pattern for valid names
- Policy type: '1' or '2' pattern

#### 4. PaymentAuthorizationResponse.json
Authorization confirmation with:
- Authorization number (ocorhist)
- Operation code (always 1098)
- Date and time
- Original and BTNF-converted amounts
- Updated pending balance

#### 5. DashboardOverviewResponse.json
Migration dashboard metrics:
- Overall progress percentage
- User story statuses
- Component migration summary (screens, rules, entities, services)
- System health indicators
- Test coverage

#### 6. ErrorResponse.json
Standardized error response:
- Success flag (always false)
- Error code (programmatic identification)
- Localized message in Portuguese
- Details array
- Timestamp
- Optional trace ID for correlation

### Usage

**Validate JSON against schema**:
```bash
# Using ajv-cli
npm install -g ajv-cli
ajv validate -s schemas/ClaimSearchRequest.json -d sample-request.json

# Using online validator
open https://www.jsonschemavalidator.net/
```

**Generate TypeScript types**:
```bash
# Using json-schema-to-typescript
npm install -g json-schema-to-typescript
json2ts -i schemas/*.json -o ../frontend/src/types/generated/
```

**Generate C# classes**:
```bash
# Using NJsonSchema
NJsonSchema --input schemas/ClaimSearchRequest.json --output ../backend/src/CaixaSeguradora.Core/DTOs/ClaimSearchRequest.cs
```

---

## Contract Testing

### REST API Contract Tests

**Using Pact (Consumer-Driven)**:
```typescript
// frontend/tests/contract/claims.pact.test.ts
import { PactV3 } from '@pact-foundation/pact';

const provider = new PactV3({
  consumer: 'ClaimsUI',
  provider: 'ClaimsAPI',
});

describe('Claims API Contract', () => {
  it('should search claims by protocol', async () => {
    await provider
      .given('a claim with protocol 001/0123456-7 exists')
      .uponReceiving('a request to search by protocol')
      .withRequest({
        method: 'POST',
        path: '/api/claims/search',
        body: {
          fonte: 1,
          protsini: 123456,
          dac: 7,
        },
      })
      .willRespondWith({
        status: 200,
        body: {
          sucesso: true,
          sinistro: {
            numeroProtocolo: '001/0123456-7',
            // ... full response
          },
        },
      });

    await provider.executeTest(async (mockServer) => {
      const client = new ClaimsApiClient(mockServer.url);
      const result = await client.searchClaims({
        fonte: 1,
        protsini: 123456,
        dac: 7,
      });
      expect(result.sucesso).toBe(true);
    });
  });
});
```

### SOAP Contract Tests

**Using SoapUI or Postman**:
1. Import WSDL files into test tool
2. Generate test cases for each operation
3. Mock external services (CNOUA, SIPUA, SIMDA)
4. Validate request/response XML against schema
5. Assert business rules

---

## Integration with .NET 9

### REST API Controllers

**Generate from OpenAPI**:
```bash
# Using NSwag Studio
# 1. Open rest-api.yaml in NSwag Studio
# 2. Select "CSharp Controller" generator
# 3. Configure namespace: CaixaSeguradora.Api.Controllers
# 4. Generate
```

**Manual Implementation**:
```csharp
[ApiController]
[Route("api/claims")]
[Produces("application/json")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;
    private readonly IMapper _mapper;

    public ClaimsController(IClaimService claimService, IMapper mapper)
    {
        _claimService = claimService;
        _mapper = mapper;
    }

    /// <summary>
    /// Pesquisar sinistros
    /// </summary>
    /// <param name="request">Critérios de pesquisa</param>
    /// <returns>Dados do sinistro encontrado</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(ClaimSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimSearchResponse>> SearchClaims(
        [FromBody] ClaimSearchRequest request)
    {
        // Implementation
    }
}
```

### SOAP Services with SoapCore

```csharp
[ServiceContract(Namespace = "http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1")]
public interface IAutenticacaoService
{
    [OperationContract]
    Task<AutenticarResponse> Autenticar(AutenticarRequest request);

    [OperationContract]
    Task<ValidarSessaoResponse> ValidarSessao(ValidarSessaoRequest request);

    [OperationContract]
    Task<EncerrarSessaoResponse> EncerrarSessao(EncerrarSessaoRequest request);
}
```

---

## Integration with React Frontend

### API Client Generation

**Using openapi-typescript-codegen**:
```bash
npx openapi-typescript-codegen \
  --input contracts/rest-api.yaml \
  --output frontend/src/services/generated \
  --client axios
```

**Generated client usage**:
```typescript
import { ClaimsService } from '@/services/generated';

// Search claims
const result = await ClaimsService.searchClaims({
  fonte: 1,
  protsini: 123456,
  dac: 7,
});

// Authorize payment
const authorization = await ClaimsService.authorizePayment(
  1, 10, 20, 789012,
  {
    tipoPagamento: 1,
    valorPrincipal: 25000.00,
    valorCorrecao: 1250.50,
    favorecido: 'João da Silva Santos',
    tipoApolice: '1',
  }
);
```

---

## Validation

### OpenAPI Validation

**Using Spectral**:
```bash
npm install -g @stoplight/spectral-cli
spectral lint rest-api.yaml
```

**Using Redocly CLI**:
```bash
npm install -g @redocly/cli
redocly lint rest-api.yaml
```

### WSDL Validation

**Using xmllint**:
```bash
xmllint --noout --schema http://schemas.xmlsoap.org/wsdl/ soap-autenticacao.wsdl
```

### JSON Schema Validation

**Using ajv**:
```bash
npm install -g ajv-cli
ajv compile -s schemas/ClaimSearchRequest.json
```

---

## Documentation Generation

### REST API Documentation

**Using Redoc**:
```bash
npx redoc-cli bundle rest-api.yaml -o docs/api-documentation.html
```

**Using Swagger UI**:
```bash
# Serve locally
npx swagger-ui-watcher rest-api.yaml
```

### WSDL Documentation

**Using wsdl2html**:
```bash
# Generate HTML documentation from WSDL
xsltproc wsdl-viewer.xsl soap-autenticacao.wsdl > docs/soap-autenticacao.html
```

---

## Best Practices

### API Design
✅ Use Portuguese field names for Brazilian business domain
✅ Include comprehensive examples in all schemas
✅ Provide detailed error messages with error codes
✅ Support both SOAP 1.1 and 1.2 for compatibility
✅ Use ISO 8601 for all date/time fields
✅ Include validation rules in schemas
✅ Provide clear operation descriptions

### Versioning
✅ Include version in namespace/URL (`/v1`)
✅ Use semantic versioning for contracts
✅ Maintain backward compatibility
✅ Document breaking changes

### Security
✅ Use JWT Bearer tokens for REST API
✅ Include session tokens in SOAP requests
✅ Never expose sensitive data in error messages
✅ Implement rate limiting
✅ Use HTTPS for all endpoints

### Testing
✅ Generate contract tests from schemas
✅ Validate all requests/responses
✅ Test error scenarios
✅ Mock external services
✅ Maintain parity tests with legacy system

---

## References

- **OpenAPI Specification**: https://spec.openapis.org/oas/v3.0.3
- **SOAP 1.2 Specification**: https://www.w3.org/TR/soap12/
- **JSON Schema Draft 07**: https://json-schema.org/draft-07/schema
- **SoapCore Documentation**: https://github.com/DigDes/SoapCore
- **NSwag Documentation**: https://github.com/RicoSuter/NSwag
- **Pact Contract Testing**: https://docs.pact.io/

---

## Changelog

### Version 1.0.0 - 2025-10-23
- Initial release
- Complete REST API specification (OpenAPI 3.0.3)
- SOAP WSDL specifications for 3 services
- 6 JSON Schemas for request/response validation
- Production-ready contracts
- Ready for code generation

---

## Support

For questions or issues related to API contracts:
- Technical Lead: ti@caixaseguradora.com.br
- Documentation: `/docs/api-documentation.html`
- Swagger UI: `https://api.caixaseguradora.com.br/swagger`
