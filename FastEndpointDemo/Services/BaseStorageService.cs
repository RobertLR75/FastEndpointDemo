using FastEndpointDemo.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace FastEndpointDemo.Services;

public abstract class BaseStorageService<T>(IMemoryCache cache) : IStorageService<T>
    where T : class, IEntity
{
    protected abstract string Name { get; }

    #region Index Management

    private Task UpdateIndexAsync(string id)
    {
        var ids = cache.Get<List<string>>(Name + ":index") ?? [];
        if (!ids.Contains(id))
        {
            ids.Add(id);
            cache.Set(Name + ":index", ids);
        }

        return Task.CompletedTask;
    }
    
    protected Task<List<string>> GetIndexAsync()
    {
        var ids = cache.Get<List<string>>(Name + ":index") ?? [];
        return Task.FromResult(ids);
    }

    #endregion

    public async Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.Id = entity.Id == Guid.Empty ? entity.Id = Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = DateTimeOffset.UtcNow;
        
        cache.Set(Name + $":{entity.Id}", entity);

        await UpdateIndexAsync(entity.Id.ToString());
        return entity.Id;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        var id = entity.Id;
        cache.Set(Name + $":{id}", entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cache.Remove(Name + $":{id}");
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var cachedItems = cache.Get<T>(Name + $":{id}");
        return Task.FromResult(cachedItems);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        var ids = await GetIndexAsync();

        return ids.Select(id => cache.Get<T>(Name + $":{id}")).OfType<T>().ToList();
    }
}