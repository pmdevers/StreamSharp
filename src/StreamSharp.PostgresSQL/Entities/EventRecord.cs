namespace StreamSharp.PostgresSQL.Entities;

public class EventRecord
{
    public Guid EventId { get; set; }
    public string StreamId { get; set; } = string.Empty;
    public int Version { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTimeOffset OccurredOn { get; set; }
}
