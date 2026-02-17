# BaseEventHandler

Extend `BaseEventHandler<TEvent>` for all FastEndpoints event handlers. It auto-logs the incoming event, calls `HandleEventAsync`, then republishes the returned event to the bus.

```csharp
public class MyEventHandler(IEventPublisher pub, ILogger<BaseEventHandler<MyEvent>> log)
    : BaseEventHandler<MyEvent>(pub, log)
{
    protected override async Task<MyEvent> HandleEventAsync(MyEvent ev)
    {
        // transform/enrich ev, then return it
        return ev with { ProcessedAt = DateTimeOffset.UtcNow };
    }
}
```

- Always override `HandleEventAsync` — without it the event republishes unmodified
- Return `null` to suppress republish (stop propagation)
- `TEvent` must implement `ISharedEvent`
- Inject dependencies via constructor alongside the required `IEventPublisher` and `ILogger`
