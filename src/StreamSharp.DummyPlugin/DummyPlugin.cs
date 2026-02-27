using Microsoft.AspNetCore.Mvc;
using StreamSharp.Plugin;

namespace StreamSharp.DummyPlugin;

public class DummyPlugin : IPlugin
{
    public string Name => "dummy";
    public string Description => "A simple dummy plugin to test assembly loading";
    public void Start()
    {

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
