using StreamSharp.Core.Entities;
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
    public static TAggregate Create<TAggregate, TId>(TId aggregateId)
        where TAggregate : AggregateRoot<TId>
        where TId : struct
    {
        var methodName = "Create";
        var type = typeof(TAggregate);
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, new[] { typeof(TId) });

        if (method == null)
        {
            var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, new[] { typeof(TId) });

            if (constructor == null)
            {
                try
                {
                    return Activator.CreateInstance<TAggregate>();
                }
                catch (MissingMethodException e)
                {
                    // catch if parameterles constructor is not found.
                }
            }

            var result = constructor?.Invoke(new object[] { aggregateId });
            if (result is TAggregate returnValue) return returnValue;
        }
        else
        {
            var result = method.Invoke(null, new object[] { aggregateId });
            if (result is TAggregate returnValue) return returnValue;
        }

        throw new NotImplementedException($"The aggregate does not have a public static method 'public static {type.Name} {methodName}({typeof(TId).Name} id)'.");
    }

    /// <summary>
    /// Loads an aggregate of <typeparamref name="TAggregate"/> from a <see cref="EventCollection{TId}" />.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="events">The stream of events.</param>
    /// <returns>A instance of <typeparamref name="TAggregate"/>.</returns>
    public static TAggregate LoadFromHistory<TAggregate, TId>(EventCollection<TId> events)
        where TAggregate : AggregateRoot<TId>
        where TId : struct
    {
        var aggregate = Create<TAggregate, TId>(events.AggregateId);
        aggregate.LoadFromHistory(events);
        return aggregate;
    }
}

/// <summary>
/// Base class for an AggregateRoot.
/// </summary>
/// <typeparam name="TId">The type of the Identifier of the aggregate.</typeparam>
public abstract class AggregateRoot<TId>
    where TId : struct
{
    private IEventCollection<TId> _events;

    /// <summary>
    /// The identifier of this aggregate.
    /// </summary>
    public TId Id => _events.AggregateId;

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

    protected TimeProvider TimeProvider { get; }

    /// <summary>
    /// Constructor to instanciate a new instance. />.
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected AggregateRoot(TId id, TimeProvider? timeProvider = null)
    {
        TimeProvider = timeProvider ?? TimeProvider.System;
        _events = EventCollection<TId>.Create(id);
    }

    /// <summary>
    /// Refresh aggregate from an event stream. 
    /// </summary>
    /// <param name="events">a event collection.</param>
    public void LoadFromHistory(IEventCollection<TId> events)
    {
        _events = events;
        foreach (var e in _events)
        {
            RecordEvent(e, false);
        }
    }

    public IEventCollection<TId> GetUncommittedEvents()
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
        AggregateRoot<TId>.SafeInvokeMethod(GetType(), this, _applyMehodName, e);
    }

    private static void SafeInvokeMethod(Type type, object target, string name, params object[] args)
    {
        const BindingFlags privateOrPublicMethodFlags = BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        try
        {
            type.InvokeMember(name, privateOrPublicMethodFlags, null, target, args);
        }
        catch (MissingMethodException)
        {
            if (type.BaseType != null)
            {
                AggregateRoot<TId>.SafeInvokeMethod(type.BaseType, target, name, args);
            }
        }
    }
}


/// <summary>
/// A Append only EventCollection
/// </summary>
public static class EventCollection
{
    /// <summary>
    /// Creates an instance of an EventCollection
    /// </summary>
    /// <param name="aggregateId">The id of the aggregate</param>
    /// <param name="events">Array of events</param>
    /// <returns>Returns an event collection </returns>
    public static EventCollection<TId> Create<TId>(TId aggregateId, DomainEvent[]? events = null)
        where TId : struct
        => EventCollection<TId>.Create(aggregateId, events);
}

/// <summary>
/// A Append only EventCollection
/// </summary>
/// <typeparam name="TId">The Type of the aggregateId</typeparam>
public sealed class EventCollection<TId> : IEventCollection<TId>
    where TId : struct
{
    private DomainEvent[] _events = Array.Empty<DomainEvent>();
    private readonly List<DomainEvent> _uncomitted = new();

    /// <summary>
    /// The Id of the aggregate
    /// </summary>
    public TId AggregateId { get; }
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
    internal static EventCollection<TId> Create(TId aggregateId, DomainEvent[]? events = null)
        => new(aggregateId)
        {
            _events = events ?? Array.Empty<DomainEvent>(),
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
    public IEventCollection<TId> GetUncommittedEvents()
        => Create(AggregateId, _uncomitted.ToArray());

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

    private EventCollection(TId aggregateId)
    {
        AggregateId = aggregateId;
    }
}
