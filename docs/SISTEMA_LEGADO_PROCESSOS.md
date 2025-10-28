# SIWEA - Processos e Workflows do Sistema Legado

**Documento:** Processos de Negócio e Fluxos de Trabalho
**Versão:** 1.0
**Data:** 2025-10-27
**Documento:** 4 de 5
**Referência:** SISTEMA_LEGADO_VISAO_GERAL.md, SISTEMA_LEGADO_REGRAS_NEGOCIO.md, SISTEMA_LEGADO_ARQUITETURA.md

---

## Índice

1. [Introdução](#introdução)
2. [Processo Principal: Autorização de Pagamento](#processo-principal-autorização-de-pagamento)
3. [Subprocesso: Busca de Sinistros](#subprocesso-busca-de-sinistros)
4. [Subprocesso: Validação de Produtos](#subprocesso-validação-de-produtos)
5. [Subprocesso: Conversão Monetária](#subprocesso-conversão-monetária)
6. [Subprocesso: Gestão de Fases](#subprocesso-gestão-de-fases)
7. [Subprocesso: Registro de Transações](#subprocesso-registro-de-transações)
8. [Fluxos de Integração Externa](#fluxos-de-integração-externa)
9. [Tratamento de Erros e Rollback](#tratamento-de-erros-e-rollback)
10. [Diagramas de Sequência](#diagramas-de-sequência)
11. [Máquinas de Estado](#máquinas-de-estado)

---

## Introdução

### Propósito deste Documento

Este documento detalha os **processos de negócio end-to-end** do sistema SIWEA, incluindo:
- Fluxogramas completos para cada processo principal
- Diagramas de sequência mostrando interações entre componentes
- Máquinas de estado para fases de processamento
- Árvores de decisão para roteamento de produtos
- Fluxos de tratamento de erros e recuperação

### Como Ler os Diagramas

**Notação de Fluxogramas:**
```
┌────────┐     Retângulo = Processo/Ação
│ Ação   │
└────────┘

◇──────◇      Losango = Decisão (Sim/Não)
│ Teste │
◇──────◇

(  Início  )  Elipse = Início/Fim
(   Fim    )

[  Dados  ]   Paralelogramo = Entrada/Saída de Dados
```

**Notação de Sequência:**
```
Terminal → CICS:     Mensagem síncrona
CICS -->> DB2:       Resposta
CICS -x- CNOUA:      Chamada com timeout
```

---

## Processo Principal: Autorização de Pagamento

### Visão Geral

**Entradas:**
- Protocolo/Sinistro já localizado via busca
- Dados de pagamento: Tipo, Valor Principal, Valor Correção, Beneficiário

**Saídas:**
- Autorização registrada no histórico (THISTSIN)
- Saldo atualizado no sinistro mestre (TMESTSIN)
- Fase atualizada (SI_SINISTRO_FASE)
- Acompanhamento registrado (SI_ACOMPANHA_SINI)

**Duração Esperada:** < 90 segundos (incluindo validações externas)

### Fluxograma Completo

```
                    ( INÍCIO )
                        |
                        v
            ┌───────────────────────┐
            │ Exibir Tela SIHM020   │
            │ com dados do sinistro │
            └───────────┬───────────┘
                        |
                        v
            ┌───────────────────────┐
            │ Operador preenche:    │
            │ - Tipo Pagamento      │
            │ - Tipo Apólice        │
            │ - Valor Principal     │
            │ - Valor Correção      │
            │ - Beneficiário        │
            └───────────┬───────────┘
                        |
                        v
            ┌───────────────────────┐
            │ ENTER pressionado     │
            └───────────┬───────────┘
                        |
                        v
        ┌───────────────────────────────────┐
        │ PASSO 1: VALIDAÇÃO DE ENTRADA     │
        │                                   │
        │ ✓ Tipo Pagamento IN (1,2,3,4,5)  │
        │ ✓ Tipo Apólice IN (1,2)           │
        │ ✓ Valor Principal > 0             │
        │ ✓ SE TPSEGU != 0:                 │
        │     Beneficiário obrigatório      │
        └───────────┬───────────────────────┘
                    |
                    v
              ◇──────────◇
              │ Válido?  │
              ◇──────────◇
                 /      \
              NÃO        SIM
               |          |
               v          v
        [Exibir Erro] ┌──────────────────────┐
        [Manter Tela] │ PASSO 2: VALIDAÇÃO   │
               |       │ DE NEGÓCIO           │
               |       │                      │
               |<────  │ ✓ Valor <= Pendente  │
               |       │ ✓ Sinistro ativo     │
               |       └──────────┬───────────┘
               |                  |
               |                  v
               |            ◇──────────◇
               |            │ Válido?  │
               |            ◇──────────◇
               |               /      \
               |            NÃO        SIM
               |             |          |
               └─────────────┘          v
                              ┌──────────────────────┐
                              │ PASSO 3: OBTER DATA  │
                              │ NEGÓCIO E TAXA BTNF  │
                              │                      │
                              │ SELECT DTMOVABE      │
                              │ FROM TSISTEMA        │
                              │ WHERE IDSISTEM='SI'  │
                              │                      │
                              │ SELECT VLCRUZAD      │
                              │ FROM TGEUNIMO        │
                              │ WHERE DTINIVIG <=    │
                              │   DTMOVABE <=        │
                              │   DTTERVIG           │
                              └──────────┬───────────┘
                                         |
                                         v
                              ┌──────────────────────┐
                              │ PASSO 4: CALCULAR    │
                              │ VALORES BTNF         │
                              │                      │
                              │ VALPRIBT =           │
                              │   VALPRI × VLCRUZAD  │
                              │ CRRMONBT =           │
                              │   CRRMON × VLCRUZAD  │
                              │ VALTOTBT =           │
                              │   VALPRIBT+CRRMONBT  │
                              └──────────┬───────────┘
                                         |
                                         v
                              ┌──────────────────────┐
                              │ PASSO 5: VALIDAÇÃO   │
                              │ EXTERNA              │
                              │                      │
                              │ [Ver Subprocesso     │
                              │  Validação Produtos] │
                              └──────────┬───────────┘
                                         |
                                         v
                              ◇──────────────────◇
                              │ Validação OK?    │
                              │ (EZERT8='000..') │
                              ◇──────────────────◇
                                 /            \
                              NÃO              SIM
                               |                |
                               v                v
                    [Exibir Erro Serviço]  ┌──────────────┐
                    [ROLLBACK]             │ PASSO 6:     │
                    [Manter Tela]          │ BEGIN TRANS  │
                               |            └──────┬───────┘
                               |                   |
                               |<──────────────────┘
                               |           (em caso de erro)
                               |                   |
                               |                   v
                               |        ┌──────────────────────┐
                               |        │ PASSO 7: INSERT      │
                               |        │ THISTSIN             │
                               |        │                      │
                               |        │ OCORHIST =           │
                               |        │  TMESTSIN.OCORHIST+1 │
                               |        │ OPERACAO = 1098      │
                               |        │ TIPCRR = '5'         │
                               |        │ DTMOVTO = DTMOVABE   │
                               |        │ EZEUSRID = {user}    │
                               |        └──────────┬───────────┘
                               |                   |
                               |<──────────────────┘
                               |           (em caso de erro)
                               |                   |
                               |                   v
                               |        ┌──────────────────────┐
                               |        │ PASSO 8: UPDATE      │
                               |        │ TMESTSIN             │
                               |        │                      │
                               |        │ SET TOTPAG =         │
                               |        │   TOTPAG + VALTOTBT  │
                               |        │ SET OCORHIST =       │
                               |        │   OCORHIST + 1       │
                               |        └──────────┬───────────┘
                               |                   |
                               |<──────────────────┘
                               |           (em caso de erro)
                               |                   |
                               |                   v
                               |        ┌──────────────────────┐
                               |        │ PASSO 9: INSERT      │
                               |        │ SI_ACOMPANHA_SINI    │
                               |        │                      │
                               |        │ COD_EVENTO = 1098    │
                               |        │ DATA_MOVTO = hoje    │
                               |        │ NUM_OCORR = {new}    │
                               |        └──────────┬───────────┘
                               |                   |
                               |<──────────────────┘
                               |           (em caso de erro)
                               |                   |
                               |                   v
                               |        ┌──────────────────────┐
                               |        │ PASSO 10: ATUALIZAR  │
                               |        │ FASES                │
                               |        │                      │
                               |        │ [Ver Subprocesso     │
                               |        │  Gestão de Fases]    │
                               |        └──────────┬───────────┘
                               |                   |
                               |<──────────────────┘
                               |           (em caso de erro)
                               |                   |
                               |                   v
                               |              ◇─────────◇
                               |              │ Erros?  │
                               |              ◇─────────◇
                               |                /     \
                               |             SIM      NÃO
                               |              |        |
                               └──────────────┘        v
                                                  ┌──────────┐
                                                  │ COMMIT   │
                                                  └────┬─────┘
                                                       |
                                                       v
                                            ┌──────────────────┐
                                            │ Exibir Mensagem: │
                                            │ "PAGAMENTO       │
                                            │  AUTORIZADO COM  │
                                            │  SUCESSO"        │
                                            └────────┬─────────┘
                                                     |
                                                     v
                                                 ( FIM )
```

### Pontos Críticos de Controle

**Checkpoint 1 - Validação de Entrada:** Se falhar, retornar erro sem acessar banco
**Checkpoint 2 - Validação de Negócio:** Verificar saldo pendente antes de prosseguir
**Checkpoint 3 - Validação Externa:** Timeout 10s, falha interrompe transação
**Checkpoint 4 - Transação ACID:** Qualquer erro nos passos 7-10 resulta em ROLLBACK total

---

## Subprocesso: Busca de Sinistros

### Fluxograma de Busca

```
              ( INÍCIO - Tela SI11M010 )
                        |
                        v
          ┌─────────────────────────────┐
          │ Operador preenche critérios │
          │ e pressiona ENTER           │
          └─────────────┬───────────────┘
                        |
                        v
          ┌─────────────────────────────┐
          │ Determinar critério ativo   │
          │ (protocolo, sinistro, líder)│
          └─────────────┬───────────────┘
                        |
                        v
            ◇────────────────────◇
            │ Protocolo completo? │
            ◇────────────────────◇
               /              \
             SIM              NÃO
              |                |
              v                v
    ┌──────────────────┐  ◇────────────────────◇
    │ Query Protocolo: │  │ Sinistro completo?  │
    │                  │  ◇────────────────────◇
    │ SELECT *         │     /              \
    │ FROM TMESTSIN    │   SIM              NÃO
    │ WHERE FONTE = X  │    |                |
    │   AND PROTSINI=Y │    v                v
    │   AND DAC = Z    │  ┌──────────────┐  ◇──────────────◇
    └────────┬─────────┘  │ Query Num:   │  │ Líder comp?  │
             |             │              │  ◇──────────────◇
             |             │ SELECT *     │    /         \
             |             │ FROM TMESTSIN│  SIM         NÃO
             |             │ WHERE ORGSIN │   |           |
             |             │   AND RMOSIN │   v           v
             |             │   AND NUMSIN │ ┌────────┐ [Erro]
             |             └──────┬───────┘ │ Query  │ [Critério]
             |                    |         │ Líder  │ [Inválido]
             |                    |         └───┬────┘
             └────────────────────┴─────────────┘
                                  |
                                  v
                         ◇────────────────◇
                         │ Registro       │
                         │ encontrado?    │
                         ◇────────────────◇
                            /          \
                         SIM            NÃO
                          |              |
                          v              v
              ┌───────────────────┐   [Exibir Erro]
              │ Carregar dados    │   ["PROTOCOLO XXX"]
              │ complementares:   │   ["NAO ENCONTRADO"]
              │                   │        |
              │ - TGERAMO (ramo)  │        |
              │ - TAPOLICE (seg.) │        |
              │ - Calcular saldo  │        |
              │   pendente        │        |
              └─────────┬─────────┘        |
                        |                  |
                        v                  |
              ┌───────────────────┐        |
              │ Exibir tela       │        |
              │ SIHM020 com dados │        |
              │ completos         │        |
              └─────────┬─────────┘        |
                        |                  |
                        v                  v
                    ( FIM )            ( FIM )
```

### Queries de Busca SQL

**Modo 1: Por Protocolo**
```sql
SELECT * FROM TMESTSIN
WHERE FONTE = :fonte
  AND PROTSINI = :protsini
  AND DAC = :dac
```

**Modo 2: Por Número do Sinistro**
```sql
SELECT * FROM TMESTSIN
WHERE ORGSIN = :orgsin
  AND RMOSIN = :rmosin
  AND NUMSIN = :numsin
```

**Modo 3: Por Código Líder**
```sql
SELECT * FROM TMESTSIN
WHERE CODLIDER = :codlider
  AND SINLID = :sinlid
```

### Dados Complementares Carregados

Após localizar registro principal em TMESTSIN:

**1. Nome do Ramo (Branch)**
```sql
SELECT NOMERAMO
FROM TGERAMO
WHERE CODRAMO = :rmosin
```

**2. Nome do Segurado (Insured Party)**
```sql
SELECT NOME
FROM TAPOLICE
WHERE ORGAPOL = :orgapo
  AND RMOAPOL = :rmoapo
  AND NUMAPOL = :numapol
```

**3. Saldo Pendente (Calculado)**
```
Saldo Pendente = SDOPAG - TOTPAG
```

---

## Subprocesso: Validação de Produtos

### Árvore de Decisão para Roteamento

```
                    ( INÍCIO )
                        |
                        v
          ┌─────────────────────────────┐
          │ Obter CODPRODU do sinistro  │
          │ (TMESTSIN.CODPRODU)         │
          └─────────────┬───────────────┘
                        |
                        v
            ◇───────────────────────◇
            │ CODPRODU IN           │
            │ (6814, 7701, 7709)?   │
            ◇───────────────────────◇
               /                  \
             SIM                  NÃO
              |                    |
              v                    v
    ┌──────────────────┐   ┌──────────────────────┐
    │ ROTA: CNOUA      │   │ Query EF_CONTR_SEG_  │
    │ (Consórcio)      │   │ HABIT para apólice   │
    │                  │   │                      │
    │ Chamada REST:    │   │ SELECT NUM_CONTRATO  │
    │ POST /validate   │   │ FROM EF_CONTR_SEG_   │
    │                  │   │ HABIT                │
    │ Body:            │   │ WHERE ORGAPOL=X      │
    │ {                │   │   AND RMOAPOL=Y      │
    │   claimId,       │   │   AND NUMAPOL=Z      │
    │   productCode,   │   └──────────┬───────────┘
    │   amount,        │              |
    │   contract       │              v
    │ }                │     ◇────────────────────◇
    │                  │     │ NUM_CONTRATO > 0?  │
    │ Timeout: 10s     │     ◇────────────────────◇
    │ Retry: 2×        │        /              \
    └────────┬─────────┘      SIM              NÃO
             |                 |                |
             |                 v                v
             |      ┌──────────────────┐  ┌──────────────┐
             |      │ ROTA: SIPUA      │  │ ROTA: SIMDA  │
             |      │ (EFP Contracts)  │  │ (HB Contr.)  │
             |      │                  │  │              │
             |      │ Chamada SOAP:    │  │ Chamada SOAP │
             |      │ validatePayment  │  │ validatePmt  │
             |      │                  │  │              │
             |      │ Timeout: 10s     │  │ Timeout: 10s │
             |      │ Retry: 2×        │  │ Retry: 2×    │
             |      └────────┬─────────┘  └──────┬───────┘
             |               |                   |
             └───────────────┴───────────────────┘
                             |
                             v
                  ┌──────────────────────┐
                  │ Aguardar resposta    │
                  │ (máx 10 segundos)    │
                  └──────────┬───────────┘
                             |
                             v
                  ◇──────────────────────◇
                  │ EZERT8 = '00000000'? │
                  ◇──────────────────────◇
                     /                \
                   SIM                NÃO
                    |                  |
                    v                  v
          ┌──────────────────┐  [Exibir Erro Específico]
          │ Validação OK     │  [Código: EZERT8]
          │ Continuar fluxo  │  [Mensagem do serviço]
          └────────┬─────────┘  [ABORTAR transação]
                   |                  |
                   v                  v
               ( FIM )            ( FIM )
```

### Especificações de Chamadas

#### CNOUA (REST API)

**Endpoint:** `POST https://api.caixa.gov.br/cnoua/v1/validate`

**Headers:**
```
Content-Type: application/json
Authorization: Bearer {token}
X-Request-ID: {uuid}
```

**Request Body:**
```json
{
  "claimId": 12345,
  "protocolNumber": "001/0123456-7",
  "productCode": "6814",
  "policyNumber": "001/0123456",
  "contractNumber": "CON-2024-001",
  "amount": 25000.00,
  "currencyCode": "BRL",
  "beneficiary": "João da Silva",
  "transactionDate": "2024-10-27"
}
```

**Success Response (200):**
```json
{
  "EZERT8": "00000000",
  "validationId": "VAL-20241027-001",
  "approved": true,
  "message": "Validação bem-sucedida"
}
```

**Error Response (200 with error code):**
```json
{
  "EZERT8": "EZERT8001",
  "approved": false,
  "errorCode": "INVALID_CONTRACT",
  "message": "Contrato de consórcio inválido"
}
```

**Timeout:** 10 segundos
**Retries:** 2 tentativas com backoff exponencial (1s, 2s)

#### SIPUA (SOAP Web Service)

**Endpoint:** `https://sipua.caixa.gov.br/services/PaymentValidation?wsdl`

**SOAP Envelope:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope
    xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
    xmlns:pay="http://caixa.gov.br/sipua/payment/v1">
  <soapenv:Header>
    <SessionId>abc123xyz</SessionId>
  </soapenv:Header>
  <soapenv:Body>
    <pay:validatePaymentRequest>
      <pay:policyNumber>001/0123456</pay:policyNumber>
      <pay:contractNumber>12345</pay:contractNumber>
      <pay:amount>25000.00</pay:amount>
      <pay:transactionDate>2024-10-27</pay:transactionDate>
    </pay:validatePaymentRequest>
  </soapenv:Body>
</soapenv:Envelope>
```

**Success Response:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<soapenv:Envelope>
  <soapenv:Body>
    <pay:validatePaymentResponse>
      <pay:EZERT8>00000000</pay:EZERT8>
      <pay:approved>true</pay:approved>
      <pay:validationId>SIPUA-20241027-001</pay:validationId>
    </pay:validatePaymentResponse>
  </soapenv:Body>
</soapenv:Envelope>
```

**Timeout:** 10 segundos
**Retries:** 2 tentativas

#### SIMDA (SOAP Web Service)

**Endpoint:** `https://simda.caixa.gov.br/services/HBValidation?wsdl`

**SOAP Envelope:** (similar a SIPUA)

**Diferenças:**
- Usado quando NUM_CONTRATO = 0 ou NULL
- Namespace: `http://caixa.gov.br/simda/hb/v1`

---

## Subprocesso: Conversão Monetária

### Fluxograma de Conversão BTNF

```
                ( INÍCIO )
                    |
                    v
      ┌─────────────────────────────┐
      │ Obter data de negócio       │
      │                             │
      │ SELECT DTMOVABE             │
      │ FROM TSISTEMA               │
      │ WHERE IDSISTEM = 'SI'       │
      └─────────────┬───────────────┘
                    |
                    v
      ┌─────────────────────────────┐
      │ Buscar taxa de conversão    │
      │                             │
      │ SELECT VLCRUZAD             │
      │ FROM TGEUNIMO               │
      │ WHERE DTINIVIG <= DTMOVABE  │
      │   AND DTMOVABE <= DTTERVIG  │
      └─────────────┬───────────────┘
                    |
                    v
          ◇─────────────────◇
          │ Taxa encontrada? │
          ◇─────────────────◇
             /           \
           SIM           NÃO
            |             |
            v             v
  ┌──────────────────┐  [Erro]
  │ Aplicar fórmulas │  ["Taxa de conversão"]
  │                  │  ["não disponível para"]
  │ VALPRIBT =       │  ["a data de negócio"]
  │  VALPRI ×        │       |
  │  VLCRUZAD        │       v
  │                  │   ( FIM )
  │ CRRMONBT =       │
  │  CRRMON ×        │
  │  VLCRUZAD        │
  │                  │
  │ VALTOTBT =       │
  │  VALPRIBT +      │
  │  CRRMONBT        │
  └────────┬─────────┘
           |
           v
  ┌──────────────────┐
  │ Arredondar para  │
  │ 2 casas decimais │
  │                  │
  │ Usar Banker's    │
  │ Rounding         │
  │ (MidpointRounding│
  │  .ToEven)        │
  └────────┬─────────┘
           |
           v
       ( FIM )
```

### Fórmulas de Conversão

**Entrada:**
- `VALPRI`: Valor principal (moeda original)
- `CRRMON`: Valor de correção (moeda original)
- `VLCRUZAD`: Taxa de conversão BTNF (8 decimais)

**Cálculo:**
```
VALPRIBT = VALPRI × VLCRUZAD
CRRMONBT = CRRMON × VLCRUZAD
VALTOTBT = VALPRIBT + CRRMONBT
```

**Precisão:**
- Entrada: DECIMAL(15,2)
- Taxa: DECIMAL(18,8)
- Cálculo intermediário: DECIMAL(33,10)
- Resultado final: DECIMAL(15,2) após arredondamento

**Arredondamento:**
- Método: Banker's Rounding (IEEE 754)
- Regra: Arredondar para o número par mais próximo quando exatamente no meio
- Exemplos:
  - 12.345 → 12.34 (arredonda para baixo, 4 é par)
  - 12.355 → 12.36 (arredonda para cima, 6 é par)
  - 12.365 → 12.36 (arredonda para baixo, 6 é par)

### Exemplo de Conversão

**Dados de Entrada:**
```
VALPRI = 10000.00 (BRL)
CRRMON = 500.00 (BRL)
VLCRUZAD = 1.23456789 (taxa BTNF do dia)
```

**Cálculo Passo a Passo:**
```
1. VALPRIBT = 10000.00 × 1.23456789
             = 12345.67890000
             = 12345.68 (arredondado)

2. CRRMONBT = 500.00 × 1.23456789
            = 617.28394500
            = 617.28 (arredondado)

3. VALTOTBT = 12345.68 + 617.28
            = 12962.96
```

**Valores Gravados:**
```
THISTSIN.VALPRIBT = 12345.68
THISTSIN.CRRMONBT = 617.28
THISTSIN.VALTOTBT = 12962.96
TMESTSIN.TOTPAG += 12962.96
```

---

## Subprocesso: Gestão de Fases

### Máquina de Estados de Fases

```
                    ┌──────────────────────────┐
                    │ CONFIGURAÇÃO DE FASES    │
                    │ (SI_REL_FASE_EVENTO)     │
                    │                          │
                    │ Para evento 1098:        │
                    │ - Abrir fase 10          │
                    │ - Fechar fase 5          │
                    └────────────┬─────────────┘
                                 |
                                 v
        ┌────────────────────────────────────────┐
        │ FASE ABERTA                            │
        │ (SI_SINISTRO_FASE)                     │
        │                                        │
        │ COD_FASE = X                           │
        │ COD_EVENTO = 1098                      │
        │ DATA_ABERT_SIFA = HOJE                 │
        │ DATA_FECHA_SIFA = '9999-12-31' (sentinela)│
        │ STATUS = 'ABERTA'                      │
        └────────────┬───────────────────────────┘
                     |
                     | (Evento de fechamento)
                     |
                     v
        ┌────────────────────────────────────────┐
        │ FASE FECHADA                           │
        │                                        │
        │ COD_FASE = X                           │
        │ COD_EVENTO = {novo evento}             │
        │ DATA_ABERT_SIFA = {data original}      │
        │ DATA_FECHA_SIFA = HOJE (atualizado)    │
        │ STATUS = 'FECHADA'                     │
        └────────────────────────────────────────┘
```

### Fluxograma de Atualização de Fases

```
                    ( INÍCIO )
                        |
                        v
          ┌─────────────────────────────┐
          │ Consultar configuração:     │
          │                             │
          │ SELECT COD_FASE,            │
          │        IND_ALTERACAO_FASE   │
          │ FROM SI_REL_FASE_EVENTO     │
          │ WHERE COD_EVENTO = 1098     │
          │   AND DATA_INIVIG_REFAEV <= │
          │       DTMOVABE <=            │
          │       DATA_TERVIG_REFAEV    │
          └─────────────┬───────────────┘
                        |
                        v
          ┌─────────────────────────────┐
          │ Para cada fase retornada:   │
          └─────────────┬───────────────┘
                        |
                        v
            ◇───────────────────────◇
            │ IND_ALTERACAO_FASE = │
            │        '1'?          │
            ◇───────────────────────◇
               /                  \
             SIM                  NÃO
              |                    |
              v                    v
    ┌──────────────────┐  ◇────────────────◇
    │ ABRIR FASE       │  │ = '2'?         │
    │                  │  ◇────────────────◇
    │ Verificar se já  │     /
    │ existe fase      │   SIM
    │ aberta:          │    |
    │                  │    v
    │ SELECT COUNT(*)  │  ┌──────────────────┐
    │ FROM SI_SINISTRO │  │ FECHAR FASE      │
    │ _FASE            │  │                  │
    │ WHERE FONTE=X    │  │ UPDATE SI_SINISTRO│
    │   AND PROTSINI=Y │  │ _FASE            │
    │   AND DAC=Z      │  │ SET              │
    │   AND COD_FASE=F │  │  DATA_FECHA_SIFA │
    │   AND COD_EVENTO │  │   = DTMOVABE     │
    │     = 1098       │  │ WHERE FONTE=X    │
    │   AND DATA_FECHA │  │   AND PROTSINI=Y │
    │     = '9999-12-31│  │   AND DAC=Z      │
    │                  │  │   AND COD_FASE=F │
    │ SE COUNT = 0:    │  │   AND DATA_FECHA │
    │   INSERT         │  │     = '9999-12-31│
    │     SI_SINISTRO_ │  │                  │
    │     FASE VALUES  │  │ Registrar LOG    │
    │     (FONTE, PROT,│  │ de fechamento    │
    │      DAC, FASE,  │  └──────────────────┘
    │      1098, HOJE, │
    │      '9999-12-31'│
    │     )            │
    │                  │
    │ SENÃO:           │
    │   LOG "Fase já   │
    │   aberta"        │
    └──────────────────┘
             |
             └─────────────────┬────────────────────┘
                               |
                               v
                  ┌────────────────────────┐
                  │ Registrar operações    │
                  │ em log de auditoria    │
                  └────────────┬───────────┘
                               |
                               v
                           ( FIM )
```

### Exemplo de Configuração de Fases

**Tabela SI_REL_FASE_EVENTO:**
```
COD_EVENTO | COD_FASE | IND_ALTERACAO_FASE | DATA_INIVIG_REFAEV
-----------|----------|--------------------|-----------------
1098       | 10       | '1' (ABRIR)        | 1900-01-01
1098       | 5        | '2' (FECHAR)       | 1900-01-01
```

**Interpretação:**
- Quando evento 1098 (autorização de pagamento) ocorrer:
  - Abrir fase 10 ("Pagamento Processando")
  - Fechar fase 5 ("Documentação Pendente")

**Resultado em SI_SINISTRO_FASE:**

Antes da autorização:
```
FONTE | PROTSINI | DAC | COD_FASE | COD_EVENTO | DATA_ABERT_SIFA | DATA_FECHA_SIFA
------|----------|-----|----------|------------|-----------------|----------------
001   | 0123456  | 7   | 5        | 1097       | 2024-10-20      | 9999-12-31
```

Após autorização (evento 1098):
```
FONTE | PROTSINI | DAC | COD_FASE | COD_EVENTO | DATA_ABERT_SIFA | DATA_FECHA_SIFA
------|----------|-----|----------|------------|-----------------|----------------
001   | 0123456  | 7   | 5        | 1097       | 2024-10-20      | 2024-10-27 ✓
001   | 0123456  | 7   | 10       | 1098       | 2024-10-27      | 9999-12-31 ✓
```

---

## Subprocesso: Registro de Transações

### Fluxograma ACID de 4 Tabelas

```
                    ( BEGIN TRANSACTION )
                            |
                            v
              ┌─────────────────────────────┐
              │ OPERAÇÃO 1:                 │
              │ INSERT INTO THISTSIN        │
              │                             │
              │ Campos obrigatórios:        │
              │ - TIPSEG, ORGSIN, RMOSIN,   │
              │   NUMSIN (chave composta)   │
              │ - OCORHIST (TMESTSIN+1)     │
              │ - OPERACAO = 1098           │
              │ - DTMOVTO = DTMOVABE        │
              │ - HORAOPER = NOW()          │
              │ - VALPRI, CRRMON, NOMFAV    │
              │ - TIPCRR = '5'              │
              │ - VALPRIBT, CRRMONBT,       │
              │   VALTOTBT                  │
              │ - SITCONTB = '0'            │
              │ - SITUACAO = '0'            │
              │ - EZEUSRID = {userid}       │
              └─────────────┬───────────────┘
                            |
                            v
                  ◇─────────────────◇
                  │ INSERT OK?      │
                  ◇─────────────────◇
                     /           \
                   SIM           NÃO
                    |             |
                    v             v
      ┌─────────────────────┐  [LOG ERRO]
      │ OPERAÇÃO 2:         │  [ROLLBACK]
      │ UPDATE TMESTSIN     │       |
      │                     │       v
      │ SET TOTPAG =        │   ( FIM )
      │   TOTPAG + VALTOTBT │
      │ SET OCORHIST =      │
      │   OCORHIST + 1      │
      │ WHERE TIPSEG=X      │
      │   AND ORGSIN=Y      │
      │   AND RMOSIN=Z      │
      │   AND NUMSIN=W      │
      └─────────┬───────────┘
                |
                v
      ◇─────────────────◇
      │ UPDATE OK?      │
      │ (1 row affected)│
      ◇─────────────────◇
         /           \
       SIM           NÃO
        |             |
        v             v
┌───────────────────┐  [LOG ERRO]
│ OPERAÇÃO 3:       │  [ROLLBACK]
│ INSERT INTO       │       |
│ SI_ACOMPANHA_SINI │       v
│                   │   ( FIM )
│ Campos:           │
│ - FONTE, PROTSINI,│
│   DAC             │
│ - COD_EVENTO=1098 │
│ - DATA_MOVTO=HOJE │
│ - NUM_OCORR={seq} │
│ - DESCR_COMPLEM   │
│ - COD_USUARIO     │
└─────────┬─────────┘
          |
          v
◇─────────────────◇
│ INSERT OK?      │
◇─────────────────◇
   /           \
 SIM           NÃO
  |             |
  v             v
┌──────────────────┐  [LOG ERRO]
│ OPERAÇÃO 4:      │  [ROLLBACK]
│ ATUALIZAR FASES  │       |
│                  │       v
│ (Subprocesso de  │   ( FIM )
│  Gestão de Fases)│
│                  │
│ 1. Consultar     │
│    SI_REL_FASE_  │
│    EVENTO        │
│ 2. Para cada fase│
│    a abrir:      │
│    INSERT        │
│ 3. Para cada fase│
│    a fechar:     │
│    UPDATE        │
└─────────┬────────┘
          |
          v
◇─────────────────◇
│ Fases OK?       │
◇─────────────────◇
   /           \
 SIM           NÃO
  |             |
  v             v
┌──────────┐  [LOG ERRO]
│ COMMIT   │  [ROLLBACK]
│          │       |
│ Gravar   │       v
│ todas as │   ( FIM )
│ mudanças │
└────┬─────┘
     |
     v
┌──────────────────┐
│ LOG SUCESSO      │
│ "Transação       │
│  confirmada"     │
└────┬─────────────┘
     |
     v
 ( FIM )
```

### Propriedades ACID Garantidas

**Atomicidade (Atomicity):**
- Todas as 4 operações devem ter sucesso, ou nenhuma é efetivada
- Qualquer erro em qualquer etapa resulta em ROLLBACK total
- Não existem estados intermediários persistidos

**Consistência (Consistency):**
- Chaves primárias e estrangeiras validadas
- Constraints verificados antes do COMMIT
- Saldo TMESTSIN.TOTPAG sempre consistente com soma de THISTSIN.VALTOTBT
- Sequência OCORHIST sempre incremental sem gaps

**Isolamento (Isolation):**
- Nível de isolamento: READ COMMITTED (DB2 padrão)
- Locks de linha aplicados em TMESTSIN durante UPDATE
- Previne dirty reads, non-repeatable reads
- Permite phantom reads (aceitável para este processo)

**Durabilidade (Durability):**
- Após COMMIT, mudanças sobrevivem a falhas do sistema
- WAL (Write-Ahead Logging) do DB2 garante persistência
- Logs de transação replicados para DR (Disaster Recovery)

### Tratamento de Deadlocks

**Detecção:**
```
DB2 Deadlock Detector (automático):
- Interval: 10 segundos
- Timeout: 30 segundos
```

**Resolução:**
```
SE SQLCODE = -911 (deadlock) ENTÃO
    ROLLBACK transação
    AGUARDAR 1 segundo (random backoff)
    RETRY operação (máx 3 tentativas)

    SE 3 tentativas falharam ENTÃO
        LOG erro crítico
        Exibir mensagem ao operador:
        "Sistema temporariamente indisponível"
    FIM SE
FIM SE
```

---

## Fluxos de Integração Externa

### Diagrama de Sequência: CNOUA (REST)

```
Operador    Tela      CICS        DB2        CNOUA API
  |          |         |           |             |
  |--ENTER-->|         |           |             |
  |          |-------->|           |             |
  |          |         |--SELECT-->|             |
  |          |         |<--dados---|             |
  |          |         |                         |
  |          |         |-----------POST /validate-->
  |          |         |           |             |
  |          |         |           |<--Processa--|
  |          |         |           |             |
  |          |         |<----------200 OK --------|
  |          |         |           |  {EZERT8}   |
  |          |         |           |             |
  |          |<--OK----|           |             |
  |<-Success-|         |           |             |
```

**Detalhes de Timeout:**
```
t=0s    : POST request enviado
t=0-10s : Aguardar resposta
t=10s   : Timeout - tentar retry 1
t=11s   : Retry 1 enviado
t=11-21s: Aguardar resposta retry 1
t=21s   : Timeout - tentar retry 2
t=22s   : Retry 2 enviado
t=22-32s: Aguardar resposta retry 2
t=32s   : Timeout final - ERRO exibido
```

### Diagrama de Sequência: SIPUA/SIMDA (SOAP)

```
Operador    Tela      CICS        DB2        SIPUA SOAP
  |          |         |           |             |
  |--ENTER-->|         |           |             |
  |          |-------->|           |             |
  |          |         |--SELECT-->|             |
  |          |         |  TMESTSIN |             |
  |          |         |<--dados---|             |
  |          |         |           |             |
  |          |         |--SELECT-->|             |
  |          |         | EF_CONTR  |             |
  |          |         |<--NUM_CTR-|             |
  |          |         |           |             |
  |          |         |-----SOAP Envelope------>|
  |          |         |    validatePayment      |
  |          |         |           |             |
  |          |         |           |<--Valida----|
  |          |         |           |             |
  |          |         |<----SOAP Response-------|
  |          |         |     {EZERT8}            |
  |          |         |           |             |
  |          |         |-BEGIN TRANS->           |
  |          |         |--INSERT-->|             |
  |          |         |--UPDATE-->|             |
  |          |         |-COMMIT--->|             |
  |          |         |           |             |
  |          |<-Success|           |             |
  |<-Mensagem|         |           |             |
```

### Circuit Breaker Pattern (Resiliência)

```
        ┌────────────────────────┐
        │ Estado: FECHADO        │
        │ (Operação Normal)      │
        │                        │
        │ Todas as chamadas      │
        │ passam normalmente     │
        └───────────┬────────────┘
                    |
         (5 falhas consecutivas)
                    |
                    v
        ┌────────────────────────┐
        │ Estado: ABERTO         │
        │ (Circuit Trip)         │
        │                        │
        │ Todas as chamadas      │
        │ retornam erro imediato │
        │ sem tentar serviço     │
        └───────────┬────────────┘
                    |
         (Após 60 segundos)
                    |
                    v
        ┌────────────────────────┐
        │ Estado: MEIO-ABERTO    │
        │ (Testing)              │
        │                        │
        │ Próxima chamada tenta  │
        │ serviço novamente      │
        └───────────┬────────────┘
                    |
              ┌─────┴─────┐
              |           |
          Sucesso      Falha
              |           |
              v           v
       [FECHADO]     [ABERTO]
```

**Configuração:**
```
Threshold de falhas: 5 consecutivas
Timeout do circuit: 60 segundos
Half-open test calls: 1 chamada
Reset após sucessos: 2 consecutivos
```

---

## Tratamento de Erros e Rollback

### Matriz de Erros e Ações

| Código Erro | Descrição | Categoria | Ação de Rollback | Mensagem ao Usuário |
|-------------|-----------|-----------|------------------|---------------------|
| **ERR-001** | Protocolo não encontrado | Validação | Não aplicável | "PROTOCOLO XXX-X NAO ENCONTRADO" |
| **ERR-002** | Sinistro não encontrado | Validação | Não aplicável | "SINISTRO XXXXXXX NAO ENCONTRADO" |
| **ERR-003** | Líder não encontrado | Validação | Não aplicável | "LIDER XXXXXXX-XXXXXXX NAO ENCONTRADO" |
| **ERR-004** | Valor excede saldo | Validação | Não aplicável | "VALOR EXCEDE SALDO PENDENTE" |
| **ERR-005** | Tipo pagamento inválido | Validação | Não aplicável | "TIPO DE PAGAMENTO DEVE SER 1, 2, 3, 4, OU 5" |
| **ERR-006** | Beneficiário obrigatório | Validação | Não aplicável | "BENEFICIARIO E OBRIGATORIO PARA ESTE TIPO DE SEGURO" |
| **ERR-007** | Taxa BTNF não disponível | Dados | Não aplicável | "TAXA DE CONVERSAO NAO DISPONIVEL PARA A DATA" |
| **ERR-008** | Validação CNOUA falhou | Integração | Sim - Não iniciar transação | "VALIDACAO CONSORCIO FALHOU: {EZERT8}" |
| **ERR-009** | Validação SIPUA falhou | Integração | Sim - Não iniciar transação | "VALIDACAO EFP FALHOU: {EZERT8}" |
| **ERR-010** | Validação SIMDA falhou | Integração | Sim - Não iniciar transação | "VALIDACAO HB FALHOU: {EZERT8}" |
| **ERR-011** | Timeout serviço externo | Integração | Sim - Não iniciar transação | "SERVICO DE VALIDACAO INDISPONIVEL" |
| **ERR-012** | Falha INSERT THISTSIN | Banco | Sim - ROLLBACK total | "ERRO AO REGISTRAR PAGAMENTO" |
| **ERR-013** | Falha UPDATE TMESTSIN | Banco | Sim - ROLLBACK total | "ERRO AO ATUALIZAR SALDO" |
| **ERR-014** | Falha INSERT SI_ACOMPANHA | Banco | Sim - ROLLBACK total | "ERRO AO REGISTRAR ACOMPANHAMENTO" |
| **ERR-015** | Falha UPDATE fases | Banco | Sim - ROLLBACK total | "ERRO AO ATUALIZAR FASES" |
| **ERR-016** | Deadlock detectado | Banco | Sim - ROLLBACK + RETRY | "TENTE NOVAMENTE EM INSTANTES" |
| **ERR-017** | Connection pool esgotado | Infraestrutura | Não aplicável | "SISTEMA TEMPORARIAMENTE INDISPONIVEL" |

### Fluxo de Rollback Detalhado

```
                ( ERRO DETECTADO )
                        |
                        v
              ┌─────────────────────┐
              │ LOG erro completo:  │
              │ - Timestamp         │
              │ - Código erro       │
              │ - Stack trace       │
              │ - Dados de contexto │
              │ - Usuário (EZEUSRID)│
              └─────────┬───────────┘
                        |
                        v
            ◇───────────────────────◇
            │ Transação já iniciada? │
            ◇───────────────────────◇
               /                  \
             SIM                  NÃO
              |                    |
              v                    v
    ┌──────────────────┐    ┌──────────────┐
    │ ROLLBACK:        │    │ Não precisa  │
    │                  │    │ rollback     │
    │ DB2 desfaz todas │    └──────┬───────┘
    │ as mudanças:     │           |
    │ - DELETE THISTSIN│           |
    │ - Restaura       │           |
    │   TOTPAG         │           |
    │ - Restaura       │           |
    │   OCORHIST       │           |
    │ - DELETE         │           |
    │   SI_ACOMPANHA   │           |
    │ - Restaura fases │           |
    │                  │           |
    │ Libera locks     │           |
    └─────────┬────────┘           |
              |                    |
              └────────┬───────────┘
                       |
                       v
            ┌──────────────────────┐
            │ Notificar operador:  │
            │                      │
            │ Exibir mensagem de   │
            │ erro apropriada      │
            │ (ver matriz acima)   │
            │                      │
            │ Destacar campos com  │
            │ problema (se aplicá- │
            │ vel)                 │
            └──────────┬───────────┘
                       |
                       v
            ┌──────────────────────┐
            │ Registrar no log de  │
            │ auditoria:           │
            │                      │
            │ - Tentativa de       │
            │   autorização falhou │
            │ - Razão da falha     │
            │ - Ação tomada        │
            │   (rollback)         │
            └──────────┬───────────┘
                       |
                       v
                  ( FIM )
```

---

## Diagramas de Sequência

### Sequência Completa: Busca + Autorização

```
Operador  Tela      CICS       DB2       CNOUA     SIPUA     SIMDA
  |        |         |          |          |         |         |
  |        |         |          |          |         |         |
  | (1) Buscar sinistro                    |         |         |
  |------->|         |          |          |         |         |
  |        |-------->|          |          |         |         |
  |        |  GET    |          |          |         |         |
  |        |         |--SELECT->|          |         |         |
  |        |         | TMESTSIN |          |         |         |
  |        |         |<--result-|          |         |         |
  |        |         |--SELECT->|          |         |         |
  |        |         | TGERAMO  |          |         |         |
  |        |         |<--nome---|          |         |         |
  |        |         |--SELECT->|          |         |         |
  |        |         | TAPOLICE |          |         |         |
  |        |         |<--segur.-|          |         |         |
  |        |<--dados-|          |          |         |         |
  |<--SIHM020        |          |          |         |         |
  |        |         |          |          |         |         |
  |        |         |          |          |         |         |
  | (2) Preencher dados de pagamento       |         |         |
  |        |         |          |          |         |         |
  | ENTER  |         |          |          |         |         |
  |------->|         |          |          |         |         |
  |        |-------->|          |          |         |         |
  |        |  POST   |          |          |         |         |
  |        |         |          |          |         |         |
  | (3) Validações                         |         |         |
  |        |         |--SELECT->|          |         |         |
  |        |         | TSISTEMA |          |         |         |
  |        |         |<-DTMOVABE|          |         |         |
  |        |         |--SELECT->|          |         |         |
  |        |         | TGEUNIMO |          |         |         |
  |        |         |<-VLCRUZAD|          |         |         |
  |        |         |          |          |         |         |
  | (4) Roteamento de produto              |         |         |
  |        |         |--SELECT->|          |         |         |
  |        |         | CODPRODU |          |         |         |
  |        |         |<--6814---|          |         |         |
  |        |         |          |          |         |         |
  |        |         |-----------POST /validate---->|         |
  |        |         |          |          |         |         |
  |        |         |          |<---------OK-------|         |
  |        |         |<----------{EZERT8='00...'}---|         |
  |        |         |          |          |         |         |
  | (5) Transação ACID                     |         |         |
  |        |         |-BEGIN--->|          |         |         |
  |        |         |          |          |         |         |
  |        |         |-INSERT-->|          |         |         |
  |        |         | THISTSIN |          |         |         |
  |        |         |<--OK-----|          |         |         |
  |        |         |          |          |         |         |
  |        |         |-UPDATE-->|          |         |         |
  |        |         | TMESTSIN |          |         |         |
  |        |         |<--OK-----|          |         |         |
  |        |         |          |          |         |         |
  |        |         |-INSERT-->|          |         |         |
  |        |         | SI_ACOMP |          |         |         |
  |        |         |<--OK-----|          |         |         |
  |        |         |          |          |         |         |
  |        |         |-UPDATE-->|          |         |         |
  |        |         | SI_FASES |          |         |         |
  |        |         |<--OK-----|          |         |         |
  |        |         |          |          |         |         |
  |        |         |-COMMIT-->|          |         |         |
  |        |         |<--OK-----|          |         |         |
  |        |         |          |          |         |         |
  |        |<-Success|          |          |         |         |
  |<--Mensagem       |          |          |         |         |
  | "PAGAMENTO       |          |          |         |         |
  |  AUTORIZADO"     |          |          |         |         |
```

**Tempo total estimado:**
- Busca (etapas 1-2): 1-2 segundos
- Validações (etapas 3-4): 3-5 segundos (inclui chamada CNOUA)
- Transação (etapa 5): 1-2 segundos
- **Total: 5-9 segundos** (dentro do limite de 90 segundos)

---

## Máquinas de Estado

### Estados do Sinistro

```
                    ┌─────────────┐
                    │   NOVO      │
                    │ (Registrado)│
                    └──────┬──────┘
                           |
                 (Documentação anexada)
                           |
                           v
                    ┌─────────────┐
                    │  PENDENTE   │
                    │(Aguardando  │
                    │  análise)   │
                    └──────┬──────┘
                           |
                    (Autorização)
                           |
                           v
                    ┌─────────────┐
            ┌───────│ EM PROCESSO │────────┐
            |       │ (Pagamento) │        |
            |       └──────┬──────┘        |
            |              |               |
      (Erro validação)     |          (Validação OK)
            |              |               |
            v              |               v
    ┌─────────────┐        |       ┌─────────────┐
    │  BLOQUEADO  │        |       │  AUTORIZADO │
    │ (Requer     │        |       │ (Aguardando │
    │  correção)  │        |       │  contábil)  │
    └──────┬──────┘        |       └──────┬──────┘
           |               |              |
    (Correção feita)       |       (Confirmação)
           |               |              |
           └───────────────┼──────────────┘
                           |
                           v
                    ┌─────────────┐
                    │   PAGO      │
                    │ (Finalizado)│
                    └─────────────┘
```

### Estados da Fase

```
        ┌──────────────┐
        │ NÃO INICIADA │
        │              │
        │ (Fase ainda  │
        │  não aberta) │
        └──────┬───────┘
               |
       (Evento de abertura)
               |
               v
        ┌──────────────┐
        │   ABERTA     │
        │              │
        │ DATA_FECHA = │
        │ '9999-12-31' │
        └──────┬───────┘
               |
       (Evento de fechamento)
               |
               v
        ┌──────────────┐
        │   FECHADA    │
        │              │
        │ DATA_FECHA = │
        │ data real    │
        └──────────────┘
```

---

## Performance e Métricas

### Métricas de Performance Esperadas

**Tempo de Resposta (Response Time):**
```
Busca de sinistro:        < 3 segundos (P95)
Validação externa:        < 10 segundos (timeout)
Transação ACID completa:  < 2 segundos (P95)
Ciclo total autorização:  < 90 segundos (SLA)
```

**Throughput:**
```
Operadores simultâneos:   até 100
Transações por segundo:   até 50 TPS
Pico de carga:            até 200 TPS (burst)
```

**Disponibilidade:**
```
Uptime alvo:              99.5% (mensal)
Downtime planejado:       4 horas/mês (manutenção)
MTTR (Mean Time Repair):  < 30 minutos
```

### Bottlenecks Conhecidos

**1. Validações Externas:**
- CNOUA/SIPUA/SIMDA podem ter latência variável
- Circuit breaker protege contra indisponibilidade prolongada

**2. Conversão Monetária:**
- Query TGEUNIMO com range de datas pode ser lenta
- **Solução:** Índice em (DTINIVIG, DTTERVIG)

**3. Locks de Banco:**
- UPDATE TMESTSIN gera lock de linha
- Deadlocks possíveis em carga alta
- **Solução:** Retry automático com backoff

---

## Resumo Executivo

### Processos Principais Documentados

1. ✅ **Autorização de Pagamento** (8 passos, tempo < 90s)
2. ✅ **Busca de Sinistros** (3 modos mutuamente exclusivos)
3. ✅ **Validação de Produtos** (CNOUA/SIPUA/SIMDA routing)
4. ✅ **Conversão Monetária BTNF** (precisão 2 decimais)
5. ✅ **Gestão de Fases** (abertura/fechamento automático)
6. ✅ **Registro de Transações ACID** (4 tabelas sincronizadas)

### Integrações Externas

- **CNOUA:** REST API (produtos 6814, 7701, 7709)
- **SIPUA:** SOAP Web Service (NUM_CONTRATO > 0)
- **SIMDA:** SOAP Web Service (NUM_CONTRATO = 0)
- Todos com timeout 10s, retry 2×, circuit breaker

### Garantias ACID

- **Atomicidade:** All-or-nothing (rollback total em erro)
- **Consistência:** Constraints validados, saldos sempre corretos
- **Isolamento:** READ COMMITTED, locks de linha
- **Durabilidade:** WAL logging, replicação DR

### Próximos Documentos

- **Diagramas UML:** Casos de uso, sequência, classes
- **Especificações OpenAPI:** APIs REST documentadas
- **Especificações WSDL:** Serviços SOAP documentados

---

**FIM DO DOCUMENTO**
