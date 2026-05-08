using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StreamSharp.PostgreSQL;

/// <summary>
/// Design-time DbContext factory for Entity Framework Core tools and CLI commands.
/// 
/// This factory is used by EF Core when running migration commands (add, apply, etc.)
/// and other tooling that needs to work with the database schema at design time.
/// It allows EF CLI to create a StreamSharpDB instance without a running application
/// and its dependency injection container.
/// 
/// Used by: dotnet ef migrations add/update, Visual Studio designer, etc.
/// NOT used at runtime (dependency injection container is used instead).
/// </summary>
public class StreamSharpDBContextFactory : IDesignTimeDbContextFactory<StreamSharpDB>
{
    public StreamSharpDB CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StreamSharpDB>()
            .UseNpgsql(
                Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                ?? "Host=localhost;Database=streamsharp;Username=postgres;Password=postgres")
            .Options;

        return new StreamSharpDB(options);
    }
}
