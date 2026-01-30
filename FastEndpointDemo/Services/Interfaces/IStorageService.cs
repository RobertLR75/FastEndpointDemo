namespace FastEndpointDemo.Services.Interfaces;

public interface IStorageService<T> where T : class, IEntity
{
    public static string Name { get; }
    public Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken = default);
    public Task UpdateAsync(T entity, CancellationToken cancellationToken=default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken=default);
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<List<T>> GetAllAsync(CancellationToken cancellationToken=default);
}
