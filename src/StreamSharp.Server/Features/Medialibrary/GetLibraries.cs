using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Queries;

namespace StreamSharp.Server.Features.Medialibrary;

public static class Getlibraries
{
    public static async Task<IResult> Handle(
        [FromServices] ILibraryQueries libraryQueries,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null
        )
    {
        var libraries = await libraryQueries.GetLibrariesAsync(page, pageSize, search, sortBy, cancellationToken);

        return TypedResults.Ok(libraries);
    }
}
