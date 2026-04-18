namespace StreamSharp.Core.Abstractions;

/// <summary>
/// An append only EventStream
/// </summary>
/// <typeparam name="TId">The Type of the aggregateId</typeparam>
[GenerateId]
public sealed class EventStream<TId> : IReadOnlyCollection<StreamEvent>
    where TId : struct
{
    private StreamEvent[] _events = [];
    private readonly List<StreamEvent> _uncomitted = [];
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// The Id of the stream
    /// </summary>
    public TId Id { get; }
    /// <summary>
    /// The version of the aggregate
    /// </summary>
    public int Version
        => _events.Length;
    /// <summary>
    /// The expected version after save.
    /// </summary>
    public int ExpectedVersion
        => Version + _uncomitted.Count;

    /// <summary>
    /// The creation date of this event stream.
    /// </summary>
    public DateTimeOffset? CreatedOn
        => _events.Length != 0
        ? _events.FirstOrDefault()?.OccouredOn
        : _uncomitted.FirstOrDefault()?.OccouredOn;

    /// <summary>
    /// The date of the last modification.
    /// </summary>
    public DateTimeOffset? LastModifiedOn
        => _uncomitted.Count != 0
        ? _uncomitted.LastOrDefault()?.OccouredOn
        : _events.LastOrDefault()?.OccouredOn;

    /// <summary>
    /// The total number event comitted.
    /// </summary>
    public int Count
        => _events.Length;

    /// <summary>
    /// Creates an instance of an EventCollection
    /// </summary>
    /// <param name="id">The id of the aggregate</param>
    /// <param name="events">Array of events</param>
    /// <returns>Returns an event collection </returns>
    public static EventStream<TId> Create(TId id, StreamEvent[]? events = null, TimeProvider? timeProvider = null)
        => new(id, timeProvider ?? TimeProvider.System)
        {
            _events = events ?? [],
        };

    /// <summary>
    /// Append a event to the collection.
    /// </summary>
    /// <param name="e"></param>
    public void Append(StreamEvent e)
    {
        e.OccouredOn = _timeProvider.GetUtcNow();
        _uncomitted.Add(e);
    }

    /// <summary>
    /// Get the uncommitted events
    /// </summary>
    /// <returns>Events that are uncommitted.</returns>
    public EventStream<TId> GetUncommittedEvents()
        => Create(Id, [.. _uncomitted]);

    /// <summary>
    /// Iterates over the comitted events
    /// </summary>
    /// <returns>Events that are committed</returns>
    public IEnumerator<StreamEvent> GetEnumerator()
        => _events.ToList().GetEnumerator();

    /// <summary>
    /// Iterates over the comitted events
    /// </summary>
    /// <returns>Events that are committed</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    private EventStream(TId id, TimeProvider timeProvider)
    {
        Id = id;
        _timeProvider = timeProvider;
    }
}
