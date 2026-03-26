using System.Threading.Channels;

namespace StreamSharp.Server;

public abstract record Message
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
}


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
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .Select(i => i.GetGenericArguments()[0])
                .FirstOrDefault()

                ?? throw new InvalidOperationException("T must be a class that implements IMessageHandler<TMessage> where TMessage : Message");

            var handlerType = typeof(IMessageHandler<>).MakeGenericType(type);
            services.AddTransient(handlerType, typeof(T));
            return services;
        }
    }
}

public interface IEventBus
{
    ValueTask PublishAsync<T>(T message, CancellationToken ct = default)
        where T : Message;
}

public class MessageQueue
{
    private readonly Channel<Message> _channel = Channel.CreateUnbounded<Message>();
    public ChannelReader<Message> Reader => _channel.Reader;
    public ChannelWriter<Message> Writer => _channel.Writer;
}

public class EventBus(MessageQueue queue) : IEventBus
{
    public ValueTask PublishAsync<T>(T message, CancellationToken ct = default)
        where T : Message
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
                var handlerType = typeof(IMessageHandler<>).MakeGenericType(message.GetType());
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

public interface IMessageHandler<in T> where T : Message
{
    Task HandleAsync(T message, CancellationToken ct = default);
}
