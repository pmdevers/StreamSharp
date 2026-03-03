using Scalar.AspNetCore;
using StreamSharp.Server.Configuration;
using StreamSharp.Server.Features.Plugins;

namespace StreamSharp.Server;

public class ServerHostBuilder()
{
    private readonly StreamSharpOptions _options = [];

    private Action<WebApplicationBuilder>? _configure;
    private Action<WebApplication>? _use;

    public ServerHostBuilder WithOptions(Action<StreamSharpOptions> options)
    {
        options(_options);
        return this;
    }
    public ServerHostBuilder Configure(Action<WebApplicationBuilder> configure)
    {
        _configure = configure;
        return this;
    }
    public ServerHostBuilder Use(Action<WebApplication> use)
    {
        _use = use;
        return this;
    }
    public ServerHost Build() => new(_options, _configure, _use);
}

public sealed class ServerHost(
    StreamSharpOptions options,
    Action<WebApplicationBuilder>? configure = null,
    Action<WebApplication>? use = null
) : IAsyncDisposable
{
    private readonly CancellationTokenSource _serverToken = new();
    private readonly PluginManager _pluginManager = new(options.PluginsRoot);

    private CancellationTokenSource? _cts;

    private Task? _runningTask;

    public StreamSharpOptions Options => options;
    public PluginManager PluginManager => _pluginManager;

    public static ServerHostBuilder CreateBuilder()
        => new();

    public async ValueTask DisposeAsync()
    {
        await _serverToken.CancelAsync();

        if (_runningTask is not null)
        {
            try
            {
                await _runningTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }

        _serverToken.Dispose();
    }

    public Task StartAsync()
    {
        _runningTask = RunServerLoopAsync();
        return _runningTask;
    }

    public Task StopAsync()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        return Task.CompletedTask;
    }

    public Task ServerStopAsync()
    {
        _serverToken?.Cancel();
        _serverToken?.Dispose();
        return Task.CompletedTask;
    }

    private async Task RunServerLoopAsync()
    {
        // Load plugins once, outside the restart loop
        if (options.LoadPlugins && !_pluginManager.GetPlugins().Any())
        {
            _pluginManager.LoadAll();
        }

        do
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(_serverToken.Token);
            WebApplication? app = null;
            try
            {
                // Clear restart flag at the beginning of each server start
                _pluginManager.ClearRestartFlag();

                var builder = WebApplication.CreateBuilder();
                builder.Services.AddOpenApi(options =>
                {
                    options.AddDocumentTransformer((document, _, _) =>
                    {
                        document.Info.Title = "StreamSharp API";
                        document.Info.Version = "v1";
                        document.Info.Description = "Media Streaming API";
                        document.Info.Contact = new() { Name = "Support", Email = "support@StreamSharp.com" };
                        document.Info.License = new() { Name = "MIT", Url = new("https://opensource.org/licenses/MIT") };
                        return Task.CompletedTask;
                    });
                });
                builder.Services.AddSingleton(options);
                builder.Services.AddSingleton(_pluginManager);
                builder.Services.AddSingleton(this);

                // Apply plugin services
                _pluginManager.ApplyServicesToBuilder(builder.Services);

                configure?.Invoke(builder);
                app = builder.Build();


                app.MapOpenApi();
                app.MapScalarApiReference();

                // Apply plugin endpoints
                _pluginManager.ApplyEndpointsToApp(app);

                use?.Invoke(app);
                await app.RunAsync(_cts.Token);
            }
            catch (Exception ex) when (options.EnableRestartOnFailure && !_cts.IsCancellationRequested)
            {
                Console.WriteLine($"ServerHost encountered an error: {ex.Message}");
                Console.WriteLine("Attempting to restart the server...");

                await Task.Delay(1000, _serverToken.Token);
            }
        } while (options.EnableRestartOnFailure && !_serverToken.IsCancellationRequested);
    }
}
