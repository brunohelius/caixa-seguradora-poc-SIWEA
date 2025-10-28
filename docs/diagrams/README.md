# Diagramas UML - SIWEA Legacy System

**Versão:** 1.0
**Data:** 2025-10-27
**Autor:** Análise Técnica do Sistema Legado

---

## Índice

1. [Visão Geral](#visão-geral)
2. [Diagramas de Casos de Uso](#diagramas-de-casos-de-uso)
3. [Diagramas de Classes](#diagramas-de-classes)
4. [Diagramas de Sequência](#diagramas-de-sequência)
5. [Diagramas de Estado e Atividade](#diagramas-de-estado-e-atividade)
6. [Como Visualizar os Diagramas](#como-visualizar-os-diagramas)
7. [Referências](#referências)

---

## Visão Geral

Esta pasta contém **todos os diagramas UML** do sistema legado SIWEA (Sistema de Liberação de Pagamento de Sinistros), organizados em 4 documentos principais:

| Documento | Conteúdo | Linhas | Diagramas |
|-----------|----------|--------|-----------|
| **01-use-cases.md** | Casos de uso e fluxos funcionais | ~2.100 | 12 casos de uso |
| **02-class-diagrams.md** | Modelo de classes (3 camadas) | ~1.800 | 8 diagramas |
| **03-sequence-diagrams.md** | Sequências de interação | ~2.600 | 5 sequências |
| **04-state-activity-diagrams.md** | Estados e atividades | ~1.400 | 11 diagramas |
| **TOTAL** | | **~7.900** | **36 diagramas** |

---

## Diagramas de Casos de Uso

**Arquivo:** `01-use-cases.md`

### Casos de Uso Principais

| UC | Nome | Ator | Complexidade |
|----|------|------|--------------|
| **UC-01** | Buscar Sinistro por Protocolo | Operador | Baixa |
| **UC-02** | Buscar Sinistro por Número | Operador | Baixa |
| **UC-03** | Buscar Sinistro por Código Líder | Operador | Baixa |
| **UC-04** | Autorizar Pagamento ⭐ | Operador | **Alta** |
| **UC-05** | Validar Produto Consórcio (CNOUA) | Sistema | Média |
| **UC-06** | Validar Contrato EFP (SIPUA) | Sistema | Média |
| **UC-07** | Validar Contrato HB (SIMDA) | Sistema | Média |
| **UC-08** | Converter Valores para BTNF | Sistema | Baixa |
| **UC-09** | Registrar Histórico | Sistema | Média |
| **UC-10** | Atualizar Fases | Sistema | Média |
| **UC-11** | Consultar Histórico de Pagamentos | Operador | Baixa |
| **UC-12** | Visualizar Dashboard | Operador | Baixa |

**Destaques:**
- ⭐ **UC-04 (Autorizar Pagamento):** Caso de uso crítico com 10 passos, validações externas, transação ACID
- Diagramas de sequência detalhados para cada UC
- Matriz de dependências entre casos de uso
- Especificação completa de fluxos alternativos

---

## Diagramas de Classes

**Arquivo:** `02-class-diagrams.md`

### Diagramas Incluídos

1. **Diagrama Principal - Domain Model**
   - 13 entidades principais
   - 3 value objects
   - Relacionamentos e cardinalidades

2. **Camada de Domínio (Core)**
   - 5 domain services
   - 6 repository interfaces
   - 2 validators

3. **Camada de API**
   - 3 controllers
   - 6 DTOs (Request/Response)
   - 2 middleware

4. **Camada de Infraestrutura**
   - 6 repository implementations
   - 1 DbContext
   - 3 external clients (CNOUA, SIPUA, SIMDA)
   - 2 resilience policies

5. **Diagrama de Pacotes - Clean Architecture**
   - Dependências entre camadas
   - Api → Core → Infrastructure

6. **AutoMapper Profiles**
   - Mapeamentos de entidades para DTOs

7. **FluentValidation Validators**
   - Validadores de requisições

8. **Diagrama de Objetos - Exemplo de Autorização**
   - Instâncias concretas em um fluxo real

### Classes por Camada

| Camada | Quantidade | Exemplos |
|--------|------------|----------|
| **API** | 10 classes | ClaimSearchController, PaymentAuthorizationRequest, ValidationErrorResponse |
| **Core** | 25+ classes | ClaimMaster, PaymentAuthorizationService, BusinessRuleValidator |
| **Infrastructure** | 15+ classes | ClaimRepository, SiweaDbContext, CNOUAClient, CircuitBreakerPolicy |
| **TOTAL** | **~50 classes** | |

### Padrões de Design

- Repository Pattern
- Service Layer
- DTO (Data Transfer Object)
- Dependency Injection
- Unit of Work
- Circuit Breaker
- Retry Policy
- Value Object
- Factory

---

## Diagramas de Sequência

**Arquivo:** `03-sequence-diagrams.md`

### Sequências Detalhadas

| Sequência | Título | Participantes | Complexidade |
|-----------|--------|---------------|--------------|
| **SEQ-1** | Busca de Sinistro por Protocolo (Fluxo Completo) | 8 | Média |
| **SEQ-2** | Autorização de Pagamento (Fluxo Completo) ⭐ | 13 | **Alta** |
| **SEQ-3** | Validação de Produto com Circuit Breaker | 4 | Média |
| **SEQ-4** | Gestão de Fases (Abertura e Fechamento) | 4 | Média |
| **SEQ-5** | Rollback de Transação (Cenário de Erro) | 7 | Alta |

**Destaques:**
- ⭐ **SEQ-2:** Sequência mais complexa (146 passos) com:
  - 12 fases de processamento
  - Validações externas (CNOUA)
  - Transação ACID de 4 tabelas
  - Retry policies e circuit breaker
  - Tratamento completo de erros

### Tempos Esperados

| Sequência | Tempo | SLA |
|-----------|-------|-----|
| SEQ-1 | 1-2s | < 3s |
| SEQ-2 | 5-15s | < 90s |
| SEQ-3 | 60s (recovery) | N/A |
| SEQ-4 | 200-500ms | < 2s |
| SEQ-5 | 100-300ms | < 1s |

### Pontos de Observabilidade

Cada sequência documenta pontos de logging:
- Timestamps e duração
- Request/response de integrações
- Transações (BEGIN/COMMIT/ROLLBACK)
- Erros com stack trace
- Métricas de performance

---

## Diagramas de Estado e Atividade

**Arquivo:** `04-state-activity-diagrams.md`

### Diagramas de Estado (4)

1. **Ciclo de Vida do Sinistro**
   - 9 estados: Novo → Pendente → EmProcesso → Autorizado → Pago → Fechado
   - Estados compostos (EmProcesso, Autorizado)
   - Transições de erro (Bloqueado, Cancelado)

2. **Fase do Sinistro**
   - 3 estados: Não Iniciada → Aberta → Fechada
   - Sentinela '9999-12-31' para fase aberta

3. **Circuit Breaker**
   - 3 estados: Fechado ↔ Aberto ↔ Meio-Aberto
   - Configuração: 5 falhas → abrir, 60s → testar

4. **Estados Compostos**
   - EmProcesso: ValidandoProduto → ConvertendoMoeda → RegistrandoTransação → AtualizandoFases
   - Autorizado: AguardandoContabilizacao → AguardandoPagamento

### Diagramas de Atividade (7)

1. **Autorização de Pagamento** - Fluxograma completo com decisões
2. **Busca de Sinistro** - Com swimlanes (Operador, Sistema, BD)
3. **Conversão Monetária BTNF** - Cálculos com arredondamento Banker's
4. **Gestão de Fases** - Loop de abertura/fechamento com verificação idempotente
5. **Retry Policy** - Backoff exponencial (1s, 2s)
6. **Transação ACID** - 4 operações com rollback em caso de erro
7. **Dashboard Refresh** - Loop de atualização a cada 30s

### Convenções

**Formas:**
- Retângulo arredondado: Estado
- Losango: Decisão
- Retângulo: Atividade
- Paralelogramo: Entrada/Saída
- Elipse: Início/Fim

**Cores:**
- 🟦 Azul: Início/Fim
- 🟩 Verde: Sucesso/Commit
- 🟥 Vermelho: Erro/Rollback

---

## Como Visualizar os Diagramas

### Opção 1: GitHub (Recomendado)

Todos os diagramas usam **Mermaid**, que é renderizado automaticamente no GitHub:

1. Abra qualquer arquivo `.md` no GitHub
2. Os diagramas são renderizados automaticamente
3. Navegação nativa sem ferramentas adicionais

### Opção 2: VS Code

**Extensão:** Markdown Preview Mermaid Support

```bash
# Instalar extensão
code --install-extension bierner.markdown-mermaid

# Abrir preview
# Ctrl+Shift+V (Windows/Linux)
# Cmd+Shift+V (Mac)
```

### Opção 3: Mermaid Live Editor

Para edição interativa:

1. Acesse: https://mermaid.live/
2. Copie o código Mermaid do diagrama
3. Cole no editor
4. Visualize e exporte (PNG, SVG, PDF)

### Opção 4: Exportar Imagens

**Usando Mermaid CLI:**

```bash
# Instalar Mermaid CLI
npm install -g @mermaid-js/mermaid-cli

# Exportar diagrama para PNG
mmdc -i 01-use-cases.md -o use-cases.png

# Exportar para SVG
mmdc -i 02-class-diagrams.md -o class-diagrams.svg

# Exportar para PDF
mmdc -i 03-sequence-diagrams.md -o sequence-diagrams.pdf
```

### Opção 5: Confluence/Wiki

**Para importar no Confluence:**

1. Copiar código Mermaid
2. Usar macro "Mermaid Diagram" ou "PlantUML"
3. Colar código e salvar

**Alternativa - Imagens:**

1. Exportar diagrama como PNG/SVG (Opção 4)
2. Fazer upload da imagem no Confluence
3. Incorporar na página

---

## Referências

### Documentos Relacionados

| Documento | Descrição | Localização |
|-----------|-----------|-------------|
| **SISTEMA_LEGADO_VISAO_GERAL.md** | Visão executiva do sistema legado | `docs/` |
| **SISTEMA_LEGADO_REGRAS_NEGOCIO.md** | 100 regras de negócio detalhadas | `docs/` |
| **SISTEMA_LEGADO_ARQUITETURA.md** | Arquitetura técnica 3-tier | `docs/` |
| **SISTEMA_LEGADO_MODELO_DADOS.md** | 13 entidades com EF Core | `docs/` |
| **SISTEMA_LEGADO_PROCESSOS.md** | Workflows e fluxos de processo | `docs/` |
| **LEGACY_SIWEA_COMPLETE_ANALYSIS.md** | Análise completa do código legado | `docs/` |

### Especificações Técnicas

- **Clean Architecture:** API → Core → Infrastructure
- **Padrões:** Repository, Service Layer, DI, UoW, Circuit Breaker
- **ORM:** Entity Framework Core 9.0 (database-first)
- **Validação:** FluentValidation 11.9.0
- **Resiliência:** Polly 8.2.0 (retry, circuit breaker)
- **Mapeamento:** AutoMapper 12.0.1

### Regras de Negócio Mapeadas

| Diagrama | Regras Aplicadas |
|----------|------------------|
| **UC-04 (Autorização)** | BR-010 a BR-074 (64 regras) |
| **SEQ-2 (Autorização)** | BR-023 a BR-042, BR-057 a BR-067 |
| **Estado Sinistro** | BR-001 a BR-009, BR-088 a BR-095 |
| **Atividade Conversão** | BR-023 a BR-033 (conversão BTNF) |
| **Atividade Fases** | BR-057 a BR-067 (gestão de fases) |

### Métricas de Cobertura

- ✅ **12 Casos de Uso** documentados (100% das funcionalidades principais)
- ✅ **13 Entidades** modeladas (100% do schema legado)
- ✅ **50+ Classes** especificadas (arquitetura completa)
- ✅ **5 Sequências Críticas** detalhadas (busca, autorização, validação, fases, rollback)
- ✅ **11 Diagramas de Estado/Atividade** (100% dos processos)

---

## Resumo Executivo

### O que foi documentado?

**Funcional:**
- ✅ Todos os casos de uso (12 UCs)
- ✅ Todos os fluxos principais e alternativos
- ✅ Integrações externas (CNOUA, SIPUA, SIMDA)
- ✅ Validações de negócio (100 regras mapeadas)

**Técnico:**
- ✅ Modelo de domínio completo (13 entidades)
- ✅ Arquitetura em 3 camadas
- ✅ Padrões de design aplicados
- ✅ Sequências de interação detalhadas
- ✅ Máquinas de estado
- ✅ Fluxogramas de processos

**Não-Funcional:**
- ✅ Resiliência (circuit breaker, retry)
- ✅ Transações ACID
- ✅ Performance (tempos esperados)
- ✅ Observabilidade (logging)

### Próximos Artefatos

1. **Especificações OpenAPI** - APIs REST documentadas (Item 3.2)
2. **Especificações WSDL** - Serviços SOAP documentados (Item 3.3)

---

## Glossário

| Termo | Definição |
|-------|-----------|
| **BTNF** | Bônus do Tesouro Nacional Fiscal (moeda padrão conversão) |
| **SIWEA** | Sistema de Liberação de Pagamento de Sinistros |
| **CNOUA** | Serviço de validação de produtos consórcio |
| **SIPUA** | Serviço de validação de contratos EFP |
| **SIMDA** | Serviço de validação de contratos HB |
| **EZERT8** | Código de retorno padrão (00000000 = sucesso) |
| **Sentinela 9999-12-31** | Valor especial indicando fase aberta |
| **ACID** | Atomicity, Consistency, Isolation, Durability |
| **Circuit Breaker** | Padrão de resiliência para falhas de serviço |
| **Banker's Rounding** | Arredondamento IEEE 754 (para o par mais próximo) |

---

## Licença e Uso

**Confidencialidade:** Documentação interna - Caixa Seguradora
**Versão:** 1.0
**Última Atualização:** 2025-10-27
**Revisores:** Equipe de Migração SIWEA

---

**FIM DO ÍNDICE DE DIAGRAMAS**
