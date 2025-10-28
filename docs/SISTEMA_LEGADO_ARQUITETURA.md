# SIWEA - Arquitetura Técnica do Sistema Legado

**Documento:** Arquitetura Técnica e Integra\u00e7\u00f5es
**Vers\u00e3o:** 1.0
**Data:** 2025-10-27
**Documento:** 3 de 5
**Refer\u00eancia:** SISTEMA_LEGADO_VISAO_GERAL.md, SISTEMA_LEGADO_REGRAS_NEGOCIO.md

---

## \u00cdndice

1. [Introdu\u00e7\u00e3o](#introdu\u00e7\u00e3o)
2. [Arquitetura em 3 Camadas](#arquitetura-em-3-camadas)
3. [Camada de Apresenta\u00e7\u00e3o](#camada-de-apresenta\u00e7\u00e3o)
4. [Camada de Neg\u00f3cio](#camada-de-neg\u00f3cio)
5. [Camada de Dados](#camada-de-dados)
6. [Integra\u00e7\u00f5es Externas](#integra\u00e7\u00f5es-externas)
7. [Gest\u00e3o de Transa\u00e7\u00f5es](#gest\u00e3o-de-transa\u00e7\u00f5es)
8. [Seguran\u00e7a e Auditoria](#seguran\u00e7a-e-auditoria)
9. [Infraestrutura de Execu\u00e7\u00e3o](#infraestrutura-de-execu\u00e7\u00e3o)
10. [Desempenho e Escalabilidade](#desempenho-e-escalabilidade)

---

## Introdu\u00e7\u00e3o

### Prop\u00f3sito deste Documento

Este documento fornece uma vis\u00e3o completa da arquitetura t\u00e9cnica do sistema legado SIWEA, incluindo:
- Arquitetura de software em 3 camadas
- Tecnologias utilizadas (mainframe, COBOL, DB2, CICS)
- Integra\u00e7\u00f5es com sistemas externos (CNOUA, SIPUA, SIMDA)
- Estrat\u00e9gias de transa\u00e7\u00e3o e consist\u00eancia de dados
- Mecanismos de seguran\u00e7a e auditoria

### Escopo

**O que est\u00e1 inclu\u00eddo:**
- Descri\u00e7\u00e3o detalhada de cada camada arquitetural
- Protocolos de comunica\u00e7\u00e3o e integra\u00e7\u00e3o
- Estrat\u00e9gias de ger\u00eancia de transa\u00e7\u00f5es
- Requisitos n\u00e3o-funcionais (performance, seguran\u00e7a, disponibilidade)

**O que N\u00c3O est\u00e1 inclu\u00eddo:**
- Regras de neg\u00f3cio detalhadas (ver SISTEMA_LEGADO_REGRAS_NEGOCIO.md)
- Modelo de dados completo (ver SISTEMA_LEGADO_MODELO_DADOS.md)
- Processos e workflows (ver SISTEMA_LEGADO_PROCESSOS.md)

---

## Arquitetura em 3 Camadas

### Vis\u00e3o Geral

O SIWEA segue uma arquitetura cl\u00e1ssica de 3 camadas (3-Tier Architecture) t\u00edpica de sistemas mainframe IBM:

```
\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
\u2502  CAMADA DE APRESENTA\u00c7\u00c3O          \u2502
\u2502  (Terminal 3270 / Emulador)       \u2502
\u2502  - SI11M010 (Busca)               \u2502
\u2502  - SIHM020 (Autoriza\u00e7\u00e3o)          \u2502
\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
             \u2502
             \u2502 TN3270 Protocol
             \u2502
\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
\u2502  CAMADA DE NEG\u00d3CIO (CICS)          \u2502
\u2502  IBM VisualAge EZEE 4.40          \u2502
\u2502  - SIWEA-V116 (Programa Principal)\u2502
\u2502  - L\u00f3gica de valida\u00e7\u00e3o            \u2502
\u2502  - Convers\u00e3o monet\u00e1ria            \u2502
\u2502  - Integra\u00e7\u00f5es externas           \u2502
\u2502  - Gest\u00e3o de fases                 \u2502
\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
             \u2502
             \u2502 SQL / DB2 API
             \u2502
\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
\u2502  CAMADA DE DADOS (DB2)             \u2502
\u2502  IBM DB2 for z/OS                  \u2502
\u2502  - 10 tabelas legadas              \u2502
\u2502  - \u00cdndices otimizados               \u2502
\u2502  - Stored procedures (m\u00ednimas)    \u2502
\u2502  - Constraints e FKs               \u2502
\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
```

### Caracter\u00edsticas Principais

**Separa\u00e7\u00e3o de Responsabilidades:**
- **Apresenta\u00e7\u00e3o**: Renderiza\u00e7\u00e3o de telas, captura de entrada
- **Neg\u00f3cio**: L\u00f3gica de valida\u00e7\u00e3o, c\u00e1lculos, integra\u00e7\u00f5es
- **Dados**: Persist\u00eancia, integridade referencial, transa\u00e7\u00f5es

**Acoplamento:**
- Camadas comunicam-se apenas com camadas adjacentes
- Apresenta\u00e7\u00e3o n\u00e3o acessa diretamente banco de dados
- L\u00f3gica de neg\u00f3cio n\u00e3o conhece detalhes de renderiza\u00e7\u00e3o

**Escalabilidade:**
- M\u00faltiplos terminais conectam-se ao mesmo CICS region
- Connection pooling de DB2 gerenciado por CICS
- Transa\u00e7\u00f5es curtas (< 90 segundos)

---

## Camada de Apresenta\u00e7\u00e3o

### Tecnologia: IBM 3270 Terminal

**Protocolo:** TN3270 (Telnet 3270)

**Caracter\u00edsticas:**
```
Resolu\u00e7\u00e3o:      80 colunas \u00d7 24 linhas (texto)
Codifica\u00e7\u00e3o:   EBCDIC (Extended Binary Coded Decimal Interchange Code)
Cores:         Monocrom\u00e1tico ou 7 cores (verde, amarelo, vermelho, azul, rosa, ciano, branco)
Atributos:     Normal, Intensified, Hidden, Protected
Navega\u00e7\u00e3o:    Tab, Enter, PF1-PF24 (teclas de fun\u00e7\u00e3o)
```

### Telas do Sistema

#### Tela SI11M010 - Busca de Sinistros

**Layout ASCII:**
```
\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
\u2502  SIWEA - SISTEMA DE LIBERA\u00c7\u00c3O DE PAGAMENTO DE SINISTROS           \u2502
\u2502  SI11M010 - BUSCA DE SINISTROS                                    \u2502
\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524
\u2502                                                                      \u2502
\u2502  Crit\u00e9rio 1: Por Protocolo                                         \u2502
\u2502  Fonte:     [___]                                                  \u2502
\u2502  Protocolo: [________]                                             \u2502
\u2502  DAC:       [_]                                                    \u2502
\u2502                                                                      \u2502
\u2502  Crit\u00e9rio 2: Por N\u00famero do Sinistro                               \u2502
\u2502  Origem:    [__]                                                   \u2502
\u2502  Ramo:      [__]                                                   \u2502
\u2502  N\u00famero:    [______]                                               \u2502
\u2502                                                                      \u2502
\u2502  Crit\u00e9rio 3: Por C\u00f3digo L\u00edder                                     \u2502
\u2502  L\u00edder:     [___]                                                  \u2502
\u2502  Sinistro:  [_______]                                              \u2502
\u2502                                                                      \u2502
\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524
\u2502  PF3=Sair   PF12=Cancelar   ENTER=Buscar                           \u2502
\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
```

**Campos de Entrada:**
- Protocolo: FONTE (3 d\u00edgitos) + PROTSINI (7 d\u00edgitos) + DAC (1 d\u00edgito)
- Sinistro: ORGSIN (2 d\u00edgitos) + RMOSIN (2 d\u00edgitos) + NUMSIN (6 d\u00edgitos)
- L\u00edder: CODLIDER (3 d\u00edgitos) + SINLID (7 d\u00edgitos)

**Valida\u00e7\u00f5es:**
- Ao menos um crit\u00e9rio completo deve ser preenchido
- Campos num\u00e9ricos apenas
- Formato com zeros \u00e0 esquerda obrigat\u00f3rio

#### Tela SIHM020 - Autoriza\u00e7\u00e3o de Pagamento

**Layout ASCII:**
```
\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
\u2502  SIHM020 - AUTORIZA\u00c7\u00c3O DE PAGAMENTO                                 \u2502
\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524
\u2502  Protocolo:       001/0123456-7                                    \u2502
\u2502  Sinistro:        10/20/789012                                     \u2502
\u2502  Ramo:            AUTOM\u00d3VEIS                                        \u2502
\u2502  Segurado:        JO\u00c3O DA SILVA                                    \u2502
\u2502                                                                      \u2502
\u2502  Saldo a Pagar:   R$ 50.000,00                                     \u2502
\u2502  Total Pago:      R$ 10.000,00                                     \u2502
\u2502  Saldo Pendente:  R$ 40.000,00                                     \u2502
\u2502                                                                      \u2502
\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524
\u2502  DADOS DA AUTORIZA\u00c7\u00c3O:                                            \u2502
\u2502                                                                      \u2502
\u2502  Tipo de Pagamento: [_] (1-5)                                      \u2502
\u2502  Tipo de Ap\u00f3lice:   [_] (1-2)                                      \u2502
\u2502  Valor Principal:   [____________]                                 \u2502
\u2502  Valor Corre\u00e7\u00e3o:     [____________] (opcional)                    \u2502
\u2502  Favorecido:        [_______________________________________]       \u2502
\u2502                                                                      \u2502
\u251c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2524
\u2502  PF3=Voltar   PF12=Cancelar   ENTER=Autorizar                      \u2502
\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
```

**Campos Protegidos (Readonly):**
- Protocolo, Sinistro, Ramo, Segurado
- Saldo a Pagar, Total Pago, Saldo Pendente

**Campos Edit\u00e1veis:**
- Tipo de Pagamento (1-5)
- Tipo de Ap\u00f3lice (1-2)
- Valor Principal (decimal 15,2)
- Valor Corre\u00e7\u00e3o (decimal 15,2, opcional)
- Favorecido (varchar 255, condicional)

### Comunica\u00e7\u00e3o com CICS

**Protocolo TN3270:**
```
Cliente (Terminal Emulador)
    |
    | 1. Envia dados de tela (structured field)
    v
Servidor CICS
    |
    | 2. CICS invoca programa SIWEA-V116
    v
Programa SIWEA
    |
    | 3. Processa l\u00f3gica de neg\u00f3cio
    | 4. Consulta DB2
    | 5. Integra com servi\u00e7os externos
    v
CICS
    |
    | 6. Retorna nova tela ao cliente
    v
Terminal 3270
```

**Comandos CICS Utilizados:**
```cobol
EXEC CICS RECEIVE MAP(...) END-EXEC        -- Recebe entrada
EXEC CICS SEND MAP(...) END-EXEC           -- Envia resposta
EXEC CICS RETURN TRANSID(...) END-EXEC     -- Retorna com persist\u00eancia
EXEC CICS HANDLE CONDITION END-EXEC        -- Tratamento de erros
EXEC CICS LINK PROGRAM(...) END-EXEC       -- Chama outro programa
```

---

## Camada de Neg\u00f3cio

### Tecnologia: IBM VisualAge EZEE 4.40

**Linguagem:** COBOL estendido com constru\u00e7\u00f5es VisualAge

**Programa Principal:** SIWEA-V116

**Estrutura do Programa:**

```cobol
IDENTIFICATION DIVISION.
PROGRAM-ID. SIWEA-V116.

ENVIRONMENT DIVISION.

DATA DIVISION.
WORKING-STORAGE SECTION.
    -- Vari\u00e1veis de trabalho
    -- Estruturas de dados
    -- Constants

LINKAGE SECTION.
    -- Comunica\u00e7\u00e3o com CICS
    -- \u00c1reas de entrada/sa\u00edda

PROCEDURE DIVISION.
    MAIN-PROCESS.
        -- L\u00f3gica principal
        -- Valida\u00e7\u00f5es
        -- C\u00e1lculos
        -- Chamadas de integra\u00e7\u00e3o
        -- Atualiza\u00e7\u00f5es de banco
```

### Principais M\u00f3dulos de Funcionalidade

#### M\u00f3dulo 1: Busca de Sinistros

**Fun\u00e7\u00e3o:** `SEARCH-CLAIM`

**Entradas:**
- Crit\u00e9rio de busca (protocolo, sinistro ou l\u00edder)
- Valores dos campos

**Processamento:**
```
1. Validar crit\u00e9rio completo
2. Determinar tipo de query
3. Executar SQL SELECT em TMESTSIN
4. LEFT JOIN TGERAMO (nome do ramo)
5. LEFT JOIN TAPOLICE (nome do segurado)
6. Calcular saldo pendente
7. Retornar dados para tela
```

**Sa\u00edda:**
- Registro do sinistro completo
- Ou mensagem de erro se n\u00e3o encontrado

#### M\u00f3dulo 2: Autoriza\u00e7\u00e3o de Pagamento

**Fun\u00e7\u00e3o:** `AUTHORIZE-PAYMENT`

**Entradas:**
- Dados do sinistro (protocolo/ID)
- Tipo de pagamento (1-5)
- Tipo de ap\u00f3lice (1-2)
- Valor principal
- Valor corre\u00e7\u00e3o (opcional)
- Favorecido (condicional)

**Processamento (8 Steps):**
```
Step 1: Recuperar dados do sinistro
Step 2: Validar entradas
    - Tipo pagamento \u2208 {1,2,3,4,5}
    - Tipo ap\u00f3lice \u2208 {1,2}
    - Valor principal > 0
    - Valor corre\u00e7\u00e3o >= 0
    - Favorecido obrigat\u00f3rio se TPSEGU != 0

Step 3: Validar contra saldo pendente
    - VALPRI <= (SDOPAG - TOTPAG)

Step 4: Validar externamente (roteamento por produto)
    - Se CODPRODU \u2208 {6814, 7701, 7709} \u2192 CNOUA
    - Sen\u00e3o consultar EF_CONTR_SEG_HABIT
        - Se NUM_CONTRATO > 0 \u2192 SIPUA
        - Sen\u00e3o \u2192 SIMDA
    - Se EZERT8 != '00000000' \u2192 ABORTAR

Step 5: Obter taxa de convers\u00e3o
    - SELECT VLCRUZAD FROM TGEUNIMO WHERE data BETWEEN...
    - Se n\u00e3o encontrar \u2192 ERRO

Step 6: Calcular valores em BTNF
    - VALPRIBT = VALPRI \u00d7 VLCRUZAD
    - CRRMONBT = CRRMON \u00d7 VLCRUZAD
    - VALTOTBT = VALPRIBT + CRRMONBT

Step 7: Iniciar transa\u00e7\u00e3o DB2
    BEGIN TRANSACTION

    7a: Inserir THISTSIN
        - OPERACAO = 1098
        - DTMOVTO = TSISTEMA.DTMOVABE
        - HORAOPER = CURRENT_TIME
        - TIPCRR = '5'
        - SITCONTB = '0'
        - SITUACAO = '0'
        - EZEUSRID = current user

    7b: Atualizar TMESTSIN
        - TOTPAG += VALTOTBT
        - OCORHIST += 1

    7c: Inserir SI_ACOMPANHA_SINI
        - COD_EVENTO = 1098
        - NUM_OCORR_SINIACO = novo OCORHIST

    7d: Atualizar SI_SINISTRO_FASE
        - Consultar SI_REL_FASE_EVENTO
        - Para cada relacionamento:
            - Se IND_ALTERACAO_FASE = '1' \u2192 ABRIR fase
            - Se IND_ALTERACAO_FASE = '2' \u2192 FECHAR fase

    COMMIT TRANSACTION

Step 8: Retornar sucesso
```

**Sa\u00edda:**
- Sucesso: Mensagem de confirma\u00e7\u00e3o
- Falha: C\u00f3digo de erro espec\u00edfico

#### M\u00f3dulo 3: Convers\u00e3o Monet\u00e1ria

**Fun\u00e7\u00e3o:** `CONVERT-TO-BTNF`

**Entradas:**
- Valor original
- Data da transa\u00e7\u00e3o

**Processamento:**
```sql
SELECT VLCRUZAD
FROM TGEUNIMO
WHERE @transaction_date BETWEEN DTINIVIG AND DTTERVIG
```

**C\u00e1lculo:**
```cobol
COMPUTE AMOUNT-BTNF = AMOUNT-ORIGINAL * VLCRUZAD
COMPUTE AMOUNT-BTNF ROUNDED = AMOUNT-BTNF
    ON SIZE ERROR
        MOVE 'ERR-OVERFLOW' TO ERROR-CODE
END-COMPUTE
```

**Precis\u00e3o:**
- Taxa: DECIMAL(18,8)
- Resultado intermedi\u00e1rio: Manter 8 decimais
- Resultado final: Arredondar para 2 decimais (Banker's Rounding)

---

## Camada de Dados

### Tecnologia: IBM DB2 for z/OS

**Vers\u00e3o:** DB2 11 for z/OS (ou superior)

**Caracter\u00edsticas:**
- Suporte a transa\u00e7\u00f5es ACID
- Isolation Level: CS (Cursor Stability) ou RR (Repeatable Read)
- Locking: Row-level locks com escalonamento autom\u00e1tico
- Recovery: Log-based com checkpoints

### Estrutura de Tabelas

**10 Tabelas Legadas:**

1. **TMESTSIN** - Claim Master (chave de 4 partes)
2. **THISTSIN** - Payment History (chave de 5 partes)
3. **TGERAMO** - Branch Master
4. **TAPOLICE** - Policy Master (chave de 3 partes)
5. **TGEUNIMO** - Currency Unit Table
6. **TSISTEMA** - System Control
7. **SI_ACOMPANHA_SINI** - Claim Accompaniment
8. **SI_SINISTRO_FASE** - Claim Phases
9. **SI_REL_FASE_EVENTO** - Phase-Event Relationships
10. **EF_CONTR_SEG_HABIT** - Consortium Contracts

### \u00cdndices Cr\u00edticos

```sql
-- Busca por protocolo (< 3s)
CREATE INDEX IX_TMESTSIN_Protocol
ON TMESTSIN (FONTE, PROTSINI, DAC);

-- Busca por sinistro (< 3s)
CREATE UNIQUE INDEX PK_TMESTSIN
ON TMESTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN);

-- Busca por l\u00edder (< 3s)
CREATE INDEX IX_TMESTSIN_Leader
ON TMESTSIN (CODLIDER, SINLID);

-- Hist\u00f3rico de pagamentos (< 500ms)
CREATE INDEX IX_THISTSIN_Claim_Occurrence
ON THISTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST DESC)
INCLUDE (OPERACAO, DTMOVTO, HORAOPER, VALPRI, CRRMON, VALTOTBT, EZEUSRID);

-- Taxa de convers\u00e3o (< 100ms)
CREATE INDEX IX_TGEUNIMO_DateRange
ON TGEUNIMO (DTINIVIG, DTTERVIG);

-- Fases abertas (< 200ms)
CREATE INDEX IX_FASE_Aberta
ON SI_SINISTRO_FASE (FONTE, PROTSINI, DAC, COD_FASE, COD_EVENTO)
WHERE DATA_FECHA_SIFA = '9999-12-31';
```

### Constraints e Foreign Keys

```sql
-- Integridade referencial
ALTER TABLE THISTSIN
ADD CONSTRAINT FK_THISTSIN_TMESTSIN
FOREIGN KEY (TIPSEG, ORGSIN, RMOSIN, NUMSIN)
REFERENCES TMESTSIN (TIPSEG, ORGSIN, RMOSIN, NUMSIN)
ON DELETE NO ACTION;

ALTER TABLE SI_ACOMPANHA_SINI
ADD CONSTRAINT FK_ACOMPANHA_TMESTSIN
FOREIGN KEY (FONTE, PROTSINI, DAC)
REFERENCES TMESTSIN (FONTE, PROTSINI, DAC)
ON DELETE NO ACTION;

-- Checks
ALTER TABLE TMESTSIN
ADD CONSTRAINT CHK_SDOPAG_NONNEG CHECK (SDOPAG >= 0);

ALTER TABLE TMESTSIN
ADD CONSTRAINT CHK_TOTPAG_NONNEG CHECK (TOTPAG >= 0);

ALTER TABLE TMESTSIN
ADD CONSTRAINT CHK_TOTPAG_LE_SDOPAG CHECK (TOTPAG <= SDOPAG);

ALTER TABLE THISTSIN
ADD CONSTRAINT CHK_OPERACAO_1098 CHECK (OPERACAO = 1098);

ALTER TABLE THISTSIN
ADD CONSTRAINT CHK_TIPCRR_5 CHECK (TIPCRR = '5');
```

### Estrat\u00e9gia de Backup

**Backup Completo:**
- Frequ\u00eancia: Di\u00e1rio (23:00 - hor\u00e1rio de menor uso)
- Reten\u00e7\u00e3o: 30 dias

**Backup Incremental:**
- Frequ\u00eancia: A cada 4 horas
- Reten\u00e7\u00e3o: 7 dias

**Transaction Logs:**
- Arquivamento cont\u00ednuo
- Reten\u00e7\u00e3o: 90 dias (compliance)

---

## Integra\u00e7\u00f5es Externas

### Vis\u00e3o Geral

O SIWEA integra-se com 3 servi\u00e7os externos de valida\u00e7\u00e3o:

```
                    \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
                    \u2502  SIWEA (Programa COBOL)  \u2502
                    \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u252c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
                             \u2502
        \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
        \u2502                                       \u2502
    Produto                                     Produto
   Cons\u00f3rcio?                                N\u00c3O-Cons\u00f3rcio
   (6814,                                         \u2502
   7701,                                   Consultar
   7709)                                 EF_CONTR_SEG_HABIT
        \u2502                                       \u2502
        \u2502                      \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2534\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
        \u2502                      \u2502                       \u2502
        \u2502                 NUM_CONTRATO > 0      NUM_CONTRATO = 0
        \u2502                      \u2502                   ou NULL
        \u2502                      \u2502                       \u2502
        v                      v                       v
\u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510        \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510       \u250c\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2510
\u2502   CNOUA   \u2502        \u2502   SIPUA   \u2502       \u2502   SIMDA   \u2502
\u2502 (REST API)\u2502        \u2502 (SOAP WS) \u2502       \u2502 (SOAP WS) \u2502
\u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518        \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518       \u2514\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2518
Cons\u00f3rcio              EFP                   HB
Nacional               (Financ. Pr\u00f3prio)     (Habita\u00e7\u00e3o)
```

### 1. CNOUA (Cons\u00f3rcio Nacional)

**Trigger:** CODPRODU \u2208 {6814, 7701, 7709}

**Tipo de Servi\u00e7o:** REST API over HTTP/HTTPS

**Base URL:** `{CNOUA_BASE_URL}/validate` (configurado externamente)

**M\u00e9todo:** POST

**Timeout:** 10 segundos

**Request Headers:**
```http
POST /validate HTTP/1.1
Host: cnoua.caixaseguradora.com.br
Content-Type: application/json
Accept: application/json
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "claimId": 12345,
  "productCode": "6814",
  "policyNumber": "001/0123456",
  "contractNumber": "CON-2024-001",
  "amount": 25000.00,
  "currencyCode": "BRL"
}
```

**Success Response (200 OK):**
```json
{
  "status": "APPROVED",
  "ezert8Code": "00000000",
  "validatedAt": "2025-10-23T14:30:00Z",
  "responseTimeMs": 1250
}
```

**Error Response (200 OK with error code):**
```json
{
  "status": "REJECTED",
  "ezert8Code": "EZERT8001",
  "message": "Contrato de cons\u00f3rcio inv\u00e1lido",
  "detail": "Contrato CON-2024-001 n\u00e3o encontrado na base CNOUA",
  "responseTimeMs": 850
}
```

**C\u00f3digos de Erro:**
| C\u00f3digo | Mensagem | A\u00e7\u00e3o |
|--------|----------|-------|
| 00000000 | Sucesso | Prosseguir |
| EZERT8001 | Contrato de cons\u00f3rcio inv\u00e1lido | Abortar |
| EZERT8002 | Contrato cancelado | Abortar |
| EZERT8003 | Grupo encerrado | Abortar |
| EZERT8004 | Cota n\u00e3o contemplada | Abortar |
| EZERT8005 | Benefici\u00e1rio n\u00e3o autorizado | Abortar |

**Resili\u00eancia:**
```
Retry Policy: 3 tentativas com backoff exponencial (2s, 4s, 8s)
Circuit Breaker: Abre ap\u00f3s 5 falhas consecutivas, break duration 30s
Fallback: Retornar erro ao usu\u00e1rio (n\u00e3o permitir prosseguir)
Timeout: 10 segundos hard limit
```

### 2. SIPUA (Sistema de Processamento de Unidades Aut\u00f4nomas)

**Trigger:** Produto N\u00c3O-Cons\u00f3rcio AND `EF_CONTR_SEG_HABIT.NUM_CONTRATO > 0`

**Tipo de Servi\u00e7o:** SOAP 1.2 Web Service

**WSDL URL:** `{SIPUA_BASE_URL}/services/validation?wsdl`

**Namespace:** `http://sipua.validation.caixa.com.br`

**Timeout:** 10 segundos

**SOAP Request:**
```xml
<soapenv:Envelope
    xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
    xmlns:val="http://sipua.validation.caixa.com.br">
   <soapenv:Header>
      <val:Authentication>
         <val:username>SIWEA_USER</val:username>
         <val:password>***</val:password>
      </val:Authentication>
   </soapenv:Header>
   <soapenv:Body>
      <val:ValidateEFPContract>
         <val:contractNumber>12345</val:contractNumber>
         <val:policyNumber>001/0123456-7</val:policyNumber>
         <val:claimNumber>10/20/789012</val:claimNumber>
         <val:principalAmount>25000.00</val:principalAmount>
      </val:ValidateEFPContract>
   </soapenv:Body>
</soapenv:Envelope>
```

**SOAP Success Response:**
```xml
<soapenv:Envelope>
   <soapenv:Body>
      <val:ValidateEFPContractResponse>
         <val:result>
            <val:ezert8Code>00000000</val:ezert8Code>
            <val:status>APPROVED</val:status>
            <val:timestamp>2025-10-23T14:30:00Z</val:timestamp>
         </val:result>
      </val:ValidateEFPContractResponse>
   </soapenv:Body>
</soapenv:Envelope>
```

**SOAP Error Response:**
```xml
<soapenv:Envelope>
   <soapenv:Body>
      <val:ValidateEFPContractResponse>
         <val:result>
            <val:ezert8Code>EZERT8002</val:ezert8Code>
            <val:status>REJECTED</val:status>
            <val:message>Contrato cancelado</val:message>
            <val:timestamp>2025-10-23T14:30:00Z</val:timestamp>
         </val:result>
      </val:ValidateEFPContractResponse>
   </soapenv:Body>
</soapenv:Envelope>
```

**Resili\u00eancia:** Mesma pol\u00edtica do CNOUA

### 3. SIMDA (Sistema de Habita\u00e7\u00e3o)

**Trigger:** Produto N\u00c3O-Cons\u00f3rcio AND (`EF_CONTR_SEG_HABIT.NUM_CONTRATO = 0` OR `NOT FOUND`)

**Tipo de Servi\u00e7o:** SOAP 1.2 Web Service

**WSDL URL:** `{SIMDA_BASE_URL}/services/validation?wsdl`

**Namespace:** `http://simda.validation.caixa.com.br`

**Timeout:** 10 segundos

**SOAP Request/Response:** Estrutura similar ao SIPUA, com campos espec\u00edficos de HB

**Resili\u00eancia:** Mesma pol\u00edtica do CNOUA e SIPUA

---

## Gest\u00e3o de Transa\u00e7\u00f5es

### Princ\u00edpio ACID

O SIWEA garante atomicidade completa da autoriza\u00e7\u00e3o de pagamento:

**Atomicidade:** Ou TUDO acontece ou NADA acontece
```
\u2705 Insert THISTSIN
\u2705 Update TMESTSIN
\u2705 Insert SI_ACOMPANHA_SINI
\u2705 Update SI_SINISTRO_FASE
\u2705 COMMIT

OU

\u274c Rollback de TUDO se qualquer step falhar
```

**Consist\u00eancia:** Dados sempre em estado v\u00e1lido
```
- TOTPAG nunca excede SDOPAG
- OCORHIST sempre incrementa em sequ\u00eancia
- Foreign keys sempre v\u00e1lidas
- Constraints sempre satisfeitas
```

**Isolamento:** Transa\u00e7\u00f5es concorrentes n\u00e3o interferem
```
Isolation Level: READ COMMITTED
Locking: Row-level pessimistic locks
Timeout: 90 segundos
Deadlock Detection: Autom\u00e1tico (DB2)
```

**Durabilidade:** Commit garante persist\u00eancia permanente
```
Write-Ahead Logging (WAL)
Checkpoints a cada 5 minutos
Recovery autom\u00e1tico em caso de crash
```

### Gerenciamento de Locks

**Estrat\u00e9gia de Locking:**
```sql
-- 1. Lock claim master para leitura
SELECT * FROM TMESTSIN
WHERE FONTE = @fonte AND PROTSINI = @protsini AND DAC = @dac
FOR UPDATE;  -- Pessimistic lock

-- 2. Insert history (sem lock adicional necess\u00e1rio)
INSERT INTO THISTSIN (...) VALUES (...);

-- 3. Update claim master (j\u00e1 locked)
UPDATE TMESTSIN
SET TOTPAG = TOTPAG + @valtotbt,
    OCORHIST = OCORHIST + 1
WHERE FONTE = @fonte AND PROTSINI = @protsini AND DAC = @dac;

-- 4. Insert accompaniment
INSERT INTO SI_ACOMPANHA_SINI (...) VALUES (...);

-- 5. Update/Insert phases
-- (locks adicionais conforme necess\u00e1rio)

COMMIT;  -- Libera todos os locks
```

**Preven\u00e7\u00e3o de Deadlocks:**
- Sempre adquirir locks na mesma ordem (TMESTSIN primeiro)
- Timeout de transa\u00e7\u00e3o: 90 segundos
- DB2 detecta e resolve deadlocks automaticamente (abort uma transa\u00e7\u00e3o)

### Rollback em Caso de Erro

**Gatilhos de Rollback:**
1. Valida\u00e7\u00e3o externa falhou (EZERT8 != '00000000')
2. Taxa de convers\u00e3o n\u00e3o encontrada
3. Viola\u00e7\u00e3o de constraint (TOTPAG > SDOPAG)
4. Erro de SQL (viola\u00e7\u00e3o de FK, unique, etc.)
5. Timeout de transa\u00e7\u00e3o
6. Erro ao atualizar fases

**C\u00f3digo COBOL:**
```cobol
EXEC SQL
    BEGIN TRANSACTION
END-EXEC.

PERFORM VALIDATE-EXTERNAL-SERVICE.
IF ERROR-OCCURRED
    EXEC SQL ROLLBACK END-EXEC
    MOVE 'VALIDATION-FAILED' TO ERROR-CODE
    GO TO ERROR-HANDLER
END-IF.

PERFORM INSERT-HISTORY.
IF SQLCODE NOT = 0
    EXEC SQL ROLLBACK END-EXEC
    MOVE 'DB-ERROR' TO ERROR-CODE
    GO TO ERROR-HANDLER
END-IF.

-- ... outros steps ...

EXEC SQL
    COMMIT
END-EXEC.

IF SQLCODE = 0
    MOVE 'SUCCESS' TO RETURN-CODE
ELSE
    EXEC SQL ROLLBACK END-EXEC
    MOVE 'COMMIT-FAILED' TO ERROR-CODE
    GO TO ERROR-HANDLER
END-IF.
```

---

## Seguran\u00e7a e Auditoria

### Autentica\u00e7\u00e3o

**M\u00e9todo:** RACF (Resource Access Control Facility) do z/OS

**Fluxo:**
```
1. Usu\u00e1rio conecta-se ao CICS via TN3270
2. CICS solicita credenciais (UserID + Password)
3. RACF valida credenciais
4. Se sucesso: CICS associa UserID \u00e0 sess\u00e3o
5. Se falha: Conex\u00e3o rejeitada
```

**UserID:** Usado para auditoria (campo EZEUSRID)

### Autoriza\u00e7\u00e3o

**Controle de Acesso:**
- RACF groups definem permiss\u00f5es
- SIWEA requer grupo `CLAIMS_OPERATORS`
- Permiss\u00f5es adicionais para opera\u00e7\u00f5es espec\u00edficas (futuro)

**N\u00edveis de Acesso (futuro):**
| Grupo | Permiss\u00f5es |
|-------|--------------|
| CLAIMS_VIEWERS | Buscar e visualizar sinistros |
| CLAIMS_OPERATORS | Autorizar pagamentos at\u00e9 R$ 50k |
| CLAIMS_SUPERVISORS | Autorizar pagamentos at\u00e9 R$ 500k |
| CLAIMS_MANAGERS | Autorizar qualquer valor |

### Trilha de Auditoria

**Campos de Auditoria em TODAS as tabelas:**
```
CREATED_BY      VARCHAR(50)  -- UserID de quem criou
CREATED_AT      DATETIME     -- Timestamp de cria\u00e7\u00e3o
UPDATED_BY      VARCHAR(50)  -- UserID de quem alterou
UPDATED_AT      DATETIME     -- Timestamp de altera\u00e7\u00e3o
```

**Campos Espec\u00edficos de Transa\u00e7\u00e3o:**
```
THISTSIN.EZEUSRID             -- Operador que autorizou
THISTSIN.DTMOVTO              -- Data da transa\u00e7\u00e3o
THISTSIN.HORAOPER             -- Hora da transa\u00e7\u00e3o
SI_ACOMPANHA_SINI.COD_USUARIO -- Operador do evento
```

**Reconstitui\u00e7\u00e3o de Hist\u00f3rico:**
```sql
-- Timeline completa de um sinistro
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
    h.FONTE = a.FONTE
    AND h.PROTSINI = a.PROTSINI
    AND h.DAC = a.DAC
    AND h.OCORHIST = a.NUM_OCORR_SINIACO
    AND h.OPERACAO = a.COD_EVENTO
)
WHERE a.FONTE = @fonte
  AND a.PROTSINI = @protsini
  AND a.DAC = @dac
ORDER BY a.DATA_EVENTO DESC, a.HORA_EVENTO DESC;
```

### Conformidade (Compliance)

**Requisitos:**
- **SOX (Sarbanes-Oxley):** Trilha de auditoria completa, imutabilidade de registros hist\u00f3ricos
- **LGPD (Lei Geral de Prote\u00e7\u00e3o de Dados):** Dados pessoais (nome segurado, favorecido) armazenados com seguran\u00e7a
- **Bacen (Banco Central):** Reten\u00e7\u00e3o de registros financeiros por 10 anos
- **SUSEP (Superintend\u00eancia de Seguros Privados):** Rastreabilidade de pagamentos de sinistros

**Imutabilidade de Dados:**
- THISTSIN: Registros hist\u00f3ricos NUNCA s\u00e3o alterados ou deletados
- DTMOVTO: Campo imut\u00e1vel (trigger impede altera\u00e7\u00e3o)
- SI_ACOMPANHA_SINI: Somente INSERT permitido (audit log)

---

## Infraestrutura de Execu\u00e7\u00e3o

### Hardware: IBM z/OS Mainframe

**Modelo:** IBM z15 (ou similar)

**Caracter\u00edsticas:**
```
Processadores:   24 cores z15 @ 5.2 GHz
Mem\u00f3ria:         512 GB RAM
Armazenamento:   SAN (Storage Area Network) com 10 TB
Redund\u00e2ncia:     Sistema duplicado em datacenter secund\u00e1rio
```

### Software: CICS Transaction Server

**Vers\u00e3o:** CICS TS 5.6 (ou superior)

**Configura\u00e7\u00e3o:**
```
Region Name:     CICSPROD
Max Tasks:       500 concurrent tasks
Transaction ID:  SIWEA
Program Name:    SIWEA-V116
```

**Connection Pooling:**
```
DB2 Connections: 50 connections (pool)
Timeout:         90 segundos
Max Wait:        30 segundos
```

### Disponibilidade

**SLA (Service Level Agreement):**
```
Disponibilidade: 99.9% (8,76 horas downtime/ano)
Hor\u00e1rio:        24x7x365
Janela Manuten\u00e7\u00e3o: Dom\u00edngos 02:00-05:00 (3h/semana)
```

**Alta Disponibilidade:**
```
Primary Datacenter:   S\u00e3o Paulo
Secondary Datacenter: Rio de Janeiro
Replication:          Synchronous (RPO = 0)
Failover Time:        < 5 minutos (RTO)
```

**Disaster Recovery:**
```
Backup Location:     Bras\u00edlia (tertiary site)
Replication:         Asynchronous
RPO:                 15 minutos
RTO:                 4 horas
```

---

## Desempenho e Escalabilidade

### Requisitos de Performance

**Busca de Sinistros:**
- **Objetivo:** < 3 segundos (95th percentile)
- **Atual:** ~1.5 segundos (m\u00e9dia)
- **Otimiza\u00e7\u00e3o:** \u00cdndices em FONTE+PROTSINI+DAC, ORGSIN+RMOSIN+NUMSIN, CODLIDER+SINLID

**Autoriza\u00e7\u00e3o de Pagamento:**
- **Objetivo:** < 90 segundos (99th percentile)
- **Atual:** ~20 segundos (m\u00e9dia, incluindo valida\u00e7\u00e3o externa)
- **Breakdown:**
  - Valida\u00e7\u00e3o entrada: ~0.5s
  - Valida\u00e7\u00e3o externa: ~5s (CNOUA/SIPUA/SIMDA)
  - Transa\u00e7\u00e3o DB2: ~2s
  - Overhead CICS: ~0.5s

**Query de Hist\u00f3rico:**
- **Objetivo:** < 500ms para 1000+ registros
- **Atual:** ~200ms (covering index)

### Escalabilidade

**Vertical Scaling (Current):**
```
Concurrent Users:     500 operadores simult\u00e2neos
Transactions/Second:  ~100 TPS (autoriza\u00e7\u00f5es)
Searches/Second:      ~500 SPS (buscas)
Database Size:        ~2 TB (hist\u00f3rico de 35 anos)
Growth Rate:          ~50 GB/ano
```

**Horizontal Scaling (Future):**
```
CICS Regions:         Pode escalar para m\u00faltiplas regions
DB2 Partitioning:     Data partitioning por ano (TMESTSIN, THISTSIN)
Read Replicas:        Queries de leitura podem usar replicas
Load Balancing:       Connection pooling autom\u00e1tico
```

### Monitoramento

**M\u00e9tricas Coletadas:**
```
- Response time por transa\u00e7\u00e3o
- CPU usage (CICS region)
- Mem\u00f3ria usage (CICS region)
- DB2 connection pool usage
- SQL query execution time
- External service response time (CNOUA, SIPUA, SIMDA)
- Error rate por tipo
- Concurrent users
```

**Alertas:**
```
- Response time > 5s: Warning
- Response time > 10s: Critical
- Error rate > 1%: Warning
- Error rate > 5%: Critical
- DB2 connection pool > 80%: Warning
- CPU usage > 80%: Warning
```

---

## Conclus\u00e3o

### Pontos Fortes da Arquitetura Legada

\u2705 **Estabilidade:** 35 anos de opera\u00e7\u00e3o cont\u00ednua sem falhas cr\u00edticas
\u2705 **Performance:** Resposta r\u00e1pida mesmo com grandes volumes
\u2705 **Integridade:** Transa\u00e7\u00f5es ACID garantem consist\u00eancia
\u2705 **Auditoria:** Trilha completa de todas as opera\u00e7\u00f5es
\u2705 **Escalabilidade:** Suporta centenas de usu\u00e1rios concorrentes

### Desafios para Migra\u00e7\u00e3o

\u26a0\ufe0f **Complexidade:** L\u00f3gica de neg\u00f3cio embarcada em COBOL legado
\u26a0\ufe0f **Integra\u00e7\u00f5es:** Servi\u00e7os externos podem ter interfaces diferentes
\u26a0\ufe0f **Precis\u00e3o:** C\u00e1lculos financeiros exigem precis\u00e3o exata (8 decimais)
\u26a0\ufe0f **Concorr\u00eancia:** Gest\u00e3o de locks e transa\u00e7\u00f5es deve ser preservada
\u26a0\ufe0f **Compliance:** Requisitos de auditoria e reten\u00e7\u00e3o devem ser mantidos

### Pr\u00f3ximos Passos

1. Revisar **SISTEMA_LEGADO_MODELO_DADOS.md** para detalhes completos das 13 entidades
2. Revisar **SISTEMA_LEGADO_PROCESSOS.md** para workflows e diagramas de sequ\u00eancia
3. Implementar POC (.NET 9 + React) seguindo arquitetura equivalente
4. Validar paridade de funcionalidade com sistema legado
5. Testes de carga e stress para validar performance

---

**FIM - DOCUMENTO 3/5 - ARQUITETURA COMPLETA**
