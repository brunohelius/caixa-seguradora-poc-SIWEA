# Sistema Proposto: Migração SIWEA para .NET 9 + React

**Projeto:** Modernização do Sistema de Liberação de Pagamento de Sinistros
**Versão:** 1.0
**Data:** 2025-10-27
**Duração do Projeto:** 3 meses (13 semanas)
**Status:** Planejamento

---

## Índice

1. [Visão Executiva](#visão-executiva)
2. [Arquitetura do Sistema Proposto](#arquitetura-do-sistema-proposto)
3. [Stack Tecnológico](#stack-tecnológico)
4. [Funcionalidades Detalhadas](#funcionalidades-detalhadas)
5. [Comparativo: Legado vs Proposto](#comparativo-legado-vs-proposto)

---

## 1. Visão Executiva

### 1.1 Objetivo do Projeto

Migrar o sistema legado SIWEA (IBM VisualAge EZEE 4.40 em mainframe CICS) para uma arquitetura moderna baseada em:
- **Backend:** .NET 9 com Clean Architecture
- **Frontend:** React 19 + TypeScript
- **Cloud:** Microsoft Azure (ou on-premises compatível)

### 1.2 Justificativa

**Problemas do Sistema Atual:**
- ⚠️ Plataforma mainframe obsoleta (35+ anos)
- 💰 Alto custo de manutenção e licenciamento CICS
- 👥 Escassez de profissionais COBOL/EZEE
- 🔧 Dificuldade de evolução e manutenção
- 📱 Interface não responsiva (terminais 3270)
- 🔌 Integração limitada com sistemas modernos

**Benefícios da Migração:**
- ✅ Redução de custos operacionais (estimado: 40-60%)
- ✅ Stack tecnológico moderno e amplamente adotado
- ✅ Interface web responsiva (desktop + mobile)
- ✅ Facilidade de integração (APIs REST/SOAP)
- ✅ Melhor performance e escalabilidade
- ✅ Facilidade de recrutamento de talentos
- ✅ Base para futuras inovações (IA, analytics, etc)

### 1.3 Escopo do Projeto

**DENTRO DO ESCOPO:**
- ✅ Migração completa das 6 User Stories (100% funcionalidade)
- ✅ Preservação de 100% das regras de negócio
- ✅ Migração de dados históricos (sinistros, pagamentos)
- ✅ Interface web responsiva (preservando Site.css)
- ✅ Integrações externas (CNOUA, SIPUA, SIMDA)
- ✅ Testes automatizados (unitários, integração, E2E)
- ✅ Documentação completa (técnica e usuário)
- ✅ Treinamento de operadores
- ✅ Cutover e go-live

**FORA DO ESCOPO:**
- ❌ Modificação de regras de negócio
- ❌ Novos recursos além dos existentes
- ❌ Migração de outros sistemas além do SIWEA
- ❌ Redesenho de processos de negócio
- ❌ Mudança de sistemas externos (CNOUA, SIPUA, SIMDA)

### 1.4 Métricas de Sucesso

| Métrica | Meta |
|---------|------|
| **Paridade Funcional** | 100% das funcionalidades migradas |
| **Paridade de Regras** | 100% das 100 regras de negócio |
| **Performance** | Busca < 3s, Autorização < 90s |
| **Disponibilidade** | 99.5% durante horário comercial |
| **Acurácia de Dados** | 100% dos cálculos idênticos ao legado |
| **Adoção de Usuários** | 95% dos operadores sem treinamento adicional |
| **Prazo** | 3 meses (13 semanas) |
| **Orçamento** | Dentro do budget aprovado |

---

## 2. Arquitetura do Sistema Proposto

### 2.1 Visão de Alto Nível

```
┌─────────────────────────────────────────────────────────────────┐
│                         FRONTEND LAYER                          │
│                    React 19 + TypeScript                        │
│                          Vite 7.1                               │
├─────────────────────────────────────────────────────────────────┤
│  📱 Aplicação SPA                                               │
│  ├─ ClaimSearch.tsx          (Busca de sinistros)              │
│  ├─ ClaimDetails.tsx         (Detalhes e autorização)          │
│  ├─ PaymentHistory.tsx       (Histórico de pagamentos)         │
│  ├─ PhaseTimeline.tsx        (Fases do workflow)               │
│  └─ MigrationDashboard.tsx   (Dashboard de migração)           │
│                                                                 │
│  🎨 Componentes UI                                              │
│  ├─ Site.css (preservado 100%)                                 │
│  ├─ Formulários com validação em tempo real                    │
│  ├─ Mensagens de erro em português                             │
│  └─ Design responsivo (desktop + mobile)                       │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ HTTPS / JSON
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                         API LAYER                               │
│                   ASP.NET Core 9.0 Web API                      │
├─────────────────────────────────────────────────────────────────┤
│  🌐 REST Endpoints                                              │
│  ├─ GET  /api/claims/search                                    │
│  ├─ POST /api/claims/authorize-payment                         │
│  ├─ GET  /api/claims/{id}/history                              │
│  └─ GET  /api/claims/{id}/phases                               │
│                                                                 │
│  🧼 SOAP Endpoints (SoapCore)                                   │
│  ├─ /soap/autenticacao      (compatibilidade legado)           │
│  ├─ /soap/solicitacao        (compatibilidade legado)          │
│  └─ /soap/assunto            (compatibilidade legado)          │
│                                                                 │
│  🛡️ Middleware                                                  │
│  ├─ GlobalExceptionHandler                                     │
│  ├─ AuthenticationMiddleware (JWT)                             │
│  ├─ RequestLoggingMiddleware (Serilog)                         │
│  └─ ValidationMiddleware (FluentValidation)                    │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ In-Process
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                         CORE LAYER                              │
│                      (Business Logic)                           │
├─────────────────────────────────────────────────────────────────┤
│  💼 Domain Services                                             │
│  ├─ ClaimService                                                │
│  │  ├─ SearchClaims() - 3 modos de busca                       │
│  │  └─ GetClaimDetails() - recuperação completa                │
│  │                                                              │
│  ├─ PaymentAuthorizationService                                │
│  │  ├─ ValidatePayment() - BR-010 a BR-022                     │
│  │  ├─ ExecuteAuthorization() - pipeline 8 etapas              │
│  │  └─ RollbackOnFailure() - atomicidade                       │
│  │                                                              │
│  ├─ CurrencyConversionService                                  │
│  │  ├─ GetConversionRate() - TGEUNIMO lookup                   │
│  │  ├─ ConvertToBTNF() - BR-023 fórmula                        │
│  │  └─ CalculateTotals() - BR-029, BR-030, BR-031              │
│  │                                                              │
│  ├─ PhaseManagementService                                     │
│  │  ├─ DeterminePhaseChanges() - SI_REL_FASE_EVENTO            │
│  │  ├─ OpenPhase() - DATA_FECHA_SIFA='9999-12-31'              │
│  │  └─ ClosePhase() - atualiza data fechamento                 │
│  │                                                              │
│  └─ ExternalValidationService                                  │
│     ├─ RouteValidation() - CNOUA/SIPUA/SIMDA                   │
│     └─ ValidateProduct() - códigos de erro                     │
│                                                                 │
│  📋 Domain Entities (13 entidades)                             │
│  ├─ ClaimMaster, ClaimHistory, BranchMaster                    │
│  ├─ PolicyMaster, CurrencyUnit, SystemControl                  │
│  ├─ ClaimAccompaniment, ClaimPhase, PhaseEventRelationship    │
│  └─ ConsortiumContract, MigrationStatus, ComponentMigration   │
│                                                                 │
│  ✅ Validators (FluentValidation)                               │
│  └─ 100+ regras de negócio implementadas                       │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ In-Process
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE LAYER                         │
│                  (Data Access + External)                       │
├─────────────────────────────────────────────────────────────────┤
│  💾 Data Access                                                 │
│  ├─ ClaimsDbContext (EF Core 9)                                │
│  │  ├─ Database-first approach                                 │
│  │  ├─ Fluent API para mapeamento legado                       │
│  │  └─ Connection pooling                                      │
│  │                                                              │
│  ├─ Repository Pattern                                         │
│  │  ├─ ClaimRepository                                         │
│  │  ├─ PaymentHistoryRepository                                │
│  │  ├─ PhaseRepository                                         │
│  │  └─ CurrencyRateRepository                                  │
│  │                                                              │
│  └─ Unit of Work                                                │
│     └─ Transaction management (BEGIN/COMMIT/ROLLBACK)           │
│                                                                 │
│  🔌 External Service Clients                                    │
│  ├─ CNOUAClient (REST API)                                      │
│  │  ├─ HttpClient com Polly                                    │
│  │  ├─ Retry policy (3x exponential backoff)                   │
│  │  └─ Circuit breaker (5 failures → 30s break)                │
│  │                                                              │
│  ├─ SIPUAClient (SOAP)                                          │
│  │  └─ SoapClient com Polly                                    │
│  │                                                              │
│  └─ SIMDAClient (SOAP)                                          │
│     └─ SoapClient com Polly                                    │
└─────────────────────────────────────────────────────────────────┘
                            │
                            │ ADO.NET / SOAP
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    EXTERNAL SYSTEMS                             │
├─────────────────────────────────────────────────────────────────┤
│  🗄️  Database (SQL Server ou DB2)                              │
│  ├─ 10 tabelas legadas (schema preservado)                     │
│  └─ 3 tabelas novas (migration tracking)                       │
│                                                                 │
│  🔗 External Services                                           │
│  ├─ CNOUA (Consortium Validation)                              │
│  ├─ SIPUA (EFP Contract Validation)                            │
│  └─ SIMDA (HB Contract Validation)                             │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Padrões Arquiteturais

#### **Clean Architecture**
```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│         (Controllers, DTOs)             │
└────────────┬────────────────────────────┘
             │ Depende de ↓
┌────────────▼────────────────────────────┐
│         Application Layer               │
│    (Use Cases, Business Logic)          │
└────────────┬────────────────────────────┘
             │ Depende de ↓
┌────────────▼────────────────────────────┐
│           Domain Layer                  │
│      (Entities, Interfaces)             │
└────────────┬────────────────────────────┘
             │ Implementado por ↑
┌────────────▼────────────────────────────┐
│       Infrastructure Layer              │
│  (EF Core, External Clients, DB)        │
└─────────────────────────────────────────┘
```

**Benefícios:**
- ✅ Testabilidade: Core layer independente de infraestrutura
- ✅ Manutenibilidade: Separação clara de responsabilidades
- ✅ Flexibilidade: Fácil trocar banco ou frameworks

#### **Repository Pattern**
```csharp
public interface IClaimRepository
{
    Task<ClaimMaster?> GetByProtocolAsync(int fonte, int protsini, int dac);
    Task<ClaimMaster?> GetByClaimNumberAsync(int orgsin, int rmosin, int numsin);
    Task<ClaimMaster?> GetByLeaderCodeAsync(int codlider, int sinlid);
    Task UpdateAsync(ClaimMaster claim);
}
```

#### **Unit of Work Pattern**
```csharp
public interface IUnitOfWork : IDisposable
{
    IClaimRepository Claims { get; }
    IPaymentHistoryRepository PaymentHistory { get; }
    IPhaseRepository Phases { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
```

---

## 3. Stack Tecnológico

### 3.1 Backend (.NET 9)

| Componente | Tecnologia | Versão | Propósito |
|------------|------------|--------|-----------|
| **Framework** | .NET | 9.0 | Runtime e APIs |
| **Web Framework** | ASP.NET Core | 9.0 | Web API |
| **ORM** | Entity Framework Core | 9.0 | Acesso a dados |
| **SOAP Support** | SoapCore | 1.1.0 | Endpoints SOAP legados |
| **Validation** | FluentValidation | 11.9.0 | Validação de regras |
| **Mapping** | AutoMapper | 12.0.1 | Mapeamento DTO ↔ Entity |
| **Logging** | Serilog | 4.0.0 | Logging estruturado |
| **Resilience** | Polly | 8.2.0 | Retry, circuit breaker |
| **Authentication** | Microsoft.Identity | 9.0 | JWT tokens |
| **Testing** | xUnit | 2.6.0 | Testes unitários |
| **Mocking** | Moq | 4.20.0 | Mocks para testes |
| **Database** | SQL Server | 2022 | Banco de dados principal |
| **Database (Alt)** | IBM DB2 | 11.5 | Compatibilidade legado |

**Justificativas:**
- **.NET 9:** LTS, performance, suporte Microsoft até Nov 2028
- **EF Core:** Produtividade, migrations, LINQ
- **SoapCore:** Compatibilidade com integrações legadas SOAP
- **Polly:** Resiliência para chamadas externas (CNOUA, SIPUA, SIMDA)

### 3.2 Frontend (React 19)

| Componente | Tecnologia | Versão | Propósito |
|------------|------------|--------|-----------|
| **Framework** | React | 19.1.1 | UI library |
| **Language** | TypeScript | 5.9 | Type safety |
| **Build Tool** | Vite | 7.1.7 | Build e dev server |
| **Router** | React Router DOM | 7.9.4 | Navegação SPA |
| **HTTP Client** | Axios | 1.12.2 | Chamadas API |
| **State/Data** | TanStack React Query | 5.90.5 | Cache e sincronização |
| **Forms** | React Hook Form | 7.50.0 | Gerenciamento forms |
| **Validation** | Zod | 3.22.0 | Validação schemas |
| **Charts** | Recharts | 3.3.0 | Dashboard visualização |
| **Date** | date-fns | 3.0.0 | Manipulação datas |
| **Testing** | Jest | 29.7.0 | Testes unitários |
| **Testing** | React Testing Library | 14.0.0 | Testes componentes |
| **E2E** | Playwright | 1.40.0 | Testes end-to-end |

**Justificativas:**
- **React 19:** Concurrent features, server components
- **TypeScript:** Type safety, IntelliSense, refatoração segura
- **Vite:** Build rápido, HMR instantâneo
- **React Query:** Cache inteligente, invalidação automática
- **Playwright:** E2E confiável, cross-browser

### 3.3 DevOps e Infraestrutura

| Componente | Tecnologia | Propósito |
|------------|------------|-----------|
| **Containerization** | Docker | 24.0 | Containerização aplicação |
| **Orchestration** | Docker Compose | 2.0 | Dev environment |
| **CI/CD** | Azure DevOps / GitHub Actions | Pipeline automação |
| **Cloud** | Microsoft Azure | Hosting produção |
| **CDN** | Azure CDN | Frontend delivery |
| **Monitoring** | Application Insights | APM e logging |
| **Secrets** | Azure Key Vault | Gerenciamento secrets |
| **Database** | Azure SQL Database | Banco gerenciado |
| **API Gateway** | Azure API Management | Segurança APIs |

**Alternativa On-Premises:**
- Windows Server 2022
- IIS 10
- SQL Server 2022
- Self-hosted monitoring (Grafana + Prometheus)

---

## 4. Funcionalidades Detalhadas

### 4.1 User Story 1: Busca e Recuperação de Sinistros

**Prioridade:** P1 (MVP - Must Have)
**Complexidade:** Média
**Pontos de Função:** 12 PF

#### Funcionalidades

**F1.1: Busca por Protocolo**
- **Entrada:** FONTE (3 dígitos) + PROTSINI (7 dígitos) + DAC (1 dígito)
- **Formato:** 001/0123456-7
- **Query:** `SELECT * FROM TMESTSIN WHERE FONTE=? AND PROTSINI=? AND DAC=?`
- **Validação:** Campos numéricos, todos obrigatórios
- **Resposta:** 0 ou 1 sinistro (protocolo único)
- **Tempo:** < 3 segundos

**F1.2: Busca por Número do Sinistro**
- **Entrada:** ORGSIN (2 dígitos) + RMOSIN (2 dígitos) + NUMSIN (6 dígitos)
- **Formato:** 10/20/789012
- **Query:** `SELECT * FROM TMESTSIN WHERE ORGSIN=? AND RMOSIN=? AND NUMSIN=?`
- **Validação:** Campos numéricos, todos obrigatórios
- **Resposta:** 0 ou 1 sinistro (número único)

**F1.3: Busca por Código Líder**
- **Entrada:** CODLIDER (3 dígitos) + SINLID (7 dígitos)
- **Formato:** 001/0000001
- **Query:** `SELECT * FROM TMESTSIN WHERE CODLIDER=? AND SINLID=?`
- **Validação:** Campos numéricos, ambos obrigatórios
- **Resposta:** 0 ou mais sinistros

**F1.4: Exibição de Dados Completos**
- **Protocolo:** FONTE/PROTSINI-DAC
- **Sinistro:** ORGSIN/RMOSIN/NUMSIN
- **Apólice:** ORGAPO/RMOAPO/NUMAPOL
- **Ramo:** NOMERAMO (join com TGERAMO)
- **Segurado:** NOME (join com TAPOLICE)
- **Saldo a Pagar:** SDOPAG (formatado R$ #.###,##)
- **Total Pago:** TOTPAG (formatado)
- **Saldo Pendente:** SDOPAG - TOTPAG (calculado)
- **Tipo Seguro:** TPSEGU
- **Produto:** CODPRODU

**F1.5: Tratamento de Erros**
- Sinistro não encontrado → "PROTOCOLO XXX NAO ENCONTRADO"
- Erro de conexão → "Erro ao conectar ao banco de dados"
- Timeout → "Tempo de busca excedido"

#### Interface (React)

**Componente:** `ClaimSearch.tsx`

```tsx
interface SearchCriteria {
  mode: 'protocol' | 'claim' | 'leader';
  protocol?: { fonte: string; protsini: string; dac: string };
  claim?: { orgsin: string; rmosin: string; numsin: string };
  leader?: { codlider: string; sinlid: string };
}

// Validação em tempo real
const schema = z.object({
  mode: z.enum(['protocol', 'claim', 'leader']),
  // ... validações por modo
});

// Hook React Query
const { data, isLoading, error } = useQuery({
  queryKey: ['claim', criteria],
  queryFn: () => claimService.search(criteria),
  enabled: isValid,
});
```

**Layout:**
- 3 seções colapsáveis (accordion)
- Radio button para selecionar modo
- Validação em tempo real (campo vermelho se inválido)
- Botão "Pesquisar" desabilitado até formulário válido
- Loading spinner durante busca
- Erro exibido em vermelho (#e80c4d)

#### Backend (.NET)

**Controller:** `ClaimsController.cs`

```csharp
[ApiController]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(ClaimDetailsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ClaimDetailsDto>> Search(
        [FromQuery] SearchClaimQuery query)
    {
        var result = await _mediator.Send(query);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(result.Error);
    }
}
```

**Service:** `ClaimService.cs`

```csharp
public async Task<Result<ClaimDetailsDto>> SearchAsync(
    SearchCriteria criteria)
{
    // BR-001: Validar critério mutuamente exclusivo
    // BR-002: Ao menos um critério completo

    ClaimMaster? claim = criteria.Mode switch
    {
        SearchMode.Protocol => await _repo.GetByProtocolAsync(...),
        SearchMode.Claim => await _repo.GetByClaimNumberAsync(...),
        SearchMode.Leader => await _repo.GetByLeaderCodeAsync(...),
        _ => null
    };

    if (claim == null)
        return Result.Failure<ClaimDetailsDto>("BR-006: Sinistro não encontrado");

    // BR-007: Recuperar nome do ramo
    // BR-008: Recuperar nome do segurado
    // BR-009: Formatar valores monetários

    return Result.Success(_mapper.Map<ClaimDetailsDto>(claim));
}
```

#### Testes

**Unitários:**
- ✅ Busca por protocolo válido retorna sinistro
- ✅ Busca por protocolo inválido retorna 404
- ✅ Busca por número de sinistro funciona
- ✅ Busca por líder retorna múltiplos resultados
- ✅ Validação de campos obrigatórios
- ✅ Formatação de valores monetários

**Integração:**
- ✅ Query no banco retorna dados corretos
- ✅ Joins com TGERAMO e TAPOLICE funcionam
- ✅ Timeout de 3 segundos é respeitado

**E2E (Playwright):**
- ✅ Usuário pode buscar por protocolo e ver resultados
- ✅ Mensagem de erro exibida quando não encontrado
- ✅ Loading spinner aparece durante busca

---

### 4.2 User Story 2: Autorização de Pagamento

**Prioridade:** P2 (Core - Must Have)
**Complexidade:** Alta
**Pontos de Função:** 35 PF

#### Funcionalidades

**F2.1: Formulário de Autorização**

**Campos de Entrada:**
1. **Tipo de Pagamento** (obrigatório)
   - Tipo: Select
   - Valores: 1, 2, 3, 4, 5
   - Validação: BR-010, BR-011

2. **Tipo de Apólice** (obrigatório)
   - Tipo: Select
   - Valores: 1, 2
   - Validação: BR-015, BR-016

3. **Valor Principal** (obrigatório)
   - Tipo: Decimal input
   - Formato: R$ #.###,##
   - Validação: BR-012, BR-013 (≤ saldo pendente)
   - Máscara: currency input

4. **Valor Correção** (opcional)
   - Tipo: Decimal input
   - Formato: R$ #.###,##
   - Default: 0,00
   - Validação: BR-017, BR-018

5. **Beneficiário** (condicional)
   - Tipo: Text input
   - Max: 255 caracteres
   - Obrigatório: SE TPSEGU ≠ 0
   - Validação: BR-019, BR-020, BR-021, BR-022

**F2.2: Pipeline de Autorização (8 Etapas)**

```
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 1: Validação de Entrada                              │
├─────────────────────────────────────────────────────────────┤
│  • Validar tipo pagamento (1-5)                             │
│  • Validar tipo apólice (1-2)                               │
│  • Validar valor principal > 0                              │
│  • Validar valor ≤ saldo pendente                           │
│  • Validar beneficiário (se TPSEGU ≠ 0)                     │
│  ❌ Falha → Retornar erro, ABORTAR                          │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 2: Obter Data Comercial e Taxa de Conversão         │
├─────────────────────────────────────────────────────────────┤
│  • Query TSISTEMA.DTMOVABE (WHERE IDSISTEM='SI')            │
│  • Query TGEUNIMO.VLCRUZAD (WHERE date BETWEEN ...)         │
│  • Validar taxa encontrada                                  │
│  ❌ Falha → "Taxa não disponível", ABORTAR                  │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 3: Cálculo de Conversão BTNF                         │
├─────────────────────────────────────────────────────────────┤
│  • VALPRIBT = VALPRI × VLCRUZAD (8 decimais)                │
│  • CRRMONBT = CRRMON × VLCRUZAD                             │
│  • VALTOTBT = VALPRIBT + CRRMONBT                           │
│  • Arredondar para 2 decimais (Banker's Rounding)          │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 4: Validação Externa                                 │
├─────────────────────────────────────────────────────────────┤
│  • Determinar roteamento:                                   │
│    - CODPRODU IN (6814,7701,7709) → CNOUA                   │
│    - NUM_CONTRATO > 0 → SIPUA                               │
│    - Senão → SIMDA                                          │
│  • Chamar serviço externo (timeout 10s)                     │
│  • Verificar EZERT8 == '00000000'                           │
│  ❌ Falha validação → Retornar erro, ABORTAR                │
│  ❌ Timeout/indisponível → Retry 3x, ABORTAR                │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 5: BEGIN TRANSACTION                                 │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 6: Criar Histórico de Pagamento (THISTSIN)          │
├─────────────────────────────────────────────────────────────┤
│  INSERT INTO THISTSIN:                                      │
│  • OPERACAO = 1098 (fixo)                                   │
│  • DTMOVTO = data comercial (TSISTEMA)                      │
│  • HORAOPER = hora atual sistema                            │
│  • TIPCRR = '5' (fixo)                                      │
│  • VALPRI, CRRMON, VALPRIBT, CRRMONBT, VALTOTBT            │
│  • SITCONTB = '0', SITUACAO = '0'                           │
│  • EZEUSRID = usuário autenticado                           │
│  • OCORHIST = TMESTSIN.OCORHIST + 1 (novo)                  │
│  ❌ Falha → ROLLBACK, retornar erro                         │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 7: Atualizar Mestre (TMESTSIN)                       │
├─────────────────────────────────────────────────────────────┤
│  UPDATE TMESTSIN:                                           │
│  • TOTPAG += VALTOTBT                                       │
│  • OCORHIST += 1                                            │
│  • UPDATED_BY, UPDATED_AT                                   │
│  ❌ Falha → ROLLBACK, retornar erro                         │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 8: Criar Acompanhamento (SI_ACOMPANHA_SINI)         │
├─────────────────────────────────────────────────────────────┤
│  INSERT INTO SI_ACOMPANHA_SINI:                             │
│  • COD_EVENTO = 1098                                        │
│  • DATA_MOVTO_SINIACO = data comercial                      │
│  • COD_USUARIO = usuário autenticado                        │
│  ❌ Falha → ROLLBACK, retornar erro                         │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 9: Atualizar Fases (SI_SINISTRO_FASE)               │
├─────────────────────────────────────────────────────────────┤
│  • Query SI_REL_FASE_EVENTO (WHERE COD_EVENTO=1098)         │
│  • Para cada relacionamento:                                │
│    - IND_ALTERACAO_FASE='1' → ABRIR fase (9999-12-31)      │
│    - IND_ALTERACAO_FASE='2' → FECHAR fase (data atual)     │
│  ❌ Falha → ROLLBACK, retornar erro                         │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ETAPA 10: COMMIT TRANSACTION                               │
└─────────────────────────────────────────────────────────────┘
                          ↓ OK
┌─────────────────────────────────────────────────────────────┐
│  ✅ SUCESSO                                                  │
│  • Retornar confirmação                                     │
│  • Exibir dados atualizados (novo saldo, histórico)        │
└─────────────────────────────────────────────────────────────┘
```

**F2.3: Tratamento de Erros e Rollback**

```csharp
public async Task<Result<PaymentAuthorizationResult>> AuthorizeAsync(
    PaymentAuthorizationCommand command)
{
    using var transaction = await _unitOfWork.BeginTransactionAsync();

    try
    {
        // Etapas 1-4: Validação e preparação
        var validationResult = await ValidateAsync(command);
        if (!validationResult.IsSuccess)
            return Result.Failure<PaymentAuthorizationResult>(
                validationResult.Error);

        // Etapas 5-10: Transação atômica
        await CreateHistoryAsync(command, validationResult.Value);
        await UpdateClaimMasterAsync(command);
        await CreateAccompanimentAsync(command);
        await UpdatePhasesAsync(command);

        await transaction.CommitAsync();

        return Result.Success(new PaymentAuthorizationResult { ... });
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "Falha na autorização de pagamento");
        return Result.Failure<PaymentAuthorizationResult>(
            "Erro ao processar autorização. Nenhuma alteração foi realizada.");
    }
}
```

#### Interface (React)

**Componente:** `PaymentAuthorization.tsx`

```tsx
const PaymentAuthorizationForm: React.FC<Props> = ({ claim }) => {
  const { register, handleSubmit, watch, formState } = useForm<FormData>({
    resolver: zodResolver(paymentSchema),
  });

  const valorPrincipal = watch('valorPrincipal');
  const saldoPendente = claim.sdopag - claim.totpag;

  // Validação em tempo real: valor ≤ saldo pendente
  useEffect(() => {
    if (valorPrincipal > saldoPendente) {
      setError('valorPrincipal', {
        message: 'Valor Superior ao Saldo Pendente',
      });
    }
  }, [valorPrincipal, saldoPendente]);

  const mutation = useMutation({
    mutationFn: claimService.authorizePayment,
    onSuccess: () => {
      toast.success('Pagamento autorizado com sucesso!');
      queryClient.invalidateQueries(['claim', claim.id]);
    },
    onError: (error) => {
      toast.error(error.message);
    },
  });

  return (
    <form onSubmit={handleSubmit(mutation.mutate)}>
      {/* Campos do formulário */}
      {/* Validações em tempo real */}
      {/* Loading state durante processamento */}
    </form>
  );
};
```

**Estados da UI:**
- **Idle:** Formulário vazio, botão "Autorizar" desabilitado
- **Validating:** Validação em tempo real, erros exibidos
- **Valid:** Formulário válido, botão "Autorizar" habilitado
- **Submitting:** Loading spinner, botão desabilitado, "Processando..."
- **Success:** Toast de sucesso, formulário reseta, dados atualizados
- **Error:** Toast de erro, formulário permanece preenchido

#### Testes

**Unitários (Backend):**
- ✅ Validação de tipo pagamento (1-5)
- ✅ Validação valor ≤ saldo pendente
- ✅ Validação beneficiário obrigatório (TPSEGU ≠ 0)
- ✅ Cálculo conversão BTNF (precisão 8 → 2 decimais)
- ✅ Roteamento validação (CNOUA/SIPUA/SIMDA)
- ✅ Rollback quando validação externa falha
- ✅ Rollback quando insert histórico falha
- ✅ Rollback quando update mestre falha
- ✅ Rollback quando update fases falha

**Integração:**
- ✅ Pipeline completo de autorização
- ✅ Transação atômica (ACID)
- ✅ Chamadas externas com Polly (retry, circuit breaker)
- ✅ Histórico criado corretamente
- ✅ Saldo atualizado corretamente
- ✅ Fases atualizadas conforme configuração

**E2E (Playwright):**
- ✅ Usuário pode autorizar pagamento válido
- ✅ Erro exibido quando valor excede saldo
- ✅ Beneficiário obrigatório para tipo seguro específico
- ✅ Loading exibido durante processamento
- ✅ Sucesso exibido após autorização
- ✅ Histórico atualizado na tela

---

### 4.3 User Story 3: Histórico de Pagamentos

**Prioridade:** P3 (Important - Should Have)
**Complexidade:** Baixa
**Pontos de Função:** 8 PF

#### Funcionalidades

**F3.1: Listagem de Histórico**
- **Query:** `SELECT * FROM THISTSIN WHERE (chave sinistro) ORDER BY OCORHIST DESC`
- **Paginação:** 20 registros por página (configurável até 100)
- **Ordenação:** OCORHIST descendente (mais recente primeiro)
- **Performance:** < 500ms para 1000+ registros (index cobridor)

**Colunas Exibidas:**
- Data (DTMOVTO) - formato DD/MM/YYYY
- Hora (HORAOPER) - formato HH:MM:SS
- Valor Principal (VALPRI) - formatado R$ #.###,##
- Valor Correção (CRRMON) - formatado R$ #.###,##
- Total (VALTOTBT) - formatado R$ #.###,## (em BTNF)
- Beneficiário (NOMFAV)
- Operador (EZEUSRID)
- Status Contábil (SITCONTB)
- Situação (SITUACAO)

**F3.2: Filtros e Busca**
- Filtro por data (range)
- Filtro por operador
- Filtro por status contábil
- Busca por beneficiário (LIKE)

**F3.3: Exportação**
- Exportar para Excel (XLSX)
- Exportar para CSV
- Exportar para PDF (relatório formatado)

#### Interface (React)

```tsx
const PaymentHistory: React.FC<{ claimId: string }> = ({ claimId }) => {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const { data, isLoading } = useQuery({
    queryKey: ['payment-history', claimId, page, pageSize],
    queryFn: () => claimService.getPaymentHistory(claimId, page, pageSize),
    keepPreviousData: true,
  });

  return (
    <div className="payment-history">
      <table>
        <thead>
          <tr>
            <th>Data</th>
            <th>Hora</th>
            <th>Valor</th>
            <th>Beneficiário</th>
            <th>Operador</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {data?.items.map(payment => (
            <tr key={payment.ocorhist}>
              <td>{formatDate(payment.dtmovto)}</td>
              <td>{payment.horaoper}</td>
              <td>{formatCurrency(payment.valtotbt)}</td>
              <td>{payment.nomfav}</td>
              <td>{payment.ezeusrid}</td>
              <td>{getStatusLabel(payment.situacao)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <Pagination
        current={page}
        total={data?.totalPages}
        onChange={setPage}
      />
    </div>
  );
};
```

---

### 4.4 User Story 4: Produtos Especiais (Consórcio)

**Prioridade:** P4 (Nice to Have - Could Have)
**Complexidade:** Média
**Pontos de Função:** 10 PF

*(Detalhamento similar às anteriores)*

---

### 4.5 User Story 5: Gestão de Fases e Workflow

**Prioridade:** P5 (Nice to Have - Could Have)
**Complexidade:** Média
**Pontos de Função:** 12 PF

*(Detalhamento similar às anteriores)*

---

### 4.6 User Story 6: Dashboard de Migração

**Prioridade:** P6 (Monitoring - Should Have)
**Complexidade:** Média
**Pontos de Função:** 15 PF

*(Detalhamento similar às anteriores)*

---

## 5. Comparativo: Legado vs Proposto

### 5.1 Arquitetura

| Aspecto | Legado (VisualAge) | Proposto (.NET 9 + React) |
|---------|-------------------|---------------------------|
| **Camada Apresentação** | Terminal 3270, CICS Maps | React 19 SPA, Responsivo |
| **Camada Negócio** | COBOL/EZEE Monolítico | C# Microservices, Clean Arch |
| **Camada Dados** | DB2 com SQL embutido | EF Core, Repository Pattern |
| **Integrações** | CICS Link, MQ | REST/SOAP com Polly |
| **Autenticação** | RACF Mainframe | JWT + Azure AD |
| **Logging** | CICS Logs | Serilog estruturado |
| **Monitoramento** | CICS Region Stats | Application Insights |

### 5.2 Performance

| Operação | Legado | Proposto | Melhoria |
|----------|--------|----------|----------|
| **Busca Sinistro** | 2-4s | < 3s | ✅ Similar |
| **Autorização** | 60-90s | < 90s | ✅ Similar |
| **Histórico (1000 reg)** | 1-2s | < 500ms | ✅ 50-75% mais rápido |
| **Throughput** | 500 tx/h | 1000+ tx/h | ✅ 100% aumento |
| **Usuários Concorrentes** | 50 | 200+ | ✅ 4x mais |

### 5.3 Custos (Estimado - Anual)

| Item | Legado | Proposto | Economia |
|------|--------|----------|----------|
| **Licenças CICS** | R$ 500k | R$ 0 | R$ 500k |
| **Licenças DB2** | R$ 200k | R$ 80k (SQL Srv) | R$ 120k |
| **Mainframe MIPS** | R$ 300k | R$ 0 | R$ 300k |
| **Infra Cloud** | R$ 0 | R$ 150k (Azure) | -R$ 150k |
| **Manutenção** | R$ 400k | R$ 200k | R$ 200k |
| **TOTAL** | **R$ 1.4M** | **R$ 430k** | **R$ 970k (69%)** |

---

*(Este documento será continuado em parte 2 com Análise de Pontos de Função, Esforço e Cronograma)*

**FIM DA PARTE 1/6 - SISTEMA PROPOSTO**
