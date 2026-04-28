using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Queries;
using StreamSharp.PostgreSQL.Projections;
using StreamSharp.PostgreSQL.Queries;

namespace StreamSharp.PostgreSQL;

public static class AddPostgreSQLExtensions
{
    public static void AddPostgreSQL(this IServiceCollection services)
    {
        services.AddScoped<EventPublishingInterceptor>();

        services.AddDbContext<StreamSharpDB>((serviceProvider, options) =>
        {
            var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                ?? throw new InvalidOperationException("POSTGRES_CONNECTION_STRING environment variable is not set.");

            options.UseNpgsql(connectionString)
                   .AddInterceptors(serviceProvider.GetRequiredService<EventPublishingInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<StreamSharpDB>());
        services.AddScoped<ILibraryQueries, LibraryQueries>();

        services.AddMessageHandler<LibraryProjector>();

        services.AddHostedService<MigrationService>();
    }
}

public class MigrationService(IServiceScopeFactory scopeFactory, ILogger<MigrationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StreamSharpDB>();

        logger.LogInformation("Applying pending database migrations...");
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Database migrations applied successfully.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
