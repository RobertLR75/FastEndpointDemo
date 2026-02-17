# Service Exceptions

Three exception types for domain errors. FastEndpoints maps these to HTTP status codes.

| Exception | HTTP | Use when |
|---|---|---|
| `ServiceException` | 500 | Unexpected domain error |
| `ServiceNotFoundException` | 404 | Entity not found |
| `ServiceConflictException` | 409 | Conflict (duplicate, invalid state) |

```csharp
// Throw from service methods — do not catch in endpoints
throw new ServiceNotFoundException("Order not found.");
throw new ServiceConflictException("Order already processed.");
```

- `BaseService` throws `ServiceNotFoundException` automatically on Update/Delete for missing entities
- Never catch these in endpoints — let FastEndpoints map them to status codes
- Do not use `ServiceGameStateException` — use `ServiceConflictException` instead