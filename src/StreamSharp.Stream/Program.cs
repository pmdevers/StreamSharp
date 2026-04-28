var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

await app.RunAsync();
