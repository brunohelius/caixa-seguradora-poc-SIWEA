# 07 - Guia de Operações: Sistema Legado SIWEA

[← Voltar ao Índice](README.md) | [→ Próximo: Telas e Interface](08-ui-screens.md)

---

## Procedimentos Operacionais

### Inicialização do Sistema

#### Startup Sequence

```
1. Verificar DB2 está online
2. Verificar CICS region está ativo
3. Verificar conectividade MQ Series
4. Carregar programas COBOL
5. Inicializar pools de conexão
6. Validar integrações externas
7. Liberar transações para usuários
```

#### Comandos CICS

```
CEMT SET PROGRAM(SIWEA116) ENABLED
CEMT SET TRANSACTION(SI10) ENABLED
CEMT SET TRANSACTION(SI20) ENABLED
CEMT INQ TRANSACTION(SI*) 
```

### Shutdown Procedures

```
1. Bloquear novas transações
2. Aguardar transações em andamento
3. Fazer checkpoint do banco
4. Desconectar integrações
5. Shutdown CICS region
6. Backup incremental
```

---

## Monitoramento

### Indicadores Críticos

| Indicador | Normal | Warning | Critical |
|-----------|--------|---------|----------|
| CPU Usage | < 60% | 60-80% | > 80% |
| Memory Usage | < 70% | 70-85% | > 85% |
| Response Time | < 3s | 3-5s | > 5s |
| DB Connections | < 150 | 150-180 | > 180 |
| Queue Depth | < 100 | 100-500 | > 500 |
| Error Rate | < 1% | 1-3% | > 3% |

### Comandos de Monitoramento

```bash
# Verificar status CICS
CEMT INQ SYSTEM

# Verificar transações ativas
CEMT INQ TASK

# Verificar DB2 connections
-DIS DDF DETAIL

# Verificar MQ queues
echo "DISPLAY QSTATUS(SIMDA.VALIDATION.REQ) CURDEPTH" | runmqsc QM.SIMDA.PRD
```

---

## Troubleshooting

### Problemas Comuns

#### 1. Timeout em Busca

**Sintomas:**
- Busca demora mais de 10 segundos
- Timeout error na tela

**Diagnóstico:**
```sql
-- Verificar locks
SELECT * FROM SYSIBM.SYSLOCKS WHERE TABLENAME = 'TMESTSIN';

-- Verificar índices
RUNSTATS TABLESPACE DBSIWEA.TSTMEST INDEX ALL;
```

**Solução:**
1. Verificar e resolver locks
2. Reorganizar índices
3. Atualizar estatísticas

#### 2. Falha na Integração

**Sintomas:**
- Erro E0010 na autorização
- Validação externa não responde

**Diagnóstico:**
```bash
# Test connectivity
ping cnoua.caixaseguradora.com.br
telnet sipua.caixaseguradora.com.br 443
```

**Solução:**
1. Verificar conectividade
2. Verificar certificados SSL
3. Ativar modo contingência se necessário

#### 3. Erro de Concorrência

**Sintomas:**
- "Record locked by another user"
- Deadlock detected

**Diagnóstico:**
```sql
-- Identificar locks
SELECT AGENT_ID, LOCK_OBJECT_NAME, LOCK_MODE
FROM TABLE(SNAP_GET_LOCK('', -1)) AS T
WHERE LOCK_OBJECT_TYPE = 'TABLE';
```

**Solução:**
1. Identificar transação bloqueadora
2. Fazer rollback se necessário
3. Implementar retry logic

---

## Backup e Recovery

### Estratégia de Backup

| Tipo | Frequência | Retenção | Janela |
|------|-----------|----------|--------|
| Full | Semanal (Dom) | 90 dias | 00:00-06:00 |
| Incremental | Diário | 30 dias | 02:00-04:00 |
| Log | 4 horas | 7 dias | Contínuo |
| Archive | Mensal | 7 anos | 1º domingo |

### Procedimento de Restore

```bash
# 1. Stop application
CEMT SET TRANSACTION(SI*) DISABLED

# 2. Restore database
db2 restore database DBSIWEA from /backup/full taken at 20251027

# 3. Roll forward logs
db2 rollforward database DBSIWEA to end of logs

# 4. Verify integrity
db2 connect to DBSIWEA
db2 "SELECT COUNT(*) FROM TMESTSIN"

# 5. Restart application
CEMT SET TRANSACTION(SI*) ENABLED
```

---

## Manutenção Programada

### Tarefas Diárias

- [ ] Verificar logs de erro
- [ ] Monitorar performance
- [ ] Verificar backup noturno
- [ ] Revisar alertas

### Tarefas Semanais

- [ ] Reorganizar índices
- [ ] Atualizar estatísticas
- [ ] Limpar logs antigos
- [ ] Revisar capacity planning

### Tarefas Mensais

- [ ] Teste de restore
- [ ] Análise de performance
- [ ] Atualização de documentação
- [ ] Simulação de DR

---

## Segurança Operacional

### Checklist de Segurança

- [ ] Patches de segurança aplicados
- [ ] Certificados SSL válidos
- [ ] Senhas dentro da política
- [ ] Logs de auditoria ativos
- [ ] Backups criptografados
- [ ] Acesso restrito a produção

### Resposta a Incidentes

```
1. DETECÇÃO
   - Alerta automático
   - Relato de usuário
   
2. CONTENÇÃO
   - Isolar sistema afetado
   - Bloquear acesso suspeito
   
3. INVESTIGAÇÃO
   - Coletar logs
   - Analisar root cause
   
4. REMEDIAÇÃO
   - Aplicar correção
   - Testar solução
   
5. DOCUMENTAÇÃO
   - Registrar incidente
   - Atualizar procedimentos
```

---

*Este documento contém os procedimentos operacionais do sistema SIWEA.*

**Última Atualização:** 27/10/2025
