using StreamSharp.Server;
using StreamSharp.Server.Features;
using StreamSharp.Server.Features.Medialibrary;
using StreamSharp.Server.Features.Setup;

if (false)
{
    var setup = ServerHost.CreateBuilder()
        .WithOptions(x =>
        {
            x.EnableRestartOnFailure = false;
            x.LoadPlugins = false;
        })
        .Use((app) =>
        {
            app.RedirectToBasePath();
            app.HealthChecks();
            app.MapSetupApi();
        }).Build();

    await setup.StartAsync();
}

var host = ServerHost.CreateBuilder()
    .WithOptions(x =>
    {
        x.PluginsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
    })
    .Configure((builder) =>
    {
        builder.Services.AddMedialibraryApi();
    })
    .Use((app) =>
    {
        app.RedirectToBasePath();
        app.HealthChecks();
        app.MapStreamSharpApi();

    }).Build();

await host.StartAsync();
