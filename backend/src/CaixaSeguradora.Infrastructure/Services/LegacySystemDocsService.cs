using CaixaSeguradora.Core.DTOs;
using CaixaSeguradora.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// Service implementation for legacy system documentation
/// Provides static data extracted from markdown documentation files
/// </summary>
public class LegacySystemDocsService : ILegacySystemDocsService
{
    private readonly ILogger<LegacySystemDocsService> _logger;
    private readonly string _docsBasePath;

    public LegacySystemDocsService(ILogger<LegacySystemDocsService> logger)
    {
        _logger = logger;
        // In production, this could come from configuration
        _docsBasePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs", "legacy-system");
    }

    public async Task<LegacySystemDashboardDto> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching legacy system dashboard data");

        // In a real implementation, this could parse the markdown files
        // For now, returning structured static data based on the documentation
        return new LegacySystemDashboardDto
        {
            SystemInfo = new LegacySystemInfoDto
            {
                ProgramId = "#SIWEA-V116.esf",
                ProgramName = "SIWEA - Sistema de Indenização e Workflow de Eventos Atendidos",
                SystemType = "Online Transaction Processing (OLTP) com interface CICS",
                Platform = "IBM Mainframe z/OS",
                Language = "COBOL/EZEE (IBM VisualAge EZEE 4.40)",
                Database = "IBM DB2 for z/OS",
                CodeSize = "851.9 KB",
                CreationDate = new DateTime(1989, 10, 1),
                LastUpdate = new DateTime(2014, 2, 11),
                YearsInProduction = DateTime.Now.Year - 1989,
                CurrentVersion = "V116",
                OriginalProgrammer = "SOLANGE",
                OriginalAnalyst = "COSMO",
                LastMaintenance = "CAD73898 (11/02/2014)"
            },
            Statistics = new SystemStatisticsDto
            {
                ActiveUsers = 200,
                DailyTransactions = 8000,
                TotalDataVolume = 2.5m,
                DataVolumeUnit = "M sinistros",
                CriticalityLevel = "Missão Crítica",
                AverageResponseTime = 2.3m, // seconds
                Availability = 99.95m // percentage
            },
            Documentation = new DocumentationMetricsDto
            {
                TotalDocuments = 13,
                TotalPages = 245,
                BusinessRules = 122,
                DatabaseTables = 13,
                ExternalIntegrations = 3,
                ScreenMaps = 5,
                WorkflowPhases = 8,
                DataStructures = 45
            },
            AvailableDocuments = GetDocumentsList(),
            TechnologyStack = new TechnologyStackDto
            {
                LegacyTechnologies = new List<string>
                {
                    "IBM z/OS Mainframe",
                    "IBM CICS Transaction Server",
                    "IBM DB2 for z/OS",
                    "IBM MQ Series",
                    "IBM VisualAge EZEE 4.40",
                    "COBOL ANSI 85",
                    "Terminal 3270"
                },
                MigrationTechnologies = new List<string>
                {
                    ".NET 9.0",
                    "React 19 + TypeScript",
                    "SQL Server / PostgreSQL",
                    "Azure Cloud",
                    "Docker / Kubernetes",
                    "RESTful APIs",
                    "Entity Framework Core"
                }
            },
            BusinessProcesses = new List<BusinessProcessDto>
            {
                new()
                {
                    Name = "Busca e Localização de Sinistros",
                    Description = "Múltiplos métodos de busca com validação em tempo real",
                    Features = new List<string>
                    {
                        "Busca por protocolo (FONTE + PROTSINI + DAC)",
                        "Busca por número de sinistro (ORGSIN + RMOSIN + NUMSIN)",
                        "Busca por código líder (CODLIDER + SINLID)",
                        "Validação contra base DB2 com 13 entidades"
                    },
                    PerformanceTarget = "< 3 segundos"
                },
                new()
                {
                    Name = "Autorização de Pagamento de Indenizações",
                    Description = "Processo completo de validação e autorização de pagamentos",
                    Features = new List<string>
                    {
                        "5 tipos de pagamento configuráveis",
                        "100+ regras de negócio automatizadas",
                        "Conversão monetária BTNF (SUSEP)",
                        "Registro de auditoria completo"
                    },
                    PerformanceTarget = "< 90 segundos (pipeline completo)"
                },
                new()
                {
                    Name = "Gestão de Workflow por Fases",
                    Description = "Sistema de fases configuráveis com transições automáticas",
                    Features = new List<string>
                    {
                        "8 fases de processamento",
                        "Transições baseadas em eventos",
                        "Histórico completo de mudanças",
                        "Controle de SLA por fase"
                    },
                    PerformanceTarget = "Tempo real"
                },
                new()
                {
                    Name = "Integração com Sistemas Externos",
                    Description = "Validações em sistemas especializados de produtos",
                    Features = new List<string>
                    {
                        "CNOUA: Produtos de consórcio (6814, 7701, 7709)",
                        "SIPUA: Validação de contratos EFP",
                        "SIMDA: Validação de contratos HB",
                        "Modo offline em caso de indisponibilidade"
                    },
                    PerformanceTarget = "< 5 segundos por validação"
                }
            },
            LastAnalyzed = DateTime.Now
        };
    }

    public async Task<string> GetDocumentContentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching document content for {DocumentId}", documentId);

        var documentPath = Path.Combine(_docsBasePath, $"{documentId}.md");

        if (!File.Exists(documentPath))
        {
            _logger.LogWarning("Document {DocumentId} not found at {Path}", documentId, documentPath);
            throw new FileNotFoundException($"Document {documentId} not found");
        }

        return await File.ReadAllTextAsync(documentPath, cancellationToken);
    }

    public async Task<List<DocumentDto>> GetAvailableDocumentsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching available documents list");
        return await Task.FromResult(GetDocumentsList());
    }

    private List<DocumentDto> GetDocumentsList()
    {
        return new List<DocumentDto>
        {
            new()
            {
                Id = "README",
                Title = "Documentação do Sistema Legado SIWEA",
                Description = "Índice geral e visão geral da documentação",
                FileName = "README.md",
                Pages = 4,
                Category = "Geral",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "01-executive-summary",
                Title = "Sumário Executivo",
                Description = "Visão executiva do sistema, objetivos e indicadores",
                FileName = "01-executive-summary.md",
                Pages = 12,
                Category = "Gestão",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "02-architecture",
                Title = "Arquitetura Técnica",
                Description = "Arquitetura em 3 camadas, infraestrutura e deployment",
                FileName = "02-architecture.md",
                Pages = 28,
                Category = "Técnico",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "03-data-structures",
                Title = "Estruturas de Dados",
                Description = "Working storage, copy books, layouts de registros",
                FileName = "03-data-structures.md",
                Pages = 22,
                Category = "Técnico",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "04-database-model",
                Title = "Modelo de Banco de Dados",
                Description = "13 entidades, relacionamentos, DDL scripts",
                FileName = "04-database-model.md",
                Pages = 35,
                Category = "Técnico",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "05-business-logic",
                Title = "Lógica de Negócio",
                Description = "122 regras de negócio documentadas e categorizadas",
                FileName = "05-business-logic.md",
                Pages = 40,
                Category = "Negócio",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "06-external-integrations",
                Title = "Integrações Externas",
                Description = "CNOUA, SIPUA, SIMDA - protocolos e especificações",
                FileName = "06-external-integrations.md",
                Pages = 25,
                Category = "Técnico",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "07-operations-guide",
                Title = "Guia de Operações",
                Description = "Procedimentos operacionais, troubleshooting, backup",
                FileName = "07-operations-guide.md",
                Pages = 18,
                Category = "Operações",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "08-ui-screens",
                Title = "Interface e Telas",
                Description = "Mapas de telas 3270, navegação, campos",
                FileName = "08-ui-screens.md",
                Pages = 20,
                Category = "Interface",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "09-migration-guide",
                Title = "Guia de Migração",
                Description = "Estratégia completa para migração .NET 9.0",
                FileName = "09-migration-guide.md",
                Pages = 30,
                Category = "Migração",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "10-glossary",
                Title = "Glossário",
                Description = "Termos de negócio e técnicos",
                FileName = "10-glossary.md",
                Pages = 15,
                Category = "Referência",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "COMPLETE_LEGACY_DOCUMENTATION",
                Title = "Documentação Completa Consolidada",
                Description = "Consolidação de toda a documentação em um único arquivo",
                FileName = "COMPLETE_LEGACY_DOCUMENTATION.md",
                Pages = 180,
                Category = "Referência",
                LastModified = new DateTime(2025, 10, 27)
            },
            new()
            {
                Id = "FUNCIONALIDADES-E-PONTOS-DE-FUNCAO",
                Title = "Análise de Funcionalidades e Pontos de Função",
                Description = "Análise detalhada de function points e estimativas",
                FileName = "FUNCIONALIDADES-E-PONTOS-DE-FUNCAO.md",
                Pages = 35,
                Category = "Gestão",
                LastModified = new DateTime(2025, 10, 27)
            }
        };
    }
}
