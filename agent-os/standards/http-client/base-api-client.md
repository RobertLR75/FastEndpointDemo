# BaseApiClient

Extend `BaseApiClient<TRequest, TDetailResponse>` for typed HTTP clients. The `Name` property drives both the named `HttpClient` and the URL prefix.

```csharp
public class OrderApiClient(IHttpClientFactory factory)
    : BaseApiClient<CreateOrderRequest, OrderResponse>(factory)
{
    protected override string Name => "orders";
}
```

URL conventions (all relative to the HttpClient base address):
- `GET /{Name}` — list all (optional `?filter=`)
- `GET /{Name}/{id}` — get by ID
- `POST /{Name}` — create
- `PUT /{Name}/{id}` — update

- `Name` is a lowercase plural noun (e.g. `"orders"`)
- Register the named `HttpClient` in DI with a base address matching `Name`
- `DeleteAsync` is not implemented in the base class — add it to the concrete client when needed