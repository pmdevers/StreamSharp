# StreamSharp.PostgreSQL Documentation Index

A comprehensive guide to the StreamSharp PostgreSQL plugin for event sourcing and read model persistence.

## Quick Links

| Document | Purpose |
|---|---|
| [README](README.md) | Overview, architecture, service registration |
| [Migrations Guide](MIGRATIONS.md) | Complete database migration workflow |
| [Architecture Deep-Dive](ARCHITECTURE.md) | Component design, data flows, patterns |

## For Different Roles

### 👨‍💼 Project Managers / Tech Leads
Start with the [README Overview](README.md#overview) section to understand what the plugin does and its key responsibilities.

### 👨‍💻 Backend Developers (Adding Features)
1. Read [README - Architecture](README.md#architecture) for high-level concepts
2. Review [Architecture Deep-Dive - Data Flow](ARCHITECTURE.md#data-flow-creating-a-library) for concrete examples
3. Check [README - Service Registration](README.md#service-registration) when integrating with the host

### 🔧 DevOps / Database Administrators
1. Start with [README - Database Schema](README.md#database-schema) to understand the data model
2. Use [Migrations Guide - Quick Reference](MIGRATIONS.md#quick-reference) for deployment procedures
3. See [Migrations Guide - Troubleshooting](MIGRATIONS.md#troubleshooting) for common issues

### 🧪 QA / Test Engineers
Read [Architecture Deep-Dive - Testing Patterns](ARCHITECTURE.md#testing-patterns) for how to write and run tests against the plugin.

### 📚 Onboarding New Team Members
Follow this path in order:
1. [README Overview](README.md#overview)
2. [README - Architecture](README.md#architecture)
3. [Architecture Deep-Dive - Overview](ARCHITECTURE.md#overview)
4. [README - Development & Migrations](README.md#development--migrations)

## Common Tasks

### I need to... 🎯

**...add a new read model (database table)**
→ Follow [Migrations Guide - Scenario: Adding a New Read Model Table](MIGRATIONS.md#scenario-adding-a-new-read-model-table)

**...understand why migrations require that specific build process**
→ Read [Migrations Guide - Understanding the Plugin Build Process](MIGRATIONS.md#understanding-the-plugin-build-process)

**...fix a unique constraint error**
→ Jump to [Architecture - Concurrency & Versions](ARCHITECTURE.md#concurrency--versions) or [Migrations - Troubleshooting](MIGRATIONS.md#unique-constraint-violation-on-eventdocument)

**...understand how events flow through the system**
→ See [Architecture - Data Flow: Creating a Library](ARCHITECTURE.md#data-flow-creating-a-library)

**...test the plugin locally**
→ Review [Architecture - Testing Patterns](ARCHITECTURE.md#testing-patterns)

**...debug why read models are out of sync**
→ Check [Architecture - Troubleshooting Guide - Read Models Out of Sync](ARCHITECTURE.md#read-models-out-of-sync)

## Key Concepts at a Glance

### Event Sourcing
All state changes are recorded as immutable events. The `EventDocument` table is the single source of truth. Read models are derived from events.

### CQRS (Command Query Responsibility Segregation)
- **Commands** (writes) go through aggregates and produce events
- **Queries** (reads) go directly to read models (`LibraryDto`, `LibraryItemDto`)

### Aggregates
Domain objects (`Library`, `LibraryItem`) that record events and enforce business rules. They live in memory; only their events are persisted.

### Read Models
Denormalized, queryable tables optimized for specific queries. Updated via event handlers (projectors).

### Migrations
Managed by Entity Framework Core with a special two-step build process to preserve migration files in the plugin output. See [Migrations Guide](MIGRATIONS.md) for details.

## Troubleshooting Quick Links

- [Assembly not found error](MIGRATIONS.md#migration-command-fails-with-assembly-not-found)
- [Migrations not appearing](MIGRATIONS.md#migrations-not-appearing-in-the-project)
- [Unique constraint violations](MIGRATIONS.md#eventdocument-unique-constraint-violated)
- [Events not persisting](ARCHITECTURE.md#events-not-appearing-in-event-store)
- [Read models out of sync](ARCHITECTURE.md#read-models-out-of-sync)

## Related Documentation

- [agents.md - Plugin System](../../agents.md#plugin-system)
- [agents.md - Event Sourcing](../../agents.md#event-sourcing)
- [agents.md - Database Migrations](../../agents.md#database-migrations)

## File Structure

```
docs/plugins/PostgreSQL/
├── README.md               ← Start here
├── MIGRATIONS.md           ← For database work
├── ARCHITECTURE.md         ← For deep understanding
├── INDEX.md                ← You are here
└── plugins/StreamSharp.PostgreSQL/
    ├── StreamSharpDB.cs              (DbContext)
    ├── StreamSharpDBContextFactory.cs (EF tooling)
    ├── EventPublishingInterceptor.cs  (Event publishing)
    ├── EventSerializer.cs            (JSON serialization)
    ├── PostgreSqlEventStore.cs       (Event retrieval)
    ├── Aggregates/
    │   ├── EventDocument.cs          (Event model)
    │   ├── EventDocumentConfiguration.cs
    │   └── ...
    ├── Projections/
    │   └── LibraryProjector.cs       (Read model updates)
    ├── Queries/
    │   └── LibraryQueries.cs         (Query DTOs)
    └── Migrations/
        ├── [timestamp]_InitialCreate.cs
        ├── [timestamp]_InitialCreate.Designer.cs
        └── StreamSharpDBModelSnapshot.cs
```

## Support & Questions

If documentation is unclear or missing:
1. Check the related [agents.md](../../agents.md) sections
2. Review [Architecture - Troubleshooting Guide](ARCHITECTURE.md#troubleshooting-guide)
3. Search code comments in `plugins/StreamSharp.PostgreSQL/`
4. Ask a senior team member or create an issue in the repository

---

**Last Updated**: 2026-01-09  
**Relevant Plugin Version**: EF Core 10.0.7, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.7
