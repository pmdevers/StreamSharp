using StreamSharp.Core;
using StreamSharp.Core.Events;
using StreamSharp.Server;
using StreamSharp.Server.Features;
using StreamSharp.Server.Features.Medialibrary;
using StreamSharp.Server.Features.Medialibrary.Events;
using StreamSharp.Server.Features.Setup;
using static StreamSharp.Server.Features.Medialibrary.CreateLibrary;

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
        builder.Services.AddCore();
    })
    .Use((app) =>
    {
        app.RedirectToBasePath();
        app.HealthChecks();
        app.MapStreamSharpApi();

    }).Build();

await host.StartAsync();


