# SIWEA - Sistema Legado: Visão Geral Completa

**Documento:** Documentação Completa do Sistema Legado SIWEA
**Versão:** 1.0
**Data:** 2025-10-27
**Autor:** Análise Técnica Completa
**Propósito:** Documentação detalhada para migração para .NET 9 + React

---

## Índice de Documentação Completa

Este é o documento mestre da documentação do sistema legado SIWEA. A documentação está organizada em 5 documentos interconectados:

1. **SISTEMA_LEGADO_VISAO_GERAL.md** (este documento) - Visão geral, histórico e contexto
2. **SISTEMA_LEGADO_REGRAS_NEGOCIO.md** - Todas as 100+ regras de negócio detalhadas
3. **SISTEMA_LEGADO_ARQUITETURA.md** - Arquitetura técnica e integrações externas
4. **SISTEMA_LEGADO_MODELO_DADOS.md** - Modelo de dados completo (13 entidades)
5. **SISTEMA_LEGADO_PROCESSOS.md** - Processos, fluxos de trabalho e pipeline de transações

---

## 1. Identificação do Sistema

### 1.1 Informações Básicas

| Atributo | Valor |
|----------|-------|
| **Nome Completo** | SIWEA - Sistema de Liberação de Pagamento de Sinistros |
| **Sigla** | SIWEA (Sistema de Indenização e Workflow de Eventos Atendidos) |
| **Programa Legado** | #SIWEA-V116.esf |
| **Plataforma Original** | IBM VisualAge EZEE 4.40 |
| **Ambiente** | Mainframe CICS |
| **Linguagem** | COBOL/EZEE |
| **Tamanho Fonte** | 851.9 KB |
| **Organização** | Caixa Seguradora |
| **Domínio** | Seguros e Indenizações |

### 1.2 Histórico de Desenvolvimento

| Marco | Data | Responsável | Versão | Descrição |
|-------|------|-------------|--------|-----------|
| **Criação Original** | Outubro 1989 | COSMO (Analista), SOLANGE (Programadora) | V1 | Sistema inicial de autorização de pagamentos |
| **Última Revisão** | 11/02/2014 | CAD73898 | V90 | Versão atual em produção |
| **Análise Migração** | 23/10/2025 | Equipe Técnica | - | Análise completa para migração .NET 9 |
| **Início Migração** | 27/10/2025 | Equipe Desenvolvimento | - | Início projeto POC Visual Age |

**Tempo em Produção:** 35+ anos (1989-2025)
**Versões Documentadas:** 90 revisões ao longo da vida útil

### 1.3 Contexto de Negócio

**Domínio:** Gestão de Sinistros de Seguros
**Área de Negócio:** Operações de Indenização
**Usuários Primários:** Operadores de sinistros da Caixa Seguradora
**Criticidade:** ALTA - Sistema crítico de missão para pagamentos de indenizações

---

## 2. Propósito e Objetivo do Sistema

### 2.1 Objetivo Principal

O SIWEA é o sistema corporativo responsável por **gerenciar a autorização de pagamentos de sinistros de seguros** na Caixa Seguradora. Ele permite que operadores autorizados:

1. **Localizem sinistros** registrados no sistema usando múltiplos critérios de busca
2. **Autorizem pagamentos** de indenizações após validação de regras de negócio
3. **Gerenciem o workflow** de processamento através de fases configuráveis
4. **Mantenham auditoria completa** de todas as operações realizadas
5. **Integrem com sistemas externos** de validação de contratos e produtos

### 2.2 Escopo Funcional

**O sistema COBRE:**
- ✅ Busca de sinistros por protocolo, número ou código líder
- ✅ Visualização de dados cadastrais do sinistro e segurado
- ✅ Cálculo de saldo pendente (reserva esperada - pagamentos realizados)
- ✅ Entrada de dados de autorização de pagamento
- ✅ Validação de regras de negócio (tipo pagamento, valores, beneficiário)
- ✅ Conversão monetária para moeda padronizada (BTNF)
- ✅ Integração com 3 sistemas de validação externa (CNOUA, SIPUA, SIMDA)
- ✅ Registro de histórico de pagamentos
- ✅ Gestão de fases do sinistro (abertura/fechamento automático)
- ✅ Auditoria completa com ID do operador e timestamps
- ✅ Controle de concorrência e integridade transacional

**O sistema NÃO COBRE:**
- ❌ Cadastro inicial de sinistros (feito em outro sistema)
- ❌ Cadastro de apólices e segurados (sistema externo)
- ❌ Cálculo de reservas técnicas (sistema atuarial)
- ❌ Emissão de relatórios gerenciais (sistema de BI)
- ❌ Gestão de documentação do sinistro (sistema de GED)
- ❌ Pagamento efetivo (sistema financeiro/bancário)

### 2.3 Valor de Negócio

**Benefícios Operacionais:**
- ⚡ **Agilidade:** Autorização de pagamentos em menos de 90 segundos
- 🔒 **Segurança:** Validação automática contra sistemas externos antes do pagamento
- 📊 **Auditoria:** Rastreabilidade completa de quem, quando e o quê foi autorizado
- 💰 **Controle Financeiro:** Impede pagamentos acima do saldo pendente
- 🔄 **Workflow:** Gerenciamento automático de fases do processo
- 🎯 **Precisão:** Conversão monetária padronizada com 8 casas decimais

**Impacto Financeiro:**
- Processa autorizações de pagamento de sinistros de seguros
- Controla reservas técnicas vs pagamentos realizados
- Previne pagamentos duplicados ou acima do devido
- Integra validações de contratos de consórcio, EFP e HB

---

## 3. Visão de Alto Nível - Funcionalidades

### 3.1 Mapa de Funcionalidades

```
┌─────────────────────────────────────────────────────────────┐
│                     SIWEA - Sistema                         │
│        Liberação de Pagamento de Sinistros                  │
└─────────────────────────────────────────────────────────────┘
                           │
        ┌──────────────────┼──────────────────┐
        ▼                  ▼                  ▼
┌───────────────┐  ┌───────────────┐  ┌───────────────┐
│   BUSCA DE    │  │  AUTORIZAÇÃO  │  │   HISTÓRICO   │
│   SINISTROS   │  │  PAGAMENTO    │  │   & AUDIT     │
│   (SI11M010)  │  │  (SIHM020)    │  │               │
└───────────────┘  └───────────────┘  └───────────────┘
        │                  │                  │
        │                  │                  │
  ┌─────┴─────┐      ┌─────┴─────┐      ┌────┴────┐
  ▼     ▼     ▼      ▼     ▼     ▼      ▼         ▼
Prot  Num  Líder   Val   Conv  Valid  Hist     Fases
                    Neg   Moeda Externa

┌─────────────────────────────────────────────────────────────┐
│                  INTEGRAÇÕES EXTERNAS                       │
├─────────────────┬─────────────────┬─────────────────────────┤
│  CNOUA          │  SIPUA          │  SIMDA                  │
│  (Consórcio)    │  (EFP)          │  (HB)                   │
│  REST API       │  SOAP 1.2       │  SOAP 1.2               │
└─────────────────┴─────────────────┴─────────────────────────┘
```

### 3.2 Funcionalidades Principais (6 User Stories)

#### **US1: Busca e Recuperação de Sinistros** (Prioridade: P1)
- **Entrada:** Protocolo OU Número do Sinistro OU Código Líder
- **Processo:** Consulta TMESTSIN, TGERAMO, TAPOLICE
- **Saída:** Dados completos do sinistro (protocolo, apólice, segurado, valores)
- **Tempo:** < 3 segundos
- **Status:** 🟢 Funcionalidade base - MVP

#### **US2: Autorização de Pagamento** (Prioridade: P2)
- **Entrada:** Tipo pagamento, valor principal, correção, beneficiário
- **Processo:** Pipeline 8 etapas (validação → conversão → externa → transação)
- **Saída:** Histórico criado, saldo atualizado, fases gerenciadas
- **Tempo:** < 90 segundos (incluindo validação externa)
- **Status:** 🟢 Funcionalidade core - essencial

#### **US3: Histórico e Auditoria** (Prioridade: P3)
- **Entrada:** Sinistro selecionado
- **Processo:** Consulta THISTSIN com paginação
- **Saída:** Lista de pagamentos (data, valor, operador, status)
- **Tempo:** < 500ms para 1000+ registros
- **Status:** 🟡 Importante para auditoria

#### **US4: Produtos Especiais (Consórcio)** (Prioridade: P4)
- **Entrada:** Código produto (6814, 7701, 7709)
- **Processo:** Roteamento para CNOUA + validação contrato
- **Saída:** Validação aprovada/rejeitada com códigos de erro
- **Tempo:** Incluído nos 90s do US2
- **Status:** 🟡 Específico para produtos consórcio

#### **US5: Gestão de Fases e Workflow** (Prioridade: P5)
- **Entrada:** Evento (ex: 1098 = pagamento autorizado)
- **Processo:** Consulta SI_REL_FASE_EVENTO → atualiza SI_SINISTRO_FASE
- **Saída:** Fases abertas/fechadas automaticamente
- **Tempo:** Parte da transação US2
- **Status:** 🟡 Workflow automático

#### **US6: Dashboard de Migração** (Prioridade: P6)
- **Entrada:** Dados do projeto de migração
- **Processo:** Agregação métricas, testes, performance
- **Saída:** Dashboard visual (progress, status, health)
- **Tempo:** Auto-refresh 30s
- **Status:** 🔵 Específico da migração

---

## 4. Arquitetura Visual do Sistema

### 4.1 Arquitetura de 3 Camadas (Legado)

```
┌─────────────────────────────────────────────────────────────┐
│                    CAMADA APRESENTAÇÃO                      │
│                      (CICS Maps)                            │
├─────────────────────────────────────────────────────────────┤
│  SI11M010 - Busca          │  SIHM020 - Autorização         │
│  (SIWEG, SIWEGH maps)      │  (Formulário entrada)          │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    CAMADA NEGÓCIO                           │
│                  (COBOL/EZEE Programs)                      │
├─────────────────────────────────────────────────────────────┤
│  • Validação regras (100+ regras)                           │
│  • Conversão monetária (BTNF)                               │
│  • Roteamento validação externa                             │
│  • Gestão transações                                        │
│  • Gestão fases (PTFASESS subroutine)                       │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    CAMADA DADOS                             │
│                    (DB2 Tables)                             │
├─────────────────────────────────────────────────────────────┤
│  • TMESTSIN (Claims Master)                                 │
│  • THISTSIN (Payment History)                               │
│  • TGEUNIMO (Currency Rates)                                │
│  • TSISTEMA (Business Date)                                 │
│  • SI_* (Workflow & Audit)                                  │
│  • 13 entidades total                                       │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  INTEGRAÇÕES EXTERNAS                       │
├─────────────────┬─────────────────┬─────────────────────────┤
│  CNOUA          │  SIPUA          │  SIMDA                  │
│  Consortium     │  EFP Contracts  │  HB Contracts           │
└─────────────────┴─────────────────┴─────────────────────────┘
```

### 4.2 Arquitetura Futura (.NET 9 + React)

```
┌─────────────────────────────────────────────────────────────┐
│                   FRONTEND (React 19)                       │
│                   (Vite, TypeScript)                        │
├─────────────────────────────────────────────────────────────┤
│  • ClaimSearch.tsx                                          │
│  • PaymentAuthorization.tsx                                 │
│  • PaymentHistory.tsx                                       │
│  • MigrationDashboard.tsx                                   │
│  • Site.css (preservado 100%)                               │
└─────────────────────────────────────────────────────────────┘
                           │ HTTP/HTTPS
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              API LAYER (ASP.NET Core 9.0)                   │
│              CaixaSeguradora.Api                            │
├─────────────────────────────────────────────────────────────┤
│  • ClaimsController (REST endpoints)                        │
│  • SoapAutorizacaoService (SOAP endpoints - SoapCore)       │
│  • DTOs, AutoMapper, FluentValidation                       │
│  • GlobalExceptionHandler                                   │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              CORE LAYER (Business Logic)                    │
│              CaixaSeguradora.Core                           │
├─────────────────────────────────────────────────────────────┤
│  • ClaimService (search, validation)                        │
│  • PaymentAuthorizationService (pipeline 8 steps)           │
│  • CurrencyConversionService (BTNF)                         │
│  • PhaseManagementService (workflow)                        │
│  • Business Rules (100+ rules)                              │
│  • Domain Entities (13 entities)                            │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│           INFRASTRUCTURE LAYER (Data Access)                │
│           CaixaSeguradora.Infrastructure                    │
├─────────────────────────────────────────────────────────────┤
│  • ClaimsDbContext (EF Core 9)                              │
│  • Repository Pattern                                       │
│  • External Service Clients (Polly resilience)              │
│  • Database-first approach (legacy schema)                  │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   DATABASE (SQL Server/DB2)                 │
│                   (Legacy Schema Preserved)                 │
├─────────────────────────────────────────────────────────────┤
│  13 entidades (10 legadas + 3 dashboard)                    │
│  Indexes otimizados para performance                        │
└─────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────┐
│              EXTERNAL SERVICES (Existing APIs)              │
├─────────────────┬─────────────────┬─────────────────────────┤
│  CNOUA Client   │  SIPUA Client   │  SIMDA Client           │
│  (HttpClient)   │  (SoapClient)   │  (SoapClient)           │
│  Polly Retry    │  Polly Retry    │  Polly Retry            │
└─────────────────┴─────────────────┴─────────────────────────┘
```

---

## 5. Principais Telas do Sistema

### 5.1 Tela SI11M010 - Busca de Sinistros

**Identificação:**
- Map Group: SIWEG, SIWEGH
- Função: Entrada de critérios de busca

**Layout ASCII:**
```
┌──────────────────────────────────────────────────────────────┐
│                    [ LOGO CAIXA SEGURADORA ]                 │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  BUSCA DE SINISTROS                                          │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Critério 1: Por Protocolo                          │    │
│  │  ────────────────────────────                       │    │
│  │  Fonte:     [___]                                   │    │
│  │  Protocolo: [________]                              │    │
│  │  DAC:       [_]                                     │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Critério 2: Por Número do Sinistro                 │    │
│  │  ─────────────────────────────────                  │    │
│  │  Órgão: [__]  Ramo: [__]  Número: [______]         │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Critério 3: Por Código Líder (Resseguro)           │    │
│  │  ─────────────────────────────────────              │    │
│  │  Código Líder: [___]  Sinistro Líder: [_______]    │    │
│  └─────────────────────────────────────────────────────┘    │
│                                                              │
│                  [ PESQUISAR ]  [ LIMPAR ]                   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ⚠️ Mensagem de Erro (quando aplicável)               │   │
│  │ DOCUMENTO XXXXXXXXXXXXXXX NAO CADASTRADO             │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
└──────────────────────────────────────────────────────────────┘

F3=Sair  F12=Voltar
```

**Campos de Entrada:**

| Campo | Tipo | Tamanho | Obrigatório | Descrição |
|-------|------|---------|-------------|-----------|
| FONTE | Numérico | 3 | Condicional | Fonte do protocolo (001-999) |
| PROTSINI | Numérico | 7 | Condicional | Número do protocolo (0000001-9999999) |
| DAC | Numérico | 1 | Condicional | Dígito verificador (0-9) |
| ORGSIN | Numérico | 2 | Condicional | Órgão sinistro (01-99) |
| RMOSIN | Numérico | 2 | Condicional | Ramo sinistro (00-99) |
| NUMSIN | Numérico | 6 | Condicional | Número sinistro (000001-999999) |
| CODLIDER | Numérico | 3 | Condicional | Código líder (001-999) |
| SINLID | Numérico | 7 | Condicional | Sinistro líder (0000001-9999999) |

**Regras de Validação:**
- Pelo menos UM conjunto completo de campos deve ser preenchido
- Os 3 critérios são mutuamente exclusivos
- Campos numéricos não aceitam texto ou caracteres especiais

**Mensagens de Erro:**
- `PROTOCOLO XXXXXXX-X NAO ENCONTRADO`
- `SINISTRO XXXXXXX NAO ENCONTRADO`
- `LIDER XXXXXXX-XXXXXXX NAO ENCONTRADO`
- `PELO MENOS UM CRITERIO DEVE SER INFORMADO`

---

### 5.2 Tela SIHM020 - Detalhes e Autorização de Pagamento

**Identificação:**
- Map Group: (definido em CICS)
- Função: Exibição de dados + Entrada de autorização

**Layout ASCII:**
```
┌──────────────────────────────────────────────────────────────┐
│                    [ LOGO CAIXA SEGURADORA ]                 │
├──────────────────────────────────────────────────────────────┤
│  SINISTRO - AUTORIZAÇÃO DE PAGAMENTO                         │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ╔══════════════════════════════════════════════════════╗   │
│  ║  DADOS DO SINISTRO (Somente Leitura)                 ║   │
│  ╠══════════════════════════════════════════════════════╣   │
│  ║  Protocolo:  001/0123456-7                           ║   │
│  ║  Sinistro:   10/20/789012                            ║   │
│  ║  Apólice:    01/05/0045678                           ║   │
│  ║  Ramo:       AUTOMÓVEL                               ║   │
│  ║  Segurado:   JOÃO DA SILVA SANTOS                    ║   │
│  ║                                                       ║   │
│  ║  Saldo a Pagar:    R$  50.000,00                     ║   │
│  ║  Total Pago:       R$  10.000,00                     ║   │
│  ║  ──────────────────────────────────                  ║   │
│  ║  Saldo Pendente:   R$  40.000,00                     ║   │
│  ║                                                       ║   │
│  ║  Tipo Seguro: 5    Produto: 6814                     ║   │
│  ╚══════════════════════════════════════════════════════╝   │
│                                                              │
│  ╔══════════════════════════════════════════════════════╗   │
│  ║  AUTORIZAÇÃO DE PAGAMENTO                            ║   │
│  ╠══════════════════════════════════════════════════════╣   │
│  ║                                                       ║   │
│  ║  Tipo de Pagamento (1-5): [_]                        ║   │
│  ║                                                       ║   │
│  ║  Tipo de Apólice (1 ou 2): [_]                       ║   │
│  ║                                                       ║   │
│  ║  Valor Principal: R$ [_______,__]                    ║   │
│  ║                                                       ║   │
│  ║  Valor Correção:  R$ [_______,__] (opcional)         ║   │
│  ║                                                       ║   │
│  ║  Beneficiário: [________________________________]    ║   │
│  ║                (obrigatório p/ tipo seguro ≠ 0)      ║   │
│  ║                                                       ║   │
│  ║          [ AUTORIZAR ]        [ CANCELAR ]           ║   │
│  ║                                                       ║   │
│  ╚══════════════════════════════════════════════════════╝   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 📋 HISTÓRICO DE PAGAMENTOS                           │   │
│  ├──────────┬────────────┬──────────┬──────────────────┤   │
│  │  Data    │   Valor    │ Operador │  Status          │   │
│  ├──────────┼────────────┼──────────┼──────────────────┤   │
│  │ 23/10/25 │  5.000,00  │ USER001  │ ✅ Autorizado    │   │
│  │ 22/10/25 │  3.500,00  │ USER002  │ ✅ Autorizado    │   │
│  │ 20/10/25 │  1.500,00  │ USER001  │ ✅ Autorizado    │   │
│  ├──────────┴────────────┴──────────┴──────────────────┤   │
│  │         [ ◀ Anterior ]    [ Próxima ▶ ]             │   │
│  │                Página 1 de 5                         │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
└──────────────────────────────────────────────────────────────┘

F3=Sair  F7=Histórico  F8=Fases  F12=Voltar
```

**Campos de Saída (Somente Leitura):**

| Campo | Fonte | Descrição |
|-------|-------|-----------|
| Protocolo | FONTE/PROTSINI-DAC | Identificação protocolo |
| Sinistro | ORGSIN/RMOSIN/NUMSIN | Número do sinistro |
| Apólice | ORGAPO/RMOAPO/NUMAPOL | Número da apólice |
| Ramo | TGERAMO.NOMERAMO | Nome do ramo de seguro |
| Segurado | TAPOLICE.NOME | Nome do segurado |
| Saldo a Pagar | TMESTSIN.SDOPAG | Reserva esperada |
| Total Pago | TMESTSIN.TOTPAG | Soma pagamentos |
| Saldo Pendente | SDOPAG - TOTPAG | Calculado |
| Tipo Seguro | TMESTSIN.TPSEGU | Tipo seguro (0=opcional benef) |
| Produto | TMESTSIN.CODPRODU | Código produto |

**Campos de Entrada (Autorização):**

| Campo | Tipo | Obrigatório | Validação | Descrição |
|-------|------|-------------|-----------|-----------|
| Tipo Pagamento | Numérico | SIM | 1-5 | Tipo de pagamento (5 valores válidos) |
| Tipo Apólice | Numérico | SIM | 1-2 | Tipo de apólice (2 valores válidos) |
| Valor Principal | Decimal(15,2) | SIM | > 0 e ≤ pendente | Valor indenização |
| Valor Correção | Decimal(15,2) | NÃO | ≥ 0 | Correção monetária (default 0) |
| Beneficiário | String(255) | CONDICIONAL | Se TPSEGU≠0 | Nome do favorecido |

**Validações Específicas:**
- Valor Principal não pode exceder Saldo Pendente
- Beneficiário obrigatório SE tipo seguro (TPSEGU) for diferente de 0
- Tipo Pagamento deve ser exatamente 1, 2, 3, 4 ou 5
- Tipo Apólice deve ser exatamente 1 ou 2

---

## 6. Princípios e Regras Gerais

### 6.1 Princípios de Design do Sistema

1. **Integridade Transacional Absoluta**
   - Princípio "Tudo ou Nada" (ACID compliance)
   - Se QUALQUER etapa falhar → ROLLBACK completo
   - Nenhuma mudança parcial permitida

2. **Auditoria Completa e Rastreável**
   - Todo registro guarda ID do operador (EZEUSRID)
   - Data e hora exatas de cada transação
   - Possível reconstruir TODO o workflow

3. **Conversão Monetária Padronizada**
   - Todas as moedas convertidas para BTNF
   - Taxa com 8 casas decimais (precisão)
   - Resultado com 2 casas decimais (negócio)
   - Fórmula ÚNICA sem variações

4. **Validação Externa Obrigatória**
   - Produtos consórcio → CNOUA
   - Contratos EFP → SIPUA
   - Contratos HB → SIMDA
   - Falha na validação = ABORT transação

5. **Workflow Automatizado por Eventos**
   - Fases abertas/fechadas por configuração
   - Evento 1098 (pagamento) dispara mudanças
   - Gestão via tabela SI_REL_FASE_EVENTO

6. **Data Comercial (não clock do sistema)**
   - Data transação = TSISTEMA.DTMOVABE
   - Hora transação = hora do sistema
   - Permite controle de fechamento contábil

7. **Preservação de Regras Legadas**
   - 100% das regras mantidas sem mudanças
   - Operação sempre 1098
   - Tipo correção sempre '5'
   - Cálculos EXATOS do legado

### 6.2 Códigos Fixos e Constantes

| Código | Valor | Significado | Contexto |
|--------|-------|-------------|----------|
| **OPERACAO** | 1098 | Código operação pagamento | THISTSIN.OPERACAO |
| **TIPCRR** | '5' | Tipo correção padrão | THISTSIN.TIPCRR |
| **IDSISTEM** | 'SI' | Identificador sistema sinistros | TSISTEMA.IDSISTEM |
| **DATA_FECHA_ABERTA** | '9999-12-31' | Fase aberta (sentinel value) | SI_SINISTRO_FASE.DATA_FECHA_SIFA |
| **IND_ABERTURA** | '1' | Indicador abertura fase | SI_REL_FASE_EVENTO.IND_ALTERACAO_FASE |
| **IND_FECHAMENTO** | '2' | Indicador fechamento fase | SI_REL_FASE_EVENTO.IND_ALTERACAO_FASE |
| **SITCONTB_INICIAL** | '0' | Status contábil inicial | THISTSIN.SITCONTB |
| **SITUACAO_INICIAL** | '0' | Situação geral inicial | THISTSIN.SITUACAO |
| **COD_EVENTO_PAGTO** | 1098 | Código evento pagamento | SI_ACOMPANHA_SINI.COD_EVENTO |

### 6.3 Produtos Especiais (Consórcio)

| Código Produto | Nome | Roteamento |
|----------------|------|------------|
| **6814** | Consórcio Habitacional | CNOUA |
| **7701** | Consórcio Automóveis | CNOUA |
| **7709** | Consórcio Caminhões | CNOUA |

**Todos os outros produtos:** Roteamento baseado em EF_CONTR_SEG_HABIT
- Se NUM_CONTRATO > 0 → SIPUA
- Se NUM_CONTRATO = 0 ou não encontrado → SIMDA

---

## 7. Requisitos Não-Funcionais

### 7.1 Performance

| Métrica | Valor | Medição |
|---------|-------|---------|
| Busca sinistro | < 3 segundos | Qualquer critério |
| Autorização completa | < 90 segundos | Incluindo validação externa |
| Consulta histórico | < 500 ms | Para 1000+ registros |
| Consulta fases | < 200 ms | Sinistro típico (5-10 fases) |
| Lookup moeda | < 100 ms | Query TGEUNIMO |

### 7.2 Disponibilidade

| Atributo | Valor |
|----------|-------|
| Uptime esperado | 99.5% (horário comercial) |
| Janela manutenção | Fora horário comercial |
| RTO (Recovery Time) | < 4 horas |
| RPO (Recovery Point) | < 1 hora |

### 7.3 Segurança

| Requisito | Implementação |
|-----------|---------------|
| Autenticação | Integrada com AD corporativo |
| Autorização | Perfis de usuário (operador, supervisor) |
| Auditoria | ID operador em TODOS os registros |
| Criptografia | TLS 1.2+ para APIs externas |
| Proteção dados | Acesso DB via credentials gerenciadas |

### 7.4 Escalabilidade

| Dimensão | Capacidade |
|----------|------------|
| Usuários concorrentes | 50-100 operadores |
| Transações/hora | 500-1000 autorizações |
| Registros histórico | 10+ milhões acumulados |
| Crescimento anual | +15% volume transações |

### 7.5 Manutenibilidade

| Aspecto | Requisito |
|---------|-----------|
| Documentação código | XML docs em métodos públicos |
| Testes unitários | > 80% coverage Core layer |
| Testes integração | > 70% coverage API endpoints |
| Logging estruturado | Serilog com níveis (Info, Warn, Error) |
| Monitoramento | Application Insights / Prometheus |

---

## 8. Glossário de Termos

### 8.1 Termos de Negócio

| Termo | Definição |
|-------|-----------|
| **Sinistro** | Ocorrência coberta pela apólice que gera direito à indenização |
| **Protocolo** | Identificação única do sinistro (FONTE + PROTSINI + DAC) |
| **Apólice** | Contrato de seguro entre segurado e seguradora |
| **Segurado** | Pessoa física/jurídica que contratou o seguro |
| **Beneficiário/Favorecido** | Quem recebe o pagamento da indenização |
| **Indenização** | Valor pago pela seguradora para cobrir o sinistro |
| **Reserva (Saldo a Pagar)** | Valor estimado total do sinistro (SDOPAG) |
| **Saldo Pendente** | Diferença entre reserva e pagamentos já feitos |
| **Líder** | Seguradora líder em caso de cosseguro/resseguro |
| **Ramo** | Tipo de seguro (Automóvel, Habitacional, Vida, etc) |
| **Consórcio** | Modalidade de compra programada de bem |
| **EFP** | Empresa Gestora de Fundos de Pensão |
| **HB** | Habitacional (tipo de contrato) |

### 8.2 Termos Técnicos

| Termo | Definição |
|-------|-----------|
| **BTNF** | Bônus do Tesouro Nacional Fiscal (moeda padronizada) |
| **VLCRUZAD** | Taxa de conversão de moeda para BTNF |
| **OCORHIST** | Contador de ocorrências/histórico (sequence number) |
| **TIPSEG** | Tipo de seguro (parte da chave do sinistro) |
| **TPSEGU** | Tipo de seguro da apólice (determina obrigatoriedade beneficiário) |
| **TIPREG** | Tipo de registro da apólice ('1' ou '2') |
| **EZERT8** | Código de retorno de validação externa ('00000000' = sucesso) |
| **EZEUSRID** | ID do usuário no sistema (para auditoria) |
| **EZEROLLB** | Flag de rollback de transação |
| **SITCONTB** | Situação contábil do registro |
| **CRRMON** | Correção monetária |
| **PRIDIABT / CRRDIABT** | Valores diários em BTNF |

### 8.3 Acrônimos

| Acrônimo | Significado |
|----------|-------------|
| **SIWEA** | Sistema de Liberação de Pagamento de Sinistros |
| **CNOUA** | (Sistema de validação de consórcio - nome interno) |
| **SIPUA** | (Sistema de validação EFP - nome interno) |
| **SIMDA** | (Sistema de validação HB - nome interno) |
| **CICS** | Customer Information Control System (IBM mainframe) |
| **EZEE** | (Plataforma IBM VisualAge) |
| **DB2** | Database 2 (IBM relational database) |
| **SOAP** | Simple Object Access Protocol |
| **REST** | Representational State Transfer |
| **ACID** | Atomicity, Consistency, Isolation, Durability |

---

## 9. Referências e Documentos Relacionados

### 9.1 Documentação do Sistema Legado

1. **Código Fonte Original**
   - Arquivo: `#SIWEA-V116.esf`
   - Tamanho: 851.9 KB
   - Localização: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/`

2. **Stylesheet**
   - Arquivo: `Site.css`
   - Localização: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/`
   - Nota: Deve ser preservado 100% sem alterações

3. **Análise Completa**
   - Arquivo: `LEGACY_SIWEA_COMPLETE_ANALYSIS.md`
   - Localização: `docs/`
   - Linhas: 1,725
   - Conteúdo: Todas as 100+ regras de negócio detalhadas

4. **Resumo Executivo**
   - Arquivo: `ANALYSIS_SUMMARY.md`
   - Localização: `docs/`
   - Conteúdo: Visão executiva e checklist de implementação

### 9.2 Documentação da Migração

1. **Especificação de Features**
   - Arquivo: `spec.md`
   - Localização: `specs/001-visualage-dotnet-migration/`
   - Conteúdo: 6 user stories, 55 requisitos funcionais

2. **Plano de Implementação**
   - Arquivo: `plan.md`
   - Localização: `specs/001-visualage-dotnet-migration/`

3. **Modelo de Dados**
   - Arquivo: `data-model.md`
   - Localização: `specs/001-visualage-dotnet-migration/`
   - Conteúdo: 13 entidades com mapeamento completo

4. **Decisões de Arquitetura**
   - Arquivo: `research.md`
   - Localização: `specs/001-visualage-dotnet-migration/`
   - Conteúdo: Fase 0 - decisões técnicas

### 9.3 Documentação Técnica Específica

1. **Índice de Regras de Negócio**
   - Arquivo: `BUSINESS_RULES_INDEX.md`
   - Localização: `docs/`
   - Conteúdo: BR-001 a BR-099 indexadas por tier

2. **Roteamento de Validação de Produtos**
   - Arquivo: `product-validation-routing.md`
   - Localização: `docs/`
   - Conteúdo: Lógica CNOUA/SIPUA/SIMDA

3. **Workflow de Gestão de Fases**
   - Arquivo: `phase-management-workflow.md`
   - Localização: `docs/`
   - Conteúdo: Sistema de fases e eventos

4. **Notas de Performance**
   - Arquivo: `performance-notes.md`
   - Localização: `docs/`
   - Conteúdo: Otimizações e benchmarks

### 9.4 Como Navegar a Documentação

**Para entender o sistema rapidamente:**
1. Leia este documento (SISTEMA_LEGADO_VISAO_GERAL.md) primeiro
2. Depois vá para SISTEMA_LEGADO_PROCESSOS.md (fluxos)
3. Consulte SISTEMA_LEGADO_REGRAS_NEGOCIO.md quando precisar de detalhes

**Para implementar funcionalidades:**
1. Use spec.md para requisitos
2. Use SISTEMA_LEGADO_MODELO_DADOS.md para entidades
3. Use SISTEMA_LEGADO_ARQUITETURA.md para integrações
4. Valide contra LEGACY_SIWEA_COMPLETE_ANALYSIS.md

**Para resolver dúvidas:**
1. BUSINESS_RULES_INDEX.md tem todas as regras indexadas
2. product-validation-routing.md explica roteamento externo
3. phase-management-workflow.md explica fases

---

## 10. Conclusão

### 10.1 Status da Documentação

✅ **COMPLETO** - Visão Geral do Sistema Legado SIWEA

Este documento fornece:
- ✅ Identificação completa do sistema
- ✅ Histórico de 35+ anos
- ✅ Objetivo e escopo funcional
- ✅ Arquitetura visual (legado e futura)
- ✅ Descrição das 2 telas principais
- ✅ Princípios e regras gerais
- ✅ Requisitos não-funcionais
- ✅ Glossário completo
- ✅ Referências cruzadas

### 10.2 Próximos Documentos

Este é o **Documento 1 de 5** da série de documentação completa:

1. ✅ **SISTEMA_LEGADO_VISAO_GERAL.md** (este documento)
2. ⏭️ **SISTEMA_LEGADO_REGRAS_NEGOCIO.md** (próximo)
3. ⏭️ **SISTEMA_LEGADO_ARQUITETURA.md**
4. ⏭️ **SISTEMA_LEGADO_MODELO_DADOS.md**
5. ⏭️ **SISTEMA_LEGADO_PROCESSOS.md**

### 10.3 Como Usar Esta Documentação

**Para Gerentes de Projeto:**
- Use seções 1-3 para entender contexto e valor de negócio
- Use seção 7 para requisitos não-funcionais e SLAs
- Use seção 9 para navegar documentação relacionada

**Para Arquitetos:**
- Use seção 4 para arquitetura de alto nível
- Veja SISTEMA_LEGADO_ARQUITETURA.md para detalhes técnicos
- Consulte decision records em research.md

**Para Desenvolvedores:**
- Use seção 5 para entender telas
- Use seção 6 para princípios e constantes
- Consulte SISTEMA_LEGADO_REGRAS_NEGOCIO.md para lógica
- Consulte SISTEMA_LEGADO_MODELO_DADOS.md para entidades

**Para QA/Testers:**
- Use seção 3.2 (user stories) para cenários
- Use SISTEMA_LEGADO_PROCESSOS.md para fluxos
- Use BUSINESS_RULES_INDEX.md para casos de teste

---

**Documento Criado:** 2025-10-27
**Última Atualização:** 2025-10-27
**Versão:** 1.0
**Autor:** Análise Técnica - Projeto Migração POC Visual Age
**Aprovação:** Pendente

---

**FIM DO DOCUMENTO 1/5**
