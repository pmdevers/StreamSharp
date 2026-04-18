using Microsoft.AspNetCore.Mvc;

namespace StreamSharp.Server.Features.Medialibrary;

public record GetLibrariesRequest(
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 10,
    [FromQuery] string? Search = null,
    [FromQuery] string? SortBy = null);

public static class Getlibraries
{
    public static async Task<IResult> Handle(
        [FromServices] ILibraryStore store,
        GetLibrariesRequest request
        )
    {
        var libraries = await store.GetLibrariesAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.SortBy
        );

        return TypedResults.Ok(libraries.Select(x => new { LibraryId = x.Id, x.Name }));
    }
}
