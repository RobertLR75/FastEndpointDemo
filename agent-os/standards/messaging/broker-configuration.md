# Broker Configuration

The message broker is selected at runtime via `appsettings.json`. Both brokers can run in any environment.

```json
{
  "MessageBroker": {
    "Type": "RabbitMq"
  },
  "ConnectionStrings": {
    "messageBroker": "<connection-string>"
  }
}
```

- `MessageBroker:Type` accepts `"RabbitMq"` or `"AzureServiceBus"` (case-insensitive)
- Connection string key must be `messageBroker` (lowercase) — matches `ServiceName.MessageBroker`
- Extend `MassTransitBaseConsumerConfiguration` and override `AddConsumers`, `ConfigureRabbitMqConsumers`, and `ConfigureServiceBusConsumers`