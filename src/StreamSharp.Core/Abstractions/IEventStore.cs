using StreamSharp.Core.Abstractions;

namespace StreamSharp.Core.Abstractions;

/// <summary>
/// Abstraction for event store operations.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Retrieves all events from the event store, ordered by creation date.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of all domain events.</returns>
    Task<IReadOnlyList<DomainEvent>> GetAllEventsAsync(CancellationToken cancellationToken = default);
}
