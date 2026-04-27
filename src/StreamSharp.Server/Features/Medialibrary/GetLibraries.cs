using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Entities;
using StreamSharp.Core.Storage;

namespace StreamSharp.Server.Features.Medialibrary;

public record GetLibrariesRequest(
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 10,
    [FromQuery] string? Search = null,
    [FromQuery] string? SortBy = null);

public static class Getlibraries
{
    public static async Task<IResult> Handle(
        [FromServices] LibraryItemRepository store,
        [FromBody] GetLibrariesRequest request
        )
    {
        var libraries = Array.Empty<Library>();

        return TypedResults.Ok(libraries.Select(x => new { LibraryId = x.Id, x.Name }));
    }
}
