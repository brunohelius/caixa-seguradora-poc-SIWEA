# 03 - Estruturas de Dados: Sistema Legado SIWEA

[← Voltar ao Índice](README.md) | [→ Próximo: Modelo de Banco de Dados](04-database-model.md)

---

## Visão Geral das Estruturas

### Categorização das Estruturas

| Categoria | Quantidade | Propósito |
|-----------|------------|-----------|
| **Working Storage** | 45 | Variáveis de trabalho e flags |
| **Copy Books** | 12 | Estruturas reutilizáveis |
| **Record Layouts** | 13 | Layouts de tabelas DB2 |
| **Screen Maps** | 2 | Mapas de tela CICS |
| **Interface Areas** | 8 | Comunicação entre programas |
| **Message Structures** | 3 | Integração com sistemas externos |

---

## Working Storage Section

### Estrutura Principal de Controle

```cobol
01  WS-CONTROL-AREA.
    05  WS-PROGRAM-ID           PIC X(08) VALUE 'SIWEA116'.
    05  WS-VERSION              PIC X(03) VALUE 'V90'.
    05  WS-TRANSACTION-ID       PIC X(04).
    05  WS-USER-ID              PIC X(08).
    05  WS-TERMINAL-ID          PIC X(08).
    05  WS-CURRENT-DATE         PIC 9(08).
    05  WS-CURRENT-TIME         PIC 9(06).
    05  WS-BUSINESS-DATE        PIC 9(08).
    05  WS-RETURN-CODE          PIC S9(04) COMP.
    05  WS-ERROR-FLAG           PIC X(01).
        88  NO-ERROR            VALUE 'N'.
        88  ERROR-OCCURRED      VALUE 'Y'.
    05  WS-PROCESS-MODE         PIC X(02).
        88  SEARCH-MODE         VALUE 'SE'.
        88  AUTH-MODE           VALUE 'AU'.
        88  HISTORY-MODE        VALUE 'HI'.
```

### Estrutura de Sinistro de Trabalho

```cobol
01  WS-CLAIM-WORK-AREA.
    05  WS-CLAIM-KEY.
        10  WS-INSURANCE-TYPE   PIC 9(02).
        10  WS-CLAIM-ORIGIN     PIC 9(02).
        10  WS-CLAIM-BRANCH     PIC 9(02).
        10  WS-CLAIM-NUMBER     PIC 9(06).

    05  WS-PROTOCOL-KEY.
        10  WS-PROTOCOL-SOURCE  PIC 9(03).
        10  WS-PROTOCOL-NUMBER  PIC 9(07).
        10  WS-PROTOCOL-CHECK   PIC 9(01).

    05  WS-LEADER-KEY.
        10  WS-LEADER-CODE      PIC 9(03).
        10  WS-LEADER-CLAIM     PIC 9(07).

    05  WS-FINANCIAL-DATA.
        10  WS-EXPECTED-RESERVE PIC 9(13)V99 COMP-3.
        10  WS-TOTAL-PAID       PIC 9(13)V99 COMP-3.
        10  WS-PENDING-VALUE    PIC 9(13)V99 COMP-3.
        10  WS-PRINCIPAL-VALUE  PIC 9(13)V99 COMP-3.
        10  WS-CORRECTION-VALUE PIC 9(13)V99 COMP-3.
        10  WS-BTNF-VALUE       PIC 9(13)V99 COMP-3.
        10  WS-CONVERSION-RATE  PIC 9(10)V9(8) COMP-3.
```

### Estrutura de Autorização

```cobol
01  WS-AUTHORIZATION-AREA.
    05  WS-AUTH-HEADER.
        10  WS-AUTH-DATE        PIC 9(08).
        10  WS-AUTH-TIME        PIC 9(06).
        10  WS-AUTH-USER        PIC X(08).
        10  WS-AUTH-TERMINAL    PIC X(08).

    05  WS-AUTH-DATA.
        10  WS-PAYMENT-TYPE     PIC 9(01).
            88  TOTAL-PAYMENT   VALUE 1.
            88  PARTIAL-PAYMENT VALUE 2.
            88  COMPLEMENT-PAY  VALUE 3.
            88  ADJUSTMENT-PAY  VALUE 4.
            88  RECALC-PAYMENT  VALUE 5.

        10  WS-POLICY-TYPE      PIC 9(01).
            88  INDIVIDUAL      VALUE 1.
            88  COLLECTIVE      VALUE 2.

        10  WS-BENEFICIARY      PIC X(50).
        10  WS-OPERATION-CODE   PIC 9(04) VALUE 1098.
        10  WS-CORRECTION-TYPE  PIC X(01) VALUE '5'.
```

### Flags de Controle

```cobol
01  WS-FLAGS.
    05  WS-SEARCH-FLAGS.
        10  WS-SEARCH-BY-PROTOCOL   PIC X(01).
            88  SEARCH-PROTOCOL      VALUE 'Y'.
        10  WS-SEARCH-BY-CLAIM      PIC X(01).
            88  SEARCH-CLAIM         VALUE 'Y'.
        10  WS-SEARCH-BY-LEADER     PIC X(01).
            88  SEARCH-LEADER        VALUE 'Y'.
        10  WS-CLAIM-FOUND          PIC X(01).
            88  CLAIM-EXISTS         VALUE 'Y'.
            88  CLAIM-NOT-FOUND      VALUE 'N'.

    05  WS-VALIDATION-FLAGS.
        10  WS-PAYMENT-VALID        PIC X(01).
            88  PAYMENT-OK           VALUE 'Y'.
        10  WS-AMOUNT-VALID         PIC X(01).
            88  AMOUNT-OK            VALUE 'Y'.
        10  WS-BENEFICIARY-VALID    PIC X(01).
            88  BENEFICIARY-OK       VALUE 'Y'.
        10  WS-EXTERNAL-VALID       PIC X(01).
            88  EXTERNAL-OK          VALUE 'Y'.

    05  WS-INTEGRATION-FLAGS.
        10  WS-CNOUA-REQUIRED       PIC X(01).
            88  NEEDS-CNOUA          VALUE 'Y'.
        10  WS-SIPUA-REQUIRED       PIC X(01).
            88  NEEDS-SIPUA          VALUE 'Y'.
        10  WS-SIMDA-REQUIRED       PIC X(01).
            88  NEEDS-SIMDA          VALUE 'Y'.
        10  WS-CONTINGENCY-MODE     PIC X(01).
            88  IN-CONTINGENCY       VALUE 'Y'.
```

---

## Copy Books

### CLAIM-RECORD - Layout do Registro de Sinistro

```cobol
      * CLAIM-RECORD.CPY - Layout completo do sinistro
       01  CLAIM-RECORD.
           05  CLM-KEY.
               10  CLM-INSURANCE-TYPE      PIC 9(02).
               10  CLM-ORIGIN              PIC 9(02).
               10  CLM-BRANCH              PIC 9(02).
               10  CLM-NUMBER              PIC 9(06).

           05  CLM-PROTOCOL.
               10  CLM-PROT-SOURCE         PIC 9(03).
               10  CLM-PROT-NUMBER         PIC 9(07).
               10  CLM-PROT-CHECK          PIC 9(01).

           05  CLM-POLICY.
               10  CLM-POL-ORIGIN          PIC 9(02).
               10  CLM-POL-BRANCH          PIC 9(02).
               10  CLM-POL-NUMBER          PIC 9(08).

           05  CLM-FINANCIAL.
               10  CLM-EXPECTED-RESERVE    PIC S9(13)V99 COMP-3.
               10  CLM-TOTAL-PAID          PIC S9(13)V99 COMP-3.
               10  CLM-PRODUCT-CODE        PIC 9(04).
               10  CLM-POLICY-TYPE         PIC X(01).
               10  CLM-INSURANCE-TYPE-POL  PIC 9(02).

           05  CLM-LEADER REDEFINES CLM-FINANCIAL.
               10  CLM-LEADER-CODE         PIC 9(03).
               10  CLM-LEADER-CLAIM        PIC 9(07).
               10  FILLER                  PIC X(25).

           05  CLM-COUNTER                 PIC 9(05).
           05  CLM-STATUS                  PIC X(02).
               88  CLM-ACTIVE              VALUE 'AT'.
               88  CLM-CLOSED              VALUE 'EN'.
               88  CLM-CANCELLED           VALUE 'CA'.
```

### HISTORY-RECORD - Layout do Histórico

```cobol
      * HISTORY-RECORD.CPY - Layout do registro de histórico
       01  HISTORY-RECORD.
           05  HST-KEY.
               10  HST-INSURANCE-TYPE      PIC 9(02).
               10  HST-ORIGIN              PIC 9(02).
               10  HST-BRANCH              PIC 9(02).
               10  HST-NUMBER              PIC 9(06).
               10  HST-SEQUENCE            PIC 9(05).

           05  HST-AUTHORIZATION.
               10  HST-AUTH-DATE           PIC 9(08).
               10  HST-AUTH-TIME           PIC 9(06).
               10  HST-AUTH-USER           PIC X(08).
               10  HST-AUTH-TERMINAL       PIC X(08).

           05  HST-PAYMENT.
               10  HST-PAYMENT-TYPE        PIC 9(01).
               10  HST-POLICY-TYPE         PIC 9(01).
               10  HST-PRINCIPAL-VALUE     PIC S9(13)V99 COMP-3.
               10  HST-CORRECTION-VALUE    PIC S9(13)V99 COMP-3.
               10  HST-TOTAL-VALUE         PIC S9(13)V99 COMP-3.
               10  HST-BTNF-VALUE          PIC S9(13)V99 COMP-3.
               10  HST-CONVERSION-RATE     PIC 9(10)V9(8) COMP-3.

           05  HST-BENEFICIARY             PIC X(50).
           05  HST-OPERATION-CODE          PIC 9(04).
           05  HST-CORRECTION-TYPE         PIC X(01).
           05  HST-STATUS                  PIC X(02).
               88  HST-APPROVED            VALUE 'AP'.
               88  HST-REJECTED            VALUE 'RJ'.
               88  HST-PENDING             VALUE 'PE'.
```

### ERROR-MESSAGES - Mensagens de Erro

```cobol
      * ERROR-MESSAGES.CPY - Tabela de mensagens
       01  ERROR-MESSAGE-TABLE.
           05  ERROR-ENTRY OCCURS 24 TIMES.
               10  ERR-CODE                PIC X(05).
               10  ERR-SEVERITY            PIC 9(01).
                   88  ERR-INFO            VALUE 1.
                   88  ERR-WARNING         VALUE 2.
                   88  ERR-ERROR           VALUE 3.
                   88  ERR-FATAL           VALUE 4.
               10  ERR-TEXT-PT             PIC X(100).
               10  ERR-TEXT-EN             PIC X(100).

       01  ERROR-MESSAGES-DATA.
           05  FILLER.
               10  FILLER PIC X(05) VALUE 'E0001'.
               10  FILLER PIC 9(01) VALUE 3.
               10  FILLER PIC X(100) VALUE
                   'Sinistro não encontrado'.
               10  FILLER PIC X(100) VALUE
                   'Claim not found'.

           05  FILLER.
               10  FILLER PIC X(05) VALUE 'E0002'.
               10  FILLER PIC 9(01) VALUE 3.
               10  FILLER PIC X(100) VALUE
                   'Tipo de pagamento inválido (deve ser 1-5)'.
               10  FILLER PIC X(100) VALUE
                   'Invalid payment type (must be 1-5)'.

           05  FILLER.
               10  FILLER PIC X(05) VALUE 'E0003'.
               10  FILLER PIC 9(01) VALUE 3.
               10  FILLER PIC X(100) VALUE
                   'Valor principal obrigatório'.
               10  FILLER PIC X(100) VALUE
                   'Principal value required'.
```

---

## Interface com Banco de Dados

### SQL Communication Area

```cobol
01  WS-SQL-AREAS.
    05  WS-SQLCODE              PIC S9(09) COMP.
        88  SQL-SUCCESS         VALUE 0.
        88  SQL-NOT-FOUND       VALUE +100.
        88  SQL-DUPLICATE       VALUE -803.
        88  SQL-DEADLOCK        VALUE -911 -913.

    05  WS-SQLSTATE             PIC X(05).
    05  WS-SQLERRM.
        10  WS-SQLERRML         PIC S9(04) COMP.
        10  WS-SQLERRMC         PIC X(70).

    05  WS-ROW-COUNT            PIC S9(09) COMP.
```

### Host Variables para DB2

```cobol
01  HV-TMESTSIN.
    05  HV-TIPSEG               PIC S9(02) COMP.
    05  HV-ORGSIN               PIC S9(02) COMP.
    05  HV-RMOSIN               PIC S9(02) COMP.
    05  HV-NUMSIN               PIC S9(06) COMP.
    05  HV-FONTE                PIC S9(03) COMP.
    05  HV-PROTSINI             PIC S9(07) COMP.
    05  HV-DAC                  PIC S9(01) COMP.
    05  HV-ORGAPO               PIC S9(02) COMP.
    05  HV-RMOAPO               PIC S9(02) COMP.
    05  HV-NUMAPOL              PIC S9(08) COMP.
    05  HV-SDOPAG               PIC S9(13)V99 COMP-3.
    05  HV-TOTPAG               PIC S9(13)V99 COMP-3.
    05  HV-CODPRODU             PIC S9(04) COMP.
    05  HV-TIPREG               PIC X(01).
    05  HV-TPSEGU               PIC S9(02) COMP.
    05  HV-CODLIDER             PIC S9(03) COMP.
    05  HV-SINLID               PIC S9(07) COMP.
    05  HV-OCORHIST             PIC S9(05) COMP.

01  HV-INDICATOR-VARIABLES.
    05  HV-IND-CODLIDER         PIC S9(04) COMP.
    05  HV-IND-SINLID           PIC S9(04) COMP.
```

### Cursores SQL

```cobol
      * Cursor para busca por protocolo
       EXEC SQL
           DECLARE CURSOR-PROTOCOL CURSOR FOR
           SELECT TIPSEG, ORGSIN, RMOSIN, NUMSIN,
                  FONTE, PROTSINI, DAC,
                  ORGAPO, RMOAPO, NUMAPOL,
                  SDOPAG, TOTPAG, CODPRODU,
                  TIPREG, TPSEGU, CODLIDER,
                  SINLID, OCORHIST
           FROM   TMESTSIN
           WHERE  FONTE = :HV-FONTE
           AND    PROTSINI = :HV-PROTSINI
           AND    DAC = :HV-DAC
           FOR READ ONLY
       END-EXEC.

      * Cursor para busca por sinistro
       EXEC SQL
           DECLARE CURSOR-CLAIM CURSOR FOR
           SELECT TIPSEG, ORGSIN, RMOSIN, NUMSIN,
                  FONTE, PROTSINI, DAC,
                  ORGAPO, RMOAPO, NUMAPOL,
                  SDOPAG, TOTPAG, CODPRODU,
                  TIPREG, TPSEGU, CODLIDER,
                  SINLID, OCORHIST
           FROM   TMESTSIN
           WHERE  ORGSIN = :HV-ORGSIN
           AND    RMOSIN = :HV-RMOSIN
           AND    NUMSIN = :HV-NUMSIN
           FOR UPDATE OF TOTPAG, OCORHIST
       END-EXEC.
```

---

## Estruturas de Integração Externa

### CNOUA - Mensagem de Validação de Consórcio

```cobol
01  CNOUA-REQUEST-MESSAGE.
    05  CNOUA-HEADER.
        10  CNOUA-MESSAGE-ID        PIC X(20).
        10  CNOUA-TIMESTAMP         PIC X(26).
        10  CNOUA-SOURCE-SYSTEM     PIC X(10) VALUE 'SIWEA'.
        10  CNOUA-VERSION           PIC X(05) VALUE '01.00'.

    05  CNOUA-BODY.
        10  CNOUA-PRODUCT-CODE      PIC 9(04).
        10  CNOUA-CONTRACT-NUMBER   PIC 9(12).
        10  CNOUA-CLAIM-NUMBER      PIC X(12).
        10  CNOUA-REQUESTED-AMOUNT  PIC 9(13)V99.
        10  CNOUA-OPERATION-TYPE    PIC X(03) VALUE 'VAL'.

01  CNOUA-RESPONSE-MESSAGE.
    05  CNOUA-RESP-HEADER.
        10  CNOUA-RESP-MESSAGE-ID   PIC X(20).
        10  CNOUA-RESP-TIMESTAMP    PIC X(26).
        10  CNOUA-RESP-STATUS       PIC X(02).
            88  CNOUA-APPROVED       VALUE 'AP'.
            88  CNOUA-REJECTED       VALUE 'RJ'.
            88  CNOUA-ERROR          VALUE 'ER'.

    05  CNOUA-RESP-BODY.
        10  CNOUA-RESP-CODE         PIC X(05).
        10  CNOUA-RESP-MESSAGE      PIC X(100).
        10  CNOUA-AVAILABLE-LIMIT   PIC 9(13)V99.
        10  CNOUA-EXPIRATION-DATE   PIC 9(08).
```

### SIPUA - Mensagem de Validação EFP

```cobol
01  SIPUA-REQUEST-RECORD.
    05  SIPUA-CONTRACT-TYPE         PIC X(03) VALUE 'EFP'.
    05  SIPUA-CONTRACT-NUMBER       PIC X(12).
    05  SIPUA-AMOUNT                PIC 9(13)V99.
    05  SIPUA-CLAIM-NUMBER          PIC X(12).
    05  SIPUA-REQUEST-DATE          PIC 9(08).
    05  SIPUA-REQUEST-TIME          PIC 9(06).

01  SIPUA-RESPONSE-RECORD.
    05  SIPUA-VALIDATED             PIC X(01).
        88  SIPUA-VALID             VALUE 'Y'.
        88  SIPUA-INVALID           VALUE 'N'.
    05  SIPUA-AVAILABLE-LIMIT       PIC 9(13)V99.
    05  SIPUA-EXPIRATION-DATE       PIC 9(08).
    05  SIPUA-REJECT-REASON         PIC X(50).
```

### SIMDA - Mensagem de Validação HB

```cobol
01  SIMDA-MESSAGE-FORMAT.
    05  SIMDA-REQUEST.
        10  SIMDA-CONTRACT-NUM      PIC X(10).
        10  SIMDA-AMOUNT            PIC 9(13)V99.
        10  SIMDA-CLAIM-NUM         PIC X(10).
        10  FILLER                  PIC X(165).

    05  SIMDA-RESPONSE.
        10  SIMDA-STATUS            PIC X(01).
            88  SIMDA-APPROVED      VALUE 'A'.
            88  SIMDA-REJECTED      VALUE 'R'.
            88  SIMDA-PENDING       VALUE 'P'.
        10  SIMDA-AVAILABLE-AMT     PIC 9(13)V99.
        10  SIMDA-MESSAGE           PIC X(84).
```

---

## Estruturas de Comunicação CICS

### COMMAREA - Área de Comunicação

```cobol
01  DFHCOMMAREA.
    05  CA-REQUEST-TYPE             PIC X(04).
        88  CA-SEARCH               VALUE 'SRCH'.
        88  CA-AUTHORIZE            VALUE 'AUTH'.
        88  CA-HISTORY              VALUE 'HIST'.
        88  CA-CANCEL               VALUE 'CANC'.

    05  CA-RESPONSE-CODE            PIC S9(04) COMP.
        88  CA-SUCCESS              VALUE 0.
        88  CA-NOT-FOUND            VALUE 4.
        88  CA-INVALID-DATA         VALUE 8.
        88  CA-SYSTEM-ERROR         VALUE 12.

    05  CA-CLAIM-DATA.
        10  CA-CLAIM-KEY.
            15  CA-INSURANCE-TYPE   PIC 9(02).
            15  CA-ORIGIN           PIC 9(02).
            15  CA-BRANCH           PIC 9(02).
            15  CA-NUMBER           PIC 9(06).

        10  CA-PROTOCOL-KEY.
            15  CA-PROT-SOURCE      PIC 9(03).
            15  CA-PROT-NUMBER      PIC 9(07).
            15  CA-PROT-CHECK       PIC 9(01).

    05  CA-AUTH-DATA.
        10  CA-PAYMENT-TYPE         PIC 9(01).
        10  CA-POLICY-TYPE          PIC 9(01).
        10  CA-PRINCIPAL-VALUE      PIC 9(13)V99.
        10  CA-CORRECTION-VALUE     PIC 9(13)V99.
        10  CA-BENEFICIARY          PIC X(50).

    05  CA-ERROR-MESSAGE            PIC X(100).
    05  CA-USER-ID                  PIC X(08).
    05  CA-TERMINAL-ID              PIC X(08).
```

### EIB - Execute Interface Block

```cobol
      * Estrutura implícita do CICS EIB
      * Acessada via DFHEIBLK

      * Principais campos utilizados:
      * EIBCALEN - Comprimento da COMMAREA
      * EIBAID   - Tecla de atenção pressionada
      * EIBTRNID - ID da transação
      * EIBTRMID - ID do terminal
      * EIBDATE  - Data do sistema
      * EIBTIME  - Hora do sistema
      * EIBRESP  - Código de resposta
      * EIBRESP2 - Código de resposta secundário
```

---

## Tabelas de Configuração

### Tabela de Produtos para Roteamento

```cobol
01  PRODUCT-ROUTING-TABLE.
    05  PRODUCT-ENTRY OCCURS 50 TIMES
        INDEXED BY PROD-IDX.
        10  PROD-CODE               PIC 9(04).
        10  PROD-SYSTEM             PIC X(05).
            88  ROUTE-TO-CNOUA      VALUE 'CNOUA'.
            88  ROUTE-TO-SIPUA      VALUE 'SIPUA'.
            88  ROUTE-TO-SIMDA      VALUE 'SIMDA'.
            88  NO-ROUTING          VALUE 'NONE '.
        10  PROD-DESCRIPTION        PIC X(30).

01  PRODUCT-ROUTING-DATA.
    05  FILLER.
        10  FILLER PIC 9(04) VALUE 6814.
        10  FILLER PIC X(05) VALUE 'CNOUA'.
        10  FILLER PIC X(30) VALUE 'CONSORCIO TIPO A'.
    05  FILLER.
        10  FILLER PIC 9(04) VALUE 7701.
        10  FILLER PIC X(05) VALUE 'CNOUA'.
        10  FILLER PIC X(30) VALUE 'CONSORCIO TIPO B'.
    05  FILLER.
        10  FILLER PIC 9(04) VALUE 7709.
        10  FILLER PIC X(05) VALUE 'CNOUA'.
        10  FILLER PIC X(30) VALUE 'CONSORCIO ESPECIAL'.
```

### Tabela de Fases e Eventos

```cobol
01  PHASE-EVENT-TABLE.
    05  PHASE-ENTRY OCCURS 20 TIMES.
        10  PHASE-CODE              PIC 9(03).
        10  PHASE-NAME              PIC X(30).
        10  PHASE-EVENT             PIC 9(03).
        10  PHASE-NEXT              PIC 9(03).
        10  PHASE-AUTO-CLOSE        PIC X(01).
            88  AUTO-CLOSE-YES      VALUE 'Y'.
            88  AUTO-CLOSE-NO       VALUE 'N'.

01  PHASE-EVENT-DATA.
    05  FILLER.
        10  FILLER PIC 9(03) VALUE 001.
        10  FILLER PIC X(30) VALUE 'ABERTURA SINISTRO'.
        10  FILLER PIC 9(03) VALUE 100.
        10  FILLER PIC 9(03) VALUE 002.
        10  FILLER PIC X(01) VALUE 'N'.
    05  FILLER.
        10  FILLER PIC 9(03) VALUE 002.
        10  FILLER PIC X(30) VALUE 'ANALISE DOCUMENTACAO'.
        10  FILLER PIC 9(03) VALUE 200.
        10  FILLER PIC 9(03) VALUE 003.
        10  FILLER PIC X(01) VALUE 'N'.
    05  FILLER.
        10  FILLER PIC 9(03) VALUE 003.
        10  FILLER PIC X(30) VALUE 'AUTORIZACAO PAGAMENTO'.
        10  FILLER PIC 9(03) VALUE 300.
        10  FILLER PIC 9(03) VALUE 004.
        10  FILLER PIC X(01) VALUE 'Y'.
```

---

## Estruturas de Performance

### Contadores e Estatísticas

```cobol
01  WS-PERFORMANCE-COUNTERS.
    05  WS-TRANSACTION-COUNT        PIC 9(09) COMP.
    05  WS-DB-CALLS                 PIC 9(09) COMP.
    05  WS-DB-READS                 PIC 9(09) COMP.
    05  WS-DB-WRITES                PIC 9(09) COMP.
    05  WS-DB-UPDATES               PIC 9(09) COMP.
    05  WS-EXTERNAL-CALLS           PIC 9(09) COMP.
    05  WS-CACHE-HITS               PIC 9(09) COMP.
    05  WS-CACHE-MISSES             PIC 9(09) COMP.

01  WS-TIMING-AREAS.
    05  WS-START-TIME               PIC 9(08).
    05  WS-END-TIME                 PIC 9(08).
    05  WS-ELAPSED-TIME             PIC 9(08).
    05  WS-DB-TIME                  PIC 9(08).
    05  WS-EXTERNAL-TIME            PIC 9(08).
    05  WS-CPU-TIME                 PIC 9(08).
```

---

## Considerações para Migração

### Mapeamento de Tipos

| COBOL Type | .NET Type | Observações |
|------------|-----------|-------------|
| PIC 9(n) | int/long | Depende do tamanho |
| PIC S9(n) COMP | short/int | Com sinal |
| PIC 9(n)V99 | decimal | Precisão monetária |
| PIC X(n) | string | Máximo length |
| COMP-3 | decimal | Packed decimal |
| 88 levels | enum | Valores nomeados |

### Pontos de Atenção

1. **REDEFINES**: Requer union types ou classes separadas
2. **OCCURS**: Mapear para List<T> ou arrays
3. **INDEXED BY**: Usar Dictionary<K,V> quando apropriado
4. **88 Levels**: Converter para enums ou constants
5. **COMP-3**: Cuidado com conversão de packed decimal
6. **EBCDIC**: Conversão para UTF-8 necessária

---

*Este documento detalha todas as estruturas de dados utilizadas pelo sistema SIWEA.*

**Última Atualização:** 27/10/2025
**Próxima Revisão:** Após implementação das entidades em .NET