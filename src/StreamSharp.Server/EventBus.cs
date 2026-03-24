using System.Threading.Channels;

namespace StreamSharp.Server;

public abstract record Message(Guid MessageId);

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
