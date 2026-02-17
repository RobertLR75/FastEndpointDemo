# Service Mappers

Mappers translate between request/response DTOs and domain entities. They are registered as **singletons** — never store state in a mapper.

**Combined mapper (most common):** implement `BaseServiceMapper<TRequest, TResponse, TEntity>`

```csharp
public class OrderMapper : BaseServiceMapper<CreateOrderRequest, OrderResponse, Order>
{
    public override Order? ToEntity(CreateOrderRequest r) =>
        new Order { CustomerId = r.CustomerId };

    public override Order? UpdateEntity(CreateOrderRequest r, Order e)
    {
        e.CustomerId = r.CustomerId;
        return e;
    }

    public override OrderResponse? FromEntity(Order e) =>
        new OrderResponse(e.Id, e.CustomerId, e.CreatedAt);
}
```

**Request-only:** extend `BaseRequestServiceMapper<TRequest, TEntity>`
**Response-only:** extend `BaseResponseServiceMapper<TResponse, TEntity>`

- Never maintain state — mappers are singletons
- Register as singleton in DI: `builder.Services.AddSingleton<OrderMapper>()`