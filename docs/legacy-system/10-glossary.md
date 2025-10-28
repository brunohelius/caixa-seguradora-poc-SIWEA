# 10 - Glossário: Sistema Legado SIWEA

[← Voltar ao Índice](README.md)

---

## Termos de Negócio

| Termo | Sigla | Definição |
|-------|-------|-----------|
| **Sinistro** | - | Evento coberto pela apólice de seguro que gera direito à indenização |
| **Protocolo** | PROT | Identificador único do processo de sinistro no formato XXX/NNNNNNN-D |
| **Reserva Esperada** | SDOPAG | Valor total previsto para pagamento do sinistro |
| **Total Pago** | TOTPAG | Soma de todos os pagamentos já realizados |
| **Saldo Pendente** | - | Diferença entre reserva esperada e total pago |
| **Beneficiário** | NOMFAV | Pessoa ou entidade que recebe a indenização |
| **Ramo** | RMO | Categoria do seguro (ex: automóvel, vida, residencial) |
| **Líder** | - | Seguradora principal em operações de cosseguro |
| **Apólice** | APO | Contrato de seguro entre segurado e seguradora |
| **Indenização** | - | Valor pago ao beneficiário pela ocorrência do sinistro |

---

## Termos Técnicos

| Termo | Sigla | Definição |
|-------|-------|-----------|
| **SIWEA** | - | Sistema de Indenização e Workflow de Eventos Atendidos |
| **CICS** | - | Customer Information Control System (IBM transaction manager) |
| **DB2** | - | Database 2 (Sistema gerenciador de banco de dados IBM) |
| **COBOL** | - | Common Business Oriented Language |
| **EZEE** | - | IBM VisualAge EZEE (ferramenta RAD para COBOL) |
| **RACF** | - | Resource Access Control Facility (segurança mainframe) |
| **JCL** | - | Job Control Language |
| **VSAM** | - | Virtual Storage Access Method |
| **TSO** | - | Time Sharing Option |
| **ISPF** | - | Interactive System Productivity Facility |
| **3270** | - | Protocolo de terminal IBM para mainframe |
| **EBCDIC** | - | Extended Binary Coded Decimal Interchange Code |
| **MQ Series** | MQ | IBM Message Queue middleware |

---

## Campos do Sistema

| Campo | Nome Completo | Descrição |
|-------|---------------|-----------|
| **TIPSEG** | Tipo de Seguro | Classificação do tipo de seguro |
| **ORGSIN** | Origem Sinistro | Código da origem do sinistro |
| **RMOSIN** | Ramo Sinistro | Código do ramo de seguro |
| **NUMSIN** | Número Sinistro | Número sequencial do sinistro |
| **FONTE** | Fonte Protocolo | Origem do protocolo (sistema/canal) |
| **PROTSINI** | Protocolo Sinistro | Número do protocolo |
| **DAC** | Dígito Auto-Conferência | Dígito verificador do protocolo |
| **ORGAPO** | Origem Apólice | Origem da apólice de seguro |
| **RMOAPO** | Ramo Apólice | Ramo da apólice |
| **NUMAPOL** | Número Apólice | Número da apólice |
| **CODPRODU** | Código Produto | Código do produto de seguro |
| **TIPREG** | Tipo Registro | Tipo de registro (1=Individual, 2=Coletivo) |
| **TPSEGU** | Tipo Segurado | Tipo de segurado |
| **CODLIDER** | Código Líder | Código da seguradora líder |
| **SINLID** | Sinistro Líder | Número do sinistro na líder |
| **OCORHIST** | Ocorrência Histórico | Contador sequencial de eventos |

---

## Valores Financeiros

| Campo | Nome | Descrição |
|-------|------|-----------|
| **VALPRI** | Valor Principal | Valor principal da indenização |
| **CRRMON** | Correção Monetária | Valor de correção monetária |
| **VALPRIBT** | Valor Principal BTNF | Valor principal convertido para BTNF |
| **CRRMONBT** | Correção BTNF | Correção convertida para BTNF |
| **VALTOTBT** | Valor Total BTNF | Soma total em BTNF |
| **BTNF** | - | Bônus do Tesouro Nacional Fiscal (moeda padronizada) |

---

## Códigos de Status

| Código | Status | Descrição |
|--------|--------|-----------|
| **AT** | Ativo | Sinistro ativo, pode receber pagamentos |
| **EN** | Encerrado | Sinistro finalizado, sem mais pagamentos |
| **CA** | Cancelado | Sinistro cancelado |
| **PE** | Pendente | Aguardando documentação ou análise |
| **AP** | Aprovado | Pagamento aprovado |
| **RJ** | Rejeitado | Pagamento rejeitado |
| **0** | Inicial | Status inicial padrão |
| **1** | Processado | Já processado pelo sistema |

---

## Tipos de Pagamento

| Código | Tipo | Descrição |
|--------|------|-----------|
| **1** | Total | Pagamento integral do sinistro |
| **2** | Parcial | Pagamento de parte do valor |
| **3** | Complementar | Complemento de pagamento anterior |
| **4** | Ajuste | Ajuste de valores |
| **5** | Recalculado | Pagamento recalculado |

---

## Integrações Externas

| Sistema | Sigla | Função |
|---------|-------|--------|
| **CNOUA** | - | Sistema de validação de produtos de consórcio |
| **SIPUA** | - | Sistema de validação de contratos EFP |
| **SIMDA** | - | Sistema de validação de contratos HB |

---

## Fases do Workflow

| Código | Fase | Descrição |
|--------|------|-----------|
| **001** | Abertura | Abertura inicial do sinistro |
| **002** | Análise Documentação | Verificação de documentos |
| **003** | Autorização Pagamento | Em processo de autorização |
| **004** | Pagamento Realizado | Pagamento efetuado |
| **005** | Aguardando Comprovação | Esperando comprovantes |
| **006** | Comprovação Recebida | Documentos recebidos |
| **007** | Análise Final | Análise para encerramento |
| **008** | Encerramento | Sinistro encerrado |

---

## Mensagens de Erro

| Código | Severidade | Significado |
|--------|------------|-------------|
| **E0001** | Erro | Sinistro não encontrado |
| **E0002** | Erro | Tipo de pagamento inválido |
| **E0003** | Erro | Valor principal obrigatório |
| **E0004** | Erro | Beneficiário obrigatório |
| **E0010** | Erro | Falha na integração externa |
| **E0011** | Erro | Timeout na validação |
| **E0020** | Erro | Usuário sem permissão |
| **E0021** | Erro | Valor excede alçada |

---

## Códigos de Retorno

| Código | Tipo | Significado |
|--------|------|-------------|
| **0** | Sucesso | Operação completada com sucesso |
| **4** | Aviso | Operação com avisos |
| **8** | Erro | Erro recuperável |
| **12** | Fatal | Erro fatal, abort necessário |
| **100** | SQL | Registro não encontrado |
| **-803** | SQL | Violação de chave única |
| **-911** | SQL | Deadlock detectado |

---

## Acrônimos do Projeto

| Sigla | Significado |
|-------|-------------|
| **POC** | Proof of Concept |
| **MVP** | Minimum Viable Product |
| **SLA** | Service Level Agreement |
| **KPI** | Key Performance Indicator |
| **ETL** | Extract, Transform, Load |
| **DR** | Disaster Recovery |
| **UAT** | User Acceptance Testing |
| **ROI** | Return on Investment |
| **TCO** | Total Cost of Ownership |
| **GDPR** | General Data Protection Regulation |
| **LGPD** | Lei Geral de Proteção de Dados |

---

## Conversões e Equivalências

| De | Para | Fator/Método |
|----|------|--------------|
| EBCDIC | UTF-8 | Tabela de conversão |
| COMP-3 | Decimal | Unpack + conversão |
| Julian Date | DateTime | Cálculo de dias |
| 9999-12-31 | NULL | Data infinita → NULL |
| Spaces | NULL | Brancos → NULL |
| Low-values | 0 | Zeros binários |

---

*Este glossário contém todos os termos utilizados no sistema SIWEA.*

**Última Atualização:** 27/10/2025
