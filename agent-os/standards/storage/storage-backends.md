# Storage Backends

Both `BaseMongoDbStorageService<T>` and `BaseStorageService<T>` (Redis) implement `IStorageService<T>`.

| Backend | Use for |
|---|---|
| MongoDB | Durable business data (source of truth) |
| Redis | Ephemeral data, sessions, cache |

- Services may use both: MongoDB as primary store, Redis as cache layer
- Both are registered against `IStorageService<T>` — inject the interface, not the concrete class
- Entities must implement `IEntity` for either backend