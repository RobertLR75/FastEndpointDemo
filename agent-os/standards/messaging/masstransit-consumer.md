# MassTransitBaseConsumer

Extend `MassTransitBaseConsumer<TEvent>` for all MassTransit consumers. Implement `Consume(TEvent, CancellationToken)` — do not interact with `ConsumeContext` directly.

```csharp
public class OrderCreatedConsumer(IEventPublisher publisher)
    : MassTransitBaseConsumer<OrderCreatedEvent>
{
    protected override async Task Consume(OrderCreatedEvent ev, CancellationToken ct)
    {
        // process event; optionally publish follow-up events
        await publisher.PublishAsync(new OrderProcessedEvent(ev.OrderId), ct);
    }
}
```

- `TEvent` must implement `ISharedEvent`
- Consumers must be registered in the `AddConsumers()` override of the service's consumer configuration — omitting this silently drops messages
- Consumers may publish follow-up events via `IEventPublisher`