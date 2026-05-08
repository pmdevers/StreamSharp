# StreamSharp.PostgreSQL Plugin

The PostgreSQL plugin provides event store and read model persistence using Entity Framework Core 10 with the Npgsql provider.

## Overview

This plugin implements:
- **Event Store** (`IEventStore`) - Persists domain events in the `EventDocument` table
- **Read Models** - Projects events into queryable DTOs (`LibraryDto`, `LibraryItemDto`)
- **Event Publishing** - Interceptor-based integration with the event bus
- **Design-time Support** - Factory for EF Core CLI tooling

## Architecture

### Database Schema

```sql
-- Event Store
CREATE TABLE "EventDocument" (
    "Id" UUID PRIMARY KEY,
    "AggregateId" UUID NOT NULL,
    "AggregateName" TEXT NOT NULL,
    "Version" INT NOT NULL,
    "Type" TEXT NOT NULL,
    "Data" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    UNIQUE("AggregateId", "AggregateName", "Version")
);

-- Read Models
CREATE TABLE "Libraries" (
    "Id" UUID PRIMARY KEY,
    "Name" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL
);

CREATE TABLE "LibraryItems" (
    "Id" UUID PRIMARY KEY,
    "LibraryId" UUID NOT NULL,
    "Name" TEXT NOT NULL,
    "Type" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL
);
```

### Key Components

#### `StreamSharpDB` (DbContext)
- Manages EF Core configuration and migrations
- Contains `DbSet` for `EventDocument`, `Libraries`, and `LibraryItems`
- Intercepts `SaveChangesAsync` to publish events
- Tracks aggregates in memory via `TrackAggregate()`

#### `EventPublishingInterceptor`
- EF Core `SaveChangesInterceptor`
- Captures added `EventDocument` entities during save
- Publishes events to `IEventBus` after successful commit
- Ensures event durability before publication

#### `PostgreSqlEventStore`
- Implements `IEventStore` interface
- Queries persisted `EventDocument` rows
- Deserializes events using `EventSerializer`
- Provides read-only access to the event history

#### `StreamSharpDBContextFactory`
- Implements `IDesignTimeDbContextFactory<StreamSharpDB>`
- Required by EF Core CLI for migrations
- **Not used at runtime** — see design-time note below

#### `LibraryProjector`
- Projects domain events onto read models
- Handles `LibraryCreated` and `LibraryItemCreated` events
- Upserts DTOs into the read-model tables

## Development & Migrations

### Initial Setup

1. Ensure PostgreSQL is running and accessible
2. Configure the connection string in the host application
3. The plugin automatically runs migrations on startup via `MigrationService`

### Adding Database Migrations

**Important**: Use this exact two-step process to preserve migration files during the plugin build.

The plugin uses a custom build process that strips non-essential output files. Without special handling, migration artifacts are lost. Follow these steps:

#### Step 1: Build with Disabled Stripping

```powershell
dotnet build plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  -c Debug `
  -p:DisableStripPluginOutput=true
```

This flag (defined in `plugins\Directory.Build.targets`) tells the build pipeline to preserve all files, including the `Migrations` folder.

#### Step 2: Create Migration with --no-build

```powershell
dotnet ef migrations add <MigrationName> `
  -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  --no-build
```

The `--no-build` flag prevents EF CLI from rebuilding the plugin with default stripping, which would remove the just-created migration files.

### Why This Process?

By default, `plugins\Directory.Build.targets` strips the plugin output to remove development artifacts before distribution. This is controlled by the `StripPluginOutput` MSBuild property:

- **Default (StripPluginOutput=true)**: Removes non-runtime files (source, Migrations, XML docs, etc.)
- **Disabled (DisableStripPluginOutput=true)**: Preserves all files for tooling and development

The EF Core CLI expects the migration files to persist in the project folder, so we must:
1. Build once with stripping disabled, ensuring migration files stay in `bin/Debug/net10.0/`
2. Run EF commands with `--no-build` to use the already-built assemblies without re-stripping

### Example: Adding a New Migration

Suppose you've modified the DbContext model (e.g., added a new read-model table). Here's how to create the migration:

```powershell
# 1. Build with DisableStripPluginOutput
dotnet build plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  -c Debug `
  -p:DisableStripPluginOutput=true

# 2. Create the migration
dotnet ef migrations add AddNewReadModel `
  -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  --no-build
```

This generates:
- `Migrations\<timestamp>_AddNewReadModel.cs` — migration logic (Up/Down)
- `Migrations\<timestamp>_AddNewReadModel.Designer.cs` — migration metadata
- `Migrations\StreamSharpDBModelSnapshot.cs` — updated model snapshot

### Applying Migrations

Migrations are automatically applied at startup via `MigrationService`. To manually apply:

```powershell
dotnet ef database update -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

### Removing a Migration

If a migration hasn't been deployed to production, you can remove it:

```powershell
dotnet ef migrations remove -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

Then follow the two-step build/migration process above to create a corrected migration.

## Service Registration

The plugin registers its services in `AddPostgreSQL()`:

```csharp
services
    .AddScoped<EventPublishingInterceptor>()
    .AddDbContext<StreamSharpDB>(/* options */)
    .AddScoped<IUnitOfWork, StreamSharpDB>()
    .AddScoped<IEventStore, PostgreSqlEventStore>()
    .AddScoped<ILibraryQueries, LibraryQueries>()
    .AddHostedService<MigrationService>();
```

**Note**: The event bus services are registered separately via `AddEventBus()` if `IEventBus` is not already in the DI container. This ensures idempotency.

## API Endpoints

The plugin does not expose HTTP endpoints directly. It provides:
- `IEventStore` — for retrieving events
- `ILibraryQueries` — for querying read models
- `IEventBus` — published after event persistence

See the main server's `Features/Events/` for event-related endpoints.

## Testing

The PostgreSQL plugin includes integration tests for:
- Event persistence and retrieval
- Read model projections
- Unique constraints (AggregateId, AggregateName, Version)
- Event publisher integration

Run tests with:

```powershell
dotnet test
```

## Design-Time Notes

### Why StreamSharpDBContextFactory?

`StreamSharpDBContextFactory` implements `IDesignTimeDbContextFactory<StreamSharpDB>` and is used **only** by EF Core CLI tools (migrations, database updates, etc.). It is **never instantiated at runtime**.

The factory allows EF tooling to:
- Instantiate `StreamSharpDB` without starting the full ASP.NET Core host
- Discover the model from a minimal DbContext instance
- Generate and apply migrations without application startup overhead

At runtime, the DbContext is created via dependency injection in `AddPostgreSQL()`.

## Troubleshooting

### Migration Command Fails with "Assembly Not Found"

**Error**: `An assembly specified in the application dependencies manifest was not found: package: 'StreamSharp.Core', version: '1.0.0', path: 'StreamSharp.Core.dll'`

**Solution**: Ensure you're using the two-step build + `--no-build` process. If you run `dotnet ef migrations add` without `--no-build`, EF rebuilds and re-strips the plugin, losing the migration files.

### Migrations Not Appearing in the Project

**Issue**: `dotnet ef migrations add` completes but no files appear.

**Causes**:
1. Not using `DisableStripPluginOutput=true` on the build
2. Not using `--no-build` on the EF command
3. StripPluginOutput target deleting files after EF creates them

**Fix**: Follow the two-step process exactly as documented above.

### "EventDocument Unique Constraint Violated"

**Error**: `23505: duplicate key value violates unique constraint "IX_EventDocument_AggregateId_AggregateName_Version"`

**Cause**: Attempting to save duplicate versions of the same aggregate.

**Fix**: Ensure your event publishing logic increments `Version` correctly and doesn't retry failed saves without handling the version conflict.

## Further Reading

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/)
- [StreamSharp Core Architecture](../../agents.md#architecture)
- [Event Sourcing Pattern](../../agents.md#event-sourcing)
