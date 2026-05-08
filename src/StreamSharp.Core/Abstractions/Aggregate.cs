using System.Reflection;

namespace StreamSharp.Core.Abstractions;

public static class AggregateRoot
{
    /// <summary>
    /// Creates a AggregateRoot.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">Throw an exception if the method is not implemented.</exception>
    public static TAggregate Create<TAggregate>(AggregateId aggregateId)
        where TAggregate : Aggregate
    {
        var methodName = "Create";
        var type = typeof(TAggregate);
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, [typeof(AggregateId)]);

        if (method == null)
        {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, [typeof(AggregateId)]);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            if (constructor == null)
            {
                try
                {
                    return Activator.CreateInstance<TAggregate>();
                }
                catch (MissingMethodException)
                {
                    // catch if parameterles constructor is not found.
                }
            }

            var result = constructor?.Invoke([aggregateId]);
            if (result is TAggregate returnValue) return returnValue;
        }
        else
        {
            var result = method.Invoke(null, [aggregateId]);
            if (result is TAggregate returnValue) return returnValue;
        }

        throw new NotImplementedException($"The aggregate does not have a public static method 'public static {type.Name} {methodName}({typeof(AggregateId).Name} id)'.");
    }

    /// <summary>
    /// Loads an aggregate of <typeparamref name="TAggregate"/> from a <see cref="EventCollection{TId}" />.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="events">The stream of events.</param>
    /// <returns>A instance of <typeparamref name="TAggregate"/>.</returns>
    public static TAggregate LoadFromHistory<TAggregate>(EventCollection events)
        where TAggregate : Aggregate
    {
        var aggregate = Create<TAggregate>(events.AggregateId);
        aggregate.LoadFromHistory(events);
        return aggregate;
    }
}

/// <summary>
/// Base class for an AggregateRoot.
/// </summary>
/// <typeparam name="TId">The type of the Identifier of the aggregate.</typeparam>
/// <remarks>
/// Constructor to instanciate a new instance. />.
/// </remarks>
/// <param name="id">The identifier.</param>
[GenerateId]
public abstract class Aggregate(AggregateId id, TimeProvider? timeProvider = null)
{
    private EventCollection _events = EventCollection.Create(id);

    /// <summary>
    /// The identifier of this aggregate.
    /// </summary>
    public AggregateId Id => _events.AggregateId;

    /// <summary>
    /// The version of the aggregate root.
    /// </summary>
    public int Version => _events.Version;

    /// <summary>
    /// The Expected verion after saving the aggregate.
    /// </summary>
    public int ExpectedVersion => _events.ExpectedVersion;

    /// <summary>
    /// The date the aggregate was created.
    /// </summary>
    public DateTimeOffset? CreatedOn => _events.CreatedOn;

    /// <summary>
    /// The Date the aggregate was last modified.
    /// </summary>
    public DateTimeOffset? LastModifiedOn => _events.LastModifiedOn;

    protected TimeProvider TimeProvider { get; } = timeProvider ?? TimeProvider.System;

    /// <summary>
    /// Refresh aggregate from an event stream. 
    /// </summary>
    /// <param name="events">a event collection.</param>
    public void LoadFromHistory(EventCollection events)
    {
        _events = events;
        foreach (var e in _events)
        {
            RecordEvent(e, false);
        }
    }

    public EventCollection GetUncommittedEvents()
        => _events.GetUncommittedEvents();

    /// <summary>
    /// Adds and applies an event to the aggregate.
    /// </summary>
    /// <param name="e"></param>
    protected internal void RecordEvent(DomainEvent e)
    {
        ArgumentNullException.ThrowIfNull(e);
        RecordEvent(e with { OccurredOn = TimeProvider.GetUtcNow() }, true);
    }

    private void RecordEvent(DomainEvent e, bool isNew)
    {
        ApplyInternal(e);
        if (isNew)
        {
            _events.Append(e);
        }
    }

    private const string _applyMehodName = "Apply";
    private void ApplyInternal(DomainEvent e)
    {
        Aggregate.SafeInvokeMethod(GetType(), this, _applyMehodName, e);
    }

    private static void SafeInvokeMethod(Type type, object target, string name, params object[] args)
    {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        const BindingFlags privateOrPublicMethodFlags = BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
        try
        {
            type.InvokeMember(name, privateOrPublicMethodFlags, null, target, args);
        }
        catch (MissingMethodException)
        {
            if (type.BaseType != null)
            {
                Aggregate.SafeInvokeMethod(type.BaseType, target, name, args);
            }
        }
    }
}


/// <summary>
/// A Append only EventCollection
/// </summary>
/// <typeparam name="TId">The Type of the aggregateId</typeparam>
public sealed class EventCollection : IEnumerable<DomainEvent>
{
    private DomainEvent[] _events = [];
    private readonly List<DomainEvent> _uncomitted = [];

    /// <summary>
    /// The Id of the aggregate
    /// </summary>
    public AggregateId AggregateId { get; }
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
        ? _events.FirstOrDefault()?.OccurredOn
        : _uncomitted.FirstOrDefault()?.OccurredOn;

    /// <summary>
    /// The date of the last modification.
    /// </summary>
    public DateTimeOffset? LastModifiedOn
        => _uncomitted.Count != 0
        ? _uncomitted.LastOrDefault()?.OccurredOn
        : _events.LastOrDefault()?.OccurredOn;

    /// <summary>
    /// The total number event comitted.
    /// </summary>
    public int Count
        => _events.Length;

    /// <summary>
    /// Creates an instance of an EventCollection
    /// </summary>
    /// <param name="aggregateId">The id of the aggregate</param>
    /// <param name="events">Array of events</param>
    /// <returns>Returns an event collection </returns>
    public static EventCollection Create(AggregateId aggregateId, DomainEvent[]? events = null)
        => new(aggregateId)
        {
            _events = events ?? [],
        };

    /// <summary>
    /// Append a event to the collection.
    /// </summary>
    /// <param name="e"></param>
    public void Append(DomainEvent e)
    {
        _uncomitted.Add(e);
    }

    /// <summary>
    /// Get the uncommitted events
    /// </summary>
    /// <returns>Events that are uncommitted.</returns>
    public EventCollection GetUncommittedEvents()
        => Create(AggregateId, [.. _uncomitted]);

    /// <summary>
    /// Iterates over the comitted events
    /// </summary>
    /// <returns>Events that are committed</returns>
    public IEnumerator<DomainEvent> GetEnumerator()
        => _events.ToList().GetEnumerator();

    /// <summary>
    /// Iterates over the comitted events
    /// </summary>
    /// <returns>Events that are committed</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => GetEnumerator();

    private EventCollection(AggregateId aggregateId)
    {
        AggregateId = aggregateId;
    }
}
