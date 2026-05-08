using Microsoft.AspNetCore.Mvc;
using StreamSharp.Core.Abstractions;

namespace StreamSharp.Server.Features.Events;

public static class RepublishAllEvents
{
    public record RepublishResponse(int EventsPublished);

    public static async Task<IResult> Handle(
        [FromServices] IEventBus eventBus,
        [FromServices] IEventStore eventStore,
        CancellationToken cancellationToken)
    {
        var eventDocuments = await eventStore.GetAllEventsAsync(cancellationToken);

        var publishedCount = 0;

        foreach (var domainEvent in eventDocuments)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
            publishedCount++;
        }

        return Results.Ok(new RepublishResponse(publishedCount));
    }
}
