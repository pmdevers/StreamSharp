//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;

//namespace StreamSharp.PostgresSQL;

//public static class AddPostgreSQLExtensions
//{
//    public static void AddPostgreSQL(this IServiceCollection services)
//    {
//        services.AddDbContext<SharpStreamDBContext>(options =>
//        {
//            var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
//                ?? throw new InvalidOperationException("POSTGRES_CONNECTION_STRING environment variable is not set.");
//            options.UseNpgsql(connectionString);
//        });

//        services.AddTransient<IEventStore>(sp => sp.GetRequiredService<SharpStreamDBContext>());
//    }
//}
