namespace StreamSharp.Server.Modeling;

/// <summary>
/// Base class for events.
/// </summary>
public abstract record EventRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for the event stream.
    /// </summary>
    /// <remarks>This property is essential for distinguishing and managing individual event streams within
    /// the application. Ensure that a valid identifier is assigned before processing events associated with a
    /// stream.</remarks>
    public EventStreamId StreamId { get; set; }

    /// <summary>
    /// The DateTimeOffset of when this event happend.
    /// </summary>
    public DateTimeOffset OccouredOn { get; set; } = DateTimeOffset.UtcNow;
}
