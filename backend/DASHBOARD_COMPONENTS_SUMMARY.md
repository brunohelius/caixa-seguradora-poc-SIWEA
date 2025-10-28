# Dashboard Components Quick Reference

## Component Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                    MigrationDashboardPage                      │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │  React Query: useQuery('/api/dashboard/overview')        │ │
│  │  Auto-refresh: 30 seconds                                │ │
│  └──────────────────────────────────────────────────────────┘ │
│                              │                                 │
│              ┌───────────────┴───────────────┐                │
│              ▼                               ▼                │
│  ┌─────────────────────┐         ┌─────────────────────┐     │
│  │  overview.          │         │  overview.           │     │
│  │  progressoGeral     │         │  statusUserStories   │     │
│  └──────────┬──────────┘         └──────────┬───────────┘     │
│             │                               │                 │
│  ┌──────────▼──────────┐         ┌─────────▼──────────┐      │
│  │  T124:              │         │  T125:              │      │
│  │  OverviewCards      │         │  UserStoryProgress  │      │
│  │                     │         │  List               │      │
│  │  4 metric cards:    │         │                     │      │
│  │  • Progress (%)     │         │  6 user stories:    │      │
│  │  • Requirements     │         │  • Status badges    │      │
│  │  • Tests            │         │  • Progress bars    │      │
│  │  • Code Coverage    │         │  • Metrics          │      │
│  └─────────────────────┘         └────────────────────┘       │
│                                                                │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │  overview.componentesMigrados                           │  │
│  └──────────────────────────┬──────────────────────────────┘  │
│                             │                                 │
│                  ┌──────────▼──────────┐                      │
│                  │  T126:              │                      │
│                  │  ComponentsGrid     │                      │
│                  │                     │                      │
│                  │  4 quadrants:       │                      │
│                  │  • Telas            │                      │
│                  │  • Regras Negócio   │                      │
│                  │  • Integrações BD   │                      │
│                  │  • Serviços Ext.    │                      │
│                  └─────────────────────┘                      │
│                                                                │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │  useQuery('/api/dashboard/performance?days=30')         │  │
│  │  Auto-refresh: 60 seconds                               │  │
│  └──────────────────────────┬──────────────────────────────┘  │
│                             │                                 │
│                  ┌──────────▼──────────┐                      │
│                  │  T127:              │                      │
│                  │  PerformanceCharts  │                      │
│                  │                     │                      │
│                  │  3 tabs:            │                      │
│                  │  • Comparison Bar   │                      │
│                  │  • Trend Line       │                      │
│                  │  • Details Table    │                      │
│                  └─────────────────────┘                      │
└────────────────────────────────────────────────────────────────┘
```

---

## T124: OverviewCards

### Component Structure
```tsx
<OverviewCards overview={overview.progressoGeral}>

  Card 1: Overall Progress
  ┌─────────────────────┐
  │  Progresso Geral    │
  │  ┌───────────┐      │
  │  │    52%    │      │ ← Circular SVG Progress
  │  └───────────┘      │
  │   3/6 User Stories  │
  └─────────────────────┘

  Card 2: Requirements
  ┌─────────────────────┐
  │   Requisitos        │
  │                     │
  │      42/55          │ ← Large number
  │  ▓▓▓▓▓▓▓▓░░░░       │ ← Progress bar
  └─────────────────────┘

  Card 3: Tests
  ┌─────────────────────┐
  │    Testes           │
  │                     │
  │   [187/200]         │ ← Colored badge
  │ Taxa: [93.5%]       │ ← Pass rate badge
  └─────────────────────┘

  Card 4: Code Coverage
  ┌─────────────────────┐
  │ Cobertura Código    │
  │  ┌───────────┐      │
  │  │   78.5%   │      │ ← Circular SVG Progress
  │  └───────────┘      │
  │    Meta: 80%        │
  └─────────────────────┘
</OverviewCards>
```

### Props Interface
```typescript
interface OverviewCardsProps {
  overview?: {
    percentualCompleto: number;      // 0-100
    userStoriesCompletas: number;    // count
    totalUserStories: number;        // count
    requisitosCompletos: number;     // count
    requisitosTotal: number;         // count
    testesAprovados: number;         // count
    testesTotal: number;             // count
    coberturaCodigo: number;         // 0-100
  };
}
```

### Key Features
- Circular progress with SVG animation
- Color-coded badges based on thresholds
- Responsive grid (1/2/4 columns)
- shadcn/ui Card, Progress, Badge components

---

## T125: UserStoryProgressList

### Component Structure
```tsx
<UserStoryProgressList stories={overview.statusUserStories}>

  User Story Card (repeated for each story)
  ┌────────────────────────────────────────────┐
  │ US001  [✅ Concluída]      Equipe Dev      │
  │ Claim Search & Filtering   Prev: 15/11/25  │
  │                                            │
  │ Progresso                          100%    │
  │ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓        │
  │                                            │
  │ Requisitos: 10/10    Testes: 8/10 (80%)   │
  │                                            │
  │ [⚠️ Bloqueio: Aguardando validação...]    │ ← If blocked
  └────────────────────────────────────────────┘
</UserStoryProgressList>
```

### Props Interface
```typescript
interface UserStory {
  codigo: string;              // "US001"
  nome: string;               // Story name
  status: string;             // COMPLETED|IN_PROGRESS|PENDING|BLOCKED
  percentualCompleto: number; // 0-100
  requisitosCompletos: number;
  requisitosTotal: number;
  testesAprovados: number;
  testesTotal: number;
  responsavel: string;
  dataEstimada: string;       // ISO date
  dataConclusao?: string;     // ISO date (optional)
  bloqueios?: string;         // Alert message (optional)
}
```

### Status Badge Mapping
| Status       | Badge Variant | Icon          | Color  |
|--------------|---------------|---------------|--------|
| COMPLETED    | default       | CheckCircle2  | Green  |
| IN_PROGRESS  | secondary     | Clock         | Blue   |
| PENDING      | outline       | Clock         | Gray   |
| BLOCKED      | destructive   | XCircle       | Red    |

### Key Features
- Status badges with icons
- Progress bars per story
- Date formatting (pt-BR)
- Blocker alerts with AlertCircle icon
- Hover effects on cards
- Responsive layout

---

## T126: ComponentsGrid

### Component Structure
```tsx
<ComponentsGrid components={overview.componentesMigrados}>

  Quadrant Grid (4 cards)
  ┌───────────────┬───────────────┐
  │ 🖥️  Telas      │ ⚙️  Regras    │
  │ Interfaces    │ Lógica        │
  │               │               │
  │    2/2        │   28/42       │
  │ ▓▓▓▓▓▓▓▓▓▓   │ ▓▓▓▓▓░░░░░   │
  │ 100%          │ 66%           │
  │ Progresso: 0  │ Progresso: 10 │
  │ Bloqueadas: 0 │ Bloqueadas: 4 │
  ├───────────────┼───────────────┤
  │ 🗄️  BD Integr.│ 🌐 Serviços   │
  │ Entidades     │ APIs          │
  │               │               │
  │    8/10       │    1/3        │
  │ ▓▓▓▓▓▓▓▓░░   │ ▓▓░░░░░░░░   │
  │ 80%           │ 33%           │
  │ Progresso: 2  │ Progresso: 2  │
  │ Bloqueadas: 0 │ Bloqueadas: 0 │
  └───────────────┴───────────────┘

  Summary Bar
  ┌─────────────────────────────────┐
  │  39 Total  │ 14 Progresso │ ... │
  └─────────────────────────────────┘
</ComponentsGrid>
```

### Props Interface
```typescript
interface ComponentCount {
  total: number;
  completas: number;
  emProgresso: number;
  bloqueadas: number;
  percentual: number; // Calculated: completas/total * 100
}

interface ComponentsGridProps {
  components?: {
    telas?: ComponentCount;
    regrasNegocio?: ComponentCount;
    integracoesBD?: ComponentCount;
    servicosExternos?: ComponentCount;
  };
}
```

### Quadrant Icons
| Quadrant          | Icon     | Color  |
|-------------------|----------|--------|
| Telas             | Monitor  | Blue   |
| Regras Negócio    | Settings | Purple |
| Integrações BD    | Database | Green  |
| Serviços Externos | Globe    | Orange |

### Key Features
- 4 quadrant grid layout
- Color-coded icons from lucide-react
- Progress bars per quadrant
- Status breakdown (in progress, blocked)
- Summary totals section
- Hover shadow effects
- Responsive (1/2/4 columns)

---

## T127: PerformanceCharts

### Component Structure
```tsx
<PerformanceCharts>

  Header
  ┌──────────────────────────────────────────┐
  │ Análise de Performance   [Período ▼]    │
  └──────────────────────────────────────────┘

  Tabs: [Comparação] [Tendência] [Detalhes]

  TAB 1: Comparison (Bar Chart)
  ┌──────────────────────────────────────────┐
  │  Tempo Resp  Throughput  Usuários  ...   │
  │   │■│        │    ■│     │     ■│        │
  │   │■│        │■   ■│     │■    ■│        │
  │   │■│        │■   ■│     │■    ■│        │
  │   └─┘        └────┘      └─────┘         │
  │  Red=Legacy  Blue=Novo                   │
  │                                          │
  │  [+85.3%] [+311%] [+900%] [+75%] [+95%] │
  └──────────────────────────────────────────┘

  TAB 2: Trend (Line Chart)
  ┌──────────────────────────────────────────┐
  │  Melhoria (%)                            │
  │  100┤                    ╱───────        │
  │   90┤          ╱────────╱                │
  │   80┤    ╱────╱                          │
  │   70┼───╱                                │
  │     └────────────────────────── Days     │
  │   — Tempo  — Throughput  — Erro         │
  └──────────────────────────────────────────┘

  TAB 3: Details (Table)
  ┌──────────────────────────────────────────┐
  │ Cenário     │ Legacy │ Novo  │ +% │ ✓    │
  ├─────────────┼────────┼───────┼────┼──────┤
  │ Busca Sin.  │ 850ms  │ 125ms │+85%│ ✅   │
  │ Autor. Pag. │ 2.5s   │ 0.8s  │+68%│ ✅   │
  │ Valid CNOUA │ 1200ms │ 350ms │+71%│ ✅   │
  │ Conv. Moeda │ 99.5%  │99.99% │+0.5│ ✅   │
  │ Atualiz Fase│ 94.2%  │ 99.7% │+5.8│ ✅   │
  └──────────────────────────────────────────┘
</PerformanceCharts>
```

### Key Metrics
| Metric                | Legacy | Novo  | Improvement |
|-----------------------|--------|-------|-------------|
| Tempo Resposta        | 850ms  | 125ms | +85.3%      |
| Throughput            | 450    | 1850  | +311.1%     |
| Usuários Concorrentes | 50     | 500   | +900%       |
| Uso de Memória        | 2048MB | 512MB | +75%        |
| Taxa de Erro          | 5.8%   | 0.3%  | +94.8%      |

### Key Features
- 3-tab interface (Recharts components)
- Bar chart with custom tooltip
- Line chart with 3 trend lines
- Detailed table with color coding
- Period selector dropdown
- React Query with 60s refresh
- Responsive design
- Custom tooltip styling

---

## Backend Data Flow

```
┌────────────────────────────────────────────────┐
│         GET /api/dashboard/overview            │
└──────────────────┬─────────────────────────────┘
                   │
         ┌─────────▼──────────┐
         │  DashboardService  │
         │  GetOverviewAsync()│
         └─────────┬──────────┘
                   │
         ┌─────────▼─────────────────────────┐
         │  Aggregate Data From:             │
         │  • _userStories (static list)     │
         │  • _claimRepository (database)    │
         │  • _healthCheckService            │
         └─────────┬─────────────────────────┘
                   │
         ┌─────────▼──────────┐
         │ Build DTOs:        │
         │ • ProgressoGeral   │
         │ • StatusUserStories│
         │ • ComponentesMig.  │
         │ • SaudeDoSistema   │
         └─────────┬──────────┘
                   │
         ┌─────────▼──────────┐
         │ Return JSON:       │
         │ DashboardOverviewDto│
         └────────────────────┘
```

### DTO Mapping
```
Backend (C#)                    Frontend (TypeScript)
─────────────────              ─────────────────────
ProgressoGeral                  overview.progressoGeral
  ├─ PercentualCompleto    →      .percentualCompleto
  ├─ RequisitosCompletos   →      .requisitosCompletos
  ├─ TestesAprovados       →      .testesAprovados
  └─ CoberturaCodigo       →      .coberturaCodigo

StatusUserStories               overview.statusUserStories
  └─ List<UserStoryDto>    →      UserStory[]
      ├─ Codigo            →        .codigo
      ├─ Status            →        .status
      └─ Bloqueios         →        .bloqueios

ComponentesMigrados             overview.componentesMigrados
  ├─ Telas                 →      .telas
  ├─ RegrasNegocio         →      .regrasNegocio
  ├─ IntegracoesBD         →      .integracoesBD
  └─ ServicosExternos      →      .servicosExternos
```

---

## API Endpoints Reference

| Endpoint                       | Method | Returns                    | Used By               |
|--------------------------------|--------|----------------------------|-----------------------|
| /api/dashboard/overview        | GET    | DashboardOverviewDto       | MigrationDashboardPage|
| /api/dashboard/components      | GET    | ComponentStatusDto[]       | Not used (legacy)     |
| /api/dashboard/performance     | GET    | PerformanceMetricDto[]     | PerformanceCharts     |
| /api/dashboard/activities      | GET    | ActivityDto[]              | ActivitiesTimeline    |
| /api/dashboard/health          | GET    | SystemHealthDto            | SystemHealthIndicators|

---

## Component Dependencies

### NPM Packages Required
```json
{
  "dependencies": {
    "react": "^19.1.1",
    "react-query": "^5.90.5",
    "recharts": "^3.3.0",
    "lucide-react": "^latest",
    "@radix-ui/react-*": "shadcn/ui components"
  }
}
```

### shadcn/ui Components Used
- Card, CardContent, CardHeader, CardTitle
- Badge
- Progress
- Select, SelectContent, SelectItem, SelectTrigger, SelectValue
- Tabs, TabsContent, TabsList, TabsTrigger

---

## Testing Checklist

### Functional Tests
- [ ] OverviewCards displays all 4 metrics correctly
- [ ] UserStoryProgressList shows all 6 stories
- [ ] ComponentsGrid renders 4 quadrants with correct data
- [ ] PerformanceCharts displays all 3 tabs
- [ ] Auto-refresh works (30s for overview, 60s for performance)
- [ ] Loading states appear during data fetch
- [ ] Error states appear on API failure

### Visual Tests
- [ ] Responsive design works on mobile (< 768px)
- [ ] Responsive design works on tablet (768-1024px)
- [ ] Responsive design works on desktop (> 1024px)
- [ ] Color scheme matches reference dashboard
- [ ] Icons display correctly
- [ ] Progress bars animate smoothly
- [ ] Hover effects work on interactive elements

### Integration Tests
- [ ] Backend API returns correct data structure
- [ ] Frontend correctly parses backend response
- [ ] React Query caching works
- [ ] Auto-refresh doesn't cause UI flicker
- [ ] Date formatting shows Portuguese locale

---

## Performance Optimization

### React Query Configuration
```typescript
// MigrationDashboardPage.tsx
const { data: overview } = useQuery({
  queryKey: ['dashboardOverview'],
  queryFn: async () => { /* ... */ },
  refetchInterval: 30000, // 30 seconds
  staleTime: 20000,       // Consider data stale after 20s
  cacheTime: 300000       // Keep in cache for 5 minutes
});
```

### Optimization Techniques
- React Query handles caching and deduplication
- Components use React.memo() for expensive renders
- SVG progress circles use CSS transforms (GPU-accelerated)
- Recharts uses virtualization for large datasets
- Lazy loading for chart components (code splitting)

---

## Troubleshooting

### Common Issues

**Issue**: Cards show "undefined" values
- **Cause**: API not returning data or property name mismatch
- **Fix**: Check browser console for API errors, verify DTO property names

**Issue**: Auto-refresh not working
- **Cause**: React Query refetchInterval not set
- **Fix**: Verify `refetchInterval` in useQuery config

**Issue**: Charts not rendering
- **Cause**: Recharts not installed or data format incorrect
- **Fix**: Run `npm install recharts`, check data structure

**Issue**: Styles not applying
- **Cause**: shadcn/ui not configured or Tailwind not processing
- **Fix**: Verify `tailwind.config.js` and `globals.css`

---

**Last Updated**: October 27, 2025
**Status**: Production Ready ✅
