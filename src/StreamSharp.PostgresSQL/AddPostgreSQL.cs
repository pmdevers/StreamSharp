using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Events;
using StreamSharp.Server;

namespace StreamSharp.PostgresSQL;

public static class AddPostgreSQLExtensions
{
    public static void AddPostgreSQL(this IServiceCollection services)
    {
        services.AddDbContext<StreamSharpDB>(options =>
        {
            var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
                ?? throw new InvalidOperationException("POSTGRES_CONNECTION_STRING environment variable is not set.");
            options.UseNpgsql(connectionString);
        });

        services.AddTransient(typeof(IEventStore<>), typeof(EventStreamStore<>));
    }
}
