# Documentação do Sistema Legado SIWEA

## Sistema de Liberação de Pagamento de Sinistros

### Visão Geral

Esta documentação completa descreve o sistema legado SIWEA (Sistema de Indenização e Workflow de Eventos Atendidos), um sistema crítico de missão desenvolvido em 1989 utilizando IBM VisualAge EZEE para mainframe, responsável pela autorização e controle de pagamentos de sinistros na Caixa Seguradora.

### Índice de Documentos

| Documento | Descrição | Páginas |
|-----------|-----------|---------|
| [01 - Sumário Executivo](01-executive-summary.md) | Visão executiva do sistema, objetivos e indicadores | 12 |
| [02 - Arquitetura Técnica](02-architecture.md) | Arquitetura em 3 camadas, infraestrutura e deployment | 28 |
| [03 - Estruturas de Dados](03-data-structures.md) | Working storage, copy books, layouts de registros | 22 |
| [04 - Modelo de Banco de Dados](04-database-model.md) | 13 entidades, relacionamentos, DDL scripts | 35 |
| [05 - Lógica de Negócio](05-business-logic.md) | 122 regras de negócio documentadas e categorizadas | 40 |
| [06 - Integrações Externas](06-external-integrations.md) | CNOUA, SIPUA, SIMDA - protocolos e especificações | 25 |
| [07 - Guia de Operações](07-operations-guide.md) | Procedimentos operacionais, troubleshooting, backup | 18 |
| [08 - Interface e Telas](08-ui-screens.md) | Mapas de telas 3270, navegação, campos | 20 |
| [09 - Guia de Migração](09-migration-guide.md) | Estratégia completa para migração .NET 9.0 | 30 |
| [10 - Glossário](10-glossary.md) | Termos de negócio e técnicos | 15 |

### Estatísticas do Sistema

| Métrica | Valor |
|---------|--------|
| **Tempo em Produção** | 35+ anos (1989-2025) |
| **Linguagem** | COBOL/EZEE |
| **Tamanho do Código** | 851.9 KB |
| **Tabelas de Dados** | 13 |
| **Regras de Negócio** | 122 |
| **Integrações Externas** | 3 |
| **Usuários Ativos** | 200+ |
| **Transações/Dia** | 8.000 |
| **Volume de Dados** | 2.5M sinistros |
| **Criticidade** | Missão Crítica |

### Tecnologias Utilizadas

#### Ambiente Legado
- IBM z/OS Mainframe
- IBM CICS Transaction Server
- IBM DB2 for z/OS
- IBM MQ Series
- IBM VisualAge EZEE 4.40
- COBOL ANSI 85
- Terminal 3270

#### Migração Planejada
- .NET 9.0
- React 19 + TypeScript
- SQL Server / PostgreSQL
- Azure Cloud
- Docker / Kubernetes

### Propósito da Documentação

Esta documentação foi criada para:

1. **Preservação do Conhecimento**: Documentar 35 anos de evolução do sistema
2. **Migração Tecnológica**: Base para migração para arquitetura moderna
3. **Conformidade**: Atender requisitos regulatórios e de auditoria
4. **Treinamento**: Material de referência para novos desenvolvedores
5. **Manutenção**: Guia para correções e evoluções

### Como Usar Esta Documentação

#### Para Desenvolvedores
1. Comece pelo [Sumário Executivo](01-executive-summary.md)
2. Estude a [Arquitetura](02-architecture.md)
3. Consulte as [Regras de Negócio](05-business-logic.md)
4. Use o [Glossário](10-glossary.md) para termos

#### Para Analistas de Negócio
1. Leia o [Sumário Executivo](01-executive-summary.md)
2. Foque nas [Regras de Negócio](05-business-logic.md)
3. Revise as [Telas](08-ui-screens.md)
4. Consulte o [Glossário](10-glossary.md)

#### Para Equipe de Migração
1. Estude todo o conjunto de documentos
2. Foque no [Guia de Migração](09-migration-guide.md)
3. Use as [Estruturas de Dados](03-data-structures.md) para mapeamento
4. Valide com as [Regras de Negócio](05-business-logic.md)

#### Para Operações
1. Use o [Guia de Operações](07-operations-guide.md)
2. Consulte [Integrações](06-external-integrations.md) para troubleshooting
3. Revise a [Arquitetura](02-architecture.md) para infraestrutura

### Documentos Relacionados

- `LEGACY_SIWEA_COMPLETE_ANALYSIS.md` - Análise técnica detalhada (1.725 linhas)
- `BUSINESS_RULES_INDEX.md` - Índice de regras de negócio
- `SISTEMA_LEGADO_*.md` - Documentação modular original
- `#SIWEA-V116.esf` - Código fonte original

### Manutenção da Documentação

| Aspecto | Responsável | Frequência |
|---------|-------------|------------|
| Atualização Técnica | Equipe Desenvolvimento | Mensal |
| Revisão de Negócio | Analistas | Trimestral |
| Validação | Arquitetura | Semestral |
| Aprovação | Gestão TI | Anual |

### Controle de Versão

| Versão | Data | Autor | Mudanças |
|--------|------|-------|----------|
| 1.0 | 27/10/2025 | Equipe Migração | Documentação inicial completa |

### Contato e Suporte

Para questões sobre esta documentação:

- **Email**: migracao-siwea@caixaseguradora.com.br
- **Wiki**: http://wiki.caixaseguradora.com.br/siwea
- **Teams**: Canal #migração-siwea

---

**CONFIDENCIAL** - Esta documentação contém informações proprietárias da Caixa Seguradora.

*Última Atualização: 27 de outubro de 2025*
