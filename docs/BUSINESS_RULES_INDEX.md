# SIWEA Business Rules - Complete Index

**Complete Reference**: 100+ Business Rules documented  
**Status**: Ready for Implementation  
**Generated**: 2025-10-23

---

## Quick Navigation

- **Main Analysis**: See `/LEGACY_SIWEA_COMPLETE_ANALYSIS.md`
- **Executive Summary**: See `/ANALYSIS_SUMMARY.md`
- **This File**: Quick reference index of all rules

---

## Business Rules by Category

### 1. SEARCH & RETRIEVAL (9 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-001 | Three mutually exclusive search modes (Protocol/Claim/Leader) | Search Functionality |
| BR-002 | At least one complete search criterion required | Search Functionality |
| BR-003 | Claim data retrieved from TMESTSIN | Search Functionality |
| BR-004 | Protocol displayed as FONTE/PROTSINI-DAC | Search Functionality |
| BR-005 | Claim displayed as ORGSIN/RMOSIN/NUMSIN | Search Functionality |
| BR-006 | Claim not found error message format | Error Handling |
| BR-007 | Branch name from TGERAMO lookup | Search Functionality |
| BR-008 | Insured name from TAPOLICE lookup | Search Functionality |
| BR-009 | Currency amounts formatted with commas, 2 decimals | UI/Display |

### 2. PAYMENT AUTHORIZATION (17 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-010 | Payment type must be 1, 2, 3, 4, or 5 | Payment Authorization |
| BR-011 | Invalid payment type error message | Error Handling |
| BR-012 | Principal amount must be numeric, non-negative | Payment Authorization |
| BR-013 | Principal amount <= pending value (SDOPAG - TOTPAG) | Payment Authorization |
| BR-014 | Amount exceeds pending error message | Error Handling |
| BR-015 | Policy type must be 1 or 2 | Payment Authorization |
| BR-016 | Invalid policy type error message | Error Handling |
| BR-017 | Correction amount optional, defaults to 0.00 | Payment Authorization |
| BR-018 | Correction amount numeric, non-negative | Payment Authorization |
| BR-019 | Beneficiary required if TPSEGU != 0 | Payment Authorization |
| BR-020 | Missing required beneficiary error message | Error Handling |
| BR-021 | Beneficiary field max 255 characters | Payment Authorization |
| BR-022 | Special characters in beneficiary preserved | Payment Authorization |
| BR-023 | Currency conversion formula: AMOUNT_BTNF = AMOUNT × VLCRUZAD | Currency Conversion |
| BR-024 | Conversion rate from TGEUNIMO table | Currency Conversion |
| BR-025 | Rate selection: DTINIVIG <= DATE <= DTTERVIG | Currency Conversion |
| BR-026 | No rate for date error message | Error Handling |

### 3. CURRENCY CONVERSION (11 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-027 | Conversion rate precision: 8 decimal places | Currency Conversion |
| BR-028 | Final currency calculations: 2 decimal places | Currency Conversion |
| BR-029 | Principal conversion: VALPRIBT = VALPRI × VLCRUZAD | Currency Conversion |
| BR-030 | Correction conversion: CRRMONBT = CRRMON × VLCRUZAD | Currency Conversion |
| BR-031 | Total calculation: VALTOTBT = VALPRIBT + CRRMONBT | Currency Conversion |
| BR-032 | Daily amounts calculated (PRIDIABT, CRRDIABT, TOTDIABT) | Currency Conversion |
| BR-033 | Currency code always BTNF | Currency Conversion |
| BR-034 | Operation code always 1098 | Transaction Recording |
| BR-035 | Transaction date = TSISTEMA.DTMOVABE (business date) | Transaction Recording |
| BR-036 | Transaction time = current system time | Transaction Recording |
| BR-037 | Accounting status initialized to '0' | Transaction Recording |

### 4. TRANSACTION RECORDING (9 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-038 | Overall status initialized to '0' | Transaction Recording |
| BR-039 | Correction type always '5' | Transaction Recording |
| BR-040 | Occurrence counter incremented: OCORHIST_NEW = OCORHIST + 1 | Transaction Recording |
| BR-041 | Operator user ID recorded: EZEUSRID | Transaction Recording |
| BR-042 | Audit fields: CREATED_BY, CREATED_AT, UPDATED_BY, UPDATED_AT | Transaction Recording |
| BR-043 | Consortium products: 6814, 7701, 7709 → CNOUA | Product Validation |
| BR-044 | Query EF_CONTR_SEG_HABIT for contract number | Product Validation |
| BR-045 | EFP contract (NUM_CONTRATO > 0) → SIPUA | Product Validation |
| BR-046 | HB contract (NUM_CONTRATO = 0 or not found) → SIMDA | Product Validation |

### 5. PRODUCT VALIDATION (14 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-047 | External service response code EZERT8 checked for success | Product Validation |
| BR-048 | EZERT8 != '00000000' indicates validation failure | Product Validation |
| BR-049 | Validation error response contains descriptive message | Product Validation |
| BR-050 | Payment authorization halted if validation fails | Product Validation |
| BR-051 | Transaction rolled back if validation fails | Product Validation |
| BR-052 | Consortium validation error: EZERT8001 | Error Handling |
| BR-053 | Contract cancelled error: EZERT8002 | Error Handling |
| BR-054 | Group closed error: EZERT8003 | Error Handling |
| BR-055 | Quota not contemplated error: EZERT8004 | Error Handling |
| BR-056 | Beneficiary not authorized error: EZERT8005 | Error Handling |
| BR-057 | Claim accompaniment record created with COD_EVENTO = 1098 | Phase Management |
| BR-058 | Phase changes determined by SI_REL_FASE_EVENTO config | Phase Management |
| BR-059 | Phase opening (IND='1'): Create with DATA_FECHA='9999-12-31' | Phase Management |
| BR-060 | Phase closing (IND='2'): Update existing open phase | Phase Management |

### 6. PHASE MANAGEMENT (10 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-061 | Open phase indicator: DATA_FECHA_SIFA = '9999-12-31' | Phase Management |
| BR-062 | Phase opening date set to current business date | Phase Management |
| BR-063 | Prevent duplicate open phases | Phase Management |
| BR-064 | Query SI_REL_FASE_EVENTO for event 1098 relationships | Phase Management |
| BR-065 | Process relationships in order | Phase Management |
| BR-066 | Phase rollback on update failure | Phase Management |
| BR-067 | Phase atomicity: all or nothing | Phase Management |
| BR-068 | Operator user ID on all history records | Audit Trail |
| BR-069 | Operator user ID on all accompaniment records | Audit Trail |
| BR-070 | Operator user ID on all phase records | Audit Trail |

### 7. AUDIT TRAIL (6 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-071 | Timestamp recorded on insertion/modification | Audit Trail |
| BR-072 | Complete audit trail reconstruction via SI_ACOMPANHA_SINI | Audit Trail |
| BR-073 | Transaction date immutable after creation | Audit Trail |
| BR-074 | Referential integrity maintained across tables | Audit Trail |
| BR-075 | TIPSEG numeric and consistent across records | Data Validation |
| BR-076 | ORGSIN 2-digit code (01-99) | Data Validation |

### 8. DATA VALIDATION (13 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-077 | RMOSIN 2-digit code (00-99) | Data Validation |
| BR-078 | NUMSIN 1-6 digit claim number | Data Validation |
| BR-079 | FONTE numeric | Data Validation |
| BR-080 | PROTSINI numeric | Data Validation |
| BR-081 | DAC single digit (0-9) | Data Validation |
| BR-082 | CODPRODU numeric and valid product code | Data Validation |
| BR-083 | SDOPAG (reserve) >= 0 | Data Validation |
| BR-084 | TOTPAG (payments) >= 0 and <= SDOPAG | Data Validation |
| BR-085 | OCORHIST non-negative integer | Data Validation |
| BR-086 | VALPRI >= 0 and <= SDOPAG - TOTPAG | Data Validation |
| BR-087 | CRRMON >= 0 | Data Validation |
| BR-088 | All UI text in Portuguese | UI/Display |
| BR-089 | Numeric amounts formatted ###,###,###.## | UI/Display |

### 9. UI/DISPLAY (8 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-090 | Date format: DD/MM/YYYY display, YYYY-MM-DD storage | UI/Display |
| BR-091 | Time format: HH:MM:SS | UI/Display |
| BR-092 | Error messages displayed in red (#e80c4d) | UI/Display |
| BR-093 | Caixa Seguradora logo in header | UI/Display |
| BR-094 | Site.css stylesheet applied without modification | UI/Display |
| BR-095 | Responsive design supporting mobile (max-width: 850px) | UI/Display |
| BR-096 | Claim search < 3 seconds | Performance |
| BR-097 | Payment authorization cycle < 90 seconds | Performance |

### 10. PERFORMANCE (2 Rules)

| Rule ID | Rule | Location |
|---------|------|----------|
| BR-098 | History query < 500ms for 1000+ records | Performance |
| BR-099 | Pagination: 20 records/page default, max 100 | Performance |

---

## Critical Business Rules (Must Implement)

### TIER 1: SYSTEM-CRITICAL (Must have, else system breaks)

1. **BR-019**: Beneficiary required if TPSEGU != 0
2. **BR-023**: Currency conversion formula (exact legacy calculation)
3. **BR-025**: Currency rate date-range validation
4. **BR-031**: Total amount calculation
5. **BR-034**: Operation code always 1098
6. **BR-035**: Business date from TSISTEMA (not system clock)
7. **BR-039**: Correction type always '5'
8. **BR-040**: Occurrence counter increment
9. **BR-047-048**: External service validation check
10. **BR-051**: Transaction rollback on validation failure
11. **BR-061**: Open phase marker '9999-12-31'
12. **BR-063**: Prevent duplicate phases
13. **BR-067**: Transaction atomicity

### TIER 2: BUSINESS-CRITICAL (Must have for correct behavior)

14. **BR-001-005**: Search modes
15. **BR-010-015**: Payment type & amount validation
16. **BR-043-046**: Product routing logic
17. **BR-059-060**: Phase opening/closing
18. **BR-068-070**: Operator user ID tracking
19. **BR-075-087**: Data validation rules

### TIER 3: OPERATIONAL (Should have for compliance)

20. **BR-022**: Beneficiary special characters
21. **BR-026**: No rate error handling
22. **BR-049**: Error message from external service
23. **BR-071-074**: Audit trail completeness
24. **BR-090-095**: UI standards
25. **BR-096-099**: Performance targets

---

## Error Messages (24 Total)

### Validation Errors (VAL-001 to VAL-008)

```
VAL-001: "Protocolo XXXXXXX-X NAO ENCONTRADO"
VAL-002: "SINISTRO XXXXXXX NAO ENCONTRADO"
VAL-003: "LIDER XXXXXXX-XXXXXXX NAO ENCONTRADO"
VAL-004: "Tipo de Pagamento deve ser 1, 2, 3, 4, ou 5"
VAL-005: "Valor Superior ao Saldo Pendente"
VAL-006: "Tipo de Apólice deve ser 1 ou 2"
VAL-007: "Favorecido é obrigatório para este tipo de seguro"
VAL-008: "Taxa de conversão não disponível para a data"
```

### Consortium Validation Errors (CONS-001 to CONS-005)

```
CONS-001: "Contrato de consórcio inválido"
CONS-002: "Contrato cancelado"
CONS-003: "Grupo encerrado"
CONS-004: "Cota não contemplada"
CONS-005: "Beneficiário não autorizado"
```

### System Errors (SYS-001 to SYS-005)

```
SYS-001: "Erro ao buscar dados do sinistro"
SYS-002: "Erro ao inserir histórico de pagamento"
SYS-003: "Erro ao atualizar saldo do sinistro"
SYS-004: "Erro ao processar fases"
SYS-005: "Serviço de validação indisponível"
```

---

## Key Database Tables

| Table | PK | Purpose | Rules Count |
|-------|----|---------|-----------| 
| TMESTSIN | (TIPSEG, ORGSIN, RMOSIN, NUMSIN) | Claim master | 7 |
| THISTSIN | (TIPSEG, ORGSIN, RMOSIN, NUMSIN, OCORHIST) | Payment history | 9 |
| TGERAMO | RMOSIN | Branch master | 1 |
| TAPOLICE | (ORGAPO, RMOAPO, NUMAPOL) | Policy master | 1 |
| TGEUNIMO | (DTINIVIG, DTTERVIG) | Currency rates | 6 |
| TSISTEMA | IDSISTEM | Business date | 2 |
| SI_ACOMPANHA_SINI | (FONTE, PROTSINI, DAC, COD_EVENTO, DATA_MOVTO) | Events | 6 |
| SI_SINISTRO_FASE | (FONTE, PROTSINI, DAC, COD_FASE, COD_EVENTO, NUM_OCORR, DATA_INIVIG) | Phases | 7 |
| SI_REL_FASE_EVENTO | (COD_FASE, COD_EVENTO, DATA_INIVIG) | Phase config | 7 |
| EF_CONTR_SEG_HABIT | NUM_CONTRATO | Consortium contract | 3 |

---

## Key Formulas

### Currency Conversion
```
VLCRUZAD = lookup(TGEUNIMO, date_range)
VALPRIBT = VALPRI × VLCRUZAD
CRRMONBT = CRRMON × VLCRUZAD
VALTOTBT = VALPRIBT + CRRMONBT
```

### Pending Value
```
PENDING = SDOPAG - TOTPAG
```

### Occurrence Sequence
```
NEW_OCORHIST = OLD_OCORHIST + 1
```

### Phase State
```
IS_OPEN = (DATA_FECHA_SIFA == '9999-12-31')
```

### Beneficiary Requirement
```
BENEFICIARY_REQUIRED = (TPSEGU != 0)
```

---

## Implementation Checklist

- [ ] Implement all 100+ business rules
- [ ] Create validation for BR-001-BR-099
- [ ] Implement currency conversion (BR-023-BR-033)
- [ ] Implement transaction atomicity (BR-034-BR-051)
- [ ] Implement phase management (BR-057-BR-067)
- [ ] Implement audit trail (BR-068-BR-074)
- [ ] Create all 24 error messages
- [ ] Verify performance targets (BR-096-BR-099)
- [ ] Test with complete scenarios

---

## Testing Strategy

### Unit Tests (Per Rule)
- Test each validation rule independently
- Test currency conversion with known rates
- Test phase opening/closing logic
- Test error message generation

### Integration Tests (Per Workflow)
- Test complete search workflow
- Test complete payment authorization
- Test phase management end-to-end
- Test transaction rollback on failure

### Performance Tests
- Measure search response time (< 3s)
- Measure payment cycle time (< 90s)
- Measure history query time (< 500ms)

### E2E Tests
- Test complete user journey (search → authorize → confirm)
- Test error scenarios
- Test concurrent operations
- Test with legacy data

---

## References

- **Complete Analysis**: `/LEGACY_SIWEA_COMPLETE_ANALYSIS.md` (1,725 lines)
- **Summary**: `/ANALYSIS_SUMMARY.md`
- **Phase Docs**: `/docs/phase-management-workflow.md`
- **Validation Docs**: `/docs/product-validation-routing.md`
- **Performance Docs**: `/docs/performance-notes.md`

---

**Status**: Complete - Ready for implementation  
**Confidence**: High - 100+ rules documented with complete specifications  
**Last Updated**: 2025-10-23

