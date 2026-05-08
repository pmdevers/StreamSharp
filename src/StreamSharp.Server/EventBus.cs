using StreamSharp.Core.Abstractions;
using System.Threading.Channels;

namespace StreamSharp.Server;

public static class EventBusExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEventBus()
            => services.AddWhenNotRegisterd<IEventBus>(s => {
                s.AddSingleton<MessageQueue>();
                s.AddSingleton<IEventBus, EventBus>();
                s.AddSingleton<IHostedService, EventBusBackgroundService>();
            });
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

public class EventBusBackgroundService(MessageQueue queue, IServiceScopeFactory scopeFactory, ILogger<EventBusBackgroundService> logger) : BackgroundService
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
                logger.LogError(ex, "Error handling message {Message}", message.MessageId);
            }
        }
    }
}
