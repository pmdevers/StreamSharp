namespace StreamSharp.Core.Abstractions;

public interface IRepository<TAggregate>
{
    Task<TAggregate?> TryFindAsync(AggregateId id, CancellationToken cancellationToken = default);
    void Add(TAggregate entity);
    void Delete(TAggregate entity);
    
    public async Task DeleteAsync(AggregateId id, CancellationToken cancellationToken = default)
    {
        var entity = await TryFindAsync(id, cancellationToken) ?? throw new InvalidOperationException("Entity not found");
        Delete(entity);
    }
}

public interface IUnitOfWork
{
    IRepository<TAggregate> GetRepository<TAggregate>()
        where TAggregate : Aggregate;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
