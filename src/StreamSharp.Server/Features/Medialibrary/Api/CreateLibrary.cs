using Microsoft.AspNetCore.Mvc;
using StreamSharp.Server.Features.Medialibrary.Events;
using StreamSharp.Server.Modeling;

namespace StreamSharp.Server.Features.Medialibrary.Api;

public static class CreateLibrary
{
    public record CreateLibraryRequest(string Name);

    public static async Task<IResult> Handle(
        [FromServices] IEventStreamProvider<LibraryId> manager,
        CreateLibraryRequest request)
    {
        var stream = await manager.GetOrCreateStream(LibraryId.New());

        stream.Append(new LibraryCreated(request.Name));

        await manager.SaveStream(stream);

        return TypedResults.Created($"/library/{stream.Id}", new { stream.Id, request.Name });
    }
}
