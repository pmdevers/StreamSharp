# Database Migrations Guide — StreamSharp.PostgreSQL

This guide covers everything you need to know about managing Entity Framework Core migrations in the PostgreSQL plugin.

## Quick Reference

### Adding a New Migration

```powershell
# Step 1: Build with stripping disabled
dotnet build plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  -c Debug `
  -p:DisableStripPluginOutput=true

# Step 2: Create migration with no rebuild
dotnet ef migrations add MyMigrationName `
  -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  --no-build
```

### Applying Migrations

```powershell
# Automatic (on application startup)
dotnet run --project src\StreamSharp.Server

# Manual
dotnet ef database update -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

## Understanding the Plugin Build Process

### Default Build Behavior (StripPluginOutput=true)

When you build a plugin normally, the `plugins\Directory.Build.targets` file applies a "strip" operation that removes non-runtime files:

```
plugins\StreamSharp.PostgreSQL\bin\Debug\net10.0\
├── plugin.json                          ✓ (kept)
├── StreamSharp.PostgreSQL.dll           ✓ (kept)
├── StreamSharp.Core.dll                 ✓ (kept)
├── StreamSharp.PostgreSQL.deps.json     ✓ (kept)
├── Migrations\                          ✗ (REMOVED)
├── *.pdb                                ✗ (REMOVED)
└── ...
```

**Why?** Plugins are designed to be distributed as slim packages. Source artifacts like migrations are not needed at runtime and would bloat the plugin distribution.

### Build with DisableStripPluginOutput=true

When you build with the flag, the strip operation is skipped:

```
plugins\StreamSharp.PostgreSQL\bin\Debug\net10.0\
├── plugin.json                          ✓
├── StreamSharp.PostgreSQL.dll           ✓
├── StreamSharp.Core.dll                 ✓
├── StreamSharp.PostgreSQL.deps.json     ✓
├── Migrations\                          ✓ (PRESERVED)
│   ├── 20260508191612_InitialCreate.cs
│   ├── StreamSharpDBModelSnapshot.cs
│   └── ...
├── *.pdb                                ✓
└── ...
```

This is necessary for EF Core CLI tooling to discover and generate migrations.

## Step-by-Step Migration Workflow

### Scenario: Adding a New Read Model Table

You've decided to add a `MediaFiles` read model to track individual media files. Here's the complete workflow:

#### 1. Update the DbContext Model

Edit `plugins\StreamSharp.PostgreSQL\StreamSharpDB.cs`:

```csharp
public class StreamSharpDB : DbContext, IUnitOfWork
{
    // ...existing DbSets...

    public DbSet<MediaFileDto> MediaFiles { get; set; }  // NEW

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ...existing configurations...
        modelBuilder.ApplyConfiguration(new MediaFileDtoConfiguration());  // NEW
    }
}
```

#### 2. Create the DTO Configuration

Create `plugins\StreamSharp.PostgreSQL\Aggregates\MediaFileDtoConfiguration.cs`:

```csharp
public class MediaFileDtoConfiguration : IEntityTypeConfiguration<MediaFileDto>
{
    public void Configure(EntityTypeBuilder<MediaFileDto> builder)
    {
        builder.ToTable("MediaFiles");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnType("uuid");
        builder.Property(m => m.Path).IsRequired();
        builder.Property(m => m.CreatedAt).HasColumnType("timestamp with time zone");
    }
}
```

#### 3. Build with Stripping Disabled

```powershell
dotnet build plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  -c Debug `
  -p:DisableStripPluginOutput=true
```

**Output**:
```
  StreamSharp.PostgreSQL net10.0 succeeded (0,3s) → plugins\StreamSharp.PostgreSQL\bin\Debug\net10.0\StreamSharp.PostgreSQL.dll
Build succeeded in 1,2s
```

#### 4. Create the Migration

```powershell
dotnet ef migrations add AddMediaFilesReadModel `
  -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  --no-build
```

**Output**:
```
Done. To undo this action, use 'ef migrations remove'
```

**Generated Files**:
```
plugins\StreamSharp.PostgreSQL\Migrations\
├── 20260508192000_AddMediaFilesReadModel.cs       (NEW)
├── 20260508192000_AddMediaFilesReadModel.Designer.cs  (NEW)
└── StreamSharpDBModelSnapshot.cs                  (UPDATED)
```

#### 5. Review the Generated Migration

Check `plugins\StreamSharp.PostgreSQL\Migrations\20260508192000_AddMediaFilesReadModel.cs`:

```csharp
public partial class AddMediaFilesReadModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MediaFiles",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Path = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MediaFiles", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "MediaFiles");
    }
}
```

#### 6. Verify and Commit

```powershell
# Ensure the solution still builds
dotnet build

# Verify migrations compile
dotnet ef migrations list -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj

# Commit the migration files to git
git add plugins\StreamSharp.PostgreSQL\Migrations\
git commit -m "Add MediaFiles read model"
```

#### 7. Apply the Migration

The migration is automatically applied on the next server startup:

```powershell
dotnet run --project src\StreamSharp.Server
```

Or manually:

```powershell
dotnet ef database update -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

## Common Tasks

### Listing All Migrations

```powershell
dotnet ef migrations list -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

**Output**:
```
20260508191612_InitialCreate
20260508192000_AddMediaFilesReadModel (Pending)
```

### Reverting to a Previous Migration

```powershell
dotnet ef database update InitialCreate -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

This rolls back all migrations after `InitialCreate`.

### Removing an Unapplied Migration

If a migration hasn't been applied to production, you can discard it:

```powershell
dotnet ef migrations remove -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

Then follow the two-step process to create a corrected migration.

### Generating SQL Without Applying

To see the SQL that would be executed:

```powershell
dotnet ef migrations script -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

Or from a specific migration to another:

```powershell
dotnet ef migrations script InitialCreate AddMediaFilesReadModel `
  -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
```

## Troubleshooting

### "Migrations Folder Missing After Build"

**Symptom**: You created a migration, but after rebuilding the solution, the `Migrations` folder disappeared.

**Cause**: You likely used `dotnet build` without `DisableStripPluginOutput=true`, causing the plugin build to strip migration files.

**Fix**:
1. Restore the migration from git history: `git checkout plugins\StreamSharp.PostgreSQL\Migrations\`
2. Use the correct two-step build process going forward

### "EF CLI Cannot Find StreamSharp.Core"

**Error**:
```
An assembly specified in the application dependencies manifest (StreamSharp.PostgreSQL.deps.json) 
was not found: package: 'StreamSharp.Core', version: '1.0.0', path: 'StreamSharp.Core.dll'
```

**Causes**:
- Running EF without `--no-build` (forces a rebuild that re-strips the output)
- Build output hasn't been updated (stale deps manifest)

**Fix**:
```powershell
# Rebuild with stripping disabled
dotnet build plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  -c Debug `
  -p:DisableStripPluginOutput=true

# Retry EF with --no-build
dotnet ef migrations add <Name> `
  -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
  --no-build
```

### "Unique Constraint Violation on EventDocument"

**Error**: `23505: duplicate key value violates unique constraint "IX_EventDocument_AggregateId_AggregateName_Version"`

**Cause**: The event store prevents duplicate versions of the same aggregate. This indicates a bug in your event publishing or version management.

**Debug Steps**:
1. Check `plugins\StreamSharp.PostgreSQL\StreamSharpDB.cs` — ensure `TrackAggregate()` increments versions correctly
2. Verify event handlers aren't retrying save operations without version conflict handling
3. Review `EventPublishingInterceptor` — ensure events are only captured once

**Fix**:
```sql
-- Check for duplicates
SELECT "AggregateId", "AggregateName", "Version", COUNT(*) as cnt
FROM "EventDocument"
GROUP BY "AggregateId", "AggregateName", "Version"
HAVING COUNT(*) > 1;

-- If found, investigate the aggregate history and remove duplicates
```

### Migration Template Doesn't Include My Model

**Symptom**: You added a DbSet and configuration, but `dotnet ef migrations add` doesn't create the corresponding table.

**Debugging**:
1. Verify the configuration is called in `OnModelCreating()`:
   ```csharp
   modelBuilder.ApplyConfiguration(new MyDtoConfiguration());
   ```

2. Check that the DTO class exists and is public:
   ```csharp
   public class MyDto { /*...*/ }
   ```

3. Ensure the configuration builds without errors:
   ```powershell
   dotnet build plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj
   ```

4. Re-run the migration command with verbose output:
   ```powershell
   dotnet ef migrations add MyMigration `
     -p plugins\StreamSharp.PostgreSQL\StreamSharp.PostgreSQL.csproj `
     --no-build `
     --verbose
   ```

## Best Practices

1. **Always use `--no-build`** on `dotnet ef migrations add` to prevent accidental re-stripping
2. **Commit migration files** to git immediately after creating them
3. **Test migrations** locally before pushing: `dotnet ef database update`
4. **Review generated SQL** with `dotnet ef migrations script` before deployment
5. **Document schema changes** in commit messages for future developers
6. **Don't edit migrations manually** after they've been applied to other environments
7. **Use descriptive migration names** (e.g., `AddMediaFilesReadModel`, not `Update1`)

## Advanced: Manual Migration Creation

In rare cases, you might need to create a migration manually (e.g., raw SQL operations). See [EF Core Custom Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#add-a-raw-sql-migration).

1. Create a file: `Migrations\<timestamp>_<Name>.cs`
2. Implement `Migration.Up()` and `Migration.Down()` with custom SQL
3. Update `StreamSharpDBModelSnapshot.cs` to reflect the new schema state
4. Test: `dotnet ef database update`

This is an escape hatch and should rarely be needed.
