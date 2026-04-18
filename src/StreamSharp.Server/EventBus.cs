using StreamSharp.Core.Abstractions;
using StreamSharp.Server.Features.Medialibrary;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace StreamSharp.Server;

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

        public IServiceCollection AddMessageHandler<T>()
            where T : class
        {
            var type = typeof(T).GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(DomainEventHandler<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault()

                ?? throw new InvalidOperationException("T must be a delegate DomainEventHandler<TMessage>");

            var handlerType = typeof(DomainEventHandler<>).MakeGenericType(type);
            services.AddTransient(handlerType, typeof(T));
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
                var handler = scope.ServiceProvider.GetServices(handlerType);

                foreach (var h in handler)
                {
                    var method = handlerType.GetMethod("HandleAsync");
                    if (method != null)
                    {
                        var task = (Task)method.Invoke(h, [message, stoppingToken])!;
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


public class EventStreamStore<TId> : IEventStore<TId>
    where TId : struct
{
    private readonly ConcurrentDictionary<TId, List<StreamEvent>> _store = new();

    public Task<EventStream<TId>> LoadAsync(TId id, CancellationToken cancellationToken = default)
    {
        var events = _store.GetOrAdd(id, []);

        return Task.FromResult(EventStream<TId>.Create(id, [.. events]));
    }

    public Task<EventStream<TId>> SaveAsync(EventStream<TId> streamEvents, CancellationToken cancellationToken = default)
    {
        var events = _store.AddOrUpdate(streamEvents.Id, [.. streamEvents], (_, existing) =>
        {
            existing.AddRange(streamEvents.GetUncommittedEvents());
            return existing;
        });

        return Task.FromResult(EventStream<TId>.Create(streamEvents.Id, [.. events]));
    }
}
