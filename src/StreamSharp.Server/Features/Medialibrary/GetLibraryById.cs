using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;
using StreamSharp.Core.Queries;

namespace StreamSharp.Server.Features.Medialibrary;

public static class GetLibraryById
{
    public static async Task<IResult> Handle(
        [FromServices] ILibraryQueries libraryQueries,
        [FromRoute] LibraryId libraryId,
        CancellationToken cancellationToken)
    {
        var library = await libraryQueries.GetLibraryByIdAsync(libraryId, cancellationToken);

        return TypedResults.Ok(library);
    }
}
