# Diagramas de Estado e Atividade - SIWEA

## Diagrama de Estados: Ciclo de Vida do Sinistro

```mermaid
stateDiagram-v2
    [*] --> Novo: Sinistro registrado

    Novo --> Pendente: Documentação anexada

    Pendente --> EmProcesso: Autorização iniciada
    Pendente --> Bloqueado: Documentação inválida

    Bloqueado --> Pendente: Documentação corrigida
    Bloqueado --> Cancelado: Prazo expirado

    EmProcesso --> Autorizado: Validação aprovada
    EmProcesso --> Bloqueado: Validação reprovada
    EmProcesso --> Pendente: Informações incompletas

    Autorizado --> Pago: Confirmação contábil
    Autorizado --> EmProcesso: Correção solicitada

    Pago --> PagamentoParcial: Pagamento < Saldo
    PagamentoParcial --> Pago: Pagamento complementar
    PagamentoParcial --> Fechado: Encerramento autorizado

    Pago --> Fechado: Quitação total

    Cancelado --> [*]
    Fechado --> [*]

    state EmProcesso {
        [*] --> ValidandoProduto
        ValidandoProduto --> ConvertendoMoeda: Produto válido
        ValidandoProduto --> [*]: Produto inválido
        ConvertendoMoeda --> RegistrandoTransacao
        RegistrandoTransacao --> AtualizandoFases
        AtualizandoFases --> [*]: Concluído
    }

    state Autorizado {
        [*] --> AguardandoContabilizacao
        AguardandoContabilizacao --> AguardandoPagamento: Contabilizado
        AguardandoPagamento --> [*]: Pago
    }
```

**Descrição dos Estados:**

| Estado | Descrição | Condição de Entrada | Ações Possíveis |
|--------|-----------|---------------------|-----------------|
| **Novo** | Sinistro recém-criado | Registro inicial | Anexar documentação |
| **Pendente** | Aguardando análise | Documentação completa | Iniciar autorização |
| **EmProcesso** | Autorização em andamento | Validações iniciadas | Aguardar conclusão |
| **Bloqueado** | Pendência impeditiva | Validação falhou | Corrigir ou cancelar |
| **Autorizado** | Pagamento aprovado | Todas validações OK | Aguardar confirmação |
| **Pago** | Valor creditado | Confirmação contábil | Verificar saldo |
| **PagamentoParcial** | Saldo remanescente | Pagamento < Esperado | Novo pagamento |
| **Fechado** | Sinistro encerrado | Quitação total | Arquivar |
| **Cancelado** | Sinistro cancelado | Prazo/erro fatal | Arquivar |

---

## Diagrama de Estados: Fase do Sinistro

```mermaid
stateDiagram-v2
    [*] --> NaoIniciada

    NaoIniciada --> Aberta: Evento de abertura<br/>(ex: evento 1098)

    Aberta --> Fechada: Evento de fechamento<br/>(ex: fase concluída)

    Fechada --> [*]

    note right of Aberta
        DATA_FECHA_SIFA = '9999-12-31'
        (valor sentinela)
    end note

    note right of Fechada
        DATA_FECHA_SIFA = data real
        (ex: '2024-10-27')
    end note
```

**Transições:**

| De | Para | Evento | Ação |
|----|------|--------|------|
| Não Iniciada | Aberta | Evento configurado em SI_REL_FASE_EVENTO com IND='1' | INSERT SI_SINISTRO_FASE com DATA_FECHA='9999-12-31' |
| Aberta | Fechada | Evento configurado em SI_REL_FASE_EVENTO com IND='2' | UPDATE SI_SINISTRO_FASE SET DATA_FECHA=HOJE |

**Exemplo:**
```
Fase 5 (Documentação Pendente):
  - Aberta em: 2024-10-20 (evento 1097)
  - DATA_FECHA: '9999-12-31' ← fase aberta
  - Fechada em: 2024-10-27 (evento 1098 - autorização)
  - DATA_FECHA: '2024-10-27' ← fase fechada

Fase 10 (Pagamento):
  - Aberta em: 2024-10-27 (evento 1098 - autorização)
  - DATA_FECHA: '9999-12-31' ← fase aberta
  - Permanece aberta até próximo evento
```

---

## Diagrama de Estados: Circuit Breaker

```mermaid
stateDiagram-v2
    [*] --> Fechado

    Fechado --> Aberto: 5 falhas consecutivas

    Aberto --> MeioAberto: Após 60 segundos

    MeioAberto --> Fechado: Chamada de teste bem-sucedida
    MeioAberto --> Aberto: Chamada de teste falhou

    Fechado --> Fechado: Sucesso<br/>(reset contador)
    Fechado --> Fechado: Falha isolada<br/>(contador < 5)

    Aberto --> Aberto: Tentativa de chamada<br/>(fail-fast, não chama serviço)

    note right of Fechado
        Operação Normal
        Todas chamadas passam
        FailureCount = 0
    end note

    note right of Aberto
        Circuit Aberto
        Fail-fast por 60s
        Não chama serviço
    end note

    note right of MeioAberto
        Testando Recuperação
        1 chamada de teste
    end note
```

**Configuração:**
```csharp
CircuitBreakerPolicy:
  FailureThreshold: 5
  OpenDuration: 60 seconds
  HalfOpenTestCalls: 1
```

---

## Diagrama de Atividade: Autorização de Pagamento

```mermaid
flowchart TD
    Start([Início]) --> Input[/Operador preenche dados/]
    Input --> Val1{Tipo Pagamento<br/>válido?}
    Val1 -->|Não| Err1[Exibir erro:<br/>Tipo inválido]
    Err1 --> Input
    Val1 -->|Sim| Val2{Tipo Apólice<br/>válido?}
    Val2 -->|Não| Err2[Exibir erro:<br/>Apólice inválida]
    Err2 --> Input
    Val2 -->|Sim| Val3{Valor > 0?}
    Val3 -->|Não| Err3[Exibir erro:<br/>Valor inválido]
    Err3 --> Input
    Val3 -->|Sim| Val4{Beneficiário<br/>obrigatório?}
    Val4 -->|Sim e vazio| Err4[Exibir erro:<br/>Beneficiário obrigatório]
    Err4 --> Input
    Val4 -->|Não ou preenchido| LoadClaim[Carregar sinistro<br/>do banco]

    LoadClaim --> Val5{Valor <= Saldo<br/>Pendente?}
    Val5 -->|Não| Err5[Erro: Valor excede<br/>saldo pendente]
    Err5 --> End([Fim])

    Val5 -->|Sim| GetRate[Obter taxa BTNF<br/>para data de negócio]
    GetRate --> RateCheck{Taxa<br/>encontrada?}
    RateCheck -->|Não| Err6[Erro: Taxa não<br/>disponível]
    Err6 --> End

    RateCheck -->|Sim| Convert[Calcular valores BTNF:<br/>VALPRIBT, CRRMONBT, VALTOTBT]
    Convert --> RouteCheck{Produto<br/>Consórcio?}

    RouteCheck -->|Sim 6814/7701/7709| CallCNOUA[Validar CNOUA<br/>REST API]
    RouteCheck -->|Não| ContractCheck{NUM_CONTRATO<br/>> 0?}
    ContractCheck -->|Sim| CallSIPUA[Validar SIPUA<br/>SOAP]
    ContractCheck -->|Não| CallSIMDA[Validar SIMDA<br/>SOAP]

    CallCNOUA --> ValResult{EZERT8 =<br/>'00000000'?}
    CallSIPUA --> ValResult
    CallSIMDA --> ValResult

    ValResult -->|Não| Err7[Erro: Validação falhou<br/>Exibir EZERT8]
    Err7 --> End

    ValResult -->|Sim| BeginTx[BEGIN TRANSACTION]

    BeginTx --> Op1[INSERT THISTSIN]
    Op1 --> Op1Check{Sucesso?}
    Op1Check -->|Não| Rollback[ROLLBACK]

    Op1Check -->|Sim| Op2[UPDATE TMESTSIN<br/>TOTPAG += VALTOTBT<br/>OCORHIST += 1]
    Op2 --> Op2Check{1 row<br/>affected?}
    Op2Check -->|Não| Rollback

    Op2Check -->|Sim| Op3[INSERT SI_ACOMPANHA_SINI]
    Op3 --> Op3Check{Sucesso?}
    Op3Check -->|Não| Rollback

    Op3Check -->|Sim| Op4[Atualizar Fases<br/>Abrir/Fechar conforme<br/>SI_REL_FASE_EVENTO]
    Op4 --> Op4Check{Sucesso?}
    Op4Check -->|Não| Rollback

    Op4Check -->|Sim| Commit[COMMIT TRANSACTION]

    Commit --> Success[Exibir: PAGAMENTO<br/>AUTORIZADO COM SUCESSO]
    Success --> End

    Rollback --> Err8[Erro: Transação<br/>não concluída]
    Err8 --> End

    style Start fill:#e3f2fd
    style End fill:#e3f2fd
    style Success fill:#c8e6c9
    style Err1 fill:#ffcdd2
    style Err2 fill:#ffcdd2
    style Err3 fill:#ffcdd2
    style Err4 fill:#ffcdd2
    style Err5 fill:#ffcdd2
    style Err6 fill:#ffcdd2
    style Err7 fill:#ffcdd2
    style Err8 fill:#ffcdd2
    style Rollback fill:#ffcdd2
    style Commit fill:#c8e6c9
```

---

## Diagrama de Atividade: Busca de Sinistro (Swimlanes)

```mermaid
flowchart TB
    subgraph Operador
        A1[Acessar tela<br/>SI11M010]
        A2[Preencher critério<br/>de busca]
        A3[Pressionar ENTER]
        A8[Visualizar dados<br/>do sinistro]
        A9[Erro exibido]
    end

    subgraph Sistema
        B1{Critério<br/>completo?}
        B2[Determinar tipo<br/>de busca]
        B3[Query TMESTSIN<br/>por protocolo]
        B4[Query TMESTSIN<br/>por número]
        B5[Query TMESTSIN<br/>por líder]
        B6{Registro<br/>encontrado?}
        B7[Carregar dados<br/>complementares]
        B8[Calcular saldo<br/>pendente]
    end

    subgraph Banco_de_Dados
        C1[TMESTSIN]
        C2[TGERAMO]
        C3[TAPOLICE]
    end

    A1 --> A2
    A2 --> A3
    A3 --> B1
    B1 -->|Não| A9
    B1 -->|Sim| B2

    B2 -->|Protocolo| B3
    B2 -->|Número| B4
    B2 -->|Líder| B5

    B3 --> C1
    B4 --> C1
    B5 --> C1

    C1 --> B6
    B6 -->|Não| A9
    B6 -->|Sim| B7

    B7 --> C2
    C2 --> B7
    B7 --> C3
    C3 --> B8

    B8 --> A8
```

---

## Diagrama de Atividade: Conversão Monetária BTNF

```mermaid
flowchart TD
    Start([Início]) --> Input[/VALPRI, CRRMON/]

    Input --> GetDate[Obter DTMOVABE<br/>de TSISTEMA]
    GetDate --> QueryRate[Query TGEUNIMO<br/>WHERE DTINIVIG <= DTMOVABE<br/>AND DTMOVABE <= DTTERVIG]

    QueryRate --> RateCheck{Taxa<br/>encontrada?}
    RateCheck -->|Não| Error[Lançar exceção:<br/>CurrencyRateNotFoundException]
    Error --> End([Fim])

    RateCheck -->|Sim| ExtractRate[Extrair VLCRUZAD<br/>taxa = 1.23456789]

    ExtractRate --> CalcPrincipal[VALPRIBT =<br/>VALPRI × VLCRUZAD]
    CalcPrincipal --> CalcCorrection[CRRMONBT =<br/>CRRMON × VLCRUZAD]
    CalcCorrection --> CalcTotal[VALTOTBT =<br/>VALPRIBT + CRRMONBT]

    CalcTotal --> RoundPrincipal[Round VALPRIBT<br/>Banker's Rounding<br/>2 decimais]
    RoundPrincipal --> RoundCorrection[Round CRRMONBT<br/>Banker's Rounding<br/>2 decimais]
    RoundCorrection --> RoundTotal[Round VALTOTBT<br/>Banker's Rounding<br/>2 decimais]

    RoundTotal --> Return[/Retornar:<br/>ConvertedValues/]
    Return --> End

    style Start fill:#e3f2fd
    style End fill:#e3f2fd
    style Return fill:#c8e6c9
    style Error fill:#ffcdd2
```

**Exemplo de Cálculo:**
```
Entrada:
  VALPRI = 25000.00
  CRRMON = 500.00
  VLCRUZAD = 1.23456789

Cálculo:
  VALPRIBT = 25000.00 × 1.23456789 = 30864.19725
  Round(30864.19725, 2, MidpointRounding.ToEven) = 30864.20

  CRRMONBT = 500.00 × 1.23456789 = 617.283945
  Round(617.283945, 2, MidpointRounding.ToEven) = 617.28

  VALTOTBT = 30864.20 + 617.28 = 31481.48

Saída:
  ConvertedValues {
    PrincipalValueBTNF: 30864.20,
    CorrectionValueBTNF: 617.28,
    TotalValueBTNF: 31481.48
  }
```

---

## Diagrama de Atividade: Gestão de Fases com Loop

```mermaid
flowchart TD
    Start([Evento disparado<br/>ex: 1098]) --> Query[Query SI_REL_FASE_EVENTO<br/>WHERE COD_EVENTO = 1098]

    Query --> HasConfig{Configurações<br/>encontradas?}
    HasConfig -->|Não| Log1[LOG: Nenhuma fase<br/>configurada para evento]
    Log1 --> End([Fim])

    HasConfig -->|Sim| LoopStart{Para cada<br/>configuração}

    LoopStart -->|Mais| CheckType{IND_ALTERACAO_FASE<br/>= '1'?}

    CheckType -->|Sim - Abrir| CheckExists[Query: Fase já aberta?<br/>COUNT WHERE DATA_FECHA='9999-12-31']
    CheckExists --> AlreadyOpen{Count > 0?}
    AlreadyOpen -->|Sim| Log2[LOG: Fase já aberta<br/>skip idempotent]
    Log2 --> LoopStart
    AlreadyOpen -->|Não| InsertPhase[INSERT SI_SINISTRO_FASE<br/>DATA_FECHA='9999-12-31']
    InsertPhase --> Log3[LOG: Fase aberta]
    Log3 --> LoopStart

    CheckType -->|Não - Fechar IND='2'| UpdatePhase[UPDATE SI_SINISTRO_FASE<br/>SET DATA_FECHA=HOJE<br/>WHERE DATA_FECHA='9999-12-31']
    UpdatePhase --> RowsAffected{Rows<br/>affected?}
    RowsAffected -->|0| Log4[LOG: Fase já fechada<br/>skip idempotent]
    Log4 --> LoopStart
    RowsAffected -->|1| Log5[LOG: Fase fechada]
    Log5 --> LoopStart

    LoopStart -->|Concluído| Success[LOG: Fases atualizadas<br/>com sucesso]
    Success --> End

    style Start fill:#e3f2fd
    style End fill:#e3f2fd
    style Success fill:#c8e6c9
```

---

## Diagrama de Atividade: Retry Policy com Backoff

```mermaid
flowchart TD
    Start([Chamada de serviço<br/>externo]) --> Attempt1[Tentativa 1:<br/>POST /validate]

    Attempt1 --> Check1{Resposta<br/>recebida?}
    Check1 -->|Sim| Success[Retornar resultado]
    Success --> End([Fim])

    Check1 -->|Não - Timeout| Log1[LOG: Timeout tentativa 1]
    Log1 --> Wait1[Aguardar 1 segundo<br/>backoff exponencial]
    Wait1 --> Attempt2[Tentativa 2:<br/>POST /validate]

    Attempt2 --> Check2{Resposta<br/>recebida?}
    Check2 -->|Sim| Success

    Check2 -->|Não - Timeout| Log2[LOG: Timeout tentativa 2]
    Log2 --> Wait2[Aguardar 2 segundos<br/>backoff exponencial]
    Wait2 --> Attempt3[Tentativa 3:<br/>POST /validate]

    Attempt3 --> Check3{Resposta<br/>recebida?}
    Check3 -->|Sim| Success

    Check3 -->|Não - Timeout| Log3[LOG: Timeout tentativa 3<br/>FINAL]
    Log3 --> CBRegister[Registrar falha<br/>no Circuit Breaker]
    CBRegister --> Error[Lançar<br/>TimeoutException]
    Error --> End

    style Start fill:#e3f2fd
    style End fill:#e3f2fd
    style Success fill:#c8e6c9
    style Error fill:#ffcdd2
```

**Configuração:**
```csharp
RetryPolicy:
  MaxRetries: 2 (total 3 tentativas)
  BackoffIntervals: [1s, 2s]
  Timeout per attempt: 10s
  Total max time: 32s (10s + 1s + 10s + 2s + 10s)
```

---

## Diagrama de Atividade: Transação ACID (4 Operações)

```mermaid
flowchart TD
    Start([Início Transação]) --> Begin[BEGIN TRANSACTION]

    Begin --> Op1[Operação 1:<br/>INSERT THISTSIN]
    Op1 --> C1{Sucesso?}
    C1 -->|Não| Rollback[ROLLBACK<br/>Desfazer todas mudanças]

    C1 -->|Sim| Op2[Operação 2:<br/>UPDATE TMESTSIN]
    Op2 --> C2{Sucesso?}
    C2 -->|Não| Rollback

    C2 -->|Sim| Op3[Operação 3:<br/>INSERT SI_ACOMPANHA_SINI]
    Op3 --> C3{Sucesso?}
    C3 -->|Não| Rollback

    C3 -->|Sim| Op4[Operação 4:<br/>Atualizar Fases]
    Op4 --> C4{Sucesso?}
    C4 -->|Não| Rollback

    C4 -->|Sim| Commit[COMMIT<br/>Persistir todas mudanças]

    Commit --> LogSuccess[LOG: Transação<br/>bem-sucedida]
    LogSuccess --> End([Fim - Sucesso])

    Rollback --> LogError[LOG: Transação<br/>revertida]
    LogError --> EndError([Fim - Erro])

    style Start fill:#e3f2fd
    style End fill:#c8e6c9
    style EndError fill:#ffcdd2
    style Commit fill:#c8e6c9
    style Rollback fill:#ffcdd2
```

**Garantias ACID:**

| Propriedade | Como é garantida |
|-------------|------------------|
| **Atomicidade** | Se qualquer operação falhar (C1, C2, C3, C4), ROLLBACK desfaz todas |
| **Consistência** | Constraints do banco validados antes do COMMIT |
| **Isolamento** | READ COMMITTED - outras transações não veem mudanças não-comitadas |
| **Durabilidade** | Após COMMIT, mudanças sobrevivem a falhas do sistema (WAL logging) |

---

## Diagrama de Atividade: Dashboard Refresh Loop

```mermaid
flowchart TD
    Start([Dashboard carregado]) --> Initial[Carregar dados iniciais]

    Initial --> Display[Renderizar dashboard]
    Display --> Wait[Aguardar 30 segundos]

    Wait --> Refresh{Página ainda<br/>ativa?}
    Refresh -->|Não| End([Fim])

    Refresh -->|Sim| Fetch[Fetch /api/dashboard/status]
    Fetch --> Success{Status<br/>200 OK?}

    Success -->|Não| LogError[LOG: Erro ao atualizar]
    LogError --> Wait

    Success -->|Sim| Parse[Parse JSON response]
    Parse --> Compare{Dados<br/>mudaram?}

    Compare -->|Não| Wait
    Compare -->|Sim| Update[Atualizar componentes:<br/>- Progress bar<br/>- User stories<br/>- Gráficos<br/>- Timeline]
    Update --> Animate[Animar transições]
    Animate --> Wait

    style Start fill:#e3f2fd
    style End fill:#e3f2fd
    style Update fill:#c8e6c9
```

---

## Resumo de Diagramas Criados

### Diagramas de Estado (4)
1. ✅ **Ciclo de Vida do Sinistro** - 9 estados principais
2. ✅ **Fase do Sinistro** - 3 estados (Não Iniciada → Aberta → Fechada)
3. ✅ **Circuit Breaker** - 3 estados (Fechado ↔ Aberto ↔ Meio-Aberto)
4. Estados compostos (EmProcesso, Autorizado) no diagrama 1

### Diagramas de Atividade (7)
1. ✅ **Autorização de Pagamento** - Fluxo completo com validações
2. ✅ **Busca de Sinistro** - Com swimlanes (Operador, Sistema, BD)
3. ✅ **Conversão Monetária BTNF** - Cálculos e arredondamento
4. ✅ **Gestão de Fases** - Loop com abertura/fechamento
5. ✅ **Retry Policy** - Backoff exponencial
6. ✅ **Transação ACID** - 4 operações com rollback
7. ✅ **Dashboard Refresh** - Loop de atualização

---

## Convenções de Notação

### Formas
- **Retângulo arredondado:** Estado
- **Losango:** Decisão
- **Retângulo:** Atividade/Processo
- **Paralelogramo:** Entrada/Saída de dados
- **Elipse:** Início/Fim

### Cores (Mermaid)
- 🟦 **Azul (#e3f2fd):** Início/Fim
- 🟩 **Verde (#c8e6c9):** Sucesso/Commit
- 🟥 **Vermelho (#ffcdd2):** Erro/Rollback

### Setas
- **Linha sólida (→):** Fluxo normal
- **Linha tracejada (-.->):** Fluxo alternativo
- **Seta dupla (↔):** Transição bidirecional

---

**FIM DO DOCUMENTO - DIAGRAMAS DE ESTADO E ATIVIDADE**
