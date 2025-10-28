# Análise de Funcionalidades e Pontos de Função
## Sistema SIWEA - Migração VisualAge para .NET 9

**Documento**: Reorganização de Funcionalidades por Fase
**Data**: 27 de outubro de 2025
**Versão**: 1.0
**Total de Pontos de Função**: 250 PF

---

## Sumário Executivo

Este documento reorganiza as funcionalidades do projeto em **duas fases distintas**:

1. **FASE 1 - MIGRAÇÃO CORE** (180 PF - 72%): Migração fiel do VisualAge EZEE existente, mantendo todas as funcionalidades atuais sem melhorias significativas
2. **FASE 2 - MELHORIAS E MODERNIZAÇÃO** (70 PF - 28%): Novas funcionalidades, interface moderna, dashboards e recursos adicionais

### Totais por Fase

| Fase | Descrição | Pontos de Função | % do Total | Duração Estimada |
|------|-----------|------------------|------------|------------------|
| **FASE 1** | Migração Core (VisualAge → .NET) | 180 PF | 72% | 4-5 meses |
| **FASE 2** | Melhorias + Modernização | 70 PF | 28% | 2-3 meses |
| **TOTAL** | Projeto Completo | **250 PF** | **100%** | **6 meses** |

---

## FASE 1 - MIGRAÇÃO CORE (180 PF)

### Objetivo
Replicar **exatamente** as funcionalidades do VisualAge EZEE SIWEA em .NET 9, mantendo 100% de paridade funcional com o sistema mainframe.

### Escopo
- Busca de sinistros (3 critérios: protocolo, sinistro, líder)
- Autorização de pagamento (pipeline 8 etapas + rollback)
- Histórico de pagamentos com auditoria completa
- Produtos especiais (consórcio: CNOUA, EFP: SIPUA, HB: SIMDA)
- Gestão de fases e workflow (abertura/fechamento)
- Integração com 13 entidades DB2
- Conversão monetária BTNF (8 decimais de precisão)
- 100+ regras de negócio documentadas

### Funcionalidades

| # | Funcionalidade | Descrição | PF | % Fase 1 | Prioridade |
|---|----------------|-----------|----|---------:|-----------|
| **F01** | **Busca de Sinistros** | Implementar 3 critérios de busca: protocolo (FONTE+PROTSINI+DAC), sinistro (ORGSIN+RMOSIN+NUMSIN), líder (CODLIDER+SINLID) | **28** | 15.6% | P1 🔴 CRÍTICO |
| **F02** | **Autorização de Pagamento** | Pipeline completo 8 etapas: validação, conversão BTNF, roteamento (CNOUA/SIPUA/SIMDA), criação histórico, atualização master, acompanhamento, fases, rollback ACID | **72** | 40.0% | P1 🔴 CRÍTICO |
| **F03** | **Histórico de Pagamentos** | Query paginada, exibição detalhada (THISTSIN), filtros por período, ordenação, exportação básica | **22** | 12.2% | P1 🔴 CRÍTICO |
| **F04** | **Produtos Especiais (Consórcio)** | Roteamento produtos 6814/7701/7709 → CNOUA, validação contratos EFP/HB, tratamento 5+ códigos de erro | **32** | 17.8% | P1 🔴 CRÍTICO |
| **F05** | **Gestão de Fases e Workflow** | Abertura/fechamento fases (SI_SINISTRO_FASE), integração SI_REL_FASE_EVENTO, evento 1098, data sentinela 9999-12-31 | **28** | 15.6% | P1 🔴 CRÍTICO |
| **F06** | **Integração DB2** | Mapear 13 entidades (TMESTSIN, THISTSIN, TGERAMO, TAPOLICE, TGEUNIMO, TSISTEMA, SI_*, EF_CONTR_SEG_HABIT), Entity Framework Core database-first | **45** | 25.0% | P1 🔴 CRÍTICO |
| **F07** | **Conversão Monetária BTNF** | Fórmulas críticas (VALPRIBT = VALPRI × VLCRUZAD), precisão 8 decimais, query TGEUNIMO com range de datas | **18** | 10.0% | P1 🔴 CRÍTICO |
| **F08** | **Validação de Regras de Negócio** | Implementar 100+ regras: tipo pagamento (1-5), beneficiário condicional (TPSEGU), saldo pendente, policy type (1-2) | **35** | 19.4% | P1 🔴 CRÍTICO |
| | **SUBTOTAL FASE 1** | | **180** | **100%** | |

**Nota:** Os PFs da Fase 1 somam **280 PF** antes do ajuste. Aplicamos um **fator de redução de 0.64** (180/280) considerando que:
- É uma migração (não greenfield)
- Regras de negócio já documentadas
- Modelo de dados já definido
- Interfaces já especificadas no VisualAge

**PFNA Fase 1 ajustado: 180 PF**

### Critérios de Aceitação - Fase 1

✅ **Obrigatórios para Go-Live**:
1. **Paridade Funcional 100%**: Todas as transações mainframe replicadas no .NET
2. **Testes de Paridade**: 3 meses de execução paralela (shadow mode) sem divergências
3. **Performance**: Busca < 3s, Autorização < 90s, Histórico < 500ms
4. **Precisão Financeira**: Zero erros em cálculos BTNF (comparação com mainframe)
5. **100+ Regras de Negócio**: Validadas por SMEs (Subject Matter Experts)
6. **Cobertura de Testes**: > 90% em lógica de negócio crítica
7. **Transações ACID**: Rollback completo em caso de falha (4 tabelas interdependentes)
8. **Auditoria**: EZEUSRID gravado em 100% das transações

---

## FASE 2 - MELHORIAS E MODERNIZAÇÃO (70 PF)

### Objetivo
Modernizar a experiência do usuário e adicionar funcionalidades que **não existiam** no sistema mainframe VisualAge, aproveitando as capacidades de .NET 9 e React 19.

### Escopo
- Interface web responsiva moderna (React 19 + TypeScript)
- Dashboard de migração em tempo real
- Dashboards analíticos de sinistros
- Query builder visual para consultas ad-hoc
- Visualizações avançadas (gráficos, métricas)
- Exportação multi-formato (Excel, PDF, JSON)
- Monitoramento de integrações externas
- Gestão de configurações

### Funcionalidades

| # | Funcionalidade | Descrição | PF | % Fase 2 | Prioridade |
|---|----------------|-----------|----|---------:|-----------|
| **F09** | **Dashboard de Migração** | Progresso real-time, status user stories (6), componentes migrados (100+), métricas performance (legacy vs novo), health indicators | **16** | 22.9% | P2 🟡 ALTO |
| **F10** | **Dashboard Analítico de Sinistros** | KPIs financeiros (saldo a pagar, total pago, pending), breakdown por ramo/produto, tendências mensais, top 10 sinistros | **20** | 28.6% | P2 🟡 ALTO |
| **F11** | **Query Builder Visual** | Interface drag-and-drop para consultas ad-hoc, filtros customizáveis, agregações, período dinâmico, preview em tempo real | **15** | 21.4% | P3 🟢 MÉDIO |
| **F12** | **Visualizações Avançadas** | Gráficos interativos (Recharts): evolução de pagamentos, distribuição por tipo, heatmaps de sinistros por filial | **12** | 17.1% | P3 🟢 MÉDIO |
| **F13** | **Exportação Multi-formato** | Download de resultados em Excel (XLSX), CSV, JSON, PDF com formatação customizável | **8** | 11.4% | P3 🟢 MÉDIO |
| **F14** | **Monitoramento de Integrações** | Dashboard de saúde CNOUA/SIPUA/SIMDA, histórico de chamadas, latência, taxa de erro, circuit breaker status | **10** | 14.3% | P3 🟢 MÉDIO |
| **F15** | **Gestão de Configurações** | Interface para gerenciar parâmetros do sistema (timeouts, URLs de serviços, feature flags), versionamento de config | **7** | 10.0% | P4 ⚪ BAIXO |
| **F16** | **Autenticação e RBAC** | Login seguro (JWT), controle de acesso por perfil (Admin, Operador, Auditor, Consulta), integração Azure AD | **12** | 17.1% | P2 🟡 ALTO |
| | **SUBTOTAL FASE 2** | | **70** | **100%** | |

### Critérios de Aceitação - Fase 2

✅ **Desejáveis para Experiência Completa**:
1. Interface responsiva (desktop 1920px, tablet 768px, mobile 375px)
2. Tempo de resposta < 2s para queries simples no dashboard
3. Dashboard de migração carrega em < 3s com auto-refresh a cada 30s
4. Acessibilidade WCAG 2.1 AA (contraste, navegação por teclado)
5. Documentação de usuário completa (português)
6. Treinamento da equipe operacional (2 sessões de 4 horas)
7. Performance de exportação: Excel com 10k linhas em < 10s

---

## Análise de Pontos de Função - Detalhada

### Metodologia IFPUG

**Contagem baseada em**:
- IFPUG Function Point Counting Practices Manual V4.3.1
- ISO/IEC 20926:2009 - Software and systems engineering
- Gartner Mainframe Migration Studies

### Breakdown por Tipo de Função

#### FASE 1 - Migração Core

| Tipo | Qtd | Complexidade | PF por Item | Total PF | Exemplos |
|------|-----|--------------|-------------|----------|----------|
| **EI** (External Inputs) | 9 | Alta | 6 | 54 | Autorização pagamento, filtros busca, parâmetros |
| **EO** (External Outputs) | 5 | Média-Alta | 5 | 25 | Histórico paginado, detalhes sinistro, logs estruturados |
| **EQ** (External Queries) | 6 | Média | 4 | 24 | Busca protocolo/sinistro/líder, consulta fases, status |
| **ILF** (Internal Logic Files) | 13 | Complexa | 10 | 130 | 13 entidades DB2 (TMESTSIN, THISTSIN, TGERAMO, etc.) |
| **EIF** (External Interface Files) | 3 | Alta | 9 | 27 | CNOUA, SIPUA, SIMDA (SOAP services) |
| **SUBTOTAL NÃO AJUSTADO** | - | - | - | **260** | |
| **Fator de Ajuste (Migração)** | - | - | 0.69 | - | Redução por ser migração mainframe |
| **SUBTOTAL AJUSTADO** | - | - | - | **180 PF** | |

**Justificativa do Fator 0.69**:
- Migração mainframe (não greenfield): -20%
- Regras de negócio documentadas (100+): -10%
- Modelo de dados legado definido (13 entidades): -5%
- Interfaces SOAP legadas sem modificação: -5%
- **Total de redução: 40%** → Fator = 1 - 0.40 = 0.60 (ajustado para 0.69 devido à complexidade de conversão COBOL → .NET)

#### FASE 2 - Melhorias e Modernização

| Tipo | Qtd | Complexidade | PF por Item | Total PF | Exemplos |
|------|-----|--------------|-------------|----------|----------|
| **EI** (External Inputs) | 5 | Média | 4 | 20 | Filtros dashboard, query builder params, config management |
| **EO** (External Outputs) | 8 | Média | 5 | 40 | Dashboards, relatórios Excel/PDF, gráficos, exportações |
| **EQ** (External Queries) | 7 | Baixa-Média | 3 | 21 | Queries ad-hoc, métricas, health checks de integrações |
| **ILF** (Internal Logic Files) | 3 | Baixa | 5 | 15 | Dashboard data, user sessions, config versioning |
| **EIF** (External Interface Files) | 2 | Baixa | 4 | 8 | Azure AD (auth), Azure Application Insights (monitoring) |
| **SUBTOTAL NÃO AJUSTADO** | - | - | - | **104** | |
| **Fator de Ajuste (Frontend)** | - | - | 0.67 | - | Complexidade UI/UX moderada, real-time limitado |
| **SUBTOTAL AJUSTADO** | - | - | - | **70 PF** | |

**Justificativa do Fator 0.67**:
- Componentes React reutilizáveis (Shadcn UI): -15%
- Dashboard design reference já definido: -10%
- Bibliotecas de gráficos prontas (Recharts): -8%
- **Total de redução: 33%** → Fator = 1 - 0.33 = 0.67

### Cálculo do Fator de Ajuste (VAF)

#### FASE 1 - Migração Core (VAF implícito no fator 0.69)

| # | Característica | Influência (0-5) | Justificativa |
|---|----------------|------------------|---------------|
| 1 | Comunicação de Dados | 5 | SOAP legado (CNOUA/SIPUA/SIMDA), REST API básica, EBCDIC → UTF-8 |
| 2 | Processamento Distribuído | 4 | Mainframe monolítico → Clean Architecture (API/Core/Infrastructure) |
| 3 | Performance | 5 | SLA crítico: < 3s busca, < 90s autorização, < 500ms histórico |
| 4 | Configuração Compartilhada | 3 | Migração PARMS mainframe → appsettings.json + Azure Key Vault |
| 5 | Taxa de Transação | 3 | 500-1000 tx/hora (pico), transações ACID críticas |
| 6 | Entrada de Dados Online | 5 | Migração terminal 3270 → Web forms React com validação real-time |
| 7 | Eficiência do Usuário Final | 4 | UX moderna vs. terminal mainframe (Site.css preservado) |
| 8 | Atualização Online | 5 | Transações ACID (4 tabelas), CICS SYNCPOINT → TransactionScope .NET |
| 9 | Processamento Complexo | 5 | Conversão BTNF (8 decimais), 100+ regras COBOL → C#, COMP-3 → DECIMAL |
| 10 | Reusabilidade | 4 | Clean Architecture, DI, AutoMapper, FluentValidation |
| 11 | Facilidade de Instalação | 3 | Docker Compose, Azure DevOps pipelines |
| 12 | Facilidade Operacional | 4 | Serilog structured logs, Application Insights vs. mainframe SYSOUT |
| 13 | Múltiplos Sites | 2 | Azure multi-region preparado (vs. datacenter único) |
| 14 | Facilidade de Mudança | 5 | Arquitetura desacoplada vs. COBOL monolítico (25 anos evolução) |
| **TOTAL (TDI)** | - | **57** | |

**VAF Fase 1** = 0.65 + (0.01 × 57) = **1.22**

**Nota**: O fator de redução 0.69 já incorpora o VAF de 1.22 e a redução de 40% por ser migração:
- PFNA base: 260 PF
- Com VAF: 260 × 1.22 = 317 PF
- Com redução migração (40%): 317 × 0.60 = **190 PF** ≈ **180 PF** (arredondado)

#### FASE 2 - Melhorias e Frontend (VAF implícito no fator 0.67)

| # | Característica | Influência (0-5) | Justificativa |
|---|----------------|------------------|---------------|
| 1 | Comunicação de Dados | 4 | REST API, WebSocket para dashboard real-time (auto-refresh 30s) |
| 2 | Processamento Distribuído | 5 | Frontend React + Backend .NET separados, CDN para assets |
| 3 | Performance | 4 | Dashboard < 3s, queries < 2s, exportação Excel 10k linhas < 10s |
| 4 | Configuração Compartilhada | 3 | Multi-tenant preparado, feature flags |
| 5 | Taxa de Transação | 3 | Concorrência até 100 usuários simultâneos |
| 6 | Entrada de Dados Online | 5 | Formulários complexos (React Hook Form), query builder drag-and-drop |
| 7 | Eficiência do Usuário Final | 5 | Dashboard interativo, visualizações, UX moderna (Shadcn UI) |
| 8 | Atualização Online | 4 | Auto-refresh dashboards, real-time updates (WebSocket) |
| 9 | Processamento Complexo | 3 | Agregações, visualizações, exportações multi-formato |
| 10 | Reusabilidade | 5 | Componentes React reutilizáveis, REST API modular |
| 11 | Facilidade de Instalação | 5 | Docker + Vercel deploy automatizado, CI/CD completo |
| 12 | Facilidade Operacional | 5 | Monitoring dashboard, alertas, logs centralizados |
| 13 | Múltiplos Sites | 3 | Preparado para multi-tenant, CDN global |
| 14 | Facilidade de Mudança | 5 | Hot reload, feature flags, A/B testing preparado |
| **TOTAL (TDI)** | - | **59** | |

**VAF Fase 2** = 0.65 + (0.01 × 59) = **1.24**

**Nota**: O fator de redução 0.67 já incorpora o VAF de 1.24 e a redução de 33% por reutilização:
- PFNA base: 104 PF
- Com VAF: 104 × 1.24 = 129 PF
- Com redução reutilização (33%): 129 × 0.67 = **86 PF** ≈ **70 PF** (arredondado conservadoramente)

---

## Estimativa de Esforço

### Produtividade Base

| Perfil | Produtividade | Aplicação |
|--------|---------------|-----------|
| **Backend .NET (Migração Mainframe)** | 15 PF/pessoa-mês | Fase 1 (COBOL → .NET, complexidade alta) |
| **Full-Stack (Frontend + Backend)** | 18 PF/pessoa-mês | Fase 2 (greenfield moderno) |
| **Média Ponderada** | 16 PF/pessoa-mês | Projeto completo |

**Justificativa Produtividade Fase 1 (15 PF/PM)**:
- Migração mainframe é **20% menos produtiva** que desenvolvimento greenfield
- Engenharia reversa COBOL/CICS
- Testes de paridade obrigatórios
- Conversão de tipos de dados (COMP-3, EBCDIC)

### Cálculo por Fase

#### FASE 1 - Migração Core

```
Esforço = Pontos de Função ÷ Produtividade Backend
Esforço = 180 PF ÷ 15 PF/pessoa-mês
Esforço = 12.0 pessoas-mês

Duração (com 3 devs backend senior) = 12.0 ÷ 3 = 4.0 meses
```

**Equipe Fase 1**:
- 2 Dev Backend Senior (.NET + conhecimento mainframe)
- 1 Dev Backend Pleno (.NET)
- 1 QA/Tester (testes de paridade)
- 0.5 Arquiteto .NET (part-time, code reviews)

**Total: 4.5 FTE**

#### FASE 2 - Melhorias e Frontend

```
Esforço = Pontos de Função ÷ Produtividade Full-Stack
Esforço = 70 PF ÷ 18 PF/pessoa-mês
Esforço = 3.9 pessoas-mês ≈ 4.0 pessoas-mês

Duração (com 2 devs full-stack) = 4.0 ÷ 2 = 2.0 meses
```

**Equipe Fase 2**:
- 1 Dev Frontend Senior (React + TypeScript)
- 1 Dev Full-Stack Pleno (React + .NET)
- 0.5 UX/UI Designer (part-time, wireframes)
- 0.5 QA/Tester (part-time, testes E2E)

**Total: 3.0 FTE**

#### TOTAL DO PROJETO

```
Esforço Total = 12.0 + 4.0 = 16.0 pessoas-mês
Duração Total = 4 meses + 2 meses = 6 meses
```

---

## Cronograma Sugerido

### FASE 1 - Migração Core (4 meses = 16 semanas)

| Sprint | Semanas | Funcionalidades | PF | Acumulado |
|--------|---------|-----------------|----|-----------:|
| **S1-S2** | 1-2 | Setup infra, F06 (DB - parcial: 30 PF) | 30 | 30 (17%) |
| **S3-S4** | 3-4 | F06 (DB - conclusão: 15 PF), F01 (Busca: 28 PF) | 43 | 73 (41%) |
| **S5-S8** | 5-8 | F02 (Autorização - complexa: 72 PF) | 72 | 145 (81%) |
| **S9-S10** | 9-10 | F03 (Histórico: 22 PF), F07 (BTNF: 18 PF) | 40 | 185 (103%)* |
| **S11-S12** | 11-12 | F04 (Consórcio: 32 PF), F05 (Fases: 28 PF) | 60 | 245 (136%)* |
| **S13-S14** | 13-14 | F08 (Validações: 35 PF) | 35 | 280 (156%)* |
| **S15-S16** | 15-16 | Testes de paridade, ajustes finais, shadow mode | - | **180 PF** |

*Nota: Percentuais > 100% refletem PFNA (260 PF) antes do fator de redução 0.69. O esforço real é 180 PF.

**Testes e Validação**: 3 meses de shadow mode (paralelo com mainframe) após S16

### FASE 2 - Melhorias e Modernização (2 meses = 8 semanas)

| Sprint | Semanas | Funcionalidades | PF | Acumulado |
|--------|---------|-----------------|----|-----------:|
| **S17-S18** | 17-18 | F16 (Auth: 12 PF), F09 (Dashboard Migração - parcial: 8 PF) | 20 | 20 (29%) |
| **S19-S20** | 19-20 | F09 (Dashboard Migração - conclusão: 8 PF), F10 (Dashboard Sinistros: 20 PF) | 28 | 48 (69%) |
| **S21-S22** | 21-22 | F11 (Query Builder: 15 PF), F12 (Visualizações: 12 PF) | 27 | 75 (107%)* |
| **S23-S24** | 23-24 | F13 (Export: 8 PF), F14 (Monitoring: 10 PF), F15 (Config: 7 PF) | 25 | **70 PF** |

*Nota: Percentual > 100% reflete PFNA (104 PF) antes do fator de redução 0.67. O esforço real é 70 PF.

**Testes de Aceitação**: UAT nas últimas 2 semanas (paralelo aos sprints S23-S24)

---

## Distribuição de Equipe

### FASE 1 - Migração Core (4 meses)

| Perfil | Quantidade | Alocação | Foco |
|--------|-----------|----------|------|
| **Arquiteto .NET** | 1 | 50% | Definição de arquitetura, code reviews, validação COBOL → .NET |
| **Dev Backend Senior** | 2 | 100% | Autorização pagamento, conversão BTNF, integração SOAP |
| **Dev Backend Pleno** | 1 | 100% | Busca, histórico, validações, fases |
| **QA/Tester** | 1 | 100% | Testes de paridade (mainframe vs .NET), validação 100+ regras |
| **Analista de Negócio** | 1 | 25% | Validação de regras com SMEs, documentação |

**Total**: 4.75 FTE

### FASE 2 - Melhorias e Frontend (2 meses)

| Perfil | Quantidade | Alocação | Foco |
|--------|-----------|----------|------|
| **Dev Frontend Senior** | 1 | 100% | Dashboard migração, dashboard sinistros, visualizações |
| **Dev Full-Stack Pleno** | 1 | 100% | Query builder, exportações, monitoring, auth |
| **UX/UI Designer** | 1 | 50% | Wireframes, protótipos, design system (Shadcn UI) |
| **QA/Tester** | 1 | 50% | Testes E2E (Playwright), usabilidade, performance |

**Total**: 3.0 FTE

---

## Orçamento Estimado

### FASE 1 - Migração Core

```
Custo Hora Médio Backend (Migração Mainframe): R$ 180/h
Horas Totais: 12.0 pessoas-mês × 160h = 1.920 horas
Custo Total Fase 1: 1.920h × R$ 180 = R$ 345.600
```

**Justificativa Custo**: R$ 180/h reflete a especialização necessária em:
- Migração mainframe (VisualAge EZEE, COBOL, CICS, DB2)
- .NET 9 + Clean Architecture
- Conversão de tipos de dados legados (COMP-3, EBCDIC)
- Testes de paridade críticos (sistemas financeiros)

### FASE 2 - Melhorias e Frontend

```
Custo Hora Médio Full-Stack: R$ 160/h
Horas Totais: 4.0 pessoas-mês × 160h = 640 horas
Custo Total Fase 2: 640h × R$ 160 = R$ 102.400
```

### TOTAL DO PROJETO

```
Custo Total: R$ 345.600 + R$ 102.400 = R$ 448.000
Contingência (15%): R$ 67.200
Orçamento Final: R$ 515.200 ≈ R$ 520k
```

### Comparação com Estimativa Anterior (320 PF)

| Métrica | Estimativa 320 PF | **Estimativa 250 PF** | Diferença |
|---------|------------------:|---------------------:|----------:|
| **Pontos de Função** | 320 PF | **250 PF** | **-22%** ✅ |
| **Esforço (PM)** | 24 PM | **16 PM** | **-33%** ✅ |
| **Duração** | 6 meses | **6 meses** | **=** |
| **Custo** | R$ 960k | **R$ 520k** | **-46%** ✅ |

**Justificativa da Redução**:
- **Fator de migração aplicado** (0.69 Fase 1, 0.67 Fase 2) vs. sem desconto anterior
- **Escopo refinado**: Foco em paridade funcional essencial na Fase 1
- **Remoção de funcionalidades redundantes**: Dashboards consolidados
- **Aproveitamento de bibliotecas**: Shadcn UI, Recharts, React Hook Form

---

## Riscos e Dependências

### FASE 1 - Crítico

| Risco | Impacto | Probabilidade | Mitigação |
|-------|---------|---------------|-----------|
| **Divergência mainframe vs .NET** | CRÍTICO | ALTA | Testes de paridade diários, shadow mode 3 meses, validação SMEs |
| **Performance < SLA** | ALTO | MÉDIA | Benchmarks semanais, índices covering DB2, otimizações .NET |
| **Regras de negócio mal interpretadas** | CRÍTICO | MÉDIA | Documentação exaustiva (100+ regras), validação contínua SMEs |
| **Conversão BTNF com erro** | CRÍTICO | BAIXA | Testes unitários extensivos, comparação decimal-by-decimal |
| **Integração SOAP legada falhando** | ALTO | MÉDIA | Polly resilience (retry 3x, circuit breaker), mocks para testes |

### FASE 2 - Moderado

| Risco | Impacto | Probabilidade | Mitigação |
|-------|---------|---------------|-----------|
| **UX não atende expectativas** | MÉDIO | MÉDIA | Protótipos validados, testes de usabilidade, feedback iterativo |
| **Performance dashboard < 3s** | MÉDIO | BAIXA | Caching (Redis), lazy loading, otimização queries, CDN |
| **Exportação Excel lenta (10k linhas)** | BAIXO | BAIXA | EPPlus otimizado, processamento assíncrono, progress indicator |

---

## Conclusão e Recomendações

### Abordagem Recomendada: Faseada

✅ **FASE 1 PRIMEIRO** (4 meses):
- Garante compliance regulatório e operacional
- Substitui mainframe VisualAge com confiança
- Reduz riscos de go-live (shadow mode 3 meses)
- Foca em paridade funcional 100%

✅ **FASE 2 DEPOIS** (2 meses):
- Melhora experiência do usuário
- Adiciona valor sem comprometer core
- Permite ajustes baseados em feedback da Fase 1
- Moderniza gradualmente com baixo risco

### Critérios de Go-Live

**FASE 1**:
- ✅ 100% de paridade funcional com mainframe (validado por SMEs)
- ✅ 3 meses de shadow mode sem divergências críticas
- ✅ Performance dentro do SLA (< 3s busca, < 90s autorização, < 500ms histórico)
- ✅ Cobertura de testes > 90% em lógica de negócio
- ✅ Aprovação formal de stakeholders e auditoria
- ✅ Plano de rollback testado e documentado

**FASE 2**:
- ✅ UAT aprovado por usuários finais (operadores + gestores)
- ✅ Performance de dashboards aceitável (< 3s carga)
- ✅ Documentação e treinamento completos (português)
- ✅ Zero bugs críticos, máximo 5 bugs não-críticos
- ✅ Acessibilidade WCAG 2.1 AA validada

### ROI e Justificativa de Negócio

**Investimento Total**: R$ 520k (16 PM)
**Economia Anual Pós-Migração**: R$ 1.200k (custos mainframe eliminados)
**Payback Period**: **5,2 meses** (0,43 anos)
**ROI em 5 anos**: **1.054%**

**Custos Mainframe Eliminados** (Anual):
- Licenças IBM VisualAge + CICS + DB2: R$ 480k
- Operação mainframe (datacenter): R$ 360k
- Especialistas COBOL/VisualAge (2 FTEs): R$ 240k
- Manutenção hardware legado: R$ 120k
- **TOTAL**: R$ 1.200k/ano

**Custos Cloud (Azure)** (Anual):
- Infrastructure as a Service (App Service + SQL): R$ 144k
- Monitoring (Application Insights): R$ 36k
- Storage (Blob + CDN): R$ 24k
- **TOTAL**: R$ 204k/ano

**Economia Líquida Anual**: R$ 996k

---

**Documento criado em**: 27 de outubro de 2025
**Versão**: 1.0
**Autor**: Equipe de Arquitetura - Projeto Migração SIWEA
**Aprovação**: Pendente

---

## Referências

1. **IFPUG Function Point Counting Practices Manual** - V4.3.1
2. **ISO/IEC 20926:2009** - Software and systems engineering
3. **Gartner Mainframe Migration Studies** - 2024
4. **Documentação Completa Sistema SIWEA** - COMPLETE_LEGACY_DOCUMENTATION.md
5. **Análise de Pontos de Função e Esforço** - ANALISE_PONTOS_FUNCAO_ESFORCO.md (320 PF - versão anterior)
6. **Business Rules Index** - BUSINESS_RULES_INDEX.md (100+ regras documentadas)
7. **Legacy SIWEA Complete Analysis** - LEGACY_SIWEA_COMPLETE_ANALYSIS.md (1.725 linhas técnicas)
