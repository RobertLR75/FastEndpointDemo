# MongoDB Patterns

## Storage service implementation

Extend `BaseMongoDbStorageService<T>` and provide a `CollectionName`:

```csharp
public class OrderStorageService(IMongoDatabase db)
    : BaseMongoDbStorageService<Order>(db)
{
    protected override string CollectionName => "orders"; // lowercase plural
}
```

## Document updates

`UpdateAsync` replaces the **entire document** — always pass the full entity, not a partial update object.

## Index setup

Call `EnsureIndexesAsync` during app startup. Services must do this manually:

```csharp
// Program.cs
await app.Services.GetRequiredService<OrderStorageService>().EnsureIndexesAsync();
```

- Default index covers `Id` + `CreatedAt` (named `idx_id_created`)
- Override `EnsureIndexesAsync` to add additional indexes for query patterns used by the service