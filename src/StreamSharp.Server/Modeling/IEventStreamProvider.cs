namespace StreamSharp.Server.Modeling;

public interface IEventStreamProvider<TId>
    where TId : struct
{
    public Task<EventStream<TId>> GetOrCreateStream(TId id);
    public Task SaveStream(EventStream<TId> stream);
}
