namespace StreamSharp.Core.Abstractions;

public interface IEventBus
{
    ValueTask PublishAsync<T>(T message, CancellationToken ct = default)
        where T : DomainEvent;
}

public abstract record DomainEvent
{
    public Guid MessageId { get; } = Guid.NewGuid();
};

public delegate Task DomainEventHandler<in T>(T @event, CancellationToken ct = default)
    where T : DomainEvent;
