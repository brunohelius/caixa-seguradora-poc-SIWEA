# 04 - Modelo de Banco de Dados: Sistema Legado SIWEA

[← Voltar ao Índice](README.md) | [→ Próximo: Lógica de Negócio](05-business-logic.md)

---

## Visão Geral do Modelo

### Estatísticas do Banco de Dados

| Métrica | Valor |
|---------|-------|
| **Total de Tabelas** | 13 |
| **Tabelas Legadas** | 10 |
| **Tabelas Dashboard** | 3 |
| **Total de Campos** | 287 |
| **Índices** | 42 |
| **Constraints** | 68 |
| **Stored Procedures** | 5 |
| **Views** | 8 |
| **Tamanho Total** | 15.8 GB |
| **Registros (Total)** | 2.5M+ |

---

## Diagrama ER Completo

```
                        ┌──────────────┐
                        │  TSISTEMA    │
                        │ Data Sistema │
                        └──────┬───────┘
                               │
                ┌──────────────┴──────────────┐
                │                             │
        ┌───────▼────────┐           ┌───────▼────────┐
        │   TMESTSIN     │           │   TGERAMO      │
        │ Sinistro Mestre│◄──────────┤    Ramos       │
        │  PK: 4 partes  │           └────────────────┘
        └───┬────────────┘
            │
    ┌───────┴────────┬──────────┬──────────┬────────────┐
    │                │          │          │            │
┌───▼────┐    ┌─────▼─────┐ ┌──▼──┐  ┌───▼───┐  ┌─────▼──────┐
│THISTSIN│    │SI_ACOMPAN │ │SI_  │  │TAPOLI │  │EF_CONTR_   │
│História│    │HA_SINI    │ │SINI │  │CE     │  │SEG_HABIT   │
│PK: 5pt │    │ Eventos   │ │STRO │  │Apólice│  │ Contratos  │
└────────┘    └───────────┘ │_FASE│  └───────┘  └────────────┘
                             └──┬──┘
                                │
                        ┌───────▼────────┐
                        │SI_REL_FASE_    │
                        │EVENTO          │
                        │ Configuração   │
                        └────────────────┘
                               
                        ┌──────────────┐
                        │  TGEUNIMO    │
                        │  Conversão   │
                        │  Monetária   │
                        └──────────────┘
```

---

## Tabelas Detalhadas

### 1. TMESTSIN - Tabela Mestre de Sinistros

**Descrição:** Registro principal de cada sinistro com dados financeiros e identificação

**Volume:** ~500.000 registros ativos

#### Estrutura de Campos

| Campo | Tipo | Tamanho | Null | Descrição |
|-------|------|---------|------|-----------|
| **TIPSEG** | INT | 2 | N | Tipo de Seguro (PK1) |
| **ORGSIN** | INT | 2 | N | Origem Sinistro (PK2) |
| **RMOSIN** | INT | 2 | N | Ramo Sinistro (PK3) |
| **NUMSIN** | INT | 6 | N | Número Sinistro (PK4) |
| FONTE | INT | 3 | N | Fonte Protocolo |
| PROTSINI | INT | 7 | N | Número Protocolo |
| DAC | INT | 1 | N | Dígito Verificador |
| ORGAPO | INT | 2 | N | Origem Apólice |
| RMOAPO | INT | 2 | N | Ramo Apólice |
| NUMAPOL | INT | 8 | N | Número Apólice |
| SDOPAG | DECIMAL | 15,2 | N | Saldo a Pagar |
| TOTPAG | DECIMAL | 15,2 | N | Total Pago |
| CODPRODU | INT | 4 | N | Código Produto |
| TIPREG | CHAR | 1 | N | Tipo Registro |
| TPSEGU | INT | 2 | N | Tipo Seguro Apólice |
| CODLIDER | INT | 3 | Y | Código Líder |
| SINLID | INT | 7 | Y | Sinistro Líder |
| OCORHIST | INT | 5 | N | Contador Ocorrências |
| CREATED_BY | VARCHAR | 50 | N | Criado Por |
| CREATED_AT | DATETIME | - | N | Data Criação |
| UPDATED_BY | VARCHAR | 50 | Y | Atualizado Por |
| UPDATED_AT | DATETIME | - | Y | Data Atualização |
| ROW_VERSION | BINARY | 8 | N | Versão (Concorrência) |

#### Scripts DDL

```sql
CREATE TABLE TMESTSIN (
    TIPSEG      INT NOT NULL,
    ORGSIN      INT NOT NULL,
    RMOSIN      INT NOT NULL,
    NUMSIN      INT NOT NULL,
    FONTE       INT NOT NULL,
    PROTSINI    INT NOT NULL,
    DAC         INT NOT NULL,
    ORGAPO      INT NOT NULL,
    RMOAPO      INT NOT NULL,
    NUMAPOL     INT NOT NULL,
    SDOPAG      DECIMAL(15,2) NOT NULL,
    TOTPAG      DECIMAL(15,2) NOT NULL DEFAULT 0,
    CODPRODU    INT NOT NULL,
    TIPREG      CHAR(1) NOT NULL CHECK (TIPREG IN ('1', '2')),
    TPSEGU      INT NOT NULL,
    CODLIDER    INT NULL,
    SINLID      INT NULL,
    OCORHIST    INT NOT NULL DEFAULT 0,
    CREATED_BY  VARCHAR(50) NOT NULL,
    CREATED_AT  DATETIME NOT NULL DEFAULT GETDATE(),
    UPDATED_BY  VARCHAR(50) NULL,
    UPDATED_AT  DATETIME NULL,
    ROW_VERSION ROWVERSION NOT NULL,
    CONSTRAINT PK_TMESTSIN PRIMARY KEY (TIPSEG, ORGSIN, RMOSIN, NUMSIN),
    CONSTRAINT CHK_TOTPAG_LE_SDOPAG CHECK (TOTPAG <= SDOPAG)
);

-- Índices
CREATE INDEX IX_TMESTSIN_Protocol ON TMESTSIN(FONTE, PROTSINI, DAC);
CREATE INDEX IX_TMESTSIN_Leader ON TMESTSIN(CODLIDER, SINLID) WHERE CODLIDER IS NOT NULL;
CREATE INDEX IX_TMESTSIN_Policy ON TMESTSIN(ORGAPO, RMOAPO, NUMAPOL);
CREATE INDEX IX_TMESTSIN_Product ON TMESTSIN(CODPRODU);
```

---

### 2. THISTSIN - Histórico de Pagamentos

**Descrição:** Registro de cada autorização de pagamento realizada

**Volume:** ~2.000.000 registros

#### Estrutura de Campos

| Campo | Tipo | Tamanho | Null | Descrição |
|-------|------|---------|------|-----------|
| **TIPSEG** | INT | 2 | N | Tipo Seguro (PK1) |
| **ORGSIN** | INT | 2 | N | Origem (PK2) |
| **RMOSIN** | INT | 2 | N | Ramo (PK3) |
| **NUMSIN** | INT | 6 | N | Número (PK4) |
| **OCORHIST** | INT | 5 | N | Sequência (PK5) |
| OPERACAO | INT | 4 | N | Código Operação (1098) |
| DTMOVTO | DATE | - | N | Data Movimento |
| HORAOPER | TIME | - | N | Hora Operação |
| VALPRI | DECIMAL | 15,2 | N | Valor Principal |
| CRRMON | DECIMAL | 15,2 | N | Valor Correção |
| VALPRIBT | DECIMAL | 15,2 | N | Principal BTNF |
| CRRMONBT | DECIMAL | 15,2 | N | Correção BTNF |
| VALTOTBT | DECIMAL | 15,2 | N | Total BTNF |
| PRIDIABT | DECIMAL | 15,2 | N | Principal Diário |
| CRRDIABT | DECIMAL | 15,2 | N | Correção Diária |
| TOTDIABT | DECIMAL | 15,2 | N | Total Diário |
| NOMFAV | VARCHAR | 255 | Y | Nome Favorecido |
| TIPCRR | CHAR | 1 | N | Tipo Correção ('5') |
| SITCONTB | CHAR | 1 | N | Situação Contábil |
| SITUACAO | CHAR | 1 | N | Situação Geral |
| EZEUSRID | VARCHAR | 50 | N | ID Operador |

---

### 3. TGERAMO - Tabela de Ramos

**Descrição:** Cadastro de ramos de seguro

**Volume:** ~50 registros

| Campo | Tipo | Tamanho | Null | Descrição |
|-------|------|---------|------|-----------|
| **CODIGO** | INT | 2 | N | Código Ramo (PK) |
| DESCRICAO | VARCHAR | 100 | N | Descrição |
| ATIVO | CHAR | 1 | N | Status Ativo |

---

### 4. TGEUNIMO - Conversão Monetária

**Descrição:** Tabela de taxas de conversão para BTNF

**Volume:** ~10.000 registros

| Campo | Tipo | Tamanho | Null | Descrição |
|-------|------|---------|------|-----------|
| **DATA_INICIO** | DATE | - | N | Data Início (PK) |
| **DATA_FIM** | DATE | - | N | Data Fim (PK) |
| VALOR_CONVERSAO | DECIMAL | 18,8 | N | Taxa Conversão |
| MOEDA | VARCHAR | 3 | N | Código Moeda |

---

### 5. SI_ACOMPANHA_SINI - Eventos de Acompanhamento

**Descrição:** Registro de eventos do workflow

**Volume:** ~1.500.000 registros

| Campo | Tipo | Tamanho | Null | Descrição |
|-------|------|---------|------|-----------|
| **ID** | GUID | - | N | ID Único (PK) |
| TIPSEG | INT | 2 | N | Tipo Seguro (FK) |
| ORGSIN | INT | 2 | N | Origem (FK) |
| RMOSIN | INT | 2 | N | Ramo (FK) |
| NUMSIN | INT | 6 | N | Número (FK) |
| COD_EVENTO | INT | 3 | N | Código Evento |
| DATA_EVENTO | DATETIME | - | N | Data/Hora Evento |
| USUARIO | VARCHAR | 50 | N | Usuário |
| OBSERVACAO | VARCHAR | 500 | Y | Observações |

---

### 6. SI_SINISTRO_FASE - Fases do Sinistro

**Descrição:** Controle de fases do processamento

**Volume:** ~800.000 registros

| Campo | Tipo | Tamanho | Null | Descrição |
|-------|------|---------|------|-----------|
| **ID** | GUID | - | N | ID Único (PK) |
| TIPSEG | INT | 2 | N | Tipo Seguro (FK) |
| ORGSIN | INT | 2 | N | Origem (FK) |
| RMOSIN | INT | 2 | N | Ramo (FK) |
| NUMSIN | INT | 6 | N | Número (FK) |
| COD_FASE | INT | 3 | N | Código Fase |
| DATA_ABERTURA | DATE | - | N | Data Abertura |
| DATA_FECHAMENTO | DATE | - | Y | Data Fechamento |
| STATUS | CHAR | 1 | N | Status |

---

## Índices e Otimizações

### Índices Críticos para Performance

```sql
-- Busca por protocolo (mais frequente)
CREATE INDEX IX_PROTOCOL_COVERING ON TMESTSIN(
    FONTE, PROTSINI, DAC
) INCLUDE (
    TIPSEG, ORGSIN, RMOSIN, NUMSIN,
    SDOPAG, TOTPAG, CODPRODU
);

-- Histórico por sinistro
CREATE INDEX IX_HISTORY_BY_CLAIM ON THISTSIN(
    TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC
) INCLUDE (
    DTMOVTO, VALPRI, CRRMON, NOMFAV
);

-- Fases abertas
CREATE INDEX IX_OPEN_PHASES ON SI_SINISTRO_FASE(
    DATA_FECHAMENTO, STATUS
) WHERE DATA_FECHAMENTO IS NULL;
```

### Estatísticas de Uso

| Índice | Uso/Dia | Hit Ratio | Tamanho |
|--------|---------|-----------|---------|
| PK_TMESTSIN | 15.000 | 99.2% | 45 MB |
| IX_PROTOCOL_COVERING | 8.500 | 98.5% | 62 MB |
| IX_HISTORY_BY_CLAIM | 5.200 | 97.8% | 125 MB |
| IX_LEADER | 450 | 94.2% | 8 MB |

---

## Procedures e Functions

### SP_AUTORIZAR_PAGAMENTO

```sql
CREATE PROCEDURE SP_AUTORIZAR_PAGAMENTO
    @TIPSEG INT,
    @ORGSIN INT,
    @RMOSIN INT,
    @NUMSIN INT,
    @VALPRI DECIMAL(15,2),
    @CRRMON DECIMAL(15,2),
    @NOMFAV VARCHAR(255),
    @USUARIO VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @OCORHIST INT;
    DECLARE @TAXA_CONVERSAO DECIMAL(18,8);
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Obter próximo número de ocorrência
        SELECT @OCORHIST = ISNULL(MAX(OCORHIST), 0) + 1
        FROM THISTSIN WITH (UPDLOCK)
        WHERE TIPSEG = @TIPSEG 
          AND ORGSIN = @ORGSIN
          AND RMOSIN = @RMOSIN
          AND NUMSIN = @NUMSIN;
        
        -- Obter taxa de conversão
        SELECT @TAXA_CONVERSAO = VALOR_CONVERSAO
        FROM TGEUNIMO
        WHERE GETDATE() BETWEEN DATA_INICIO AND DATA_FIM;
        
        -- Inserir histórico
        INSERT INTO THISTSIN (...)
        VALUES (...);
        
        -- Atualizar mestre
        UPDATE TMESTSIN
        SET TOTPAG = TOTPAG + @VALPRI + @CRRMON,
            OCORHIST = @OCORHIST,
            UPDATED_BY = @USUARIO,
            UPDATED_AT = GETDATE()
        WHERE TIPSEG = @TIPSEG 
          AND ORGSIN = @ORGSIN
          AND RMOSIN = @RMOSIN
          AND NUMSIN = @NUMSIN;
        
        COMMIT TRANSACTION;
        RETURN 0;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END
```

---

## Views Importantes

### VW_SINISTROS_PENDENTES

```sql
CREATE VIEW VW_SINISTROS_PENDENTES AS
SELECT 
    T.TIPSEG, T.ORGSIN, T.RMOSIN, T.NUMSIN,
    T.FONTE, T.PROTSINI, T.DAC,
    T.SDOPAG AS VALOR_ESPERADO,
    T.TOTPAG AS VALOR_PAGO,
    (T.SDOPAG - T.TOTPAG) AS SALDO_PENDENTE,
    R.DESCRICAO AS RAMO_DESCRICAO,
    F.COD_FASE AS FASE_ATUAL,
    F.DATA_ABERTURA AS DATA_FASE_ATUAL
FROM TMESTSIN T
    INNER JOIN TGERAMO R ON T.RMOSIN = R.CODIGO
    LEFT JOIN SI_SINISTRO_FASE F ON 
        T.TIPSEG = F.TIPSEG 
        AND T.ORGSIN = F.ORGSIN
        AND T.RMOSIN = F.RMOSIN
        AND T.NUMSIN = F.NUMSIN
        AND F.DATA_FECHAMENTO IS NULL
WHERE T.SDOPAG > T.TOTPAG;
```

---

## Considerações de Migração

### Desafios Identificados

1. **Chaves Compostas**: Migrar para chaves simples (GUIDs)
2. **EBCDIC para UTF-8**: Conversão de caracteres especiais
3. **Packed Decimal**: Conversão de COMP-3 para decimal
4. **Datas 9999-12-31**: Substituir por NULL
5. **Campos CHAR(1)**: Converter para enums em C#

### Estratégia de Migração de Dados

```sql
-- Script de migração exemplo
INSERT INTO NEW_CLAIM_MASTER (
    Id,
    InsuranceType,
    ClaimOrigin,
    ClaimBranch,
    ClaimNumber,
    -- ... outros campos
)
SELECT 
    NEWID() as Id,
    TIPSEG,
    ORGSIN,
    RMOSIN,
    NUMSIN,
    -- ... conversões necessárias
FROM TMESTSIN;
```

---

*Este documento detalha o modelo completo de banco de dados do sistema SIWEA.*

**Última Atualização:** 27/10/2025
