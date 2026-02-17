# RabbitMQ Topology Setup

Producer services declare their own exchanges via `RabbitMqBaseSetupService` (an `IHostedService` that runs on startup). Consumer services do not declare exchanges.

```csharp
// In the producer service
public class MyTopology : RabbitMqTopology
{
    public override IEnumerable<(string exchange, string type)> AllExchanges =>
    [
        ("order-events", ExchangeType.Fanout)
    ];
}

// Register in Program.cs
builder.Services.AddHostedService<RabbitMqBaseSetupService>();
// Override GetExchanges() by subclassing RabbitMqBaseSetupService or registering MyTopology
```

- Default exchange type is `fanout` — broadcasts to all bound queues
- Exchanges are declared durable, non-auto-delete
- Only services that publish to an exchange should declare it; consumers rely on it existing