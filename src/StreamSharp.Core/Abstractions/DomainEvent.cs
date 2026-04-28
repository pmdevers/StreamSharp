namespace StreamSharp.Core.Abstractions;

public abstract record DomainEvent
{
    public Guid MessageId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; set; }
};
