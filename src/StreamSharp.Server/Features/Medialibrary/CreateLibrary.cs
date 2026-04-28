using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;

namespace StreamSharp.Server.Features.Medialibrary;

public static class CreateLibrary
{
    public record CreateLibraryRequest(string Name);

    public static async Task<IResult> Handle(
        [FromServices] IUnitOfWork uow,
        [FromBody] CreateLibraryRequest request,
        CancellationToken cancellationToken)
    {
        var repo = uow.GetRepository<Library, LibraryId>();

        var library = Library.Create(request.Name);

        repo.Add(library);

        await uow.SaveChangesAsync(cancellationToken);

        return Results.Ok(library);
    }
}
