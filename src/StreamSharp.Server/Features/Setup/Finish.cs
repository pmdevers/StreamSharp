using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Setup;

public static class Finish
{
    public static async Task<IResult> Handle([FromServices] ServerHost server)
    {
        await server.StopAsync();
        return Results.Redirect(server.Options.BasePath);
    }
}
