using Microsoft.AspNetCore.Mvc;
using StreamSharp.Plugin;

namespace StreamSharp.DummyPlugin;

public class DummyPlugin : IPlugin
{
    public string Name => "dummy";
    public string Description => "A simple dummy plugin to test assembly loading";

    public void Start(IPluginContext context)
    {
        context.RegisterServices(services =>
        {
            services.AddSingleton<TestService>();
        });

        context.RegisterApi(endpoints =>
        {
            endpoints.MapGet("/dummy/test", DummyEndpoint.ResultAsync)
                .WithName("DummyTest")
                .WithTags("Dummy Plugin");
        });
    }

    public void Stop()
    {

    }
}

public static class DummyEndpoint
{
    public static async Task<IResult> ResultAsync([FromServices] TestService service)
    {
        return TypedResults.Ok(service.GetTest());
    }
}

public class TestService
{
    public string GetTest() => "test";
}
