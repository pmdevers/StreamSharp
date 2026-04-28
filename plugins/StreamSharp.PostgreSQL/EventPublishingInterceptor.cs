using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using StreamSharp.Core.Abstractions;
using StreamSharp.PostgreSQL.Aggregates;

namespace StreamSharp.PostgreSQL;

/// <summary>
/// EF Core interceptor that publishes domain events to the event bus after they are saved to the database
/// </summary>
public class EventPublishingInterceptor : SaveChangesInterceptor
{
    private readonly IEventBus? _eventBus;
    private readonly ILogger<EventPublishingInterceptor> _logger;
    private readonly List<EventDocument> _pendingEvents = [];

    public EventPublishingInterceptor(IEventBus? eventBus, ILogger<EventPublishingInterceptor> logger)
    {
        _eventBus = eventBus;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogDebug("EventPublishingInterceptor instance created");
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        _logger.LogDebug("SavingChanges interceptor called");
        if (eventData.Context != null)
        {
            CaptureAddedEvents(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SavingChangesAsync interceptor called");
        if (eventData.Context != null)
        {
            CaptureAddedEvents(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        _logger.LogDebug("SavedChanges interceptor called, publishing {Count} events", _pendingEvents.Count);
        PublishEventsAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_pendingEvents.Count > 0)
        {
            _logger.LogInformation("Publishing {Count} domain events after SaveChanges", _pendingEvents.Count);
        }
        await PublishEventsAsync(cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureAddedEvents(DbContext context)
    {
        _pendingEvents.Clear();
        var entries = context.ChangeTracker.Entries<EventDocument>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        _pendingEvents.AddRange(entries);
        _logger.LogDebug("Captured {Count} event documents", entries.Count);
    }

    private async Task PublishEventsAsync(CancellationToken cancellationToken)
    {
        if (_eventBus == null)
        {
            if (_pendingEvents.Count > 0)
            {
                _logger.LogWarning("IEventBus not available - event publishing disabled");
            }

            _pendingEvents.Clear();
            return;
        }

        foreach (var eventDoc in _pendingEvents)
        {
            var domainEvent = EventSerializer.Deserialize(eventDoc.Data, eventDoc.Type);
            if (domainEvent != null)
            {
                _logger.LogDebug("Publishing event: {EventType}", eventDoc.Type);
                await _eventBus.PublishAsync(domainEvent, cancellationToken);
            }
        }
        _pendingEvents.Clear();
    }
}
