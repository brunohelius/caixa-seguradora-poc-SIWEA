# Diagramas de Casos de Uso - SIWEA

## Diagrama Principal - Sistema Completo

```mermaid
graph TB
    subgraph Sistema_SIWEA["SISTEMA SIWEA - Liberação de Pagamento de Sinistros"]
        UC1[UC-01: Buscar Sinistro por Protocolo]
        UC2[UC-02: Buscar Sinistro por Número]
        UC3[UC-03: Buscar Sinistro por Código Líder]
        UC4[UC-04: Autorizar Pagamento]
        UC5[UC-05: Validar Produto Consórcio]
        UC6[UC-06: Validar Contrato EFP]
        UC7[UC-07: Validar Contrato HB]
        UC8[UC-08: Converter Valores para BTNF]
        UC9[UC-09: Registrar Histórico]
        UC10[UC-10: Atualizar Fases]
        UC11[UC-11: Consultar Histórico de Pagamentos]
        UC12[UC-12: Visualizar Dashboard]
    end

    subgraph Atores_Principais["Atores"]
        OP[Operador de Sinistros]
        SYS[Sistema Contábil]
    end

    subgraph Sistemas_Externos["Sistemas Externos"]
        CNOUA[API CNOUA<br/>Consórcios]
        SIPUA[WS SIPUA<br/>EFP]
        SIMDA[WS SIMDA<br/>HB]
    end

    OP -->|executa| UC1
    OP -->|executa| UC2
    OP -->|executa| UC3
    OP -->|executa| UC4
    OP -->|executa| UC11
    OP -->|executa| UC12

    UC4 -->|includes| UC8
    UC4 -->|includes| UC9
    UC4 -->|includes| UC10

    UC4 -->|extends| UC5
    UC4 -->|extends| UC6
    UC4 -->|extends| UC7

    UC5 -.->|chama| CNOUA
    UC6 -.->|chama| SIPUA
    UC7 -.->|chama| SIMDA

    UC9 -.->|notifica| SYS

    style UC4 fill:#ff6b6b,stroke:#c92a2a,stroke-width:3px
    style OP fill:#4dabf7,stroke:#1971c2
    style CNOUA fill:#51cf66,stroke:#2f9e44
    style SIPUA fill:#51cf66,stroke:#2f9e44
    style SIMDA fill:#51cf66,stroke:#2f9e44
```

---

## UC-01: Buscar Sinistro por Protocolo

```mermaid
sequenceDiagram
    actor OP as Operador
    participant UI as Tela SI11M010
    participant SIWEA as Sistema SIWEA
    participant DB as Banco de Dados

    OP->>UI: Informa FONTE + PROTSINI + DAC
    OP->>UI: Pressiona ENTER
    UI->>SIWEA: ValidarCriterioProtocolo()

    alt Critério válido
        SIWEA->>DB: SELECT FROM TMESTSIN WHERE FONTE=X AND PROTSINI=Y AND DAC=Z

        alt Sinistro encontrado
            DB-->>SIWEA: Dados do sinistro
            SIWEA->>DB: SELECT FROM TGERAMO (Nome do ramo)
            DB-->>SIWEA: Nome do ramo
            SIWEA->>DB: SELECT FROM TAPOLICE (Nome segurado)
            DB-->>SIWEA: Nome segurado
            SIWEA->>UI: Exibir SIHM020 com dados completos
            UI-->>OP: Tela de autorização carregada
        else Sinistro não encontrado
            DB-->>SIWEA: 0 registros
            SIWEA->>UI: Erro "PROTOCOLO XXX-X NAO ENCONTRADO"
            UI-->>OP: Mensagem de erro
        end
    else Critério incompleto
        SIWEA->>UI: Erro "Informe critério completo"
        UI-->>OP: Mensagem de validação
    end
```

**Descrição:**
- **Ator Principal:** Operador de Sinistros
- **Pré-condição:** Operador autenticado no sistema
- **Pós-condição:** Sinistro localizado e tela SIHM020 exibida

**Fluxo Principal:**
1. Operador informa FONTE, PROTSINI e DAC
2. Sistema valida que todos os 3 campos foram preenchidos
3. Sistema consulta TMESTSIN usando os 3 campos
4. Sistema carrega dados complementares (ramo, segurado)
5. Sistema calcula saldo pendente (SDOPAG - TOTPAG)
6. Sistema exibe tela SIHM020 com dados do sinistro

**Fluxo Alternativo - Protocolo não encontrado:**
- 3a. Sistema não localiza registro
- 3b. Sistema exibe mensagem "PROTOCOLO XXX-X NAO ENCONTRADO"
- 3c. Operador pode tentar outro critério

**Regras de Negócio:**
- BR-001: Três critérios mutuamente exclusivos
- BR-002: Obrigatoriedade de critério completo
- BR-003: Recuperação de dados do registro mestre

---

## UC-02: Buscar Sinistro por Número

```mermaid
sequenceDiagram
    actor OP as Operador
    participant UI as Tela SI11M010
    participant SIWEA as Sistema SIWEA
    participant DB as Banco de Dados

    OP->>UI: Informa ORGSIN + RMOSIN + NUMSIN
    OP->>UI: Pressiona ENTER
    UI->>SIWEA: ValidarCriterioSinistro()

    alt Critério válido
        SIWEA->>DB: SELECT FROM TMESTSIN WHERE ORGSIN=X AND RMOSIN=Y AND NUMSIN=Z

        alt Sinistro encontrado
            DB-->>SIWEA: Dados do sinistro
            SIWEA->>SIWEA: CarregarDadosComplementares()
            SIWEA->>UI: Exibir SIHM020
            UI-->>OP: Tela de autorização
        else Sinistro não encontrado
            DB-->>SIWEA: 0 registros
            SIWEA->>UI: Erro "SINISTRO XXXXXXX NAO ENCONTRADO"
            UI-->>OP: Mensagem de erro
        end
    else Critério incompleto
        SIWEA->>UI: Erro validação
        UI-->>OP: Mensagem
    end
```

**Descrição:**
- **Ator Principal:** Operador de Sinistros
- **Pré-condição:** Operador autenticado
- **Pós-condição:** Sinistro localizado

**Fluxo Principal:**
1. Operador informa ORGSIN (2 dígitos), RMOSIN (2 dígitos), NUMSIN (6 dígitos)
2. Sistema valida formato e completude
3. Sistema consulta TMESTSIN pela chave composta
4. Sistema exibe dados do sinistro

**Validações:**
- ORGSIN: 01-99
- RMOSIN: 00-99
- NUMSIN: 000001-999999

---

## UC-03: Buscar Sinistro por Código Líder

```mermaid
graph LR
    A[Operador informa<br/>CODLIDER + SINLID] --> B{Campos<br/>completos?}
    B -->|Sim| C[Query TMESTSIN<br/>by Leader Code]
    B -->|Não| D[Erro: Critério<br/>incompleto]
    C --> E{Encontrado?}
    E -->|Sim| F[Carregar dados<br/>complementares]
    E -->|Não| G[Erro: Líder não<br/>encontrado]
    F --> H[Exibir SIHM020]

    style A fill:#e3f2fd
    style H fill:#c8e6c9
    style D fill:#ffcdd2
    style G fill:#ffcdd2
```

**Descrição:**
- **Ator Principal:** Operador de Sinistros
- **Objetivo:** Localizar sinistro por código de líder de resseguro

**Regra de Negócio Específica:**
- Este modo identifica sinistros de resseguro/líder
- CODLIDER: 3 dígitos (001-999)
- SINLID: 7 dígitos (0000001-9999999)

---

## UC-04: Autorizar Pagamento (Caso de Uso Principal)

```mermaid
sequenceDiagram
    actor OP as Operador
    participant UI as SIHM020
    participant SIWEA as Sistema SIWEA
    participant VAL as Validador
    participant CONV as Conversor BTNF
    participant EXT as Serviços Externos
    participant DB as Banco de Dados
    participant FASE as Gestor de Fases

    Note over OP,FASE: FASE 1 - Entrada de Dados
    OP->>UI: Preenche dados:<br/>- Tipo Pagamento<br/>- Tipo Apólice<br/>- Valor Principal<br/>- Valor Correção<br/>- Beneficiário
    OP->>UI: Pressiona ENTER

    Note over OP,FASE: FASE 2 - Validações de Entrada
    UI->>VAL: ValidarDadosEntrada()
    VAL->>VAL: ✓ Tipo Pagamento IN (1,2,3,4,5)
    VAL->>VAL: ✓ Tipo Apólice IN (1,2)
    VAL->>VAL: ✓ Valor Principal > 0
    VAL->>VAL: ✓ Beneficiário (se TPSEGU != 0)

    alt Validação falhou
        VAL-->>UI: Erro de validação
        UI-->>OP: Exibir erro, manter tela
    end

    Note over OP,FASE: FASE 3 - Validações de Negócio
    VAL->>DB: SELECT SDOPAG, TOTPAG FROM TMESTSIN
    DB-->>VAL: Saldos atuais
    VAL->>VAL: Verificar: Valor <= (SDOPAG - TOTPAG)

    alt Valor excede saldo
        VAL-->>UI: Erro "VALOR EXCEDE SALDO PENDENTE"
        UI-->>OP: Mensagem erro
    end

    Note over OP,FASE: FASE 4 - Obter Taxa BTNF
    VAL->>DB: SELECT DTMOVABE FROM TSISTEMA
    DB-->>VAL: Data de negócio
    VAL->>DB: SELECT VLCRUZAD FROM TGEUNIMO<br/>WHERE DTINIVIG <= DATA <= DTTERVIG
    DB-->>VAL: Taxa de conversão

    alt Taxa não encontrada
        VAL-->>UI: Erro "TAXA NAO DISPONIVEL"
        UI-->>OP: Mensagem erro
    end

    Note over OP,FASE: FASE 5 - Conversão Monetária
    VAL->>CONV: ConverterParaBTNF(VALPRI, CRRMON, VLCRUZAD)
    CONV->>CONV: VALPRIBT = VALPRI × VLCRUZAD
    CONV->>CONV: CRRMONBT = CRRMON × VLCRUZAD
    CONV->>CONV: VALTOTBT = VALPRIBT + CRRMONBT
    CONV->>CONV: Arredondar para 2 decimais (Banker's Rounding)
    CONV-->>VAL: Valores convertidos

    Note over OP,FASE: FASE 6 - Validação Externa
    VAL->>VAL: DeterminarRota(CODPRODU)

    alt CODPRODU IN (6814,7701,7709)
        VAL->>EXT: POST /cnoua/validate
        EXT-->>VAL: {EZERT8: '00000000'}
    else NUM_CONTRATO > 0
        VAL->>EXT: SOAP SIPUA.validatePayment
        EXT-->>VAL: {EZERT8: '00000000'}
    else NUM_CONTRATO = 0 or NULL
        VAL->>EXT: SOAP SIMDA.validatePayment
        EXT-->>VAL: {EZERT8: '00000000'}
    end

    alt EZERT8 != '00000000'
        EXT-->>VAL: Erro de validação
        VAL-->>UI: Exibir erro do serviço
        UI-->>OP: Mensagem erro
    end

    Note over OP,FASE: FASE 7 - Transação ACID
    VAL->>DB: BEGIN TRANSACTION

    VAL->>DB: INSERT INTO THISTSIN<br/>(OPERACAO=1098, TIPCRR='5', VALPRIBT, CRRMONBT, VALTOTBT)
    DB-->>VAL: OK

    VAL->>DB: UPDATE TMESTSIN<br/>SET TOTPAG = TOTPAG + VALTOTBT,<br/>    OCORHIST = OCORHIST + 1
    DB-->>VAL: 1 row affected

    VAL->>DB: INSERT INTO SI_ACOMPANHA_SINI<br/>(COD_EVENTO=1098)
    DB-->>VAL: OK

    VAL->>FASE: AtualizarFases(evento=1098)
    FASE->>DB: SELECT FROM SI_REL_FASE_EVENTO<br/>WHERE COD_EVENTO=1098
    DB-->>FASE: Fases a abrir/fechar

    loop Para cada fase
        alt IND_ALTERACAO_FASE = '1'
            FASE->>DB: INSERT SI_SINISTRO_FASE<br/>(DATA_FECHA='9999-12-31')
        else IND_ALTERACAO_FASE = '2'
            FASE->>DB: UPDATE SI_SINISTRO_FASE<br/>SET DATA_FECHA=HOJE
        end
    end

    FASE-->>VAL: Fases atualizadas

    alt Algum erro na transação
        VAL->>DB: ROLLBACK
        VAL-->>UI: Erro de sistema
        UI-->>OP: Mensagem erro
    else Tudo OK
        VAL->>DB: COMMIT
        DB-->>VAL: Committed
        VAL-->>UI: Sucesso
        UI-->>OP: "PAGAMENTO AUTORIZADO COM SUCESSO"
    end
```

**Descrição Completa:**

**Atores:**
- **Principal:** Operador de Sinistros
- **Secundário:** Sistema Contábil (notificado após COMMIT)

**Pré-condições:**
1. Operador autenticado
2. Sinistro localizado via UC-01, UC-02 ou UC-03
3. Tela SIHM020 exibida com dados do sinistro

**Pós-condições (Sucesso):**
1. Registro inserido em THISTSIN
2. TMESTSIN.TOTPAG atualizado
3. TMESTSIN.OCORHIST incrementado
4. Registro inserido em SI_ACOMPANHA_SINI
5. Fases abertas/fechadas conforme configuração
6. Sistema contábil notificado

**Pós-condições (Falha):**
1. Nenhuma mudança persistida (ROLLBACK)
2. Mensagem de erro exibida ao operador
3. Log de auditoria registrado

**Fluxo Principal:**
1. Operador preenche dados de pagamento
2. Sistema valida entrada (tipo, formato, obrigatoriedade)
3. Sistema valida negócio (saldo disponível)
4. Sistema obtém data de negócio e taxa BTNF
5. Sistema calcula valores convertidos
6. Sistema determina rota de validação (CNOUA/SIPUA/SIMDA)
7. Sistema chama serviço externo de validação
8. Sistema inicia transação de banco de dados
9. Sistema insere histórico (THISTSIN)
10. Sistema atualiza sinistro mestre (TMESTSIN)
11. Sistema registra acompanhamento (SI_ACOMPANHA_SINI)
12. Sistema atualiza fases (SI_SINISTRO_FASE)
13. Sistema comita transação
14. Sistema exibe mensagem de sucesso

**Fluxos Alternativos:**

**A1: Validação de entrada falha**
- Sistema exibe erro específico
- Operador corrige e reenvia

**A2: Valor excede saldo pendente**
- Sistema exibe "VALOR EXCEDE SALDO PENDENTE"
- Operador reduz valor ou cancela

**A3: Taxa BTNF não disponível**
- Sistema exibe "TAXA DE CONVERSAO NAO DISPONIVEL PARA A DATA"
- Operador contacta suporte técnico

**A4: Validação externa falha**
- Sistema exibe código EZERT8 e mensagem do serviço
- Operador verifica dados do sinistro/contrato

**A5: Timeout serviço externo**
- Sistema tenta retry (2 vezes)
- Se falha persistente: Circuit breaker abre
- Sistema exibe "SERVICO DE VALIDACAO INDISPONIVEL"

**A6: Erro na transação de banco**
- Sistema executa ROLLBACK total
- Sistema registra log de erro
- Sistema exibe "ERRO AO PROCESSAR PAGAMENTO"
- Operador pode tentar novamente

**Regras de Negócio Aplicadas:**
- BR-010 a BR-022: Autorização de Pagamento
- BR-023 a BR-033: Conversão Monetária
- BR-034 a BR-042: Registro de Transações
- BR-043 a BR-056: Validação de Produtos
- BR-057 a BR-067: Gestão de Fases
- BR-068 a BR-074: Auditoria

**Requisitos Não-Funcionais:**
- **Performance:** Ciclo completo < 90 segundos
- **Timeout:** Validação externa = 10 segundos
- **Retry:** 2 tentativas com backoff exponencial
- **Isolamento:** READ COMMITTED
- **Disponibilidade:** Circuit breaker após 5 falhas consecutivas

---

## UC-05: Validar Produto Consórcio (CNOUA)

```mermaid
sequenceDiagram
    participant SIWEA as Sistema SIWEA
    participant CNOUA as API CNOUA
    participant CB as Circuit Breaker

    SIWEA->>SIWEA: IF CODPRODU IN (6814, 7701, 7709)

    SIWEA->>CB: Verificar estado do circuit

    alt Circuit ABERTO (muitas falhas)
        CB-->>SIWEA: Erro imediato (fail-fast)
        SIWEA->>SIWEA: LOG "Circuit breaker aberto"
    else Circuit FECHADO ou MEIO-ABERTO
        CB-->>SIWEA: Permitir chamada

        SIWEA->>CNOUA: POST /validate<br/>{claimId, productCode, amount, contract}

        Note over SIWEA,CNOUA: Timeout: 10 segundos

        alt Resposta recebida < 10s
            alt EZERT8 = '00000000'
                CNOUA-->>SIWEA: {EZERT8: '00000000', approved: true}
                SIWEA->>CB: Registrar sucesso
                CB->>CB: Reset contador de falhas
                SIWEA->>SIWEA: Continuar com autorização
            else EZERT8 != '00000000'
                CNOUA-->>SIWEA: {EZERT8: 'EZERT8001', approved: false, message: 'Contrato inválido'}
                SIWEA->>CB: Registrar falha de negócio (não conta para circuit)
                SIWEA->>SIWEA: ABORTAR transação
                SIWEA->>SIWEA: Exibir erro: "VALIDACAO CONSORCIO FALHOU: {message}"
            end
        else Timeout (10s)
            CNOUA-xSIWEA: Sem resposta

            SIWEA->>SIWEA: Retry 1 (após 1 segundo)
            SIWEA->>CNOUA: POST /validate (retry 1)

            alt Retry 1 sucesso
                CNOUA-->>SIWEA: {EZERT8: '00000000'}
                SIWEA->>SIWEA: Continuar
            else Retry 1 timeout
                SIWEA->>SIWEA: Retry 2 (após 2 segundos)
                SIWEA->>CNOUA: POST /validate (retry 2)

                alt Retry 2 sucesso
                    CNOUA-->>SIWEA: {EZERT8: '00000000'}
                    SIWEA->>SIWEA: Continuar
                else Retry 2 timeout
                    SIWEA->>CB: Registrar falha técnica
                    CB->>CB: Incrementar contador (se >= 5: ABRIR circuit)
                    SIWEA->>SIWEA: ABORTAR transação
                    SIWEA->>SIWEA: Exibir: "SERVICO DE VALIDACAO INDISPONIVEL"
                end
            end
        end
    end
```

**Descrição:**
- **Trigger:** CODPRODU IN (6814, 7701, 7709)
- **Timeout:** 10 segundos por tentativa
- **Retries:** 2 tentativas (backoff: 1s, 2s)
- **Circuit Breaker:** Abre após 5 falhas consecutivas, fecha após 60s

**Códigos de Resposta CNOUA:**
- `00000000`: Validação bem-sucedida
- `EZERT8001`: Contrato de consórcio inválido
- `EZERT8002`: Contrato cancelado
- `EZERT8003`: Grupo fechado
- `EZERT8004`: Cota não contemplada
- `EZERT8005`: Beneficiário não autorizado

---

## UC-06: Validar Contrato EFP (SIPUA)

```mermaid
graph TB
    A[Sistema determina<br/>NUM_CONTRATO > 0] --> B[Preparar SOAP Envelope]
    B --> C[POST https://sipua.caixa.gov.br/<br/>services/PaymentValidation]
    C --> D{Resposta<br/>< 10s?}
    D -->|Sim| E{EZERT8 =<br/>'00000000'?}
    D -->|Não| F[Retry 1]
    F --> G{Sucesso?}
    G -->|Não| H[Retry 2]
    H --> I{Sucesso?}
    I -->|Não| J[Erro: Serviço<br/>indisponível]
    E -->|Sim| K[Validação OK<br/>Continuar]
    E -->|Não| L[Erro: Validação<br/>falhou]
    G -->|Sim| K
    I -->|Sim| K

    style K fill:#c8e6c9
    style J fill:#ffcdd2
    style L fill:#ffcdd2
```

**Descrição:**
- **Trigger:** NUM_CONTRATO > 0 (produto não-consórcio)
- **Protocolo:** SOAP 1.2
- **Endpoint:** https://sipua.caixa.gov.br/services/PaymentValidation?wsdl

**Request SOAP:**
```xml
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                  xmlns:pay="http://caixa.gov.br/sipua/payment/v1">
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

---

## UC-07: Validar Contrato HB (SIMDA)

```mermaid
sequenceDiagram
    participant SIWEA as Sistema SIWEA
    participant DB as Banco de Dados
    participant SIMDA as WS SIMDA

    SIWEA->>DB: SELECT NUM_CONTRATO<br/>FROM EF_CONTR_SEG_HABIT

    alt NUM_CONTRATO = 0 OR NULL
        DB-->>SIWEA: NULL ou 0
        SIWEA->>SIWEA: Rota: SIMDA (HB)

        SIWEA->>SIMDA: SOAP validatePayment
        Note over SIWEA,SIMDA: Timeout: 10s<br/>Retry: 2x

        alt Validação OK
            SIMDA-->>SIWEA: {EZERT8: '00000000'}
            SIWEA->>SIWEA: Continuar autorização
        else Validação falhou
            SIMDA-->>SIWEA: {EZERT8: 'ERROR_CODE'}
            SIWEA->>SIWEA: ABORTAR transação
        end
    else NUM_CONTRATO > 0
        DB-->>SIWEA: Número válido
        SIWEA->>SIWEA: Rota: SIPUA (não SIMDA)
    end
```

**Descrição:**
- **Trigger:** NUM_CONTRATO = 0 ou NULL
- **Protocolo:** SOAP 1.2
- **Endpoint:** https://simda.caixa.gov.br/services/HBValidation?wsdl

---

## UC-08: Converter Valores para BTNF

```mermaid
graph LR
    A[Valor Principal<br/>VALPRI] --> B[Buscar Taxa<br/>VLCRUZAD]
    C[Valor Correção<br/>CRRMON] --> B
    B --> D{Taxa<br/>encontrada?}
    D -->|Não| E[Erro: Taxa<br/>não disponível]
    D -->|Sim| F[VALPRIBT =<br/>VALPRI × VLCRUZAD]
    F --> G[CRRMONBT =<br/>CRRMON × VLCRUZAD]
    G --> H[VALTOTBT =<br/>VALPRIBT + CRRMONBT]
    H --> I[Arredondar<br/>Banker's Rounding<br/>2 decimais]
    I --> J[Valores<br/>convertidos]

    style J fill:#c8e6c9
    style E fill:#ffcdd2
```

**Descrição:**
- **Entrada:** VALPRI (Decimal 15,2), CRRMON (Decimal 15,2)
- **Taxa:** VLCRUZAD (Decimal 18,8) de TGEUNIMO
- **Saída:** VALPRIBT, CRRMONBT, VALTOTBT (Decimal 15,2)

**Fórmulas:**
```
VALPRIBT = VALPRI × VLCRUZAD
CRRMONBT = CRRMON × VLCRUZAD
VALTOTBT = VALPRIBT + CRRMONBT
```

**Arredondamento:**
- Método: Banker's Rounding (IEEE 754)
- Casas decimais: 2
- Exemplos:
  - 12.345 → 12.34
  - 12.355 → 12.36
  - 12.365 → 12.36

---

## UC-09: Registrar Histórico

```mermaid
graph TB
    A[Início transação] --> B[INSERT THISTSIN]
    B --> C[Campos obrigatórios:<br/>- OCORHIST = TMESTSIN+1<br/>- OPERACAO = 1098<br/>- TIPCRR = '5'<br/>- DTMOVTO = DTMOVABE<br/>- EZEUSRID = userid]
    C --> D{INSERT<br/>OK?}
    D -->|Não| E[ROLLBACK<br/>Erro]
    D -->|Sim| F[UPDATE TMESTSIN<br/>TOTPAG += VALTOTBT<br/>OCORHIST += 1]
    F --> G{UPDATE<br/>OK?}
    G -->|Não| E
    G -->|Sim| H[INSERT<br/>SI_ACOMPANHA_SINI]
    H --> I{INSERT<br/>OK?}
    I -->|Não| E
    I -->|Sim| J[Atualizar fases]
    J --> K[COMMIT]

    style K fill:#c8e6c9
    style E fill:#ffcdd2
```

**Descrição:**
- **Tabelas afetadas:** THISTSIN, TMESTSIN, SI_ACOMPANHA_SINI, SI_SINISTRO_FASE
- **Tipo:** Transação ACID (all-or-nothing)

**Valores fixos:**
- OPERACAO = 1098 (código de autorização)
- TIPCRR = '5' (tipo de correção padrão)
- SITCONTB = '0' (situação contábil inicial)
- SITUACAO = '0' (situação geral inicial)

---

## UC-10: Atualizar Fases

```mermaid
sequenceDiagram
    participant SIWEA as Sistema
    participant REL as SI_REL_FASE_EVENTO
    participant FASE as SI_SINISTRO_FASE

    SIWEA->>REL: SELECT COD_FASE, IND_ALTERACAO_FASE<br/>WHERE COD_EVENTO=1098
    REL-->>SIWEA: Lista de fases

    loop Para cada fase retornada
        alt IND_ALTERACAO_FASE = '1' (ABRIR)
            SIWEA->>FASE: Verificar se fase já aberta<br/>(DATA_FECHA='9999-12-31')

            alt Fase não aberta
                SIWEA->>FASE: INSERT (FONTE, PROTSINI, DAC, COD_FASE,<br/>1098, HOJE, '9999-12-31')
                FASE-->>SIWEA: OK - Fase aberta
            else Fase já aberta
                SIWEA->>SIWEA: LOG "Fase já aberta, ignorar"
            end

        else IND_ALTERACAO_FASE = '2' (FECHAR)
            SIWEA->>FASE: UPDATE SET DATA_FECHA=HOJE<br/>WHERE FONTE=X AND COD_FASE=Y<br/>AND DATA_FECHA='9999-12-31'
            FASE-->>SIWEA: 1 row updated - Fase fechada
        end
    end

    SIWEA->>SIWEA: LOG auditoria de fases
```

**Descrição:**
- **Configuração:** Tabela SI_REL_FASE_EVENTO define quais fases abrir/fechar para cada evento
- **Sentinel:** '9999-12-31' indica fase aberta
- **Evento 1098:** Tipicamente abre fase 10 (Pagamento) e fecha fase 5 (Documentação)

---

## UC-11: Consultar Histórico de Pagamentos

```mermaid
graph TB
    A[Operador seleciona<br/>sinistro] --> B[Sistema consulta<br/>THISTSIN]
    B --> C[SELECT * FROM THISTSIN<br/>WHERE TIPSEG=X AND ORGSIN=Y<br/>AND RMOSIN=Z AND NUMSIN=W<br/>ORDER BY OCORHIST DESC]
    C --> D[Listar histórico:<br/>- Data movimento<br/>- Operação<br/>- Valor<br/>- Beneficiário<br/>- Usuário]
    D --> E[Calcular totais:<br/>- Total pago<br/>- Pendente]
    E --> F[Exibir grid<br/>com histórico]

    style F fill:#c8e6c9
```

**Descrição:**
- **Ordenação:** Decrescente por OCORHIST (mais recente primeiro)
- **Filtros:** Por sinistro (chave 4 partes)
- **Campos exibidos:**
  - Data movimento (DTMOVTO)
  - Tipo operação (OPERACAO)
  - Valor total BTNF (VALTOTBT)
  - Beneficiário (NOMFAV)
  - Usuário (EZEUSRID)

---

## UC-12: Visualizar Dashboard

```mermaid
graph TB
    A[Operador acessa<br/>Dashboard] --> B[Consultar status<br/>MIGRATION_STATUS]
    B --> C[Consultar componentes<br/>COMPONENT_MIGRATION]
    C --> D[Consultar métricas<br/>PERFORMANCE_METRICS]
    D --> E[Calcular KPIs:<br/>- % conclusão<br/>- Velocidade<br/>- Testes passados]
    E --> F[Renderizar gráficos:<br/>- Pizza progress<br/>- Barras user stories<br/>- Timeline atividades]
    F --> G[Exibir dashboard<br/>interativo]

    style G fill:#c8e6c9
```

**Descrição:**
- **Dados exibidos:**
  - Progresso geral do projeto
  - Status de user stories (6 stories)
  - Componentes migrados (telas, regras, entidades)
  - Métricas de performance (Visual Age vs .NET)
  - Timeline de atividades recentes

**Atualização:** Refresh automático a cada 30 segundos

---

## Resumo de Casos de Uso

| UC | Nome | Ator | Complexidade | Prioridade |
|----|------|------|--------------|------------|
| UC-01 | Buscar Sinistro por Protocolo | Operador | Baixa | Alta |
| UC-02 | Buscar Sinistro por Número | Operador | Baixa | Alta |
| UC-03 | Buscar Sinistro por Código Líder | Operador | Baixa | Média |
| UC-04 | Autorizar Pagamento | Operador | **Alta** | **Crítica** |
| UC-05 | Validar Produto Consórcio | Sistema | Média | Alta |
| UC-06 | Validar Contrato EFP | Sistema | Média | Alta |
| UC-07 | Validar Contrato HB | Sistema | Média | Alta |
| UC-08 | Converter Valores BTNF | Sistema | Baixa | Alta |
| UC-09 | Registrar Histórico | Sistema | Média | Crítica |
| UC-10 | Atualizar Fases | Sistema | Média | Alta |
| UC-11 | Consultar Histórico | Operador | Baixa | Média |
| UC-12 | Visualizar Dashboard | Operador | Baixa | Baixa |

---

## Matriz de Dependências

```mermaid
graph TB
    UC04[UC-04<br/>Autorizar Pagamento] --> UC08[UC-08<br/>Converter BTNF]
    UC04 --> UC09[UC-09<br/>Registrar Histórico]
    UC04 --> UC10[UC-10<br/>Atualizar Fases]
    UC04 --> UC05[UC-05<br/>Validar CNOUA]
    UC04 --> UC06[UC-06<br/>Validar SIPUA]
    UC04 --> UC07[UC-07<br/>Validar SIMDA]

    UC01[UC-01<br/>Buscar Protocolo] -.-> UC04
    UC02[UC-02<br/>Buscar Número] -.-> UC04
    UC03[UC-03<br/>Buscar Líder] -.-> UC04

    UC09 -.-> UC11[UC-11<br/>Consultar Histórico]

    style UC04 fill:#ff6b6b,stroke:#c92a2a,stroke-width:3px
    style UC09 fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px
```

**Legenda:**
- Linha sólida (→): `includes` ou dependência obrigatória
- Linha tracejada (-.->): `extends` ou dependência opcional
- Cor vermelha: Casos de uso críticos

---

**FIM DO DOCUMENTO - CASOS DE USO**
