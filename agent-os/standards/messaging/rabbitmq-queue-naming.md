# RabbitMQ Queue Naming

Queue names follow the pattern: `{EventType.Name}_{ServiceName}`

Examples:
- `OrderCreatedEvent_OrderService`
- `UserRegisteredEvent_NotificationService`

- `subscriptionName` passed to `ConfigureRabbitMqConsumer` should be the consuming service's name
- Each service gets its own queue per event type (fan-out pattern)
- Queue names are generated automatically by `MassTransitBaseConsumerConfiguration` — do not set them manually