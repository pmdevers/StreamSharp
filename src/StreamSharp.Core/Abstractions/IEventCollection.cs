namespace StreamSharp.Core.Abstractions;

/// <summary>
/// Interface for an append-only event collection
/// </summary>
/// <typeparam name="TId">The Type of the aggregateId</typeparam>
public interface IEventCollection<TId> : IReadOnlyCollection<DomainEvent>
    where TId : struct
{
    /// <summary>
    /// The Id of the aggregate
    /// </summary>
    TId AggregateId { get; }

    /// <summary>
    /// The version of the aggregate
    /// </summary>
    int Version { get; }

    /// <summary>
    /// The expected version after save.
    /// </summary>
    int ExpectedVersion { get; }

    /// <summary>
    /// The creation date of this event stream.
    /// </summary>
    DateTimeOffset? CreatedOn { get; }

    /// <summary>
    /// The date of the last modification.
    /// </summary>
    DateTimeOffset? LastModifiedOn { get; }

    /// <summary>
    /// Append a event to the collection.
    /// </summary>
    /// <param name="e"></param>
    void Append(DomainEvent e);

    IEventCollection<TId> GetUncommittedEvents();
}
