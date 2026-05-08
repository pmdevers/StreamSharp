# StreamSharp — Agent Guidelines

StreamSharp is a cloud-native, Kubernetes-targeted personal media server built with event-sourced Domain-Driven Design. It exposes an ASP.NET Core API, embeds a Vue SPA, and supports runtime-loaded plugins via isolated `AssemblyLoadContext`.

## Projects

| Project | Role |
|---|---|
| `StreamSharp.Core` | Domain model — aggregates, domain events, repositories, `IEventStore`, `IEventBus`, `AggregateRoot` |
| `StreamSharp.Server` | Main executable — ASP.NET Core host, API feature endpoints, plugin loading, EventBus |
| `StreamSharp.UI` | Vue 3 + TypeScript + Vite + TailwindCSS SPA embedded into the server assembly |
| `StreamSharp.PostgresSQL` | EF Core + Npgsql `IEventStore<TId>` implementation; read-model projections |
| `StreamSharp.Plugin` | Public plugin contract — `IPlugin`, `IPluginContext` (the only assembly plugin authors reference) |
| `StreamSharp.DummyPlugin` | Reference plugin — demonstrates service and endpoint registration |
| `StreamSharp.SourceGenerators` | Roslyn incremental generator — emits strongly-typed value-object Id types from `[GenerateId]` |
| `StreamSharp.Stream` | Dedicated streaming microservice (stub — not yet implemented) |

## Build & Test

```bash
dotnet build                                    # build entire solution
dotnet run --project src/StreamSharp.Server     # run the server
dotnet test                                     # run all tests (TUnit)
```

The Vue frontend is built automatically by MSBuild (`bun install && bun run build` in `StreamSharp.UI/`). You need [Bun](https://bun.sh) installed.

```bash
# Frontend only (from plugins/StreamSharp.UI/)
bun run dev          # dev server
bun run build        # production build
bun run test:unit    # Vitest unit tests
bun run lint         # ESLint
```

For detailed UI development guidance, see [docs/plugins/UI/DEVELOPMENT.md](docs/plugins/UI/DEVELOPMENT.md).

NuGet packages use Central Package Management (`Directory.Packages.props`) with committed lock files.

## Database Migrations

### Adding a New Migration

**IMPORTANT**: Always use this two-step process to preserve migration files:

```powershell
# Step 1: Build PostgreSQL plugin with DisableStripPluginOutput=true
# This ensures the Migrations folder is preserved in the build output
dotnet build plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  -c Debug `
  -p:DisableStripPluginOutput=true

# Step 2: Create migration without rebuilding
# The --no-build flag prevents the default build that strips migration files
dotnie ef migrations add <MigrationName> `
  -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  --no-build
```

### Why This Process?

- **DisableStripPluginOutput=true**: By default, `plugins\Directory.Build.targets` removes non-essential files. This flag preserves the `Migrations` folder and all migration files.
- **--no-build flag**: Prevents EF CLI from rebuilding with default settings (which would strip migrations). Uses already-built assemblies instead.

### Migration File Structure

```
plugins\StreamSharp.PostgreSQL\Migrations\
├── 20260509000000_InitialCreate.cs              # Migration logic (Up/Down)
├── 20260509000000_InitialCreate.Designer.cs     # Migration metadata
└── StreamSharpDBModelSnapshot.cs                # Current EF Core model state
```

### Applying Migrations

Migrations are automatically applied on startup via `MigrationService` (see `plugins\StreamSharp.PostgreSQL\AddPostgreSQL.cs`).

To manually apply:
```powershell
dotnet ef database update -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

### Important Notes

- Always commit migration files to git after creation
- The `StreamSharpDBModelSnapshot.cs` file represents the current model state and is auto-updated
- EventDocument table has a unique constraint on (AggregateId, AggregateName, Version)
- For comprehensive PostgreSQL plugin documentation, see [docs/plugins/PostgreSQL/README.md](docs/plugins/PostgreSQL/README.md) and [docs/plugins/PostgreSQL/MIGRATIONS.md](docs/plugins/PostgreSQL/MIGRATIONS.md)

## Architecture

### Event Sourcing

All state changes go through domain events. Aggregates inherit `AggregateRoot<TId>` and call `RecordEvent(e)`, which appends to the uncommitted list and dispatches to an `Apply(TEvent)` method via reflection. Repositories rehydrate aggregates from `EventStream` via `AggregateRoot.LoadFromHistory`, then publish uncommitted events through `IEventBus` after saving.

### Vertical Slice Features

Each API feature lives in `src/StreamSharp.Server/Features/<Area>/`. Every feature is a static class with a `Handle` method. API routing registration is co-located in `*Api.cs` files per area using C# `extension` blocks on `WebApplication`.

```csharp
// Example pattern (net10 extension blocks)
public static class MyFeatureApi
{
    public static extension(WebApplication app) RegisterMyFeatureEndpoints()
    {
        app.MapGet("/myfeature/", MyFeature.Handle);
    }
}
```

### Strongly-Typed IDs

Every aggregate and `EventStream` is decorated with `[GenerateId]`. The source generator emits a value-object `struct` (e.g. `LibraryId`) with:
- Private constructor (prevents direct instantiation)
- `New()` factory method (for creating new IDs with `Guid.NewGuid()`)
- `From(Guid)` factory method (for converting from existing Guid)
- Implicit conversion to underlying `Guid` type
- No public `Value` property — use implicit casting instead

Always use the generated Id type — never raw `Guid` or `string` for entity identity.

### Plugin System

Plugins are loaded from a configurable `PluginsRoot` directory via `PluginManager.LoadAll()`. Each plugin runs in an isolated, collectible `AssemblyLoadContext`. `StreamSharp.Plugin` is shared (not re-loaded in isolation). Plugins receive `IPluginContext` to register services and endpoints.

### EventBus

Channel-based (`Channel.CreateUnbounded<DomainEvent>`). `EventBus` enqueues; `EventBusBackgroundService` dequeues and dispatches to registered `DomainEventHandler<T>` delegates. Handlers can be lambdas or convention-based classes with `Task Method(T event, CancellationToken ct)` methods.

Only registers services if `IEventBus` isn't already registered (uses conditional check in `AddEventBus()`).

## Conventions

- **Namespaces** follow project/folder structure exactly (e.g. `StreamSharp.Server.Features.Medialibrary`).
- **Domain events** are `record` types inheriting `DomainEvent`. API request DTOs are also `record` types.
- **C# `extension` blocks** (net10 preview) are used pervasively for `IServiceCollection` and `WebApplication` extension methods.
- **`Apply` pattern** — aggregates declare `void Apply(TEvent e)` methods; `RecordEvent` dispatches by reflection. Do not switch/dispatch manually.
- **Nullable reference types and implicit usings** are enabled everywhere.
- **Internals** are automatically exposed to `*.Tests` projects via `Directory.Build.props`.
- **SonarAnalyzer** runs on every build — do not suppress warnings without justification.
- **SBOM** (`Microsoft.Sbom.Targets`) is generated on every build.

## Testing

Tests use **TUnit** (not xUnit or NUnit). The test project is `test/StreamSharp.Server.Tests/`. Global retry (`[assembly: Retry(3)]`) is applied. Use `[Before(TestSession)]` / `[After(TestSession)]` for session-scoped setup, not constructors.

## Key Files

- [docs/introduction.md](docs/introduction.md) — project overview
- [docs/getting-started.md](docs/getting-started.md) — setup guide
- [docs/plugins/README.md](docs/plugins/README.md) — plugin documentation hub
- [docs/plugins/PostgreSQL/README.md](docs/plugins/PostgreSQL/README.md) — PostgreSQL plugin guide
- [docs/plugins/PostgreSQL/MIGRATIONS.md](docs/plugins/PostgreSQL/MIGRATIONS.md) — database migration workflow
- [docs/plugins/PostgreSQL/ARCHITECTURE.md](docs/plugins/PostgreSQL/ARCHITECTURE.md) — plugin architecture deep-dive
- [docs/plugins/UI/README.md](docs/plugins/UI/README.md) — UI plugin overview & architecture
- [docs/plugins/UI/DEVELOPMENT.md](docs/plugins/UI/DEVELOPMENT.md) — UI development guide & tutorials
- [Directory.Packages.props](Directory.Packages.props) — all NuGet version pins
- [src/Directory.Build.props](src/Directory.Build.props) — shared MSBuild config
- [src/StreamSharp.UI/AGENTS.md](src/StreamSharp.UI/AGENTS.md) — frontend-specific guidelines (legacy)
- [agents.md](agents.md) — this file
