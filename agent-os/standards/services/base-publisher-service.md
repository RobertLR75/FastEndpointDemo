# BasePublisherService

Service-layer equivalent of `BaseEventHandler`. Use when publishing events from the service tier (not from a FastEndpoints handler).

```csharp
public class OrderNotificationService(IEventPublisher pub, ILogger<BasePublisherService<OrderCreatedEvent>> log)
    : BasePublisherService<OrderCreatedEvent>(pub, log)
{
    protected override async Task<OrderCreatedEvent> HandleEventAsync(OrderCreatedEvent ev)
    {
        // enrich or transform before publishing
        return ev with { ProcessedAt = DateTimeOffset.UtcNow };
    }
}
```

| Use | Class |
|---|---|
| FastEndpoints handler receives and re-publishes event | `BaseEventHandler<TEvent>` |
| Service layer initiates and publishes event | `BasePublisherService<TEvent>` |

- Same contract as `BaseEventHandler`: override `HandleEventAsync`, return `null` to suppress publish
- `TEvent` must implement `ISharedEvent`