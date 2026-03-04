using StreamSharp.Server.Configuration;

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
