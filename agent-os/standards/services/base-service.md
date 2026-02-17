# BaseService

Extend `BaseService<T>` for domain services. It wraps `IStorageService<T>` with CRUD operations and optional domain event publishing.

```csharp
public class OrderService(IStorageService<Order> storage, IEventPublisher publisher)
    : BaseService<Order>(storage, publisher)
{
    protected override ISharedEvent? CreateCreatedEvent(Order result) =>
        new OrderCreatedEvent(result.Id);

    protected override ISharedEvent? CreateUpdatedEvent(Order result, Order existing) =>
        new OrderUpdatedEvent(result.Id);

    // Return null to skip publishing (default behavior)
    protected override ISharedEvent? CreateDeletedEvent(Guid id) => null;
}
```

- Override event factory methods only when consumers exist for that event
- `ServiceNotFoundException` is thrown automatically if entity not found on Update/Delete
- `CreateAsync` and `UpdateAsync` return the persisted entity (re-fetched after write)