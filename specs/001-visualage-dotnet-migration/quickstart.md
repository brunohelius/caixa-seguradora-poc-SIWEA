# Quickstart Guide - Visual Age Claims System Migration
## Guia Rápido de Onboarding para Desenvolvedores

**Objetivo**: Configurar seu ambiente de desenvolvimento e executar a aplicação completa em menos de 30 minutos.

**Versão**: 1.0
**Última Atualização**: 2025-10-23
**Projeto**: Migração do Sistema SIWEA (Visual Age) para .NET 9 + React

---

## 📋 Índice

1. [Pré-requisitos](#pré-requisitos)
2. [Visão Geral do Projeto](#visão-geral-do-projeto)
3. [Configuração Inicial](#configuração-inicial)
4. [Backend (.NET 9)](#backend-net-9)
5. [Frontend (React 18)](#frontend-react-18)
6. [Banco de Dados](#banco-de-dados)
7. [Docker - Stack Completo](#docker---stack-completo)
8. [Executando Testes](#executando-testes)
9. [Endpoints e Acesso](#endpoints-e-acesso)
10. [Estrutura do Projeto](#estrutura-do-projeto)
11. [Troubleshooting](#troubleshooting)
12. [Próximos Passos](#próximos-passos)

---

## Pré-requisitos

### Ferramentas Obrigatórias

#### 1. .NET 9 SDK
**Download**: https://dotnet.microsoft.com/download/dotnet/9.0

Verificar instalação:
```bash
dotnet --version
# Deve retornar: 9.0.x ou superior
```

#### 2. Node.js 18+ e npm
**Download**: https://nodejs.org/ (recomenda-se a versão LTS)

Verificar instalação:
```bash
node --version
# Deve retornar: v18.x.x ou superior

npm --version
# Deve retornar: 9.x.x ou superior
```

#### 3. Git
**Download**: https://git-scm.com/downloads

Verificar instalação:
```bash
git --version
# Deve retornar: git version 2.x.x ou superior
```

### Ferramentas Recomendadas

#### 4. Docker Desktop (Opcional mas recomendado)
**Download**: https://www.docker.com/products/docker-desktop

Para desenvolvimento full-stack simplificado e banco de dados local.

Verificar instalação:
```bash
docker --version
# Deve retornar: Docker version 24.x.x ou superior

docker-compose --version
# Deve retornar: Docker Compose version 2.x.x ou superior
```

#### 5. IDE/Editor

**Opção 1 - Visual Studio Code** (Recomendado para full-stack)
- Download: https://code.visualstudio.com/
- Extensões recomendadas:
  - C# Dev Kit (Microsoft)
  - REST Client
  - Docker
  - ESLint
  - Prettier
  - GitLens

**Opção 2 - Visual Studio 2022** (Recomendado para backend)
- Download: https://visualstudio.microsoft.com/
- Workloads: ASP.NET and web development

**Opção 3 - JetBrains Rider** (Alternativa premium)
- Download: https://www.jetbrains.com/rider/

### Requisitos de Sistema

- **SO**: Windows 10/11, macOS 12+, ou Linux (Ubuntu 20.04+)
- **RAM**: Mínimo 8GB (recomendado 16GB)
- **Disco**: 5GB de espaço livre
- **Processador**: Dual-core 2GHz ou superior

---

## Visão Geral do Projeto

### O que é este projeto?

Migração completa do sistema legado de autorização de pagamento de sinistros (SIWEA) da IBM VisualAge EZEE para uma arquitetura moderna:

- **Backend**: .NET 9 Web API (REST + SOAP)
- **Frontend**: React 18 com TypeScript
- **Database**: DB2 ou SQL Server (sem alterações de schema)
- **Arquitetura**: Clean Architecture (API → Core → Infrastructure)

### Funcionalidades Principais

1. **Busca de Sinistros**: Pesquisa por número de protocolo, número de sinistro ou código líder
2. **Autorização de Pagamento**: Registro de autorizações de indenização com validações
3. **Histórico de Pagamentos**: Visualização completa de autorizações anteriores
4. **Produtos Especiais**: Validação para consórcio (códigos 6814, 7701, 7709)
5. **Gestão de Fases**: Controle de workflow de processamento
6. **Dashboard de Migração**: Acompanhamento do progresso do projeto

### Repositórios

```
POC Visual Age/                    # Projeto legado e specs
├── #SIWEA-V116.esf               # Código fonte Visual Age (legacy)
├── Site.css                      # Stylesheet a ser migrado
└── specs/
    └── 001-visualage-dotnet-migration/
        ├── spec.md               # Especificação completa
        ├── plan.md               # Plano de implementação
        ├── data-model.md         # Modelo de dados
        └── quickstart.md         # Este arquivo

CaixaSeguradora.MVP/              # Projeto .NET existente (referência)
├── src/
│   ├── CaixaSeguradora.Api/
│   ├── CaixaSeguradora.Core/
│   └── CaixaSeguradora.Infrastructure/
└── tests/
```

---

## Configuração Inicial

### 1. Clone o Repositório

```bash
# Navegue até seu diretório de projetos
cd ~/Development

# Clone o repositório principal (ajuste a URL conforme seu repositório)
git clone <repository-url> "Caixa Seguradora"

cd "Caixa Seguradora/POC Visual Age"
```

### 2. Checkout da Branch de Desenvolvimento

```bash
# Verifique a branch atual
git branch

# Se necessário, crie/mude para a branch da feature
git checkout -b 001-visualage-dotnet-migration

# Ou faça pull da branch existente
git pull origin 001-visualage-dotnet-migration
```

### 3. Estrutura de Diretórios

Após o clone, você deve ter esta estrutura:

```
Development/
└── Caixa Seguradora/
    ├── POC Visual Age/           # Legado e especificações
    └── CaixaSeguradora.MVP/      # Aplicação .NET
```

---

## Backend (.NET 9)

### 1. Navegue para o Projeto Backend

```bash
cd "/Users/<seu-usuario>/Development/Caixa Seguradora/CaixaSeguradora.MVP"
```

### 2. Restaure as Dependências

```bash
# Restaura pacotes NuGet para todos os projetos
dotnet restore

# Ou restaure especificamente
dotnet restore CaixaSeguradora.MVP.sln
```

### 3. Configure o appsettings.json

O arquivo já existe em: `src/CaixaSeguradora.Api/appsettings.json`

**Para desenvolvimento local**, crie `appsettings.Development.json`:

```bash
cd src/CaixaSeguradora.Api
```

Crie o arquivo `appsettings.Development.json` com:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ServiceSettings": {
    "Environment": "Development",
    "UseMockData": true,
    "TimeoutSeconds": 30,
    "CacheExpirationMinutes": 15
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CaixaSeguradoraMVP_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "SalesforceSettings": {
    "UseMockData": true
  },
  "IntegrationSettings": {
    "GEWSV0012": {
      "UseMockData": true
    },
    "Ouvidoria": {
      "UseMockData": true
    }
  }
}
```

**Notas**:
- `UseMockData: true` permite desenvolvimento sem dependências externas
- A connection string usa LocalDB (SQL Server Express)
- Para DB2, use: `"Server=localhost;Database=CAIXADB;User Id=db2admin;Password=yourpassword"`

### 4. Build do Projeto

```bash
# Build em modo Debug
dotnet build

# Ou build em modo Release
dotnet build --configuration Release
```

**Esperado**: Build bem-sucedido sem erros.

### 5. Execute o Backend

**Opção A - Linha de Comando**:
```bash
cd src/CaixaSeguradora.Api
dotnet run --urls "http://localhost:5000;https://localhost:5001"
```

**Opção B - Script Automatizado**:
```bash
# Na raiz do projeto CaixaSeguradora.MVP
chmod +x run.sh
./run.sh
```

**Opção C - Visual Studio**:
- Abra `CaixaSeguradora.MVP.sln`
- Pressione F5 ou clique em "Run"

**Opção D - VS Code**:
- Abra a pasta do projeto
- Pressione F5 (ou Run → Start Debugging)

### 6. Verifique se o Backend está Rodando

Abra o navegador em: **https://localhost:5001/swagger**

Você deve ver a UI do Swagger com todos os endpoints da API.

**Teste rápido via curl**:
```bash
curl -k https://localhost:5001/api/health
# Esperado: {"status":"Healthy"}
```

### 7. Certificado SSL (Dev)

Se receber avisos de certificado SSL:

**Windows**:
```bash
dotnet dev-certs https --trust
```

**macOS/Linux**:
```bash
dotnet dev-certs https --trust
# Ou aceite o certificado no navegador
```

---

## Frontend (React 18)

**Nota**: A implementação do frontend React está planejada mas ainda não existe no repositório atual. Esta seção descreve a configuração futura.

### 1. Estrutura do Frontend (Planejada)

```
frontend/
├── package.json
├── tsconfig.json
├── public/
│   ├── index.html
│   └── Site.css              # CSS migrado do Visual Age
├── src/
│   ├── App.tsx
│   ├── index.tsx
│   ├── components/           # Componentes reutilizáveis
│   ├── pages/                # Páginas principais
│   ├── services/             # Chamadas à API
│   └── models/               # Interfaces TypeScript
└── tests/
```

### 2. Inicialização do Projeto React (Quando Criado)

```bash
cd "/Users/<seu-usuario>/Development/Caixa Seguradora/POC Visual Age/frontend"

# Instale as dependências
npm install

# Ou use yarn
yarn install
```

### 3. Configuração de Ambiente

Crie `.env.local`:

```bash
REACT_APP_API_BASE_URL=https://localhost:5001
REACT_APP_API_TIMEOUT=30000
REACT_APP_MOCK_DATA=true
```

### 4. Execute o Frontend (Dev Server)

```bash
npm start
# Ou: yarn start

# Abre automaticamente em http://localhost:3000
```

### 5. Build de Produção

```bash
npm run build
# Ou: yarn build

# Gera pasta build/ com arquivos otimizados
```

### 6. Dependências Principais (package.json)

```json
{
  "name": "caixaseguradora-frontend",
  "version": "1.0.0",
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.20.0",
    "axios": "^1.6.0",
    "recharts": "^2.10.0",
    "typescript": "^5.3.0"
  },
  "devDependencies": {
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "@testing-library/react": "^14.1.0",
    "@testing-library/jest-dom": "^6.1.0",
    "eslint": "^8.54.0",
    "prettier": "^3.1.0"
  },
  "scripts": {
    "start": "react-scripts start",
    "build": "react-scripts build",
    "test": "react-scripts test",
    "eject": "react-scripts eject"
  }
}
```

---

## Banco de Dados

### Opção 1: SQL Server LocalDB (Windows - Recomendado para Dev)

**Incluído com Visual Studio**, sem necessidade de instalação separada.

#### Verificar LocalDB:
```bash
sqllocaldb info
# Lista instâncias disponíveis: MSSQLLocalDB
```

#### Criar Database (se não existir):
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE CaixaSeguradoraMVP_Dev"
```

#### Connection String:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CaixaSeguradoraMVP_Dev;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### Opção 2: SQL Server via Docker (Cross-platform)

```bash
# Execute SQL Server em container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password123" \
  -p 1433:1433 --name sqlserver-caixa \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Aguarde 30 segundos para inicialização

# Crie o database
docker exec -it sqlserver-caixa \
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Password123" \
  -Q "CREATE DATABASE CaixaSeguradoraMVP_Dev"
```

#### Connection String:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=CaixaSeguradoraMVP_Dev;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True"
}
```

### Opção 3: DB2 (Para Ambiente de Produção)

**Download**: IBM DB2 Express-C ou IBM Data Server Driver

#### Connection String:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost:50000;Database=CAIXADB;User Id=db2admin;Password=yourpassword;CurrentSchema=SIDATA"
}
```

#### Pacote NuGet Necessário:
```bash
dotnet add package IBM.Data.DB2.Core
```

### Migrações (Entity Framework Core)

**Nota**: Este projeto usa "Database First" - o schema legado já existe.

#### Gerar Modelos a partir do Banco (se necessário):
```bash
cd src/CaixaSeguradora.Infrastructure

# SQL Server
dotnet ef dbcontext scaffold "Server=(localdb)\mssqllocaldb;Database=CaixaSeguradoraMVP_Dev;Trusted_Connection=True" \
  Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir Data/Models \
  --context-dir Data \
  --context CaixaSeguradoraContext \
  --force

# DB2
dotnet ef dbcontext scaffold "Server=localhost:50000;Database=CAIXADB;User Id=db2admin;Password=yourpassword" \
  IBM.EntityFrameworkCore \
  --output-dir Data/Models \
  --context-dir Data \
  --context CaixaSeguradoraContext \
  --force
```

**Nota**: Não execute migrações automáticas - o schema legado não deve ser modificado (requisito FR-053).

---

## Docker - Stack Completo

### 1. Arquivo docker-compose.yml

O arquivo já existe em: `CaixaSeguradora.MVP/docker-compose.yml`

```yaml
version: '3.8'

services:
  caixaseguradora-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: caixaseguradora-mvp
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80;https://+:443
      - ServiceSettings__UseMockData=true
      - ServiceSettings__CacheExpirationMinutes=15
      - ServiceSettings__TimeoutSeconds=30
      - SalesforceSettings__UseMockData=true
      - IntegrationSettings__GEWSV0012__UseMockData=true
    volumes:
      - ./logs:/app/logs
    networks:
      - caixaseguradora-network
    restart: unless-stopped

networks:
  caixaseguradora-network:
    driver: bridge
```

### 2. Executar Stack Completo

```bash
cd "/Users/<seu-usuario>/Development/Caixa Seguradora/CaixaSeguradora.MVP"

# Build e start dos containers
docker-compose up --build

# Ou rode em background (detached)
docker-compose up -d --build
```

### 3. Verificar Containers em Execução

```bash
docker ps
# Deve mostrar: caixaseguradora-mvp (RUNNING)
```

### 4. Ver Logs

```bash
# Logs em tempo real
docker-compose logs -f

# Logs do serviço específico
docker-compose logs -f caixaseguradora-api
```

### 5. Parar e Remover Containers

```bash
# Parar
docker-compose stop

# Parar e remover
docker-compose down

# Remover incluindo volumes
docker-compose down -v
```

### 6. Acessar o Container

```bash
# Shell interativo
docker exec -it caixaseguradora-mvp bash

# Verificar variáveis de ambiente
docker exec caixaseguradora-mvp env | grep ASPNETCORE
```

### 7. Docker Compose Completo (Com Database)

Para incluir SQL Server no stack, adicione ao `docker-compose.yml`:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: sqlserver-caixa
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - caixaseguradora-network
    restart: unless-stopped

  caixaseguradora-api:
    depends_on:
      - sqlserver
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CaixaSeguradoraMVP_Dev;User Id=sa;Password=YourStrong@Password123;TrustServerCertificate=True
    # ... resto da configuração

volumes:
  sqlserver-data:
```

---

## Executando Testes

### 1. Testes Unitários (.NET)

```bash
cd "/Users/<seu-usuario>/Development/Caixa Seguradora/CaixaSeguradora.MVP"

# Executar todos os testes
dotnet test

# Executar com output detalhado
dotnet test --logger "console;verbosity=detailed"

# Executar com cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### 2. Testes de Projeto Específico

```bash
# Testes da API
dotnet test tests/CaixaSeguradora.Api.Tests/CaixaSeguradora.Api.Tests.csproj

# Testes do Core
dotnet test tests/CaixaSeguradora.Core.Tests/CaixaSeguradora.Core.Tests.csproj

# Testes da Infrastructure
dotnet test tests/CaixaSeguradora.Infrastructure.Tests/CaixaSeguradora.Infrastructure.Tests.csproj
```

### 3. Testes Filtrados

```bash
# Executar apenas testes de uma categoria
dotnet test --filter "Category=Unit"

# Executar testes de uma classe específica
dotnet test --filter "FullyQualifiedName~ClaimServiceTests"

# Executar teste específico
dotnet test --filter "Name=SearchClaim_WithValidProtocol_ReturnsClaimDetails"
```

### 4. Testes de Integração E2E

Scripts disponíveis na raiz do projeto:

```bash
cd "/Users/<seu-usuario>/Development/Caixa Seguradora/CaixaSeguradora.MVP"

# Testar todos os endpoints REST
chmod +x test-all-endpoints.sh
./test-all-endpoints.sh

# Testar API específica
chmod +x test-api.sh
./test-api.sh

# E2E completo com validações
chmod +x test-e2e-complete.sh
./test-e2e-complete.sh
```

**Exemplo de saída esperada**:
```
✅ Teste 1: Autenticação - PASSED
✅ Teste 2: Criar Solicitação - PASSED
✅ Teste 3: Consultar Solicitação - PASSED
✅ Teste 4: Listar Assuntos - PASSED

Resumo: 4/4 testes passaram
```

### 5. Testes Frontend (React)

```bash
cd frontend

# Executar testes com Jest
npm test

# Modo interativo com watch
npm test -- --watch

# Gerar relatório de cobertura
npm test -- --coverage
```

### 6. Testes E2E com Playwright/Cypress (Planejado)

```bash
cd frontend

# Instalar Playwright
npm install -D @playwright/test

# Executar testes E2E
npx playwright test

# Modo UI interativo
npx playwright test --ui

# Ver relatório
npx playwright show-report
```

### 7. Interpretar Resultados

**Teste bem-sucedido**:
```
Passed!  - Failed:     0, Passed:    42, Skipped:     0, Total:    42
```

**Teste com falha**:
```
Failed!  - Failed:     1, Passed:    41, Skipped:     0, Total:    42

[xUnit.net 00:00:05.23]     CaixaSeguradora.Tests.ClaimServiceTests.SearchClaim_InvalidProtocol_ReturnsError [FAIL]
  Expected: "DOCUMENTO XXXXXXXXXXXXXXX NAO CADASTRADO"
  Actual:   null
```

---

## Endpoints e Acesso

### REST API Base URL

**Local Development**:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001` (recomendado)

**Docker**:
- HTTP: `http://localhost:5000`

### Swagger UI

**URL**: https://localhost:5001/swagger

Interface interativa para testar todos os endpoints REST.

### SOAP Endpoints

**Autenticação**:
```
URL: https://localhost:5001/soap/autenticacao
Namespace: http://ls.caixaseguradora.com.br/LS1134WSV0001_Autenticacao/v1
```

**Solicitação**:
```
URL: https://localhost:5001/soap/solicitacao
Namespace: http://ls.caixaseguradora.com.br/LS1134WSV0007_Solicitacao/v1
```

**Assunto**:
```
URL: https://localhost:5001/soap/assunto
Namespace: http://ls.caixaseguradora.com.br/LS1134WSV0006_Assunto/v1
```

### Credenciais de Teste

Para autenticação nos endpoints:

```json
{
  "codUsuario": "teste.usuario",
  "desSenha": "senha123",
  "codSistema": "S1"
}
```

### Exemplos de Chamadas cURL

#### 1. Health Check
```bash
curl -k https://localhost:5001/api/health
```

#### 2. Autenticação
```bash
curl -k -X POST https://localhost:5001/api/autenticacao/login \
  -H "Content-Type: application/json" \
  -d '{
    "codUsuario": "teste.usuario",
    "desSenha": "senha123",
    "codSistema": "S1"
  }'
```

#### 3. Buscar Sinistro (Exemplo - Endpoint planejado)
```bash
curl -k -X POST https://localhost:5001/api/claims/search \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "fonte": "01",
    "protsini": "123456",
    "dac": "7"
  }'
```

#### 4. Autorizar Pagamento (Exemplo - Endpoint planejado)
```bash
curl -k -X POST https://localhost:5001/api/claims/{id}/authorize-payment \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "paymentType": 1,
    "principalAmount": 10000.00,
    "correctionValue": 150.50,
    "beneficiaryName": "João Silva"
  }'
```

### Frontend URLs (Planejado)

**Aplicação**:
- Development: `http://localhost:3000`

**Páginas**:
- Home/Busca: `http://localhost:3000/`
- Detalhes do Sinistro: `http://localhost:3000/claims/{id}`
- Dashboard de Migração: `http://localhost:3000/dashboard`

---

## Estrutura do Projeto

### Backend (.NET 9)

```
CaixaSeguradora.MVP/
├── src/
│   ├── CaixaSeguradora.Api/              # API Layer
│   │   ├── Controllers/                   # REST Controllers
│   │   │   ├── AutenticacaoController.cs
│   │   │   ├── SolicitacaoController.cs
│   │   │   ├── AssuntoController.cs
│   │   │   └── ClaimsController.cs        # Novo - Sinistros
│   │   ├── SoapServices/                  # SOAP Endpoints
│   │   │   ├── AutenticacaoSoapService.cs
│   │   │   ├── SolicitacaoSoapService.cs
│   │   │   └── AssuntoSoapService.cs
│   │   ├── DTOs/                          # Data Transfer Objects
│   │   │   ├── Request/
│   │   │   └── Response/
│   │   ├── Mappings/                      # AutoMapper Profiles
│   │   ├── Middleware/                    # Error handling, logging
│   │   ├── Program.cs                     # Application entry point
│   │   ├── appsettings.json               # Configuration
│   │   └── appsettings.Development.json   # Dev overrides
│   │
│   ├── CaixaSeguradora.Core/             # Business Logic Layer
│   │   ├── Entities/                      # Domain Models
│   │   │   ├── ClaimMaster.cs             # TMESTSIN
│   │   │   ├── ClaimHistory.cs            # THISTSIN
│   │   │   ├── BranchMaster.cs            # TGERAMO
│   │   │   ├── CurrencyUnit.cs            # TGEUNIMO
│   │   │   ├── PolicyMaster.cs            # TAPOLICE
│   │   │   └── ...
│   │   ├── Interfaces/                    # Abstractions
│   │   │   ├── IClaimRepository.cs
│   │   │   ├── IClaimService.cs
│   │   │   └── IExternalValidationService.cs
│   │   ├── Services/                      # Business Logic
│   │   │   ├── ClaimService.cs
│   │   │   ├── PaymentAuthorizationService.cs
│   │   │   ├── PhaseManagementService.cs
│   │   │   └── CurrencyConversionService.cs
│   │   ├── Validators/                    # Business Rules
│   │   │   ├── ClaimSearchValidator.cs
│   │   │   ├── PaymentAuthorizationValidator.cs
│   │   │   └── ConsortiumProductValidator.cs
│   │   └── Enums/                         # Domain Enumerations
│   │       ├── PaymentType.cs
│   │       ├── InsuranceType.cs
│   │       └── PhaseIndicator.cs
│   │
│   └── CaixaSeguradora.Infrastructure/   # Data Access Layer
│       ├── Data/                          # EF Core
│       │   ├── CaixaSeguradoraContext.cs  # DbContext
│       │   ├── Configurations/            # Fluent API configs
│       │   └── Models/                    # EF Core entities (if scaffolded)
│       ├── Repositories/                  # Repository Implementations
│       │   ├── ClaimRepository.cs
│       │   ├── ClaimHistoryRepository.cs
│       │   └── ...
│       ├── ExternalServices/              # External APIs
│       │   ├── CNOUAService.cs            # Consortium validation
│       │   ├── SIPUAService.cs            # EFP validation
│       │   └── SIMDAService.cs            # HB validation
│       └── Logging/                       # Serilog configuration
│
└── tests/
    ├── CaixaSeguradora.Api.Tests/        # API Integration Tests
    ├── CaixaSeguradora.Core.Tests/       # Unit Tests
    └── CaixaSeguradora.Infrastructure.Tests/  # Repository Tests
```

### Frontend (React 18 - Planejado)

```
frontend/
├── public/
│   ├── index.html
│   ├── Site.css                          # Migrated from Visual Age
│   └── favicon.ico
│
├── src/
│   ├── components/                       # Reusable Components
│   │   ├── common/
│   │   │   ├── Button.tsx
│   │   │   ├── Input.tsx
│   │   │   ├── Modal.tsx
│   │   │   └── Loader.tsx
│   │   ├── claims/
│   │   │   ├── ClaimSearchForm.tsx
│   │   │   ├── ClaimDetailView.tsx
│   │   │   ├── PaymentAuthorizationForm.tsx
│   │   │   └── PaymentHistoryTable.tsx
│   │   └── dashboard/
│   │       ├── ProgressCard.tsx
│   │       ├── MetricsChart.tsx
│   │       ├── ComponentStatusGrid.tsx
│   │       └── ActivitiesTimeline.tsx
│   │
│   ├── pages/                            # Page Components
│   │   ├── ClaimSearchPage.tsx           # Main search screen
│   │   ├── ClaimDetailPage.tsx           # Claim details + authorization
│   │   └── MigrationDashboardPage.tsx    # Project status dashboard
│   │
│   ├── services/                         # API Communication
│   │   ├── claimsApi.ts                  # Claims CRUD
│   │   ├── authApi.ts                    # Authentication
│   │   ├── dashboardApi.ts               # Dashboard metrics
│   │   └── httpClient.ts                 # Axios config
│   │
│   ├── models/                           # TypeScript Interfaces
│   │   ├── Claim.ts
│   │   ├── ClaimHistory.ts
│   │   ├── PaymentAuthorization.ts
│   │   └── MigrationStatus.ts
│   │
│   ├── utils/                            # Utility Functions
│   │   ├── formatters.ts                 # Date, currency formatting
│   │   ├── validators.ts                 # Client-side validation
│   │   └── constants.ts                  # App constants
│   │
│   ├── hooks/                            # Custom React Hooks
│   │   ├── useAuth.ts
│   │   ├── useClaims.ts
│   │   └── useDashboard.ts
│   │
│   ├── App.tsx                           # Root component
│   ├── index.tsx                         # Entry point
│   └── routes.tsx                        # React Router config
│
├── tests/
│   ├── components/                       # Component tests
│   ├── integration/                      # Integration tests
│   └── e2e/                              # End-to-end tests
│
├── package.json
├── tsconfig.json
└── .env.local
```

### Arquivos de Configuração

**Backend**:
- `CaixaSeguradora.MVP.sln` - Solution file
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Dev overrides
- `Dockerfile` - Container image
- `docker-compose.yml` - Multi-container setup

**Frontend**:
- `package.json` - NPM dependencies
- `tsconfig.json` - TypeScript config
- `.env.local` - Environment variables

---

## Troubleshooting

### Problema 1: dotnet command not found

**Sintoma**: `bash: dotnet: command not found`

**Solução**:
1. Verifique se o .NET 9 SDK está instalado:
   ```bash
   # Windows
   where dotnet

   # macOS/Linux
   which dotnet
   ```

2. Se não instalado, baixe em: https://dotnet.microsoft.com/download/dotnet/9.0

3. Reinicie o terminal após instalação

4. Verifique novamente:
   ```bash
   dotnet --version
   ```

### Problema 2: Erro de Certificado SSL

**Sintoma**:
```
The SSL connection could not be established
System.Net.Http.HttpRequestException: The SSL connection could not be established
```

**Solução**:
```bash
# Windows/macOS
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Linux
dotnet dev-certs https --clean
dotnet dev-certs https
# Adicione manualmente ao trusted certificates
```

**Alternativa**: Use HTTP para desenvolvimento:
```bash
dotnet run --urls "http://localhost:5000"
```

### Problema 3: Porta Já em Uso

**Sintoma**:
```
Error: Unable to bind to https://localhost:5001 on the IPv4 loopback interface: 'Address already in use'
```

**Solução**:
```bash
# Windows
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# macOS/Linux
lsof -ti:5001 | xargs kill -9

# Ou use uma porta diferente
dotnet run --urls "http://localhost:5050;https://localhost:5051"
```

### Problema 4: Erro de Conexão com Banco de Dados

**Sintoma**:
```
SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server
```

**Solução para LocalDB**:
```bash
# Verificar status do LocalDB
sqllocaldb info MSSQLLocalDB

# Se parado, iniciar
sqllocaldb start MSSQLLocalDB

# Verificar se o database existe
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases"
```

**Solução para Docker SQL Server**:
```bash
# Verificar se container está rodando
docker ps | grep sqlserver

# Se não, iniciar
docker start sqlserver-caixa

# Testar conexão
docker exec -it sqlserver-caixa /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Password123" -Q "SELECT @@VERSION"
```

### Problema 5: NuGet Package Restore Failed

**Sintoma**:
```
error NU1102: Unable to find package 'PackageName' with version (>= X.X.X)
```

**Solução**:
```bash
# Limpar cache do NuGet
dotnet nuget locals all --clear

# Restaurar novamente
dotnet restore --force

# Se persistir, adicione fonte do NuGet
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
```

### Problema 6: Docker Build Failed

**Sintoma**:
```
ERROR [internal] load metadata for mcr.microsoft.com/dotnet/aspnet:9.0
```

**Solução**:
```bash
# Verificar conexão com Docker Hub
docker pull mcr.microsoft.com/dotnet/aspnet:9.0

# Se falhar, verificar autenticação
docker login

# Tentar build novamente
docker-compose build --no-cache
```

### Problema 7: Frontend - Module Not Found

**Sintoma** (quando frontend existir):
```
Module not found: Error: Can't resolve 'axios'
```

**Solução**:
```bash
cd frontend

# Deletar node_modules e package-lock.json
rm -rf node_modules package-lock.json

# Reinstalar
npm install

# Ou com cache limpo
npm cache clean --force
npm install
```

### Problema 8: Testes Falhando

**Sintoma**:
```
Failed!  - Failed:     5, Passed:    37, Skipped:     0
```

**Solução**:
1. Verifique se o `UseMockData: true` está configurado
2. Verifique se o appsettings.Development.json está correto
3. Execute apenas os testes falhando:
   ```bash
   dotnet test --filter "Name~FailingTestName" --logger "console;verbosity=detailed"
   ```
4. Verifique logs detalhados

### Problema 9: Memory Issues ao Rodar Docker

**Sintoma**:
```
docker-compose up -d
Creating caixaseguradora-mvp ... error
ERROR: Insufficient memory
```

**Solução**:
1. Aumente memória do Docker Desktop:
   - Windows/Mac: Settings → Resources → Memory (mínimo 4GB)
2. Feche outros containers:
   ```bash
   docker stop $(docker ps -aq)
   ```
3. Limpe recursos não usados:
   ```bash
   docker system prune -a
   ```

### Problema 10: Visual Studio Code não Reconhece C#

**Sintoma**: IntelliSense não funciona, erros não aparecem

**Solução**:
1. Instale extensão "C# Dev Kit" da Microsoft
2. Reinicie VS Code
3. Aguarde download do OmniSharp
4. Verifique output:
   - View → Output → C# (ou OmniSharp)
5. Se falhar, reinstale:
   ```bash
   code --uninstall-extension ms-dotnettools.csharp
   code --install-extension ms-dotnettools.csdevkit
   ```

### Logs e Diagnóstico

**Backend Logs**:
```bash
# Logs em tempo real
cd src/CaixaSeguradora.Api
dotnet run | tee -a debug.log

# Logs do Serilog (se configurado)
tail -f logs/log-$(date +%Y%m%d).txt
```

**Docker Logs**:
```bash
# Logs do container
docker logs -f caixaseguradora-mvp

# Últimas 100 linhas
docker logs --tail 100 caixaseguradora-mvp
```

**Habilitar Debug Logging**:

Em `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

---

## Próximos Passos

### 1. Explorar a Aplicação

- [x] Backend rodando localmente
- [ ] Testar endpoints via Swagger UI
- [ ] Executar testes unitários
- [ ] Explorar código fonte (Controllers, Services, Repositories)

### 2. Configurar IDE

**VS Code**:
```bash
# Abrir workspace
code "/Users/<seu-usuario>/Development/Caixa Seguradora/CaixaSeguradora.MVP"
```

**Extensões recomendadas**:
- C# Dev Kit
- REST Client (para testar APIs)
- Docker
- GitLens

### 3. Familiarizar-se com a Especificação

Leia os documentos de especificação:

```bash
cd "/Users/<seu-usuario>/Development/Caixa Seguradora/POC Visual Age/specs/001-visualage-dotnet-migration"

# Especificação completa
cat spec.md

# Plano de implementação
cat plan.md

# Modelo de dados
cat data-model.md
```

### 4. Entender o Sistema Legado

- **Visual Age Source**: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/#SIWEA-V116.esf`
- **Stylesheet**: `/Users/brunosouza/Development/Caixa Seguradora/POC Visual Age/Site.css`

### 5. Tarefas de Desenvolvimento

Use o comando Speckit para gerar tarefas:

```bash
# Gerar lista de tarefas
/speckit.tasks

# Ver tarefas
cat tasks.md

# Implementar tarefas
/speckit.implement
```

### 6. Criar seu Primeiro Feature

**Branch Strategy**:
```bash
# Crie uma branch para sua feature
git checkout -b feature/meu-nome/descrição

# Exemplo
git checkout -b feature/joao/claim-search-endpoint
```

**Development Cycle**:
1. Escrever testes (TDD)
2. Implementar funcionalidade
3. Rodar testes
4. Commit
5. Push e criar PR

### 7. Participar do Dashboard de Migração

Quando o dashboard estiver implementado:
- Acesse: `http://localhost:3000/dashboard`
- Atualize status das suas tarefas
- Acompanhe progresso do time

### 8. Documentação Adicional

**Docs Online**:
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [React Documentation](https://react.dev/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)

**Docs do Projeto**:
```
specs/001-visualage-dotnet-migration/
├── spec.md               # Requisitos funcionais completos
├── plan.md               # Plano de implementação
├── data-model.md         # Modelo de dados e entidades
├── research.md           # Decisões técnicas
├── contracts/            # OpenAPI e WSDL specs
└── quickstart.md         # Este guia
```

### 9. Contribuir

**Padrões de Código**:
- Siga convenções C# (.NET Coding Style)
- Use `async/await` para operações I/O
- Escreva testes para novas funcionalidades
- Documente APIs com XML comments

**Commit Messages**:
```bash
# Formato: tipo(escopo): descrição
feat(claims): add claim search by protocol number
fix(auth): resolve token expiration issue
test(payment): add payment authorization tests
docs(readme): update setup instructions
```

### 10. Obter Ajuda

**Dúvidas Técnicas**:
- Consulte a especificação (`spec.md`)
- Verifique a seção Troubleshooting deste guia
- Consulte os testes existentes como exemplos

**Erros Comuns**:
- Sempre verifique logs primeiro
- Use `dotnet --info` para diagnosticar ambiente
- Verifique se `UseMockData: true` para desenvolvimento

**Suporte do Time**:
- Slack/Teams channel: [#caixa-seguradora-migration]
- Tech Lead: [nome@empresa.com]
- Reunião diária: [horário]

---

## Checklist de Onboarding

Use este checklist para confirmar que seu ambiente está 100% funcional:

### Ferramentas

- [ ] .NET 9 SDK instalado (`dotnet --version`)
- [ ] Node.js 18+ instalado (`node --version`)
- [ ] Git instalado (`git --version`)
- [ ] Docker instalado (opcional) (`docker --version`)
- [ ] IDE configurada (VS Code, Visual Studio, ou Rider)

### Repositório

- [ ] Repositório clonado
- [ ] Branch correta checkout (`001-visualage-dotnet-migration`)
- [ ] Estrutura de pastas verificada

### Backend

- [ ] `dotnet restore` executado sem erros
- [ ] `dotnet build` completado com sucesso
- [ ] `appsettings.Development.json` configurado
- [ ] Backend rodando (`dotnet run`)
- [ ] Swagger UI acessível (https://localhost:5001/swagger)
- [ ] Health check retorna 200 OK

### Banco de Dados

- [ ] SQL Server ou LocalDB instalado
- [ ] Connection string configurada
- [ ] Database criado (se necessário)
- [ ] Conexão testada

### Docker (Opcional)

- [ ] Docker Desktop instalado
- [ ] `docker-compose up` executado
- [ ] Container rodando (`docker ps`)
- [ ] API acessível via Docker (http://localhost:5000)

### Testes

- [ ] `dotnet test` executado
- [ ] Todos os testes passando
- [ ] Scripts E2E testados (`./test-all-endpoints.sh`)

### Frontend (Quando Disponível)

- [ ] `npm install` completado
- [ ] `.env.local` configurado
- [ ] `npm start` rodando
- [ ] App acessível (http://localhost:3000)

### Documentação

- [ ] `spec.md` lido
- [ ] `plan.md` revisado
- [ ] `data-model.md` compreendido
- [ ] Este `quickstart.md` seguido completamente

### Verificação Final

- [ ] Consegue fazer login via API
- [ ] Consegue acessar todos os endpoints via Swagger
- [ ] Consegue rodar testes com sucesso
- [ ] Consegue fazer build de produção (`dotnet build --configuration Release`)
- [ ] Compreende a estrutura do projeto
- [ ] Sabe onde buscar ajuda

---

## Referências Rápidas

### Comandos Essenciais

```bash
# Backend
cd "/Users/<seu-usuario>/Development/Caixa Seguradora/CaixaSeguradora.MVP"
dotnet restore                    # Restaurar dependências
dotnet build                      # Build do projeto
dotnet run                        # Executar aplicação
dotnet test                       # Rodar testes
dotnet watch run                  # Hot reload

# Frontend (quando disponível)
cd frontend
npm install                       # Instalar dependências
npm start                         # Dev server
npm test                          # Rodar testes
npm run build                     # Build produção

# Docker
docker-compose up --build         # Build e start
docker-compose down               # Stop e remove
docker-compose logs -f            # Ver logs
docker ps                         # Containers rodando

# Git
git status                        # Ver mudanças
git checkout -b feature/xyz       # Nova branch
git add .                         # Stage mudanças
git commit -m "mensagem"          # Commit
git push origin feature/xyz       # Push
```

### URLs Importantes

| Serviço | URL | Descrição |
|---------|-----|-----------|
| Backend API | https://localhost:5001 | API principal |
| Swagger UI | https://localhost:5001/swagger | Documentação interativa |
| SOAP Autenticação | https://localhost:5001/soap/autenticacao | Endpoint SOAP |
| Frontend (planejado) | http://localhost:3000 | Aplicação React |
| Dashboard (planejado) | http://localhost:3000/dashboard | Dashboard de migração |

### Credenciais de Teste

| Campo | Valor |
|-------|-------|
| codUsuario | teste.usuario |
| desSenha | senha123 |
| codSistema | S1 |

### Arquivos de Configuração

| Arquivo | Localização | Propósito |
|---------|-------------|-----------|
| appsettings.json | src/CaixaSeguradora.Api/ | Config base |
| appsettings.Development.json | src/CaixaSeguradora.Api/ | Config dev (criar) |
| docker-compose.yml | Raiz do projeto | Docker setup |
| .env.local | frontend/ | Env vars React |

---

## Conclusão

Parabéns! Se você seguiu todos os passos, seu ambiente de desenvolvimento está completo e funcional.

**Você agora consegue**:
- ✅ Rodar o backend .NET 9 localmente
- ✅ Acessar APIs via Swagger
- ✅ Executar testes unitários e de integração
- ✅ Usar Docker para ambiente isolado
- ✅ Entender a estrutura do projeto
- ✅ Resolver problemas comuns

**Próximos marcos**:
1. Implementar frontend React
2. Criar endpoints de sinistros (Claims API)
3. Integrar validações externas (CNOUA, SIPUA, SIMDA)
4. Desenvolver dashboard de migração
5. Testes E2E completos
6. Deploy em ambiente de staging

**Tempo de onboarding esperado**: 20-30 minutos

**Ficou alguma dúvida?**
- Consulte a seção [Troubleshooting](#troubleshooting)
- Revise a [Especificação Completa](spec.md)
- Entre em contato com o time de migração

---

**Última atualização**: 2025-10-23
**Versão do Documento**: 1.0
**Mantido por**: Equipe de Migração Caixa Seguradora

---

## Apêndice A: Estrutura de Dados (Resumo)

### Entidades Principais

| Entidade | Tabela Legacy | Descrição |
|----------|---------------|-----------|
| ClaimMaster | TMESTSIN | Registro principal do sinistro |
| ClaimHistory | THISTSIN | Histórico de autorizações |
| BranchMaster | TGERAMO | Dados de filiais |
| CurrencyUnit | TGEUNIMO | Taxas de conversão |
| PolicyMaster | TAPOLICE | Dados de apólices |
| ClaimPhase | SI_SINISTRO_FASE | Fases de workflow |

### Códigos e Enums

| Enum | Valores | Descrição |
|------|---------|-----------|
| PaymentType | 1-5 | Tipo de pagamento |
| PhaseIndicator | 1-2 | Abertura/Fechamento de fase |
| InsuranceType | 0-N | Tipo de seguro |
| OperationCode | 1098 | Código de operação (autorização) |

## Apêndice B: Variáveis de Ambiente

### Backend (.NET)

```bash
# appsettings.Development.json
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000;https://+:5001
ServiceSettings__UseMockData=true
ConnectionStrings__DefaultConnection="Server=(localdb)\\mssqllocaldb;Database=CaixaSeguradoraMVP_Dev;Trusted_Connection=True"
```

### Frontend (React)

```bash
# .env.local
REACT_APP_API_BASE_URL=https://localhost:5001
REACT_APP_API_TIMEOUT=30000
REACT_APP_MOCK_DATA=true
REACT_APP_ENABLE_DASHBOARD=true
```

### Docker

```bash
# docker-compose.yml environment section
ASPNETCORE_ENVIRONMENT=Development
ServiceSettings__UseMockData=true
ConnectionStrings__DefaultConnection="Server=sqlserver;Database=CaixaSeguradoraMVP_Dev;User Id=sa;Password=YourStrong@Password123"
```

## Apêndice C: Recursos de Aprendizado

### .NET 9 e C#

- [Microsoft Learn - ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [Clean Architecture in .NET](https://jasontaylor.dev/clean-architecture-getting-started/)
- [Entity Framework Core Tutorial](https://www.entityframeworktutorial.net/efcore/entity-framework-core.aspx)

### React e TypeScript

- [React Official Docs](https://react.dev/learn)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [React + TypeScript Cheatsheet](https://react-typescript-cheatsheet.netlify.app/)

### Clean Architecture

- [Clean Architecture by Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [DDD, Hexagonal, Onion, Clean, CQRS](https://herbertograca.com/2017/11/16/explicit-architecture-01-ddd-hexagonal-onion-clean-cqrs-how-i-put-it-all-together/)

### Docker

- [Docker Get Started](https://docs.docker.com/get-started/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)

---

**Boa sorte no desenvolvimento! 🚀**
