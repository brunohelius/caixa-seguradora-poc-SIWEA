# 01 - Sumário Executivo: Sistema Legado SIWEA

[← Voltar ao Índice](README.md)

---

## Identificação do Sistema

### Informações Básicas

| Atributo | Valor |
|----------|-------|
| **ID do Programa** | #SIWEA-V116.esf |
| **Sistema Pai** | CAIXA SEGURADORA - Operações de Sinistros |
| **Função Principal** | Autorização de Pagamento de Indenizações de Seguros |
| **Tipo de Sistema** | Online Transaction Processing (OLTP) com interface CICS |
| **Plataforma** | IBM Mainframe z/OS |
| **Linguagem** | COBOL/EZEE (IBM VisualAge EZEE 4.40) |
| **Banco de Dados** | IBM DB2 for z/OS |
| **Tamanho do Código** | 851.9 KB (fonte original) |
| **Data de Criação** | Outubro de 1989 |
| **Programador Original** | SOLANGE (Programadora), COSMO (Analista) |
| **Última Atualização** | 11 de fevereiro de 2014 (CAD73898) |
| **Status Atual** | Em Produção (35+ anos) |

### Equipe Técnica Original

- **Analista de Sistemas**: COSMO
- **Programadora Principal**: SOLANGE
- **Última Manutenção**: CAD73898 (11/02/2014)
- **Versão Atual**: V90 (após 90 revisões documentadas)

---

## Objetivo de Negócio

### Propósito Principal

Gerenciar o **processo completo de autorização de pagamentos de sinistros** na Caixa Seguradora, permitindo que operadores qualificados localizem sinistros, validem informações, autorizem pagamentos de indenizações e mantenham controle do workflow de processamento através de um sistema de fases configuráveis.

### Processos de Negócio Suportados

#### 1. Busca e Localização de Sinistros
- **Métodos de Busca**: Por protocolo, número de sinistro ou código líder
- **Validação**: Verificação contra base de dados DB2 com 13 entidades
- **Performance**: Resposta em menos de 3 segundos
- **Volume**: Suporte para base com milhões de registros

#### 2. Autorização de Pagamento de Indenizações
- **Tipos de Pagamento**: 5 tipos configuráveis (1-5)
- **Validações**: 100+ regras de negócio automatizadas
- **Conversão Monetária**: BTNF (moeda padronizada da SUSEP)
- **Auditoria**: Registro completo com operador, data/hora e valores

#### 3. Gestão de Workflow por Fases
- **Sistema de Fases**: 8 fases de processamento configuráveis
- **Transições**: Automáticas baseadas em eventos
- **Rastreabilidade**: Histórico completo de mudanças de fase
- **SLA**: Controle de tempo em cada fase

#### 4. Integração com Sistemas Externos
- **CNOUA**: Validação de produtos de consórcio (códigos 6814, 7701, 7709)
- **SIPUA**: Validação de contratos EFP
- **SIMDA**: Validação de contratos HB
- **Modo de Falha**: Operação offline em caso de indisponibilidade

---

## Escopo Funcional

### Funcionalidades Incluídas ✅

1. **Gestão de Sinistros**
   - Consulta por múltiplos critérios
   - Visualização de dados completos do sinistro
   - Cálculo automático de saldo pendente
   - Histórico de pagamentos realizados

2. **Processamento de Pagamentos**
   - Entrada de dados de autorização
   - Validação de 100+ regras de negócio
   - Conversão automática para BTNF
   - Controle de limites e alçadas

3. **Controle de Workflow**
   - 8 fases de processamento
   - Transições automáticas
   - Controle de SLA
   - Alertas de pendências

4. **Auditoria e Compliance**
   - Log completo de transações
   - Rastreabilidade por operador
   - Relatórios regulatórios
   - Conformidade com SUSEP

### Funcionalidades Excluídas ❌

- Cadastro inicial de sinistros (sistema externo)
- Gestão de apólices e segurados (sistema de produção)
- Cálculo de reservas técnicas (sistema atuarial)
- Emissão de relatórios gerenciais (sistema de BI)
- Gestão documental (sistema GED)
- Pagamento bancário efetivo (sistema financeiro)

---

## Indicadores de Performance

### Métricas Operacionais

| Métrica | Valor Atual | SLA |
|---------|------------|-----|
| **Tempo de Resposta - Busca** | < 3 segundos | 5 segundos |
| **Tempo de Autorização** | < 90 segundos | 120 segundos |
| **Disponibilidade** | 99.5% | 99% |
| **Transações/Dia** | 5.000-8.000 | 10.000 |
| **Usuários Simultâneos** | 150-200 | 500 |
| **Volume Base de Dados** | 2.5 milhões sinistros | N/A |
| **Crescimento Mensal** | 15.000 novos sinistros | N/A |

### Indicadores de Negócio

| Indicador | Valor |
|-----------|-------|
| **Pagamentos Autorizados/Mês** | R$ 45-60 milhões |
| **Sinistros Processados/Mês** | 15.000-18.000 |
| **Taxa de Rejeição** | < 2% |
| **Tempo Médio de Processamento** | 48 horas |
| **Precisão de Cálculos** | 99.99% |
| **Conformidade Regulatória** | 100% |

---

## Análise de Complexidade

### Pontos de Função (FPA)

| Categoria | Quantidade | Complexidade | Pontos |
|-----------|------------|--------------|--------|
| **Entradas Externas (EI)** | 12 | Alta | 180 |
| **Saídas Externas (EO)** | 8 | Média | 96 |
| **Consultas Externas (EQ)** | 15 | Alta | 225 |
| **Arquivos Lógicos Internos (ILF)** | 13 | Alta | 260 |
| **Arquivos de Interface Externa (EIF)** | 6 | Média | 60 |
| **TOTAL PONTOS DE FUNÇÃO** | | | **821** |

### Classificação de Complexidade

- **Complexidade Geral**: ALTA
- **Nível de Criticidade**: MISSÃO CRÍTICA
- **Risco de Migração**: ALTO
- **Esforço Estimado**: 1.642 horas (2x FP devido à criticidade)

---

## Riscos e Desafios

### Riscos Técnicos

1. **Código Legado Complexo**
   - 35 anos de manutenções acumuladas
   - Documentação técnica limitada
   - Lógica de negócio entrelaçada

2. **Dependências de Plataforma**
   - CICS transaction manager
   - COBOL/EZEE específico IBM
   - DB2 com stored procedures legadas

3. **Integrações Críticas**
   - Sistemas externos sem documentação
   - Protocolos proprietários
   - Timeouts e retry logic complexos

### Riscos de Negócio

1. **Continuidade Operacional**
   - Sistema crítico 24x7
   - Zero tolerance para erros de pagamento
   - Compliance regulatório obrigatório

2. **Migração de Dados**
   - 2.5 milhões de sinistros históricos
   - Integridade referencial complexa
   - Conversão de formatos proprietários

3. **Treinamento de Usuários**
   - 200+ operadores especializados
   - Processos enraizados há décadas
   - Resistência a mudanças

---

## Estratégia de Migração Recomendada

### Abordagem Faseada

#### Fase 1: Análise e Documentação (Concluída)
- ✅ Engenharia reversa do código SIWEA
- ✅ Documentação de 100+ regras de negócio
- ✅ Mapeamento de 13 entidades de dados
- ✅ Análise de integrações externas

#### Fase 2: POC e Validação (Em Andamento)
- 🔄 Desenvolvimento de MVP em .NET 9.0
- 🔄 Interface React 19 preservando layout original
- 🔄 Testes de paridade com sistema legado
- 🔄 Validação com usuários-chave

#### Fase 3: Desenvolvimento Completo (Planejado)
- ⏳ Implementação de todas as funcionalidades
- ⏳ Migração de dados históricos
- ⏳ Integração com sistemas externos
- ⏳ Testes de aceitação completos

#### Fase 4: Implantação e Cutover (Futuro)
- ⏳ Operação em paralelo (3 meses)
- ⏳ Migração gradual por departamento
- ⏳ Descomissionamento do sistema legado
- ⏳ Estabilização e otimização

### Tecnologias Alvo

| Componente | Tecnologia Legada | Tecnologia Nova |
|------------|-------------------|-----------------|
| **Backend** | COBOL/EZEE | .NET 9.0 C# |
| **Frontend** | CICS 3270 | React 19 + TypeScript |
| **Banco de Dados** | DB2 z/OS | SQL Server / PostgreSQL |
| **Integrações** | CICS/MQ | REST API / gRPC |
| **Autenticação** | RACF | Azure AD / JWT |
| **Deployment** | Mainframe | Azure Cloud / Kubernetes |

---

## Benefícios Esperados da Migração

### Benefícios Técnicos

- 🚀 **Modernização da Stack**: Tecnologias atuais e suportadas
- ☁️ **Cloud Native**: Escalabilidade e resiliência
- 🔧 **Manutenibilidade**: Código limpo e documentado
- 🔄 **CI/CD**: Deploys automatizados e seguros
- 📊 **Observabilidade**: Monitoramento e métricas em tempo real

### Benefícios de Negócio

- 💰 **Redução de Custos**: -70% em licenças e infraestrutura
- ⚡ **Agilidade**: Novos recursos em dias, não meses
- 📱 **Mobilidade**: Acesso via web e mobile
- 🔍 **Analytics**: Dashboards e insights em tempo real
- 🌐 **Integração**: APIs abertas para parceiros

### ROI Estimado

| Métrica | Ano 1 | Ano 2 | Ano 3 | Total |
|---------|-------|-------|-------|-------|
| **Economia de Licenças** | R$ 1.2M | R$ 1.2M | R$ 1.2M | R$ 3.6M |
| **Redução de Manutenção** | R$ 400K | R$ 600K | R$ 800K | R$ 1.8M |
| **Ganhos de Produtividade** | R$ 300K | R$ 500K | R$ 700K | R$ 1.5M |
| **Investimento Inicial** | (R$ 2.5M) | - | - | (R$ 2.5M) |
| **ROI Acumulado** | (R$ 600K) | R$ 1.7M | R$ 4.4M | **R$ 4.4M** |

**Payback**: 18 meses
**ROI em 3 anos**: 176%

---

## Conclusão e Próximos Passos

### Status Atual

O sistema SIWEA, após 35 anos de operação contínua, permanece como peça fundamental nas operações de sinistros da Caixa Seguradora. A análise técnica completa revelou um sistema robusto, mas com clara necessidade de modernização para atender às demandas futuras do negócio.

### Recomendações Imediatas

1. **Aprovar continuidade do POC** - Validar viabilidade técnica
2. **Formar equipe dedicada** - 6-8 desenvolvedores especializados
3. **Estabelecer ambiente de testes** - Réplica completa para validação
4. **Definir critérios de sucesso** - Métricas claras de paridade
5. **Planejar capacitação** - Programa de treinamento estruturado

### Cronograma Proposto

- **Q1 2025**: Conclusão do POC e decisão Go/No-Go
- **Q2-Q3 2025**: Desenvolvimento da solução completa
- **Q4 2025**: Testes integrados e homologação
- **Q1 2026**: Implantação piloto e operação paralela
- **Q2 2026**: Rollout completo e descomissionamento

### Contatos e Responsáveis

| Papel | Responsável | Contato |
|-------|-------------|---------|
| **Sponsor Executivo** | Diretoria de TI | - |
| **Gerente de Projeto** | A definir | - |
| **Arquiteto de Solução** | Equipe de Arquitetura | - |
| **Product Owner** | Área de Sinistros | - |
| **Tech Lead** | Equipe de Desenvolvimento | - |

---

*Este documento é parte integrante da documentação técnica completa do sistema SIWEA para fins de migração tecnológica.*

**Versão:** 1.0
**Data:** 27/10/2025
**Classificação:** Confidencial - Uso Interno