using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Abstractions;
using StreamSharp.Core.Entities;
using StreamSharp.Server.Features.Medialibrary.Events;

namespace StreamSharp.Server.Features.Medialibrary;

public static class CreateLibrary
{
    public record CreateLibraryRequest(string Name);

    public static async Task<IResult> Handle(
        [FromServices] IEventStore<LibraryId> eventStore,
        [FromServices] IEventBus eventBus,
        [FromBody] CreateLibraryRequest request)
    {
        var stream = EventStream<LibraryId>.Create(LibraryId.New(), []);

        stream.Append(new LibraryCreatedEvent(request.Name));

        await eventStore.SaveAsync(stream);

        return TypedResults.Created($"/library/{stream.Id}", new { LibraryId = stream.Id, request.Name });
    }
}
