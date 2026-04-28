namespace StreamSharp.Core.Abstractions;

public interface IRepository<TAggregate, TId>
{
    Task<TAggregate?> TryFindAsync(TId id, CancellationToken cancellationToken = default);
    void Add(TAggregate entity);
    void Delete(TAggregate entity);
    
    public async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await TryFindAsync(id, cancellationToken) ?? throw new InvalidOperationException("Entity not found");
        Delete(entity);
    }
}

public interface IUnitOfWork
{
    IRepository<TAggregate, TId> GetRepository<TAggregate, TId>()
        where TAggregate : AggregateRoot<TId>
        where TId : struct;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
