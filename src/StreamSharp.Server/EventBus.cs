using StreamSharp.Core.Abstractions;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace StreamSharp.Server;

public delegate Task DomainEventHandler<in T>(T message, CancellationToken cancellationToken) 
    where T : DomainEvent;

public static class EventBusExtensions
{
    extension(IServiceCollection services)
    {

        public IServiceCollection AddEventBus()
        {
            services.AddSingleton<MessageQueue>();
            services.AddSingleton<IEventBus, EventBus>();
            services.AddHostedService<EventBusBackgroundService>();

            return services;
        }

        public IServiceCollection AddMessageHandler<T>(DomainEventHandler<T> handler)
            where T : DomainEvent
        {
            services.AddSingleton(handler);
            return services;
        }

        public IServiceCollection AddMessageHandler<THandler>()
            where THandler : class
        {
            var handlerType = typeof(THandler);

            var methods = handlerType
                .GetMethods()
                .Where(m =>
                {
                    if (m.ReturnType != typeof(Task))
                        return false;
                    var parameters = m.GetParameters();
                    return parameters.Length == 2
                        && typeof(DomainEvent).IsAssignableFrom(parameters[0].ParameterType)
                        && parameters[1].ParameterType == typeof(CancellationToken);
                })
                .ToArray();

            if (methods.Length == 0)
                throw new InvalidOperationException(
                    $"{handlerType.Name} has no methods matching the DomainEventHandler delegate signature " +
                    $"'Task MethodName(T message, CancellationToken ct) where T : DomainEvent'.");

            services.AddTransient<THandler>();

            foreach (var method in methods)
            {
                var eventType = method.GetParameters()[0].ParameterType;
                var delegateType = typeof(DomainEventHandler<>).MakeGenericType(eventType);

                services.AddTransient(delegateType, sp =>
                {
                    var instance = sp.GetRequiredService<THandler>();
                    return Delegate.CreateDelegate(delegateType, instance, method);
                });
            }

            return services;
        }
    }
}

public class MessageQueue
{
    private readonly Channel<DomainEvent> _channel = Channel.CreateUnbounded<DomainEvent>();
    public ChannelReader<DomainEvent> Reader => _channel.Reader;
    public ChannelWriter<DomainEvent> Writer => _channel.Writer;
}

public class EventBus(MessageQueue queue) : IEventBus
{
    public ValueTask PublishAsync<T>(T message, CancellationToken ct = default)
        where T : DomainEvent
    {
        return queue.Writer.WriteAsync(message, ct);
    }
}

public class EventBusBackgroundService(MessageQueue queue, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {

                using var scope = scopeFactory.CreateScope();
                var handlerType = typeof(DomainEventHandler<>).MakeGenericType(message.GetType());
                var handlers = scope.ServiceProvider.GetServices(handlerType);

                foreach (var h in handlers)
                {
                    if (h is Delegate del)
                    {
                        var task = (Task)del.DynamicInvoke(message, stoppingToken)!;
                        await task;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling message {message.MessageId}: {ex}");
            }
        }
    }
}


public class EventStreamStore<TId>(IEventBus eventBus) : IEventStore<TId>
    where TId : struct
{
    private readonly ConcurrentDictionary<TId, List<DomainEvent>> _store = new();

    public Task<EventStream<TId>> LoadAsync(TId id, CancellationToken cancellationToken = default)
    {
        if(_store.TryGetValue(id, out var events))
        {
            return Task.FromResult(EventStream<TId>.Create(id, [.. events]));
        }
        
        return Task.FromResult(EventStream<TId>.Create(id));
    }

    public async Task<EventStream<TId>> SaveAsync(EventStream<TId> streamEvents, CancellationToken cancellationToken = default)
    {
        var uncommitted = streamEvents.GetUncommittedEvents();
        var events = _store.AddOrUpdate(streamEvents.Id, [.. streamEvents], (_, existing) =>
        {
            existing.AddRange(uncommitted);
            return existing;
        });

        foreach (var e in uncommitted)
        {
            await eventBus.PublishAsync(e, cancellationToken);
        }

        return EventStream<TId>.Create(streamEvents.Id, [.. events]);
    }
}
