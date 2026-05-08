using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;
using StreamSharp.Core.Queries;

namespace StreamSharp.Server.Features.Medialibrary;

public static class GetLibraryItems
{
    public static async Task<IResult> Handle(
        [FromServices] ILibraryQueries queries,
        [FromRoute] LibraryId libraryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        CancellationToken cancellationToken = default
    )
    {
        var items = await queries.GetLibraryItemsAsync(libraryId, page, pageSize, search, sortBy, cancellationToken);

        return Results.Ok(items);
    }
}
