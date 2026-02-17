# ISharedEvent

All classes that are serialized and sent across service boundaries (via message bus or HTTP) must implement `ISharedEvent`.

```csharp
public record OrderCreatedEvent(Guid OrderId, string CustomerId) : ISharedEvent;
```

- Define event classes in the shared contracts library, not in individual services
- `ISharedEvent` is a marker interface (no members) — implementation is a compile-time contract
- Required by `BaseEventHandler<TEvent>`, `IEventPublisher`, and `MessageDeserializer<T>`
- Plain DTOs used only within a service boundary do not need `ISharedEvent`