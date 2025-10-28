# SIWEA - Regras de Negócio Completas (100+ Regras)

**Documento:** Regras de Negócio Detalhadas do Sistema Legado SIWEA
**Versão:** 1.0
**Data:** 2025-10-27
**Documento:** 2 de 5
**Referência:** SISTEMA_LEGADO_VISAO_GERAL.md

---

## Índice

1. [Introdução](#introdução)
2. [Taxonomia e Classificação](#taxonomia-e-classificação)
3. [Regras de Busca e Recuperação (BR-001 a BR-009)](#regras-de-busca)
4. [Regras de Autorização de Pagamento (BR-010 a BR-022)](#regras-de-autorização)
5. [Regras de Conversão Monetária (BR-023 a BR-033)](#regras-de-conversão)
6. [Regras de Registro de Transações (BR-034 a BR-042)](#regras-de-registro)
7. [Regras de Validação de Produtos (BR-043 a BR-056)](#regras-de-validação)
8. [Regras de Gestão de Fases (BR-057 a BR-067)](#regras-de-fases)
9. [Regras de Auditoria (BR-068 a BR-074)](#regras-de-auditoria)
10. [Regras de Validação de Dados (BR-075 a BR-087)](#regras-de-validação-dados)
11. [Regras de Interface e Display (BR-088 a BR-095)](#regras-de-interface)
12. [Regras de Performance (BR-096 a BR-100)](#regras-de-performance)
13. [Matriz de Rastreabilidade](#matriz-de-rastreabilidade)

---

## Introdução

Este documento contém a especificação completa de **todas as 100+ regras de negócio** do sistema SIWEA identificadas durante a análise do código legado Visual Age EZEE 4.40.

### Propósito

Garantir que a migração para .NET 9 + React preserve **100% da lógica de negócio** do sistema legado sem simplificações ou "melhorias" que possam alterar o comportamento estabelecido.

### Como Usar Este Documento

- **Desenvolvedores:** Use como referência durante implementação de cada user story
- **QA/Testers:** Use para criar casos de teste e validar comportamento
- **Analistas:** Use para esclarecer dúvidas sobre comportamento do sistema
- **Arquitetos:** Use para entender dependências entre regras

### Notação

- **BR-XXX:** Identificador único da regra de negócio
- **Origem:** Localização no código legado ou documento de análise
- **Tier:** Classificação de criticidade (System-Critical, Business-Critical, Operational)
- **Dependências:** Outras regras que esta regra depende
- **Impacto:** Consequência de não implementar corretamente

---

## Taxonomia e Classificação

### Tiers de Criticidade

#### **Tier 1: System-Critical** (Falha = Sistema Inoperante)
Regras que, se não implementadas corretamente, tornam o sistema inoperante ou causam corrupção de dados.

Exemplos: BR-034, BR-035, BR-036, BR-051, BR-066, BR-067

#### **Tier 2: Business-Critical** (Falha = Erro de Negócio)
Regras que, se não implementadas corretamente, causam erros de cálculo, pagamentos incorretos ou violações de compliance.

Exemplos: BR-010, BR-013, BR-019, BR-023, BR-043, BR-048

#### **Tier 3: Operational** (Falha = Experiência Degradada)
Regras que afetam usabilidade, performance ou experiência do usuário, mas não impedem operação crítica.

Exemplos: BR-004, BR-009, BR-088, BR-096

### Categorias Funcionais

| Categoria | Quantidade | Identificadores |
|-----------|------------|-----------------|
| **Busca e Recuperação** | 9 | BR-001 a BR-009 |
| **Autorização de Pagamento** | 13 | BR-010 a BR-022 |
| **Conversão Monetária** | 11 | BR-023 a BR-033 |
| **Registro de Transações** | 9 | BR-034 a BR-042 |
| **Validação de Produtos** | 14 | BR-043 a BR-056 |
| **Gestão de Fases** | 11 | BR-057 a BR-067 |
| **Auditoria** | 7 | BR-068 a BR-074 |
| **Validação de Dados** | 13 | BR-075 a BR-087 |
| **Interface e Display** | 8 | BR-088 a BR-095 |
| **Performance** | 5 | BR-096 a BR-100 |
| **TOTAL** | **100** | BR-001 a BR-100 |

---

## Regras de Busca e Recuperação

### BR-001: Três Critérios Mutuamente Exclusivos

**Tier:** Operational
**Categoria:** Busca
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 876

**Descrição:**
O sistema DEVE permitir busca de sinistros usando EXATAMENTE UM dos três critérios disponíveis:
1. Protocolo (FONTE + PROTSINI + DAC)
2. Número do Sinistro (ORGSIN + RMOSIN + NUMSIN)
3. Código Líder (CODLIDER + SINLID)

**Lógica:**
```
SE (Protocolo COMPLETO) ENTÃO
    Usar critério 1
SENÃO SE (Número Sinistro COMPLETO) ENTÃO
    Usar critério 2
SENÃO SE (Código Líder COMPLETO) ENTÃO
    Usar critério 3
SENÃO
    ERRO: "Pelo menos um critério completo deve ser informado"
FIM SE
```

**Validação:**
- Critério "completo" = TODOS os campos do critério preenchidos
- Não é permitido usar múltiplos critérios simultaneamente
- Ao menos um critério completo deve ser fornecido

**Dependências:** Nenhuma

**Impacto:** Baixo - Interface de usuário degradada se não implementado

**Testes:**
- ✅ Apenas protocolo preenchido → Busca por protocolo
- ✅ Apenas número sinistro preenchido → Busca por número
- ✅ Apenas código líder preenchido → Busca por líder
- ❌ Protocolo + número sinistro preenchidos → Erro
- ❌ Nenhum critério completo → Erro

---

### BR-002: Obrigatoriedade de Critério Completo

**Tier:** Operational
**Categoria:** Busca
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 880

**Descrição:**
O sistema DEVE validar que ao menos um conjunto completo de campos de busca foi fornecido antes de executar a consulta.

**Lógica:**
```
PROTOCOLO_COMPLETO = (FONTE != VAZIO) E (PROTSINI != VAZIO) E (DAC != VAZIO)
SINISTRO_COMPLETO = (ORGSIN != VAZIO) E (RMOSIN != VAZIO) E (NUMSIN != VAZIO)
LIDER_COMPLETO = (CODLIDER != VAZIO) E (SINLID != VAZIO)

SE NÃO (PROTOCOLO_COMPLETO OU SINISTRO_COMPLETO OU LIDER_COMPLETO) ENTÃO
    RETORNAR ERRO "Informe ao menos um critério de busca completo"
FIM SE
```

**Validação:**
- Campo vazio = NULL, string vazia ("") ou contém apenas espaços

**Dependências:** BR-001

**Impacto:** Baixo - Previne queries desnecessárias ao banco

**Mensagem Erro:**
- Português: "Informe ao menos um critério de busca completo"

---

### BR-003: Recuperação de Dados do Registro Mestre

**Tier:** System-Critical
**Categoria:** Busca
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 882

**Descrição:**
Após identificar o critério de busca válido, o sistema DEVE executar a query apropriada na tabela TMESTSIN usando a chave primária composta de 4 partes (TIPSEG, ORGSIN, RMOSIN, NUMSIN).

**Lógica:**

**Query 1: Por Protocolo**
```sql
SELECT * FROM TMESTSIN
WHERE FONTE = @fonte
  AND PROTSINI = @protsini
  AND DAC = @dac
```
**Retorna:** 0 ou 1 registro (protocolo é único)

**Query 2: Por Número do Sinistro**
```sql
SELECT * FROM TMESTSIN
WHERE ORGSIN = @orgsin
  AND RMOSIN = @rmosin
  AND NUMSIN = @numsin
```
**Retorna:** 0 ou 1 registro (número é único)

**Query 3: Por Código Líder**
```sql
SELECT * FROM TMESTSIN
WHERE CODLIDER = @codlider
  AND SINLID = @sinlid
```
**Retorna:** 0 ou mais registros (pode haver múltiplos sinistros do mesmo líder)

**Validação:**
- Todas as queries devem usar índices apropriados
- Timeout de query: 3 segundos (ver BR-096)

**Dependências:** BR-001, BR-002

**Impacto:** Alto - Sistema inoperante se queries falharem

**Performance:**
- Índices obrigatórios:
  - IX_TMESTSIN_Protocol (FONTE, PROTSINI, DAC)
  - IX_TMESTSIN_Leader (CODLIDER, SINLID)
  - PK sobre (TIPSEG, ORGSIN, RMOSIN, NUMSIN)

---

### BR-004: Formato de Exibição de Protocolo

**Tier:** Operational
**Categoria:** Display
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 884

**Descrição:**
O número de protocolo DEVE ser exibido no formato visual `FONTE/PROTSINI-DAC` (3 partes separadas por barra e traço).

**Lógica:**
```
ENTRADA: FONTE=1, PROTSINI=123456, DAC=7
SAÍDA: "001/0123456-7"

Formato:
  FONTE:    3 dígitos com zeros à esquerda
  PROTSINI: 7 dígitos com zeros à esquerda
  DAC:      1 dígito
  Separadores: "/" e "-"
```

**Implementação:**
```csharp
string FormatarProtocolo(int fonte, int protsini, int dac)
{
    return $"{fonte:000}/{protsini:0000000}-{dac}";
}
```

**Dependências:** Nenhuma

**Impacto:** Baixo - Problema cosmético se não implementado

---

### BR-005: Formato de Exibição de Número do Sinistro

**Tier:** Operational
**Categoria:** Display
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 885

**Descrição:**
O número do sinistro DEVE ser exibido no formato `ORGSIN/RMOSIN/NUMSIN` (3 partes separadas por barra).

**Lógica:**
```
ENTRADA: ORGSIN=10, RMOSIN=20, NUMSIN=789012
SAÍDA: "10/20/789012"

Formato:
  ORGSIN: 2 dígitos com zeros à esquerda
  RMOSIN: 2 dígitos com zeros à esquerda
  NUMSIN: 6 dígitos com zeros à esquerda
  Separador: "/"
```

**Implementação:**
```csharp
string FormatarNumeroSinistro(int orgsin, int rmosin, int numsin)
{
    return $"{orgsin:00}/{rmosin:00}/{numsin:000000}";
}
```

**Dependências:** Nenhuma

**Impacto:** Baixo - Problema cosmético

---

### BR-006: Mensagem de Erro - Sinistro Não Encontrado

**Tier:** Business-Critical
**Categoria:** Validação
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 886

**Descrição:**
Quando a busca não retorna nenhum registro, o sistema DEVE exibir mensagem de erro específica indicando que o documento não foi encontrado, usando o formato do critério pesquisado.

**Mensagens por Critério:**

**Protocolo:**
```
"PROTOCOLO {FONTE}/{PROTSINI}-{DAC} NAO ENCONTRADO"
Exemplo: "PROTOCOLO 001/0123456-7 NAO ENCONTRADO"
```

**Sinistro:**
```
"SINISTRO {ORGSIN}/{RMOSIN}/{NUMSIN} NAO ENCONTRADO"
Exemplo: "SINISTRO 10/20/789012 NAO ENCONTRADO"
```

**Líder:**
```
"LIDER {CODLIDER}-{SINLID} NAO ENCONTRADO"
Exemplo: "LIDER 001-0000001 NAO ENCONTRADO"
```

**Genérico (quando não identificável):**
```
"DOCUMENTO XXXXXXXXXXXXXXX NAO CADASTRADO"
```

**Validação:**
- Mensagem DEVE estar em PORTUGUÊS (maiúsculas/minúsculas conforme legado)
- Mensagem DEVE ser exibida em cor vermelha (#e80c4d)

**Dependências:** BR-001, BR-003

**Impacto:** Médio - Operador pode não entender motivo da falha

---

### BR-007: Recuperação de Nome do Ramo

**Tier:** Business-Critical
**Categoria:** Relacionamento de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 887

**Descrição:**
O sistema DEVE recuperar o nome descritivo do ramo de seguro da tabela TGERAMO usando o código RMOSIN do sinistro como chave estrangeira.

**Lógica:**
```sql
SELECT t.*, g.NOMERAMO
FROM TMESTSIN t
LEFT JOIN TGERAMO g ON g.RMOSIN = t.RMOSIN
WHERE t.FONTE = @fonte
  AND t.PROTSINI = @protsini
  AND t.DAC = @dac
```

**Comportamento:**
- Se registro encontrado em TGERAMO → Exibir NOMERAMO
- Se registro NÃO encontrado → Exibir RMOSIN numérico (fallback)

**Validação:**
- Relacionamento é N:1 (muitos sinistros → um ramo)
- RMOSIN pode ser NULL em teoria (verificar com DBA)

**Dependências:** BR-003

**Impacto:** Médio - Operador vê código em vez de nome legível

---

### BR-008: Recuperação de Nome do Segurado

**Tier:** Business-Critical
**Categoria:** Relacionamento de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 888

**Descrição:**
O sistema DEVE recuperar o nome do segurado da tabela TAPOLICE usando a chave composta de apólice (ORGAPO, RMOAPO, NUMAPOL) do sinistro.

**Lógica:**
```sql
SELECT t.*, p.NOME AS NOME_SEGURADO
FROM TMESTSIN t
LEFT JOIN TAPOLICE p ON (
    p.ORGAPO = t.ORGAPO
    AND p.RMOAPO = t.RMOAPO
    AND p.NUMAPOL = t.NUMAPOL
)
WHERE t.FONTE = @fonte
  AND t.PROTSINI = @protsini
  AND t.DAC = @dac
```

**Comportamento:**
- Se apólice encontrada → Exibir NOME
- Se apólice NÃO encontrada → Exibir "N/A" ou deixar vazio

**Validação:**
- Relacionamento é N:1 (muitos sinistros → uma apólice)
- Apólice DEVE existir (integridade referencial)

**Dependências:** BR-003

**Impacto:** Alto - Nome do segurado é informação crítica para pagamento

---

### BR-009: Formatação de Valores Monetários

**Tier:** Operational
**Categoria:** Display
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 889

**Descrição:**
Todos os valores monetários DEVEM ser exibidos com:
- Separador de milhares: `.` (ponto)
- Separador decimal: `,` (vírgula)
- 2 casas decimais SEMPRE
- Prefixo `R$` para valores em reais

**Formato:**
```
Padrão brasileiro: R$ #.###.###,##

Exemplos:
1234.56     → "R$ 1.234,56"
50000.00    → "R$ 50.000,00"
0.50        → "R$ 0,50"
999999999.99 → "R$ 999.999.999,99"
```

**Implementação C#:**
```csharp
decimal valor = 50000.00m;
string formatado = valor.ToString("C2", new CultureInfo("pt-BR"));
// Resultado: "R$ 50.000,00"
```

**Validação:**
- Valores negativos exibidos com parênteses ou sinal negativo
- Zero exibido como "R$ 0,00"
- Valores NULL exibidos como "R$ 0,00" ou "N/A"

**Campos Afetados:**
- SDOPAG (Saldo a Pagar)
- TOTPAG (Total Pago)
- Saldo Pendente (calculado)
- VALPRI, CRRMON, VALTOTBT (histórico)

**Dependências:** Nenhuma

**Impacto:** Baixo - Problema de legibilidade

---

## Regras de Autorização de Pagamento

### BR-010: Valores Válidos para Tipo de Pagamento

**Tier:** Business-Critical
**Categoria:** Validação de Entrada
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 900

**Descrição:**
O campo "Tipo de Pagamento" DEVE aceitar APENAS os valores numéricos 1, 2, 3, 4 ou 5. Qualquer outro valor DEVE ser rejeitado.

**Lógica:**
```
SE TipoPagamento NOT IN (1, 2, 3, 4, 5) ENTÃO
    RETORNAR ERRO "Tipo de Pagamento deve ser 1, 2, 3, 4 ou 5"
FIM SE
```

**Significado dos Códigos:**
```
1 = [Significado definido em documentação de negócio]
2 = [Significado definido em documentação de negócio]
3 = [Significado definido em documentação de negócio]
4 = [Significado definido em documentação de negócio]
5 = [Significado definido em documentação de negócio]
```
*Nota: Significados exatos devem ser validados com área de negócio*

**Validação:**
- Campo obrigatório
- Deve ser numérico inteiro
- Deve estar no conjunto {1, 2, 3, 4, 5}
- Não aceita NULL, vazio ou caracteres não-numéricos

**Dependências:** Nenhuma

**Impacto:** Alto - Pagamento pode ser categorizado incorretamente

**Mensagem Erro:**
```
Português: "Tipo de Pagamento deve ser 1, 2, 3, 4 ou 5"
Cor: #e80c4d (vermelho)
```

---

### BR-011: Mensagem de Erro - Tipo de Pagamento Inválido

**Tier:** Operational
**Categoria:** Mensagem de Erro
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 901

**Descrição:**
Quando o usuário tenta submeter um tipo de pagamento inválido, a mensagem de erro DEVE seguir o formato exato do sistema legado.

**Mensagem:**
```
"Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5"
```

**Especificações:**
- Idioma: Português
- Capitalização: Primeira letra maiúscula
- Pontuação: Vírgula antes do "ou"
- Cor: #e80c4d (vermelho - ver BR-092)
- Posição: Abaixo do campo de entrada

**Dependências:** BR-010

**Impacto:** Baixo - UX degradada se mensagem diferente

---

### BR-012: Validação de Valor Principal

**Tier:** Business-Critical
**Categoria:** Validação de Entrada
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 902

**Descrição:**
O campo "Valor Principal" DEVE ser:
- Obrigatório (não pode ser vazio ou NULL)
- Numérico decimal
- Não-negativo (>= 0)
- Máximo 15 dígitos inteiros + 2 decimais

**Lógica:**
```
SE ValorPrincipal IS NULL OR ValorPrincipal = "" ENTÃO
    ERRO "Valor Principal é obrigatório"
FIM SE

SE ValorPrincipal < 0 ENTÃO
    ERRO "Valor Principal não pode ser negativo"
FIM SE

SE ValorPrincipal > 999999999999999.99 ENTÃO
    ERRO "Valor Principal excede limite permitido"
FIM SE
```

**Tipo de Dados:**
```
SQL: DECIMAL(15,2)
C#: decimal
```

**Validação de Formato:**
- Aceita entrada com `.` ou `,` como separador decimal
- Remove separadores de milhares antes de processar
- Arredonda para 2 casas decimais se necessário

**Dependências:** Nenhuma

**Impacto:** Alto - Valor incorreto resulta em pagamento errado

---

### BR-013: Validação de Valor Contra Saldo Pendente

**Tier:** System-Critical
**Categoria:** Validação de Negócio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 903

**Descrição:**
O Valor Principal informado DEVE ser menor ou igual ao Saldo Pendente do sinistro. Esta é uma regra CRÍTICA que previne pagamentos acima do devido.

**Lógica:**
```
SALDO_PENDENTE = TMESTSIN.SDOPAG - TMESTSIN.TOTPAG

SE ValorPrincipal > SALDO_PENDENTE ENTÃO
    RETORNAR ERRO "Valor Superior ao Saldo Pendente"
FIM SE
```

**Cenários:**

**Cenário 1: Valor OK**
```
SDOPAG = 50.000,00
TOTPAG = 10.000,00
PENDENTE = 40.000,00
ValorPrincipal = 15.000,00 ✅ ACEITO (15k < 40k)
```

**Cenário 2: Valor no limite**
```
SDOPAG = 50.000,00
TOTPAG = 10.000,00
PENDENTE = 40.000,00
ValorPrincipal = 40.000,00 ✅ ACEITO (40k = 40k)
```

**Cenário 3: Valor excedido**
```
SDOPAG = 50.000,00
TOTPAG = 10.000,00
PENDENTE = 40.000,00
ValorPrincipal = 45.000,00 ❌ REJEITADO (45k > 40k)
Mensagem: "Valor Superior ao Saldo Pendente"
```

**Validação:**
- Cálculo deve considerar conversão para BTNF (ver BR-023 a BR-031)
- Se múltiplas moedas, comparar após conversão
- Verificar concorrência: outro operador pode ter autorizado pagamento simultâneo

**Dependências:** BR-012

**Impacto:** CRÍTICO - Previne pagamento indevido

**Mensagem Erro:**
```
Português: "Valor Superior ao Saldo Pendente"
Cor: #e80c4d
Ação: Abortar transação completamente
```

---

### BR-014: Mensagem de Erro - Valor Excede Pendente

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 904

**Descrição:**
Mensagem exata quando BR-013 é violado.

**Mensagem:**
```
"Valor Superior ao Saldo Pendente"
```

**Especificações:**
- Primeira letra maiúscula em cada palavra principal
- Sem ponto final
- Exibir junto: Saldo Pendente atual para referência do operador

**Exemplo de Display:**
```
┌─────────────────────────────────────────┐
│ ⚠️ ERRO                                  │
│                                         │
│ Valor Superior ao Saldo Pendente        │
│                                         │
│ Saldo Pendente: R$ 40.000,00           │
│ Valor Informado: R$ 45.000,00          │
│ Diferença: R$ 5.000,00 (excesso)       │
└─────────────────────────────────────────┘
```

**Dependências:** BR-013

**Impacto:** Médio - Operador precisa entender motivo da rejeição

---

### BR-015: Valores Válidos para Tipo de Apólice

**Tier:** Business-Critical
**Categoria:** Validação de Entrada
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 905

**Descrição:**
O campo "Tipo de Apólice" DEVE aceitar APENAS os valores 1 ou 2.

**Lógica:**
```
SE TipoApolice NOT IN (1, 2) ENTÃO
    RETORNAR ERRO "Tipo de Apólice deve ser 1 ou 2"
FIM SE
```

**Significado:**
```
1 = [Tipo definido em documentação de negócio]
2 = [Tipo definido em documentação de negócio]
```
*Nota: Consultar área de negócio para significado exato*

**Validação:**
- Campo obrigatório
- Deve ser numérico inteiro
- Deve ser 1 ou 2
- Não aceita NULL, vazio ou outros valores

**Relação com TIPREG:**
- TIPREG da tabela TMESTSIN: indica tipo de registro da apólice original
- TipoApolice do formulário: pode ser usado para classificação adicional
- Validar se existe relação de negócio entre TIPREG e TipoApolice informado

**Dependências:** Nenhuma

**Impacto:** Alto - Pode afetar processamento contábil ou fiscal

---

### BR-016: Mensagem de Erro - Tipo de Apólice Inválido

**Tier:** Operational
**Categoria:** Mensagem de Erro
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 906

**Descrição:**
Mensagem de erro quando tipo de apólice inválido é informado.

**Mensagem:**
```
"Tipo de Apólice deve ser 1 ou 2"
```

**Especificações:**
- Capitalização conforme mostrado
- Cor: #e80c4d (vermelho)
- Posição: Abaixo do campo

**Dependências:** BR-015

**Impacto:** Baixo

---

### BR-017: Valor de Correção Opcional

**Tier:** Business-Critical
**Categoria:** Validação de Entrada
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 907

**Descrição:**
O campo "Valor da Correção" é OPCIONAL. Se não informado, o sistema DEVE assumir o valor padrão de 0,00 (zero).

**Lógica:**
```
SE ValorCorrecao IS NULL OR ValorCorrecao = "" ENTÃO
    ValorCorrecao = 0.00
FIM SE
```

**Comportamento:**
- Campo vazio = 0,00
- Campo NULL = 0,00
- Campo "0" = 0,00
- Todos são equivalentes

**Validação:**
- Se informado, deve seguir mesmas regras de BR-012 (decimal, não-negativo)
- Não entra no cálculo de BR-013 (validação contra saldo pendente)
- É somado ao valor principal na conversão BTNF (ver BR-030, BR-031)

**Dependências:** Nenhuma

**Impacto:** Médio - Afeta cálculo de valores totais em BTNF

---

### BR-018: Validação de Valor de Correção Quando Informado

**Tier:** Business-Critical
**Categoria:** Validação de Entrada
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 908

**Descrição:**
Quando o Valor da Correção é informado (não-vazio), ele DEVE seguir as mesmas regras de validação numérica do Valor Principal.

**Lógica:**
```
SE ValorCorrecao IS NOT NULL AND ValorCorrecao != "" ENTÃO
    SE ValorCorrecao < 0 ENTÃO
        ERRO "Valor de Correção não pode ser negativo"
    FIM SE

    SE ValorCorrecao > 999999999999999.99 ENTÃO
        ERRO "Valor de Correção excede limite permitido"
    FIM SE
FIM SE
```

**Validação:**
- Tipo: DECIMAL(15,2)
- Não-negativo
- Máximo 15 dígitos + 2 decimais
- Se omitido, usar 0,00 (BR-017)

**Dependências:** BR-017

**Impacto:** Médio - Valor incorreto afeta total do pagamento

---

### BR-019: Beneficiário Obrigatório Quando TPSEGU ≠ 0

**Tier:** System-Critical
**Categoria:** Validação Condicional
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 909

**Descrição:**
O campo "Beneficiário" (Favorecido) é OBRIGATÓRIO se e somente se o campo TPSEGU (Tipo de Seguro da Apólice) do sinistro for diferente de zero.

**Lógica:**
```
SE TMESTSIN.TPSEGU != 0 ENTÃO
    SE Beneficiario IS NULL OR Beneficiario = "" OR TRIM(Beneficiario) = "" ENTÃO
        RETORNAR ERRO "Favorecido é obrigatório para este tipo de seguro"
    FIM SE
FIM SE
```

**Cenários:**

**Cenário 1: TPSEGU = 0**
```
TPSEGU = 0
Beneficiario = "" ✅ ACEITO (campo opcional)
Beneficiario = "JOÃO SILVA" ✅ ACEITO (pode preencher)
```

**Cenário 2: TPSEGU ≠ 0**
```
TPSEGU = 5
Beneficiario = "" ❌ REJEITADO
Beneficiario = "JOÃO SILVA" ✅ ACEITO
```

**Validação:**
- TPSEGU é campo da tabela TMESTSIN (não do formulário)
- Deve ser recuperado junto com os dados do sinistro (BR-003)
- Beneficiário vazio = NULL, "", ou string apenas com espaços
- Se obrigatório e não informado, abortar transação ANTES de chamar validação externa

**Dependências:** BR-003 (TPSEGU vem do registro mestre)

**Impacto:** CRÍTICO - Pagamento pode ir para pessoa errada ou violar compliance

---

### BR-020: Mensagem de Erro - Beneficiário Obrigatório

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 910

**Descrição:**
Mensagem exata quando BR-019 é violado.

**Mensagem:**
```
"Favorecido é obrigatório para este tipo de seguro"
```

**Especificações:**
- Usar termo "Favorecido" (não "Beneficiário") conforme legado
- Primeira letra maiúscula
- Cor: #e80c4d
- Exibir junto: Tipo de seguro atual para contexto

**Exemplo de Display:**
```
┌─────────────────────────────────────────┐
│ ⚠️ ERRO                                  │
│                                         │
│ Favorecido é obrigatório para este     │
│ tipo de seguro                          │
│                                         │
│ Tipo Seguro: 5                         │
└─────────────────────────────────────────┘
```

**Dependências:** BR-019

**Impacto:** Médio - Operador precisa entender por que campo é obrigatório

---

### BR-021: Tamanho Máximo do Campo Beneficiário

**Tier:** Operational
**Categoria:** Validação de Entrada
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 911

**Descrição:**
O campo "Beneficiário" DEVE aceitar até 255 caracteres.

**Validação:**
```
SE LENGTH(Beneficiario) > 255 ENTÃO
    ERRO "Nome do beneficiário muito longo (máximo 255 caracteres)"
FIM SE
```

**Tipo de Dados:**
```
SQL: VARCHAR(255)
C#: string (MaxLength = 255)
```

**Comportamento:**
- Caracteres especiais: permitidos (acentos, hífen, apóstrofo)
- Múltiplos espaços: reduzir para um único espaço
- Espaços iniciais/finais: remover (trim)
- Maiúsculas/minúsculas: preservar conforme digitado

**Dependências:** BR-019

**Impacto:** Baixo - Campo raramente excede 255 caracteres

---

### BR-022: Preservação de Caracteres Especiais no Beneficiário

**Tier:** Operational
**Categoria:** Processamento de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 912

**Descrição:**
O sistema DEVE preservar caracteres especiais no nome do beneficiário conforme digitado pelo operador, incluindo:
- Acentos (á, é, í, ó, ú, ã, õ, â, ê, ô, ç)
- Hífen (-)
- Apóstrofo (')
- Espaços

**Lógica:**
```
Entrada: "João D'Arc-Silvá Peréira Júnior"
Processamento:
  1. Trim espaços iniciais/finais
  2. Reduzir múltiplos espaços para um
  3. Preservar TUDO o resto
Armazenamento: "João D'Arc-Silvá Peréira Júnior"
Exibição: "João D'Arc-Silvá Peréira Júnior"
```

**Validação:**
- Não remover ou substituir acentos
- Não converter para maiúsculas/minúsculas
- Não remover hífens ou apóstrofos
- Encoding: UTF-8

**Dependências:** BR-019, BR-021

**Impacto:** Baixo - Nome incorreto pode causar problemas no pagamento efetivo

---

## Regras de Conversão Monetária

### BR-023: Fórmula de Conversão para BTNF

**Tier:** System-Critical
**Categoria:** Cálculo Financeiro
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 923

**Descrição:**
Todos os valores monetários DEVEM ser convertidos para a moeda padronizada BTNF (Bônus do Tesouro Nacional Fiscal) usando a fórmula EXATA do sistema legado.

**Fórmula:**
```
AMOUNT_BTNF = AMOUNT_ORIGINAL × VLCRUZAD

Onde:
  AMOUNT_ORIGINAL = Valor na moeda de trabalho
  VLCRUZAD = Taxa de conversão da tabela TGEUNIMO
  AMOUNT_BTNF = Valor convertido em BTNF
```

**Exemplo:**
```
Valor Original: R$ 10.000,00
Taxa (VLCRUZAD): 1,23456789 (8 decimais)
Cálculo: 10000.00 × 1.23456789
Resultado: 12345,68 (arredondado para 2 decimais)
```

**Validação:**
- Taxa VLCRUZAD DEVE ter 8 casas decimais (precisão)
- Resultado DEVE ser arredondado para 2 casas decimais
- Usar DECIMAL type (nunca FLOAT) para evitar erros de arredondamento
- Arredondamento: Banker's Rounding (IEEE 754)

**Dependências:** BR-024, BR-025

**Impacto:** CRÍTICO - Erro de cálculo afeta valores financeiros

---

### BR-024: Fonte de Taxa de Conversão

**Tier:** System-Critical
**Categoria:** Lookup de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 924

**Descrição:**
A taxa de conversão VLCRUZAD DEVE ser obtida da tabela TGEUNIMO, nunca hardcoded ou calculada.

**Query:**
```sql
SELECT VLCRUZAD
FROM TGEUNIMO
WHERE @data_transacao BETWEEN DTINIVIG AND DTTERVIG
```

**Comportamento:**
- Usar data da transação (TSISTEMA.DTMOVABE), não data/hora atual
- Query deve retornar exatamente 1 registro
- Se 0 registros: ERRO "Taxa não disponível"
- Se 2+ registros: ERRO de configuração (investigar DBA)

**Validação:**
- Taxa sempre > 0
- Taxa tipicamente próxima de 1,0 (verificar com área financeira)

**Dependências:** BR-035 (data transação), BR-025 (validação de range de datas)

**Impacto:** CRÍTICO - Taxa incorreta corrompe todos os valores

---

### BR-025: Validação de Data na Tabela TGEUNIMO

**Tier:** System-Critical
**Categoria:** Validação de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 925

**Descrição:**
O sistema DEVE validar que existe uma taxa de conversão válida para a data da transação, usando os campos DTINIVIG (data inicial vigência) e DTTERVIG (data término vigência).

**Lógica:**
```sql
WHERE @data_transacao >= DTINIVIG
  AND @data_transacao <= DTTERVIG
```

**Ambos limites são INCLUSIVOS:**
```
DTINIVIG = 2025-01-01
DTTERVIG = 2025-01-31
Data transação = 2025-01-01 ✅ VÁLIDO (limite inferior)
Data transação = 2025-01-15 ✅ VÁLIDO (meio do range)
Data transação = 2025-01-31 ✅ VÁLIDO (limite superior)
Data transação = 2025-02-01 ❌ INVÁLIDO (fora do range)
```

**Validação:**
- Ranges não devem ter gaps (validação do processo de manutenção de taxas)
- Ranges não devem se sobrepor (validação do processo)
- Data futura pode não ter taxa (validar antes de autorizar pagamento)

**Dependências:** BR-024

**Impacto:** CRÍTICO - Sem taxa, pagamento não pode ser processado

---

### BR-026: Erro Quando Taxa Não Disponível

**Tier:** System-Critical
**Categoria:** Mensagem de Erro
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 926

**Descrição:**
Se nenhuma taxa for encontrada para a data da transação, o sistema DEVE abortar a operação com mensagem de erro específica.

**Mensagem:**
```
"Taxa de conversão não disponível para a data do movimento"
```

**Especificações:**
- Exibir data do movimento para contexto
- Cor: #e80c4d (vermelho)
- Ação: ABORTAR transação completamente
- Não permitir override manual

**Exemplo de Display:**
```
┌─────────────────────────────────────────┐
│ ⚠️ ERRO CRÍTICO                          │
│                                         │
│ Taxa de conversão não disponível        │
│ para a data do movimento                │
│                                         │
│ Data Movimento: 27/10/2025             │
│                                         │
│ Contate o setor financeiro.            │
└─────────────────────────────────────────┘
```

**Ação do Operador:**
- Contatar setor financeiro para cadastrar taxa
- Não tentar novamente até confirmação

**Dependências:** BR-024, BR-025

**Impacto:** CRÍTICO - Operação bloqueada até resolver

---

### BR-027: Precisão da Taxa de Conversão

**Tier:** System-Critical
**Categoria:** Precisão Numérica
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 927

**Descrição:**
O campo VLCRUZAD DEVE armazenar e processar a taxa com exatamente 8 casas decimais para garantir precisão financeira.

**Tipo de Dados:**
```
SQL: DECIMAL(18,8)
C#: decimal
```

**Exemplos:**
```
1,23456789 ✅ CORRETO (8 decimais)
1,2345678  ❌ INCORRETO (7 decimais - padding com zero)
1,234      ❌ INCORRETO (3 decimais - padding com zeros)
```

**Validação:**
- Ao recuperar de TGEUNIMO, manter 8 decimais
- Ao fazer cálculos, não truncar durante operações intermediárias
- Apenas arredondar resultado final para 2 decimais

**Cálculo Correto:**
```
ValorOriginal = 10000.00 (DECIMAL(15,2))
VLCRUZAD = 1.23456789 (DECIMAL(18,8))
Resultado Intermediário = 12345.6789 (manter precisão)
Resultado Final = 12345.68 (arredondar para 2 decimais)
```

**Cálculo Incorreto:**
```
VLCRUZAD = 1.23 (perda de precisão)
Resultado = 12300.00 (diferença de R$ 45,68!)
```

**Dependências:** BR-023, BR-024

**Impacto:** CRÍTICO - Erro acumulativo em milhares de transações

---

### BR-028: Precisão do Resultado Final

**Tier:** Business-Critical
**Categoria:** Precisão Numérica
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 928

**Descrição:**
Todos os valores monetários armazenados e exibidos DEVEM ter exatamente 2 casas decimais (centavos).

**Tipo de Dados:**
```
SQL: DECIMAL(15,2)
C#: decimal
```

**Arredondamento:**
```
Usar: Banker's Rounding (MidpointRounding.ToEven)

Exemplos:
12345.6749 → 12345.67 (arredonda para baixo)
12345.6750 → 12345.68 (meio-termo, arredonda para par)
12345.6751 → 12345.68 (arredonda para cima)
12345.6850 → 12345.68 (meio-termo, par mais próximo)
```

**Implementação C#:**
```csharp
decimal valor = 12345.6789m;
decimal arredondado = Math.Round(valor, 2, MidpointRounding.ToEven);
// Resultado: 12345.68
```

**Validação:**
- NUNCA truncar (sempre arredondar)
- NUNCA usar FLOAT ou DOUBLE (usar DECIMAL)
- Aplicar arredondamento apenas no resultado final

**Dependências:** BR-023, BR-027

**Impacto:** Alto - Valores incorretos afetam conciliação contábil

---

### BR-029: Conversão de Valor Principal

**Tier:** System-Critical
**Categoria:** Cálculo Específico
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 929

**Descrição:**
O Valor Principal informado pelo operador DEVE ser convertido para BTNF usando a fórmula da BR-023 e armazenado no campo VALPRIBT da tabela THISTSIN.

**Lógica:**
```
VALPRIBT = VALPRI × VLCRUZAD

Onde:
  VALPRI = Valor Principal informado (entrada do operador)
  VLCRUZAD = Taxa de conversão (TGEUNIMO)
  VALPRIBT = Valor Principal em BTNF (armazenado em THISTSIN)
```

**Exemplo:**
```
Entrada operador: R$ 10.000,00
Taxa VLCRUZAD: 1,23456789
Cálculo: 10000.00 × 1.23456789 = 12345.6789
Arredondamento: 12345.68
VALPRIBT armazenado: 12345.68
```

**Validação:**
- VALPRI e VALPRIBT são campos SEPARADOS (ambos armazenados)
- VALPRI: valor na moeda original
- VALPRIBT: valor convertido em BTNF
- Ambos necessários para auditoria e conciliação

**Dependências:** BR-023, BR-024, BR-028

**Impacto:** CRÍTICO - Valor incorreto afeta pagamento

---

### BR-030: Conversão de Valor de Correção

**Tier:** System-Critical
**Categoria:** Cálculo Específico
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 930

**Descrição:**
O Valor de Correção (se informado) DEVE ser convertido para BTNF usando a mesma fórmula e armazenado no campo CRRMONBT.

**Lógica:**
```
CRRMONBT = CRRMON × VLCRUZAD

Onde:
  CRRMON = Valor Correção informado (entrada, pode ser 0)
  VLCRUZAD = Taxa de conversão (mesma usada em BR-029)
  CRRMONBT = Valor Correção em BTNF (armazenado em THISTSIN)
```

**Exemplo com correção:**
```
Entrada operador: R$ 500,00
Taxa VLCRUZAD: 1,23456789
Cálculo: 500.00 × 1.23456789 = 617.28395
Arredondamento: 617.28
CRRMONBT armazenado: 617.28
```

**Exemplo sem correção (BR-017):**
```
Entrada operador: (vazio)
CRRMON assume: 0.00
Cálculo: 0.00 × 1.23456789 = 0.00
CRRMONBT armazenado: 0.00
```

**Validação:**
- CRRMON e CRRMONBT: campos separados
- Se CRRMON = 0, CRRMONBT também será 0
- Usar mesma taxa (VLCRUZAD) para ambos os valores

**Dependências:** BR-017, BR-023, BR-029

**Impacto:** CRÍTICO - Afeta valor total da transação

---

### BR-031: Cálculo de Valor Total em BTNF

**Tier:** System-Critical
**Categoria:** Cálculo Agregado
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 931

**Descrição:**
O Valor Total em BTNF DEVE ser calculado como a soma simples de VALPRIBT + CRRMONBT e armazenado no campo VALTOTBT.

**Fórmula:**
```
VALTOTBT = VALPRIBT + CRRMONBT
```

**NÃO É:**
```
VALTOTBT ≠ (VALPRI + CRRMON) × VLCRUZAD  ❌ INCORRETO

Por quê? Potencial diferença de arredondamento.
```

**Exemplo:**
```
VALPRIBT = 12345.68 (já convertido e arredondado)
CRRMONBT = 617.28 (já convertido e arredondado)
VALTOTBT = 12345.68 + 617.28 = 12962.96 ✅ CORRETO
```

**Por que calcular após conversões individuais?**
```
Método Correto (usado pelo legado):
  VALPRI = 10000.00
  CRRMON = 500.00
  VALPRIBT = 10000.00 × 1.23456789 = 12345.68
  CRRMONBT = 500.00 × 1.23456789 = 617.28
  VALTOTBT = 12345.68 + 617.28 = 12962.96

Método Incorreto (se somarmos antes):
  VALTOTAL = 10500.00 × 1.23456789 = 12962.96 (aparentemente igual)

  MAS: Com números que geram arredondamentos diferentes,
       os resultados divergem!
```

**Validação:**
- Converter VALPRI primeiro
- Converter CRRMON depois
- Somar os resultados convertidos
- VALTOTBT DEVE ser decimal(15,2)

**Dependências:** BR-029, BR-030

**Impacto:** CRÍTICO - Este é o valor usado para atualizar TOTPAG do sinistro

---

### BR-032: Cálculo de Valores Diários

**Tier:** Business-Critical
**Categoria:** Cálculo Específico
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 932

**Descrição:**
O sistema DEVE calcular valores diários (provavelmente para projeções atuariais ou cálculo de juros) e armazená-los nos campos PRIDIABT, CRRDIABT e TOTDIABT.

**Lógica (hipotética - validar com área de negócio):**
```
PRIDIABT = VALPRIBT / DIAS_PERIODO
CRRDIABT = CRRMONBT / DIAS_PERIODO
TOTDIABT = VALTOTBT / DIAS_PERIODO

Onde DIAS_PERIODO pode ser:
  - Dias entre data evento e data pagamento
  - Dias do mês
  - 30 (convenção financeira)
  - Outro critério definido
```

**Exemplo (assumindo 30 dias):**
```
VALPRIBT = 12345.68
DIAS_PERIODO = 30
PRIDIABT = 12345.68 / 30 = 411.52 (arredondado para 2 decimais)
```

**⚠️ ATENÇÃO:**
Esta regra requer validação com área de negócio para entender:
- Como DIAS_PERIODO é determinado?
- Esses campos são usados em cálculos subsequentes?
- São apenas informativos?

**Validação:**
- Campos PRIDIABT, CRRDIABT, TOTDIABT existem em THISTSIN
- Tipo: DECIMAL(15,2)
- Se não utilizados, podem ser calculados como 0 ou NULL

**Dependências:** BR-029, BR-030, BR-031

**Impacto:** Médio - Pode afetar cálculos atuariais ou relatórios

---

### BR-033: Código da Moeda Padronizada

**Tier:** Operational
**Categoria:** Identificação
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 933

**Descrição:**
A moeda padronizada do sistema é BTNF (Bônus do Tesouro Nacional Fiscal). Todos os valores convertidos devem ser identificados com este código.

**Sigla:**
```
BTNF = Bônus do Tesouro Nacional Fiscal
```

**Uso:**
- Em logs: "Valor convertido para BTNF: 12345.68"
- Em documentação: "Valores armazenados em BTNF"
- Em campos de código de moeda (se existirem): "BTNF"

**Contexto Histórico:**
BTNF foi uma unidade monetária brasileira usada em períodos de alta inflação. No contexto do sistema, representa a "moeda base" para comparabilidade.

**Validação:**
- Verificar se existem campos CODE_MOEDA ou similar
- Garantir consistência em toda documentação

**Dependências:** Todas as regras BR-023 a BR-032

**Impacto:** Baixo - Principalmente documentação

---

## Regras de Registro de Transações

### BR-034: Código de Operação Fixo

**Tier:** System-Critical
**Categoria:** Constante de Sistema
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 945

**Descrição:**
O campo OPERACAO da tabela THISTSIN DEVE SEMPRE ser preenchido com o valor **1098** para todas as autorizações de pagamento. Este é um código FIXO e IMUTÁVEL.

**Lógica:**
```
THISTSIN.OPERACAO = 1098  // SEMPRE, sem exceções
```

**Significado:**
```
1098 = Código de operação "Autorização de Pagamento de Sinistro"
```

**Validação:**
- Valor HARDCODED na aplicação (não entrada do usuário)
- Nunca NULL
- Nunca outro valor
- Se histórico tem OPERACAO != 1098, foi criado por outro programa

**Uso:**
```csharp
// Constante da aplicação
public const int OPERACAO_AUTORIZACAO_PAGAMENTO = 1098;

// Ao inserir histórico
historyRecord.Operacao = OPERACAO_AUTORIZACAO_PAGAMENTO;
```

**Dependências:** Nenhuma

**Impacto:** CRÍTICO - Queries e relatórios filtram por OPERACAO=1098

---

### BR-035: Data de Transação do Sistema de Controle

**Tier:** System-Critical
**Categoria:** Data de Negócio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 946

**Descrição:**
A data da transação (DTMOVTO) DEVE ser obtida do campo DTMOVABE da tabela TSISTEMA (onde IDSISTEM='SI'), NÃO do relógio do sistema operacional.

**Lógica:**
```sql
-- Query ANTES de criar transação
SELECT DTMOVABE
FROM TSISTEMA
WHERE IDSISTEM = 'SI'
```

**Por quê?**
- Data comercial != data do sistema operacional
- Permite controle de fechamento contábil
- Banco pode "fechar" o dia 27/10 às 18h, mas SO continua em 27/10
- Próxima autorização após fechamento usará data 28/10 (mesmo que SO mostre 27/10 23:59)

**Exemplo:**
```
Sistema Operacional: 27/10/2025 18:30
TSISTEMA.DTMOVABE: 28/10/2025 (já fechou dia anterior)
DTMOVTO gravado: 28/10/2025 ✅ CORRETO
```

**Validação:**
- Query TSISTEMA deve retornar exatamente 1 registro
- Se não encontrar (IDSISTEM='SI'), ERRO CRÍTICO: "Sistema de controle não configurado"
- Data não pode estar muito no passado ou futuro (validação de sanidade)

**Dependências:** Nenhuma

**Impacto:** CRÍTICO - Data incorreta corrompe relatórios contábeis

---

### BR-036: Hora da Transação do Sistema Operacional

**Tier:** System-Critical
**Categoria:** Timestamp
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 947

**Descrição:**
A hora da transação (HORAOPER) DEVE ser a hora atual do sistema operacional no momento da criação do registro, formato HH:MM:SS.

**Lógica:**
```csharp
DateTime agora = DateTime.Now;
TimeSpan horaOperacao = agora.TimeOfDay;
// Ou
string horaOperacao = agora.ToString("HH:mm:ss");
```

**Formato:**
```
HH:MM:SS (24 horas)
Exemplos:
  14:30:15 (14h 30min 15seg)
  09:05:03 (9h 5min 3seg)
  00:00:01 (meia-noite e 1 segundo)
```

**Validação:**
- Usar horário local do servidor (Brasil: BRT/BRST)
- Precisão: segundos (não milissegundos)
- Formato: 24h (não AM/PM)

**Importante:**
- DATA vem de TSISTEMA (BR-035)
- HORA vem do sistema operacional (BR-036)
- Isso permite que múltiplas transações no mesmo "dia comercial" tenham horas diferentes

**Dependências:** BR-035

**Impacto:** Alto - Auditoria usa timestamp completo (data + hora)

---

### BR-037: Status Contábil Inicial

**Tier:** Business-Critical
**Categoria:** Inicialização de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 948

**Descrição:**
O campo SITCONTB (Situação Contábil) DEVE ser inicializado com o valor '0' (caractere zero) ao criar um novo registro de histórico.

**Lógica:**
```
THISTSIN.SITCONTB = '0'  // Inicialização padrão
```

**Tipo:**
```
SQL: CHAR(1)
C#: string (length 1)
Valor: '0' (caractere, não numérico)
```

**Significado (hipotético - validar com contabilidade):**
```
'0' = Não contabilizado / Pendente contabilização
'1' = Contabilizado
'2' = Estornado
'3' = Em análise
... (outros valores definidos pela área contábil)
```

**Validação:**
- Campo NUNCA deve ser NULL
- Sempre iniciar com '0'
- Será atualizado posteriormente por processo contábil (não pelo SIWEA)

**Dependências:** Nenhuma

**Impacto:** Médio - Processo contábil depende deste flag

---

### BR-038: Situação Geral Inicial

**Tier:** Business-Critical
**Categoria:** Inicialização de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 949

**Descrição:**
O campo SITUACAO (Situação Geral) DEVE ser inicializado com o valor '0' (caractere zero) ao criar um novo registro de histórico.

**Lógica:**
```
THISTSIN.SITUACAO = '0'  // Inicialização padrão
```

**Tipo:**
```
SQL: CHAR(1)
C#: string (length 1)
Valor: '0' (caractere, não numérico)
```

**Significado (hipotético - validar com área de negócio):**
```
'0' = Autorizado / Pendente confirmação
'1' = Confirmado / Pago
'2' = Cancelado
'3' = Estornado
... (outros valores definidos pela área)
```

**Diferença de SITCONTB:**
- SITCONTB: Status na contabilidade
- SITUACAO: Status geral do pagamento

**Validação:**
- Campo NUNCA deve ser NULL
- Sempre iniciar com '0'
- Pode ser atualizado por processos subsequentes

**Dependências:** Nenhuma

**Impacto:** Médio - Relatórios filtram por SITUACAO

---

### BR-039: Tipo de Correção Fixo

**Tier:** System-Critical
**Categoria:** Constante de Sistema
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 950

**Descrição:**
O campo TIPCRR (Tipo de Correção) DEVE SEMPRE ser preenchido com o valor '5' (caractere cinco) para todas as autorizações de pagamento.

**Lógica:**
```
THISTSIN.TIPCRR = '5'  // SEMPRE, sem exceções
```

**Tipo:**
```
SQL: CHAR(1)
C#: string (length 1)
Valor: '5' (caractere, não numérico)
```

**Significado:**
```
'5' = Tipo de correção padrão para autorizações de pagamento
       (significado exato deve ser validado com área financeira)
```

**Validação:**
- Valor HARDCODED na aplicação
- Nunca NULL
- Nunca outro valor para SIWEA
- Outros programas podem usar valores diferentes

**Uso:**
```csharp
// Constante
public const string TIPO_CORRECAO_PAGAMENTO = "5";

// Ao inserir
historyRecord.TipoCorrecao = TIPO_CORRECAO_PAGAMENTO;
```

**Dependências:** Nenhuma

**Impacto:** Alto - Relatórios financeiros filtram por TIPCRR

---

### BR-040: Incremento do Contador de Ocorrências

**Tier:** System-Critical
**Categoria:** Sequência
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 951

**Descrição:**
O campo OCORHIST (Contador de Ocorrências) DEVE ser incrementado em 1 a cada novo pagamento autorizado no sinistro.

**Lógica:**
```sql
-- 1. Ler valor atual
SELECT OCORHIST
FROM TMESTSIN
WHERE TIPSEG = @tipseg
  AND ORGSIN = @orgsin
  AND RMOSIN = @rmosin
  AND NUMSIN = @numsin
FOR UPDATE;  -- Lock pessimista

-- 2. Calcular próximo valor
NOVO_OCORHIST = OCORHIST_ATUAL + 1

-- 3. Inserir histórico com novo valor
INSERT INTO THISTSIN (..., OCORHIST, ...)
VALUES (..., NOVO_OCORHIST, ...)

-- 4. Atualizar mestre
UPDATE TMESTSIN
SET OCORHIST = NOVO_OCORHIST
WHERE ... (mesma chave)
```

**Concorrência:**
- Usar lock otimista (ROW_VERSION em TMESTSIN)
- Ou lock pessimista (SELECT FOR UPDATE)
- Prevenir que dois operadores criem histórico com mesmo OCORHIST

**Validação:**
- OCORHIST começa em 1 (primeiro pagamento)
- Incrementa monotonicamente (nunca decresce)
- Sem gaps (1, 2, 3, 4... não 1, 2, 4, 5)
- OCORHIST é parte da PK de THISTSIN

**Dependências:** Nenhuma

**Impacto:** CRÍTICO - Chave primária de THISTSIN inclui OCORHIST

---

### BR-041: Registro do ID do Operador

**Tier:** System-Critical
**Categoria:** Auditoria
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 952

**Descrição:**
O sistema DEVE registrar o ID do operador que autorizou o pagamento no campo EZEUSRID da tabela THISTSIN.

**Lógica:**
```csharp
// Obter usuário autenticado
string operadorId = User.Identity.Name; // ou HttpContext.User.Identity.Name

// Registrar em THISTSIN
historyRecord.OperadorId = operadorId;
```

**Formato:**
```
Tipo: VARCHAR(50)
Exemplo: "USER001", "bruno.souza", "12345678901" (CPF)
```

**Fonte do ID:**
- Usuário autenticado no AD / Sistema de autenticação
- Nunca aceitar como entrada do formulário
- Nunca hardcoded
- Obtido de forma automática e confiável

**Validação:**
- EZEUSRID NUNCA deve ser NULL
- Deve existir no sistema de autenticação
- Usado em auditoria e relatórios

**Dependências:** Sistema de autenticação empresarial

**Impacto:** CRÍTICO - Auditoria e compliance dependem deste campo

---

### BR-042: Campos de Auditoria Padrão

**Tier:** System-Critical
**Categoria:** Auditoria
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 953

**Descrição:**
Todos os registros criados/atualizados DEVEM preencher os campos de auditoria padrão: CREATED_BY, CREATED_AT, UPDATED_BY, UPDATED_AT.

**Lógica:**
```csharp
// Na inserção
entity.CreatedBy = currentUserId;
entity.CreatedAt = DateTime.UtcNow;
entity.UpdatedBy = null;  // NULL em inserção
entity.UpdatedAt = null;  // NULL em inserção

// Na atualização
entity.UpdatedBy = currentUserId;
entity.UpdatedAt = DateTime.UtcNow;
// CreatedBy e CreatedAt permanecem inalterados
```

**Tipos:**
```
CREATED_BY: VARCHAR(50)
CREATED_AT: DATETIME
UPDATED_BY: VARCHAR(50) NULL
UPDATED_AT: DATETIME NULL
```

**Validação:**
- CREATED_BY e CREATED_AT: sempre preenchidos, nunca NULL
- UPDATED_BY e UPDATED_AT: NULL em inserção, preenchidos em atualização
- Usar UTC para timestamps (evitar problemas de timezone)
- Converter para horário local apenas na exibição

**Dependências:** BR-041 (ID do operador)

**Impacto:** CRÍTICO - Auditoria completa depende desses campos

---

## Regras de Validação de Produtos

### BR-043: Roteamento de Produtos Consórcio

**Tier:** System-Critical
**Categoria:** Roteamento de Validação
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 978

**Descrição:**
Sinistros com produtos de consórcio (códigos 6814, 7701, 7709) DEVEM ser roteados para validação no serviço externo CNOUA (Consórcio Nacional de Ouvidoria e Atendimento).

**Lógica:**
```
SE TMESTSIN.CODPRODU IN (6814, 7701, 7709) ENTÃO
    CHAMAR Serviço_CNOUA(protocolo, valor, contrato)
    SE resposta.EZERT8 != '00000000' ENTÃO
        ABORTAR TRANSAÇÃO com mensagem de erro
    FIM SE
SENÃO
    Seguir para BR-044 (verificar EFP/HB)
FIM SE
```

**Produtos Consórcio:**
```
6814 = Consórcio Habitacional
7701 = Consórcio Automóvel
7709 = Consórcio Serviços
```

**Validação:**
- Produtos consórcio TÊM PRIORIDADE sobre outras validações
- Se produto é consórcio, NÃO valida em SIPUA ou SIMDA
- Validação CNOUA é bloqueante (não prossegue se falhar)

**Dependências:** BR-003 (CODPRODU recuperado do sinistro)

**Impacto:** CRÍTICO - Pagamento de consórcio sem validação pode violar compliance

---

### BR-044: Query de Contrato Habitacional

**Tier:** System-Critical
**Categoria:** Lookup de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 980

**Descrição:**
Para produtos NÃO-CONSÓRCIO, o sistema DEVE consultar a tabela EF_CONTR_SEG_HABIT para determinar o número do contrato associado à apólice.

**Query:**
```sql
SELECT NUM_CONTRATO
FROM EF_CONTR_SEG_HABIT
WHERE ORGAPO = @orgapo
  AND RMOAPO = @rmoapo
  AND NUMAPOL = @numapol
```

**Comportamento:**
- Se encontrado E NUM_CONTRATO > 0 → Produto EFP (BR-045)
- Se encontrado E NUM_CONTRATO = 0 → Produto HB (BR-046)
- Se NÃO encontrado → Produto HB (BR-046)

**Validação:**
- Query retorna 0 ou 1 registro (ORGAPO+RMOAPO+NUMAPOL é chave única)
- NUM_CONTRATO é INTEGER, pode ser NULL ou 0

**Dependências:** BR-043 (executado apenas se não for consórcio)

**Impacto:** CRÍTICO - Roteamento incorreto leva a validação errada

---

### BR-045: Roteamento EFP (Contrato > 0)

**Tier:** System-Critical
**Categoria:** Roteamento de Validação
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 983

**Descrição:**
Sinistros com NUM_CONTRATO > 0 na tabela EF_CONTR_SEG_HABIT DEVEM ser roteados para validação no serviço SIPUA (Sistema de Processamento de Unidades Autônomas).

**Lógica:**
```
SE NUM_CONTRATO > 0 ENTÃO
    CHAMAR Serviço_SIPUA(numContrato, protocolo, valor)
    SE resposta.EZERT8 != '00000000' ENTÃO
        ABORTAR TRANSAÇÃO com mensagem de erro
    FIM SE
SENÃO
    Seguir para BR-046 (validação HB)
FIM SE
```

**Validação:**
- SIPUA valida contratos EFP (Empreendimento de Financiamento Próprio)
- Timeout: 10 segundos
- Retry: 3 tentativas com backoff exponencial

**Dependências:** BR-044 (NUM_CONTRATO obtido da query)

**Impacto:** CRÍTICO - Validação incorreta pode autorizar pagamento indevido

---

### BR-046: Roteamento HB (Contrato = 0 ou Não Encontrado)

**Tier:** System-Critical
**Categoria:** Roteamento de Validação
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 984

**Descrição:**
Sinistros com NUM_CONTRATO = 0 ou não encontrado em EF_CONTR_SEG_HABIT DEVEM ser roteados para validação no serviço SIMDA (Sistema de Habitação).

**Lógica:**
```
SE NUM_CONTRATO = 0 OU NUM_CONTRATO IS NULL ENTÃO
    CHAMAR Serviço_SIMDA(protocolo, apólice, valor)
    SE resposta.EZERT8 != '00000000' ENTÃO
        ABORTAR TRANSAÇÃO com mensagem de erro
    FIM SE
FIM SE
```

**Validação:**
- SIMDA valida contratos HB (Habitação)
- Timeout: 10 segundos
- Retry: 3 tentativas com backoff exponencial

**Dependências:** BR-044, BR-045

**Impacto:** CRÍTICO - Roteamento final (fallback)

---

### BR-047: Código de Sucesso EZERT8

**Tier:** System-Critical
**Categoria:** Validação de Resposta
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 987

**Descrição:**
A validação externa é considerada BEM-SUCEDIDA se e somente se o campo EZERT8 da resposta for exatamente '00000000' (8 zeros).

**Lógica:**
```
RESPOSTA validacao_externa = chamar_servico(...)

SE RESPOSTA.EZERT8 == '00000000' ENTÃO
    VALIDAÇÃO_OK = TRUE
    Prosseguir com transação
SENÃO
    VALIDAÇÃO_OK = FALSE
    ABORTAR com erro específico (ver BR-048 a BR-056)
FIM SE
```

**Formato EZERT8:**
```
Tipo: STRING (8 caracteres)
Sucesso: "00000000"
Erro: Código específico (ex: "EZERT8001", "EZERT8002", etc.)
```

**Validação:**
- Comparação case-sensitive
- Exatamente 8 caracteres
- String, não numérico

**Dependências:** BR-043, BR-045, BR-046

**Impacto:** CRÍTICO - Determina se pagamento pode ser autorizado

---

### BR-048: Indicador de Falha EZERT8

**Tier:** System-Critical
**Categoria:** Validação de Resposta
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 988

**Descrição:**
Qualquer valor de EZERT8 diferente de '00000000' indica falha na validação e DEVE abortar a transação imediatamente.

**Lógica:**
```
SE RESPOSTA.EZERT8 != '00000000' ENTÃO
    mensagemErro = MAPEAR_ERRO(RESPOSTA.EZERT8)
    LOG_ERRO(RESPOSTA.EZERT8, mensagemErro, RESPOSTA.detalhe)
    ABORTAR_TRANSAÇÃO()
    EXIBIR_MENSAGEM_USUARIO(mensagemErro)
FIM SE
```

**Ação:**
- NÃO criar registro em THISTSIN
- NÃO atualizar TMESTSIN.TOTPAG
- NÃO criar registros de fase ou acompanhamento
- Rollback completo da transação

**Dependências:** BR-047

**Impacto:** CRÍTICO - Previne pagamentos não autorizados

---

### BR-049: Mensagem Descritiva de Erro

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 989

**Descrição:**
O serviço de validação externo DEVE retornar uma mensagem descritiva em português junto com o código EZERT8 para facilitar diagnóstico.

**Estrutura de Resposta:**
```json
{
  "status": "REJECTED",
  "ezert8Code": "EZERT8003",
  "message": "Grupo encerrado",
  "detail": "Grupo 12345 encerrado em 15/09/2024"
}
```

**Validação:**
- Campo "message" obrigatório em caso de erro
- Idioma: português
- Máximo 255 caracteres

**Exibição ao Usuário:**
```
┌─────────────────────────────────────────┐
│ ⚠️ VALIDAÇÃO REPROVADA                   │
│                                         │
│ Grupo encerrado                         │
│                                         │
│ Grupo 12345 encerrado em 15/09/2024    │
│                                         │
│ Código: EZERT8003                      │
└─────────────────────────────────────────┘
```

**Dependências:** BR-048

**Impacto:** Médio - Facilita diagnóstico de erros

---

### BR-050: Interrupção da Autorização

**Tier:** System-Critical
**Categoria:** Controle de Fluxo
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 990

**Descrição:**
Se a validação externa falhar, o sistema DEVE interromper IMEDIATAMENTE o processo de autorização e NÃO prosseguir para o Step 4 (Insert THISTSIN).

**Fluxo:**
```
Step 1: Buscar sinistro ✅
Step 2: Validar entrada ✅
Step 3a: Consultar produto ✅
Step 3b: Chamar validação externa
    SE EZERT8 != '00000000' ENTÃO
        🛑 PARAR AQUI
        NÃO executar Steps 4-8
        RETORNAR ERRO ao usuário
    FIM SE
Step 4: Insert THISTSIN (não executado)
...
```

**Validação:**
- NÃO usar try-catch para "contornar" erro
- NÃO log como "warning" (é ERRO)
- NÃO permitir override manual pelo operador

**Dependências:** BR-048, BR-051

**Impacto:** CRÍTICO - Previne transações parciais

---

### BR-051: Rollback de Transação

**Tier:** System-Critical
**Categoria:** Atomicidade
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 994

**Descrição:**
Se a validação externa falhar, TODAS as mudanças no banco de dados até aquele ponto DEVEM ser desfeitas (rolled back).

**Lógica de Transação:**
```sql
BEGIN TRANSACTION

-- Step 1-3: Read operations (reversíveis naturalmente)

-- Step 3b: Validação externa
EXEC @ezert8 = chamar_validacao_externa(...)

IF @ezert8 != '00000000'
BEGIN
    ROLLBACK TRANSACTION
    RAISERROR('Validação externa falhou: %s', 16, 1, @mensagemErro)
    RETURN
END

-- Step 4-8: Write operations
...

COMMIT TRANSACTION
```

**Validação:**
- Usar transações de banco de dados, não "pseudo-transações" em código
- Isolation Level: READ COMMITTED ou superior
- Timeout de transação: 90 segundos (BR-097)

**Dependências:** BR-050

**Impacto:** CRÍTICO - Garante consistência do banco

---

### BR-052: Erro Contrato Consórcio Inválido

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro Específica
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 996

**Descrição:**
Código EZERT8001 indica que o número do contrato de consórcio não existe ou está incorreto.

**Mensagem:**
```
"Contrato de consórcio inválido"
```

**Código:** EZERT8001

**Possíveis Causas:**
- Número do contrato digitado incorretamente
- Contrato não existe na base CNOUA
- Contrato pertence a outra instituição
- Contrato foi transferido

**Ação do Operador:**
- Verificar número do contrato com cliente
- Consultar sistema CNOUA diretamente
- Contatar setor de consórcios

**Dependências:** BR-043, BR-049

**Impacto:** Alto - Bloqueio de pagamento legítimo requer investigação

---

### BR-053: Erro Contrato Cancelado

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro Específica
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 998

**Descrição:**
Código EZERT8002 indica que o contrato existe mas foi cancelado e não permite mais pagamentos.

**Mensagem:**
```
"Contrato cancelado"
```

**Código:** EZERT8002

**Possíveis Causas:**
- Cliente cancelou consórcio
- Inadimplência prolongada levou ao cancelamento
- Cancelamento por desistência
- Cancelamento por morte do consorciado

**Ação do Operador:**
- Verificar motivo do cancelamento
- Consultar se há direito a reembolso
- Não autorizar pagamento de sinistro

**Dependências:** BR-043, BR-049

**Impacto:** Alto - Pagamento em contrato cancelado é indevido

---

### BR-054: Erro Grupo Encerrado

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro Específica
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1000

**Descrição:**
Código EZERT8003 indica que o grupo do consórcio foi encerrado e não aceita mais movimentações.

**Mensagem:**
```
"Grupo encerrado"
```

**Código:** EZERT8003

**Possíveis Causas:**
- Grupo completou todos os contemplamentos
- Grupo foi encerrado administrativamente
- Prazo do grupo expirou

**Ação do Operador:**
- Verificar data de encerramento do grupo
- Consultar se sinistro ocorreu antes do encerramento
- Pode requerer exceção manual de área específica

**Dependências:** BR-043, BR-049

**Impacto:** Alto - Requer análise de datas para determinar elegibilidade

---

### BR-055: Erro Cota Não Contemplada

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro Específica
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1002

**Descrição:**
Código EZERT8004 indica que a cota do consórcio ainda não foi contemplada e portanto não há direito ao pagamento.

**Mensagem:**
```
"Cota não contemplada"
```

**Código:** EZERT8004

**Possíveis Causas:**
- Cota ainda em fase de pagamento de parcelas
- Não foi sorteada nem deu lance
- Cliente ainda não foi contemplado

**Ação do Operador:**
- Não autorizar pagamento
- Informar cliente sobre status da contemplação
- Aguardar contemplação para processar sinistro

**Dependências:** BR-043, BR-049

**Impacto:** Alto - Pagamento sem contemplação é violação de regras de consórcio

---

### BR-056: Erro Beneficiário Não Autorizado

**Tier:** Business-Critical
**Categoria:** Mensagem de Erro Específica
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1004

**Descrição:**
Código EZERT8005 indica que o beneficiário informado não está autorizado a receber pagamentos deste contrato de consórcio.

**Mensagem:**
```
"Beneficiário não autorizado"
```

**Código:** EZERT8005

**Possíveis Causas:**
- Beneficiário não consta no contrato
- Beneficiário foi removido/substituído
- Nome digitado incorretamente
- CPF não confere com cadastro

**Ação do Operador:**
- Verificar lista de beneficiários autorizados no CNOUA
- Confirmar grafia correta do nome
- Solicitar atualização cadastral se necessário

**Dependências:** BR-019 (beneficiário obrigatório), BR-043, BR-049

**Impacto:** Alto - Previne pagamento a terceiros não autorizados

---

## Regras de Gestão de Fases

### BR-057: Criação de Registro de Acompanhamento

**Tier:** System-Critical
**Categoria:** Auditoria de Eventos
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1010

**Descrição:**
Para cada autorização de pagamento, o sistema DEVE criar um registro na tabela SI_ACOMPANHA_SINI com COD_EVENTO = 1098.

**Query:**
```sql
INSERT INTO SI_ACOMPANHA_SINI (
    FONTE, PROTSINI, DAC, COD_EVENTO, NUM_OCORR_SINIACO,
    DATA_INIVIG_REFAEV, DATA_EVENTO, HORA_EVENTO,
    OBS_SINIACO, COD_USUARIO, CREATED_BY, CREATED_AT
) VALUES (
    @fonte, @protsini, @dac, 1098, @ocorhist,
    @data_inivig_refaev, @business_date, @hora_sistema,
    'Autorização de Pagamento - Valor: R$ ' + CONVERT(VARCHAR, @valtotbt),
    @user_id, @user_id, GETDATE()
)
```

**Campos Importantes:**
- COD_EVENTO: Sempre 1098 (autorização de pagamento)
- NUM_OCORR_SINIACO: Mesmo valor de OCORHIST (vincula ao histórico)
- DATA_EVENTO: Data de negócio (TSISTEMA.DTMOVABE)
- HORA_EVENTO: Hora do sistema (DateTime.Now)
- COD_USUARIO: ID do operador autenticado

**Validação:**
- Registro criado ANTES de atualizar fases
- Faz parte da mesma transação (rollback se falhar)

**Dependências:** BR-035 (data), BR-036 (hora), BR-041 (usuário)

**Impacto:** CRÍTICO - Trilha de auditoria incompleta sem este registro

---

### BR-058: Configuração de Mudanças de Fase

**Tier:** System-Critical
**Categoria:** Lookup de Configuração
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1012

**Descrição:**
As mudanças de fase (aberturas e fechamentos) para o evento 1098 DEVEM ser determinadas pela tabela de configuração SI_REL_FASE_EVENTO.

**Query:**
```sql
SELECT COD_FASE, IND_ALTERACAO_FASE, DATA_INIVIG_REFAEV
FROM SI_REL_FASE_EVENTO
WHERE COD_EVENTO = 1098
  AND DATA_INIVIG_REFAEV <= @business_date
ORDER BY DATA_INIVIG_REFAEV DESC, COD_FASE
```

**Retorno:**
```
Pode retornar 0, 1 ou múltiplos registros

Exemplo:
COD_FASE | IND_ALTERACAO_FASE | DATA_INIVIG_REFAEV
---------|-------------------|------------------
   05    |        '1'        | 2020-01-01
   03    |        '2'        | 2020-01-01
   07    |        '1'        | 2020-01-01
```

**Significado:**
- IND_ALTERACAO_FASE = '1': ABRIR fase
- IND_ALTERACAO_FASE = '2': FECHAR fase

**Validação:**
- Configuração pode mudar ao longo do tempo (DATA_INIVIG_REFAEV)
- Usar configuração vigente na data da transação
- Processar TODAS as linhas retornadas (pode abrir E fechar fases)

**Dependências:** BR-035 (business_date)

**Impacto:** CRÍTICO - Workflow do sinistro depende desta configuração

---

### BR-059: Abertura de Fase

**Tier:** System-Critical
**Categoria:** Gestão de Estado
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1014

**Descrição:**
Quando IND_ALTERACAO_FASE = '1', o sistema DEVE criar um novo registro em SI_SINISTRO_FASE com DATA_FECHA_SIFA = '9999-12-31' (sentinela de "fase aberta").

**Lógica:**
```sql
-- 1. Verificar se fase já está aberta
SELECT COUNT(*)
FROM SI_SINISTRO_FASE
WHERE FONTE = @fonte AND PROTSINI = @protsini AND DAC = @dac
  AND COD_FASE = @cod_fase
  AND COD_EVENTO = 1098
  AND DATA_FECHA_SIFA = '9999-12-31'

-- 2. Se COUNT = 0, inserir nova fase
IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO SI_SINISTRO_FASE (
        FONTE, PROTSINI, DAC, COD_FASE, COD_EVENTO,
        NUM_OCORR_SIFA, DATA_INIVIG_REFAEV,
        DATA_ABERTURA_SIFA, DATA_FECHA_SIFA,
        CREATED_BY, CREATED_AT
    ) VALUES (
        @fonte, @protsini, @dac, @cod_fase, 1098,
        @ocorhist, @data_inivig_refaev,
        @business_date, '9999-12-31',
        @user_id, GETDATE()
    )
END
```

**Significado de 9999-12-31:**
- Data sentinela indicando "fase ainda não encerrada"
- Permite queries simples: `WHERE DATA_FECHA_SIFA = '9999-12-31'` = fases abertas
- Data impossível na prática

**Validação:**
- Prevenir duplicatas (BR-063)
- DATA_ABERTURA_SIFA = data de negócio (BR-035)

**Dependências:** BR-058, BR-063

**Impacto:** CRÍTICO - Fases abertas controlam workflow

---

### BR-060: Fechamento de Fase

**Tier:** System-Critical
**Categoria:** Gestão de Estado
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1016

**Descrição:**
Quando IND_ALTERACAO_FASE = '2', o sistema DEVE localizar a fase aberta correspondente e atualizar DATA_FECHA_SIFA com a data de negócio atual.

**Lógica:**
```sql
UPDATE SI_SINISTRO_FASE
SET DATA_FECHA_SIFA = @business_date,
    UPDATED_BY = @user_id,
    UPDATED_AT = GETDATE()
WHERE FONTE = @fonte
  AND PROTSINI = @protsini
  AND DAC = @dac
  AND COD_FASE = @cod_fase
  AND COD_EVENTO = 1098
  AND DATA_FECHA_SIFA = '9999-12-31'
```

**Comportamento:**
- Se nenhum registro afetado (@@ROWCOUNT = 0): Fase não estava aberta (pode ignorar ou logar warning)
- Se 1 registro afetado: Sucesso
- Se múltiplos registros afetados: ERRO - dados corrompidos (investigar)

**Validação:**
- Só atualizar fases com DATA_FECHA_SIFA = '9999-12-31'
- DATA_FECHA_SIFA recebe data de negócio (não data do sistema)
- Atualizar UPDATED_BY e UPDATED_AT

**Dependências:** BR-035, BR-058, BR-059

**Impacto:** CRÍTICO - Fase não fechada pode bloquear outros processos

---

### BR-061: Indicador de Fase Aberta

**Tier:** System-Critical
**Categoria:** Convenção de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1018

**Descrição:**
O sistema DEVE usar a data sentinela '9999-12-31' no campo DATA_FECHA_SIFA como indicador de que a fase ainda está aberta.

**Convenção:**
```
DATA_FECHA_SIFA = '9999-12-31' → Fase ABERTA
DATA_FECHA_SIFA = qualquer outra data → Fase FECHADA naquela data
```

**Queries Comuns:**
```sql
-- Listar fases abertas
SELECT *
FROM SI_SINISTRO_FASE
WHERE DATA_FECHA_SIFA = '9999-12-31'

-- Listar fases fechadas
SELECT *
FROM SI_SINISTRO_FASE
WHERE DATA_FECHA_SIFA < '9999-12-31'

-- Duração de fases fechadas
SELECT COD_FASE,
       DATA_ABERTURA_SIFA,
       DATA_FECHA_SIFA,
       DATEDIFF(DAY, DATA_ABERTURA_SIFA, DATA_FECHA_SIFA) AS DIAS_ABERTA
FROM SI_SINISTRO_FASE
WHERE DATA_FECHA_SIFA < '9999-12-31'
```

**Validação:**
- NUNCA usar NULL para fase aberta (usar 9999-12-31)
- DATA_FECHA_SIFA é NOT NULL
- Tipo: DATE

**Dependências:** BR-059, BR-060

**Impacto:** Alto - Convenção usada em múltiplos sistemas

---

### BR-062: Data de Abertura de Fase

**Tier:** Business-Critical
**Categoria:** Timestamp
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1020

**Descrição:**
O campo DATA_ABERTURA_SIFA DEVE ser preenchido com a data de negócio (TSISTEMA.DTMOVABE) no momento da criação da fase.

**Lógica:**
```sql
-- Obter data de negócio
DECLARE @business_date DATE
SELECT @business_date = DTMOVABE FROM TSISTEMA WHERE IDSISTEM = 'SI'

-- Usar na abertura de fase
INSERT INTO SI_SINISTRO_FASE (...)
VALUES (..., @business_date, '9999-12-31', ...)
```

**Validação:**
- Usar data de negócio, NÃO GETDATE() ou DateTime.Now
- DATA_ABERTURA_SIFA <= DATA_FECHA_SIFA (sempre)
- DATA_ABERTURA_SIFA é NOT NULL

**Dependências:** BR-035 (business date), BR-059

**Impacto:** Médio - Afeta cálculos de SLA e relatórios

---

### BR-063: Prevenção de Fases Duplicadas

**Tier:** System-Critical
**Categoria:** Validação de Integridade
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1022

**Descrição:**
O sistema DEVE prevenir a criação de múltiplas fases abertas para a mesma combinação de sinistro + fase + evento.

**Validação:**
```sql
-- Verificar ANTES de inserir
IF EXISTS (
    SELECT 1
    FROM SI_SINISTRO_FASE
    WHERE FONTE = @fonte
      AND PROTSINI = @protsini
      AND DAC = @dac
      AND COD_FASE = @cod_fase
      AND COD_EVENTO = @cod_evento
      AND DATA_FECHA_SIFA = '9999-12-31'
)
BEGIN
    -- Fase já aberta, NÃO inserir novamente
    RAISERROR('Fase %d já está aberta para este sinistro', 16, 1, @cod_fase)
    RETURN
END
```

**Abordagem Alternativa:**
```sql
-- Constraint único
CREATE UNIQUE INDEX IX_FASE_ABERTA_UNICA
ON SI_SINISTRO_FASE (FONTE, PROTSINI, DAC, COD_FASE, COD_EVENTO)
WHERE DATA_FECHA_SIFA = '9999-12-31'
```
*(filtered index em SQL Server)*

**Dependências:** BR-059

**Impacto:** CRÍTICO - Duplicatas corrompem workflow

---

### BR-064: Processamento de Múltiplos Relacionamentos

**Tier:** System-Critical
**Categoria:** Processamento em Lote
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1024

**Descrição:**
O sistema DEVE processar TODOS os relacionamentos retornados da query SI_REL_FASE_EVENTO, pois o evento 1098 pode abrir E fechar múltiplas fases simultaneamente.

**Lógica:**
```csharp
// Query retorna múltiplas linhas
var relationships = await _db.PhaseEventRelationships
    .Where(r => r.EventCode == 1098 && r.EffectiveDate <= businessDate)
    .OrderBy(r => r.EffectiveDate)
    .ThenBy(r => r.PhaseCode)
    .ToListAsync();

// Processar TODAS as linhas
foreach (var rel in relationships)
{
    if (rel.PhaseChangeIndicator == '1')
        await OpenPhaseAsync(rel.PhaseCode, rel.EffectiveDate);
    else if (rel.PhaseChangeIndicator == '2')
        await ClosePhaseAsync(rel.PhaseCode);
}
```

**Exemplo de Configuração:**
```
Evento 1098 pode:
- Abrir fase 05 (Pagamento Autorizado)
- Fechar fase 03 (Análise Pendente)
- Abrir fase 07 (Aguardando Contabilização)
```

**Validação:**
- NÃO parar no primeiro relacionamento
- Processar em ordem de DATA_INIVIG_REFAEV
- Se um processamento falhar, rollback de TODOS

**Dependências:** BR-058, BR-059, BR-060

**Impacto:** CRÍTICO - Workflow pode requerer múltiplas mudanças

---

### BR-065: Ordem de Processamento de Fases

**Tier:** Business-Critical
**Categoria:** Sequência de Operações
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1026

**Descrição:**
Relacionamentos fase-evento DEVEM ser processados em ordem de DATA_INIVIG_REFAEV ASC, depois COD_FASE ASC.

**Lógica:**
```sql
SELECT COD_FASE, IND_ALTERACAO_FASE, DATA_INIVIG_REFAEV
FROM SI_REL_FASE_EVENTO
WHERE COD_EVENTO = 1098
  AND DATA_INIVIG_REFAEV <= @business_date
ORDER BY DATA_INIVIG_REFAEV ASC,  -- Mais antiga primeiro
         COD_FASE ASC              -- Menor código primeiro
```

**Por quê?**
- Configurações mais antigas têm precedência
- Fases com códigos menores geralmente são precedentes no workflow
- Garante ordem determinística

**Validação:**
- Se múltiplas configs para mesma data, ordem por COD_FASE
- Não usar ORDER BY aleatória ou não especificada

**Dependências:** BR-058, BR-064

**Impacto:** Médio - Ordem incorreta pode causar workflow inconsistente

---

### BR-066: Rollback de Fases

**Tier:** System-Critical
**Categoria:** Atomicidade
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1028

**Descrição:**
Se QUALQUER operação de fase falhar (abertura ou fechamento), TODAS as mudanças de fase E mudanças financeiras DEVEM ser revertidas.

**Lógica de Transação:**
```sql
BEGIN TRANSACTION

-- Step 4: Insert THISTSIN
...

-- Step 5: Update TMESTSIN
...

-- Step 6: Insert SI_ACOMPANHA_SINI
...

-- Step 7: Update fases
BEGIN TRY
    -- Processar todos os relacionamentos
    ...
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION
    RAISERROR('Erro ao atualizar fases: %s', 16, 1, ERROR_MESSAGE())
    RETURN
END CATCH

COMMIT TRANSACTION
```

**Validação:**
- Steps 4-7 TODOS dentro da mesma transação
- Se Step 7 falha, Steps 4-6 também são revertidos
- Isolation Level: READ COMMITTED ou superior

**Dependências:** BR-051 (rollback), BR-064

**Impacto:** CRÍTICO - Inconsistência de dados sem rollback

---

### BR-067: Atomicidade Total da Transação

**Tier:** System-Critical
**Categoria:** Princípio ACID
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1030

**Descrição:**
A autorização de pagamento DEVE ser uma transação ATÔMICA: ou TUDO acontece (Steps 4-7), ou NADA acontece.

**Princípio All-or-Nothing:**
```
Transação Bem-Sucedida:
  ✅ THISTSIN inserido
  ✅ TMESTSIN atualizado
  ✅ SI_ACOMPANHA_SINI inserido
  ✅ SI_SINISTRO_FASE atualizado
  ✅ COMMIT

Transação Falha:
  ❌ THISTSIN NÃO inserido
  ❌ TMESTSIN NÃO atualizado
  ❌ SI_ACOMPANHA_SINI NÃO inserido
  ❌ SI_SINISTRO_FASE NÃO atualizado
  ❌ ROLLBACK
```

**Validação:**
- NÃO permitir "commit parcial"
- NÃO logar falhas como "avisos" - são ERROS
- Operador deve ver claramente: sucesso ou falha total

**Dependências:** BR-051, BR-066

**Impacto:** CRÍTICO - Fundamento da integridade do sistema

---

## Regras de Auditoria

### BR-068: ID do Operador em Histórico

**Tier:** System-Critical
**Categoria:** Auditoria
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1036

**Descrição:**
O campo EZEUSRID da tabela THISTSIN DEVE registrar o ID do operador autenticado que autorizou o pagamento.

**Lógica:**
```csharp
// Obter do contexto de autenticação
var operatorId = _httpContextAccessor.HttpContext.User.Identity.Name;
// OU
var operatorId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

// Registrar em THISTSIN
historyRecord.OperatorId = operatorId;
```

**Formato:**
```
Tipo: VARCHAR(50)
Exemplos:
  - "12345678901" (CPF)
  - "F0123456" (matrícula funcional)
  - "bruno.souza@caixaseguradora.com.br" (email)
```

**Validação:**
- NUNCA NULL
- NUNCA hardcoded
- NUNCA aceitar como input do usuário
- Sempre obtido do sistema de autenticação

**Dependências:** Sistema de autenticação empresarial

**Impacto:** CRÍTICO - Auditoria e compliance

---

### BR-069: ID do Operador em Acompanhamento

**Tier:** System-Critical
**Categoria:** Auditoria
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1038

**Descrição:**
O campo COD_USUARIO da tabela SI_ACOMPANHA_SINI DEVE registrar o ID do operador.

**Lógica:**
```sql
INSERT INTO SI_ACOMPANHA_SINI (
    ..., COD_USUARIO, ...
) VALUES (
    ..., @current_user_id, ...
)
```

**Validação:**
- Mesmo valor usado em THISTSIN.EZEUSRID
- NUNCA NULL
- Permite rastrear quem executou cada evento

**Dependências:** BR-068

**Impacto:** CRÍTICO - Trilha de auditoria completa

---

### BR-070: ID do Operador em Fases

**Tier:** System-Critical
**Categoria:** Auditoria
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1040

**Descrição:**
Os campos CREATED_BY e UPDATED_BY da tabela SI_SINISTRO_FASE DEVEM registrar o ID do operador.

**Lógica:**
```sql
-- Ao abrir fase
INSERT INTO SI_SINISTRO_FASE (
    ..., CREATED_BY, CREATED_AT, ...
) VALUES (
    ..., @current_user_id, GETDATE(), ...
)

-- Ao fechar fase
UPDATE SI_SINISTRO_FASE
SET DATA_FECHA_SIFA = @business_date,
    UPDATED_BY = @current_user_id,
    UPDATED_AT = GETDATE()
WHERE ...
```

**Validação:**
- CREATED_BY: preenchido na inserção, imutável
- UPDATED_BY: NULL na inserção, preenchido na atualização
- Ambos VARCHAR(50)

**Dependências:** BR-068, BR-042

**Impacto:** CRÍTICO - Rastreabilidade de mudanças de fase

---

### BR-071: Timestamps de Auditoria

**Tier:** System-Critical
**Categoria:** Auditoria
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1042

**Descrição:**
Todos os registros DEVEM ter timestamps de criação (CREATED_AT) e modificação (UPDATED_AT).

**Lógica:**
```csharp
// Base class para todas as entidades
public abstract class AuditableEntity
{
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Ao salvar (DbContext interceptor)
public override int SaveChanges()
{
    var entries = ChangeTracker.Entries<AuditableEntity>();

    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedBy = _currentUserId;
            entry.Entity.CreatedAt = DateTime.UtcNow;
        }
        else if (entry.State == EntityState.Modified)
        {
            entry.Entity.UpdatedBy = _currentUserId;
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    return base.SaveChanges();
}
```

**Validação:**
- Usar UTC para evitar problemas de timezone
- CREATED_AT: NOT NULL
- UPDATED_AT: NULL em inserção, NOT NULL após primeira atualização

**Dependências:** BR-042, BR-068

**Impacto:** CRÍTICO - Base de auditoria temporal

---

### BR-072: Reconstrução de Trilha de Auditoria

**Tier:** System-Critical
**Categoria:** Auditoria Completa
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1044

**Descrição:**
Deve ser possível reconstruir TODA a linha do tempo de um sinistro usando a tabela SI_ACOMPANHA_SINI.

**Query de Reconstrução:**
```sql
SELECT
    a.DATA_EVENTO,
    a.HORA_EVENTO,
    a.COD_EVENTO,
    a.NUM_OCORR_SINIACO,
    a.OBS_SINIACO,
    a.COD_USUARIO,
    h.VALTOTBT,
    h.VALPRI
FROM SI_ACOMPANHA_SINI a
LEFT JOIN THISTSIN h ON (
    h.FONTE = a.FONTE AND
    h.PROTSINI = a.PROTSINI AND
    h.DAC = a.DAC AND
    h.OCORHIST = a.NUM_OCORR_SINIACO AND
    h.OPERACAO = a.COD_EVENTO
)
WHERE a.FONTE = @fonte
  AND a.PROTSINI = @protsini
  AND a.DAC = @dac
ORDER BY a.DATA_EVENTO DESC, a.HORA_EVENTO DESC, a.NUM_OCORR_SINIACO DESC
```

**Retorno:**
```
27/10/2025 14:30:15 | 1098 | Ocorr 5 | Autorização R$ 15.000,00 | USER001
26/10/2025 09:15:00 | 1098 | Ocorr 4 | Autorização R$ 10.000,00 | USER003
25/10/2025 16:45:30 | 1098 | Ocorr 3 | Autorização R$ 5.000,00  | USER001
...
```

**Validação:**
- Cada evento 1098 tem registro correspondente em THISTSIN
- Timestamps permitem ordenação cronológica precisa
- OBS_SINIACO contém descrição legível

**Dependências:** BR-057, BR-068, BR-069

**Impacto:** CRÍTICO - Requisito de compliance e auditoria externa

---

### BR-073: Imutabilidade de Data de Transação

**Tier:** System-Critical
**Categoria:** Integridade Temporal
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1046

**Descrição:**
O campo DTMOVTO (data da transação) NUNCA deve ser alterado após a criação do registro.

**Validação:**
```csharp
// Entity Framework configuration
modelBuilder.Entity<ClaimHistory>(entity =>
{
    entity.Property(e => e.TransactionDate)
        .HasColumnName("DTMOVTO")
        .ValueGeneratedOnAdd()  // Apenas na inserção
        .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw); // Erro se tentar atualizar
});
```

**Ou via trigger SQL:**
```sql
CREATE TRIGGER TR_THISTSIN_NO_UPDATE_DTMOVTO
ON THISTSIN
FOR UPDATE
AS
BEGIN
    IF UPDATE(DTMOVTO)
    BEGIN
        RAISERROR('Campo DTMOVTO não pode ser alterado', 16, 1)
        ROLLBACK TRANSACTION
    END
END
```

**Validação:**
- DTMOVTO é imutável
- Representa data HISTÓRICA da autorização
- Usado em relatórios contábeis e fiscais

**Dependências:** BR-035

**Impacto:** CRÍTICO - Alteração corrompe histórico contábil

---

### BR-074: Integridade Referencial

**Tier:** System-Critical
**Categoria:** Consistência de Dados
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1048

**Descrição:**
TODAS as tabelas envolvidas na autorização de pagamento DEVEM manter integridade referencial via foreign keys.

**Relacionamentos:**
```sql
-- THISTSIN → TMESTSIN
ALTER TABLE THISTSIN
ADD CONSTRAINT FK_THISTSIN_TMESTSIN
FOREIGN KEY (TIPSEG, ORGSIN, RMOSIN, NUMSIN)
REFERENCES TMESTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN)
ON DELETE NO ACTION;  -- Não permitir deleção se houver histórico

-- SI_ACOMPANHA_SINI → TMESTSIN
ALTER TABLE SI_ACOMPANHA_SINI
ADD CONSTRAINT FK_ACOMPANHA_TMESTSIN
FOREIGN KEY (FONTE, PROTSINI, DAC)
REFERENCES TMESTSIN (FONTE, PROTSINI, DAC)
ON DELETE NO ACTION;

-- SI_SINISTRO_FASE → TMESTSIN
ALTER TABLE SI_SINISTRO_FASE
ADD CONSTRAINT FK_FASE_TMESTSIN
FOREIGN KEY (FONTE, PROTSINI, DAC)
REFERENCES TMESTSIN (FONTE, PROTSINI, DAC)
ON DELETE NO ACTION;

-- SI_ACOMPANHA_SINI → SI_REL_FASE_EVENTO
ALTER TABLE SI_ACOMPANHA_SINI
ADD CONSTRAINT FK_ACOMPANHA_EVENTO
FOREIGN KEY (COD_EVENTO, DATA_INIVIG_REFAEV)
REFERENCES SI_REL_FASE_EVENTO (COD_EVENTO, DATA_INIVIG_REFAEV)
ON DELETE NO ACTION;
```

**Validação:**
- Todas as FKs devem existir
- ON DELETE NO ACTION (histórico é imutável)
- Cascata apenas para leitura de joins

**Dependências:** Todas as regras de transação

**Impacto:** CRÍTICO - Sem FKs, dados podem ficar órfãos

---

## Regras de Validação de Dados

### BR-075: Tipo de Seguro Consistente

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1054

**Descrição:**
O campo TIPSEG (Tipo de Seguro) deve ser numérico e consistente em todos os registros de um mesmo sinistro.

**Validação:**
```sql
-- Verificar consistência
SELECT TIPSEG, COUNT(DISTINCT TIPSEG) AS tipos_diferentes
FROM TMESTSIN
WHERE ORGSIN = @orgsin AND RMOSIN = @rmosin AND NUMSIN = @numsin
GROUP BY TIPSEG
HAVING COUNT(DISTINCT TIPSEG) > 1
```
*(Deve retornar 0 registros)*

**Tipo:**
```
SQL: SMALLINT ou INT
C#: int
Valores típicos: 0-9 (validar com área de negócio)
```

**Dependências:** BR-019 (TIPSEG usado para validar beneficiário)

**Impacto:** Médio - Dados inconsistentes afetam lógica de negócio

---

### BR-076: Código de Origem Válido

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1056

**Descrição:**
O campo ORGSIN (Origem do Sinistro) deve ser código de 2 dígitos entre 01 e 99.

**Validação:**
```csharp
[Range(1, 99, ErrorMessage = "Origem deve estar entre 01 e 99")]
public int OrigemSinistro { get; set; }
```

**Formato:**
```
Armazenamento: INT (1-99)
Exibição: "01", "02", "15", "99" (2 dígitos com zero à esquerda)
```

**Dependências:** BR-003, BR-005

**Impacto:** Médio - Parte da chave primária de TMESTSIN

---

### BR-077: Código de Ramo Válido

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1058

**Descrição:**
O campo RMOSIN (Ramo do Sinistro) deve ser código de 2 dígitos entre 00 e 99.

**Validação:**
```csharp
[Range(0, 99, ErrorMessage = "Ramo deve estar entre 00 e 99")]
public int RamoSinistro { get; set; }
```

**Formato:**
```
Armazenamento: INT (0-99)
Exibição: "00", "01", "20", "99" (2 dígitos com zero à esquerda)
```

**Dependências:** BR-003, BR-005, BR-007 (FK para TGERAMO)

**Impacto:** Alto - Parte da PK e FK para tabela de ramos

---

### BR-078: Número de Sinistro Válido

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1060

**Descrição:**
O campo NUMSIN (Número do Sinistro) deve ser numérico entre 1 e 999999 (6 dígitos).

**Validação:**
```csharp
[Range(1, 999999, ErrorMessage = "Número do sinistro deve estar entre 000001 e 999999")]
public int NumeroSinistro { get; set; }
```

**Formato:**
```
Armazenamento: INT (1-999999)
Exibição: "000001", "000123", "789012", "999999" (6 dígitos)
```

**Dependências:** BR-003, BR-005

**Impacto:** Alto - Parte da PK de TMESTSIN

---

### BR-079: Código de Fonte Válido

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1062

**Descrição:**
O campo FONTE (Fonte do Protocolo) deve ser numérico, tipicamente entre 1 e 9.

**Validação:**
```csharp
[Range(1, 999, ErrorMessage = "Fonte deve ser numérica")]
public int Fonte { get; set; }
```

**Formato:**
```
Armazenamento: INT
Exibição: "001", "002", "009" (3 dígitos com zeros à esquerda)
```

**Dependências:** BR-003, BR-004 (parte do protocolo)

**Impacto:** Médio - Usado na busca por protocolo

---

### BR-080: Número de Protocolo Válido

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1064

**Descrição:**
O campo PROTSINI (Número do Protocolo) deve ser numérico, tipicamente entre 1 e 9999999 (7 dígitos).

**Validação:**
```csharp
[Range(1, 9999999, ErrorMessage = "Protocolo deve estar entre 0000001 e 9999999")]
public int NumeroProtocolo { get; set; }
```

**Formato:**
```
Armazenamento: INT (1-9999999)
Exibição: "0000001", "0123456", "9999999" (7 dígitos)
```

**Dependências:** BR-003, BR-004

**Impacto:** Médio - Parte do protocolo único

---

### BR-081: Dígito de Controle Válido

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1066

**Descrição:**
O campo DAC (Dígito de Controle) deve ser numérico de 0 a 9 (single digit).

**Validação:**
```csharp
[Range(0, 9, ErrorMessage = "DAC deve ser um dígito de 0 a 9")]
public int DigitoControle { get; set; }
```

**Formato:**
```
Armazenamento: INT (0-9)
Exibição: "0", "1", "7", "9" (1 dígito)
```

**Dependências:** BR-003, BR-004

**Impacto:** Médio - Validação de integridade do protocolo

---

### BR-082: Código de Produto Válido

**Tier:** Business-Critical
**Categoria:** Validação de Domínio
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1068

**Descrição:**
O campo CODPRODU (Código do Produto) deve ser numérico e existir em tabela de produtos válidos.

**Validação:**
```csharp
// Validação de existência
var validProducts = new[] { 6814, 7701, 7709, 8001, 8002, ... };
if (!validProducts.Contains(productCode))
{
    return Error("Código de produto inválido");
}
```

**Produtos Especiais:**
```
6814 = Consórcio Habitacional → CNOUA
7701 = Consórcio Automóvel → CNOUA
7709 = Consórcio Serviços → CNOUA
```

**Dependências:** BR-043 (roteamento por produto)

**Impacto:** Alto - Produto incorreto leva a validação errada

---

### BR-083: Saldo a Pagar Não-Negativo

**Tier:** Business-Critical
**Categoria:** Validação Financeira
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1070

**Descrição:**
O campo SDOPAG (Saldo a Pagar / Reserva) deve ser sempre >= 0.

**Validação:**
```csharp
[Range(0, double.MaxValue, ErrorMessage = "Saldo a pagar não pode ser negativo")]
public decimal SaldoAPagar { get; set; }
```

**Tipo:**
```
SQL: DECIMAL(15,2)
C#: decimal
Valor mínimo: 0.00
```

**Dependências:** BR-013 (usado no cálculo de saldo pendente)

**Impacto:** Alto - Saldo negativo é inconsistência grave

---

### BR-084: Total Pago Consistente

**Tier:** System-Critical
**Categoria:** Validação Financeira
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1072

**Descrição:**
O campo TOTPAG (Total Pago) deve ser >= 0 e <= SDOPAG.

**Validação:**
```csharp
public class ClaimMaster
{
    public decimal SaldoAPagar { get; set; }

    private decimal _totalPago;
    public decimal TotalPago
    {
        get => _totalPago;
        set
        {
            if (value < 0)
                throw new ValidationException("Total pago não pode ser negativo");
            if (value > SaldoAPagar)
                throw new ValidationException("Total pago não pode exceder saldo a pagar");
            _totalPago = value;
        }
    }
}
```

**Validação de Consistência:**
```sql
-- Query de verificação
SELECT *
FROM TMESTSIN
WHERE TOTPAG < 0 OR TOTPAG > SDOPAG
```
*(Deve retornar 0 registros)*

**Dependências:** BR-013, BR-083

**Impacto:** CRÍTICO - Inconsistência financeira fundamental

---

### BR-085: Contador de Ocorrências Não-Negativo

**Tier:** Business-Critical
**Categoria:** Validação de Sequência
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1074

**Descrição:**
O campo OCORHIST (Contador de Ocorrências) deve ser inteiro não-negativo, incrementado monotonicamente.

**Validação:**
```csharp
[Range(0, int.MaxValue, ErrorMessage = "Contador de ocorrências deve ser >= 0")]
public int OccurrenceCounter { get; set; }
```

**Regra de Incremento:**
```
Primeiro pagamento: OCORHIST = 1
Segundo pagamento: OCORHIST = 2
...
N-ésimo pagamento: OCORHIST = N
```

**Dependências:** BR-040 (lógica de incremento)

**Impacto:** Alto - Parte da PK de THISTSIN

---

### BR-086: Valor Principal Dentro do Saldo

**Tier:** System-Critical
**Categoria:** Validação Financeira
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1076

**Descrição:**
O campo VALPRI (Valor Principal) deve ser >= 0 e <= (SDOPAG - TOTPAG).

**Validação:**
```csharp
var saldoPendente = claim.SaldoAPagar - claim.TotalPago;

if (valorPrincipal < 0)
    return Error("Valor principal não pode ser negativo");

if (valorPrincipal > saldoPendente)
    return Error($"Valor principal ({valorPrincipal:C}) excede saldo pendente ({saldoPendente:C})");
```

**Dependências:** BR-012, BR-013

**Impacto:** CRÍTICO - Regra fundamental de autorização

---

### BR-087: Valor de Correção Não-Negativo

**Tier:** Business-Critical
**Categoria:** Validação Financeira
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1078

**Descrição:**
O campo CRRMON (Valor de Correção) deve ser >= 0 (pode ser zero se não houver correção).

**Validação:**
```csharp
[Range(0, double.MaxValue, ErrorMessage = "Valor de correção não pode ser negativo")]
public decimal ValorCorrecao { get; set; } = 0.00m;  // Default 0
```

**Dependências:** BR-017, BR-018, BR-030

**Impacto:** Médio - Afeta valor total da transação

---

## Regras de Interface e Display

### BR-088: Idioma Português

**Tier:** Operational
**Categoria:** Localização
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1084

**Descrição:**
TODA a interface do usuário DEVE ser em português brasileiro, incluindo labels, mensagens, botões e documentação.

**Exemplos:**
```
✅ "Buscar Sinistro"        ❌ "Search Claim"
✅ "Valor Principal"        ❌ "Principal Amount"
✅ "Autorizar Pagamento"    ❌ "Authorize Payment"
✅ "Favorecido"             ❌ "Beneficiary"
```

**Validação:**
- Sem termos em inglês
- Usar terminologia do domínio de seguros brasileiro
- Seguir glossário estabelecido

**Dependências:** Nenhuma

**Impacto:** Médio - Usabilidade para operadores brasileiros

---

### BR-089: Formatação de Valores Monetários

**Tier:** Operational
**Categoria:** Display
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1086

**Descrição:**
Valores monetários DEVEM ser formatados com separador de milhares (ponto) e separador decimal (vírgula).

**Formato:**
```
Padrão: ###.###.###,##

Exemplos:
1234.56       → "1.234,56"
50000.00      → "50.000,00"
999999999.99  → "999.999.999,99"
0.50          → "0,50"
```

**Implementação:**
```csharp
decimal valor = 50000.00m;
string formatado = valor.ToString("N2", new CultureInfo("pt-BR"));
// Resultado: "50.000,00"

// Com prefixo R$
string comMoeda = valor.ToString("C2", new CultureInfo("pt-BR"));
// Resultado: "R$ 50.000,00"
```

**Dependências:** BR-009

**Impacto:** Baixo - Problema cosmético

---

### BR-090: Formato de Data

**Tier:** Operational
**Categoria:** Display
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1088

**Descrição:**
Datas DEVEM ser exibidas no formato DD/MM/YYYY e armazenadas no formato YYYY-MM-DD.

**Formato de Exibição:**
```
27/10/2025 (dia/mês/ano)
```

**Formato de Armazenamento:**
```sql
2025-10-27 (ISO 8601)
```

**Implementação:**
```csharp
// Exibição
DateTime data = new DateTime(2025, 10, 27);
string exibicao = data.ToString("dd/MM/yyyy");  // "27/10/2025"

// Armazenamento (EF Core faz automaticamente para tipo DATE)
entity.TransactionDate = data;  // Armazenado como 2025-10-27
```

**Dependências:** Nenhuma

**Impacto:** Baixo - Padrão brasileiro

---

### BR-091: Formato de Hora

**Tier:** Operational
**Categoria:** Display
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1090

**Descrição:**
Horas DEVEM ser exibidas no formato HH:MM:SS (24 horas).

**Formato:**
```
14:30:15 (não 2:30:15 PM)
09:05:03 (não 9:5:3)
```

**Implementação:**
```csharp
TimeSpan hora = new TimeSpan(14, 30, 15);
string exibicao = hora.ToString(@"hh\:mm\:ss");  // "14:30:15"
```

**Dependências:** BR-036

**Impacto:** Baixo - Padrão brasileiro

---

### BR-092: Cor de Mensagens de Erro

**Tier:** Operational
**Categoria:** UI/UX
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1092

**Descrição:**
Mensagens de erro DEVEM ser exibidas na cor vermelha (#e80c4d).

**CSS:**
```css
.error-message {
    color: #e80c4d;
    font-weight: bold;
    margin-top: 8px;
}
```

**Exemplo:**
```html
<div class="error-message">
    Valor Superior ao Saldo Pendente
</div>
```

**Dependências:** Todas as regras de mensagem de erro

**Impacto:** Baixo - Problema de usabilidade

---

### BR-093: Logo Caixa Seguradora

**Tier:** Operational
**Categoria:** Branding
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1093

**Descrição:**
O logo da Caixa Seguradora DEVE ser exibido no cabeçalho de todas as telas.

**Especificações:**
```
Fonte: Base64 PNG embed em spec.md
Posição: Cabeçalho superior esquerdo
Tamanho: Altura máxima 60px
Alt text: "Caixa Seguradora"
```

**Implementação:**
```jsx
<img
    src="data:image/png;base64,{base64String}"
    alt="Caixa Seguradora"
    style={{ maxHeight: '60px' }}
/>
```

**Dependências:** Nenhuma

**Impacto:** Baixo - Branding corporativo

---

### BR-094: Stylesheet Site.css

**Tier:** System-Critical
**Categoria:** UI Constraints
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1094

**Descrição:**
O sistema DEVE aplicar o stylesheet Site.css legado SEM MODIFICAÇÕES.

**Validação:**
```
- NÃO alterar classes CSS existentes
- NÃO remover regras CSS
- NÃO modificar cores ou dimensões
- PODE adicionar novas classes se necessário (não conflitar)
```

**Arquivo:**
```
frontend/public/Site.css (deve ser preservado exatamente)
```

**Dependências:** Nenhuma

**Impacto:** CRÍTICO - Requisito de consistência visual

---

### BR-095: Design Responsivo

**Tier:** Operational
**Categoria:** UI/UX
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1095

**Descrição:**
A interface DEVE suportar desktop e mobile com largura máxima de 850px.

**Media Queries:**
```css
/* Desktop */
@media (min-width: 851px) {
    .container {
        max-width: 850px;
        margin: 0 auto;
    }
}

/* Mobile */
@media (max-width: 850px) {
    .container {
        width: 100%;
        padding: 0 16px;
    }
}
```

**Validação:**
- Testar em 850px, 768px, 375px
- Elementos não devem quebrar layout
- Scroll horizontal não permitido

**Dependências:** BR-094

**Impacto:** Médio - Usabilidade em dispositivos móveis

---

## Regras de Performance

### BR-096: Tempo de Busca de Sinistro

**Tier:** Operational
**Categoria:** Performance
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1104

**Descrição:**
A busca de sinistro DEVE completar em menos de 3 segundos.

**Medição:**
```csharp
var stopwatch = Stopwatch.StartNew();
var result = await SearchClaimAsync(criteria);
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 3000)
{
    _logger.LogWarning("Search exceeded 3s: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
}
```

**Otimizações:**
- Índices em FONTE+PROTSINI+DAC
- Índice em ORGSIN+RMOSIN+NUMSIN
- Índice em CODLIDER+SINLID
- Connection pooling
- Query optimization

**Dependências:** BR-003

**Impacto:** Médio - Experiência do usuário

---

### BR-097: Tempo de Autorização de Pagamento

**Tier:** Operational
**Categoria:** Performance
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1106

**Descrição:**
O ciclo completo de autorização de pagamento (Steps 1-8) DEVE completar em menos de 90 segundos.

**Breakdown:**
```
Step 1: Buscar sinistro           < 3s
Step 2: Validar entrada           < 1s
Step 3: Validação externa         < 10s (timeout)
Step 4: Insert THISTSIN           < 1s
Step 5: Update TMESTSIN           < 1s
Step 6: Insert SI_ACOMPANHA_SINI  < 1s
Step 7: Update fases              < 2s
Step 8: Commit                    < 1s
TOTAL:                            < 20s (margem de 70s para retries)
```

**Timeout de Transação:**
```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer(connectionString, opts =>
        opts.CommandTimeout(90))  // 90 segundos
    .Options;
```

**Dependências:** Todas as regras de transação

**Impacto:** Alto - Usuário pode desistir se demorar muito

---

### BR-098: Tempo de Query de Histórico

**Tier:** Operational
**Categoria:** Performance
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1108

**Descrição:**
Query de histórico DEVE retornar em menos de 500ms para sinistros com 1000+ registros.

**Otimização:**
```sql
-- Índice composto
CREATE INDEX IX_THISTSIN_CLAIM_LOOKUP
ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)
```

**Query Eficiente:**
```sql
SELECT TOP 20 *
FROM THISTSIN
WHERE TIPSEG = @tipseg
  AND ORGSIN = @orgsin
  AND RMOSIN = @rmosin
  AND NUMSIN = @numsin
ORDER BY OCORHIST DESC
```

**Dependências:** BR-100 (ordenação), BR-099 (paginação)

**Impacto:** Médio - Histórico grande pode travar UI

---

### BR-099: Paginação de Histórico

**Tier:** Operational
**Categoria:** Performance
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1110

**Descrição:**
Histórico DEVE ser paginado com 20 registros por página (padrão), máximo 100 por página.

**Implementação:**
```csharp
public async Task<PagedResult<ClaimHistory>> GetHistoryAsync(
    int claimId, int page = 1, int pageSize = 20)
{
    if (pageSize > 100)
        pageSize = 100;  // Limite máximo

    var query = _db.ClaimHistories
        .Where(h => h.ClaimId == claimId)
        .OrderByDescending(h => h.OccurrenceCounter);

    var total = await query.CountAsync();
    var items = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResult<ClaimHistory>
    {
        Items = items,
        TotalCount = total,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(total / (double)pageSize)
    };
}
```

**Validação:**
- pageSize mínimo: 1
- pageSize máximo: 100
- pageSize padrão: 20

**Dependências:** BR-098

**Impacto:** Médio - Previne carregamento excessivo

---

### BR-100: Ordenação de Histórico

**Tier:** Operational
**Categoria:** UX
**Origem:** LEGACY_SIWEA_COMPLETE_ANALYSIS.md linha 1112

**Descrição:**
Histórico DEVE ser ordenado por OCORHIST DESC (mais recente primeiro).

**Query:**
```sql
SELECT *
FROM THISTSIN
WHERE TIPSEG = @tipseg
  AND ORGSIN = @orgsin
  AND RMOSIN = @rmosin
  AND NUMSIN = @numsin
ORDER BY OCORHIST DESC  -- Mais recente primeiro
```

**Exibição:**
```
Pagamento #5 - R$ 15.000,00 - 27/10/2025 14:30 - USER001
Pagamento #4 - R$ 10.000,00 - 26/10/2025 09:15 - USER003
Pagamento #3 - R$  5.000,00 - 25/10/2025 16:45 - USER001
...
```

**Dependências:** BR-040 (OCORHIST)

**Impacto:** Baixo - UX: usuário quer ver mais recente primeiro

---

## Matriz de Rastreabilidade

### Tabela de Dependências Críticas

| Regra | Depende de | Impacto se Falhar |
|-------|-----------|-------------------|
| BR-013 | BR-003, BR-012 | Pagamento acima do devido |
| BR-023 | BR-024, BR-025 | Valor de pagamento incorreto |
| BR-034 | Nenhuma | Queries e relatórios falham |
| BR-035 | Nenhuma | Data contábil incorreta |
| BR-047 | BR-043, BR-045, BR-046 | Validação externa ignorada |
| BR-051 | BR-048, BR-050 | Transação parcial (corrupção) |
| BR-066 | BR-059, BR-060, BR-064 | Fases inconsistentes |
| BR-067 | BR-051, BR-066 | Dados corrompidos |

### Regras por User Story

**US1 - Busca de Sinistros:**
BR-001, BR-002, BR-003, BR-004, BR-005, BR-006, BR-007, BR-008, BR-009

**US2 - Autorização de Pagamento:**
BR-010 a BR-042 (todas as regras de autorização, conversão e transação)

**US3 - Histórico:**
BR-098, BR-099, BR-100

**US4 - Validação Consórcio:**
BR-043, BR-047, BR-048, BR-049, BR-052 a BR-056

**US5 - Gestão de Fases:**
BR-057 a BR-067

**US6 - Dashboard:**
BR-096, BR-097, BR-098 (métricas de performance)

### Regras de Compliance (Alta Prioridade)

| ID | Regra | Área de Compliance |
|----|-------|-------------------|
| BR-013 | Validação de saldo | Financeiro |
| BR-019 | Beneficiário obrigatório | Legal |
| BR-023 | Conversão monetária | Contábil |
| BR-034 | Código operação fixo | Auditoria |
| BR-035 | Data de negócio | Contábil |
| BR-041 | Registro de operador | Auditoria |
| BR-047 | Validação externa | Compliance |
| BR-067 | Atomicidade | Integridade |
| BR-068-074 | Trilha de auditoria | SOX/Auditoria |
| BR-073 | Imutabilidade temporal | Contábil |

---

## Status do Documento

**Progresso:** 100 de 100 regras documentadas (100% ✅)

**Documentação Completa:**
- ✅ Regras de Busca e Recuperação (BR-001 a BR-009)
- ✅ Regras de Autorização de Pagamento (BR-010 a BR-022)
- ✅ Regras de Conversão Monetária (BR-023 a BR-033)
- ✅ Regras de Registro de Transações (BR-034 a BR-042)
- ✅ Regras de Validação de Produtos (BR-043 a BR-056)
- ✅ Regras de Gestão de Fases (BR-057 a BR-067)
- ✅ Regras de Auditoria (BR-068 a BR-074)
- ✅ Regras de Validação de Dados (BR-075 a BR-087)
- ✅ Regras de Interface e Display (BR-088 a BR-095)
- ✅ Regras de Performance (BR-096 a BR-100)

**Próximos Documentos:**
- SISTEMA_LEGADO_ARQUITETURA.md
- SISTEMA_LEGADO_MODELO_DADOS.md
- SISTEMA_LEGADO_PROCESSOS.md

---

**FIM - DOCUMENTO 2/5 - 100% COMPLETO**
