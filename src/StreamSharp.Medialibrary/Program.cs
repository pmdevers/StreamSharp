using StreamSharp.Medialibrary.Features;
using StreamSharp.Server;
using StreamSharp.Server.Features;

var host = ServerHost.CreateBuilder()
    .WithOptions(x =>
    {
        x.BasePath = "/api";
        x.EnableRestartOnFailure = false;
        x.LibraryPath = "/movies";
    })
    .Use((app) =>
    {
        app.RedirectToBasePath();
        app.HealthChecks();
        app.MapMedialibrary();

    }).Build();

await host.StartAsync();
