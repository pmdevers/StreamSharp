namespace StreamSharp.Core.Abstractions;

public interface IEventBus
{
    ValueTask PublishAsync<T>(T message, CancellationToken ct = default)
        where T : DomainEvent;
}

public static class EventBusExtensions
{
    public static async ValueTask PublishAsync<T>(this IEventBus bus, IEnumerable<T> messages, CancellationToken ct = default)
        where T : DomainEvent
    {

        foreach (var message in messages)
        {
            await bus.PublishAsync(message, ct);
        }
    }
}

public delegate Task DomainEventHandler<in T>(T @event, CancellationToken ct = default)
    where T : DomainEvent;
