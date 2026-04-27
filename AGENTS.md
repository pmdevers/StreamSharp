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
# Frontend only (from src/StreamSharp.UI/)
bun run dev          # dev server
bun run build        # production build
bun run test:unit    # Vitest unit tests
bun run lint         # ESLint
```

NuGet packages use Central Package Management (`Directory.Packages.props`) with committed lock files.

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

Every aggregate and `EventStream` is decorated with `[GenerateId]`. The source generator emits a value-object `struct` (e.g. `LibraryId`) with `New()` and `Empty` members. Always use the generated Id type — never raw `Guid` or `string` for entity identity.

### Plugin System

Plugins are loaded from a configurable `PluginsRoot` directory via `PluginManager.LoadAll()`. Each plugin runs in an isolated, collectible `AssemblyLoadContext`. `StreamSharp.Plugin` is shared (not re-loaded in isolation). Plugins receive `IPluginContext` to register services and endpoints.

### EventBus

Channel-based (`Channel.CreateUnbounded<DomainEvent>`). `EventBus` enqueues; `EventBusBackgroundService` dequeues and dispatches to registered `DomainEventHandler<T>` delegates. Handlers can be lambdas or convention-based classes with `Task Method(T event, CancellationToken ct)` methods.

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
- [Directory.Packages.props](Directory.Packages.props) — all NuGet version pins
- [src/Directory.Build.props](src/Directory.Build.props) — shared MSBuild config
- [src/StreamSharp.UI/AGENTS.md](src/StreamSharp.UI/AGENTS.md) — frontend-specific guidelines
