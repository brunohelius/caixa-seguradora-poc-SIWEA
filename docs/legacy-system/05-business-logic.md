# 05 - Lógica de Negócio: Sistema Legado SIWEA

[← Voltar ao Índice](README.md) | [→ Próximo: Integrações Externas](06-external-integrations.md)

---

## Visão Geral das Regras de Negócio

### Estatísticas

| Categoria | Quantidade | Criticidade |
|-----------|------------|-------------|
| **Regras de Validação** | 45 | Alta |
| **Regras de Cálculo** | 28 | Crítica |
| **Regras de Workflow** | 22 | Média |
| **Regras de Integração** | 15 | Alta |
| **Regras de Auditoria** | 12 | Média |
| **TOTAL** | **122** | - |

---

## Regras Críticas de Sistema

### BR-001: Validação de Tipo de Pagamento

**Categoria:** Validação
**Criticidade:** CRÍTICA

```
REGRA:
    O tipo de pagamento DEVE ser um valor entre 1 e 5
    
VALORES:
    1 = Pagamento Total
    2 = Pagamento Parcial
    3 = Pagamento Complementar
    4 = Pagamento de Ajuste
    5 = Pagamento Recalculado
    
AÇÃO SE INVÁLIDO:
    Rejeitar transação com erro E0002
    Log de tentativa de fraude se valor > 5
```

### BR-002: Valor Principal Obrigatório

**Categoria:** Validação
**Criticidade:** CRÍTICA

```
REGRA:
    Valor principal (VALPRI) DEVE ser > 0
    
EXCEÇÃO:
    Permitido = 0 apenas se tipo pagamento = 4 (Ajuste)
    E valor correção > 0
    
VALIDAÇÃO:
    IF VALPRI <= 0 AND (TIPPAG <> 4 OR CRRMON <= 0) THEN
        ERRO E0003
```

### BR-003: Beneficiário Obrigatório Condicional

**Categoria:** Validação
**Criticidade:** ALTA

```
REGRA:
    Beneficiário (NOMFAV) é obrigatório SE:
    - Tipo de seguro da apólice (TPSEGU) != 0
    
LÓGICA:
    IF TPSEGU <> 0 AND (NOMFAV IS NULL OR TRIM(NOMFAV) = '') THEN
        ERRO E0004 "Beneficiário obrigatório para este tipo de seguro"
```

---

## Regras de Cálculo Financeiro

### BR-010: Conversão para BTNF

**Categoria:** Cálculo
**Criticidade:** CRÍTICA

```cobol
COMPUTE VALPRIBT = VALPRI * TAXA-CONVERSAO
COMPUTE CRRMONBT = CRRMON * TAXA-CONVERSAO
COMPUTE VALTOTBT = VALPRIBT + CRRMONBT

ONDE:
    TAXA-CONVERSAO vem de TGEUNIMO
    baseado na data de movimento (DTMOVTO)
    
PRECISÃO:
    Todas as operações com DECIMAL(15,2)
    Arredondamento: ROUND_HALF_UP
```

### BR-011: Cálculo de Saldo Pendente

**Categoria:** Cálculo
**Criticidade:** ALTA

```
SALDO_PENDENTE = SDOPAG - TOTPAG

VALIDAÇÕES:
    1. SALDO_PENDENTE não pode ser negativo
    2. Novo pagamento não pode exceder SALDO_PENDENTE
    3. Se SALDO_PENDENTE = 0, sinistro deve ser fechado
```

### BR-012: Atualização de Total Pago

**Categoria:** Cálculo
**Criticidade:** CRÍTICA

```sql
UPDATE TMESTSIN
SET TOTPAG = TOTPAG + (VALPRI + CRRMON)
WHERE <chave_sinistro>

CONSTRAINT:
    TOTPAG nunca pode exceder SDOPAG
    Se exceder, ROLLBACK completo
```

---

## Regras de Workflow e Fases

### BR-020: Abertura Automática de Fase

**Categoria:** Workflow
**Criticidade:** ALTA

```
QUANDO: Autorização de pagamento aprovada

AÇÃO:
    1. Verificar se existe fase aberta (DATA_FECHAMENTO = NULL)
    2. Se não existe, criar fase com código 300 (Autorização)
    3. DATA_ABERTURA = data atual
    4. DATA_FECHAMENTO = NULL (aberta)
    
SQL:
    IF NOT EXISTS (fase aberta) THEN
        INSERT INTO SI_SINISTRO_FASE
        VALUES (sinistro, 300, GETDATE(), NULL, 'A')
```

### BR-021: Fechamento Automático de Fase

**Categoria:** Workflow
**Criticidade:** MÉDIA

```
QUANDO: 
    - Pagamento total realizado (TOTPAG = SDOPAG)
    - OU evento de encerramento manual

AÇÃO:
    UPDATE SI_SINISTRO_FASE
    SET DATA_FECHAMENTO = GETDATE(),
        STATUS = 'F'
    WHERE <sinistro> AND DATA_FECHAMENTO IS NULL
```

### BR-022: Sequência de Fases

**Categoria:** Workflow
**Criticidade:** ALTA

```
SEQUÊNCIA OBRIGATÓRIA:
    001 → 002 → 003 → 004 → 005 → 006 → 007 → 008

FASES:
    001 = Abertura
    002 = Análise Documentação
    003 = Autorização Pagamento
    004 = Pagamento Realizado
    005 = Aguardando Comprovação
    006 = Comprovação Recebida
    007 = Análise Final
    008 = Encerramento

REGRA:
    Não é possível pular fases
    Fase anterior deve estar fechada
```

---

## Regras de Integração Externa

### BR-030: Roteamento por Produto

**Categoria:** Integração
**Criticidade:** CRÍTICA

```
REGRA DE ROTEAMENTO:
    
SE CODPRODU IN (6814, 7701, 7709) ENTÃO
    → Validar com CNOUA (Consórcio)
    
SE CODPRODU BETWEEN 5000 AND 5999 ENTÃO
    → Validar com SIPUA (EFP)
    
SE CODPRODU BETWEEN 8000 AND 8999 ENTÃO
    → Validar com SIMDA (HB)
    
SENÃO
    → Sem validação externa necessária
```

### BR-031: Timeout de Integração

**Categoria:** Integração
**Criticidade:** ALTA

```
TIMEOUTS:
    CNOUA: 30 segundos
    SIPUA: 20 segundos
    SIMDA: 25 segundos

RETRY:
    Máximo 3 tentativas
    Backoff exponencial: 1s, 2s, 4s

CONTINGÊNCIA:
    Se todas falham E modo contingência ativo:
        Aprovar com flag CONTINGENCY = 'Y'
    Senão:
        Rejeitar com erro E0010
```

### BR-032: Validação de Resposta Externa

**Categoria:** Integração
**Criticidade:** CRÍTICA

```
RESPOSTAS VÁLIDAS:
    APROVADO: Continuar processamento
    REJEITADO: Abortar com erro específico
    PENDENTE: Aguardar callback (máx 24h)
    ERRO: Aplicar regra de retry

VALIDAÇÃO DE LIMITE:
    Se sistema retorna limite disponível:
        Valor solicitado não pode exceder limite
```

---

## Regras de Auditoria e Compliance

### BR-040: Registro de Operador

**Categoria:** Auditoria
**Criticidade:** ALTA

```
TODA transação DEVE registrar:
    - EZEUSRID (ID do operador CICS)
    - Terminal ID
    - Data/Hora da operação
    - IP de origem (se disponível)

IMUTABILIDADE:
    Após gravado, EZEUSRID não pode ser alterado
```

### BR-041: Log de Alterações Críticas

**Categoria:** Auditoria
**Criticidade:** CRÍTICA

```
EVENTOS QUE DEVEM SER LOGADOS:
    1. Autorização de pagamento > R$ 100.000
    2. Alteração manual de valores
    3. Override de validação
    4. Operação em modo contingência
    5. Tentativa de fraude detectada

RETENÇÃO:
    Logs críticos: 7 anos
    Logs normais: 2 anos
```

---

## Regras de Validação de Dados

### BR-050: Validação de Protocolo

**Categoria:** Validação
**Criticidade:** ALTA

```
FORMATO: XXX/NNNNNNN-D

ONDE:
    XXX = Fonte (3 dígitos)
    NNNNNNN = Número (7 dígitos)
    D = Dígito verificador (1 dígito)

CÁLCULO DO DÍGITO:
    Módulo 11 dos 10 dígitos anteriores
```

### BR-051: Validação de Sinistro

**Categoria:** Validação
**Criticidade:** ALTA

```
FORMATO: OO/RR/NNNNNN

ONDE:
    OO = Origem (2 dígitos, 01-99)
    RR = Ramo (2 dígitos, 01-50)
    NNNNNN = Número (6 dígitos)

VALIDAÇÕES:
    - Origem deve existir em tabela
    - Ramo deve existir em TGERAMO
    - Número sequencial único por origem/ramo
```

---

## Regras de Controle de Concorrência

### BR-060: Lock Pessimista

**Categoria:** Concorrência
**Criticidade:** CRÍTICA

```sql
-- Ao iniciar autorização
SELECT * FROM TMESTSIN WITH (UPDLOCK)
WHERE <chave_sinistro>

-- Lock mantido até:
    - COMMIT (sucesso)
    - ROLLBACK (erro)
    - Timeout (120 segundos)
```

### BR-061: Detecção de Conflito

**Categoria:** Concorrência
**Criticidade:** ALTA

```
USANDO ROW_VERSION:
    1. Ler ROW_VERSION no SELECT
    2. Incluir no WHERE do UPDATE
    3. Se afetou 0 linhas = conflito
    4. Recarregar e tentar novamente (máx 3x)
```

---

## Regras de Performance

### BR-070: Cache de Dados Estáticos

**Categoria:** Performance
**Criticidade:** MÉDIA

```
DADOS PARA CACHE (TTL = 1 hora):
    - TGERAMO (ramos)
    - TGEUNIMO (taxas do dia)
    - SI_REL_FASE_EVENTO (configuração)
    
INVALIDAÇÃO:
    - Por timeout
    - Por comando administrativo
    - Por mudança detectada
```

### BR-071: Paginação de Resultados

**Categoria:** Performance
**Criticidade:** MÉDIA

```
BUSCA DE SINISTROS:
    - Máximo 100 registros por página
    - Ordenação por data descendente
    - Usar OFFSET/FETCH (SQL Server)
    - Ou ROWNUM (Oracle/DB2)
```

---

## Regras de Segurança

### BR-080: Autorização por Alçada

**Categoria:** Segurança
**Criticidade:** CRÍTICA

```
NÍVEIS DE ALÇADA:
    Operador Jr: até R$ 10.000
    Operador Pleno: até R$ 50.000
    Operador Senior: até R$ 200.000
    Supervisor: até R$ 1.000.000
    Gerente: sem limite

VALIDAÇÃO:
    IF valor_total > alçada_usuario THEN
        Requer aprovação superior
```

### BR-081: Prevenção de Fraude

**Categoria:** Segurança
**Criticidade:** CRÍTICA

```
INDICADORES DE FRAUDE:
    1. Múltiplos pagamentos mesmo beneficiário/dia
    2. Valor 50% acima da média do ramo
    3. Beneficiário em blacklist
    4. Padrão anormal de horário
    
AÇÃO:
    Flag para análise manual
    Notificação ao compliance
```

---

## Matriz de Dependência de Regras

| Regra | Depende de | É pré-requisito para |
|-------|------------|---------------------|
| BR-001 | - | BR-010, BR-020 |
| BR-002 | - | BR-011, BR-012 |
| BR-003 | BR-051 | BR-040 |
| BR-010 | BR-002 | BR-012 |
| BR-011 | BR-010 | BR-021 |
| BR-012 | BR-011 | BR-020, BR-021 |
| BR-020 | BR-012 | BR-022 |
| BR-030 | BR-001 | BR-031, BR-032 |
| BR-040 | Todas | - |
| BR-060 | - | BR-012, BR-020 |

---

## Implementação em .NET

### Exemplo de Implementação

```csharp
public class PaymentAuthorizationService
{
    // BR-001: Validação de Tipo de Pagamento
    private void ValidatePaymentType(int paymentType)
    {
        if (paymentType < 1 || paymentType > 5)
        {
            throw new BusinessRuleException("E0002", 
                "Tipo de pagamento inválido");
        }
    }
    
    // BR-010: Conversão BTNF
    private decimal ConvertToBTNF(decimal value, DateTime date)
    {
        var rate = _currencyService.GetConversionRate(date);
        return Math.Round(value * rate, 2, 
            MidpointRounding.ToEven);
    }
    
    // BR-030: Roteamento por Produto
    private async Task<ValidationResult> ValidateExternal(
        int productCode, decimal amount)
    {
        return productCode switch
        {
            6814 or 7701 or 7709 => 
                await _cnouaService.ValidateAsync(amount),
            >= 5000 and <= 5999 => 
                await _sipuaService.ValidateAsync(amount),
            >= 8000 and <= 8999 => 
                await _simdaService.ValidateAsync(amount),
            _ => ValidationResult.Approved()
        };
    }
}
```

---

*Este documento detalha toda a lógica de negócio do sistema SIWEA.*

**Última Atualização:** 27/10/2025
