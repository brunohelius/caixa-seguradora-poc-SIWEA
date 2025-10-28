# Cronograma Detalhado - Projeto de Migração SIWEA (3 Meses)

**Projeto:** Migração SIWEA para .NET 9 + React
**Duração:** 3 meses (13 semanas) = 65 dias úteis
**Data Início:** 2025-11-01 (exemplo)
**Data Fim:** 2026-01-31
**Metodologia:** Scrum (Sprints de 2 semanas)
**Versão:** 1.0

---

## Índice

1. [Visão Geral do Cronograma](#visão-geral-do-cronograma)
2. [Fases do Projeto](#fases-do-projeto)
3. [Sprint Plan (6 Sprints × 2 semanas)](#sprint-plan)
4. [Cronograma Gantt](#cronograma-gantt)
5. [Marcos (Milestones)](#marcos-milestones)
6. [Dependências Críticas](#dependências-críticas)
7. [Gestão de Riscos no Cronograma](#gestão-de-riscos-no-cronograma)

---

## 1. Visão Geral do Cronograma

### 1.1 Resumo Executivo

| Métrica | Valor |
|---------|-------|
| **Duração Total** | 13 semanas (3 meses) |
| **Sprints** | 6 sprints de 2 semanas |
| **Sprint 0 (Setup)** | Semana 1-2 (2 semanas) |
| **Desenvolvimento** | Semana 3-10 (8 semanas = 4 sprints) |
| **Testes e Estabilização** | Semana 11-12 (2 semanas = 1 sprint) |
| **Deploy e Cutover** | Semana 13 (1 semana) |
| **Equipe** | 4 pessoas em tempo integral + 4 parciais |
| **Horas Total** | 1.240 horas |

### 1.2 Distribuição de Tempo por Fase

```
Mês 1 (Novembro)
├─ Semana 1-2: Sprint 0 (Setup)          ████████████████████ 15%
└─ Semana 3-4: Sprint 1 (US1 + Infra)    ████████████████████ 20%

Mês 2 (Dezembro)
├─ Semana 5-6: Sprint 2 (US2 Part 1)     ████████████████████ 20%
├─ Semana 7-8: Sprint 3 (US2 Part 2)     ████████████████████ 20%
└─ Semana 9-10: Sprint 4 (US3,4,5,6)     ████████████████████ 15%

Mês 3 (Janeiro)
├─ Semana 11-12: Sprint 5 (Testes/Fix)   ███████████████ 8%
└─ Semana 13: Cutover e Go-Live          ██ 2%
```

---

## 2. Fases do Projeto

### Fase 0: Setup e Preparação (Semana 1-2)
**Duração:** 2 semanas
**Objetivo:** Preparar infraestrutura, ferramentas e ambiente

**Entregas:**
- ✅ Repositórios Git configurados (backend + frontend)
- ✅ Pipelines CI/CD funcionando (Azure DevOps)
- ✅ Ambientes Dev, QA, Staging provisionados
- ✅ Esqueleto Clean Architecture (.NET 9)
- ✅ Projeto React 19 inicial com Vite
- ✅ Conexão com banco de dados legado (read-only)
- ✅ Documentação técnica baseline
- ✅ Backlog refinado e priorizado

### Fase 1: MVP - Core Features (Semana 3-10)
**Duração:** 8 semanas (4 sprints)
**Objetivo:** Desenvolver funcionalidades core (US1, US2, US3, US4, US5, US6)

**Entregas:**
- ✅ US1: Busca de sinistros (3 modos)
- ✅ US2: Autorização de pagamento (pipeline 8 etapas)
- ✅ US3: Histórico de pagamentos
- ✅ US4: Validação produtos especiais (consórcio)
- ✅ US5: Gestão de fases e workflow
- ✅ US6: Dashboard de migração
- ✅ 100% regras de negócio implementadas
- ✅ Testes automatizados (unit + integration)

### Fase 2: Testes e Estabilização (Semana 11-12)
**Duração:** 2 semanas (1 sprint)
**Objetivo:** Testes completos, correção de bugs, validação

**Entregas:**
- ✅ Testes E2E completos (Playwright)
- ✅ Testes de performance (carga, stress)
- ✅ UAT (User Acceptance Testing) com operadores
- ✅ Correção de bugs P1 e P2
- ✅ Documentação de usuário final
- ✅ Treinamento de operadores
- ✅ Migração de dados validada

### Fase 3: Deploy e Cutover (Semana 13)
**Duração:** 1 semana
**Objetivo:** Migração final e go-live

**Entregas:**
- ✅ Migração final de dados produção
- ✅ Deploy em produção
- ✅ Cutover do sistema legado
- ✅ Monitoramento intensivo 24/7
- ✅ Suporte hipercare (primeira semana)

---

## 3. Sprint Plan (6 Sprints × 2 semanas)

### Sprint 0: Setup e Fundação (Semana 1-2)

**Data:** 01/11/2025 a 14/11/2025
**Objetivo:** Preparar ambiente e estrutura base

| Atividade | Responsável | Horas | Status |
|-----------|-------------|-------|--------|
| **Setup Infraestrutura** | | | |
| Provisionar Azure resources (App Service, SQL, etc) | DevOps | 12h | 🔵 |
| Configurar Azure DevOps (boards, repos, pipelines) | DevOps | 8h | 🔵 |
| Criar pipeline CI/CD backend | DevOps | 10h | 🔵 |
| Criar pipeline CI/CD frontend | DevOps | 8h | 🔵 |
| Configurar ambientes Dev/QA/Staging | DevOps | 12h | 🔵 |
| **Projeto Backend** | | | |
| Criar solução .NET 9 (3 layers: API, Core, Infra) | Tech Lead | 10h | 🔵 |
| Configurar EF Core + DbContext | Backend Dev 1 | 12h | 🔵 |
| Mapear entidades legadas (database-first) | Backend Dev 1 | 16h | 🔵 |
| Implementar Repository Pattern + Unit of Work | Backend Dev 2 | 12h | 🔵 |
| Configurar AutoMapper profiles | Backend Dev 2 | 8h | 🔵 |
| Configurar FluentValidation | Backend Dev 2 | 6h | 🔵 |
| Setup Serilog (structured logging) | Tech Lead | 4h | 🔵 |
| **Projeto Frontend** | | | |
| Criar projeto React 19 + Vite + TypeScript | Frontend Dev | 6h | 🔵 |
| Configurar React Router | Frontend Dev | 4h | 🔵 |
| Integrar Site.css (legado) | Frontend Dev | 6h | 🔵 |
| Setup Axios + React Query | Frontend Dev | 8h | 🔵 |
| Criar componentes base (Layout, Header, Footer) | Frontend Dev | 12h | 🔵 |
| **Documentação** | | | |
| Documentação de arquitetura | Tech Lead | 8h | 🔵 |
| Documentação de setup (README) | Tech Lead | 4h | 🔵 |
| **Planejamento** | | | |
| Refinar backlog com PO | PM + BA | 8h | 🔵 |
| Estimar User Stories | Toda equipe | 4h | 🔵 |
| Sprint Planning Sprint 1 | Toda equipe | 2h | 🔵 |
| **TOTAL SPRINT 0** | | **180h** | |

**Entregáveis Sprint 0:**
- ✅ Ambientes funcionando
- ✅ Pipelines CI/CD rodando
- ✅ Estrutura backend + frontend criada
- ✅ Conexão com banco legado funcionando
- ✅ Backlog refinado

---

### Sprint 1: US1 Busca de Sinistros (Semana 3-4)

**Data:** 15/11/2025 a 28/11/2025
**Objetivo:** Implementar busca completa de sinistros (3 modos)

**User Stories:**
- ✅ US1.1: Busca por protocolo
- ✅ US1.2: Busca por número sinistro
- ✅ US1.3: Busca por código líder
- ✅ US1.4: Exibição detalhes sinistro

| Atividade | Responsável | Horas | Status |
|-----------|-------------|-------|--------|
| **Backend Development** | | | |
| Criar ClaimService (search logic) | Backend Dev 1 | 16h | 🔵 |
| Implementar ClaimRepository (3 queries) | Backend Dev 1 | 12h | 🔵 |
| Implementar BR-001 a BR-009 (validações busca) | Backend Dev 2 | 14h | 🔵 |
| Criar ClaimsController (REST endpoints) | Backend Dev 1 | 10h | 🔵 |
| Implementar joins (TGERAMO, TAPOLICE) | Backend Dev 2 | 8h | 🔵 |
| Implementar formatação valores (BR-009) | Backend Dev 2 | 6h | 🔵 |
| **Frontend Development** | | | |
| Criar ClaimSearch.tsx (componente busca) | Frontend Dev | 16h | 🔵 |
| Implementar 3 formulários de busca | Frontend Dev | 14h | 🔵 |
| Validação em tempo real (Zod schemas) | Frontend Dev | 10h | 🔵 |
| Criar ClaimDetails.tsx (exibição) | Frontend Dev | 12h | 🔵 |
| Implementar loading states e erros | Frontend Dev | 8h | 🔵 |
| **Testes** | | | |
| Testes unitários backend (ClaimService) | Backend Dev 1 | 10h | 🔵 |
| Testes integração (repository + DB) | QA | 8h | 🔵 |
| Testes E2E (Playwright - busca) | QA | 10h | 🔵 |
| **Documentação** | | | |
| Swagger docs (API endpoints) | Backend Dev 1 | 3h | 🔵 |
| Storybook (componentes React) | Frontend Dev | 4h | 🔵 |
| **Cerimônias** | | | |
| Daily standups (10 dias × 0.5h) | Toda equipe | 5h | 🔵 |
| Sprint Review | Toda equipe | 2h | 🔵 |
| Sprint Retrospective | Toda equipe | 1,5h | 🔵 |
| Sprint Planning Sprint 2 | Toda equipe | 2h | 🔵 |
| **TOTAL SPRINT 1** | | **171,5h** | |

**Entregáveis Sprint 1:**
- ✅ US1 completa e funcionando
- ✅ 3 modos de busca implementados
- ✅ Testes automatizados (unit + E2E)
- ✅ Deploy em ambiente QA

**Definition of Done (DoD):**
- ✅ Código revisado (code review)
- ✅ Testes passando (100% dos testes)
- ✅ Cobertura de código > 80%
- ✅ Build passando no CI/CD
- ✅ Deploy automático em QA
- ✅ Documentação atualizada
- ✅ Demo para stakeholders

---

### Sprint 2: US2 Autorização Part 1 (Semana 5-6)

**Data:** 29/11/2025 a 12/12/2025
**Objetivo:** Implementar metade do pipeline de autorização (etapas 1-5)

**Escopo:**
- ✅ Formulário de entrada (5 campos)
- ✅ Validações (BR-010 a BR-022)
- ✅ Cálculo conversão BTNF (BR-023 a BR-033)
- ✅ Validação externa (CNOUA, SIPUA, SIMDA)
- ✅ Início da transação

| Atividade | Responsável | Horas | Status |
|-----------|-------------|-------|--------|
| **Backend Development** | | | |
| Criar PaymentAuthorizationService | Backend Dev 1 | 20h | 🔵 |
| Implementar validações BR-010 a BR-022 | Backend Dev 1 | 18h | 🔵 |
| Criar CurrencyConversionService | Backend Dev 2 | 14h | 🔵 |
| Implementar cálculos BTNF (BR-023 a BR-033) | Backend Dev 2 | 16h | 🔵 |
| Criar ExternalValidationService | Backend Dev 1 | 12h | 🔵 |
| Implementar CNOUAClient (HTTP + Polly) | Backend Dev 2 | 12h | 🔵 |
| Implementar SIPUAClient (SOAP + Polly) | Backend Dev 2 | 12h | 🔵 |
| Implementar SIMDAClient (SOAP + Polly) | Backend Dev 2 | 12h | 🔵 |
| Configurar Polly (retry + circuit breaker) | Tech Lead | 8h | 🔵 |
| **Frontend Development** | | | |
| Criar PaymentAuthorizationForm.tsx | Frontend Dev | 18h | 🔵 |
| Implementar validação em tempo real | Frontend Dev | 12h | 🔵 |
| Implementar máscaras de input (currency) | Frontend Dev | 8h | 🔵 |
| Criar componente de confirmação | Frontend Dev | 10h | 🔵 |
| **Testes** | | | |
| Testes unitários (validações) | Backend Dev 1 | 12h | 🔵 |
| Testes integração (CNOUA/SIPUA/SIMDA mocks) | QA | 12h | 🔵 |
| Testes E2E (formulário + validações) | QA | 10h | 🔵 |
| **Cerimônias** | | | |
| Dailies, Review, Retro, Planning | Toda equipe | 10,5h | 🔵 |
| **TOTAL SPRINT 2** | | **206,5h** | |

**Entregáveis Sprint 2:**
- ✅ Formulário de autorização funcionando
- ✅ Validações completas
- ✅ Cálculos BTNF precisos
- ✅ Integrações externas funcionando

---

### Sprint 3: US2 Autorização Part 2 (Semana 7-8)

**Data:** 13/12/2025 a 26/12/2025
**Objetivo:** Completar pipeline de autorização (etapas 6-10)

**Escopo:**
- ✅ Criar histórico (THISTSIN)
- ✅ Atualizar mestre (TMESTSIN)
- ✅ Criar acompanhamento (SI_ACOMPANHA_SINI)
- ✅ Atualizar fases (SI_SINISTRO_FASE)
- ✅ Transação ACID completa

| Atividade | Responsável | Horas | Status |
|-----------|-------------|-------|--------|
| **Backend Development** | | | |
| Implementar criação THISTSIN (BR-034 a BR-042) | Backend Dev 1 | 16h | 🔵 |
| Implementar atualização TMESTSIN | Backend Dev 1 | 10h | 🔵 |
| Criar PhaseManagementService | Backend Dev 2 | 14h | 🔵 |
| Implementar SI_REL_FASE_EVENTO lookup | Backend Dev 2 | 10h | 🔵 |
| Implementar abertura/fechamento fases | Backend Dev 2 | 12h | 🔵 |
| Implementar Unit of Work + Transactions | Tech Lead | 12h | 🔵 |
| Implementar rollback em falhas | Backend Dev 1 | 10h | 🔵 |
| Criar endpoint POST /api/claims/authorize-payment | Backend Dev 1 | 8h | 🔵 |
| **Frontend Development** | | | |
| Implementar loading states (spinner) | Frontend Dev | 8h | 🔵 |
| Implementar mensagens sucesso/erro | Frontend Dev | 8h | 🔵 |
| Atualizar dados após autorização | Frontend Dev | 10h | 🔵 |
| Implementar confirmação modal | Frontend Dev | 8h | 🔵 |
| **Testes** | | | |
| Testes unitários (transações) | Backend Dev 1 | 14h | 🔵 |
| Testes integração (pipeline completo) | QA | 16h | 🔵 |
| Testes E2E (autorização end-to-end) | QA | 14h | 🔵 |
| Testes de rollback (cenários de falha) | QA | 10h | 🔵 |
| **Performance Testing** | | | |
| Testes de carga (autorização < 90s) | QA | 8h | 🔵 |
| Otimização de queries | Backend Dev 2 | 8h | 🔵 |
| **Cerimônias** | | | |
| Dailies, Review, Retro, Planning | Toda equipe | 10,5h | 🔵 |
| **TOTAL SPRINT 3** | | **196,5h** | |

**Entregáveis Sprint 3:**
- ✅ US2 completa (autorização end-to-end)
- ✅ Transação ACID funcionando
- ✅ Rollback automático em falhas
- ✅ Performance < 90s validada

---

### Sprint 4: US3, US4, US5, US6 (Semana 9-10)

**Data:** 27/12/2025 a 09/01/2026
**Objetivo:** Implementar funcionalidades complementares

**Escopo:**
- ✅ US3: Histórico de pagamentos
- ✅ US4: Produtos especiais (já parcialmente feito em Sprint 2-3)
- ✅ US5: Gestão de fases
- ✅ US6: Dashboard de migração

| Atividade | Responsável | Horas | Status |
|-----------|-------------|-------|--------|
| **US3: Histórico** | | | |
| Criar endpoint GET /api/claims/{id}/history | Backend Dev 1 | 8h | 🔵 |
| Implementar paginação (20/página) | Backend Dev 1 | 6h | 🔵 |
| Criar PaymentHistory.tsx | Frontend Dev | 12h | 🔵 |
| Implementar paginação frontend | Frontend Dev | 6h | 🔵 |
| Testes (unit + E2E) | QA | 8h | 🔵 |
| **US5: Gestão de Fases** | | | |
| Criar endpoint GET /api/claims/{id}/phases | Backend Dev 2 | 6h | 🔵 |
| Criar PhaseTimeline.tsx | Frontend Dev | 14h | 🔵 |
| Implementar visualização timeline | Frontend Dev | 10h | 🔵 |
| Testes | QA | 6h | 🔵 |
| **US6: Dashboard Migração** | | | |
| Criar MigrationStatusService | Backend Dev 2 | 10h | 🔵 |
| Criar endpoints dashboard | Backend Dev 2 | 8h | 🔵 |
| Criar MigrationDashboard.tsx | Frontend Dev | 16h | 🔵 |
| Implementar gráficos (Recharts) | Frontend Dev | 12h | 🔵 |
| Implementar auto-refresh (30s) | Frontend Dev | 6h | 🔵 |
| Testes | QA | 8h | 🔵 |
| **Refinamentos US4** | | | |
| Ajustes validação consórcio | Backend Dev 1 | 6h | 🔵 |
| Testes específicos produtos | QA | 6h | 🔵 |
| **Cerimônias** | | | |
| Dailies, Review, Retro, Planning | Toda equipe | 10,5h | 🔵 |
| **TOTAL SPRINT 4** | | **158,5h** | |

**Entregáveis Sprint 4:**
- ✅ US3, US5, US6 completas
- ✅ Dashboard de migração funcionando
- ✅ Todas as 6 User Stories implementadas

---

### Sprint 5: Testes e Estabilização (Semana 11-12)

**Data:** 10/01/2026 a 23/01/2026
**Objetivo:** Testes completos, correção de bugs, preparação para produção

**Escopo:**
- ✅ Testes E2E completos
- ✅ Testes de performance
- ✅ UAT com usuários finais
- ✅ Correção de bugs
- ✅ Migração de dados
- ✅ Documentação final

| Atividade | Responsável | Horas | Status |
|-----------|-------------|-------|--------|
| **Testes E2E Completos** | | | |
| Cenários críticos (20 cenários) | QA | 30h | 🔵 |
| Testes cross-browser (Chrome, Firefox, Edge) | QA | 12h | 🔵 |
| Testes mobile (responsividade) | QA | 8h | 🔵 |
| **Testes de Performance** | | | |
| Testes de carga (100 usuários concorrentes) | QA | 12h | 🔵 |
| Testes de stress (pico 200 usuários) | QA | 10h | 🔵 |
| Testes de performance DB (queries) | Backend Dev 2 | 8h | 🔵 |
| Otimizações baseadas em resultados | Backend Dev 1+2 | 16h | 🔵 |
| **UAT (User Acceptance Testing)** | | | |
| Preparação ambiente UAT | DevOps | 6h | 🔵 |
| Sessões UAT com operadores (3 sessões) | BA + QA | 18h | 🔵 |
| Documentação de feedback | BA | 6h | 🔵 |
| **Correção de Bugs** | | | |
| Bugs P1 (críticos) | Backend + Frontend | 30h | 🔵 |
| Bugs P2 (altos) | Backend + Frontend | 20h | 🔵 |
| Bugs P3 (médios) | Backend + Frontend | 10h | 🔵 |
| **Migração de Dados** | | | |
| Scripts migração incrementais | Backend Dev 1 | 16h | 🔵 |
| Testes de migração (ambiente staging) | QA | 12h | 🔵 |
| Validação integridade dados | Backend Dev 2 | 10h | 🔵 |
| **Documentação** | | | |
| Manual do usuário | BA + Frontend Dev | 12h | 🔵 |
| Guia de operação (ops) | DevOps + Backend Dev | 8h | 🔵 |
| Runbook de deploy | DevOps | 6h | 🔵 |
| **Treinamento** | | | |
| Material de treinamento | BA | 8h | 🔵 |
| Sessões de treinamento (2 turmas) | BA + Tech Lead | 16h | 🔵 |
| **Cerimônias** | | | |
| Dailies, Review, Retro | Toda equipe | 10,5h | 🔵 |
| **TOTAL SPRINT 5** | | **274,5h** | |

**Entregáveis Sprint 5:**
- ✅ Sistema testado e estável
- ✅ Bugs críticos corrigidos
- ✅ Migração de dados validada
- ✅ Usuários treinados
- ✅ Documentação completa
- ✅ **GO/NO-GO Decision Point**

---

### Semana 13: Cutover e Go-Live

**Data:** 24/01/2026 a 31/01/2026
**Objetivo:** Migração final e lançamento em produção

**Plano de Cutover (fim de semana 24-25/01):**

**Sexta-feira 24/01 (18h):**
- Freeze do sistema legado (último dia operacional)
- Backup completo banco legado
- Comunicação aos usuários (sistema indisponível sábado/domingo)

**Sábado 25/01 (08h-20h):**
- Execução migração final de dados
- Validação integridade (checksums, contadores)
- Deploy produção (.NET 9 + React)
- Smoke tests em produção
- Configuração monitoramento (Application Insights)

**Domingo 26/01 (08h-18h):**
- Testes de homologação produção
- Validação acessos usuários
- Testes de integração externa (CNOUA, SIPUA, SIMDA)
- Dry-run de operações críticas

**Segunda-feira 27/01 (07h):**
- **GO-LIVE** 🚀
- Sistema novo disponível para operadores
- Equipe completa em standby
- Monitoramento intensivo 24/7

**Segunda a Sexta (27-31/01):**
- Suporte hipercare (toda equipe disponível)
- Resolução imediata de problemas
- Coleta de feedback
- Ajustes rápidos se necessário

| Atividade | Responsável | Horas | Status |
|-----------|-------------|-------|--------|
| **Preparação** | | | |
| Checklist pré-cutover | PM | 4h | 🔵 |
| Comunicação stakeholders | PM | 4h | 🔵 |
| Preparação rollback plan | DevOps + Tech Lead | 6h | 🔵 |
| **Execução Cutover** | | | |
| Freeze sistema legado | DevOps | 2h | 🔵 |
| Backup final | DevOps | 3h | 🔵 |
| Migração dados produção | Backend Dev 1+2 | 12h | 🔵 |
| Validação dados | QA + Backend Dev | 8h | 🔵 |
| Deploy produção | DevOps | 4h | 🔵 |
| Smoke tests | QA | 4h | 🔵 |
| Configuração monitoramento | DevOps | 3h | 🔵 |
| **Testes Homologação Produção** | | | |
| Testes funcionais | QA | 12h | 🔵 |
| Testes integrações | Backend Dev | 6h | 🔵 |
| Validação acessos | QA | 4h | 🔵 |
| **Go-Live e Suporte** | | | |
| Go-live (presencial) | Toda equipe | 8h | 🔵 |
| Suporte dia 1 (Mon) | Toda equipe | 10h | 🔵 |
| Suporte dia 2-5 (Tue-Fri) | Toda equipe | 32h | 🔵 |
| **Encerramento** | | | |
| Retrospectiva final | Toda equipe | 2h | 🔵 |
| Documentação lições aprendidas | PM | 6h | 🔵 |
| Transferência para suporte N2 | Tech Lead | 4h | 🔵 |
| **TOTAL SEMANA 13** | | **134h** | |

**Entregáveis Semana 13:**
- ✅ Sistema em produção e funcionando
- ✅ Dados migrados com 100% integridade
- ✅ Operadores usando sistema novo
- ✅ Suporte hipercare ativo
- ✅ **PROJETO CONCLUÍDO** 🎉

---

## 4. Cronograma Gantt

```
LEGENDA:
████ = Atividade em execução
░░░░ = Atividade concluída
┊┊┊┊ = Dependência crítica

                 Nov 2025          Dec 2025          Jan 2026
SEMANA:    1  2  3  4  5  6  7  8  9  10 11 12 13

FASE 0: Setup
├─ Infra      ████████
├─ Backend    ████████
├─ Frontend   ████████
└─ Docs       ████████

SPRINT 1: US1
├─ Backend          ████████
├─ Frontend         ████████
└─ Testes           ┊┊┊┊████

SPRINT 2: US2 P1
├─ Backend                ████████
├─ Frontend               ████████
└─ Testes                 ┊┊┊┊████

SPRINT 3: US2 P2
├─ Backend                      ████████
├─ Frontend                     ████████
└─ Testes                       ┊┊┊┊████

SPRINT 4: US3-6
├─ Backend                            ████████
├─ Frontend                           ████████
└─ Testes                             ┊┊┊┊████

SPRINT 5: Testes
├─ E2E Tests                                ████████
├─ Performance                              ████████
├─ UAT                                      ┊┊┊┊████
├─ Bug Fixes                                ████████
└─ Migration                                ████████

CUTOVER
├─ Preparação                                     ████
├─ Migração                                       ██
└─ Go-Live                                        ██
```

---

## 5. Marcos (Milestones)

| # | Marco | Data | Critério de Aceitação |
|---|-------|------|----------------------|
| M1 | **Kickoff do Projeto** | 01/11/2025 | Equipe formada, backlog priorizado |
| M2 | **Infraestrutura Pronta** | 14/11/2025 | Ambientes funcionando, pipelines CI/CD ativos |
| M3 | **US1 Completa (Busca)** | 28/11/2025 | 3 modos de busca funcionando, testes passando |
| M4 | **US2 Part 1 Completa** | 12/12/2025 | Validações e integrações externas funcionando |
| M5 | **US2 Completa (Autorização)** | 26/12/2025 | Pipeline 8 etapas end-to-end, transação ACID |
| M6 | **MVP Completo (US1-6)** | 09/01/2026 | Todas User Stories implementadas e testadas |
| M7 | **UAT Aprovado** | 23/01/2026 | Usuários aprovam sistema, bugs críticos resolvidos |
| M8 | **GO/NO-GO Decision** | 23/01/2026 | Decisão formal para prosseguir com cutover |
| M9 | **Go-Live** | 27/01/2026 | Sistema em produção, operadores trabalhando |
| M10 | **Encerramento Projeto** | 31/01/2026 | Hipercare concluído, projeto oficialmente entregue |

---

## 6. Dependências Críticas

### 6.1 Dependências Internas (Sequenciais)

```
Sprint 0 (Setup)
    ↓ BLOQUEIA
Sprint 1 (US1) ← Precisa de infraestrutura pronta
    ↓ BLOQUEIA
Sprint 2 (US2 P1) ← Precisa de US1 (busca) para testar autorização
    ↓ BLOQUEIA
Sprint 3 (US2 P2) ← Precisa de validações e cálculos (Sprint 2)
    ↓ BLOQUEIA
Sprint 5 (Testes) ← Precisa de todas US implementadas
    ↓ BLOQUEIA
Cutover ← Precisa de UAT aprovado
```

### 6.2 Dependências Externas

| Dependência | Fornecedor | Prazo Crítico | Risco |
|-------------|------------|---------------|-------|
| **Ambientes Azure** | Microsoft | Semana 1 | 🟢 Baixo (self-service) |
| **Acesso DB Legado** | Equipe Mainframe | Semana 1 | 🟡 Médio (coordenação) |
| **Credenciais CNOUA** | Time CNOUA | Semana 5 | 🟡 Médio (homologação) |
| **Credenciais SIPUA** | Time SIPUA | Semana 5 | 🟡 Médio (homologação) |
| **Credenciais SIMDA** | Time SIMDA | Semana 5 | 🟡 Médio (homologação) |
| **Janela de Cutover** | Operações | Semana 13 | 🟢 Baixo (agendado) |
| **Aprovação UAT** | Usuários Finais | Semana 11-12 | 🔴 Alto (critical path) |

**Ações de Mitigação:**
- 🟡 Médio: Solicitar credenciais e acessos na Semana 1 (antecipação)
- 🔴 Alto: Envolver usuários finais desde Sprint 1 (demos quinzenais)

---

## 7. Gestão de Riscos no Cronograma

### 7.1 Riscos de Atraso

| Risco | Probabilidade | Impacto | Semanas Risco | Mitigação |
|-------|---------------|---------|---------------|-----------|
| **Integrações externas indisponíveis** | 40% | Alto | S5-S6 | Mocks realistas, testes com providers antecipados |
| **Bugs críticos em UAT** | 30% | Alto | S11-S12 | Testes contínuos desde Sprint 1, QA embarcado |
| **Migração de dados falha** | 20% | Crítico | S12-S13 | Dry-runs desde Sprint 4, validação incremental |
| **Aprovação UAT atrasada** | 25% | Alto | S12 | Buffer de 1 semana, envolvimento early |
| **Equipe incompleta** | 15% | Médio | S1-S13 | Contratar com antecedência, backup skill matrix |
| **Mudança de escopo** | 30% | Médio | S1-S10 | Scope freeze após Sprint 0, change control |

### 7.2 Buffer Management

**Buffers incluídos no cronograma:**

1. **Sprint Buffer:** 3% de cada sprint para imprevistos
2. **Integration Buffer:** Sprint 5 inteiro dedicado a testes e ajustes
3. **Cutover Buffer:** 1 semana completa (S13) para cutover + hipercare
4. **Weekend Buffer:** Cutover em fim de semana (rollback possível)

**Total Buffer: ~2 semanas** (15% do cronograma total)

### 7.3 Plano de Contingência

**Se atrasarmos 1 semana:**
- ✅ Usar buffer de testes (reduzir Sprint 5 de 2 para 1,5 semanas)
- ✅ Aumentar equipe temporariamente (contractors)
- ✅ Reduzir escopo US6 (dashboard) para post-MVP

**Se atrasarmos 2+ semanas:**
- ⚠️ Negociar nova data de cutover com stakeholders
- ⚠️ Avaliar MVP reduzido (apenas US1 + US2)
- ⚠️ Considerar abordagem faseada (cutover parcial)

---

## Anexo A: Calendário Detalhado

### Novembro 2025

| Semana | Datas | Sprint | Atividades Principais |
|--------|-------|--------|----------------------|
| S1 | 01-07 Nov | Sprint 0 (Part 1) | Setup Azure, projetos base |
| S2 | 08-14 Nov | Sprint 0 (Part 2) | EF Core, React, CI/CD |
| S3 | 15-21 Nov | Sprint 1 (Part 1) | Backend US1 (busca) |
| S4 | 22-28 Nov | Sprint 1 (Part 2) | Frontend US1, testes |

### Dezembro 2025

| Semana | Datas | Sprint | Atividades Principais |
|--------|-------|--------|----------------------|
| S5 | 29 Nov - 05 Dez | Sprint 2 (Part 1) | Validações US2, BTNF |
| S6 | 06-12 Dez | Sprint 2 (Part 2) | Integrações externas |
| S7 | 13-19 Dez | Sprint 3 (Part 1) | Transações ACID |
| S8 | 20-26 Dez | Sprint 3 (Part 2) | Rollback, testes |

### Janeiro 2026

| Semana | Datas | Sprint | Atividades Principais |
|--------|-------|--------|----------------------|
| S9 | 27 Dez - 02 Jan | Sprint 4 (Part 1) | US3, US5, US6 |
| S10 | 03-09 Jan | Sprint 4 (Part 2) | Dashboard, refinamentos |
| S11 | 10-16 Jan | Sprint 5 (Part 1) | E2E tests, performance |
| S12 | 17-23 Jan | Sprint 5 (Part 2) | UAT, bug fixes, docs |
| S13 | 24-31 Jan | Cutover | Migração, go-live, hipercare |

---

## Anexo B: Métricas de Acompanhamento

**Acompanhar semanalmente:**

| Métrica | Meta | Alerta | Ação |
|---------|------|--------|------|
| **Velocity** | 12-15 PF/sprint | < 10 PF | Identificar impedimentos |
| **Burn Rate** | Linear | > 20% desvio | Realocar recursos |
| **Bugs Abertos** | < 10 | > 20 | Bug bash session |
| **Cobertura Testes** | > 80% | < 70% | Focar em testes |
| **Build Success** | > 95% | < 90% | Revisar CI/CD |
| **Performance Busca** | < 3s | > 3s | Otimizar queries |
| **Performance Autorização** | < 90s | > 90s | Otimizar integrações |

---

**FIM - CRONOGRAMA 3 MESES**

**Status:** ✅ APROVADO PARA EXECUÇÃO
**Próximos Passos:** Iniciar Sprint 0 em 01/11/2025
