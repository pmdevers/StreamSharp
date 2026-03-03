using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Medialibrary.Api;

public static class Getlibraries
{
    public static async Task<IResult> Handle(
        [FromServices] MedialibraryManager manager
        )
    {
        var libraries = await manager.FindLibraries();
        return TypedResults.Ok(libraries.Select(x => new { x.Id, x.Name }));
    }
}
