# Redis Key Naming

Redis keys follow the pattern `{Name}:{id}`. The index of all IDs is stored at `{Name}:index`.

```csharp
public class OrderStorageService(IDistributedCache cache) : BaseStorageService<Order>(cache)
{
    public override string Name => "orders"; // lowercase plural
}
```

Key examples for `Name = "orders"`:
- Single entity: `orders:3f1a2b4c-...`
- Index: `orders:index` (JSON array of all IDs)

- `Name` must be a lowercase plural noun (e.g. `"orders"`, `"users"`, `"products"`)
- Must be unique across all Redis storage services in the system
- Do not include version or namespace prefixes — keep it short