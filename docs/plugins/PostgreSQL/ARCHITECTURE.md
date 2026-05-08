# StreamSharp.PostgreSQL Architecture

This document provides an in-depth look at the design decisions, data flows, and component interactions within the PostgreSQL plugin.

## Overview

The PostgreSQL plugin bridges domain events (from `StreamSharp.Core`) with persistent storage and read models. It implements two core patterns:

1. **Event Sourcing** — All state mutations are recorded as immutable events in the `EventDocument` table
2. **CQRS** — Queries are served from denormalized read models (`Libraries`, `LibraryItems`, future `MediaFiles`, etc.)

```
┌─────────────────────┐
│  Domain Aggregates  │
│  (Library, Item)    │
└──────────┬──────────┘
           │ RecordEvent()
           ▼
┌─────────────────────────┐
│  Uncommitted Events     │
│  (in memory)            │
└──────────┬──────────────┘
           │ SaveChangesAsync()
           ▼
┌──────────────────────────────────┐
│  EventDocument (Event Store)     │
│  AggregateId, Version, Data, ... │
└──────────┬───────────────────────┘
           │
      ┌────┴────┐
      │          │
      ▼          ▼
┌─────────────┐ ┌──────────────────┐
│ EventPublish│ │ Event Projector  │
│ Interceptor │ │ (LibraryProjector)
└─────┬───────┘ └────────┬─────────┘
      │                  │
      ▼                  ▼
 ┌─────────────────────────────────┐
 │ IEventBus (Event Hub)           │
 │ EventBusBackgroundService       │
 └────────────┬────────────────────┘
              │
         ┌────┴─────┐
         │           │
         ▼           ▼
   Read Models  Event Handlers
   (Libraries)  (Subscribers)
```

## Data Models

### EventDocument

The immutable, event-store table:

```csharp
public class EventDocument
{
    public Guid Id { get; set; }                   // Primary key (new Guid())
    public Guid AggregateId { get; set; }          // Which aggregate owns this event
    public string AggregateName { get; set; }      // "Library", "LibraryItem"
    public int Version { get; set; }               // Event version for this aggregate
    public string Type { get; set; }               // Event type, e.g., "LibraryCreated"
    public string Data { get; set; }               // Serialized event payload (JSON)
    public DateTimeOffset CreatedAt { get; set; }  // When recorded
}
```

**Unique Index**: `(AggregateId, AggregateName, Version)` — Prevents duplicate versions.

**Data Format**: `Data` contains JSON serialized to a UTF-8 string. The `EventSerializer` handles round-tripping.

### Read Models (DTOs)

Denormalized tables for efficient querying:

```csharp
// In Libraries read model
public record LibraryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

// In LibraryItems read model
public record LibraryItemDto
{
    public Guid Id { get; set; }
    public Guid LibraryId { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

These are **mutable** in the database but **immutable** during queries (they're `record` types).

## Key Components

### 1. StreamSharpDB (DbContext)

**File**: `plugins\StreamSharp.PostgreSQL\StreamSharpDB.cs`

**Responsibilities**:
- Define the EF Core model (`DbSet<EventDocument>`, read models, etc.)
- Intercept `SaveChangesAsync()` to capture and persist uncommitted events
- Track aggregates in memory (via `_trackedAggregates`)

**Key Methods**:

```csharp
public class StreamSharpDB : DbContext, IUnitOfWork
{
    public DbSet<EventDocument> EventDocuments { get; set; }
    public DbSet<LibraryDto> Libraries { get; set; }
    public DbSet<LibraryItemDto> LibraryItems { get; set; }

    // Track in-memory aggregates
    private readonly Dictionary<string, Aggregate> _trackedAggregates = new();

    public void TrackAggregate(Aggregate aggregate)
    {
        var key = $"{aggregate.Id}:{aggregate.GetType().Name}";
        _trackedAggregates[key] = aggregate;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Convert tracked aggregates' uncommitted events to EventDocument rows
        var eventDocs = ConvertTrackedAggregatesToEventDocuments();

        // 2. Add to DbSet
        foreach (var doc in eventDocs)
        {
            EventDocuments.Add(doc);
        }

        // 3. Save all changes (both EventDocuments and read-model updates)
        int changes = await base.SaveChangesAsync(ct);

        // 4. Clear tracked aggregates
        _trackedAggregates.Clear();

        return changes;
    }
}
```

**Why Not Attach Aggregates Directly?**

Domain aggregates (e.g., `Library`) are business logic entities. They should **not** be mapped as EF entities because:
- Aggregates have complex business rules (event sourcing, concurrency, etc.)
- EF Core's change tracking would interfere with domain logic
- Only events and read models should be persisted; aggregates are reconstructed on load

Instead, we track aggregates in a side dictionary and only persist their events.

### 2. EventPublishingInterceptor

**File**: `plugins\StreamSharp.PostgreSQL\EventPublishingInterceptor.cs`

**Responsibilities**:
- Hook into EF's save pipeline via `SaveChangesInterceptor`
- Capture `EventDocument` entities before and after commit
- Publish to `IEventBus` **after successful commit**

**Key Methods**:

```csharp
public class EventPublishingInterceptor : SaveChangesInterceptor
{
    private List<EventDocument>? _capturedDocuments;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct)
    {
        // Capture EventDocuments **before** commit
        _capturedDocuments = eventData.Context?.ChangeTracker
            .Entries<EventDocument>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct)
    {
        // Publish **after** successful commit
        if (_capturedDocuments?.Count > 0)
        {
            foreach (var doc in _capturedDocuments)
            {
                var domainEvent = _eventSerializer.Deserialize(doc);
                await _eventBus.PublishAsync(domainEvent, ct);
            }
        }

        return result;
    }
}
```

**Why Publish After Commit?**

Publishing before commit risks:
- Event reaching subscribers before database confirms persistence
- Subscribers processing stale data if the database transaction fails
- Inconsistency between event store and subscriber state

Publishing after ensures durability-first semantics: events are persistent before anyone reacts.

### 3. PostgreSqlEventStore

**File**: `plugins\StreamSharp.PostgreSQL\PostgreSqlEventStore.cs` (if it exists) or implemented inline

**Implements**: `IEventStore`

**Responsibilities**:
- Read-only access to persisted events
- Query events by aggregate ID and name
- Deserialize event data back into domain events

**Example Usage**:

```csharp
var eventStore = serviceProvider.GetRequiredService<IEventStore>();

// Get all events for a specific library (aggregate)
var events = await eventStore.GetEventsAsync(
    aggregateId: libraryId,
    aggregateName: "Library",
    cancellationToken: ct);

// Reconstruct the aggregate from history
var library = AggregateRoot.LoadFromHistory<Library>(
    aggregateId: libraryId,
    events: events);
```

### 4. LibraryProjector

**File**: `plugins\StreamSharp.PostgreSQL\Projections\LibraryProjector.cs`

**Responsibilities**:
- Subscribe to domain events (`LibraryCreated`, `LibraryItemCreated`, etc.)
- Update read models (upsert `LibraryDto`, `LibraryItemDto`)
- Ensure eventual consistency between event store and read models

**Example**:

```csharp
public class LibraryProjector : IDomainEventHandler<LibraryCreated>
{
    private readonly StreamSharpDB _db;

    public async Task HandleAsync(LibraryCreated @event, CancellationToken ct)
    {
        var dto = new LibraryDto
        {
            Id = @event.AggregateId,
            Name = @event.Name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Libraries.Add(dto);
        await _db.SaveChangesAsync(ct);
    }
}
```

**Why Separate Projector?**

- **Single Responsibility**: Event capture vs. read-model building are distinct concerns
- **Resilience**: If projection fails, the event is still durably stored; retry projections independently
- **Flexibility**: Projectors can be replayed (delete read models, re-run projectors from event stream)

### 5. StreamSharpDBContextFactory

**File**: `plugins\StreamSharp.PostgreSQL\StreamSharpDBContextFactory.cs`

**Implements**: `IDesignTimeDbContextFactory<StreamSharpDB>`

**Responsibilities**:
- Create `StreamSharpDB` instances **for EF tooling only** (migrations, updates, etc.)
- Not used at runtime (dependency injection is used instead)

**Why Separate Factory?**

EF Core CLI tools run outside the ASP.NET Core host. They need a way to instantiate the DbContext independently. This factory provides that without requiring the full application startup.

```csharp
public class StreamSharpDBContextFactory : IDesignTimeDbContextFactory<StreamSharpDB>
{
    public StreamSharpDB CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StreamSharpDB>()
            .UseNpgsql("Host=localhost;Database=streamsharp;...")
            .AddInterceptors(new EventPublishingInterceptor(/*...*/))
            .Options;

        return new StreamSharpDB(options);
    }
}
```

## Data Flow: Creating a Library

### Step 1: User Makes Request

```http
POST /api/libraries HTTP/1.1
Content-Type: application/json

{ "name": "My Library" }
```

### Step 2: Handler Orchestrates

```csharp
// In Features/Libraries/CreateLibrary.cs
public static async Task<IResult> Handle(
    CreateLibraryRequest req,
    IUnitOfWork db,
    IEventBus eventBus)
{
    // 1. Create aggregate (purely in-memory)
    var library = Library.Create(req.Name);

    // 2. Track in the DbContext (but don't attach as EF entity)
    var streamSharpDb = (StreamSharpDB)db;
    streamSharpDb.TrackAggregate(library);

    // 3. Persist events and read models
    await db.SaveChangesAsync();

    return Results.Created($"/api/libraries/{library.Id}", library);
}
```

### Step 3: Aggregate Records Event

```csharp
// In Library.cs (domain model)
public class Library : AggregateRoot<LibraryId>
{
    public static Library Create(string name)
    {
        var library = new Library(LibraryId.New());

        // Record the domain event
        library.RecordEvent(new LibraryCreated(
            AggregateId: library.Id.ToString(),
            Name: name));

        return library;
    }

    private void Apply(LibraryCreated e)
    {
        Name = e.Name;
    }
}
```

### Step 4: SaveChangesAsync() Converts Events to Documents

```
StreamSharpDB.SaveChangesAsync()
    │
    ├─ Convert tracked Aggregate.UncommittedEvents → EventDocument
    │  LibraryCreated { Id, Name, ... }
    │  becomes
    │  EventDocument {
    │      AggregateId: library.Id,
    │      AggregateName: "Library",
    │      Version: 1,
    │      Type: "LibraryCreated",
    │      Data: JSON(LibraryCreated),
    │      CreatedAt: now
    │  }
    │
    ├─ Add EventDocument to DbSet
    │
    └─ Call base.SaveChangesAsync()
        │
        ├─ EF validates and commits to PostgreSQL
        │
        └─ Interceptor.SavedChangesAsync() fires
           │
           └─ Publish event to IEventBus
              │
              └─ EventBusBackgroundService picks up event
                 │
                 └─ Dispatch to IDomainEventHandler<LibraryCreated>
                    │
                    └─ LibraryProjector.HandleAsync()
                       │
                       └─ Insert LibraryDto into "Libraries" table
```

## Concurrency & Versions

### Event Versioning

Each event for an aggregate has a `Version`:
- First event: `Version = 1`
- Second event: `Version = 2`
- Etc.

This ensures idempotency and enables conflict detection:

```sql
-- Unique constraint enforces one event per version
UNIQUE ("AggregateId", "AggregateName", "Version")
```

If you try to save two events with `Version=1` for the same aggregate, PostgreSQL rejects it.

### Optimistic Concurrency

When loading an aggregate:

```csharp
var events = await eventStore.GetEventsAsync(libraryId, "Library");
var library = AggregateRoot.LoadFromHistory<Library>(libraryId, events);

// Business logic modifies library
library.Rename("New Name");

// If another request modified the library in between,
// RecordEvent() increments Version, but won't match what's in the database
// (This is eventual consistency — the test suite should handle it)
```

Read-side eventual consistency means queries may temporarily show stale read models. This is acceptable for most use cases (and avoids locking).

## Event Serialization

### EventSerializer

**File**: `plugins\StreamSharp.PostgreSQL\EventSerializer.cs`

Converts domain events ↔ JSON:

```csharp
public class EventSerializer
{
    public string Serialize(DomainEvent @event)
    {
        var json = JsonConvert.SerializeObject(@event);
        return json;
    }

    public DomainEvent Deserialize(EventDocument doc)
    {
        // Resolve type by doc.Type name
        var type = Type.GetType($"StreamSharp.Core.Events.{doc.Type}");

        if (type == null)
            throw new InvalidOperationException($"Unknown event type: {doc.Type}");

        return (DomainEvent)JsonConvert.DeserializeObject(doc.Data, type)!;
    }
}
```

**Important**: Event types must be registered or discoverable by name. The serializer uses reflection to resolve `doc.Type` back to the C# type.

## Testing Patterns

### Unit Testing Aggregates

```csharp
[Test]
public void LibraryCreated_SetsName()
{
    // 1. Create aggregate (doesn't require DB)
    var library = Library.Create("My Library");

    // 2. Assert domain state
    Assert.That(library.Name, Is.EqualTo("My Library"));

    // 3. Assert events were recorded
    var events = library.GetUncommittedEvents();
    Assert.That(events, Has.Length.EqualTo(1));
    Assert.That(events[0], Is.TypeOf<LibraryCreated>());
}
```

### Integration Testing (Full Flow)

```csharp
[Test]
public async Task CreateLibrary_PersistsEventAndReadModel()
{
    using var db = new StreamSharpDB(/* test options */);

    // 1. Create and save
    var library = Library.Create("My Library");
    db.TrackAggregate(library);
    await db.SaveChangesAsync();

    // 2. Assert event was persisted
    var doc = await db.EventDocuments
        .SingleAsync(e => e.AggregateId == library.Id);
    Assert.That(doc.Type, Is.EqualTo("LibraryCreated"));

    // 3. Assert read model was updated (after projection)
    var dto = await db.Libraries
        .SingleAsync(l => l.Id == library.Id);
    Assert.That(dto.Name, Is.EqualTo("My Library"));
}
```

## Performance Considerations

### Event Store Queries

```csharp
// ❌ Inefficient: Loads all events, filters in memory
var allEvents = db.EventDocuments.ToList();
var libraryEvents = allEvents.Where(e => e.AggregateId == id).ToList();

// ✅ Efficient: Filters at database
var libraryEvents = await db.EventDocuments
    .Where(e => e.AggregateId == id)
    .OrderBy(e => e.Version)
    .ToListAsync();
```

### Read Model Queries

```csharp
// ❌ Inefficient: N+1 query (one per item)
foreach (var lib in libraries)
{
    lib.Items = await db.LibraryItems
        .Where(i => i.LibraryId == lib.Id)
        .ToListAsync();  // DB hit per library
}

// ✅ Efficient: Single query with join
var librariesWithItems = await db.Libraries
    .Include(l => l.Items)
    .ToListAsync();
```

### Indexing

Ensure indexes on commonly queried columns:
- `EventDocument.AggregateId` — Query events for a specific aggregate
- `EventDocument.AggregateName` — Query by aggregate type
- `LibraryItems.LibraryId` — Query items in a library
- Foreign key columns

## Troubleshooting Guide

### Events Not Appearing in Event Store

**Symptom**: `RecordEvent()` called, but `EventDocument` rows not in database.

**Checklist**:
1. Is `SaveChangesAsync()` being called? Events are only persisted on save.
2. Is the aggregate being tracked? `db.TrackAggregate(aggregate)` must be called.
3. Did the transaction commit? Check transaction logs.

### Read Models Out of Sync

**Symptom**: Event store has events, but read models are empty.

**Checklist**:
1. Is the projector registered? `AddScoped<IDomainEventHandler<TEvent>, Projector>()`
2. Is `IEventBus` working? Check `EventBusBackgroundService` logs.
3. Did the event publish successfully? Add logging in `EventPublishingInterceptor.SavedChangesAsync()`.

**Fix**: Replay projections:
```sql
-- 1. Clear read models
DELETE FROM "Libraries";

-- 2. Re-run application (projector subscribes to events and rebuilds)
```

### Unique Constraint Violations

**Error**: `23505: duplicate key value violates unique constraint "IX_EventDocument_AggregateId_AggregateName_Version"`

**Cause**: Two events with the same `(AggregateId, AggregateName, Version)` attempted to insert.

**Fix**:
1. Check version incrementing logic in `Aggregate.RecordEvent()`
2. Ensure `SaveChangesAsync()` is not called twice for the same uncommitted event
3. Review retry logic — failed saves should not retry without clearing uncommitted events

## Further Reading

- [Event Sourcing Pattern](https://martinfowler.com/eaaDev/EventSourcing.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/)
- [StreamSharp Core Architecture](../../agents.md#architecture)
