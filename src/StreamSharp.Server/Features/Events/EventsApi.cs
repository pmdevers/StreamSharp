namespace StreamSharp.Server.Features.Events;

public static class EventsApi
{
    public static void MapEventsApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/events").WithTags("Events");

        group.MapPost("/republish", RepublishAllEvents.Handle)
            .WithName("RepublishAllEvents")
            .WithDescription("Republishes all events to the event bus. Useful for rebuilding projections or debugging event handlers.");
    }
}
