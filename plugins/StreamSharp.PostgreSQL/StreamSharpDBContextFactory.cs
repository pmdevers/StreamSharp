using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StreamSharp.PostgreSQL;

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
