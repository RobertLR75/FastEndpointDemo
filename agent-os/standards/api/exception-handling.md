# Exception Handling

All unhandled exceptions return RFC 7807 ProblemDetails with HTTP 500. Always call `UseEndpoints()` — never register a custom exception handler.

**Response format:**
```json
{
  "title": "En uventet feil oppstod",
  "detail": "<exception message>",
  "status": 500,
  "instance": "/path/to/endpoint"
}
```

- Content-Type is always `application/problem+json`
- Title is fixed Norwegian string — do not change per service
- Never catch-and-swallow exceptions in endpoints; let the global handler respond